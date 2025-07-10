using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FJ.Manifest.Business.Testing
{
	public class AsycudaManifestHeaderValidationTest : TestCaseWithFactory
	{
		public virtual void TestCheckAMA_RN_NKConveyanceNationality()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = "SEA";
			header.AMA_RN_NKConveyanceNationality = "XX";
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Fiji;
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_RN_NKConveyanceNationality = ZString.Empty;
			AssertHasMessageErrorContaining(header.AMA_RN_NKConveyanceNationalityInfo, MandatoryValidation.YouHaveNotEntered);
			header.AMA_RN_NKConveyanceNationality = Core.Constants.CountryCodes.Singapore;
			AssertNoMessageErrorContaining(header.AMA_RN_NKConveyanceNationalityInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckAMA_OA_ShippingAgent()
		{
			var shippingAgent = Factory.NewWithValidTestData<OrgHeader>();

			var shippingAgentWithAgentCode = Factory.NewWithValidTestData<OrgHeader>();
			shippingAgentWithAgentCode.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "123456", CountryCodes.Fiji);

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_OA_ShippingAgent = shippingAgent.MainAddress.PK;
			AssertHasMessageError(header.AMA_OA_ShippingAgentInfo, "A Shipping Agent code cannot be determined. This organization does not contain a CCD code for FJ.");

			header.AMA_OA_ShippingAgent = shippingAgentWithAgentCode.MainAddress.PK;
			AssertNoMessageError(header.AMA_OA_ShippingAgentInfo, "A Shipping Agent code cannot be determined. This organization does not contain a CCD code for FJ.");
		}
	}
}
