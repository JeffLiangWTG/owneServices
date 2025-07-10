using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class JobDeclarationLightValidationTester : LightValidationTester
	{
		public JobDeclarationLightValidationTester(BusinessObject businessObject)
			: base(businessObject)
		{
		}

		protected override bool ShouldTestProperty(ZPropertyInfo info)
		{
			if (info.BizObj is JobDocAddress docAddress)
			{
				var addressType = docAddress.E2_AddressType;
				return addressType != DocAddressTypes.Codes.CFIAPaymentParty && addressType != DocAddressTypes.Codes.CommercialInvoiceOriginator && addressType != DocAddressTypes.Codes.Manufacturer;
			}
			else
			{
				return base.ShouldTestProperty(info);
			}
		}
	}
}
