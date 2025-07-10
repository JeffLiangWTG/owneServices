using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSInvoiceLineCusOutturnValidation : CusOutturnValidation
	{
		public EMCSInvoiceLineCusOutturnValidation(EMCSInvoiceLineCusOutturn parent) : base(parent)
		{
		}

		protected new EMCSInvoiceLineCusOutturn Parent => (EMCSInvoiceLineCusOutturn)base.Parent;

		protected EMCSJobComInvoiceLine InvoiceLine => Parent.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateObservedDifference();
		}

		protected override void CheckC5_RejectedQuantity()
		{
			base.CheckC5_RejectedQuantity();

			var parent = Parent;
			var propertyInfo = parent.C5_RejectedQuantityInfo;
			MandatoryValidation.CheckNotNegative(propertyInfo);

			var rejectedQuantity = parent.C5_RejectedQuantity;
			if (rejectedQuantity > 0 && parent.ReportOfReceiptReasons.Count == 0)
			{
				propertyInfo.AddMessageError(Res.GetString("02E112AE-10D0-456C-B0A5-967974A130F8", "You have not entered a Reason."));
			}

			if (rejectedQuantity > InvoiceLine.JI_CustomsQuantity)
			{
				propertyInfo.AddMessageError(Res.GetString("7E167FC4-102E-4441-81D1-CCE5A1434525", "The entered Refused Quantity must be less than or equal the Customs Quantity."));
			}
		}

		protected override void CheckC5_OutturnResultReason()
		{
			base.CheckC5_OutturnResultReason();
			if (Parent.ObservedDifference != 0 && InvoiceLine.Declaration.ZG_ExplanationOnReasonForShortageValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.C5_OutturnResultReasonInfo, Res.GetString("C1D374C8-C053-4B05-9066-E721691D1AE5", "Explanation for Excess or Shortage"));
			}
		}

		public void ValidateObservedDifference()
		{
			ValidateCalculatedProperty(Parent.ObservedDifferenceInfo);
		}

		protected void CheckObservedDifference()
		{
			var parent = Parent;
			if (InvoiceLine.Declaration.JE_DeclarantType.EqualsIgnoringCase(EMCSEntryTypeList.Codes.Consignee) && parent.ObservedDifference != 0 && !parent.ReportOfReceiptReasons.Any())
			{
				parent.ObservedDifferenceInfo.AddMessageError(Res.GetString("5CEE5C74-2D4E-459F-89C1-E3DBC3994485", "Please provide a reason"));
			}
		}
	}
}
