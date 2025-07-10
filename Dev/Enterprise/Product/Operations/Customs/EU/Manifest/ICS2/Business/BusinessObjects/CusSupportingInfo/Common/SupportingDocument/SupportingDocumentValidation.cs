using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SupportingDocumentValidation : Customs.Business.CusSupportingInfoValidation
	{
		public SupportingDocumentValidation(SupportingDocument parent)
			: base(parent)
		{
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();

			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_CodeInfo, ResString.GetMultilingualString("063E35A7-6CFE-4C81-86B1-8FA78F26ED59", "Supporting Document Type"));
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo, Res.GetString("D92DA734-14C7-4D42-ACA2-61064438453A", "Supporting Document Number"));
		}

		protected override void CheckCSI_Type()
		{
			base.CheckCSI_Type();
			var supportingDocument = Parent as SupportingDocument;

			supportingDocument.RemoveRowWarning(IncompatibleSpecificCircumstanceIndicatorWarningMessage);
			if (!IsSupportingDocumentParentCompatibleWithSpecificCircumstanceIndicator(supportingDocument))
			{
				supportingDocument.AddRowWarning(IncompatibleSpecificCircumstanceIndicatorWarningMessage);
			}
		}

		bool IsSupportingDocumentParentCompatibleWithSpecificCircumstanceIndicator(SupportingDocument supportingDocument)
		{
			switch (supportingDocument.CSI_ParentTableCode)
			{
				case AsycudaManifestHeaderSchema.Constants.Prefix:
					var header = supportingDocument.Parent as AsycudaManifestHeader;
					return header != null && IndicatorHelper.IsSpecificCircumstanceIndicatorApplicableForManifestHeaderSupportingDocuments(header.SpecificCircumstanceIndicator);
				case AsycudaBillSchema.Constants.Prefix:
					header = (supportingDocument.Parent as AsycudaBill)?.Header;
					return header != null && IndicatorHelper.IsSpecificCircumstanceIndicatorApplicableForBillSupportingDocuments(header.SpecificCircumstanceIndicator);
				case AsycudaPackSchema.Constants.Prefix:
					header = (supportingDocument.Parent as AsycudaPack)?.Bill?.Header as AsycudaManifestHeader;
					return header != null && IndicatorHelper.IsSpecificCircumstanceIndicatorApplicableForPacksSupportingDocuments(header.SpecificCircumstanceIndicator);
				default:
					return true;
			}
		}

		string IncompatibleSpecificCircumstanceIndicatorWarningMessage => Res.GetString("D4971607-52FE-4C2E-95DA-6A75A2684655", "The Specific Circumstance does not support Supporting Document details at this level. These will not be sent in the ICS2 message.");
	}
}
