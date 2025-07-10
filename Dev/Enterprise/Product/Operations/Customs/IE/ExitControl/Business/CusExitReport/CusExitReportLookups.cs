using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using UniversalReferenceConstants = Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class CusExitReportLookups : EU.ExitControl.Business.CusExitReportLookups
	{
		public CusExitReportLookups(CusExitReport parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList AdditionalDeclarationTypes => Factory.GetCachedValue<EntrySubStyleList>();
		public CodeDescriptionPairList EnquiryInformationCodeTypes => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL210, ZDateTime.Today);
		public CustomsOfficeCodeCollection OfficeOfExportList => CustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredRoles(Factory, Parent.CountryCode, EuOfficeCodesTypes.Codes.OfficeOfExit, EuOfficeCodesTypes.Codes.OfficeOfExitInland);

		protected override CodeDescriptionPairList MessageStatusListCore => Factory.GetCachedValue<IELogicalStatusList>();

		protected new CusExitReport Parent => (CusExitReport)base.Parent;
	}
}
