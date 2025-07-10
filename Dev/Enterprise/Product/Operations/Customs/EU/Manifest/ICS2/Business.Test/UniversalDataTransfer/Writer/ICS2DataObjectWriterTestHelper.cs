using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	static class ICS2DataObjectWriterTestHelper
	{
		public static AsycudaManifestHeader SetupManifestHeader(UniversalObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory.BOFactory);
			_ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			_ = helper.CreateNewOrGetExistingCusCodeList(
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IC2MS,
				Core.Constants.CountryCodes.France,
				nameof(Core.Constants.CountryCodes.France),
				ZDateTime.Today.AddYears(-2),
				ZDateTime.Today.AddYears(2));

			factory.SaveForTesting();

			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			header.AddressedMemberState = Core.Constants.CountryCodes.France;

			return header;
		}
	}
}
