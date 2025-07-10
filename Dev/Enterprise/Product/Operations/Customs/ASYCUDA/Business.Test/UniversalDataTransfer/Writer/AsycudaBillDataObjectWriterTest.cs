using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	partial class AsycudaWriterTest
	{
		public void TestExportBill()
		{
			PrepareCusCodeDataForTesting();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_MasterBill = "MKS23432";
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "BN001";
			bill.ABL_RL_NKOrigin = "SBHIR";
			bill.ABL_RL_NKFinalDestination = "USLAX";
			bill.ABL_GoodsDescription = "GoodsDesc";

			bill.ABL_BillStatus = "NOT";
			bill.ABL_SenderReference = "SR001";
			bill.CustomsEntryNumber = "EN001";
			bill.CustomsEntryNumberType = "ENT";
			bill.ABL_GoodsLocation = "00000047";
			bill.ABL_LocationInformation = "LocationInfo";
			bill.ABL_ShipmentType = "IMP";
			bill.ABL_BillIssuer = "BI";

			Factory.SaveForTesting();
			var headerHelper = new AsycudaManifestHeaderDataObjectWriterHelper(header);

			var writer = new AsycudaBillDataObjectWriterForTest(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header)), headerHelper);
			var billData = writer.GetDataObject(bill);
			AssertEquals("BN001", billData.WayBillNumber);
			AssertEquals("SBHIR", billData.PortOfOrigin.Code);
			AssertEquals("USLAX", billData.PortOfDestination.Code);
			AssertEquals("GoodsDesc", billData.GoodsDescription);
			var billEntryHeaderData1 = billData.EntryHeaderCollection.First();

			AssertEquals("NOT", billEntryHeaderData1.EntryStatus.Code);
			AssertEquals(1, billEntryHeaderData1.CustomsReferenceCollection.Count);
			AssertEquals(Constants.CustomsReferenceType.ABL_SenderReferenceType, billEntryHeaderData1.CustomsReferenceCollection[0].Type.Code);
			AssertEquals("SR001", billEntryHeaderData1.CustomsReferenceCollection[0].Reference);
			AssertEquals(1, billEntryHeaderData1.EntryNumberCollection.Count);
			AssertEquals("ENT", billEntryHeaderData1.EntryNumberCollection[0].Type.Code);
			AssertEquals("EN001", billEntryHeaderData1.EntryNumberCollection[0].Number);

			var link1 = billEntryHeaderData1.EntryInstructionLink;
			var billEntryInstructionData1 = billData.EntryInstructionCollection.FirstOrDefault(x => x.Link == link1);
			AssertEquals("00000047", billEntryInstructionData1.LocationAtClearance.Code);
			AssertEquals("IMP", billEntryInstructionData1.Style);
			AssertEquals(1, billEntryInstructionData1.OrganizationAddressCollection.Count);
			AssertEquals(nameof(DocAddressType.HouseBillIssuingParty), billEntryInstructionData1.OrganizationAddressCollection[0].AddressType);
			AssertEquals(1, billEntryInstructionData1.OrganizationAddressCollection[0].RegistrationNumberCollection.Count);
			AssertEquals(Constants.RegistrationTypes.BillIssuer, billEntryInstructionData1.OrganizationAddressCollection[0].RegistrationNumberCollection[0].Type.Code);
			AssertEquals(header.AMA_RN_NKCountry, billEntryInstructionData1.OrganizationAddressCollection[0].RegistrationNumberCollection[0].CountryOfIssue.Code);
			AssertEquals("BI", billEntryInstructionData1.OrganizationAddressCollection[0].RegistrationNumberCollection[0].Value);

			var locationInfo1 = billEntryInstructionData1.AddInfoCollection.FirstOrDefault(x => x.Key.Value == AsycudaBillSchema.Constants.ABL_LocationInformation);

			AssertEquals("LocationInfo", locationInfo1.Value);
		}

		public void TestBillShipperMapping()
		{
			var manifestHeader = SetupManifestHeader<AsycudaManifestHeader>("SG", "MGI", Core.Constants.TransportModes.Sea, "MKS23432");
			var bill = SetupBillShipper(manifestHeader.Bills.AddNew(), "BOB", "STREET 1", "STREET 2", "CT", "ST", "2366", "NZ", "+43453");
			Factory.SaveForTesting();
			var manifestHeaderData = (UniversalShipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, manifestHeader);
			AssertEquals("manifestHeaderData.SubShipmentCollection.Count", 1, manifestHeaderData.SubShipmentCollection.Count);
			var billData = manifestHeaderData.SubShipmentCollection[0];
			AssertOrganizationAddressContents(billData.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.Value == nameof(DocAddressType.ConsignorDocumentaryAddress)), "BOB", "STREET 1", "STREET 2", "CT", "ST", "2366", "NZ", "+43453");
		}

		public void TestBillConsigneeMapping()
		{
			var manifestHeader = SetupManifestHeader<AsycudaManifestHeader>("SG", "MGI", Core.Constants.TransportModes.Sea, "MKS23432");
			var bill = SetupBillConsignee(manifestHeader.Bills.AddNew(), "BOB", "STREET 1", "STREET 2", "CT", "ST", "2366", "NZ", "+43453");
			Factory.SaveForTesting();
			var manifestHeaderData = (UniversalShipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, manifestHeader);
			AssertEquals("manifestHeaderData.SubShipmentCollection.Count", 1, manifestHeaderData.SubShipmentCollection.Count);
			var billData = manifestHeaderData.SubShipmentCollection[0];
			AssertOrganizationAddressContents(billData.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.Value == nameof(DocAddressType.ConsigneeDocumentaryAddress)), "BOB", "STREET 1", "STREET 2", "CT", "ST", "2366", "NZ", "+43453");
		}

		public void TestBillNotifyPartyMapping()
		{
			var manifestHeader = SetupManifestHeader<AsycudaManifestHeader>("SG", "MGI", Core.Constants.TransportModes.Sea, "MKS23432");
			var bill = SetupBillNotifyParty(manifestHeader.Bills.AddNew(), "BOB", "STREET 1", "STREET 2", "CT", "ST", "2366", "NZ", "+43453");
			Factory.SaveForTesting();
			var manifestHeaderData = (UniversalShipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, manifestHeader);
			AssertEquals("manifestHeaderData.SubShipmentCollection.Count", 1, manifestHeaderData.SubShipmentCollection.Count);
			var billData = manifestHeaderData.SubShipmentCollection[0];
			AssertOrganizationAddressContents(billData.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.Value == nameof(DocAddressType.NotifyParty)), "BOB", "STREET 1", "STREET 2", "CT", "ST", "2366", "NZ", "+43453");
		}

		public void TestBillForwarderMapping()
		{
			var manifestHeader = SetupManifestHeader<AsycudaManifestHeader>("SG", "MGI", Core.Constants.TransportModes.Sea, "MKS23432");
			var bill = SetupBillForwader(manifestHeader.Bills.AddNew(), "BOB", "STREET 1", "STREET 2", "CT", "ST", "2366", "NZ", "+43453");
			Factory.SaveForTesting();
			var manifestHeaderData = (UniversalShipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, manifestHeader);
			AssertEquals("manifestHeaderData.SubShipmentCollection.Count", 1, manifestHeaderData.SubShipmentCollection.Count);
			var billData = manifestHeaderData.SubShipmentCollection[0];
			AssertOrganizationAddressContents(billData.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.Value == nameof(DocAddressType.Forwarder)), "BOB", "STREET 1", "STREET 2", "CT", "ST", "2366", "NZ", "+43453");
		}

		public void TestPacksForMessage()
		{
			var manifestHeader = SetupManifestHeader<AsycudaManifestHeader>(Core.Constants.CountryCodes.Fiji, "ASY", Core.Constants.TransportModes.Sea, "MKS23432");
			var manager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, manifestHeader));
			var bill = Factory.New<AsycudaBill>();
			var helper = new AsycudaManifestHeaderDataObjectWriterHelper(manifestHeader);
			var writer = new AsycudaBillDataObjectWriterForTest(manager, helper);
			var uxml = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bill.ABL_AMA = manifestHeader.PK;

			var pack = bill.Packs.AddNew();
			pack.APA_GoodsDescription = "Pack stuff 1";

			var pack2 = bill.Packs.AddNew();
			pack2.APA_GoodsDescription = "Pack stuff 2";

			writer.ExposedPopulateDataObject(bill, uxml);
			AssertNotEquals(null, uxml.PackingLineCollection);
			AssertEquals(2, uxml.PackingLineCollection.Count);
		}

		public void TestPacksWhenManifestContextIsNull()
		{
			var manifestHeader = SetupManifestHeader<AsycudaManifestHeader>(Core.Constants.CountryCodes.Fiji, "ASY", Core.Constants.TransportModes.Sea, "MKS23432");
			var manager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, manifestHeader));
			var bill = Factory.New<AsycudaBill>();
			var helper = new AsycudaManifestHeaderDataObjectWriterHelper(manifestHeader);
			var writer = new AsycudaBillDataObjectWriterForTest(manager, helper);
			var uxml = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bill.ABL_AMA = manifestHeader.PK;

			var pack = bill.Packs.AddNew();
			pack.APA_GoodsDescription = "Pack stuff 1";

			var pack2 = bill.Packs.AddNew();
			pack2.APA_GoodsDescription = "Pack stuff 2";

			writer.ExposedPopulateDataObject(bill, uxml);
			AssertNotEquals(null, uxml.PackingLineCollection);
			AssertEquals(2, uxml.PackingLineCollection.Count);
		}

		public void TestMatchingReferenceExported()
		{
			var header = SetupManifestHeader<AsycudaManifestHeader>(Core.Constants.CountryCodes.Singapore, "MGI", Core.Constants.TransportModes.Sea, "MKS23432");
			var bill = header.Bills.AddNew();
			bill.MatchingReference = "TESTMATCHREFERNCE";
			Factory.SaveForTesting();
			var manifestHeaderData = (UniversalShipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, header);
			AssertEquals("manifestHeaderData.SubShipmentCollection.Count", 1, manifestHeaderData.SubShipmentCollection.Count);
			var billData = manifestHeaderData.SubShipmentCollection[0];
			AssertEquals("TESTMATCHREFERNCE", billData.AddInfoCollection.FirstOrDefault(x => x.Key.Value == AddInfoConstants.Bill.MatchingReference).Value.Value);
		}

		public void TestABL_MarksAndNumbersExported()
		{
			var header = SetupManifestHeader<AsycudaManifestHeader>(Core.Constants.CountryCodes.Singapore, "MGI", Core.Constants.TransportModes.Sea, "MKS23432");

			var bill = header.Bills.AddNew();
			bill.ABL_MarksAndNumbers = "TESTBILLHEADERMARKSANDNUMBERS";
			Factory.SaveForTesting();
			var manifestHeaderData = (UniversalShipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, header);
			AssertEquals("manifestHeaderData.SubShipmentCollection.Count", 1, manifestHeaderData.SubShipmentCollection.Count);
			var billData = manifestHeaderData.SubShipmentCollection[0];
			AssertEquals("TESTBILLHEADERMARKSANDNUMBERS", billData.AddInfoCollection.FirstOrDefault(x => x.Key.Value == AddInfoConstants.Bill.ABL_MarksAndNumbers).Value.Value);
		}

		public void TestABL_UCRNumberExported()
		{
			var header = SetupManifestHeader<AsycudaManifestHeader>(Core.Constants.CountryCodes.Fiji, "ASY", Core.Constants.TransportModes.Sea, "MKS23432");

			var bill = header.Bills.AddNew();
			bill.ABL_UCRNumber = "FAKEUCR";
			Factory.SaveForTesting();
			var manifestHeaderData = (UniversalShipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, header);
			AssertEquals("manifestHeaderData.SubShipmentCollection.Count", 1, manifestHeaderData.SubShipmentCollection.Count);
			var billData = manifestHeaderData.SubShipmentCollection[0];
			AssertEquals("FAKEUCR", billData.AddInfoCollection.FirstOrDefault(x => x.Key.Value == AddInfoConstants.Bill.ABL_UCRNumber).Value.Value);
		}

		public void TestPopulateCustomFeilds()
		{
			var manifestHeader = SetupManifestHeader<AsycudaManifestHeader>("SG", "MGI", Core.Constants.TransportModes.Sea, "MKS23432");
			var bill = SetupBillShipper(manifestHeader.Bills.AddNew(), "BOB", "STREET 1", "STREET 2", "CT", "ST", "2366", "NZ", "+43453");

			bill.SetUserDefinedValue("CustomField1", (ZString)"Hello World");
			bill.SetUserDefinedValue("CustomField2", ZDateTime.BrettsBirthday);
			bill.SetUserDefinedValue("CustomField3", (ZDecimal)200.5);
			bill.SetUserDefinedValue("CustomField4", (ZBool)true);

			Factory.SaveForTesting();

			var manifestHeaderData = (UniversalShipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, manifestHeader);
			AssertEquals("manifestHeaderData.SubShipmentCollection.Count", 1, manifestHeaderData.SubShipmentCollection.Count);
			var billData = manifestHeaderData.SubShipmentCollection[0];

			AssertEquals("Count of CustomizedFieldCollection", 4, billData.CustomizedFieldCollection.Count);
			billData.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.String, "CustomField1", "Hello World");
			billData.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.DateTime, "CustomField2", ZDateTime.BrettsBirthday.ToISO8601String());
			billData.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.Decimal, "CustomField3", "200.5");
			billData.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.Boolean, "CustomField4", "true");
		}

		AsycudaBill SetupBillShipper(AsycudaBill bill, ZString name, ZString street1, ZString street2, ZString city, ZString state, ZString postcode, ZString country, ZString phone)
		{
			bill.ABL_ShipperName = name;
			bill.ABL_ShipperStreet1 = street1;
			bill.ABL_ShipperStreet2 = street2;
			bill.ABL_ShipperCity = city;
			bill.ABL_ShipperState = state;
			bill.ABL_ShipperPostcode = postcode;
			bill.ABL_RN_NKShipperCountry = country;
			bill.ABL_ShipperPhone = phone;
			return bill;
		}

		AsycudaBill SetupBillForwader(AsycudaBill bill, ZString name, ZString street1, ZString street2, ZString city, ZString state, ZString postcode, ZString country, ZString phone)
		{
			var org = Factory.NewWithValidTestData<OrgAddress>();
			org.Header.OH_FullName = name;
			org.OA_Address1 = street1;
			org.OA_Address2 = street2;
			org.OA_City = city;
			org.OA_State = state;
			org.OA_PostCode = postcode;
			org.OA_Phone = phone;
			org.OA_RN_NKCountryCode = country;

			bill.ABL_OA_Forwarder = org.PK;
			return bill;
		}

		AsycudaBill SetupBillConsignee(AsycudaBill bill, ZString name, ZString street1, ZString street2, ZString city, ZString state, ZString postcode, ZString country, ZString phone)
		{
			bill.ABL_ConsigneeName = name;
			bill.ABL_ConsigneeStreet1 = street1;
			bill.ABL_ConsigneeStreet2 = street2;
			bill.ABL_ConsigneeCity = city;
			bill.ABL_ConsigneeState = state;
			bill.ABL_ConsigneePostcode = postcode;
			bill.ABL_RN_NKConsigneeCountry = country;
			bill.ABL_ConsigneePhone = phone;
			return bill;
		}

		AsycudaBill SetupBillNotifyParty(AsycudaBill bill, ZString name, ZString street1, ZString street2, ZString city, ZString state, ZString postcode, ZString country, ZString phone)
		{
			bill.ABL_NotifyPartyName = name;
			bill.ABL_NotifyPartyStreet1 = street1;
			bill.ABL_NotifyPartyStreet2 = street2;
			bill.ABL_NotifyPartyCity = city;
			bill.ABL_NotifyPartyState = state;
			bill.ABL_NotifyPartyPostcode = postcode;
			bill.ABL_RN_NKNotifyPartyCountry = country;
			bill.ABL_NotifyPartyPhone = phone;
			return bill;
		}

		void AssertOrganizationAddressContents(OrganizationAddress organizationAddressData, ZString? name, ZString? street1, ZString? street2, ZString? city, ZString? state, ZString? postcode, ZString? country, ZString? phone)
		{
			AssertNotNull("Precondition: mawbData", organizationAddressData);

			CombineAssertions(delegate
			{
				AssertEquals("organizationAddressData.CompanyName", name, organizationAddressData.CompanyName);
				AssertEquals("organizationAddressData.Address1", street1, organizationAddressData.Address1);
				AssertEquals("organizationAddressData.Address2", street2, organizationAddressData.Address2);
				AssertEquals("organizationAddressData.City", city, organizationAddressData.City);
				AssertEquals("organizationAddressData.State", state, (ZString?)organizationAddressData.State);
				AssertEquals("organizationAddressData.Postcode", postcode, organizationAddressData.Postcode);
				if (country.HasValue)
				{
					AssertEquals("organizationAddressData.Country.Code", country, organizationAddressData.Country.Code);
				}
				else
				{
					AssertNull("organizationAddressData.Country", organizationAddressData.Country);
				}
				AssertEquals("organizationAddressData.Phone", phone, organizationAddressData.Phone);
			});
		}

		sealed class AsycudaBillDataObjectWriterForTest : AsycudaBillDataObjectWriter<AsycudaBill>
		{
			public AsycudaBillDataObjectWriterForTest(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper) : base(manager, helper)
			{
			}

			public void ExposedPopulateDataObject(AsycudaBill sourceBill, UniversalShipment uxml)
			{
				PopulateDataObject(sourceBill, uxml);
			}
		}
	}
}
