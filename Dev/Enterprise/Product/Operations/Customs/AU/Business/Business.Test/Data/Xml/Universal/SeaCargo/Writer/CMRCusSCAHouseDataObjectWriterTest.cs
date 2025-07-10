using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	partial class CMRCusSCAOceanBillDataObjectWriterTest
	{
		public void TestMappings_ICSRelease()
		{
			var houseBill = base.AddHouseBill1(OceanBill);
			houseBill.CA_ConsigneeBusinessNumber = "consigneeABN";
			houseBill.CA_ConsigneeIdentifier = "consigneeid";
			houseBill.CA_ConsignorIdentifier = "consigorid";
			houseBill.CA_VendorIdentifier = "vendor";
			var writer = GetWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.HSA, OceanBill)));
			var shipment = (Shipment)writer.GetDataObject(OceanBill);
			AssertEquals("shipment.SubShipmentCollection.Count", 3, shipment.SubShipmentCollection.Count);
			var hawbData = shipment.SubShipmentCollection[1];
			AssertEquals("consigneeABN", hawbData.ConsigneeBussinessNumber);
			AssertEquals("consigneeid", hawbData.ConsigneeIdentifier);
			AssertEquals("consigorid", hawbData.ConsignorIdentifier);
			AssertEquals("vendor", hawbData.VendorIdentifier);
		}

		public void TestConsignorMappings()
		{
			var shippingAgent1 = Factory.New<OrgHeader>();
			shippingAgent1.OH_Code = "CCC";
			shippingAgent1.OH_FullName = "Consignee";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = shippingAgent1.PK;
			orgAddress.OA_Code = "CRAHOLSYD";
			orgAddress.OA_Address1 = "Consignee Address1";
			orgAddress.OA_City = "NY";
			orgAddress.OA_State = "NY";
			orgAddress.OA_PostCode = "1000";
			orgAddress.OA_Phone = "12345678";
			orgAddress.OA_RN_NKCountryCode = "AU";

			var oceanBill = CreateOceanBill();
			var houseBill = AddHouseBill1(oceanBill);
			houseBill.CA_IsMasterHouse = false;
			houseBill.CA_OA_ConsignorAddress = orgAddress.PK;
			Factory.Save();

			var writer = GetWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.HSA, oceanBill)));
			var shipment = (Shipment)writer.GetDataObject(oceanBill);
			AssertEquals("shipment.SubShipmentCollection.Count", 1, shipment.SubShipmentCollection.Count);
			var hawbData = shipment.SubShipmentCollection[0];
			var hawbDataConsignorDocumentaryAddress = hawbData.OrganizationAddressCollection.Single(org => org != null && org.AddressType.HasValue && org.AddressType.Value == nameof(DocAddressType.ConsignorDocumentaryAddress));
			AssertAddress("ConsignorDocumentaryAddress", hawbDataConsignorDocumentaryAddress, nameof(DocAddressType.ConsignorDocumentaryAddress), shippingAgent1.OH_Code, shippingAgent1.OH_FullName, false, orgAddress.OA_Address1, "", orgAddress.OA_City, orgAddress.OA_State, orgAddress.OA_PostCode, orgAddress.OA_RN_NKCountryCode, null, "", "", null, orgAddress.OA_Phone);
		}

		public void TestConsigneeMappings()
		{
			var shippingAgent1 = Factory.New<OrgHeader>();
			shippingAgent1.OH_Code = "CCC";
			shippingAgent1.OH_FullName = "Consignee";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = shippingAgent1.PK;
			orgAddress.OA_Code = "CRAHOLSYD";
			orgAddress.OA_Address1 = "Consignee Address1";
			orgAddress.OA_City = "NY";
			orgAddress.OA_State = "NY";
			orgAddress.OA_PostCode = "1000";
			orgAddress.OA_Phone = "12345678";
			orgAddress.OA_RN_NKCountryCode = "AU";

			var oceanBill = CreateOceanBill();
			var houseBill = AddHouseBill1(oceanBill);
			houseBill.CA_IsMasterHouse = false;
			houseBill.CA_OA_ConsigneeAddress = orgAddress.PK;
			Factory.Save();

			var writer = GetWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.HSA, oceanBill)));
			var shipment = (Shipment)writer.GetDataObject(oceanBill);
			AssertEquals("shipment.SubShipmentCollection.Count", 1, shipment.SubShipmentCollection.Count);
			var hawbData = shipment.SubShipmentCollection[0];
			var hawbDataConsigneeDocumentaryAddress = hawbData.OrganizationAddressCollection.Single(org => org != null && org.AddressType.HasValue && org.AddressType.Value == nameof(DocAddressType.ConsigneeDocumentaryAddress));
			AssertAddress("ConsigneeDocumentaryAddress", hawbDataConsigneeDocumentaryAddress, nameof(DocAddressType.ConsigneeDocumentaryAddress), shippingAgent1.OH_Code, shippingAgent1.OH_FullName, false, orgAddress.OA_Address1, "", orgAddress.OA_City, orgAddress.OA_State, orgAddress.OA_PostCode, orgAddress.OA_RN_NKCountryCode, null, "", "", null, orgAddress.OA_Phone);
		}

		public static void AssertAddress(string message, OrganizationAddress addressData, string addressType, string organizationCode, string companyName, ZBool? addressOverride, string address1, string address2, string city, string state, string postcode, string country, string contact, string email, string fax, string mobile, string phone)
		{
			var addressFormatted = new OrganizationAddressFormatted(addressData);
			CombineAssertions(message, delegate
			{
				AssertEquals(".AddressType", addressType, addressFormatted.AddressType);
				AssertEquals(".OrganizationCode", organizationCode, addressData.OrganizationCode);
				AssertEquals(".CompanyName", companyName, addressFormatted.CompanyName);
				AssertEquals(".AddressOverride", addressOverride, addressFormatted.AddressOverride);
				AssertEquals(".Address1", address1, addressFormatted.Address1);
				AssertEquals(".Address2", address2, addressFormatted.Address2);
				AssertEquals(".City", city, addressFormatted.City);
				AssertEquals(".State", state, addressFormatted.State);
				AssertEquals(".Postcode", postcode, addressFormatted.Postcode);
				if (country == null)
				{
					AssertNull(".Country should be null", addressFormatted.Country);
				}
				else
				{
					AssertEquals(".Country.Code", country, addressFormatted.Country.Code);
				}

				AssertEquals(".Contact", contact, addressFormatted.Contact);
				AssertEquals(".Email", email, addressFormatted.Email);
				AssertEquals(".Fax", fax, addressFormatted.Fax);
				AssertEquals(".Mobile", mobile, addressFormatted.Mobile);
				AssertEquals(".Phone", phone, addressFormatted.Phone);
			});
		}

		protected override CusSCAHouse AddHouseBill1(CusSCAOceanBill oceanBill)
		{
			var houseBill1 = base.AddHouseBill1(oceanBill);
			SetOAAddressForHouseBill(houseBill1);
			houseBill1.CA_VendorIdentifier = "1234567";
			houseBill1.CA_PrepaidCollectOther = "CA";
			houseBill1.CA_RN_NKGoodsOrigin = "CA";
			houseBill1.CA_RL_NK_PortOfOrigin = "USCHI";
			houseBill1.CA_MessageStatus = "ACZ";

			houseBill1.SetUserDefinedValue("Test3", (ZString)"Test3Value");
			houseBill1.SetUserDefinedValue("Test4", (ZString)"Test4Value");
			houseBill1.SetUserDefinedValue("Test5", (ZInt)222222);
			return houseBill1;
		}

		protected override CusSCAHouse AddHouseBill2(CusSCAOceanBill oceanBill)
		{
			var houseBill2 = base.AddHouseBill2(oceanBill);
			SetOAAddressForHouseBill(houseBill2);
			houseBill2.CA_VendorIdentifier = "2345678";
			houseBill2.CA_PrepaidCollectOther = "A";
			houseBill2.CA_RN_NKGoodsOrigin = "AD";
			houseBill2.CA_RL_NK_PortOfOrigin = "AIBLP";
			return houseBill2;
		}

		void SetOAAddressForHouseBill(CusSCAHouse houseBill)
		{
			houseBill.CA_OA_ConsigneeAddress = Factory.Load<OrgHeader>(houseBill.CA_OH_Consignee).MainAddress.PK;
			houseBill.CA_OA_ConsignorAddress = Factory.Load<OrgHeader>(houseBill.CA_OH_Consignor).MainAddress.PK;
		}
	}
}
