using System;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.IL.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationCarrierWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationCarrier>
	{
		public void TestNewOrNull()
		{
			AssertNull("When asycudaManifestHeader is null", DeclarationCarrierWrapper.NewOrNull(null));
			AssertNotNull("When asycudaManifestHeader is not null,  with Carrier valid Israeli VAT", DeclarationCarrierWrapper.NewOrNull(asycudaManifestHeader));

			asycudaManifestHeader.Carrier?.Header.CustomsCodes.RemoveAndDeleteAll();
			AssertNull("When asycudaManifestHeader is not null, but without Carrier valid Israeli VAT", DeclarationSubmitterWrapper.NewOrNull(asycudaManifestHeader));
		}

		public void TestId()
		{
			var wrapper = Provider;
			AssertEquals("ID should equal IL carrier VAT.", "689542378", wrapper.Id.Value);
		}

		protected override IDeclarationCarrier GetProvider() => DeclarationCarrierWrapper.NewOrNull(asycudaManifestHeader);

		protected override void SetUp()
		{
			base.SetUp();

			disposableAction = ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ALL");
			var factory = Factory;

			asycudaManifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			asycudaManifestHeader.AMA_RN_NKCountry = "IL";
			asycudaManifestHeader.AMA_ApplicationCode = "NVC";
			asycudaManifestHeader.AMA_ManifestType = "785";

			var carrier = factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "CARRIER";

			var carrierAddress = carrier.MainAddress;
			carrierAddress.CustomsCodes.AddNew("VAT", "689542378", Core.Constants.CountryCodes.Israel);
			asycudaManifestHeader.AMA_OA_Carrier = carrierAddress.PK;
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
