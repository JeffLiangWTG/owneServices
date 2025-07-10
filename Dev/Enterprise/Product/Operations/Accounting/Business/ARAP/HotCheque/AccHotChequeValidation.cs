using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Validation;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.HotCheque
{
	public class AccHotChequeValidation : AutoAccHotChequeValidation
	{
		public AccHotChequeValidation(AutoAccHotCheque parent)
			: base(parent)
		{
		}

		internal AccValidationHelper ValidationHelper
		{
			get
			{
				if (fValidationHelper == null)
				{
					fValidationHelper = new AccValidationHelper();
				}
				return fValidationHelper;
			}
		}
		AccValidationHelper fValidationHelper;

		new AccHotCheque Parent
		{
			get { return (AccHotCheque)base.Parent; }
		}

		protected override void CheckAQ_OH()
		{
			base.CheckAQ_OH();
			MandatoryValidation.CheckEntered(Parent.AQ_OHInfo);
			ListValidation.ErrorIfInvalidPK(Parent.AQ_OHInfo);
		}

		protected override void CheckAQ_AK()
		{
			base.CheckAQ_AK();
			MandatoryValidation.CheckEntered(Parent.AQ_AKInfo);

			if (Parent.ChequeBook != null && Parent.ChequeBook.BankAccount != null && !Parent.ChequeBook.BankAccount.AB_IsActive)
			{
				Parent.AQ_AKInfo.AddError(Res.GetString("8814EA07-EBE6-4D4F-B484-700C6B45C590", "This check book is linked to inactive bank account."));
			}

			ZString errorMessage = Parent.AutoAllocationValidation.GetErrorsForChequeBook(Parent.ChequeBook, Parent.IsChequeNumberAutoAllocated);
			if (!errorMessage.IsEmpty)
			{
				Parent.AQ_AKInfo.AddError(errorMessage);
			}

			if (!Parent.AQ_Printed && Parent.ChequeBook != null && Parent.ChequeBook.AK_AutoPrintCheque)
			{
				if (Parent.ChequeBook.BankAccount.AB_SO_ChequeTemplate.IsEmpty)
				{
					Parent.AQ_AKInfo.AddError(Res.GetString("5F76D46B-C2DA-4205-93BF-8474558D8CB2", "Auto printing of check is not configured properly."));
				}
				if (!Env.Security.PrintCheque.IsAllowed)
				{
					Parent.AQ_AKInfo.AddError(Res.GetString("2FEDEB85-A447-4b18-8F6C-4CAA1CBFA0DA", "You do not have the permission to print Check. Please Contact System Administrator."));
				}
			}
		}

		protected override void CheckAQ_ChequeNumber()
		{
			base.CheckAQ_ChequeNumber();

			if (!Parent.IsChequeNumberAutoAllocated)
			{
				MandatoryValidation.CheckEntered(Parent.AQ_ChequeNumberInfo);
				if (!Parent.AQ_ChequeNumberInfo.HasErrors())
				{
					AccChequeBook chequeBookCached = Parent.Factory.Load<AccChequeBook>(Parent.AQ_AK);

					ZString chequeNumberErrorMessage = ChequeOrReferenceValidationHelper.CheckIsNumbersLettersAllowed(true, Parent.AQ_ChequeNumber);
					if (chequeNumberErrorMessage.IsEmpty)
					{
						chequeNumberErrorMessage = ChequeOrReferenceValidationHelper.CheckIsChequeNumberNotInBook(chequeBookCached, Parent.AQ_ChequeNumber);
					}
					if (!chequeNumberErrorMessage.IsEmpty)
					{
						Parent.AQ_ChequeNumberInfo.AddError(chequeNumberErrorMessage);
					}
					else if (chequeBookCached != null && chequeBookCached.BankAccount != null)
					{
						ValidationHelper.ValidateChequeDigits(Parent.AQ_ChequeNumberInfo, chequeBookCached.BankAccount.AB_ChequeNumDigits);

						if (chequeBookCached.BankAccount.HasChequeNumberBeenUsed(Parent.AQ_ChequeNumber, Parent.PK))
						{
							Parent.AQ_ChequeNumberInfo.AddError(Res.GetString("1e887c0b-fb1b-4163-9625-f7ada462e2ed", "This check number is already in use"));
						}
						if (!Parent.AQ_ChequeNumberInfo.HasErrors() && !IsChequeNumberUnique)
						{
							Parent.AQ_ChequeNumberInfo.AddError(Res.GetString("1e887c0b-fb1b-4163-9625-f7ada462e2ed", "This check number is already in use"));
						}
					}
				}
			}
		}

		bool IsChequeNumberUnique
		{
			get
			{
				if (Parent.ChequeBook != null && Parent.ChequeBook.BankAccount != null)
				{
					ZDBOnlyQuery dBOnlyQuery = new ZDBOnlyQuery(typeof(AccHotCheque));
					dBOnlyQuery.AddToFilter(AccHotChequeSchema.AQ_ChequeNumber, Parent.AQ_ChequeNumber);
					dBOnlyQuery.AddToFilter(AccHotChequeSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
					ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(AccChequeBook), AccHotChequeSchema.AQ_AK);
					subQuery.AddToFilter(AccChequeBookSchema.AK_AB, Parent.ChequeBook.BankAccount.PK);

					dBOnlyQuery.AddSubQuery(subQuery, JoinCondition.And);
					AccHotCheque hotCheque = Parent.Factory.LoadTop1<AccHotCheque>(dBOnlyQuery);

					return (hotCheque == null);
				}
				else
				{
					return true;
				}
			}
		}

		protected override void CheckAQ_ChequePayee()
		{
			base.CheckAQ_ChequePayee();
			MandatoryValidation.CheckEntered(Parent.AQ_ChequePayeeInfo);
		}

		protected override void CheckAQ_Amount()
		{
			base.CheckAQ_Amount();
			CompareValidation.CheckNumberGreaterThanZero(Parent.AQ_AmountInfo);
		}

		protected override void CheckAQ_ChequeDate()
		{
			base.CheckAQ_ChequeDate();
			if (!Parent.AQ_ChequeDateInfo.HasErrors() && AccountingConfigurationRegistry.Instance.AccountingRequiredChequeDate.Value)
			{
				MandatoryValidation.CheckEntered(Parent.AQ_ChequeDateInfo);
			}
		}

		protected override void CheckAQ_JH()
		{
			base.CheckAQ_JH();
			if (!Parent.AQ_JHInfo.HasErrors() && AccountingConfigurationRegistry.Instance.AccountingRequiredJobNumber.Value)
			{
				MandatoryValidation.CheckEntered(Parent.AQ_JHInfo);
			}
		}

		protected override void CheckAQ_HouseBill()
		{
			base.CheckAQ_HouseBill();
			if (!Parent.AQ_HouseBillInfo.HasErrors() && AccountingConfigurationRegistry.Instance.AccountingRequiredHouseBillNo.Value)
			{
				MandatoryValidation.CheckEntered(Parent.AQ_HouseBillInfo);
			}
		}

		protected override void CheckAQ_MasterBill()
		{
			base.CheckAQ_MasterBill();
			if (!Parent.AQ_MasterBillInfo.HasErrors() && AccountingConfigurationRegistry.Instance.AccountingRequiredMasterBillNo.Value)
			{
				MandatoryValidation.CheckEntered(Parent.AQ_MasterBillInfo);
			}
		}

		protected override void CheckAQ_Description()
		{
			base.CheckAQ_Description();
			if (!Parent.AQ_DescriptionInfo.HasErrors() && AccountingConfigurationRegistry.Instance.AccountingRequiredDescription.Value)
			{
				MandatoryValidation.CheckEntered(Parent.AQ_DescriptionInfo);
			}
		}

		protected override void CheckAQ_GS_NKResponsibleStaff()
		{
			base.CheckAQ_GS_NKResponsibleStaff();
			ListValidation.ErrorIfInvalidCode(Parent.AQ_GS_NKResponsibleStaffInfo);
			if (!Parent.AQ_GS_NKResponsibleStaffInfo.HasErrors() && AccountingConfigurationRegistry.Instance.AccountingRequiredStaff.Value)
			{
				MandatoryValidation.CheckEntered(Parent.AQ_GS_NKResponsibleStaffInfo);
			}
		}
	}
}
