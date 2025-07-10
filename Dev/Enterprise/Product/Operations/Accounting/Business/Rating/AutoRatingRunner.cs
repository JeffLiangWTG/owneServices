using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class AutoRatingRunner
	{
		public AutoRatingRunner(IBusiness parentConsumer, IRatingContext ratingContext, bool isSaveCalledManuallyAfterSession = true)
		{
			Argument.NotNull(parentConsumer, nameof(parentConsumer));
			Argument.NotNull(ratingContext, nameof(ratingContext));

			Parent = parentConsumer;
			this.ratingContext = ratingContext;
			this.isSaveCalledManuallyAfterSession = isSaveCalledManuallyAfterSession;

			if (Parent is IStmNoteParent noteParent)
			{
				var autoRatingParentsToAddFetchHintsFor = noteParent.IsInDatabase // no fetch hints necessary if not in the database
					? RatingCache.LocalSession?.GetCachedValue("AutoRatingRunner|AutoRatingParentsToLoadNotesFor", GetAutoRatingParentsToAddFetchHintsFor)
					: null;

				if (autoRatingParentsToAddFetchHintsFor == null
					|| autoRatingParentsToAddFetchHintsFor.Add(noteParent)) // no need to hook the same parent twice
				{
					Parent.Factory.Saving += Factory_Saving;
				}
			}

			HashSet<IStmNoteParent> GetAutoRatingParentsToAddFetchHintsFor()
			{
				var result = new HashSet<IStmNoteParent>();
				Parent.Factory.Saving += AddFetchHints;
				return result;

				void AddFetchHints(BusinessObjectFactory factory)
				{
					factory.Saving -= AddFetchHints;

					foreach (var item in result)
					{
						factory.AddFetchHint(new FetchHint(StmNoteSchema.ST_ParentID, item.NotesParentPK, StmNoteSchema.ST_NoteText));
					}
				}
			}
		}

		readonly IBusiness Parent;
		readonly IRatingContext ratingContext;
		readonly bool isSaveCalledManuallyAfterSession;

		void Factory_Saving(BusinessObjectFactory factory)
		{
			factory.Saving -= Factory_Saving;
			RemoveAutoRatingAuditLogIfRequired((BusinessObject)Parent);
		}

		#region Execution

		public AutoRateInfoCollection RetrieveAllCharges(IAutoRating[] nonCustomsJobsToRate, IRatingAdaptersProvider adaptersProvider, CostSell costOrSell, RatingProgressReporter progressReporter = null)
		{
			var resultCharges = new AutoRateInfoCollection(Parent.Factory);

			AutoRater.Reset();
			ratingContext.PercentageLinesApplied.Clear();

			var jobServicePresenceChecker = new JobServicePresenceChecker();
			var dictionary = new Dictionary<IAutoRating, AutoRateResult>();

			foreach (var itemToRate in nonCustomsJobsToRate)
			{
				var subject = ratingContext.IsInRebateCalculationMode
					? (NoResString)"PROFIT SHARE"
					: costOrSell == CostSell.Cost
						? "COSTS"
						: "REVENUE";
				ratingContext.Logger.Information(string.Format(CultureInfo.InvariantCulture, (NoResString)"AUTORATING {0} FOR {1}", subject, itemToRate.HumanReadableName()));

				var results = RateItem(itemToRate, costOrSell);
				var cancellation = results.RateInfoCollection.Cancellation;
				if (cancellation != null)
				{
					resultCharges.CancelAutoRating(cancellation);
					break;
				}

				// Some charges depend on other (like Percentage Calculator or Cost Based Calculator), so, we need to make sure
				// that intermediate results get added to previous results collection so that they become available for charges
				// in the queue for calculation.
				ratingContext.InterimAutoRatingResults.AddRange(results.RateInfoCollection);

				jobServicePresenceChecker.CombineRatingResults(itemToRate, results.RateInfoCollection);

				dictionary.Add(itemToRate, results);
				progressReporter?.IncrementAndReport(itemToRate);
			}

			if (!ratingContext.IsInRebateCalculationMode)
			{
				if (!ratingContext.SearchForRatesMode)
				{
					var emailFactory = isSaveCalledManuallyAfterSession ? null : Parent.Factory;
					SendEmailsForAdditionalRates(emailFactory, dictionary);
				}

				OnAfterRatingNotification?.Invoke(jobServicePresenceChecker.GetMissingJobServicesNotification(Parent.HumanReadableName));
			}

			// In some cases, jobs have specific logic about filtering results. So, we give them right to do so here before they
			// are finally summarized and converted to Job Charges.
			HandleResult(adaptersProvider, dictionary);

			var mergedCharges = new RateResultsSummarizer(Parent.Factory).MergeInfosCrossAdapter(dictionary);
			resultCharges.CheckAndAddRange(mergedCharges);

			return resultCharges;
		}

		public delegate void PostRatingNotificationsHandler(string message);
		public event PostRatingNotificationsHandler OnAfterRatingNotification;

		AutoRateResult RateItem(IAutoRating itemToRate, CostSell costOrSell)
		{
			AutoRateResult jobResult = new AutoRateResult(Parent.Factory, costOrSell);

			try
			{
				jobResult = AutoRater.AutoRate(Parent.Factory, itemToRate, costOrSell, ratingContext);

				PossibleMathes.Clear();
				PossibleMathes.AddRange(jobResult.PossibleMatchesWrapper.PossibleMatches.Cast<RateEntry>());
			}
			finally
			{
				LogChargesCalculated(jobResult.RateInfoCollection, costOrSell);
			}

			return jobResult;
		}

		#region Send Emails For Additional Rates

		/// <summary>
		/// If factory is null, emails will be created and saved in a new factory for each email.
		/// </summary>
		static void SendEmailsForAdditionalRates(BusinessObjectFactory factory, Dictionary<IAutoRating, AutoRateResult> autoRatingJobsAndResults)
		{
			// Tests are in Rating solution.
			// Look for tests that set
			//		RatingDataRegistry.Instance.ClientRateGoingToExpireNotification
			//		RatingDataRegistry.Instance.CompanyTariffGoingToExpireNotification

			var expiringRates = new Dictionary<IAutoRating, IEnumerable<RateEntry>>();

			foreach (var jobAndResults in autoRatingJobsAndResults)
			{
				if (jobAndResults.Value.RatesGoingToExpire.Any())
				{
					expiringRates.Add(jobAndResults.Key, jobAndResults.Value.RatesGoingToExpire.OfType<RateEntry>());
				}
			}

			if (!expiringRates.Any())
			{
				return;
			}

			var emails = RatingEmailDef.GetEmailsForRateEntriesGoingToExpire(expiringRates);
			foreach (var email in emails)
			{
				email.Send(factory);
			}
		}

		#endregion

		#region Handle Results

		void HandleResult(IRatingAdaptersProvider adaptersProvider, IReadOnlyDictionary<IAutoRating, AutoRateResult> result)
		{
			if (!adaptersProvider.NeedsHandleResult)
			{
				return;
			}

			// HandleResult works with instances of type IAutoRatedCharge rather than AutoRateInfo (which implements IAutoRatedCharge).
			// It is because of project cross references which makes hard force them to use the same type without refactoring and changes in 100+ files.
			// So, we need to this trick, i.e. cast dictionary to the one using IAutoRatedCharge, and...
			var adaptedResult = result
				.ToDictionary(
					pair => (IRatingAdapter)pair.Key,
					pair => pair.Value.RateInfoCollection.Cast<IAutoRatedCharge>().ToList());

			if (!adaptersProvider.HandleResult(adaptedResult))
			{
				// Nothing to do as results were not changed
				return;
			}

			// ... reflect the changes back to the original result
			foreach (var record in adaptedResult)
			{
				var original = result[(IAutoRating)record.Key].RateInfoCollection;
				var updated = record.Value;

				var itemsToDelete = original.Except(updated.OfType<AutoRateInfo>()).ToList();
				var itemsToAdd = updated.Except(original).ToList();

				foreach (var item in itemsToDelete)
				{
					original.Remove(item);
				}

				foreach (var item in itemsToAdd)
				{
					original.Add((AutoRateInfo)item);
				}
			}
		}

		#endregion

		public AutoRater AutoRater
		{
			get { return autoRater ?? (autoRater = GetNewAutoRater()); }
		}
		AutoRater autoRater;

		protected virtual AutoRater GetNewAutoRater()
		{
			return new AutoRater();
		}

		public List<RateEntry> PossibleMathes
		{
			get { return possibleMathes ?? (possibleMathes = new List<RateEntry>()); }
		}
		List<RateEntry> possibleMathes;

		#endregion

		#region Fallback Note

		public static MultilingualString CalculationXML
		{
			get { return ResString.GetMultilingualString("a8a13077-9cf2-483b-836a-407f9d132f5b", "Calculation XML"); }
		}

		public static MultilingualString CalculationXMLDoesNotExistReason
		{
			get
			{
				return ResString.GetMultilingualString("7f6f68e7-e0c1-4699-aa38-e1945033c899", @"This may be because:
 - The billing line was entered manually.
 - The charge code is not {0}.", new BusinessObjectFactory().Load<AccChargeCode>(Env.Registry.FreightChargeCode)?.AC_Code);
			}
		}

		public static MultilingualString AutoRatingLog
		{
			get { return ResString.GetMultilingualString("82c0114e-9b30-4a9d-8e1e-43a343ac6852", "Autorating Log"); }
		}

		protected void LogChargesCalculated(AutoRateInfoCollection jobRates, CostSell costOrSell)
		{
			#region SuppressResourceStringsCheckRegion

			var fallbackDescription = new ZStringBuilder();
			fallbackDescription.AppendLine("CHARGES CALCULATED:");

			foreach (var info in jobRates.Where(rateInfo => !rateInfo.CalculationDescription.IsEmpty && !rateInfo.IsInclusiveCalculator).OrderBy(x => x.SingleLineDescription))
			{
				if (info.IsDisbursement && costOrSell == CostSell.Revenue)
				{
					fallbackDescription.AppendLine($"{info.SingleLineDescription}\t:	 Disbursements autorated from Sell Rates override unapportioned Cost or a Cost that is not flagged 'Override Rating'.");
				}
				else
				{
					fallbackDescription.AppendLine(info.SingleLineDescription);
				}
			}

			ratingContext.Logger.Information(fallbackDescription.ToString());

			#endregion
		}

		public void WriteAutoratingLogIntoNote(IStmNoteParent noteParent)
		{
			if (noteParent == null)
			{
				return;
			}

			var note = GetAutoRatingLogNoteAndPurgeUnnecessaryNotes(noteParent.Notes);
			var noteText = note.ST_NoteText;
			var noteLengthRemaining = note.NoteTextMaxLength - noteText.Length;

			if (noteLengthRemaining > 0)
			{
				var noteBuilder = new ZStringBuilder();
				noteBuilder.AppendIfNotEmpty(note.ST_NoteText);
				if (noteBuilder.IsEmpty)
				{
					noteBuilder.Append((NoResString)"User:\t\t\t\t");
					noteBuilder.AppendLine(GlbStaff.CurrentUser.GS_FullName);
					noteBuilder.Append((NoResString)"Time:\t\t\t\t");
					noteBuilder.AppendLine(ZDateTime.Now.ToLongTimeString());
				}

				noteBuilder.AppendLine();
				AppendLogToNote(note, noteBuilder);
				note.ReadOnly = true;
			}
			else
			{
				// We call dump logs to clear the current logs
				DumpLogs();
			}
		}

		protected virtual void AppendLogToNote(StmNote note, ZStringBuilder noteBuilder)
		{
			noteBuilder.Append(DumpLogs());
			note.ST_NoteText = noteBuilder.ToString().TrimToFit(note.NoteTextMaxLength);
		}

		StmNote GetAutoRatingLogNoteAndPurgeUnnecessaryNotes(Notes notes)
		{
			StmNote note = null;
			notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description)
				.Where(x => x.IsBelongingToCurrentLoginCompany)
				.ForEach(x =>
				{
					if (note == null && x.ST_GC_RelatedCompany != ZGuid.Empty)
					{
						note = x;
					}
					else
					{
						x.Delete();
					}
				});

			if (note == null)
			{
				note = notes.AddNew();
				using (note.GetValidationSuspender())
				{
					note.ST_Description = PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description;
					note.ST_GC_RelatedCompany = Env.CurrentCompany.PK;
					note.ST_IsCustomDescription = false;
				}
			}

			return note;
		}

		string DumpLogs() => (ratingContext.Logger as LoggerDecorator)?.DumpLog().ToStringWithNewLineBetweenStrings();

		static void RemoveAutoRatingAuditLogIfRequired(BusinessObject parentAsBusinessObject)
		{
			if (parentAsBusinessObject != null)
			{
				var autoRatingAuditLogs = parentAsBusinessObject.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description)?.Where(x => x.IsBelongingToCurrentLoginCompany);
				if (autoRatingAuditLogs != null)
				{
					var collectionToDelete = !DataRegistryRating.Instance.AllowSavingOfAutoRatingLogNote.Value
						? autoRatingAuditLogs
						: autoRatingAuditLogs.Where(n => n.HasErrors);

					foreach (var note in collectionToDelete)
					{
						note.Delete();
					}
				}
			}
		}

		#endregion
	}
}
