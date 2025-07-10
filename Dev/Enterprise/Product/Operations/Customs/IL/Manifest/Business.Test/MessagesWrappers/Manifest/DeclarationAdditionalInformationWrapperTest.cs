using System;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.IL.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationAdditionalInformationWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationAdditionalInformation>
	{
		public void TestNewOrNull()
		{
			AssertNull("When asycudaManifestHeader is null", DeclarationAdditionalInformationWrapper.NewOrNull(null));
			AssertNotNull("When asycudaManifestHeader is not null", DeclarationAdditionalInformationWrapper.NewOrNull(asycudaManifestHeader));
		}

		public void TestStatementCode()
		{
			var wrapper = Provider;
			AssertNull(wrapper.StatementCode?.Value);

			asycudaManifestHeader.AMA_TransportMode = "SEA";
			wrapper = GetProvider();
			AssertEquals("When sea", "1", wrapper.StatementCode.Value);

			asycudaManifestHeader.AMA_TransportMode = "ROA";
			wrapper = GetProvider();
			AssertEquals("When inland", "2", wrapper.StatementCode.Value);
		}

		public void TestStatementTypeCode()
		{
			var wrapper = Provider;
			AssertEquals("StatementTypeCode have to be constant ManifestStatementTypeCode", "1", Provider.StatementTypeCode.Value);
		}

		protected override IDeclarationAdditionalInformation GetProvider() => DeclarationAdditionalInformationWrapper.NewOrNull(asycudaManifestHeader);

		protected override void SetUp()
		{
			base.SetUp();

			disposableAction = ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ALL");
			asycudaManifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			asycudaManifestHeader.AMA_RN_NKCountry = "IL";
			asycudaManifestHeader.AMA_ApplicationCode = "NVC";
			asycudaManifestHeader.AMA_ManifestType = "785";
		}

		protected override void TearDown()
		{
			base.TearDown();
			disposableAction.Dispose();
		}

		IDisposable disposableAction;
		AsycudaManifestHeader asycudaManifestHeader;
	}
}
