using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CA;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public static class LVXJobsConsolidateHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static void ConsolidateSingleLVXJob(JobDeclaration lvxToConsolidate, BusinessObjectFactory factory,
			List<JobDeclaration> targetLVSJobs, Func<JobDeclaration, object> getFormatArg, Action<LogType, string, object[]> notify, Func<JobDeclaration, IEnumerable<IConsolidationStrategy>> getStratergy = null)
		{
			Argument.NotNull(lvxToConsolidate, "lvxToConsolidate");

			var validateResult = GetJobValidateResult(lvxToConsolidate, getFormatArg);
			if (validateResult.Item1 != null)
			{
				notify(LogType.Warning, validateResult.Item1, validateResult.Item2);
				return;
			}

			var strategies = getStratergy == null ? GetConsolidationStrategies(lvxToConsolidate) : getStratergy(lvxToConsolidate);
			var matchingDeclaration = FindMatchingDeclarationJustCreated(targetLVSJobs, strategies) ?? FindMatchingLVSDeclarationInDb(factory, strategies);
			if (matchingDeclaration == null)
			{
				matchingDeclaration = CreateNewLVSDeclaration(factory);

				strategies.ForEach(x => x.FillDataForNewDeclaration(matchingDeclaration));

				ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null, true);
			}

			if (matchingDeclaration.HasAB3AcceptedOrWaiting)
			{
				notify(LogType.Warning, GetLVXHasAdditionalDeclarationWithB3AcceptedLog, new object[] { getFormatArg(lvxToConsolidate), getFormatArg(matchingDeclaration) });
			}
			else
			{
				var invoice = lvxToConsolidate.Invoices.OfType<JobComInvoiceHeader>().FirstOrDefault();
				using (invoice.SuspendRefreshAdditionalInvoice())
				{
					AttachToConsolidatedLVSDeclaration(invoice, matchingDeclaration);
				}
				if (!targetLVSJobs.Contains(matchingDeclaration))
				{
					targetLVSJobs.Add(matchingDeclaration);
				}

				notify(LogType.Information, GetLVXAttachedToConsolidatedLVSLog, new object[] { getFormatArg(lvxToConsolidate), getFormatArg(matchingDeclaration) });
			}
		}

		static (string, object[]) GetJobValidateResult(JobDeclaration lvxToConsolidate, Func<JobDeclaration, object> getFormatArg)
		{
			var lvxArg = getFormatArg(lvxToConsolidate);
			if (lvxToConsolidate.IsCancelled)
			{
				return (GetDeclarationDeactivedLog, new object[] { lvxArg });
			}

			var invoice = lvxToConsolidate.Invoices.OfType<JobComInvoiceHeader>().FirstOrDefault();

			if (invoice == null)
			{
				return (GetDeclarationHasNoInvoicesLog, new object[] { lvxArg });
			}
			else if (!invoice.CA_ReadyForConsolidation)
			{
				return (GetDeclarationNotReadyToConsolidationLog, new object[] { lvxArg });
			}
			else if (!lvxToConsolidate.JE_EntryAuthorisationDate.IsValidSmallDateTime)
			{
				return (GetAuthorisationDateIsNotValidLog, new object[] { lvxArg });
			}
			else if (lvxToConsolidate.ActiveEntryHeaders.Count == 0)
			{
				var notifier = new SendsMessagesToCustomsShutterUpperer(false);
				if (!lvxToConsolidate.DoMerge(notifier))
				{
					return (GetDeclarationMergeErrorLog, new object[] { lvxArg, notifier.InvalidOperationText });
				}
			}

			if (lvxToConsolidate.ActiveEntryHeaders.Count == 0)
			{
				return (GetDeclarationHasNoEntryHeadersLog, new object[] { lvxArg });
			}
			else if (!invoice.SupportAdditionalDeclarations)
			{
				return (GetDeclarationNotAllowMultipleDeclarationLog, new object[] { lvxArg });
			}
			else if (invoice.FirstAdditionalDeclaration is JobDeclaration additionalDec)
			{
				return (GetLVXHasAdditionalDeclarationLog, new object[] { lvxArg, getFormatArg(additionalDec) });
			}
			return default(ValueTuple<string, object[]>);
		}

		internal static void RaiseSaving(List<JobDeclaration> targetLVSJobs, Action fireSave, Action<LogType, string, object[]> notify)
		{
			foreach (var lvs in CargoWise.Common.IEnumerableExtensions.DistinctBy(targetLVSJobs, x => x.PK))
			{
				lvs.MessageInitiator = new LVXJobNotificationCollector(notify);
				PrepareAndRunMergeForLVS(lvs);
			}

			targetLVSJobs.Clear();

			fireSave();
		}

		#region GetLogs

		internal static string GetDeclarationNotReadyToConsolidationLog => Res.GetString("03A0BD6F-FD1A-44A6-B374-CD33F2E8F251", "{0} is not ready for Consolidation.", ParaHolder0);

		internal static string GetDeclarationDeactivedLog => Res.GetString("3e51e8af-af52-4cd7-9da9-36d7cd6fe129", "{0} has already been deactivated.", ParaHolder0);

		internal static string GetDeclarationMergeErrorLog => Res.GetString("ED764E3D-2BE2-4C5E-A9DA-4903534C5BB0", "Can't merge {0} and it has no entry headers, it is ignored. Merge failed reason : {1}", ParaHolder0, ParaHolder1);

		internal static string GetDeclarationHasNoInvoicesLog => Res.GetString("0aea61f8-6539-45a0-bbff-2cef2077db56", "{0} has no invoices.", ParaHolder0);

		internal static string GetDeclarationHasNoEntryHeadersLog => Res.GetString("bfc7ec60-6053-4844-938f-b69bf230faf6", "{0} has no entry headers, it is ignored.", ParaHolder0);

		internal static string GetDeclarationNotAllowMultipleDeclarationLog => Res.GetString("75019a6a-a4da-49dd-864a-46bf749500fd", "{0} does not support multiple declarations.", ParaHolder0);

		internal static string GetLVXHasAdditionalDeclarationLog => Res.GetString("5e853d89-4da9-4206-8409-ae535111e2db", "{0} has already been attached to Consolidated LVS Declaration {1}, it is ignored.", ParaHolder0, ParaHolder1);

		internal static string GetAuthorisationDateIsNotValidLog => Res.GetString("10A9502A-1D72-4528-A523-6117E83CEBCB", "{0} has invalidate Entry Authorization Date.", ParaHolder0);

		internal static string GetLVXAttachedToConsolidatedLVSLog => Res.GetString("64e290b5-9a28-42e6-980d-0c71fa19b1e5", "{0} is attached to Consolidated LVS Declaration {1}.", ParaHolder0, ParaHolder1);

		internal static string GetLVXDetachedFromConsolidatedLVSLog => Res.GetString("45271dc6-4967-4114-aba2-d15c6f8c67ed", "{0} is detached from the additional declaration {1}.", ParaHolder0, ParaHolder1);

		internal static string GetLVXHasNoAdditionalDeclarationLog => Res.GetString("ff2cee40-989f-40f2-b4f9-0a96d7636fe1", "{0} is not attached to an additional declaration, it is ignored.", ParaHolder0);

		public static string GetLVXHasAdditionalDeclarationWithB3AcceptedLog => Res.GetString("fed62bbe-7773-42f1-b763-db42e0169b88", "{0} 's additional declaration {1} has had a CAD accepted or is waiting for a response, it cannot be detached.", ParaHolder0, ParaHolder1);

		internal static string ParaHolder0 => "{0}";
		internal static string ParaHolder1 => "{1}";
		#endregion

		internal static IEnumerable<IConsolidationStrategy> GetConsolidationStrategies(JobDeclaration lvxJob)
		{
			var wrapper = new JobDeclarationConsolidationOptionsWrapper(lvxJob);
			return ConsolidationStrategyProvider.GetConsolidationStrategies(wrapper);
		}

		internal static JobDeclaration FindMatchingLVSDeclarationInDb(BusinessObjectFactory factory, IEnumerable<IConsolidationStrategy> strategies)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.LowValueShipments);
			result.AddToFilter(JobDeclarationSchema.JE_GC, GlbCompany.CurrentCompany.PK);

			var b3AcceptedQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE, true);
			b3AcceptedQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, new[] { MessageTypeList.Codes.B3CUSDEC, MessageTypeList.Codes.CommercialAccountingDeclaration });
			var closedAndLodgedQuery = new ZQuery();
			closedAndLodgedQuery.AddToFilter(JoinCondition.Or, CusEntryHeaderSchema.CH_EntryStatus, B3EntryStatusList.Codes.Accepted);
			closedAndLodgedQuery.AddToFilter(JoinCondition.Or, CusEntryHeaderSchema.CH_EntryStatus, B3EntryStatusList.Codes.Confirmed);
			closedAndLodgedQuery.AddToFilter(JoinCondition.Or, CusEntryHeaderSchema.CH_EntryStatus, CADEntryStatusList.Codes.Approved);
			closedAndLodgedQuery.AddToFilter(JoinCondition.Or, CusEntryHeaderSchema.CH_Status, MessageStatusList.Codes.AwaitingOriginal);
			b3AcceptedQuery.AddToFilter(closedAndLodgedQuery, JoinCondition.And);
			result.AddSubQuery(b3AcceptedQuery, JoinCondition.And);
			result.AddToFilter(ModelViewColumnHelper.GetDateFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, "JE_LVSCloseDate", DateComparisonOperator.HasNoDateEntered, ZDateTime.Empty, ZDateTime.Empty));

			foreach (var strategy in strategies)
			{
				strategy.AddMatchingFilter(result);
			}
			result.OrderBy = JobDeclarationSchema.JE_SystemCreateTimeUtc.Name + " DESC";

			return factory.LoadTop1<JobDeclaration>(result);
		}

		static JobDeclaration FindMatchingDeclarationJustCreated(List<JobDeclaration> associatedLVSDeclarations, IEnumerable<IConsolidationStrategy> strategies)
		{
			return associatedLVSDeclarations.FirstOrDefault(p => strategies.All(x => x.IsMatching(p)));
		}

		#region ModelViewColumnHelper

		public static ModelViewColumnQueryHelper<JobDeclaration> ModelViewColumnHelper
		{
			get
			{
				return new ModelViewColumnQueryHelper<JobDeclaration>();
			}
		}
		public const string ModelView = "CAJobDeclaration";
		public const string ModelViewPK = "JE_PK";

		#endregion

		#region CreateNewLVSDeclaration

		static JobDeclaration CreateNewLVSDeclaration(BusinessObjectFactory factory)
		{
			var newDeclaration = factory.New<JobDeclaration>();
			newDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			newDeclaration.JE_GB = ZGuid.Empty;
			newDeclaration.JE_OH_Importer = ZGuid.Empty;
			newDeclaration.CA_ProvinceOfClearance = ZString.Empty;
			newDeclaration.JE_GS_NKCusAgent = ZString.Empty;
			newDeclaration.JE_GC = GlbCompany.CurrentCompany.PK;

			return newDeclaration;
		}

		#endregion

		public static void AttachToConsolidatedLVSDeclaration(JobComInvoiceHeader invoice, JobDeclaration lvsJob)
		{
			Argument.NotNull(invoice, "JobComInvoiceHeader");
			Argument.NotNull(lvsJob, "lvsJob");

			invoice.AttachToAdditionalDeclaration(lvsJob);
			var groupHeader = invoice.GroupHeader;
			if (groupHeader != null)
			{
				groupHeader.AttachToAdditionalDeclaration(lvsJob);
			}

			var dec = invoice.JobDeclaration;

			if (dec != null)
			{
				dec.LogCustomsCommencedIfNeeded(lvsJob.JE_DeclarationReference, false);
			}
			invoice.CA_ReadyForConsolidation = false;
		}

		public static void DetachFromConsolidatedLVSDeclaration(JobComInvoiceHeader invoice, JobDeclaration lvsJob)
		{
			Argument.NotNull(invoice, "JobComInvoiceHeader");
			Argument.NotNull(lvsJob, "lvsJob");

			invoice.DetachFromAdditionalDeclaration(lvsJob);
			var groupHeader = invoice.GroupHeader;
			if (groupHeader != null)
			{
				groupHeader.JZ_InvoiceNumber = "All Invoices";
				groupHeader.DetachFromAdditionalDeclaration(lvsJob);
			}

			PrepareAndRunMergeForLVS(lvsJob);

			var jobComInvoiceLinePKs = invoice.JobComInvoiceLines.GetPKs();
			if (jobComInvoiceLinePKs.Any())
			{
				var query = new ZQuery(CusUnderbondDecSchema.BU_ClusterKey, lvsJob.JE_ClusterKey);
				query.AddToFilter(CusUnderbondDecSchema.BU_JI, jobComInvoiceLinePKs);
				invoice.Factory.Load<AdditionalInvoiceLineEntryLineLink>(query).DeleteAll();
			}
			var dec = invoice.JobDeclaration;
			if (dec != null)
			{
				dec.CancelCustomsEvents();
			}
		}

		public static void PrepareAndRunMergeForLVS(JobDeclaration lvsJob)
		{
			lvsJob.Invoices.RefreshAdditionalInvoices();
			lvsJob.InvoiceLines.Load();

			if (lvsJob.IsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader)
			{
				lvsJob.CA_RequiresMerge = true;

				if (!lvsJob.HasMessageInitiator)
				{
					lvsJob.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				}

				lvsJob.RunMergeForLVSIfRequired();
			}
		}
	}
}

