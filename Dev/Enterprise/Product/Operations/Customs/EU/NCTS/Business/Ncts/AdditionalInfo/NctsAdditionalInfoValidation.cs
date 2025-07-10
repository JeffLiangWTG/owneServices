using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsAdditionalInfoValidation : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoValidation
	{
		public NctsAdditionalInfoValidation(NctsAdditionalInfo parent)
			: base(parent)
		{
		}

		protected override void CheckCSI_RN_NKCountryCode()
		{
			base.CheckCSI_RN_NKCountryCode();
			GoodsItemValidationHelper.CheckConditionC075(Parent.CSI_RN_NKCountryCodeInfo, Parent);
		}

		protected override void CheckCSI_NctsExportFromEC()
		{
			base.CheckCSI_NctsExportFromEC();
			GoodsItemValidationHelper.CheckConditionC075(Parent.CSI_NctsExportFromECInfo, Parent);
		}

		protected new NctsAdditionalInfo Parent => (NctsAdditionalInfo)base.Parent;

		protected override bool ShouldCodeBeInTheList => true;

		protected override void CheckCSI_Status()
		{
			if (IsOtherFieldsEnabled)
			{
				ValidateCSI_Code();
			}
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();

			var parent = Parent;
			if (parent.ParentAsNctsHeader is NctsHeader header)
			{
				var cusCodeList = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(parent.Factory, parent.CSI_Code, header.DefaultDataGroupingCode, parent.CodeListType, ZDateTime.Today);

				var codeHasReference = cusCodeList?.HasAttribute(UniversalReferenceConstants.RefCusCodeListAttributeTypes.Reference, UniversalReferenceConstants.RefCusCodeListAttributeValues.Yes) ?? false;

				if (codeHasReference)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_ReferenceNumberInfo);
				}
			}
		}

		protected override void CheckCSI_Description()
		{
			base.CheckCSI_Description();

			var parent = Parent;
			if (parent.ParentAsNctsHeader != null)
			{
				if (parent.CSI_Code == UniversalReferenceConstants.AdditionalDocumentTypes.T0000)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_DescriptionInfo);
				}
			}
		}
	}
}
