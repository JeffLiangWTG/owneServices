using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Customs.Asycuda;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeList;
using static Enterprise.Integration.Customs;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaBill))]
	sealed class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
	{
		public void TestCaptions()
		{
			var bill = Factory.New<AsycudaBill>();
			CombineAssertions(() =>
			{
				AssertEquals("ABL_AMA caption", "Manifest", DataBoundResourceStrings.GetDataForProperty(bill.ABL_AMAInfo).Caption);
				AssertEquals("ABL_Incoterm caption", "Incoterm", DataBoundResourceStrings.GetDataForProperty(bill.ABL_IncotermInfo).Caption);
				AssertEquals("ABL_OA_Shipper caption", "Shipper", DataBoundResourceStrings.GetDataForProperty(bill.ABL_OA_ShipperInfo).Caption);
				AssertEquals("ABL_RN_NKShipperCountry caption", "Shipper Country", DataBoundResourceStrings.GetDataForProperty(bill.ABL_RN_NKShipperCountryInfo).Caption);
				AssertEquals("ABL_SequenceNumber caption", "Sequence Number", DataBoundResourceStrings.GetDataForProperty(bill.ABL_SequenceNumberInfo).Caption);
				AssertEquals("ABL_SequenceNumber short caption", "Seq. No", DataBoundResourceStrings.GetDataForProperty(bill.ABL_SequenceNumberInfo).ShortCaption);
				AssertEquals("ABL_ShipperCity caption", "Shipper City", DataBoundResourceStrings.GetDataForProperty(bill.ABL_ShipperCityInfo).Caption);
				AssertEquals("ABL_ShipperName caption", "Shipper Name", DataBoundResourceStrings.GetDataForProperty(bill.ABL_ShipperNameInfo).Caption);
				AssertEquals("ABL_ShipperPostcode caption", "Shipper Postcode", DataBoundResourceStrings.GetDataForProperty(bill.ABL_ShipperPostcodeInfo).Caption);
				AssertEquals("ABL_ShipperState caption", "Shipper State", DataBoundResourceStrings.GetDataForProperty(bill.ABL_ShipperStateInfo).Caption);
				AssertEquals("ABL_ShipperStreet1 caption", "Shipper Street 1", DataBoundResourceStrings.GetDataForProperty(bill.ABL_ShipperStreet1Info).Caption);
				AssertEquals("ABL_ShipperStreet2 caption", "Shipper Street 2", DataBoundResourceStrings.GetDataForProperty(bill.ABL_ShipperStreet2Info).Caption);
				AssertEquals("ABL_UCRNumber caption", "UCR Number", DataBoundResourceStrings.GetDataForProperty(bill.ABL_UCRNumberInfo).Caption);
				AssertEquals("ConsigneeOrgPK caption", "Consignee", DataBoundResourceStrings.GetDataForProperty(bill.ConsigneeOrgPKInfo).Caption);
				AssertEquals("DiscountValue caption", "Discount Value", DataBoundResourceStrings.GetDataForProperty(bill.DiscountValueInfo).Caption);
				AssertEquals("DiscountValue short caption", "Disc. Val.", DataBoundResourceStrings.GetDataForProperty(bill.DiscountValueInfo).ShortCaption);
				AssertEquals("DiscountValueCurrency caption", "Discount Currency", DataBoundResourceStrings.GetDataForProperty(bill.DiscountValueCurrencyInfo).Caption);
				AssertEquals("DiscountValueCurrency medium caption", "Currency", DataBoundResourceStrings.GetDataForProperty(bill.DiscountValueCurrencyInfo).MediumCaption);
				AssertEquals("DiscountValueCurrency short caption", "Curr.", DataBoundResourceStrings.GetDataForProperty(bill.DiscountValueCurrencyInfo).ShortCaption);
				AssertEquals("ForwarderOrgPK caption", "Freight Forwarder", DataBoundResourceStrings.GetDataForProperty(bill.ForwarderOrgPKInfo).Caption);
				AssertEquals("NotifyPartyOrgPK caption", "Notify Party", DataBoundResourceStrings.GetDataForProperty(bill.NotifyPartyOrgPKInfo).Caption);
				AssertEquals("OtherChargesValue caption", "Other Charges Value", DataBoundResourceStrings.GetDataForProperty(bill.OtherChargesValueInfo).Caption);
				AssertEquals("OtherChargesValue caption", "Other Charges", DataBoundResourceStrings.GetDataForProperty(bill.OtherChargesValueInfo).ShortCaption);
				AssertEquals("OtherChargesValueCurrency caption", "Other Charges Currency", DataBoundResourceStrings.GetDataForProperty(bill.OtherChargesValueCurrencyInfo).Caption);
				AssertEquals("OtherChargesValueCurrency medium caption", "Currency", DataBoundResourceStrings.GetDataForProperty(bill.OtherChargesValueCurrencyInfo).MediumCaption);
				AssertEquals("OtherChargesValueCurrency short caption", "Curr.", DataBoundResourceStrings.GetDataForProperty(bill.OtherChargesValueCurrencyInfo).ShortCaption);
				AssertEquals("ShipperOrgPK short caption", "Shipper", DataBoundResourceStrings.GetDataForProperty(bill.ShipperOrgPKInfo).Caption);
				AssertEquals("ABL_CustomsValue caption", "Cust. Val", DataBoundResourceStrings.GetDataForProperty(bill.ABL_CustomsValueInfo).Caption);
				AssertEquals("ABL_CustomsValue caption", "Customs Value", DataBoundResourceStrings.GetDataForProperty(bill.ABL_CustomsValueInfo).MediumCaption);
				AssertEquals("ABL_OA_Seller caption", "Seller", DataBoundResourceStrings.GetDataForProperty(bill.ABL_OA_SellerInfo).Caption);
				AssertEquals("ABL_SellerName caption", "Seller Name", DataBoundResourceStrings.GetDataForProperty(bill.ABL_SellerNameInfo).Caption);
				AssertEquals("ABL_SellerStreet1 caption", "Seller Street 1", DataBoundResourceStrings.GetDataForProperty(bill.ABL_SellerStreet1Info).Caption);
				AssertEquals("ABL_SellerStreet2 caption", "Seller Street 2", DataBoundResourceStrings.GetDataForProperty(bill.ABL_SellerStreet2Info).Caption);
				AssertEquals("ABL_SellerCity caption", "Seller City", DataBoundResourceStrings.GetDataForProperty(bill.ABL_SellerCityInfo).Caption);
				AssertEquals("ABL_SellerState caption", "Seller State", DataBoundResourceStrings.GetDataForProperty(bill.ABL_SellerStateInfo).Caption);
				AssertEquals("ABL_SellerPostcode caption", "Seller Postcode", DataBoundResourceStrings.GetDataForProperty(bill.ABL_SellerPostcodeInfo).Caption);
				AssertEquals("ABL_SellerPhone caption", "Seller Phone", DataBoundResourceStrings.GetDataForProperty(bill.ABL_SellerPhoneInfo).Caption);
				AssertEquals("ABL_RN_NKSellerCountry caption", "Seller Country", DataBoundResourceStrings.GetDataForProperty(bill.ABL_RN_NKSellerCountryInfo).Caption);
				AssertEquals("ABL_SellerRegNo caption", "Seller Reg No", DataBoundResourceStrings.GetDataForProperty(bill.ABL_SellerRegNoInfo).Caption);
				AssertEquals("ABL_SellerRegNoType caption", "Seller Reg No Type", DataBoundResourceStrings.GetDataForProperty(bill.ABL_SellerRegNoTypeInfo).Caption);
				AssertEquals("ABL_RX_NKCustomsValueCurrency caption", "Customs Value Currency", DataBoundResourceStrings.GetDataForProperty(bill.ABL_RX_NKCustomsValueCurrencyInfo).Caption);
				AssertEquals("ABL_OA_ContainerAgent caption", "Agent", DataBoundResourceStrings.GetDataForProperty(bill.ABL_OA_ContainerAgentInfo).Caption);
			});
		}

		public void TestIAmsBill_IsBillAlreadyOnFile()
		{
			var bill = CreateBillForHasBeenSubmittedTests();
			var provider = (IManifestBillForSynchroniser)bill;
			CombineAssertions(() =>
			{
				AssertEquals("Not Submitted", false, bill.HasManifestBeenSubmittedToCustoms);
				AssertEquals("Not Submitted->IsBillAlreadyOnFile", false, provider.IsBillAlreadyOnFile);
				bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Awaiting;
				AssertEquals("Submitted", true, bill.HasManifestBeenSubmittedToCustoms);
				AssertEquals("Submitted->IsBillAlreadyOnFile", true, provider.IsBillAlreadyOnFile);
			});
		}

		public void TestISetterSuspenderSupporterMembers()
		{
			var bill = Factory.New<AsycudaBill>();
			ISetterSuspenderSupporter supporter = bill;
			var setter = supporter.SetterSuspender;
			AssertNotNull(setter);
			AssertSame(setter, supporter.SetterSuspender);
			AssertEquals(0, supporter.SupportedFields.Count());
		}

		public void TestABL_SequenceNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var bill3 = header.Bills.AddNew();
			var bill4 = header.Bills.AddNew();
			AssertEquals((short)1, bill1.ABL_SequenceNumber);
			AssertEquals((short)2, bill2.ABL_SequenceNumber);
			AssertEquals((short)3, bill3.ABL_SequenceNumber);
			AssertEquals((short)4, bill4.ABL_SequenceNumber);

			bill3.Delete();
			AssertEquals((short)1, bill1.ABL_SequenceNumber);
			AssertEquals((short)2, bill2.ABL_SequenceNumber);
			AssertEquals((short)3, bill4.ABL_SequenceNumber);

			var bill5 = header.Bills.AddNew();
			AssertEquals((short)4, bill5.ABL_SequenceNumber);
		}

		public void TestBusinessObjectsWithRelatedEvents()
		{
			var manifestHeader = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Singapore, "MGI");
			manifestHeader.FillWithValidTestData();
			var bill = manifestHeader.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.GetSGPackedItemForTesting();
			Assert("Should be something in the related logs", bill.BusinessObjectsWithRelatedEvents.Length > 0);
			Assert("Pack should be in the list of related objects", ((IList)bill.BusinessObjectsWithRelatedEvents).Contains(pack));
			Assert("Pack country should not be in the list of related objects", !((IList)bill.BusinessObjectsWithRelatedEvents).Contains(packedItem));
		}

		public void TestBillNumberWithHyphen()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "!ABC--1234 56789";
			AssertEquals("ABC-12345678", bill.BillNumberWithHyphen);
		}

		public void TestISelectionItem_SelectionDescription_CycleFields()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill = (Integration.Customs.ASYCUDA.SGAccess.IAsycudaBill)header.Bills.AddNew();
			bill.ABL_BillNumber = "Bill1";
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;
			//Todo Cycle Fields on Bill
			bill.CycleDate = new ZDateTime(2018, 6, 7);
			bill.CycleNumber = "2";

			AssertContains("Bill Number - Bill1 - Cycle: 07-Jun-18/2", ((ISelectionItem)bill).SelectionDescription(true));

			AssertNotContains("Cycle: 07-Jun-18/2", ((ISelectionItem)bill).SelectionDescription(false));
		}

		public void TestDefaultPartyOrgAddressDetails()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var org = Factory.NewWithValidTestData<OrgAddress>();
			org.Header.OH_FullName = "FullName";
			org.OA_Address1 = "Address1";
			org.OA_Address2 = "Address2";
			org.OA_City = "SIN";
			org.OA_State = "STATE";
			org.OA_PostCode = "0001";
			org.OA_Phone = "+00123456888";
			org.OA_RN_NKCountryCode = "SG";

			bill.ABL_OA_Shipper = org.PK;
			AssertEquals("Shipper", "FullName", bill.ABL_ShipperName);
			AssertEquals("Shipper", "Address1", bill.ABL_ShipperStreet1);
			AssertEquals("Shipper", "Address2", bill.ABL_ShipperStreet2);
			AssertEquals("Shipper", "SIN", bill.ABL_ShipperCity);
			AssertEquals("Shipper", "STATE", bill.ABL_ShipperState);
			AssertEquals("Shipper", "0001", bill.ABL_ShipperPostcode);
			AssertEquals("Shipper", "+00123456888", bill.ABL_ShipperPhone);
			AssertEquals("Shipper", "SG", bill.ABL_RN_NKShipperCountry);

			bill.ABL_OA_Consignee = org.PK;
			AssertEquals("Consignee", "FullName", bill.ABL_ConsigneeName);
			AssertEquals("Consignee", "Address1", bill.ABL_ConsigneeStreet1);
			AssertEquals("Consignee", "Address2", bill.ABL_ConsigneeStreet2);
			AssertEquals("Consignee", "SIN", bill.ABL_ConsigneeCity);
			AssertEquals("Consignee", "STATE", bill.ABL_ConsigneeState);
			AssertEquals("Consignee", "0001", bill.ABL_ConsigneePostcode);
			AssertEquals("Consignee", "+00123456888", bill.ABL_ConsigneePhone);
			AssertEquals("Consignee", "SG", bill.ABL_RN_NKConsigneeCountry);

			bill.ABL_OA_NotifyParty = org.PK;
			AssertEquals("NotifyParty", "FullName", bill.ABL_NotifyPartyName);
			AssertEquals("NotifyParty", "Address1", bill.ABL_NotifyPartyStreet1);
			AssertEquals("NotifyParty", "Address2", bill.ABL_NotifyPartyStreet2);
			AssertEquals("NotifyParty", "SIN", bill.ABL_NotifyPartyCity);
			AssertEquals("NotifyParty", "STATE", bill.ABL_NotifyPartyState);
			AssertEquals("NotifyParty", "0001", bill.ABL_NotifyPartyPostcode);
			AssertEquals("NotifyParty", "+00123456888", bill.ABL_NotifyPartyPhone);
			AssertEquals("NotifyParty", "SG", bill.ABL_RN_NKNotifyPartyCountry);

			bill.ABL_OA_Buyer = org.PK;
			AssertEquals("Buyer", "FullName", bill.ABL_BuyerName);
			AssertEquals("Buyer", "Address1", bill.ABL_BuyerStreet1);
			AssertEquals("Buyer", "Address2", bill.ABL_BuyerStreet2);
			AssertEquals("Buyer", "SIN", bill.ABL_BuyerCity);
			AssertEquals("Buyer", "STATE", bill.ABL_BuyerState);
			AssertEquals("Buyer", "0001", bill.ABL_BuyerPostcode);
			AssertEquals("Buyer", "+00123456888", bill.ABL_BuyerPhone);
			AssertEquals("Buyer", "SG", bill.ABL_RN_NKBuyerCountry);

			bill.ABL_OA_Shipper = ZGuid.Empty;
			bill.ABL_OA_Consignee = ZGuid.Empty;
			bill.ABL_OA_NotifyParty = ZGuid.Empty;
			bill.ABL_OA_Buyer = ZGuid.Empty;
			org.OA_CompanyNameOverride = "Override Company Name";
			bill.ABL_OA_Shipper = org.PK;
			bill.ABL_OA_Consignee = org.PK;
			bill.ABL_OA_NotifyParty = org.PK;
			bill.ABL_OA_Buyer = org.PK;

			AssertEquals("Shipper", "Override Company Name", bill.ABL_ShipperName);
			AssertEquals("Consignee", "Override Company Name", bill.ABL_ConsigneeName);
			AssertEquals("NotifyParty", "Override Company Name", bill.ABL_NotifyPartyName);
			AssertEquals("Buyer", "Override Company Name", bill.ABL_BuyerName);
		}

		public void TestPartyOrgAddressValueNotClearWhenNewValueIsEmpty()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var org = Factory.NewWithValidTestData<OrgAddress>();
			org.Header.OH_FullName = "FullName";
			org.OA_Address1 = "Address1";
			org.OA_Address2 = "Address2";
			org.OA_City = "SIN";
			org.OA_State = "STATE";
			org.OA_PostCode = "0001";
			org.OA_Phone = "+00123456888";
			org.OA_RN_NKCountryCode = "SG";

			bill.ABL_OA_Shipper = org.PK;
			AssertEquals("Shipper", "FullName", bill.ABL_ShipperName);
			AssertEquals("Shipper", "Address1", bill.ABL_ShipperStreet1);
			AssertEquals("Shipper", "Address2", bill.ABL_ShipperStreet2);
			AssertEquals("Shipper", "SIN", bill.ABL_ShipperCity);
			AssertEquals("Shipper", "STATE", bill.ABL_ShipperState);
			AssertEquals("Shipper", "0001", bill.ABL_ShipperPostcode);
			AssertEquals("Shipper", "+00123456888", bill.ABL_ShipperPhone);
			AssertEquals("Shipper", "SG", bill.ABL_RN_NKShipperCountry);

			bill.ABL_OA_Consignee = org.PK;
			AssertEquals("Consignee", "FullName", bill.ABL_ConsigneeName);
			AssertEquals("Consignee", "Address1", bill.ABL_ConsigneeStreet1);
			AssertEquals("Consignee", "Address2", bill.ABL_ConsigneeStreet2);
			AssertEquals("Consignee", "SIN", bill.ABL_ConsigneeCity);
			AssertEquals("Consignee", "STATE", bill.ABL_ConsigneeState);
			AssertEquals("Consignee", "0001", bill.ABL_ConsigneePostcode);
			AssertEquals("Consignee", "+00123456888", bill.ABL_ConsigneePhone);
			AssertEquals("Consignee", "SG", bill.ABL_RN_NKConsigneeCountry);

			bill.ABL_OA_NotifyParty = org.PK;
			AssertEquals("NotifyParty", "FullName", bill.ABL_NotifyPartyName);
			AssertEquals("NotifyParty", "Address1", bill.ABL_NotifyPartyStreet1);
			AssertEquals("NotifyParty", "Address2", bill.ABL_NotifyPartyStreet2);
			AssertEquals("NotifyParty", "SIN", bill.ABL_NotifyPartyCity);
			AssertEquals("NotifyParty", "STATE", bill.ABL_NotifyPartyState);
			AssertEquals("NotifyParty", "0001", bill.ABL_NotifyPartyPostcode);
			AssertEquals("NotifyParty", "+00123456888", bill.ABL_NotifyPartyPhone);
			AssertEquals("NotifyParty", "SG", bill.ABL_RN_NKNotifyPartyCountry);

			bill.ABL_OA_Shipper = ZGuid.Empty;
			bill.ABL_OA_Consignee = ZGuid.Empty;
			bill.ABL_OA_NotifyParty = ZGuid.Empty;

			AssertEquals("Shipper", "FullName", bill.ABL_ShipperName);
			AssertEquals("Shipper", "Address1", bill.ABL_ShipperStreet1);
			AssertEquals("Shipper", "Address2", bill.ABL_ShipperStreet2);
			AssertEquals("Shipper", "SIN", bill.ABL_ShipperCity);
			AssertEquals("Shipper", "STATE", bill.ABL_ShipperState);
			AssertEquals("Shipper", "0001", bill.ABL_ShipperPostcode);
			AssertEquals("Shipper", "+00123456888", bill.ABL_ShipperPhone);
			AssertEquals("Shipper", "SG", bill.ABL_RN_NKShipperCountry);

			AssertEquals("Consignee", "FullName", bill.ABL_ConsigneeName);
			AssertEquals("Consignee", "Address1", bill.ABL_ConsigneeStreet1);
			AssertEquals("Consignee", "Address2", bill.ABL_ConsigneeStreet2);
			AssertEquals("Consignee", "SIN", bill.ABL_ConsigneeCity);
			AssertEquals("Consignee", "STATE", bill.ABL_ConsigneeState);
			AssertEquals("Consignee", "0001", bill.ABL_ConsigneePostcode);
			AssertEquals("Consignee", "+00123456888", bill.ABL_ConsigneePhone);
			AssertEquals("Consignee", "SG", bill.ABL_RN_NKConsigneeCountry);

			AssertEquals("NotifyParty", "FullName", bill.ABL_NotifyPartyName);
			AssertEquals("NotifyParty", "Address1", bill.ABL_NotifyPartyStreet1);
			AssertEquals("NotifyParty", "Address2", bill.ABL_NotifyPartyStreet2);
			AssertEquals("NotifyParty", "SIN", bill.ABL_NotifyPartyCity);
			AssertEquals("NotifyParty", "STATE", bill.ABL_NotifyPartyState);
			AssertEquals("NotifyParty", "0001", bill.ABL_NotifyPartyPostcode);
			AssertEquals("NotifyParty", "+00123456888", bill.ABL_NotifyPartyPhone);
			AssertEquals("NotifyParty", "SG", bill.ABL_RN_NKNotifyPartyCountry);
		}

		public void TestBuyerClearValueIfNeeded()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgAddress.OA_RN_NKCountryCode = "";
			CombineAssertions(() =>
			{
				AssertClearValueWithBuyer(bill.ABL_BuyerNameInfo);
				AssertClearValueWithBuyer(bill.ABL_BuyerStreet1Info);
				AssertClearValueWithBuyer(bill.ABL_BuyerStreet2Info);
				AssertClearValueWithBuyer(bill.ABL_BuyerCityInfo);
				AssertClearValueWithBuyer(bill.ABL_BuyerStateInfo);
				AssertClearValueWithBuyer(bill.ABL_BuyerPostcodeInfo);
				AssertClearValueWithBuyer(bill.ABL_RN_NKBuyerCountryInfo);
				AssertClearValueWithBuyer(bill.ABL_BuyerPhoneInfo);
				AssertClearValueWithBuyer(bill.ABL_BuyerRegNoInfo);
				AssertClearValueWithBuyer(bill.ABL_BuyerRegNoTypeInfo);
			});

			void AssertClearValueWithBuyer(ZPropertyInfo targetInfo)
			{
				targetInfo.Value = new ZString("XX");
				bill.ABL_OA_Buyer = orgAddress.PK;
				AssertEquals($"When Buyer filled, {targetInfo.Name} Value", ZString.Empty, targetInfo.Value);

				targetInfo.Value = new ZString("XY");
				bill.ABL_OA_Buyer = ZGuid.Empty;
				AssertNotEquals($"When Buyer empty, {targetInfo.Name} Value", ZString.Empty, targetInfo.Value);
			}
		}

		public void TestCanConvertBuyerToOrganization()
		{
			CombineAssertions(() =>
			{
				var bill = (AsycudaBill)GetNewBusinessObject();

				AssertEquals("No address field set, we have no details with which to create an org.", false, bill.CanConvertBuyerToOrganization);

				bill = (AsycudaBill)GetNewBusinessObject();
				bill.ABL_BuyerName = "Test";
				AssertEquals(true, bill.CanConvertBuyerToOrganization);

				bill = (AsycudaBill)GetNewBusinessObject();
				bill.ABL_BuyerStreet1 = "Test";
				AssertEquals(true, bill.CanConvertBuyerToOrganization);

				bill = (AsycudaBill)GetNewBusinessObject();
				bill.ABL_BuyerStreet2 = "Test";
				AssertEquals(true, bill.CanConvertBuyerToOrganization);

				bill = (AsycudaBill)GetNewBusinessObject();
				bill.ABL_BuyerCity = "Test";
				AssertEquals(true, bill.CanConvertBuyerToOrganization);

				bill = (AsycudaBill)GetNewBusinessObject();
				bill.ABL_BuyerState = "Test";
				AssertEquals(true, bill.CanConvertBuyerToOrganization);

				bill = (AsycudaBill)GetNewBusinessObject();
				bill.ABL_BuyerPostcode = "12345";
				AssertEquals(true, bill.CanConvertBuyerToOrganization);

				bill = (AsycudaBill)GetNewBusinessObject();
				bill.ABL_RN_NKBuyerCountry = "DE";
				AssertEquals(true, bill.CanConvertBuyerToOrganization);

				bill = (AsycudaBill)GetNewBusinessObject();
				bill.ABL_BuyerPhone = "09876543";
				AssertEquals(true, bill.CanConvertBuyerToOrganization);

				bill.ABL_OA_Buyer = Factory.New<OrgAddress>().PK;
				AssertEquals("Buyer Address is already set", false, bill.CanConvertBuyerToOrganization);
			});
		}

		public void TestBuyerABLAddress()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "BOB THE BUILDER";
			org.OH_RL_NKClosestPort = "AU";
			var orgAddress = org.MainAddress;
			orgAddress.OA_Address1 = "ADDRESS 1";
			orgAddress.OA_Address2 = "ADDRESS 2";
			orgAddress.OA_City = "CITY";
			orgAddress.OA_State = "STATE";
			orgAddress.OA_PostCode = "203023";
			orgAddress.OA_Phone = "+4234232";
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.ABL_OA_Buyer = orgAddress.PK;
			IAddressDetails addressDetails = orgAddress;
			var billAddress = bill.BuyerABLAddress;
			AssertEquals(addressDetails.CompanyName, billAddress.CompanyName);
			AssertEquals(addressDetails.AddressLine1, billAddress.Address1);
			AssertEquals(addressDetails.AddressLine2, billAddress.Address2);
			AssertEquals(addressDetails.City, billAddress.City);
			AssertEquals(addressDetails.State, billAddress.State);
			AssertEquals(addressDetails.PostCode, billAddress.Postcode);
			AssertEquals(addressDetails.Country, billAddress.RN_NKCountryCode);
			AssertEquals(addressDetails.Phone, billAddress.Phone);
		}

		public void TestBuyerUseRealOrg()
		{
			CombineAssertions(() =>
			{
				var bill = (AsycudaBill)GetNewBusinessObject();
				AssertEquals("No Buyer is set", false, bill.BuyerUseRealOrg);
				bill.ABL_OA_Buyer = Factory.New<OrgAddress>().PK;
				AssertEquals("A Buyer is set", true, bill.BuyerUseRealOrg);
			});
		}

		public void TestBuyerFieldsReadOnly()
		{
			CombineAssertions(() =>
			{
				var bill = (AsycudaBill)GetNewBusinessObject();
				AssertEquals("No Buyer is set", false, bill.BuyerFieldsReadOnly);
				bill.ABL_OA_Buyer = Factory.New<OrgAddress>().PK;
				AssertEquals("A Buyer is set", true, bill.BuyerFieldsReadOnly);
			});
		}

		public void TestBuyerRelatedFieldsReadOnly()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				AssertReadOnly(bill.ABL_BuyerNameInfo);
				AssertReadOnly(bill.ABL_BuyerStreet1Info);
				AssertReadOnly(bill.ABL_BuyerStreet2Info);
				AssertReadOnly(bill.ABL_BuyerCityInfo);
				AssertReadOnly(bill.ABL_BuyerStateInfo);
				AssertReadOnly(bill.ABL_BuyerPostcodeInfo);
				AssertReadOnly(bill.ABL_RN_NKBuyerCountryInfo);
				AssertReadOnly(bill.ABL_BuyerPhoneInfo);
				AssertReadOnly(bill.ABL_BuyerRegNoInfo);
				AssertReadOnly(bill.ABL_BuyerRegNoTypeInfo);
			});

			void AssertReadOnly(ZPropertyInfo targetInfo)
			{
				bill.ABL_OA_Buyer = orgAddress.PK;
				AssertEquals($"When Buyer filled, {targetInfo.Name} ReadOnly", true, targetInfo.ReadOnly);
				bill.ABL_OA_Buyer = ZGuid.Empty;
				AssertEquals($"When Buyer empty, {targetInfo.Name} ReadOnly", false, targetInfo.ReadOnly);
			}
		}

		public void TestBuyerOrgPKCaption()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var propertyData = DataBoundResourceStrings.GetDataForProperty(bill.BuyerOrgPKInfo);
			AssertEquals("Caption", "Buyer", propertyData.Caption);
		}

		public void TestHasManifestBeenSubmittedToCustomsIncludingChildren()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals(false, bill.HasManifestBeenSubmittedToCustomsIncludingChildren);

			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItemForTesting();
			AssertEquals(false, bill.HasManifestBeenSubmittedToCustomsIncludingChildren);
			packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Awaiting;
			AssertEquals(true, bill.HasManifestBeenSubmittedToCustomsIncludingChildren);
			packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Error;
			AssertEquals(false, bill.HasManifestBeenSubmittedToCustomsIncludingChildren);

			AssertEquals(false, bill.HasManifestBeenSubmittedToCustomsIncludingChildren);
			bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Awaiting;
			AssertEquals(true, bill.HasManifestBeenSubmittedToCustomsIncludingChildren);
			bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Error;
			AssertEquals(false, bill.HasManifestBeenSubmittedToCustomsIncludingChildren);
		}

		public void TestPacksAreReapportionWhenBillIsSaved()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";

			var packedItems = new List<AsycudaPackedItem>();

			for (var i = 0; i < 5; i++)
			{
				var bill = header.Bills.AddNew();
				bill.FillWithValidTestData();
				bill.ABL_BillNumber = i.ToString("0000");

				bill.ABL_TransportValue = 10;
				bill.ABL_InsuranceValue = 20m;
				bill.OtherChargesValue = 30m;
				bill.DiscountValue = 40m;
				bill.ABL_CustomsValue = 0m;

				bill.ABL_RX_NKTransportValueCurrency = Core.Constants.CurrencyCodes.Singapore;
				bill.ABL_RX_NKInsuranceValueCurrency = Core.Constants.CurrencyCodes.Singapore;
				bill.OtherChargesValueCurrency = Core.Constants.CurrencyCodes.Singapore;
				bill.DiscountValueCurrency = Core.Constants.CurrencyCodes.Singapore;

				bill.DutyAmount = 0m;
				bill.TaxAmount = 0m;

				var pack = bill.Packs.AddNew();
				pack.LinePrice = 15m;
				pack.LinePriceCurrency = Core.Constants.CurrencyCodes.Singapore;

				var packedItem = pack.PackedItemForTesting();
				packedItem.API_CustomsValue = 999m;

				packedItems.Add(packedItem);
			}

			CombineAssertions(() =>
			{
				foreach (var country in packedItems)
				{
					AssertEquals("Should be true as many related values are changed.", true, country.Pack.Bill.ApportionmentDirty);
					AssertEquals(999m, country.API_CustomsValue);
				}
			});

			Factory.Save();

			CombineAssertions(() =>
			{
				foreach (var country in packedItems)
				{
					AssertEquals("Should be false after the calculation.", false, country.Pack.Bill.ApportionmentDirty);
					AssertEquals("15 + (30 + 10 + 20 - 40) * 1", 35m, country.API_CustomsValue);
				}
			});

			var newFactory = NewFactory();
			var query = new ZQuery(AsycudaPackedItemSchema.PK, packedItems.Select(c => c.PK));

			packedItems = newFactory.Load<AsycudaPackedItem>(query).ToList();

			CombineAssertions(() =>
			{
				foreach (var country in packedItems)
				{
					AssertEquals("Should default to false.", false, country.Pack.Bill.ApportionmentDirty);
					AssertEquals("Should are saved.", 35m, country.API_CustomsValue);
				}
			});
		}

		public void TestPacksAreDeletedWhenBillIsSaved()
		{
			using (ObjectFactory.Substitute("GlobalManifestApplicationBusinessProvider", ObjectFactory.Get<IEnumerable>("GlobalManifestApplicationBusinessProvider").Cast<ApplicationBusinessProvider>().Append(new DummyApplicationBusinessProvider("XYZ", Core.Constants.CountryCodes.UnitedStates)).ToList()))
			{
				var sgHeader = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
				sgHeader.AMA_ManifestType = "MGI";
				var sgBill = sgHeader.Bills.AddNew();
				var sgPack = sgBill.Packs.AddNew();

				var usHeader = Factory.New<DummyAsycudaManifestHeader>();
				usHeader.AMA_ManifestType = "XYZ";
				var usBill = usHeader.Bills.AddNew();
				var usPack = usBill.Packs.AddNew();

				Assert("A AsycudaManifestHeader whose SupportsAsycudaPacks should be true", sgHeader.ApplicationBusinessProvider.FeatureProvider.SupportsAsycudaPacks);
				Assert("A AsycudaManifestHeader whose SupportsAsycudaPacks should be false", !usHeader.ApplicationBusinessProvider.FeatureProvider.SupportsAsycudaPacks);

				Factory.Save();

				var newFactory = NewFactory();
				var sgQuery = new ZQuery(AsycudaPackSchema.PK, sgPack.PK);
				var usQuery = new ZQuery(AsycudaPackSchema.PK, usPack.PK);

				var sgPacks = newFactory.Load<AsycudaPack>(sgQuery).ToList();
				AssertEquals("Should not delete Packs when SupportsAsycudaPacks is true", 1, sgPacks.Count);
				var usPacks = newFactory.Load<AsycudaPack>(usQuery).ToList();
				AssertEquals("Should delete Packs when SupportsAsycudaPacks is false", 0, usPacks.Count);
			}
		}

		public void TestUpdateApportionmentDirty_Property()
		{
			AssertApportionmentDirty(AsycudaBill.Schema.DiscountValue, AsycudaBill.Schema.DiscountValueCurrency);
			AssertApportionmentDirty(AsycudaBill.Schema.OtherChargesValue, AsycudaBill.Schema.OtherChargesValueCurrency);
			AssertApportionmentDirty(AutoAsycudaBill.Schema.ABL_TransportValue, AutoAsycudaBill.Schema.ABL_RX_NKTransportValueCurrency);
			AssertApportionmentDirty(AutoAsycudaBill.Schema.ABL_InsuranceValue, AutoAsycudaBill.Schema.ABL_RX_NKInsuranceValueCurrency);
		}

		public void TestABL_CustomsValue_ReadOnly()
		{
			var sgHeader = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			var sgBill = sgHeader.Bills.AddNew();
			AssertEquals("Should be true.", true, sgBill.ABL_CustomsValueInfo.ReadOnly);

			var usHeader = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			var usBill = usHeader.Bills.AddNew();
			AssertEquals("Should be false.", false, usBill.ABL_CustomsValueInfo.ReadOnly);
		}

		public void TestABL_RX_NKCustomsValueCurrency_ReadOnly()
		{
			var sgHeader = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			var sgBill = sgHeader.Bills.AddNew();
			sgBill.ABL_FreightValue = 1m;
			AssertEquals("Should be true.", true, sgBill.ABL_RX_NKCustomsValueCurrencyInfo.ReadOnly);
			AssertEquals("Default value is SGD", Core.Constants.CurrencyCodes.Singapore, sgBill.ABL_RX_NKCustomsValueCurrency);

			var usHeader = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			var usBill = usHeader.Bills.AddNew();
			usBill.ABL_FreightValue = 1m;
			AssertEquals("Should be false.", false, usBill.ABL_RX_NKCustomsValueCurrencyInfo.ReadOnly);
		}

		public void TestABL_BillNumber_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			AssertEquals("Default to false.", false, bill.ABL_BillNumberInfo.ReadOnly);
		}

		public void TestCanDelete()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.FillWithValidTestData();
			header.AMA_TransportMode = "AIR";
			header.AMA_ManifestType = "SGA";

			var bill = header.Bills.AddNew();
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Export22;
			bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Awaiting;
			AssertEquals(false, bill.CanDelete);
			AssertEquals("This Bill is already registered with Customs.\r\nYou need to delete it from Customs file before deleting it here.", bill.ReasonForNotAbleToDelete);

			bill.ABL_MessageStatus = "";
			AssertEquals(true, bill.CanDelete);
		}

		public void TestShouldBeReadOnly_WhenBillMessageStatusIsUPDAndIsSGExport_ExpectFalse()
		{
			TestShouldBeReadOnly_Bill(false, MessageStatusCodeList.Codes.Updated, ShipmentTypeList.Codes.Export22);
		}

		public void TestShouldBeReadOnly_WhenPackMessageStatusIsBlank_ExpectFalse()
		{
			TestShouldBeReadOnly_Bill(false, "");
		}

		public void TestShouldBeReadOnly_WhenPackMessageStatusIsNOT_ExpectFalse()
		{
			TestShouldBeReadOnly_Bill(false, MessageStatusCodeList.Codes.NotSent);
		}

		public void TestShouldBeReadOnly_WhenPackMessageStatusIsERR_ExpectFalse()
		{
			TestShouldBeReadOnly_Bill(false, MessageStatusCodeList.Codes.Error);
		}

		public void TestShouldBeReadOnly_WhenPackMessageStatusIsUPDAndIsSGImport_ExpectTrue()
		{
			TestShouldBeReadOnly_Bill(true, MessageStatusCodeList.Codes.Updated, ShipmentTypeList.Codes.Import23);
		}

		public void TestShouldBeReadOnly_WhenPackMessageStatusIsACP_ExpectTrue()
		{
			TestShouldBeReadOnly_Bill(true, MessageStatusCodeList.Codes.Accepted);
		}

		public void TestShouldBeReadOnly_WhenPackMessageStatusIsAWA_ExpectTrue()
		{
			TestShouldBeReadOnly_Bill(true, MessageStatusCodeList.Codes.Awaiting);
		}

		public void TestShouldBeReadOnly_WhenPackMessageStatusIsSNT_ExpectTrue()
		{
			TestShouldBeReadOnly_Bill(true, MessageStatusCodeList.Codes.Sent);
		}

		public void TestShouldBeReadOnly_WhenPackMessageStatusIsUNK_ExpectTrue()
		{
			TestShouldBeReadOnly_Bill(true, MessageStatusCodeList.Codes.Unknown);
		}

		public void TestAmendBillDetails()
		{
			Factory.Save();

			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.FillWithValidTestData();
			header.AMA_TransportMode = "AIR";
			header.AMA_ManifestType = "MGI";
			AssertEquals(true, header.LockedBills);
			header.EnableBillsLock(true);
			var bill = header.Bills.AddNew();
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Export22;

			var pack = bill.Packs.AddNew();
			var sGPackedItem = pack.GetSGPackedItemForTesting();
			sGPackedItem.API_MessageStatus = MessageStatusCodeList.Codes.Awaiting;

			AssertEquals(true, bill.ShouldBeReadOnly);
			header.RefreshBillLock(bill);
			AssertEquals(true, bill.ABL_BillNumberInfo.ReadOnly);

			bill.AmendBillDetails();
			AssertEquals(false, bill.ShouldBeReadOnly);
			AssertEquals(MessageStatusCodeList.Codes.Updated, sGPackedItem.API_MessageStatus);
		}

		public void TestCargoStatusIndicator()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals(CargoStatusIndicator.Fullshipment, bill.CargoStatusIndicator);
			bill.ABL_CargoStatus = CargoStatusList.Codes.LastPartShipment;
			AssertEquals(CargoStatusIndicator.LastPartShipment, bill.CargoStatusIndicator);
			bill.ABL_CargoStatus = CargoStatusList.Codes.PartShipment;
			AssertEquals(CargoStatusIndicator.PartShipment, bill.CargoStatusIndicator);
		}

		public void TestGetNewValidation()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var child = header.MasterBill;
			var realBill = header.Bills.AddNew();
			AssertEquals("GetNewValidationForRegularBill is overrided in ASYCUDAManifest.Business", "Enterprise.Customs.ASYCUDAManifest.Business.AsycudaBillValidationForRegularBill", realBill.Validation.GetType().ToString());
			AssertType(typeof(AsycudaBillValidationForMasterChild), child.Validation);
		}

		public void TestFreightAndCustomsValue()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals(0m, bill.ABL_CustomsValue);
			AssertEquals(0m, bill.ABL_FreightValue);
			bill.ABL_FreightValue = 10m;
			AssertEquals(10m, bill.ABL_CustomsValue);
			AssertEquals(10m, bill.ABL_FreightValue);
			bill.ABL_FreightValue = 20m;
			AssertEquals(20m, bill.ABL_CustomsValue);
			AssertEquals(20m, bill.ABL_FreightValue);
			bill.ABL_CustomsValue = 30m;
			AssertEquals(30m, bill.ABL_CustomsValue);
			AssertEquals(20m, bill.ABL_FreightValue);
			bill.ABL_FreightValue = 40m;
			AssertEquals(30m, bill.ABL_CustomsValue);
			AssertEquals(40m, bill.ABL_FreightValue);

			bill = header.Bills.AddNew();
			AssertEquals(0m, bill.ABL_CustomsValue);
			AssertEquals(0m, bill.ABL_FreightValue);
			bill.ABL_CustomsValue = 30m;
			AssertEquals(30m, bill.ABL_CustomsValue);
			AssertEquals(0m, bill.ABL_FreightValue);
			bill.ABL_FreightValue = 40m;
			AssertEquals(30m, bill.ABL_CustomsValue);
			AssertEquals(40m, bill.ABL_FreightValue);
		}

		public void TestShipperDefaultAddress()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "BOB THE BUILDER";
			org.OH_RL_NKClosestPort = "AU";
			var mainAddress = org.MainAddress;
			mainAddress.OA_Address1 = "MAIN ADDRESS 1";
			mainAddress.OA_Address2 = "ADDRESS 2";
			mainAddress.OA_City = "CITY";
			mainAddress.OA_State = "STATE";
			mainAddress.OA_PostCode = "203023";
			mainAddress.OA_Phone = "+4234232";
			var pickupAddress = org.Addresses.AddNew(OrgAddressType.Pickup, true);
			pickupAddress.OA_Address1 = "PICKUP ADDRESS 1";
			pickupAddress.OA_Address2 = "ADDRESS 2";
			pickupAddress.OA_City = "CITY";
			pickupAddress.OA_State = "STATE";
			pickupAddress.OA_PostCode = "203023";
			pickupAddress.OA_Phone = "+4234232";
			var deliveryAddress = org.Addresses.AddNew(OrgAddressType.Delivery, true);
			deliveryAddress.OA_Address1 = "DELIVERY ADDRESS 1";
			deliveryAddress.OA_Address2 = "ADDRESS 2";
			deliveryAddress.OA_City = "CITY";
			deliveryAddress.OA_State = "STATE";
			deliveryAddress.OA_PostCode = "203023";
			deliveryAddress.OA_Phone = "+4234232";
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ShipperOrgPK = org.PK;
			AssertEquals("Should be the pickup", pickupAddress.PK, bill.ABL_OA_Shipper);
		}

		public void TestShipper()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "BOB THE BUILDER";
			org.OH_RL_NKClosestPort = "AU";
			var orgAddress = org.MainAddress;
			orgAddress.OA_Address1 = "ADDRESS 1";
			orgAddress.OA_Address2 = "ADDRESS 2";
			orgAddress.OA_City = "CITY";
			orgAddress.OA_State = "STATE";
			orgAddress.OA_PostCode = "203023";
			orgAddress.OA_Phone = "+4234232";
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_OA_Shipper = ZGuid.Empty;
			AssertEquals("bill.ABL_ShipperNameInfo.ReadOnly", false, bill.ABL_ShipperNameInfo.ReadOnly);
			bill.ABL_ShipperName = "BOB";
			AssertEquals("bill.ABL_ShipperStreet1Info.ReadOnly", false, bill.ABL_ShipperStreet1Info.ReadOnly);
			bill.ABL_ShipperStreet1 = "STREET 1";
			AssertEquals("bill.ABL_ShipperStreet2Info.ReadOnly", false, bill.ABL_ShipperStreet2Info.ReadOnly);
			bill.ABL_ShipperStreet2 = "STREET 2";
			AssertEquals("bill.ABL_ShipperCityInfo.ReadOnly", false, bill.ABL_ShipperCityInfo.ReadOnly);
			bill.ABL_ShipperCity = "CT";
			AssertEquals("bill.ABL_ShipperStateInfo.ReadOnly", false, bill.ABL_ShipperStateInfo.ReadOnly);
			bill.ABL_ShipperState = "ST";
			AssertEquals("bill.ABL_ShipperPostcodeInfo.ReadOnly", false, bill.ABL_ShipperPostcodeInfo.ReadOnly);
			bill.ABL_ShipperPostcode = "4343";
			AssertEquals("bill.ABL_RN_NKShipperCountryInfo.ReadOnly", false, bill.ABL_RN_NKShipperCountryInfo.ReadOnly);
			bill.ABL_RN_NKShipperCountry = "NZ";
			AssertEquals("bill.ABL_ShipperRegNoInfo.ReadOnly", false, bill.ABL_ShipperRegNoInfo.ReadOnly);
			bill.ABL_ShipperRegNo = "123456";
			AssertEquals("bill.ABL_ShipperPhoneInfo.ReadOnly", false, bill.ABL_ShipperPhoneInfo.ReadOnly);
			bill.ABL_ConsigneePhone = "+4234232";

			bill.ABL_OA_Shipper = orgAddress.PK;
			AssertPropertyInfo(bill.ABL_ShipperNameInfo, true, orgAddress.Header.OH_FullName);
			AssertPropertyInfo(bill.ABL_ShipperStreet1Info, true, orgAddress.OA_Address1);
			AssertPropertyInfo(bill.ABL_ShipperStreet2Info, true, orgAddress.OA_Address2);
			AssertPropertyInfo(bill.ABL_ShipperCityInfo, true, orgAddress.OA_City);
			AssertPropertyInfo(bill.ABL_ShipperStateInfo, true, orgAddress.OA_State);
			AssertPropertyInfo(bill.ABL_ShipperPostcodeInfo, true, orgAddress.OA_PostCode);
			AssertPropertyInfo(bill.ABL_RN_NKShipperCountryInfo, true, orgAddress.OA_RN_NKCountryCode);
			AssertPropertyInfo(bill.ABL_ShipperRegNoInfo, true, ZString.Empty);
			AssertPropertyInfo(bill.ABL_ShipperPhoneInfo, true, orgAddress.PhoneNumber.FormattedForBinding);
			AssertEquals(orgAddress.PK, bill.ABL_OA_Shipper);
			IAddressDetails addressDetails = orgAddress;
			var billAddress = bill.ShipperABLAddress;
			AssertEquals(addressDetails.CompanyName, billAddress.CompanyName);
			AssertEquals(addressDetails.AddressLine1, billAddress.Address1);
			AssertEquals(addressDetails.AddressLine2, billAddress.Address2);
			AssertEquals(addressDetails.City, billAddress.City);
			AssertEquals(addressDetails.State, billAddress.State);
			AssertEquals(addressDetails.PostCode, billAddress.Postcode);
			AssertEquals(addressDetails.Country, billAddress.RN_NKCountryCode);
			AssertEquals(addressDetails.Phone, billAddress.Phone);

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var billReloaded = newFactory.Load<AsycudaBill>(bill.PK);
			AssertEquals(orgAddress.PK, billReloaded.ABL_OA_Shipper);
		}

		public void TestConsigneeDefaultAddress()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "BOB THE BUILDER";
			org.OH_RL_NKClosestPort = "AU";
			var mainAddress = org.MainAddress;
			mainAddress.OA_Address1 = "MAIN ADDRESS 1";
			mainAddress.OA_Address2 = "ADDRESS 2";
			mainAddress.OA_City = "CITY";
			mainAddress.OA_State = "STATE";
			mainAddress.OA_PostCode = "203023";
			mainAddress.OA_Phone = "+4234232";
			var pickupAddress = org.Addresses.AddNew(OrgAddressType.Pickup, true);
			pickupAddress.OA_Address1 = "PICKUP ADDRESS 1";
			pickupAddress.OA_Address2 = "ADDRESS 2";
			pickupAddress.OA_City = "CITY";
			pickupAddress.OA_State = "STATE";
			pickupAddress.OA_PostCode = "203023";
			pickupAddress.OA_Phone = "+4234232";
			var deliveryAddress = org.Addresses.AddNew(OrgAddressType.Delivery, true);
			deliveryAddress.OA_Address1 = "DELIVERY ADDRESS 1";
			deliveryAddress.OA_Address2 = "ADDRESS 2";
			deliveryAddress.OA_City = "CITY";
			deliveryAddress.OA_State = "STATE";
			deliveryAddress.OA_PostCode = "203023";
			deliveryAddress.OA_Phone = "+4234232";
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ConsigneeOrgPK = org.PK;
			AssertEquals("Should be the delivery", deliveryAddress.PK, bill.ABL_OA_Consignee);
		}

		public void TestConsignee()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "BOB THE BUILDER";
			org.OH_RL_NKClosestPort = "AU";
			var orgAddress = org.MainAddress;
			orgAddress.OA_Address1 = "ADDRESS 1";
			orgAddress.OA_Address2 = "ADDRESS 2";
			orgAddress.OA_City = "CITY";
			orgAddress.OA_State = "STATE";
			orgAddress.OA_PostCode = "203023";
			orgAddress.OA_Phone = "+4234232";
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_OA_Consignee = ZGuid.Empty;
			AssertEquals("bill.ABL_ConsigneeNameInfo.ReadOnly", false, bill.ABL_ConsigneeNameInfo.ReadOnly);
			bill.ABL_ConsigneeName = "BOB";
			AssertEquals("bill.ABL_ConsigneeStreet1Info.ReadOnly", false, bill.ABL_ConsigneeStreet1Info.ReadOnly);
			bill.ABL_ConsigneeStreet1 = "STREET 1";
			AssertEquals("bill.ABL_ConsigneeStreet2Info.ReadOnly", false, bill.ABL_ConsigneeStreet2Info.ReadOnly);
			bill.ABL_ConsigneeStreet2 = "STREET 2";
			AssertEquals("bill.ABL_ConsigneeCityInfo.ReadOnly", false, bill.ABL_ConsigneeCityInfo.ReadOnly);
			bill.ABL_ConsigneeCity = "CT";
			AssertEquals("bill.ABL_ConsigneeStateInfo.ReadOnly", false, bill.ABL_ConsigneeStateInfo.ReadOnly);
			bill.ABL_ConsigneeState = "ST";
			AssertEquals("bill.ABL_ConsigneePostcodeInfo.ReadOnly", false, bill.ABL_ConsigneePostcodeInfo.ReadOnly);
			bill.ABL_ConsigneePostcode = "4343";
			AssertEquals("bill.ABL_RN_NKConsigneeCountryInfo.ReadOnly", false, bill.ABL_RN_NKConsigneeCountryInfo.ReadOnly);
			bill.ABL_RN_NKConsigneeCountry = "NZ";
			AssertEquals("bill.ABL_ConsigneePhoneInfo.ReadOnly", false, bill.ABL_ConsigneePhoneInfo.ReadOnly);
			bill.ABL_ConsigneePhone = "+7554554";
			AssertEquals("bill.ABL_ConsigneeRegoNoInfo.ReadOnly", false, bill.ABL_ConsigneeRegNoInfo.ReadOnly);
			bill.ABL_ConsigneeRegNo = "123456";

			bill.ABL_OA_Consignee = orgAddress.PK;
			AssertPropertyInfo(bill.ABL_ConsigneeNameInfo, true, orgAddress.Header.OH_FullName);
			AssertPropertyInfo(bill.ABL_ConsigneeStreet1Info, true, orgAddress.OA_Address1);
			AssertPropertyInfo(bill.ABL_ConsigneeStreet2Info, true, orgAddress.OA_Address2);
			AssertPropertyInfo(bill.ABL_ConsigneeCityInfo, true, orgAddress.OA_City);
			AssertPropertyInfo(bill.ABL_ConsigneeStateInfo, true, orgAddress.OA_State);
			AssertPropertyInfo(bill.ABL_ConsigneePostcodeInfo, true, orgAddress.OA_PostCode);
			AssertPropertyInfo(bill.ABL_RN_NKConsigneeCountryInfo, true, orgAddress.OA_RN_NKCountryCode);
			AssertPropertyInfo(bill.ABL_ConsigneePhoneInfo, true, orgAddress.PhoneNumber.FormattedForBinding);
			AssertPropertyInfo(bill.ABL_ConsigneeRegNoInfo, true, ZString.Empty);
			AssertEquals(orgAddress.PK, bill.ABL_OA_Consignee);
			IAddressDetails addressDetails = orgAddress;
			var billAddress = bill.ConsigneeABLAddress;
			AssertEquals(addressDetails.CompanyName, billAddress.CompanyName);
			AssertEquals(addressDetails.AddressLine1, billAddress.Address1);
			AssertEquals(addressDetails.AddressLine2, billAddress.Address2);
			AssertEquals(addressDetails.City, billAddress.City);
			AssertEquals(addressDetails.State, billAddress.State);
			AssertEquals(addressDetails.PostCode, billAddress.Postcode);
			AssertEquals(addressDetails.Country, billAddress.RN_NKCountryCode);
			AssertEquals(addressDetails.Phone, billAddress.Phone);

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var billReloaded = newFactory.Load<AsycudaBill>(bill.PK);
			AssertEquals(orgAddress.PK, billReloaded.ABL_OA_Consignee);
		}

		public void TestNotifyPartyDefaultAddress()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "BOB THE BUILDER";
			org.OH_RL_NKClosestPort = "AU";
			var mainAddress = org.MainAddress;
			mainAddress.OA_Address1 = "MAIN ADDRESS 1";
			mainAddress.OA_Address2 = "ADDRESS 2";
			mainAddress.OA_City = "CITY";
			mainAddress.OA_State = "STATE";
			mainAddress.OA_PostCode = "203023";
			mainAddress.OA_Phone = "+4234232";
			var pickupAddress = org.Addresses.AddNew(OrgAddressType.Pickup, true);
			pickupAddress.OA_Address1 = "PICKUP ADDRESS 1";
			pickupAddress.OA_Address2 = "ADDRESS 2";
			pickupAddress.OA_City = "CITY";
			pickupAddress.OA_State = "STATE";
			pickupAddress.OA_PostCode = "203023";
			pickupAddress.OA_Phone = "+4234232";
			var deliveryAddress = org.Addresses.AddNew(OrgAddressType.Delivery, true);
			deliveryAddress.OA_Address1 = "DELIVERY ADDRESS 1";
			deliveryAddress.OA_Address2 = "ADDRESS 2";
			deliveryAddress.OA_City = "CITY";
			deliveryAddress.OA_State = "STATE";
			deliveryAddress.OA_PostCode = "203023";
			deliveryAddress.OA_Phone = "+4234232";
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.NotifyPartyOrgPK = org.PK;
			AssertEquals("Should be the main office", mainAddress.PK, bill.ABL_OA_NotifyParty);
		}

		public void TestNotifyParty()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "BOB THE BUILDER";
			org.OH_RL_NKClosestPort = "AU";
			var orgAddress = org.MainAddress;
			orgAddress.OA_Address1 = "ADDRESS 1";
			orgAddress.OA_Address2 = "ADDRESS 2";
			orgAddress.OA_City = "CITY";
			orgAddress.OA_State = "STATE";
			orgAddress.OA_PostCode = "203023";
			orgAddress.OA_Phone = "+4234232";
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_OA_NotifyParty = ZGuid.Empty;
			AssertEquals("bill.ABL_NotifyPartyNameInfo.ReadOnly", false, bill.ABL_NotifyPartyNameInfo.ReadOnly);
			bill.ABL_NotifyPartyName = "BOB";
			AssertEquals("bill.ABL_NotifyPartyStreet1Info.ReadOnly", false, bill.ABL_NotifyPartyStreet1Info.ReadOnly);
			bill.ABL_NotifyPartyStreet1 = "STREET 1";
			AssertEquals("bill.ABL_NotifyPartyStreet2Info.ReadOnly", false, bill.ABL_NotifyPartyStreet2Info.ReadOnly);
			bill.ABL_NotifyPartyStreet2 = "STREET 2";
			AssertEquals("bill.ABL_NotifyPartyCityInfo.ReadOnly", false, bill.ABL_NotifyPartyCityInfo.ReadOnly);
			bill.ABL_NotifyPartyCity = "CT";
			AssertEquals("bill.ABL_NotifyPartyStateInfo.ReadOnly", false, bill.ABL_NotifyPartyStateInfo.ReadOnly);
			bill.ABL_NotifyPartyState = "ST";
			AssertEquals("bill.ABL_NotifyPartyPostcodeInfo.ReadOnly", false, bill.ABL_NotifyPartyPostcodeInfo.ReadOnly);
			bill.ABL_NotifyPartyPostcode = "4343";
			AssertEquals("bill.ABL_RN_NKNotifyPartyCountryInfo.ReadOnly", false, bill.ABL_RN_NKNotifyPartyCountryInfo.ReadOnly);
			bill.ABL_RN_NKNotifyPartyCountry = "NZ";
			AssertEquals("bill.ABL_NotifyPartyPhoneInfo.ReadOnly", false, bill.ABL_NotifyPartyPhoneInfo.ReadOnly);
			bill.ABL_NotifyPartyPhone = "+7554554";
			AssertEquals("bill.ABL_NotifyPartyRegNoInfo.ReadOnly", false, bill.ABL_NotifyPartyRegNoInfo.ReadOnly);
			bill.ABL_NotifyPartyRegNo = "123456";

			bill.ABL_OA_NotifyParty = orgAddress.PK;
			AssertPropertyInfo(bill.ABL_NotifyPartyNameInfo, true, orgAddress.Header.OH_FullName);
			AssertPropertyInfo(bill.ABL_NotifyPartyStreet1Info, true, orgAddress.OA_Address1);
			AssertPropertyInfo(bill.ABL_NotifyPartyStreet2Info, true, orgAddress.OA_Address2);
			AssertPropertyInfo(bill.ABL_NotifyPartyCityInfo, true, orgAddress.OA_City);
			AssertPropertyInfo(bill.ABL_NotifyPartyStateInfo, true, orgAddress.OA_State);
			AssertPropertyInfo(bill.ABL_NotifyPartyPostcodeInfo, true, orgAddress.OA_PostCode);
			AssertPropertyInfo(bill.ABL_RN_NKNotifyPartyCountryInfo, true, orgAddress.OA_RN_NKCountryCode);
			AssertPropertyInfo(bill.ABL_NotifyPartyPhoneInfo, true, orgAddress.PhoneNumber.FormattedForBinding);
			AssertPropertyInfo(bill.ABL_NotifyPartyRegNoInfo, true, ZString.Empty);
			AssertEquals(orgAddress.PK, bill.ABL_OA_NotifyParty);
			IAddressDetails addressDetails = orgAddress;
			var billAddress = bill.NotifyPartyABLAddress;
			AssertEquals(addressDetails.CompanyName, billAddress.CompanyName);
			AssertEquals(addressDetails.AddressLine1, billAddress.Address1);
			AssertEquals(addressDetails.AddressLine2, billAddress.Address2);
			AssertEquals(addressDetails.City, billAddress.City);
			AssertEquals(addressDetails.State, billAddress.State);
			AssertEquals(addressDetails.PostCode, billAddress.Postcode);
			AssertEquals(addressDetails.Country, billAddress.RN_NKCountryCode);
			AssertEquals(addressDetails.Phone, billAddress.Phone);

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var billReloaded = newFactory.Load<AsycudaBill>(bill.PK);
			AssertEquals(orgAddress.PK, billReloaded.ABL_OA_NotifyParty);
		}

		public void TestSeller()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "BOB THE BUILDER";
			org.OH_RL_NKClosestPort = "AU";
			var orgAddress = org.MainAddress;
			orgAddress.OA_Address1 = "ADDRESS 1";
			orgAddress.OA_Address2 = "ADDRESS 2";
			orgAddress.OA_City = "CITY";
			orgAddress.OA_State = "STATE";
			orgAddress.OA_PostCode = "203023";
			orgAddress.OA_Phone = "+4234232";
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_OA_Seller = ZGuid.Empty;
			AssertEquals("bill.ABL_SellerNameInfo.ReadOnly", false, bill.ABL_SellerNameInfo.ReadOnly);
			bill.ABL_SellerName = "BOB";
			AssertEquals("bill.ABL_SellerStreet1Info.ReadOnly", false, bill.ABL_SellerStreet1Info.ReadOnly);
			bill.ABL_SellerStreet1 = "STREET 1";
			AssertEquals("bill.ABL_SellerStreet2Info.ReadOnly", false, bill.ABL_SellerStreet2Info.ReadOnly);
			bill.ABL_SellerStreet2 = "STREET 2";
			AssertEquals("bill.ABL_SellerCityInfo.ReadOnly", false, bill.ABL_SellerCityInfo.ReadOnly);
			bill.ABL_SellerCity = "CT";
			AssertEquals("bill.ABL_SellerStateInfo.ReadOnly", false, bill.ABL_SellerStateInfo.ReadOnly);
			bill.ABL_SellerState = "ST";
			AssertEquals("bill.ABL_SellerPostcodeInfo.ReadOnly", false, bill.ABL_SellerPostcodeInfo.ReadOnly);
			bill.ABL_SellerPostcode = "4343";
			AssertEquals("bill.ABL_RN_NKSellerCountryInfo.ReadOnly", false, bill.ABL_RN_NKSellerCountryInfo.ReadOnly);
			bill.ABL_RN_NKSellerCountry = "NZ";
			AssertEquals("bill.ABL_SellerPhoneInfo.ReadOnly", false, bill.ABL_SellerPhoneInfo.ReadOnly);
			bill.ABL_SellerPhone = "+7554554";
			AssertEquals("bill.ABL_SellerRegoNoInfo.ReadOnly", false, bill.ABL_SellerRegNoInfo.ReadOnly);
			bill.ABL_SellerRegNo = "123456";

			bill.ABL_OA_Seller = orgAddress.PK;
			AssertPropertyInfo(bill.ABL_SellerNameInfo, true, orgAddress.Header.OH_FullName);
			AssertPropertyInfo(bill.ABL_SellerStreet1Info, true, orgAddress.OA_Address1);
			AssertPropertyInfo(bill.ABL_SellerStreet2Info, true, orgAddress.OA_Address2);
			AssertPropertyInfo(bill.ABL_SellerCityInfo, true, orgAddress.OA_City);
			AssertPropertyInfo(bill.ABL_SellerStateInfo, true, orgAddress.OA_State);
			AssertPropertyInfo(bill.ABL_SellerPostcodeInfo, true, orgAddress.OA_PostCode);
			AssertPropertyInfo(bill.ABL_SellerPhoneInfo, true, orgAddress.PhoneNumber.FormattedForBinding);
			AssertPropertyInfo(bill.ABL_SellerRegNoInfo, true, ZString.Empty);
			AssertEquals(orgAddress.PK, bill.ABL_OA_Seller);
			IAddressDetails addressDetails = orgAddress;
			var billAddress = bill.SellerABLAddress;
			AssertEquals(addressDetails.CompanyName, billAddress.CompanyName);
			AssertEquals(addressDetails.AddressLine1, billAddress.Address1);
			AssertEquals(addressDetails.AddressLine2, billAddress.Address2);
			AssertEquals(addressDetails.City, billAddress.City);
			AssertEquals(addressDetails.State, billAddress.State);
			AssertEquals(addressDetails.PostCode, billAddress.Postcode);
			AssertEquals(addressDetails.Country, billAddress.RN_NKCountryCode);
			AssertEquals(addressDetails.Phone, billAddress.Phone);

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var billReloaded = newFactory.Load<AsycudaBill>(bill.PK);
			AssertEquals(orgAddress.PK, billReloaded.ABL_OA_Seller);
		}

		public void TestHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals(header, bill.Header);
			AssertEquals(header, ((IManifestBillForSynchroniser)bill).Header);
		}

		public void TestDelete()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			var pack = bill.Packs.AddNew();
			bill.Delete();
			Assert(pack.IsDeleted);
		}

		public void TestVolumeInM3()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertEquals(0m, bill.VolumeInM3);
			bill.ABL_Volume = 1000m;
			bill.ABL_VolumeUQ = Core.Constants.Volume.CubicMetres;
			AssertEquals(1000m, bill.VolumeInM3);
			bill.ABL_VolumeUQ = Core.Constants.Volume.CubicDecimetres;
			AssertEquals(1m, bill.VolumeInM3);
			bill.ABL_VolumeUQ = "1";
			AssertNoExceptionThrown(() =>
			{
				AssertEquals(0m, bill.VolumeInM3);
			});
		}

		public void TestMassInKilos()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertEquals(0m, bill.MassInKilos);
			bill.ABL_GrossWeight = 1000m;
			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals(1000m, bill.MassInKilos);
			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Grams;
			AssertEquals(1m, bill.MassInKilos);
			bill.ABL_GrossWeightUQ = "1";
			AssertNoExceptionThrown(() =>
			{
				AssertEquals(0m, bill.MassInKilos);
			});
		}

		public void TestPacks()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertEquals(0, bill.Packs.Count);
			bill.Packs.AddNew();
			AssertEquals(1, bill.Packs.Count);
			Factory.Save();
			var billReloaded = new BusinessObjectFactory().Load<AsycudaBill>(bill.PK);
			AssertEquals("should be generic type", true, billReloaded.Packs.GetType().IsGenericType);
			var genericParams = billReloaded.Packs.GetType().GetGenericArguments();
			AssertEquals("should be 2 generic arguments", 2, genericParams.Length);
			AssertEquals("first argument should be AsycudaPack", true, typeof(AsycudaPack).IsAssignableFrom(genericParams[0]));
			AssertEquals("first argument should be AsycudaBill", true, typeof(AsycudaBill).IsAssignableFrom(genericParams[1]));
			AssertEquals(1, billReloaded.Packs.Count);
		}

		public void TestBillScreenings()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertEquals(0, bill.BillScreenings.Count);
			bill.BillScreenings.AddNew();
			AssertEquals(1, bill.BillScreenings.Count);
			Factory.Save();
			var billReloaded = new BusinessObjectFactory().Load<AsycudaBill>(bill.PK);
			AssertEquals(1, billReloaded.BillScreenings.Count);
		}

		public void TestHumanReadableName()
		{
			var bill1 = (AsycudaBill)GetNewBusinessObject();
			bill1.ABL_BolType = "STD";
			bill1.ABL_BillNumber = "BILL1";
			var bill2 = (AsycudaBill)GetNewBusinessObject();
			bill2.ABL_BolType = "BOL";
			bill2.ABL_BillNumber = "MASTERBILL1";
			AssertEquals("Manifest Bill BILL1", bill1.HumanReadableName);
			AssertEquals("Manifest Master Bill MASTERBILL1", bill2.HumanReadableName);
		}

		public void TestMatchingReference()
		{
			var bill = Factory.New<AsycudaBill>();
			AssertEquals(ZString.Empty, bill.MatchingReference);
			bill.MatchingReference = "TESTREFERENCE";
			AssertEquals("TESTREFERENCE", bill.MatchingReference);
		}

		public void TestConsigneePhoneNumberLengthNotExceededFromOrganisationLink()
		{
			var orgHeaderSql = "INSERT INTO dbo.OrgHeader (OH_PK, OH_CODE) VALUES ('C870DDA5-8A8E-4260-8070-396D3433D765', 'TESTCODE')";
			Db.Connection.ExecuteScalar(orgHeaderSql);
			var orgAddressSql = @"INSERT INTO dbo.OrgAddress (OA_PK, OA_ADDRESS1, OA_PHONE, OA_RN_NKCountryCode, OA_OH)
				VALUES ('EB4F0143-A8E2-45CD-B5BA-07FD8E4EB18D', '111, NORTH BRIDGE ROAD, #02-07A,', '67883350 EXT 401', 'SG', 'C870DDA5-8A8E-4260-8070-396D3433D765')";
			Db.Connection.ExecuteScalar(orgAddressSql);

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			ZGuid addressPk;
			ZGuid.TryParse("EB4F0143-A8E2-45CD-B5BA-07FD8E4EB18D", out addressPk);
			AssertNoExceptionThrown(() => bill.ABL_OA_Consignee = addressPk);
		}

		public void TestIsAir()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			Assert(!bill.IsAir);
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			Assert(!bill.IsAir);
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			Assert(bill.IsAir);
		}

		public void TestIsSea()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			Assert(!bill.IsSea);
			header.AMA_TransportMode = Core.Constants.TransportModes.Mail;
			Assert(!bill.IsSea);
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			Assert(bill.IsSea);
		}

		public void TestGetCusCodeDataTypes()
		{
			var cusCodeDataTypeSupporter = (ICusCodeDataTypeSupporter)Factory.New<AsycudaBill>();
			AssertEquals(0, cusCodeDataTypeSupporter.GetCusCodeDataTypes().Count);
		}

		public void TestCusCodeDataSupporterFetchStrategies()
		{
			var cusCodeDataTypeSupporter = (ICusCodeDataTypeSupporter)Factory.New<AsycudaBill>();
			AssertEquals("Base has no CusCodeDatTypes so no Fetch Strategy", false, cusCodeDataTypeSupporter.GetFetchStrategies().Any());
		}

		public void TestShipperRegNo()
		{
			var factory = new BusinessObjectFactory();

			var org = factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "ER";
			org.OH_FullName = "FULL NAME";
			var orgAddress = org.MainAddress;
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.UniversalOfficeCode, "123456");
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "62318879");
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.AgentCode, "123456978");

			var org1 = factory.New<OrgHeader>();
			org1.OH_RL_NKClosestPort = "ER";
			org1.OH_FullName = "SECOND";
			var orgAddress1 = org1.MainAddress;

			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();

			var bill = Factory.New<AsycudaBillRegNoForTest>();

			header.Bills.Add(bill);

			bill.ABL_OA_Shipper = ZGuid.Empty;
			factory.Save();

			bill.ABL_OA_Shipper = orgAddress.PK;
			AssertEquals("62318879", bill.ABL_ShipperRegNo);
			AssertEquals(OrgCusCode.CodeTypes.VATCode, bill.ABL_ShipperRegNoType);

			bill.ABL_OA_Shipper = orgAddress1.PK;
			AssertEquals(ZString.Empty, bill.ABL_ShipperRegNo);
			AssertEquals(ZString.Empty, bill.ABL_ShipperRegNoType);
		}

		public void TestConsigneeNo()
		{
			var factory = new BusinessObjectFactory();

			var org = factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "ER";
			org.OH_FullName = "FULL NAME";

			var orgAddress = org.MainAddress;
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.UniversalOfficeCode, "123456");
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "62318879");
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.AgentCode, "123456978");

			var org1 = factory.New<OrgHeader>();
			org1.OH_RL_NKClosestPort = "ER";
			org1.OH_FullName = "SECOND";
			var orgAddress1 = org1.MainAddress;

			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();

			var bill = Factory.New<AsycudaBillRegNoForTest>();

			header.Bills.Add(bill);

			bill.ABL_OA_Consignee = ZGuid.Empty;
			factory.Save();
			bill.ABL_OA_Consignee = orgAddress.PK;
			AssertEquals("123456978", bill.ABL_ConsigneeRegNo);
			AssertEquals(OrgCusCode.CodeTypes.AgentCode, bill.ABL_ConsigneeRegNoType);

			bill.ABL_OA_Consignee = orgAddress1.PK;
			AssertEquals(ZString.Empty, bill.ABL_ConsigneeRegNo);
			AssertEquals(ZString.Empty, bill.ABL_ConsigneeRegNoType);
		}

		public void TestNotifyPartyNo()
		{
			var factory = new BusinessObjectFactory();

			var org = factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "ER";
			org.OH_FullName = "FULL NAME";

			var orgAddress = org.MainAddress;
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.UniversalOfficeCode, "123456");
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "62318879");
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.AgentCode, "123456978");

			var org1 = factory.New<OrgHeader>();
			org1.OH_RL_NKClosestPort = "ER";
			org1.OH_FullName = "SECOND";
			var orgAddress1 = org1.MainAddress;

			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();

			var bill = Factory.New<AsycudaBillRegNoForTest>();

			header.Bills.Add(bill);

			bill.ABL_OA_NotifyParty = ZGuid.Empty;
			factory.Save();
			bill.ABL_OA_NotifyParty = orgAddress.PK;
			AssertEquals("123456", bill.ABL_NotifyPartyRegNo);
			AssertEquals(OrgCusCode.CodeTypes.UniversalOfficeCode, bill.ABL_NotifyPartyRegNoType);

			bill.ABL_OA_NotifyParty = orgAddress1.PK;
			AssertEquals(ZString.Empty, bill.ABL_NotifyPartyRegNo);
			AssertEquals(ZString.Empty, bill.ABL_NotifyPartyRegNoType);
		}

		public void TestGetNewAsycudaBillProcessTaskCollection()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertType("WorkflowItems's type should be ProcessTaskCollection<AsycudaBillProcessTask, AsycudaBill>", typeof(ProcessTaskCollection<AsycudaBillProcessTask, AsycudaBill>), ((IWorkflowProvider)bill).WorkflowItems);
		}

		public void TestCustomsEntryNumberMaxLength()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals("The CustomsEntryNumber MaxLength default value should be CE_EntryNumMaxLength", ABLEntryNum.Schema.CE_EntryNumMaxLength, bill.CustomsEntryNumberMaxLength);
		}

		public void TestABL_BillStatusDescription()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "CustomsManifestStatus");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "8", "Proceed to Border", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "9", "Already on Customs system", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Singapore, "MGI");
			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = "8";
			AssertEquals("Proceed to Border", bill.ABL_BillStatusDescription);
			bill.ABL_BillStatus = "9";
			AssertEquals("Already on Customs system", bill.ABL_BillStatusDescription);
			bill.ABL_BillStatus = "10";
			AssertEquals(ZString.Empty, bill.ABL_BillStatusDescription);
		}

		public void TestIStatusSupporter_SupportsPackLevelMessages()
		{
			Factory.Save();

			var sgRegistry = ObjectFactory.Get<SG.ISGCustomsRegistry>();
			sgRegistry.ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var headerSG = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Singapore, "MGE");
			headerSG.FillWithValidTestData();
			var billSG = headerSG.Bills.AddNew();
			Assert(((IStatusSupporter)billSG).SupportsPackLevelMessages);

			var headerZA = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB'");
			headerZA.FillWithValidTestData();
			var billZA = headerZA.Bills.AddNew();
			Assert(!((IStatusSupporter)billZA).SupportsPackLevelMessages);

			var headerUS = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
			var billUS = headerUS.Bills.AddNew();
			Assert(!((IStatusSupporter)billUS).SupportsPackLevelMessages);
		}

		public void TestDeletionInCorrectOrder()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var bill = header.Bills.AddNew();
			bill.Packs.AddNew();
			var entryNumber = bill.CustomsEntryNumbers.AddNew();
			var registrationNumber = CusEntryNumber.New<ABLEntryNum>(bill, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.Eritrea);
			var message = bill.Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var processTask = bill.WorkflowItems.AddNew();
			Factory.Save();

			entryNumber.NotificationsChanged += (object sender, NotificationsChangedEventArgs e) =>
			{
				if (bill.IsDeleted)
				{
					Assert("This is wrong Entry Numbers should not be deleted after the Bill", false);
				}
			};
			message.NotificationsChanged += (object sender, NotificationsChangedEventArgs e) =>
			{
				if (bill.IsDeleted)
				{
					Assert("This is wrong Messages should not be deleted after the Bill", false);
				}
			};
			registrationNumber.NotificationsChanged += (object sender, NotificationsChangedEventArgs e) =>
			{
				if (bill.IsDeleted)
				{
					Assert("This is wrong Registration Number should not be deleted after the Bill", false);
				}
			};

			bill.Delete();
			AssertNoExceptionThrown(Factory.Save);
			AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIMessage)));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(CusEntryNumber)));
			AssertEquals(1, Factory.GetDatabaseCount(typeof(AsycudaBill))); //MasterBill on the header still exist.
			AssertEquals(true, processTask.IsDeleted); //Header Workflow tasks still exist.
		}

		public void TestDeleteWorkflowItems()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var bill = header.Bills.AddNew();

			var workflowItem = bill.WorkflowItems.AddNew();
			bill.Delete();

			Assert(bill.IsDeleted);
			Assert(workflowItem.IsDeleted);
		}

		public void TestRegistrationNumber_WhenEntryNumberTypeIsNotAsy()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var entryNum = bill.CustomsEntryNumbers.AddNew();
			entryNum.CE_EntryType = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
			entryNum.CE_EntryNum = "123";
			AssertEquals("123", bill.RegistrationNumber);
			entryNum.CE_EntryType = "";
			AssertEquals("", bill.RegistrationNumber);
		}

		public void TestRegistrationEntryNumberIsCorrectType()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var entryNum = bill.CustomsEntryNumbers.AddNew();
			entryNum.CE_EntryType = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
			AssertNull(((IBusiness)bill).Children.OfType<CusEntryNumber>().FirstOrDefault(x => x.PK == entryNum.PK));
			AssertEquals("", bill.RegistrationNumber);
			var registrationEntryNumber = ((IBusiness)bill).Children.OfType<CusEntryNumber>().First(x => x.PK == entryNum.PK);
			AssertEquals(entryNum, registrationEntryNumber);
			AssertEquals(entryNum, bill.RegistrationEntryNumber);
		}

		public void TestCycleDate()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var billSG = (Integration.Customs.ASYCUDA.SGAccess.IAsycudaBill)bill;
			billSG.CycleDate = new ZDateTime(2017, 9, 1);
			AssertEquals(new ZDateTime(2017, 9, 1), billSG.CycleDate);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			billSG = newFactory.Load<Integration.Customs.ASYCUDA.SGAccess.IAsycudaBill>(billSG.PK);
			AssertEquals(new ZDateTime(2017, 9, 1), billSG.CycleDate);

			billSG.CycleDate = new ZDateTime(2017, 9, 5, 14, 30, 21);
			AssertEquals(new ZDateTime(2017, 9, 5), billSG.CycleDate);

			billSG.CycleDate = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, billSG.CycleDate);

			billSG.CycleDate = ZDateTime.Invalid;
			AssertEquals(ZDateTime.Invalid, billSG.CycleDate);
		}

		public void TestCycleNumber()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var billSG = (Integration.Customs.ASYCUDA.SGAccess.IAsycudaBill)bill;
			billSG.CycleNumber = "1";
			AssertEquals("1", billSG.CycleNumber);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			billSG = newFactory.Load<Integration.Customs.ASYCUDA.SGAccess.IAsycudaBill>(billSG.PK);
			AssertEquals("1", billSG.CycleNumber);
		}

		public void TestDutyAmount_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			AssertEquals("Default to false.", false, bill.DutyAmountInfo.ReadOnly);

			header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			bill = header.Bills.AddNew();
			AssertEquals("Should be true when country is SG.", true, bill.DutyAmountInfo.ReadOnly);

			header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			bill = header.Bills.AddNew();
			AssertEquals("Should be false when country is not SG.", false, bill.DutyAmountInfo.ReadOnly);
		}

		public void TestTaxAmount_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			AssertEquals("Default to false.", false, bill.TaxAmountInfo.ReadOnly);

			header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			bill = header.Bills.AddNew();
			AssertEquals("Should be true when country is SG.", true, bill.TaxAmountInfo.ReadOnly);

			header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			bill = header.Bills.AddNew();
			AssertEquals("Should be false when country is not SG.", false, bill.TaxAmountInfo.ReadOnly);
		}

		public void TestABL_CargoStatusCaptions()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			var resData = DataBoundResourceStrings.GetDataForProperty(bill.ABL_CargoStatusInfo);
			CombineAssertions(() =>
			{
				AssertNotNull("Res string data", resData);
				AssertEquals("Caption", "Cargo Status", resData.Caption);
				AssertEquals("MediumCaption", "Status", resData.MediumCaption);
				AssertEquals("ShortCaption", "", resData.ShortCaption);
			});
		}

		public void TestCustomsEntryNumber_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			AssertEquals("Default to false.", false, bill.CustomsEntryNumberInfo.ReadOnly);
		}

		public void TestPropertyThatAffectWorkflowChanged()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			var workflowAffectedPropertyProvider = (IWorkflowAffectedPropertyProvider)bill;
			AssertContainsExactElementsInAnyOrder(new[] { header.AMA_RN_NKCountryInfo, header.AMA_ManifestTypeInfo }, workflowAffectedPropertyProvider.PropertyThatAffectWorkflowChanged);
		}

		public void TestISelectionItemMembers()
		{
			CombineAssertions(() =>
			{
				ZZDataTestHelper.SetupZZ(Factory, Core.Constants.CountryCodes.SouthAfrica);
				var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				var bill = header.Bills.AddNew();
				bill.ABL_BillNumber = "HB324";
				ISelectionItem selectionItem = bill;
				AssertEquals("Bill Number - HB324", selectionItem.SelectionDescription(false));
				AssertEquals("Bill Number - HB324 - Original", selectionItem.SelectionDescription(true));

				IStatusSupporter statusSupporter = bill;
				statusSupporter.ManifestPermitNumber = "PER32";
				AssertEquals("Bill Number - HB324 - Amendment", selectionItem.SelectionDescription(true));
			});
		}

		public void TestIsDeclarationCreationEnabled()
		{
			var helper = new ZZDataTestHelper(Factory);
			Factory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Singapore, "MGI");
				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				var bill = header.Bills.AddNew();
				bill.ABL_BillNumber = "HB324";
				AssertEquals("IsDeclarationCreationEnabled", true, bill.IsDeclarationCreationEnabled);
				bill.ABL_BillNumber = ZString.Empty;
				AssertEquals("IsDeclarationCreationEnabled", false, bill.IsDeclarationCreationEnabled);
				bill.ABL_BillNumber = "HB324";
				AssertEquals("IsDeclarationCreationEnabled", true, bill.IsDeclarationCreationEnabled);
				bill.CustomsJobNumber = "JD34332";
				AssertEquals("IsDeclarationCreationEnabled", false, bill.IsDeclarationCreationEnabled);
				bill.CustomsJobNumber = ZString.Empty;
				AssertEquals("IsDeclarationCreationEnabled", true, bill.IsDeclarationCreationEnabled);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea))
			{
				var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Eritrea, "ASY");
				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				var bill = header.Bills.AddNew();
				bill.ABL_BillNumber = "HB324";
				AssertEquals("IsDeclarationCreationEnabled", false, bill.IsDeclarationCreationEnabled);
			}
		}

		public void TestHasManifestBeenSubmittedToCustoms_WhenManifestIsNotAcceptedByCustomsAndMessageIsBlank_ExpectFalse()
		{
			var country = CreateBillForHasBeenSubmittedTests();
			AssertEquals(false, country.MessageStatusProvider.HasManifestBeenAcceptedByCustoms(country));
			AssertEquals(false, country.HasManifestBeenSubmittedToCustoms);
		}

		public void TestHasManifestBeenSubmittedToCustoms_WhenManifestIsNotAcceptedByCustomsAndMessageIsERR_ExpectFalse()
		{
			var bill = CreateBillForHasBeenSubmittedTests();
			bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Error;
			AssertEquals(false, bill.MessageStatusProvider.HasManifestBeenAcceptedByCustoms(bill));
			AssertEquals(false, bill.HasManifestBeenSubmittedToCustoms);
		}

		public void TestHasManifestBeenSubmittedToCustoms_WhenManifestIsNotAcceptedByCustomsAndMessageIsNOT_ExpectFalse()
		{
			var bill = CreateBillForHasBeenSubmittedTests();
			bill.ABL_MessageStatus = MessageStatusCodeList.Codes.NotSent;
			AssertEquals(false, bill.MessageStatusProvider.HasManifestBeenAcceptedByCustoms(bill));
			AssertEquals(false, bill.HasManifestBeenSubmittedToCustoms);
		}

		public void TestHasManifestBeenSubmittedToCustoms_WhenManifestIsNotAcceptedByCustomsAndMessageIsACP_ExpectTrue()
		{
			var beenSubmittedTests = CreateBillForHasBeenSubmittedTests();
			beenSubmittedTests.ABL_MessageStatus = MessageStatusCodeList.Codes.Accepted;
			AssertEquals(false, beenSubmittedTests.MessageStatusProvider.HasManifestBeenAcceptedByCustoms(beenSubmittedTests));
			AssertEquals(true, beenSubmittedTests.HasManifestBeenSubmittedToCustoms);
		}

		public void TestHasManifestBeenSubmittedToCustoms_WhenManifestIsNotAcceptedByCustomsAndMessageIsAWA_ExpectTrue()
		{
			var bill = CreateBillForHasBeenSubmittedTests();
			bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Awaiting;
			AssertEquals(false, bill.MessageStatusProvider.HasManifestBeenAcceptedByCustoms(bill));
			AssertEquals(true, bill.HasManifestBeenSubmittedToCustoms);
		}

		public void TestHasManifestBeenSubmittedToCustoms_WhenManifestIsNotAcceptedByCustomsAndMessageIsSNT_ExpectTrue()
		{
			var bill = CreateBillForHasBeenSubmittedTests();
			bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Sent;
			AssertEquals(false, bill.MessageStatusProvider.HasManifestBeenAcceptedByCustoms(bill));
			AssertEquals(true, bill.HasManifestBeenSubmittedToCustoms);
		}

		public void TestHasManifestBeenSubmittedToCustoms_WhenManifestIsNotAcceptedByCustomsAndMessageIsUNK_ExpectTrue()
		{
			var bill = CreateBillForHasBeenSubmittedTests();
			bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Unknown;
			AssertEquals(false, bill.MessageStatusProvider.HasManifestBeenAcceptedByCustoms(bill));
			AssertEquals(true, bill.HasManifestBeenSubmittedToCustoms);
		}

		public void TestHasManifestBeenSubmittedToCustoms_WhenManifestIsNotAcceptedByCustomsAndMessageIsUPD_ExpectTrue()
		{
			var bill = CreateBillForHasBeenSubmittedTests();
			bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Updated;
			AssertEquals(false, bill.MessageStatusProvider.HasManifestBeenAcceptedByCustoms(bill));
			AssertEquals(true, bill.HasManifestBeenSubmittedToCustoms);
		}

		public void TestIsExport_WhenShipmentTypeIsBlank_ExpectFalse()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals(false, bill.IsExport);
		}

		public void TestIsExport_WhenShipmentTypeIsExp_ExpectTrue()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Export22;
			AssertEquals(true, bill.IsExport);
		}

		public void TestIStatusSupporterMembers()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Singapore, "MGI");
			var bill = header.Bills.AddNew();
			IStatusSupporter supporter = bill;
			supporter.CustomsStatus = "C1";
			supporter.MessageStatus = "M1";
			AssertEquals("country.ABL_BillStatus", "C1", bill.ABL_BillStatus);
			AssertEquals("country.ABL_MessageStatus", "M1", bill.ABL_MessageStatus);
			supporter.CustomsStatus = "C2";
			supporter.MessageStatus = "M3";
			AssertEquals("country.ABL_BillStatus", "C2", bill.ABL_BillStatus);
			AssertEquals("country.ABL_MessageStatus", "M3", bill.ABL_MessageStatus);
		}

		public void TestIsIssuerCodeMandatory()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "Manifest Validation");

			var validationRuleZa = helper.CreateNewOrGetExistingCusCodeList("ZA", RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.BillIssuer, "A Bill issuer is required", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(validationRuleZa.PK, "MANDATORYFORMESSAGETYPE", "FWB");
			helper.CreateNewOrGetExistingCusCodeListAttribute(validationRuleZa.PK, "MANDATORYFORMANIFESTTYPE", "COH");
			helper.CreateTransportModeForCusCodeList(validationRuleZa.PK, "AIR");

			Factory.Save();

			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			var bill = header.Bills.AddNew();

			AssertEquals(false, bill.IsIssuerCodeMandatory);
			header.AMA_TransportMode = "AIR";
			AssertEquals(false, bill.IsIssuerCodeMandatory);
			header.AMA_TransportMode = "SEA";
			header.AMA_ManifestType = nameof(Universal.Messaging.CUSCAR.ManifestDocumentType.FWB);
			AssertEquals(true, bill.IsIssuerCodeMandatory);
			header.AMA_ManifestType = nameof(Universal.Messaging.CUSCAR.ManifestDocumentType.COH);
			AssertEquals(true, bill.IsIssuerCodeMandatory);
		}

		public void TestDeleteCustomsNumbersNoLongerApplicable()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			bill.Header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			bill.CustomsEntryNumber = "LRN1234";
			var entryNumToRemove = bill.CustomsEntryNumbers.AddNew();
			AssertEquals(2, bill.CustomsEntryNumbers.Count);

			Factory.Save();

			AssertEquals(1, bill.CustomsEntryNumbers.Count);
			AssertEquals(bill.CustomsEntryNumber, bill.CustomsEntryNumbers[0].CE_EntryNum);

			bill.CustomsEntryNumber = "";
			bill.CustomsEntryNumberType = "TST";
			var cusEntryNumber = bill.CusEntryNumber;
			Assert(!cusEntryNumber.IsDeleted);
			Factory.Save();
			Assert(cusEntryNumber.IsDeleted);
			AssertEquals(0, bill.CustomsEntryNumbers.Count);

			var entryNum = bill.CustomsEntryNumbers.AddNew();
			entryNum.CE_EntryLineReference = "1234";
			Factory.Save();
			Assert(!entryNum.IsDeleted);
			AssertEquals(1, bill.CustomsEntryNumbers.Count);
		}

		public void TestDeleteCustomsNumbersNoLongerApplicable_ShouldNotWorkedkWhenSupportMultipleCustomsNumbers()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.CustomsEntryNumber = "TRY1234";
			var entryNum2 = bill.CustomsEntryNumbers.AddNew();
			entryNum2.CE_EntryNum = "TRY1235";
			AssertEquals(2, bill.CustomsEntryNumbers.Count);
			var entryNum3 = bill.CustomsEntryNumbers.AddNew();
			entryNum3.CE_EntryNum = ZString.Empty;

			var prevTotalofEntryNumbers = bill.CustomsEntryNumbers.Count;

			Factory.Save();

			AssertEquals(3, prevTotalofEntryNumbers);
			AssertEquals(2, bill.CustomsEntryNumbers.Count);
			AssertEquals(bill.CustomsEntryNumber, bill.CustomsEntryNumbers[0].CE_EntryNum);
			AssertEquals(entryNum2.CE_EntryNum, bill.CustomsEntryNumbers[1].CE_EntryNum);
		}

		public void TestDeleteCustomsNumbersNoLongerApplicable_ShouldNotWorkWhenSupportMultipleCustomsNumbers_ZA_Road()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			header.AMA_ManifestType = "RFM";
			var bill = header.Bills.AddNew();
			bill.CustomsEntryNumber = "LRN1234";
			var entryNum2 = bill.CustomsEntryNumbers.AddNew();
			entryNum2.CE_EntryNum = "LRN4567";
			AssertEquals(2, bill.CustomsEntryNumbers.Count);

			Factory.Save();

			AssertEquals(2, bill.CustomsEntryNumbers.Count);
			AssertEquals(bill.CustomsEntryNumber, bill.CustomsEntryNumbers[0].CE_EntryNum);
			AssertEquals(entryNum2.CE_EntryNum, bill.CustomsEntryNumbers[1].CE_EntryNum);
		}

		public void TestDeleteCustomsNumbersNoLongerApplicable_ShouldNotWorkWhenSupportMultipleCustomsNumbers_ZA_AQM()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.AQM);
			var bill = header.Bills.AddNew();
			bill.CustomsEntryNumber = "LRN1234";
			var entryNum2 = bill.CustomsEntryNumbers.AddNew();
			entryNum2.CE_EntryNum = "LRN4567";
			AssertEquals(2, bill.CustomsEntryNumbers.Count);

			Factory.Save();

			AssertEquals(2, bill.CustomsEntryNumbers.Count);
			AssertEquals(bill.CustomsEntryNumber, bill.CustomsEntryNumbers[0].CE_EntryNum);
			AssertEquals(entryNum2.CE_EntryNum, bill.CustomsEntryNumbers[1].CE_EntryNum);
		}

		public void TestLogMessageStatusChangeEventOnParent()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.AQM);
			var bill = header.Bills.AddNew();
			bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Sent;

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageStatusChangeCode);
			logQuery.AddToFilter(StmALogSchema.SL_Reference, MessageStatusCodeList.Codes.Sent);

			Assert(bill.Logs.HasLogWith(logQuery));
		}

		public void TestLogCustomsManifestStatusEventOnParent()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.AQM);
			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = "8";

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsManifestStatusCode);
			logQuery.AddToFilter(StmALogSchema.SL_Reference, "8");

			Assert(bill.Logs.HasLogWith(logQuery));
		}

		public void TestBillIssuer()
		{
			var carrier = Factory.New<ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "DJC";
			carrier.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Ethiopia;
			carrier.ZZ4_Description = "Daniel";
			Factory.Save();
			var abl = (AsycudaBill)GetNewBusinessObject();
			abl.ABL_BillIssuer = "DJC";
			AssertEquals("", abl.BillIssuerName);
			AssertEquals(nameof(FieldType.Text), abl.BillIssuerNameFieldType);
			abl.Header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Ethiopia;
			AssertEquals("Daniel", abl.BillIssuerName);
			AssertEquals(nameof(FieldType.TextCodeFindBox), abl.BillIssuerNameFieldType);
		}

		public void TestBillIssuerName()
		{
			var carrier = Factory.New<ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "111";
			carrier.ZZ4_CountryOrGrouping = "XX";
			carrier.ZZ4_Description = "One";
			Factory.Save();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "XX";
			var bill = header.Bills.AddNew();
			AssertEquals("", bill.BillIssuerName);
			bill.ABL_BillIssuer = carrier.ZZ4_Code;
			AssertEquals("One", bill.BillIssuerName);
			header.AMA_RN_NKCountry = "YY";
			AssertEquals("", bill.BillIssuerName);
		}

		public void TestCustomsEntryNumberEverything()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ethiopia);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsEntryNumberTypes, "Customs Entry Number");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Ethiopia, "CEN", "ABC", "Lame", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertEquals(0, bill.Lookups.CustomsEntryNumberTypes.Count);
			AssertEquals(typeof(UntranslatableCodeDescriptionPairList), bill.Lookups.CustomsEntryNumberTypes.GetType());
			bill.Header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Ethiopia;
			AssertEquals(1, bill.Lookups.CustomsEntryNumberTypes.Count);
			AssertEquals("ABC", bill.CustomsEntryNumberType);

			helper.CreateCusCodeList("ET", "CEN", "XYZ", "Lame", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			bill.CustomsEntryNumberType = "";
			Factory.Save();

			bill = new BusinessObjectFactory().Load<AsycudaBill>(bill.PK);  // avoid factory cache
			bill.Header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Ethiopia;
			AssertEquals(2, bill.Lookups.CustomsEntryNumberTypes.Count);
			AssertEquals("", bill.CustomsEntryNumberType);

			AssertNoMessageErrors(bill.CustomsEntryNumberTypeInfo);
			bill.CustomsEntryNumber = "Anything";
			bill.Validation.ValidateCustomsEntryNumberType();
			AssertHasMessageErrorContaining(bill.CustomsEntryNumberTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(bill.CustomsEntryNumberTypeInfo, ListValidation.InvalidCodeMessageError);
			bill.CustomsEntryNumberType = "ABC";
			AssertNoMessageErrors(bill.CustomsEntryNumberTypeInfo);
			bill.CustomsEntryNumberType = "DEF";
			AssertNoMessageErrorContaining(bill.CustomsEntryNumberTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(bill.CustomsEntryNumberTypeInfo, ListValidation.InvalidCodeMessageError);
			bill.Header.AMA_RN_NKCountry = "00";
			bill.CustomsEntryNumberType = "GHI";
			AssertHasMessageErrorContaining(bill.CustomsEntryNumberTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestRegistrationDate()
		{
			var date = ZDateTime.Today;
			var abc = (AsycudaBill)GetNewBusinessObject();
			AssertEquals(ZDateTime.Empty, abc.RegistrationDate);
			abc.RegistrationDate = date;
			AssertEquals(date, abc.RegistrationDate);
			Factory.Save();

			abc = new BusinessObjectFactory().Load<AsycudaBill>(abc.PK);
			AssertEquals(date, abc.RegistrationDate);
		}

		public void TestDefaultSGDataIfNeeded()
		{
			var sgRegistry = ObjectFactory.Get<SG.ISGCustomsRegistry>();
			sgRegistry.ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var helper = new ZZDataTestHelper(Factory);
			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			shipper.OH_Code = "SUP3234";
			shipper.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "AUUEN00001", Core.Constants.CountryCodes.Australia);
			shipper.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "SGUEN00001", Core.Constants.CountryCodes.Singapore);
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "OWN3234";
			consignee.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "AUUEN00002", Core.Constants.CountryCodes.Australia);
			consignee.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "SGUEN00002", Core.Constants.CountryCodes.Singapore);
			consignee.MiscServ.OM_IMPaymentMethod = SGPayeeIndicatorList.Codes.Q;
			Factory.Save();

			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "SGSIN";
			header.AMA_RL_NKPortOfDischarge = "AUSYD";
			var bill = header.Bills.AddNew();
			bill.ABL_OA_Shipper = shipper.MainAddress.PK;
			bill.ABL_OA_Consignee = consignee.MainAddress.PK;
			bill.ABL_RL_NKOrigin = "SGSIN";
			bill.ABL_RL_NKFinalDestination = "AUSYD";

			var billSG = (Integration.Customs.ASYCUDA.SGAccess.IAsycudaBill)bill;
			billSG.SG_PartyStatus = "A";
			billSG.SG_PartyID = ZString.Empty;
			billSG.SG_PayeeIndicator = ZString.Empty;
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Transhipment28;
			AssertSGData(billSG, ZString.Empty, ZString.Empty, ZString.Empty);
			var billCountryBizObj = (BusinessObject)billSG;
			var partyIDInfo = billCountryBizObj.ZPropertyInfoHash.GetPropertySafe("SG_PartyID");
			var partyStatusInfo = billCountryBizObj.ZPropertyInfoHash.GetPropertySafe("SG_PartyStatus");
			var payeeIndicatorInfo = billCountryBizObj.ZPropertyInfoHash.GetPropertySafe("SG_PayeeIndicator");
			AssertEquals("billSG.SG_PartyIDInfo.ReadOnly", true, partyIDInfo.ReadOnly);
			AssertEquals("billSG.SG_PartyStatusInfo.ReadOnly", true, partyStatusInfo.ReadOnly);
			AssertEquals("billSG.SG_PayeeIndicatorInfo.ReadOnly", true, payeeIndicatorInfo.ReadOnly);

			billSG.SG_PartyStatus = "A";
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;
			AssertSGData(billSG, "SGUEN00002", "A", SGPayeeIndicatorList.Codes.Q);
			AssertEquals("billSG.SG_PartyIDInfo.ReadOnly", false, partyIDInfo.ReadOnly);
			AssertEquals("billSG.SG_PartyStatusInfo.ReadOnly", false, partyStatusInfo.ReadOnly);
			AssertEquals("billSG.SG_PayeeIndicatorInfo.ReadOnly", false, payeeIndicatorInfo.ReadOnly);

			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Export22;
			AssertSGData(billSG, "SGUEN00001", ZString.Empty, ZString.Empty);
			AssertEquals("billSG.SG_PartyIDInfo.ReadOnly", false, partyIDInfo.ReadOnly);
			AssertEquals("billSG.SG_PartyStatusInfo.ReadOnly", true, partyStatusInfo.ReadOnly);
			AssertEquals("billSG.SG_PayeeIndicatorInfo.ReadOnly", true, payeeIndicatorInfo.ReadOnly);
		}

		public void TestStatusDescription()
		{
			CombineAssertions(() =>
			{
				AssertStatusDescription(MessageStatusCodeList.Codes.NotSent, MessageStatusCodeList.Descriptions.NotSent);
				AssertStatusDescription(MessageStatusCodeList.Codes.Unknown, MessageStatusCodeList.Descriptions.Unknown);
				AssertStatusDescription(MessageStatusCodeList.Codes.Registered, MessageStatusCodeList.Descriptions.Registered);
				AssertStatusDescription(MessageStatusCodeList.Codes.Sent, MessageStatusCodeList.Descriptions.Sent);
				AssertStatusDescription(MessageStatusCodeList.Codes.Awaiting, MessageStatusCodeList.Descriptions.Awaiting);
				AssertStatusDescription(MessageStatusCodeList.Codes.Updated, MessageStatusCodeList.Descriptions.Updated);
				AssertStatusDescription(MessageStatusCodeList.Codes.Accepted, MessageStatusCodeList.Descriptions.Accepted);
				AssertStatusDescription(MessageStatusCodeList.Codes.Error, "ERR - R01 COUNTRY CODE/ORIGIN OF GOODS IS INVALID");
			});

			var bill = (AsycudaBill)GetNewBusinessObject();
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(bill.StatusDescriptionInfo);
			AssertEquals("Caption", "Message Status Desc.", resourceStringData.Caption);
		}

		public void TestDefaultSG_PayeeIndicatorWhenCompanyDataNotExist()
		{
			var sgRegistry = ObjectFactory.Get<SG.ISGCustomsRegistry>();
			sgRegistry.ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CON3234";
			Factory.Save();

			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "SGSIN";
			header.AMA_RL_NKPortOfDischarge = "AUSYD";
			var bill = header.Bills.AddNew();
			var billSG = (Integration.Customs.ASYCUDA.SGAccess.IAsycudaBill)bill;
			billSG.SG_PartyStatus = "A";
			billSG.SG_PartyID = ZString.Empty;
			billSG.SG_PayeeIndicator = ZString.Empty;
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;

			bill.ABL_OA_Consignee = consignee.MainAddress.PK;

			var newFactory = new BusinessObjectFactory();
			var consigneeInNewFactory = newFactory.Load<OrgHeader>(consignee.PK);
			consigneeInNewFactory.MainAddress.Address1 = "test";
			newFactory.Save();
			bool saveFailed = false;
			try
			{
				Factory.Save();
			}
			catch
			{
				saveFailed = true;
			}
			Assert("manifest is saved", !saveFailed);

			var consignee2 = Factory.New<OrgHeader>();
			consignee2.OH_Code = "CON2222";
			consignee2.CompanyData.OB_AREftCustomsPaymentMethod = SGPayeeIndicatorList.Codes.Q;
			bill.ABL_OA_Consignee = consignee2.MainAddress.PK;
			Factory.Save();
			((BusinessObject)billSG).Reload();
			AssertEquals("SG_PayeeIndicator is defaulted", SGPayeeIndicatorList.Codes.Q, billSG.SG_PayeeIndicator);
		}

		public void TestSGPartyStatusUpdatesPackGoodsType()
		{
			var sgRegistry = ObjectFactory.Get<SG.ISGCustomsRegistry>();
			sgRegistry.ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Factory.Save();

			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "AUSYD";
			header.AMA_RL_NKPortOfDischarge = "SGSIN";
			header.AMA_ManifestType = "MGI";

			var bill = header.Bills.AddNew();
			bill.ABL_RL_NKOrigin = "AUSYD";
			bill.ABL_RL_NKFinalDestination = "SGSIN";
			var billSG = (Integration.Customs.ASYCUDA.SGAccess.IAsycudaBill)bill;

			var pack = bill.Packs.AddNew();
			var packedItem = pack.GetSGPackedItemForTesting();
			packedItem.GoodsType = "NT";

			var pack2 = bill.Packs.AddNew();
			var packedItem2 = pack2.GetSGPackedItemForTesting();
			packedItem2.GoodsType = "NT";

			billSG.SG_PartyStatus = "Y";
			AssertEquals("ME", packedItem.GoodsType);
			AssertEquals("ME", packedItem2.GoodsType);
			billSG.SG_PartyStatus = "A";
			AssertEquals("NT", packedItem.GoodsType);
			AssertEquals("NT", packedItem2.GoodsType);

			header.AMA_ManifestType = "MGE";
			billSG.SG_PartyStatus = "Y";
			AssertEquals("NT", packedItem.GoodsType);
			AssertEquals("NT", packedItem2.GoodsType);
		}

		public void TestTotalPacksGrossWeight()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;

			var pack = bill.Packs.AddNew();
			pack.APA_Weight = 4;
			pack.APA_WeightUQ = Core.Constants.Weight.Kilograms;

			pack = bill.Packs.AddNew();
			pack.APA_Weight = 3000;
			pack.APA_WeightUQ = Core.Constants.Weight.Grams;

			AssertEquals("TotalPacksGrossWeight is equal to 7 KG", 7m, bill.TotalPacksGrossWeight);
		}

		public void TestTotalPacksVolume()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			bill.ABL_VolumeUQ = Core.Constants.Volume.MegaLitre;

			var pack = bill.Packs.AddNew();
			pack.APA_Volume = 5000000;
			pack.APA_VolumeUQ = Core.Constants.Volume.Litre;

			pack = bill.Packs.AddNew();
			pack.APA_Volume = 1;
			pack.APA_VolumeUQ = Core.Constants.Volume.MegaLitre;

			AssertEquals("TotalPacksVolume is equal to 6 ML", 6m, bill.TotalPacksVolume);
		}

		public void TestDataGrouping()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var bill = header.Bills.AddNew();
			AssertEquals("DataGrouping", "ER", bill.DataGrouping);
		}

		public void TestABL_OA_Buyer_ZAddress()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals("DefaultAddressType", ZArchitecture.Business.AddressType.DLV, bill.ABL_OA_Buyer_ZAddress.DefaultAddressType);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			return header.Bills.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			ZZDataTestHelper.SetupZZ(Factory, Core.Constants.CountryCodes.SouthAfrica);
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			ZZDataTestHelper.SetupZZ(Factory, Core.Constants.CountryCodes.SouthAfrica);
			var obj = (AsycudaBill)base.GetNewBusinessObjectForSettingValueCallsRefreshBindingTest();
			obj.ABL_BillStatus = "8";
			return obj;
		}

		void AssertPropertyInfo(ZPropertyInfo info, bool isReadOnly, IZType value)
		{
			var name = info.Name;
			AssertEquals(name + ".ReadOnly", isReadOnly, info.ReadOnly);
			AssertEquals(name + ".Value", value, info.Value);
		}

		void AssertApportionmentDirty(string valuePropertyName, string currencyPropertyName)
		{
			var bill = Factory.New<AsycudaBill>();
			AssertEquals("Default to false.", false, bill.ApportionmentDirty);

			bill[valuePropertyName] = 0m;
			AssertEquals(false, bill.ApportionmentDirty);

			bill[valuePropertyName] = 150m;
			AssertEquals(true, bill.ApportionmentDirty);

			bill.ApportionmentDirty = false;

			bill[currencyPropertyName] = ZString.Empty;
			AssertEquals(false, bill.ApportionmentDirty);

			bill[currencyPropertyName] = Core.Constants.CurrencyCodes.UnitedStates;
			AssertEquals(true, bill.ApportionmentDirty);
		}

		void TestShouldBeReadOnly_Bill(bool expectedReadOnly, ZString messageStatus, string shipmentType = null)
		{
			Factory.Save();

			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.FillWithValidTestData();
			header.AMA_TransportMode = "AIR";
			header.AMA_ManifestType = "MGI";

			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			if (shipmentType != null)
			{
				bill.ABL_ShipmentType = shipmentType;
			}

			var packedItem = pack.GetSGPackedItemForTesting();
			if (!messageStatus.IsEmpty)
			{
				packedItem.API_MessageStatus = messageStatus;
			}

			AssertEquals(expectedReadOnly, bill.ShouldBeReadOnly);
		}

		AsycudaBill CreateBillForHasBeenSubmittedTests()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var bill = header.Bills.AddNew();
			return bill;
		}

		void AssertSGData(Integration.Customs.ASYCUDA.SGAccess.IAsycudaBill billSG, ZString partyID, ZString partyStatus, ZString payeeIndicator)
		{
			AssertEquals("billSG.SG_PartyID", partyID, billSG.SG_PartyID);
			AssertEquals("billSG.SG_PartyStatus", partyStatus, billSG.SG_PartyStatus);
			AssertEquals("billSG.SG_PayeeIndicator", payeeIndicator, billSG.SG_PayeeIndicator);
		}

		void AssertStatusDescription(ZString statusCode, ZString expectedResult)
		{
			var testItem = Factory.NewWithValidTestData<AsycudaBill>();
			testItem.Logs.AddNew(Events.MessageStatusChange, "ERR - NOT THE MOST RECENT ERROR", new ZDateTimeOffset(2018, 3, 11));
			testItem.Logs.AddNew(Events.MessageStatusChange, "ERR - R01 COUNTRY CODE/ORIGIN OF GOODS IS INVALID", new ZDateTimeOffset(2018, 3, 12));
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			testItem.Logs.AddNew(Events.EditedARecord, "Edited", new ZDateTimeOffset(2018, 3, 13));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			testItem.ABL_MessageStatus = statusCode;
			AssertEquals("Status for code " + statusCode, expectedResult, testItem.StatusDescription);
		}

		sealed class AsycudaBillRegNoForTest : AsycudaBill
		{
			public AsycudaBillRegNoForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			public override ZString[] ShipperRegNoTypes() => new ZString[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.UniversalOfficeCode, OrgCusCode.CodeTypes.AgentCode };

			public override ZString[] ConsigneeRegNoTypes() => new ZString[] { OrgCusCode.CodeTypes.AgentCode, OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.UniversalOfficeCode };

			public override ZString[] NotifyPartyRegNoTypes() => new ZString[] { OrgCusCode.CodeTypes.UniversalOfficeCode, OrgCusCode.CodeTypes.AgentCode, OrgCusCode.CodeTypes.VATCode, };
		}
	}
}
