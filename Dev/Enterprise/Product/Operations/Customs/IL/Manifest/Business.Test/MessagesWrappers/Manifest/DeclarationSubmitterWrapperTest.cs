using System;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.IL.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationSubmitterWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationSubmitter>
	{
		public void TestNewOrNull()
		{
			AssertNull("When asycudaManifestHeader is null", DeclarationSubmitterWrapper.NewOrNull(null));
			AssertNotNull("When asycudaManifestHeader is not null, with Declarant valid Israeli VAT", DeclarationSubmitterWrapper.NewOrNull(asycudaManifestHeader));

			asycudaManifestHeader.Declarant?.Header.CustomsCodes.RemoveAndDeleteAll();
			AssertNull("When asycudaManifestHeader is not null, but without Declarant valid Israeli VAT", DeclarationSubmitterWrapper.NewOrNull(asycudaManifestHeader));
		}

		public void TestId()
		{
			var wrapper = Provider;
			AssertEquals("When Declarant have valid Israeli VAT", "689542378", wrapper.Id.Value);
		}

		protected override IDeclarationSubmitter GetProvider() => DeclarationSubmitterWrapper.NewOrNull(asycudaManifestHeader);

		protected override void SetUp()
		{
			base.SetUp();
			disposableAction = ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ALL");

			var factory = Factory;
			asycudaManifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			asycudaManifestHeader.AMA_RN_NKCountry = "IL";
			asycudaManifestHeader.AMA_ApplicationCode = "NVC";
			asycudaManifestHeader.AMA_ManifestType = "785";

			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = factory.NewWithValidTestData<OrgAddress>();
			orgHeader.OH_Code = "DEC1";
			orgAddress.OA_OH = orgHeader.PK;
			orgHeader.CustomsCodes.AddNew("VAT", "689542378", "IL");
			asycudaManifestHeader.AMA_OA_Declarant = orgAddress.PK;
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
