using CargoWise.Customs.IL.MessageDefinitions.DOC.REQ_271;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class ConnectedEntityWrapperTest : Customs.Business.Testing.DataProviderTestCase<IConnectedEntity>
	{
		public void TestNewOrNull()
		{
			var factory = Factory;
			AssertNull("When SupportingDocument is null", ConnectedEntityWrapper.NewOrNull(null));
			AssertNull("When SupportingDocument parent is not bill", ConnectedEntityWrapper.NewOrNull(factory.New<SupportingDocument>()));
			AssertNull("When SupportingDocument parent is bill without header", ConnectedEntityWrapper.NewOrNull(factory.New<AsycudaBill>().SupportingDocuments.AddNew()));
			AssertNotNull("When SupportingDocument parent is bill with header", Provider);
		}

		public void TestEntityType()
		{
			AssertEquals("Entity Type is constant 11152", 11152, Provider.EntityType);
		}

		public void TestEntityIdKey1()
		{
			AssertEquals("When TransportMode is Sea", "MAN123", Provider.EntityIdKey1);
			AssertEquals("When TransportMode is Road", "MAN123", Provider.EntityIdKey1);
			AssertEquals("When TransportMode is not Sea or Road", "MAN123", Provider.EntityIdKey1);
		}

		public void TestEntityIdKey2()
		{
			AssertEquals("When TransportMode is not sea", ZString.Empty, Provider.EntityIdKey2);
			header.AMA_TransportMode = "SEA";
			AssertEquals("When TransportMode is Sea and CSI_Code is IL1", "REFIL1", Provider.EntityIdKey2);
			header.AMA_TransportMode = "SEA";
			asycudaTransportDocumentInfo.CSI_Code = "IL2";
			AssertEquals("When TransportMode is Sea and CSI_Code is not IL1", ZString.Empty, Provider.EntityIdKey2);
		}

		public void TestEntityIdKey3()
		{
			AssertNull("EntityIdKey3 is always null", Provider.EntityIdKey3);
		}

		public void TestEntityIdExternalReferenceId()
		{
			AssertNull("EntityIdExternalReferenceId is always null", Provider.EntityIdExternalReferenceId);
		}

		public void TestEntityPath()
		{
			AssertEquals("Entity Path is constant 11", "11", Provider.EntityPath);
		}

		protected override IConnectedEntity GetProvider()
			=> ConnectedEntityWrapper.NewOrNull(supportingDocument);

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestNumber = "MAN123";
			var bill = header.Bills.AddNew();
			asycudaTransportDocumentInfo = bill.TransportDocuments.AddNew();
			asycudaTransportDocumentInfo.CSI_Code = "IL1";
			asycudaTransportDocumentInfo.CSI_ReferenceNumber = "REFIL1";
			supportingDocument = bill.SupportingDocuments.AddNew();
		}

		SupportingDocument supportingDocument;
		AsycudaManifestHeader header;
		AsycudaTransportDocumentInfo asycudaTransportDocumentInfo;
	}
}
