using System;
using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_820;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class ManifestQueryCargoIdentifierWrapperTest : Customs.Business.Testing.DataProviderTestCase<ICargoIdentifier>
	{
		public void TestNewOrNull()
		{
			AssertNull("When messageSendingObject is null", ManifestQueryCargoIdentifierWrapper.NewOrNull(null));
			AssertNotNull("When messageSendingObject is not null", ManifestQueryCargoIdentifierWrapper.NewOrNull(new AsycudaManifestQueryMessageSendingObject(asycudaManifestHeader)));
		}

		public void TestCargoIdentifierKey1()
		{
			AssertEquals("123", Provider.CargoIdentifierKey1);
		}

		public void TestCargoIdentifierKey2()
		{
			AssertEquals("456", Provider.CargoIdentifierKey2);
		}

		public void TestCargoIdentifierKey3()
		{
			AssertNull(Provider.CargoIdentifierKey3);
		}

		public void TestCargoIdentifierType()
		{
			AssertEquals(11, Provider.CargoIdentifierType);
		}

		protected override ICargoIdentifier GetProvider()
		{
			var asycudaManifestQueryHeaderWrapper = asycudaManifestHeader.MessageSendingConfiguration.GetNewQueryMessageSendingObjectParent(asycudaManifestHeader);
			return ManifestQueryCargoIdentifierWrapper.NewOrNull((AsycudaManifestQueryMessageSendingObject)asycudaManifestQueryHeaderWrapper.SelectedSendingObjects.Single());
		}

		protected override void SetUp()
		{
			base.SetUp();
			disposableAction = ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ALL");
			asycudaManifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			asycudaManifestHeader.AMA_RN_NKCountry = "IL";
			asycudaManifestHeader.AMA_ApplicationCode = "NVC";
			asycudaManifestHeader.AMA_ManifestType = "785";
			asycudaManifestHeader.AMA_ManifestNumber = "123";
			var asycudaBill = asycudaManifestHeader.Bills.AddNew();
			var asycudaTransportDocumentInfo = asycudaBill.TransportDocuments.AddNew();
			asycudaTransportDocumentInfo.CSI_Code = TransportDocsTypeList.Codes.IL2;
			asycudaTransportDocumentInfo.CSI_ReferenceNumber = "456";
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


