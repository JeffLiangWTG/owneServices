using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class HAWBToShipmentBridgeTest : TestCaseWithFactory
	{
		public void TestFieldsAreSynchedByDefault()
		{
			var bridge = new HAWBToShipmentBridge(hAWB);

			hAWB.CS_OA_ConsigneeAddress = ZGuid.NewZGuid();
			hAWB.CS_OA_ConsignorAddress = ZGuid.NewZGuid();

			ZDecimal actualWeight = 55m;
			ZString coLoadMasterShipmentHouseBillNumber = "52343";
			ZString consigneeCity = "City1";
			ZString consigneeContact = "Contact1";
			ZString consigneeName = "Name1";
			ZString consigneePhone = "Phone1";
			ZGuid consigneePK = Factory.NewWithValidTestData<OrgHeader>().PK;
			ZString consigneePostCode = "PostCode1";
			ZString consigneeState = "State1";
			ZString consigneeStreet2 = "Street12";
			ZString consigneeStreet = "Street1";
			ZString consignorCity = "City2";
			ZString consignorContact = "Contact2";
			ZString consignorName = "Name2";
			ZString consignorPhone = "Phone2";
			ZGuid consignorPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			ZString consignorPostCode = "PostCode2";
			ZString consignorState = "State2";
			ZString consignorStreet2 = "Street22";
			ZString consignorStreet = "Street2";
			ZString consignorCountry = "AU";
			ZString consigneeCountry = "US";
			ZString destination = "AUSYD";
			ZString goodsCurrency = "AUD";
			ZString goodsDescription = "Cuckoo Squeakers";
			ZDecimal goodsValue = 62m;
			ZString houseBillNumber = "HAWB1$%#* ";
			ZBool isCoload = true;
			ZString origin = "USLAX";
			ZInt outerPacks = 2;
			ZString paymentTerm = "CC";
			ZString uniqueConsignRef = "REFREFREF";
			ZString unitOfWeight = "KG";

			((IUpdateFromShipment)bridge).ActualWeight = actualWeight;
			((IUpdateFromShipment)bridge).CoLoadMasterShipmentHouseBillNumber = coLoadMasterShipmentHouseBillNumber;

			shipment.ConsigneePK = consigneePK;
			((IUpdateFromShipment)bridge).CopyConsigneeDetails(shipment);
			AssertEquals(consigneePK, hAWB.Consignee.PK);

			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDocumentaryAddress.E2_CompanyName = consigneeName;
			shipment.ConsigneeDocumentaryAddress.E2_Contact = consigneeContact;
			shipment.ConsigneeDocumentaryAddress.E2_City = consigneeCity;
			shipment.ConsigneeDocumentaryAddress.E2_Phone = consigneePhone;
			shipment.ConsigneeDocumentaryAddress.E2_Postcode = consigneePostCode;
			shipment.ConsigneeDocumentaryAddress.E2_State = consigneeState;
			shipment.ConsigneeDocumentaryAddress.E2_Address1 = consigneeStreet;
			shipment.ConsigneeDocumentaryAddress.E2_Address2 = consigneeStreet2;
			shipment.ConsigneeDocumentaryAddress.E2_RN_NKCountryCode = consigneeCountry;
			((IUpdateFromShipment)bridge).CopyConsigneeDetails(shipment);

			shipment.ConsignorPK = consignorPK;
			((IUpdateFromShipment)bridge).CopyConsignorDetails(shipment);
			AssertEquals(consignorPK, hAWB.Consignor.PK);

			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsignorDocumentaryAddress.E2_CompanyName = consignorName;
			shipment.ConsignorDocumentaryAddress.E2_Contact = consignorContact;
			shipment.ConsignorDocumentaryAddress.E2_City = consignorCity;
			shipment.ConsignorDocumentaryAddress.E2_Phone = consignorPhone;
			shipment.ConsignorDocumentaryAddress.E2_Postcode = consignorPostCode;
			shipment.ConsignorDocumentaryAddress.E2_State = consignorState;
			shipment.ConsignorDocumentaryAddress.E2_Address1 = consignorStreet;
			shipment.ConsignorDocumentaryAddress.E2_Address2 = consignorStreet2;
			shipment.ConsignorDocumentaryAddress.E2_RN_NKCountryCode = consignorCountry;
			((IUpdateFromShipment)bridge).CopyConsignorDetails(shipment);

			((IUpdateFromShipment)bridge).Destination = destination;
			((IUpdateFromShipment)bridge).Origin = origin;
			((IUpdateFromShipment)bridge).GoodsDescription = goodsDescription;
			((IUpdateFromShipment)bridge).GoodsCurrency = goodsCurrency;
			((IUpdateFromShipment)bridge).GoodsValue = goodsValue;
			((IUpdateFromShipment)bridge).HouseBillNumber = houseBillNumber;
			((IUpdateFromShipment)bridge).IsCoload = isCoload;
			((IUpdateFromShipment)bridge).OuterPacks = outerPacks;
			((IUpdateFromShipment)bridge).PaymentTerm = paymentTerm;
			((IUpdateFromShipment)bridge).UniqueConsignRef = uniqueConsignRef;
			((IUpdateFromShipment)bridge).UnitOfWeight = unitOfWeight;

			AssertEquals(actualWeight, hAWB.CS_Weight);
			AssertEquals(coLoadMasterShipmentHouseBillNumber, hAWB.CS_MasterHouseBill);
			AssertEquals(consigneeCity, hAWB.CS_ConsigneeCity);
			AssertEquals(consigneeContact, hAWB.CS_ConsigneeContactName);
			AssertEquals(consigneeName, hAWB.CS_ConsigneeName);
			AssertEquals(consigneePhone, hAWB.CS_ConsigneePhone);
			AssertEquals(ZGuid.Empty, hAWB.CS_OA_ConsigneeAddress);
			AssertEquals(consigneePostCode, hAWB.CS_ConsigneePostcode);
			AssertEquals(consigneeState, hAWB.CS_ConsigneeState);
			AssertEquals(consigneeStreet2, hAWB.CS_ConsigneeStreet2);
			AssertEquals(consigneeStreet, hAWB.CS_ConsigneeStreet);
			AssertEquals(consignorCity, hAWB.CS_ConsignorCity);
			AssertEquals(consignorContact, hAWB.CS_ConsignorContactName);
			AssertEquals(consignorName, hAWB.CS_ConsignorName);
			AssertEquals(consignorPhone, hAWB.CS_ConsignorPhone);
			AssertEquals(ZGuid.Empty, hAWB.CS_OA_ConsignorAddress);
			AssertEquals(consignorPostCode, hAWB.CS_ConsignorPostcode);
			AssertEquals(consignorState, hAWB.CS_ConsignorState);
			AssertEquals(consignorStreet2, hAWB.CS_ConsignorStreet2);
			AssertEquals(consignorStreet, hAWB.CS_ConsignorStreet);
			AssertEquals(consignorCountry, hAWB.CS_RN_NKConsignorCountry);
			AssertEquals(consigneeCountry, hAWB.CS_RN_NKConsigneeCountry);
			AssertEquals(destination, hAWB.CS_RL_NKDestination);
			AssertEquals(goodsCurrency, hAWB.CS_RX_NKGoodsCurrency);
			AssertEquals(goodsDescription, hAWB.CS_GoodsDescription);
			AssertEquals(goodsValue, hAWB.CS_GoodsValue);
			AssertEquals(houseBillNumber, hAWB.CS_HAWB);
			AssertEquals(isCoload, hAWB.CS_IsMasterHouse);
			AssertEquals(origin, hAWB.CS_RL_NKOrigin);
			AssertEquals(outerPacks, hAWB.CS_PiecesManifested);
			AssertEquals("CC", hAWB.CS_FreightPrepaidCollect);
			AssertEquals(uniqueConsignRef, hAWB.CS_MessageReference);
			AssertEquals(unitOfWeight, hAWB.CS_WeightUQ);
		}

		public void TestFieldsAreNotSynchedWhenMessageSent()
		{
			var bridge = new HAWBToShipmentBridge(hAWB);
			hAWB.CS_MsgStatus = CMRBaseStatuses.Codes.OriginalAccepted;

			ZDecimal actualWeight = 55m;
			ZString coLoadMasterShipmentHouseBillNumber = "52343";
			ForwardingShipment coLoad = Factory.New<ForwardingShipment>();
			ZGuid coLoadMasterShipmentPK = coLoad.PK;
			coLoad.JS_HouseBill = coLoadMasterShipmentHouseBillNumber;
			ZString consigneeCity = "City1";
			ZString consigneeContact = "Contact1";
			ZString consigneeName = "Name1";
			ZString consigneePhone = "Phone1";
			ZGuid consigneePK = ZGuid.NewZGuid();
			ZString consigneePostCode = "PostCode1";
			ZString consigneeState = "State1";
			ZString consigneeStreet2 = "Street12";
			ZString consigneeStreet = "Street1";
			ZString consignorCity = "City2";
			ZString consignorContact = "Contact2";
			ZString consignorName = "Name2";
			ZString consignorPhone = "Phone2";
			ZGuid consignorPK = ZGuid.NewZGuid();
			ZString consignorPostCode = "PostCode2";
			ZString consignorState = "State2";
			ZString consignorStreet2 = "Street22";
			ZString consignorStreet = "Street2";
			ZString consignorCountry = "AU";
			ZString consigneeCountry = "US";
			ZString destination = "AUBNE";
			ZString goodsCurrency = "NZD";
			ZString goodsDescription = "Cuckoo Squeakers";
			ZDecimal goodsValue = 62m;
			ZString houseBillNumber = "HAWB1";
			ZBool isCoload = true;
			ZString origin = "NZAKL";
			ZInt outerPacks = 2;
			ZString paymentTerm = "CC";
			ZString uniqueConsignRef = "REFREFREF";
			ZString unitOfWeight = "G";

			((IUpdateFromShipment)bridge).ActualWeight = actualWeight;
			((IUpdateFromShipment)bridge).CoLoadMasterShipmentPK = coLoadMasterShipmentPK;

			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDocumentaryAddress.E2_CompanyName = consigneeName;
			shipment.ConsigneeDocumentaryAddress.E2_Contact = consigneeContact;
			shipment.ConsigneeDocumentaryAddress.E2_City = consigneeCity;
			shipment.ConsigneeDocumentaryAddress.E2_Phone = consigneePhone;
			shipment.ConsigneeDocumentaryAddress.E2_Postcode = consigneePostCode;
			shipment.ConsigneeDocumentaryAddress.E2_State = consigneeState;
			shipment.ConsigneeDocumentaryAddress.E2_Address1 = consigneeStreet;
			shipment.ConsigneeDocumentaryAddress.E2_Address2 = consigneeStreet2;
			shipment.ConsigneeDocumentaryAddress.E2_RN_NKCountryCode = consigneeCountry;
			((IUpdateFromShipment)bridge).CopyConsigneeDetails(shipment);

			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsignorDocumentaryAddress.E2_CompanyName = consignorName;
			shipment.ConsignorDocumentaryAddress.E2_Contact = consignorContact;
			shipment.ConsignorDocumentaryAddress.E2_City = consignorCity;
			shipment.ConsignorDocumentaryAddress.E2_Phone = consignorPhone;
			shipment.ConsignorDocumentaryAddress.E2_Postcode = consignorPostCode;
			shipment.ConsignorDocumentaryAddress.E2_State = consignorState;
			shipment.ConsignorDocumentaryAddress.E2_Address1 = consignorStreet;
			shipment.ConsignorDocumentaryAddress.E2_Address2 = consignorStreet2;
			shipment.ConsignorDocumentaryAddress.E2_RN_NKCountryCode = consignorCountry;
			((IUpdateFromShipment)bridge).CopyConsignorDetails(shipment);

			((IUpdateFromShipment)bridge).Destination = destination;
			((IUpdateFromShipment)bridge).Origin = origin;
			((IUpdateFromShipment)bridge).GoodsDescription = goodsDescription;
			((IUpdateFromShipment)bridge).GoodsCurrency = goodsCurrency;
			((IUpdateFromShipment)bridge).GoodsValue = goodsValue;
			((IUpdateFromShipment)bridge).HouseBillNumber = houseBillNumber;
			((IUpdateFromShipment)bridge).IsCoload = isCoload;
			((IUpdateFromShipment)bridge).OuterPacks = outerPacks;
			((IUpdateFromShipment)bridge).PaymentTerm = paymentTerm;
			((IUpdateFromShipment)bridge).UniqueConsignRef = uniqueConsignRef;
			((IUpdateFromShipment)bridge).UnitOfWeight = unitOfWeight;

			AssertEquals(hAWB.CS_Weight.Default, hAWB.CS_Weight);
			AssertEquals(hAWB.CS_MasterHouseBill.Default, hAWB.CS_MasterHouseBill);
			AssertEquals(hAWB.CS_CS_MasterHouseBill.Default, hAWB.CS_CS_MasterHouseBill);
			AssertEquals(hAWB.CS_ConsigneeCity.Default, hAWB.CS_ConsigneeCity);
			AssertEquals(hAWB.CS_ConsigneeContactName.Default, hAWB.CS_ConsigneeContactName);
			AssertEquals(hAWB.CS_ConsigneeName.Default, hAWB.CS_ConsigneeName);
			AssertEquals(hAWB.CS_ConsigneePhone.Default, hAWB.CS_ConsigneePhone);
			AssertEquals(hAWB.CS_OH_Consignee.Default, hAWB.CS_OH_Consignee);
			AssertEquals(hAWB.CS_ConsigneePostcode.Default, hAWB.CS_ConsigneePostcode);
			AssertEquals(hAWB.CS_ConsigneeState.Default, hAWB.CS_ConsigneeState);
			AssertEquals(hAWB.CS_ConsigneeStreet2.Default, hAWB.CS_ConsigneeStreet2);
			AssertEquals(hAWB.CS_ConsigneeStreet.Default, hAWB.CS_ConsigneeStreet);
			AssertEquals(hAWB.CS_ConsignorCity.Default, hAWB.CS_ConsignorCity);
			AssertEquals(hAWB.CS_ConsignorContactName.Default, hAWB.CS_ConsignorContactName);
			AssertEquals(hAWB.CS_ConsignorName.Default, hAWB.CS_ConsignorName);
			AssertEquals(hAWB.CS_ConsignorPhone.Default, hAWB.CS_ConsignorPhone);
			AssertEquals(hAWB.CS_OH_Consignor.Default, hAWB.CS_OH_Consignor);
			AssertEquals(hAWB.CS_ConsignorPostcode.Default, hAWB.CS_ConsignorPostcode);
			AssertEquals(hAWB.CS_RN_NKConsignorCountry.Default, hAWB.CS_RN_NKConsignorCountry);
			AssertEquals(hAWB.CS_RN_NKConsigneeCountry.Default, hAWB.CS_RN_NKConsigneeCountry);
			AssertEquals(hAWB.CS_ConsignorState.Default, hAWB.CS_ConsignorState);
			AssertEquals(hAWB.CS_ConsignorStreet2.Default, hAWB.CS_ConsignorStreet2);
			AssertEquals(hAWB.CS_ConsigneeStreet, hAWB.CS_ConsigneeStreet);
			AssertEquals("Defaulted from Consol originally", "AUSYD", hAWB.CS_RL_NKDestination);
			AssertEquals("AUD", hAWB.CS_RX_NKGoodsCurrency);
			AssertEquals(hAWB.CS_GoodsDescription.Default, hAWB.CS_GoodsDescription);
			AssertEquals(hAWB.CS_GoodsValue.Default, hAWB.CS_GoodsValue);
			AssertEquals(hAWB.CS_HAWB.Default, hAWB.CS_HAWB);
			AssertEquals(hAWB.CS_IsMasterHouse.Default, hAWB.CS_IsMasterHouse);
			AssertEquals("Defaulted from Consol Originally", "USLAX", hAWB.CS_RL_NKOrigin);
			AssertEquals(hAWB.CS_PiecesManifested.Default, hAWB.CS_PiecesManifested);
			AssertEquals("", hAWB.CS_FreightPrepaidCollect);
			AssertEquals(hAWB.CS_MessageReference.Default, hAWB.CS_MessageReference);
			AssertEquals("KG", hAWB.CS_WeightUQ);
		}

		public void TestSystemDefinedOrgsAreLinkedToConsignee()
		{
			var bridge = new HAWBToShipmentBridge(hAWB);

			var consigneePK = Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation;
			shipment.ConsigneePK = consigneePK;

			((IUpdateFromShipment)bridge).CopyConsigneeDetails(shipment);

			AssertEquals(consigneePK, hAWB.Consignee.PK);
		}

		public void TestSystemDefinedOrgsAreLinkedToConsignor()
		{
			var bridge = new HAWBToShipmentBridge(hAWB);

			var consignorPK = Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation;
			shipment.ConsignorPK = consignorPK;

			((IUpdateFromShipment)bridge).CopyConsignorDetails(shipment);

			AssertEquals(consignorPK, hAWB.Consignor.PK);
		}

		public void TestNullNotSupported()
		{
			bool correctExceptionThrown = false;
			try
			{
				HAWBToShipmentBridge bridge = new HAWBToShipmentBridge(null);
			}
			catch (ArgumentNullException)
			{
				correctExceptionThrown = true;
			}
			Assert("ArgumentNullException should have been thrown because MAWB cannot be null", correctExceptionThrown);
		}

		CusMAWB mAWB;
		CusHAWB hAWB;
		ForwardingShipment shipment;

		protected override void SetUp()
		{
			base.SetUp();
			mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08111111111";
			mAWB.CM_FlightNo = "QF123";
			mAWB.CM_ArrivalDate = new ZDateTime(2005, 11, 12);
			mAWB.CM_RL_NKDischargePort = "AUSYD";
			mAWB.CM_RL_NKLoadPort = "USLAX";
			hAWB = mAWB.ChildBills.AddNew();
			shipment = Factory.New<ForwardingShipment>();
			hAWB.CS_JS = shipment.PK;
		}
	}
}
