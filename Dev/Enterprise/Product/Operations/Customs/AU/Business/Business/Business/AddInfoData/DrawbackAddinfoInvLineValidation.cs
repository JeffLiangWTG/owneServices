using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DrawbackAddinfoInvLineValidation : AUAddInfoLineValidation
	{
		public DrawbackAddinfoInvLineValidation(AUAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckZA_DAM_Hidden()
		{
			base.CheckZA_DAM_Hidden();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ZA_DAM_HiddenInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.ZA_DAM_HiddenInfo, Parent.Lookups.ZA_DAM_List);
		}

		protected override void CheckZA_DARC_Hidden()
		{
			base.CheckZA_DARC_Hidden();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZA_DARC_HiddenInfo, Parent.Lookups.DrawbackAmberCodeList);
		}

		protected override void CheckZA_PRF()
		{
		}

		protected override void CheckZA_DDN_Hidden()
		{
			base.CheckZA_DDN_Hidden();
			new CANValidation().ValidateCAN(Parent.ZA_DDN_HiddenInfo);
			if (InvoiceLine != null)
			{
				if (InvoiceLine.DrawbackAssesmentMethod == JobDeclaration.DrawbackAssessmentMethods.RepresentativeShipment &&
					!InvoiceLine.DrawbackImportDeclarationNumber.IsEmpty && InvoiceLine.DrawbackCusEntryLineCollection.Count > 0)
				{
					AddInfo.ZA_DDN_HiddenInfo.AddError("Enter either an Import Declaration Number or use the button to select multiple Import Entries to calculate an average, but not both on the same line.");
				}
			}
		}

		protected override void CheckZA_EDN_Hidden()
		{
			base.CheckZA_EDN_Hidden();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ZA_EDN_HiddenInfo);
			new Common.AU.CMR.CANValidation().ValidateCANField(Parent.ZA_EDN_HiddenInfo);
		}

		protected override void CheckZA_DDT_Hidden()
		{
			base.CheckZA_DDT_Hidden();
			if (Parent.Parent is JobComInvoiceLine && !Parent.InvoiceLine.DrawbackDutyAmountOverriden.IsEmpty && Parent.InvoiceLine.DrawbackDutyAmountOverriden != Parent.ZA_DDT_Hidden)
			{
				Parent.ZA_DDT_HiddenInfo.AddWarning(DrawbackValueOverrideWarningMessage + " " + Parent.InvoiceLine.DrawbackDutyAmountOverriden.ToString(2));
			}
		}
		public const string DrawbackValueOverrideWarningMessage = "The system has recalculated and overriden a value for this field.\r\nThe value before recalculation was ";

		protected override void CheckZA_DCV_Hidden()
		{
			base.CheckZA_DCV_Hidden();
			if (Parent.Parent is JobComInvoiceLine && !Parent.InvoiceLine.DrawbackCustomsValueOverriden.IsEmpty && Parent.InvoiceLine.DrawbackCustomsValueOverriden != Parent.ZA_DCV_Hidden)
			{
				Parent.ZA_DCV_HiddenInfo.AddWarning(DrawbackValueOverrideWarningMessage + " " + Parent.InvoiceLine.DrawbackCustomsValueOverriden.ToString(2));
			}
		}
	}
}
