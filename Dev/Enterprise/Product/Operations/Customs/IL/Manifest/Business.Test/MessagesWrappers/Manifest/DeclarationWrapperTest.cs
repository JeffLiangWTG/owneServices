using System;
using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclaration>
	{
		public void TestNewOrNull()
		{
			AssertNull(DeclarationWrapper.NewOrNull(null));
			AssertNotNull(DeclarationWrapper.NewOrNull(asycudaManifestHeader));
		}

		public void TestAdditionalInformation()
		{
			var wrapper = GetProvider();
			var additionalInformation = wrapper.AdditionalInformation;
			AssertNotNull(nameof(IDeclaration.AdditionalInformation), additionalInformation);
			AssertEquals("Count", 1, additionalInformation.Count);

			asycudaManifestHeader.AMA_TransportMode = "SEA";
			wrapper = GetProvider();
			additionalInformation = wrapper.AdditionalInformation;
			AssertNotNull(nameof(IDeclaration.AdditionalInformation), additionalInformation);
			AssertEquals("Count", 1, additionalInformation.Count);
			var additionalInformation1 = additionalInformation.Single();
			AssertType<DeclarationAdditionalInformationWrapper>(additionalInformation1);
			AssertSame("Cached", additionalInformation, wrapper.AdditionalInformation);
			AssertEquals("When Sea, StatementCode", "1", additionalInformation1.StatementCode.Value);
			AssertEquals("When Sea, StatementTypeCode", "1", additionalInformation1.StatementTypeCode.Value);

			asycudaManifestHeader.AMA_TransportMode = "ROA";
			wrapper = GetProvider();
			additionalInformation = wrapper.AdditionalInformation;
			AssertNotNull(nameof(IDeclaration.AdditionalInformation), additionalInformation);
			AssertEquals("Count", 1, additionalInformation.Count);
			additionalInformation1 = additionalInformation.Single();
			AssertType<DeclarationAdditionalInformationWrapper>(additionalInformation1);
			AssertSame("Cached", additionalInformation, wrapper.AdditionalInformation);
			AssertEquals("When Inland, StatementCode", "2", additionalInformation1.StatementCode.Value);
			AssertEquals("When Inland, StatementTypeCode", "1", additionalInformation1.StatementTypeCode.Value);
		}

		public void TestConsignment()
		{
			AssertNotNull(Provider.Consignment);
			var consignment = Provider.Consignment;
			AssertNotNull(nameof(IDeclaration.Consignment), consignment);
			AssertEquals("When Bill type = HBW", 1, consignment.Count);
			AssertType<DeclarationConsignmentWrapper>(consignment.Single());

			bill.ABL_BolType = "";
			var wrapper = GetProvider();
			AssertEquals("When Bill type != HBW", 0, wrapper.Consignment.Count);
		}

		public void TestId()
		{
			var wrapper = GetProvider();
			AssertNull("when ManifestNumber is empty", wrapper.Id);

			asycudaManifestHeader.AMA_ManifestNumber = "I922021539627555";
			wrapper = GetProvider();
			AssertEquals("when ManifestNumber is not empty", "I922021539627555", wrapper.Id.Value);
		}

		public void TestSubmitter()
		{
			var wrapper = GetProvider();
			AssertNull(wrapper.Submitter);

			var factory = Factory;
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = factory.NewWithValidTestData<OrgAddress>();
			orgHeader.OH_Code = "DEC1";
			orgAddress.OA_OH = orgHeader.PK;
			orgHeader.CustomsCodes.AddNew("VAT", "689542378", "IL");

			asycudaManifestHeader.AMA_OA_Declarant = orgAddress.PK;
			wrapper = GetProvider();
			AssertEquals("689542378", wrapper.Submitter.Id.Value);
		}

		public void TestTypeCode()
		{
			var wrapper = GetProvider();
			AssertEquals("785", wrapper.TypeCode.Value);
		}

		public void TestAgent()
		{
			var wrapper = GetProvider();
			AssertNull(wrapper.Agent);
		}

		public void TestBorderTransportMeans()
		{
			var wrapper = GetProvider();
			var borderTransportMeans = wrapper.BorderTransportMeans;
			AssertNotNull(nameof(IDeclaration.BorderTransportMeans), borderTransportMeans);
			AssertEquals("Count", 0, borderTransportMeans.Count);

			asycudaManifestHeader.TransportMeans.AddNew();
			asycudaManifestHeader.TransportMeans.AddNew();

			wrapper = GetProvider();
			AssertEquals("There should be 2 Transport Means", 2, wrapper.BorderTransportMeans.Count);
		}

		public void TestCarrier()
		{
			var wrapper = GetProvider();
			var carrier = wrapper.Carrier;
			AssertNotNull(nameof(IDeclaration.Carrier), carrier);
			AssertEquals("Count", 1, wrapper.Carrier.Count);
			AssertNull(wrapper.Carrier.FirstOrDefault()?.Id);

			var factory = Factory;
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = factory.NewWithValidTestData<OrgAddress>();
			orgHeader.OH_Code = "DEC1";
			orgAddress.OA_OH = orgHeader.PK;
			orgHeader.CustomsCodes.AddNew("VAT", "689542378", "IL");

			asycudaManifestHeader.AMA_OA_Carrier = orgAddress.PK;
			wrapper = GetProvider();
			AssertEquals("689542378", wrapper.Carrier.FirstOrDefault().Id.Value);
		}

		protected override IDeclaration GetProvider() => DeclarationWrapper.NewOrNull(asycudaManifestHeader);

		protected override void SetUp()
		{
			base.SetUp();
			disposableAction = ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ALL");
			asycudaManifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			asycudaManifestHeader.AMA_RN_NKCountry = "IL";
			asycudaManifestHeader.AMA_ApplicationCode = "NVC";
			asycudaManifestHeader.AMA_ManifestType = "785";

			bill = asycudaManifestHeader.Bills.AddNew();
		}

		protected override void TearDown()
		{
			base.TearDown();
			disposableAction.Dispose();
		}

		IDisposable disposableAction;
		AsycudaManifestHeader asycudaManifestHeader;
		AsycudaBill bill;
	}
}
