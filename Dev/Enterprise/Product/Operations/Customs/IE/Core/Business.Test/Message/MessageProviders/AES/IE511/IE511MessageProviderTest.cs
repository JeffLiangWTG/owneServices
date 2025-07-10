using System;
using System.Linq;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class IE511MessageProviderTest : Customs.Business.Testing.DataProviderTestCase<IE511MessageProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("EntryHeader missing", () => new IE511MessageProvider(null));

				var entryHeader = Factory.New<CusEntryHeader>();
				AssertExceptionThrown<ArgumentException>("Declaration missing", () => new IE511MessageProvider(entryHeader));
			});
		}

		public void TestExportOperation()
		{
			AssertSame("ExportOperation", Provider, Provider.ExportOperation);
		}

		public void TestLRN()
		{
			entryHeader.CH_BGMReference = "LRN2343234242";
			AssertEquals("LRN", "LRN2343234242", Provider.LRN);
		}

		public void TestMRN()
		{
			entryHeader.MovementReferenceNumberSetter("22IEDU4EU157452570");
			AssertEquals("MRN", "22IEDU4EU157452570", Provider.MRN);
		}

		public void TestPresentationOffice()
		{
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfPresentation, "IE000001");
			AssertEquals("PresentationOffice", "IE000001", Provider.PresentationOffice);
		}

		public void TestExportOffice()
		{
			declaration.JE_CustomsOffice = "Export22";
			AssertEquals("ExportOffice", "Export22", Provider.ExportOffice);
		}

		public void TestGoodsShipment()
		{
			AssertSame("GoodsShipment", Provider, Provider.GoodsShipment);
		}

		public void TestDeclarant()
		{
			var declarantHeader = Factory.New<OrgHeader>();
			declarantHeader.OH_FullName = "TestDeclarantHeader";
			var declarant = Factory.New<OrgAddress>();
			declaration.JE_OA_DeclarantAddress = declarant.PK;
			declarant.OA_OH = declarantHeader.PK;
			AssertEquals("Declarant", "TestDeclarantHeader", Provider.Declarant.Name);
		}

		public void TestRepresentative()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "TestRepresentativeHeader";
			var address = Factory.New<OrgAddress>();
			address.OA_CompanyNameOverride = "TestRepresentativeHeader Override";
			declaration.JE_OA_Representative = address.PK;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			address.OA_OH = org.PK;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "BOB THE BUILDER";
			CombineAssertions("Representantive", () =>
			{
				var representative = Provider.Representative;
				AssertEquals("Status", "2", representative.Status);
				AssertEquals("Contact.Name", "BOB THE BUILDER", representative.Contact.Name);
			});
		}

		public void TestContainerIndicator()
		{
			var provider = GetProvider();
			AssertEquals("No for no elements in Containers", AESFlagCodeList.Codes.No, provider.ContainerIndicator);
			declaration.CusContainers.AddNew();
			provider = GetProvider();
			AssertEquals("Yes for elements in Containers", AESFlagCodeList.Codes.Yes, provider.ContainerIndicator);
		}

		public void TestInlandTransportMode()
		{
			declaration.JE_TransportModeInland = Core.Constants.TransportModes.Sea;
			AssertEquals("InlandTransportMode", "1", Provider.InlandTransportMode);
		}

		public void TestTransportEquipment()
		{
			AssertEquals("TransportEquipment being empty for now.", 0, Provider.TransportEquipment.Count);
		}

		public void TestLocationOfGoods()
		{
			AssertSame("LocationOfGoods", Provider, Provider.LocationOfGoods);
		}

		public void TestDepartureTransportMeans()
		{
			declaration.JE_TransportModeInland = Core.Constants.TransportModes.Sea;
			declaration.JE_TransportMeans = "AB";
			declaration.JE_TransportIDInland = "ID1234567";
			declaration.JE_RN_NKTransportNationalityInland = "NZ";
			CombineAssertions(() =>
			{
				DepartureTransportMeansProviderTest.AssertDepartureTransportMeans(Provider.DepartureTransportMeans.Single(), "AB", "ID1234567", "NZ");
			});
		}

		public void TestConsignment()
		{
			declaration.JE_TransportModeInland = Core.Constants.TransportModes.Sea;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			CombineAssertions(() =>
			{
				var consignment = Provider.Consignment;
				AssertEquals("Consignment.InlandTransportMode", "1", consignment.InlandTransportMode);
			});
		}

		#region ILocationOfGoods Members
		public void TestLocationCodeType()
		{
			declaration.JE_LocationOtherInformation = "KD";
			AssertEquals("LocationCodeType", "KD", Provider.LocationCodeType);
		}

		public void TestUNLocode()
		{
			declaration.JE_LocationOfGoods = "KD";
			AssertEquals("UNLocode", "KD", Provider.UNLocode);
		}
		#endregion

		protected override IE511MessageProvider GetProvider() => new IE511MessageProvider(entryHeader);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;
	}
}
