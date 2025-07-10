using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ExportSupportingDocumentValidation : CommonSupportingDocumentValidation
	{
		public ExportSupportingDocumentValidation(SupportingDocument parent) : base(parent)
		{
		}

		protected override bool ShouldCheckReferenceNumberMandatoryBasedOnCusConditions => false;

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);

			var parent = Parent;
			if (parent.CSI_ReferenceNumber.Length > 35 && parent.IsParentInvoiceOrLineOrCusEntryAndIsTransitionPeriod)
			{
				parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("D327E508-E623-483C-88FF-216C32928FDC", "Reference Number of Supporting Document can have up to 35 alpha numeric characters."));
			}
		}
	}
}
