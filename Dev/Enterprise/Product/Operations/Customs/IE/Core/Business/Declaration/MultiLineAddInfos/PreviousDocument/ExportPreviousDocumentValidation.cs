using CargoWise.Types;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ExportPreviousDocumentValidation : CommonPreviousDocumentValidation
	{
		const string PreviousDocumentType = "PRE";

		public ExportPreviousDocumentValidation(PreviousDocument parent) : base(parent)
		{
		}

		protected override void CheckCSI_ItemNumber()
		{
			base.CheckCSI_ItemNumber();
			if (Parent.CSI_Type.Equals(PreviousDocumentType)
				&& (Parent.CSI_Code == PreviousDocumentTypeList.Codes.AdministrativeAccompanyingDocument || Parent.CSI_Code == PreviousDocumentTypeList.Codes.FallbackEAD)
				&& Parent.CSI_ItemNumber.Equals(ZInt.Zero))
			{
				Parent.CSI_ItemNumberInfo.AddMessageError(Res.GetString("8E3AE38E-2015-43D5-8196-D618C9B64369", "When Previous Document is in either C651 or C658, then Previous Document Item No. is required."));
			}
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();

			var parent = Parent;
			if (parent.CSI_ReferenceNumber.Length > 35 && parent.IsParentInvoiceOrLineOrCusEntryAndIsTransitionPeriod)
			{
				parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("D327E508-E623-483C-88FF-216C32928FDB", "Reference Number of Previous Document can have up to 35 alpha numeric characters."));
			}
		}
	}
}
