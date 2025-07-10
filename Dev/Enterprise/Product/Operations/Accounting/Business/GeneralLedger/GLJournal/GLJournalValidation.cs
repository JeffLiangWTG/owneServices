using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals
{
	public class GLJournalValidation : TransactionHeaderWithLinesValidation
	{
		public GLJournalValidation(GLJournal parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		protected override PeriodValidationProvider GetPeriodValidationProvider()
		{
			return new GLPeriodValidationProvider(Parent.Factory);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckAH_GLJournalLines();
		}

		protected void CheckAH_GLJournalLines()
		{
			var message = Res.GetString("8C98E3A6-B605-43F3-94CC-E3C754268594", "You cannot post a GL Journal without transaction lines. Please add at least two lines to have a zero balance amount.");
			Parent.RemoveRowError(message);

			if (Parent.Lines == null || Parent.Lines.Count == 0)
			{
				Parent.AddRowError(message);
			}
		}

		protected override void CheckAH_OSExTaxAmount()
		{
			base.CheckAH_OSExTaxAmount();

			if (!Parent.AH_OSExTaxAmountInfo.HasErrors() && Parent.IsLevelAuthorizationRequired)
			{
				Parent.AH_OSExTaxAmountInfo.AddWarning(Res.GetString("f53306e1-62be-4e00-a0a8-cbe3fe012582", "You do not have rights to create a journal for this amount without authorization."));
			}
		}

		#region CheckAH_Desc

		protected override void CheckAH_Desc()
		{
			base.CheckAH_Desc();
			MandatoryValidation.CheckEntered(Parent.AH_DescInfo);
		}

		#endregion

		#region CheckAH_TransactionType

		protected override void CheckAH_TransactionType()
		{
			base.CheckAH_TransactionType();
			MandatoryValidation.CheckEntered(Parent.AH_TransactionTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AH_TransactionTypeInfo, Parent.TransactionType_List);

			if (Parent.IsEliminationJournal && Parent.AH_TransactionType != TransactionTypes.GLStandardJournal)
			{
				Parent.AH_TransactionTypeInfo.AddError(Res.GetString("d6e10cef-e760-4d1c-9cb6-7b1f58d22c2d", "Transaction Type must be General Journal when Presentation Category is of type Elimination Journal."));
			}

			if (Parent.IsNoteJournal && Parent.GLJournalLines.Cast<GLJournalLine>().Any(x => x.GLHeader != null && x.GLHeader.AG_AccountType != AccountType.Note))
			{
				Parent.AH_TransactionTypeInfo.AddError(Res.GetString("ab5e514b-c011-41f1-ab25-4ab7645b5c46", "GL Account with 'BSH' and 'P&L' account types cannot be used for the creation of NTE journal. Please select another GL Account."));
			}
		}

		#endregion

		#region CheckAH_TransactionCategory

		protected override void CheckAH_TransactionCategory()
		{
			base.CheckAH_TransactionCategory();
			if (!Parent.AH_TransactionCategory.IsEmpty && !Parent.IsInDatabase)
			{
				ListValidation.ErrorIfInvalidCode(Parent.AH_TransactionCategoryInfo, Parent.TransactionCategory_List);
			}

			if (Parent.IsEliminationJournal && Parent.AH_TransactionType != TransactionTypes.GLStandardJournal)
			{
				Parent.AH_TransactionCategoryInfo.AddError(Res.GetString("e785b56d-1681-4482-8617-bc7f9b8efc79", "Presentation Category can only be of Elimination Journal type when transaction type is General Journal."));
			}
		}

		#endregion

		#region CheckAH_DueDate

		protected override void CheckAH_DueDateIsValidZDateTimeRange()
		{
		}

		protected override void CheckAH_DueDate()
		{
			base.CheckAH_DueDate();

			if (Parent.AH_TransactionType == TransactionTypes.GLReversingJournal)
			{
				PeriodValidation.CheckDateFallsIntoValidPeriod(Parent.AH_DueDateInfo);
			}

			if (Parent.AgePeriodInfo.HasErrors())
			{
				foreach (var err in Parent.AgePeriodInfo.GetErrors())
				{
					Parent.AH_DueDateInfo.AddError(err.Message);
				}
			}
		}

		#endregion

		#region CheckAH_PostDate

		// Can post to any open GL period
		protected override void CheckAH_PostDateNotInFuture()
		{
		}

		// Can post to any open GL period
		protected override void CheckAH_PostDateNotInPast()
		{
		}

		protected override void CheckAH_PostDateIsValidZDateTimeRange()
		{
		}

		protected override void CheckAH_PostDate()
		{
			base.CheckAH_PostDate();
			if (Parent.PostPeriodInfo.HasErrors())
			{
				foreach (var err in Parent.PostPeriodInfo.GetErrors())
				{
					Parent.AH_PostDateInfo.AddError(err.Message);
				}
			}
		}

		#endregion

		#region Calculated Properties Validation

		#region CheckPostPeriod

		protected override void CheckPostPeriod()
		{
			base.CheckPostPeriod();
			if (!Parent.PostPeriodInfo.HasErrors())
			{
				int period = Parent.PostPeriod;
				string error;

				if (!PeriodCalculator.IsPeriodValid(period))
				{
					error = InvalidPeriodError;
				}
				else
				{
					error = ValidatePeriod(period);
					if (string.IsNullOrEmpty(error) && !Parent.CanUserPostToPreviousPeriods && PeriodCalculator.IsPreviousPeriod(period))
					{
						error = InsufficientRightsToPostToPreviousPeriodError;
					}
				}

				if (!string.IsNullOrEmpty(error))
				{
					Parent.PostPeriodInfo.AddError(error);
					ValidateAH_PostDate();
				}
			}
			if (Parent.AH_PostDate.IsEmpty && !Parent.PostPeriod.IsEmpty && !Parent.PostPeriodInfo.HasErrors())
			{
				Parent.PostPeriodInfo.AddError(PeriodCreatedConcurrentError(Parent.PostPeriod, Parent.PostPeriodInfo.HumanReadableName, Parent.AH_PostDateInfo.HumanReadableName));
			}
		}

		#endregion

		#region CheckAgePeriod

		protected override void CheckAgePeriod()
		{
			base.CheckAgePeriod();

			if (Parent.AH_TransactionType == TransactionTypes.GLAutoJournal || Parent.AH_TransactionType == TransactionTypes.GLReversingJournal)
			{
				int period = Parent.AgePeriod;
				string error;

				if (!PeriodCalculator.IsPeriodValid(period))
				{
					error = InvalidPeriodError;
				}
				else if (period <= Parent.PostPeriod)
				{
					error = AgePeriodMustBeGreaterThanPostPeriodError;
				}
				else
				{
					error = ValidatePeriod(period);
				}
				if (string.IsNullOrEmpty(error) && Parent.AH_TransactionType == TransactionTypes.GLAutoJournal && !Parent.PostPeriodInfo.HasErrors())
				{
					int tempPeriod = PeriodCalculator.GetNextPeriod(Parent.PostPeriod);
					while (tempPeriod != 0 && tempPeriod < period)
					{
						error = ValidatePeriod(tempPeriod);
						if (!string.IsNullOrEmpty(error))
						{
							break;
						}
						tempPeriod = PeriodCalculator.GetNextPeriod(tempPeriod);
					}
				}

				if (!string.IsNullOrEmpty(error))
				{
					Parent.AgePeriodInfo.AddError(error);
					ValidateAH_DueDate();
				}
				if (Parent.AH_DueDate.IsEmpty && !Parent.AgePeriodInfo.HasErrors())
				{
					Parent.AgePeriodInfo.AddError(PeriodCreatedConcurrentError(Parent.AgePeriod, Parent.AgePeriodInfo.HumanReadableName, Parent.AH_DueDateInfo.HumanReadableName));
				}
			}
		}

		#endregion

		#endregion

		string ValidatePeriod(int period)
		{
			string message = ZString.Empty;
			if (Parent.AH_TransactionCategory.IsEmpty)
			{
				if (PeriodCalculator.IsPeriodGLClosed(period))
				{
					if (PeriodCalculator.IsPeriodSubledgerClosedForAdjustments(period))
					{
						message = ClosedPeriodError(period);
					}
					else
					{
						message = ClosedForGeneralLedgerPeriodError(period);
					}
				}
			}
			else
			{
				if (PeriodCalculator.IsPeriodSubledgerClosedForAdjustments(period))
				{
					if (PeriodCalculator.IsPeriodGLClosed(period))
					{
						message = ClosedPeriodError(period);
					}
					else
					{
						message = ClosedForAdjustmentsPeriodError(period);
					}
				}
			}
			return message;
		}

		new string ClosedPeriodError(int period)
		{
			return Res.GetString("55CE0F24-1587-44ca-9F66-CB605287A4D1", "{0} period is closed. You cannot post to closed periods.", period);
		}

		string ClosedForAdjustmentsPeriodError(int period)
		{
			return Res.GetString("BF9D49F7-8E02-4c8f-AB6A-D3D42E2A0510", "Period {0} is closed for Adjustments.  Only Regular Journals can now be created. A Presentation Category cannot be assigned.", period);
		}

		string ClosedForGeneralLedgerPeriodError(int period)
		{
			return Res.GetString("456E95B5-96BD-4058-B1E6-77843911049D", "The General Ledger is closed for {0} period.  Only Presentation Journals can now be created. A Presentation Category must be assigned.", period);
		}

		internal static string PeriodCreatedConcurrentError(int period, string periodName, string dependentFieldName)
		{
			return Res.GetString("B1F4C57B-3318-4B23-B9A2-E6A366DE868F", "The period {0} was created after entering {1}. Please delete and re-enter to set dependent field {2}", period, periodName, dependentFieldName);
		}

		protected new GLJournal Parent;
	}
}
