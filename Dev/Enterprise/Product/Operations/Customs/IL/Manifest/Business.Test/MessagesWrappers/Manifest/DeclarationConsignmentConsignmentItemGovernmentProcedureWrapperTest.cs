using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationConsignmentConsignmentItemGovernmentProcedureWrapperTest : DataProviderTestCase<IDeclarationConsignmentConsignmentItemGovernmentProcedure>
	{
		public void TestCurrentCode()
		{
			asycudaManifestHeader.AMA_Nature = null;
			AssertNull("Code should be null when Manifest Nature is not Import or Export", Provider.CurrentCode?.Value);
			asycudaManifestHeader.AMA_Nature = ShipmentTypeList.Codes.Import23;
			var wrapper = GetProvider();
			AssertEquals("Code should be 4000000 when Manifest Nature is Import", "4000000", wrapper.CurrentCode?.Value);
			asycudaManifestHeader.AMA_Nature = ShipmentTypeList.Codes.Export22;
			wrapper = GetProvider();
			AssertEquals("Code should be 1000000 when Manifest Nature is Export", "1000000", wrapper.CurrentCode?.Value);
		}

		public void TestNewOrNull()
		{
			AssertNull(DeclarationConsignmentConsignmentItemGovernmentProcedureWrapper.NewOrNull(null));
			AssertNotNull(DeclarationConsignmentConsignmentItemGovernmentProcedureWrapper.NewOrNull(Factory.New<AsycudaManifestHeader>()));
		}

		protected override IDeclarationConsignmentConsignmentItemGovernmentProcedure GetProvider() => DeclarationConsignmentConsignmentItemGovernmentProcedureWrapper.NewOrNull(asycudaManifestHeader);

		protected override void SetUp()
		{
			base.SetUp();
			asycudaManifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		}

		AsycudaManifestHeader asycudaManifestHeader;
	}
}
