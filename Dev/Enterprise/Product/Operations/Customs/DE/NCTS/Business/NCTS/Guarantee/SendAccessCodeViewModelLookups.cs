using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class SendAccessCodeViewModelLookups : ZLookups
	{
		public SendAccessCodeViewModelLookups(SendAccessCodeViewModel parent, BusinessObjectFactory factory)
			: base(parent)
		{
			Factory = factory;
		}

		protected override BusinessObjectFactory Factory { get; }

		public CustomsOfficeCodeCollection Offices => CustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredRoles(
			Factory, Constants.CountryCodes.Germany, EuOfficeCodesTypes.Codes.OfficeOfGuarantee);
	}
}
