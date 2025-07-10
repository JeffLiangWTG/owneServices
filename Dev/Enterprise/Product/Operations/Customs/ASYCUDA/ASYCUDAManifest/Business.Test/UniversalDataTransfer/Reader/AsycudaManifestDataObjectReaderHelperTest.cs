using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Testing.Core;

namespace Enterprise.Customs.ASYCUDAManifest.Business.UniversalDataTransfer.Testing
{
	sealed class AsycudaManifestDataObjectReaderHelperTest : TestCaseWithUniversalObjectFactory
	{
		public void TestBillDataObjectReaderType()
		{
			var header = Factory.BOFactory.New<AsycudaManifestHeader>();
			var helper = new AsycudaManifestDataObjectReaderHelper(Core.Constants.CountryCodes.Bangladesh, Factory.BOFactory);
			AssertEquals("AsycudaBillDataObjectReader Type", typeof(AsycudaBillDataObjectReader), helper.GetBillDataObjectReader(new Shipment(), new TestErrorLogger(), Factory, header, helper, false).GetType());
		}
	}
}
