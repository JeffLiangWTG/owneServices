using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.AccStatement;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Auto
{
	public class AccStatementValidation : AutoAccStatementValidation
	{
		public AccStatementValidation(AutoAccStatement parent)
			: base(parent)
		{
		}

		new Statement Parent
		{
			get { return (Statement)base.Parent; }
		}

		protected override void CheckAS_DebitCredit()
		{
			base.CheckAS_DebitCredit();
			MandatoryValidation.CheckEntered(Parent.AS_DebitCreditInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AS_DebitCreditInfo);
		}

		protected override void CheckAS_Amount()
		{
			base.CheckAS_Amount();

			if (Parent.AS_Amount <= 0.0m)
			{
				Parent.AS_AmountInfo.AddError(Res.GetString("ff3a4b30-4c02-4390-8bf1-e1ffe83ef09c", "Amount should be positive."));
			}
		}

		protected override void CheckAS_Type()
		{
			base.CheckAS_Type();
			MandatoryValidation.CheckEntered(Parent.AS_TypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AS_TypeInfo);
		}

		protected override void CheckAS_ChequeOrReference()
		{
			base.CheckAS_ChequeOrReference();
			if (Parent.AS_Type != ReceiptTypes.DirectDebit && Parent.AS_Type != ReceiptTypes.DirectCredit)
			{
				MandatoryValidation.CheckEntered(Parent.AS_ChequeOrReferenceInfo);
			}
			else
			{
				string referenceWarning = Res.GetString("7b922bec-e732-4076-89b9-df63d63e3f64", "The Reference field for DCR and DDR statement entry types is not mandatory.\r\nPlease confirm if the field should remain blank as it may be used when auto reconciling with your cash book transactions.");
				Parent.AS_ChequeOrReferenceInfo.AddWarning(referenceWarning);
			}
		}

		protected override void CheckAS_StatementDate()
		{
			base.CheckAS_StatementDate();
			MandatoryValidation.CheckEntered(Parent.AS_StatementDateInfo);
		}

		protected override void CheckAS_PageNumber()
		{
			base.CheckAS_PageNumber();
			MandatoryValidation.CheckEntered(Parent.AS_PageNumberInfo);

			if (!Parent.AS_PageNumberInfo.HasErrors())
			{
				if (Parent.Master != null && !((BankStatement)Parent.Master).IsPageExistForThisCollection(Parent.AS_PageNumber - 1))
				{
					Parent.AS_PageNumberInfo.AddError(Res.GetString("c5f1651d-e4a6-4306-80eb-372de8befbc2", "You can only enter next page number"));
				}
			}
		}
	}
}
