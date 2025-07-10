using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.JP.MessageDefinitions.Outbound;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(ManifestHeaderMesssageProvider))]
	public class ManifestHeaderMesssageProviderTest : TestCaseWithFactory
	{
		public void TestHCH01()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";

			var bthPassword = Factory.NewWithValidTestData<GlbExternalPasswordCUS>();
			bthPassword.GP_GS = staff.PK;
			bthPassword.GP_MailBoxID = "12345";
			bthPassword.GP_UserID = "TEST2";
			bthPassword.CurrentDecryptedPassword = "TESTPASS";

			var consolidator = Factory.New<OrgHeader>();
			consolidator.FillWithValidTestData();

			var contractorAddress = consolidator.Addresses.AddNew();
			contractorAddress.OA_Code = "Code1";
			contractorAddress.OA_Address1 = "OA_Address1";
			contractorAddress.OA_City = "Tokyo";
			contractorAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);

			var customsCode = contractorAddress.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Japan;
			customsCode.OK_CodeType = OrgCusCode.JapanCodeTypes.NUC;
			customsCode.OK_CustomsRegNo = "12345";
			Header.AMA_OA_Consolidator = contractorAddress.PK;

			Header.AMA_CustomsOffice = "TY";
			Header.AMA_MasterBill = "MasterBill";
			Header.AMA_AgentType = "H";
			Header.AMA_Voyage = "INAKA";
			Header.MasterBill.ABL_E_DEP = new ZDateTime(2024, 6, 1);

			var unloco1 = Factory.New<RefUNLOCO>();
			unloco1.RL_Code = "JP001";
			unloco1.RL_IATA = "TKU";
			var unloco2 = Factory.New<RefUNLOCO>();
			unloco2.RL_Code = "JP002";
			unloco2.RL_IATA = "KYU";
			Header.AMA_RL_NKPortOfFirstArrival = "JP001";
			Header.AMA_RL_NKPortOfLoading = "JP002";
			Header.PortOfDischargeIATACode = "NRT";

			Bill.ABL_BillNumber = "11111";
			Bill.ABL_ManifestQty = 111;
			Bill.ABL_GrossWeight = 123456.890;
			Bill.ABL_GrossWeightUQ = Core.Constants.Weight.Pounds;
			Bill.ABL_GoodsDescription = "KURUMA";
			Bill.ABL_SpecialCargoCode = "CAC";
			Bill.ABL_RL_NKFinalDestination = "JP001";
			Bill.ABL_GoodsLocation = "Osaka";

			Bill.ABL_ShipperName = "ABC";
			Bill.ABL_ShipperStreet1 = "KOBAYASHI";
			Bill.ABL_ShipperStreet2 = "KOKODAYO";
			Bill.ABL_ShipperCity = "Fukuoka";
			Bill.ABL_ShipperState = "Kyushu";
			Bill.ABL_ShipperPostcode = "215600";
			Bill.ABL_RN_NKShipperCountry = "CN";
			Bill.ABL_ShipperPhone = "08035288888";

			Bill.ABL_ConsigneeRegNo = "13579";
			Bill.ABL_ConsigneeName = "GINNZA";
			Bill.ABL_ConsigneePostcode = "365000";
			Bill.ABL_ConsigneeState = "ABCDE";
			Bill.ABL_ConsigneeCity = "SAPO";
			Bill.ABL_ConsigneeStreet1 = "ConsigneeStreet1";
			Bill.ABL_ConsigneeStreet2 = "ConsigneeStreet2";
			Bill.ABL_ConsigneePhone = "08035286666";

			Factory.Save();

			CombineAssertions(() =>
			{
				using (Header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.HDF01 }))
				{
					var sendingObjectParent = new ManifestMessageSendingObjectParent(Header);
					var sendingObject = sendingObjectParent.SendingObjectsCollection.FirstOrDefault() as ManifestMessageSendingObject;
					sendingObject.ShouldSend = true;
					var provider = new ManifestHeaderMesssageProvider(new List<ManifestMessageSendingObject>() { sendingObject });

					AssertEquals("ConsolidatorCode", customsCode.OK_CustomsRegNo, provider.ConsolidatorCode);
					AssertEquals("CustomsOffice", Header.AMA_CustomsOffice, provider.CustomsOffice);
					AssertEquals("MAWB", Header.AMA_MasterBill, provider.MAWB);
					AssertEquals("IsSubConsolidation", "Y", provider.IsSubConsolidation);
					AssertEquals("ArrivalFlightNumber", Header.AMA_Voyage, provider.ArrivalFlightNumber);
					AssertEquals("ArrivalFlightDate", "01JUN", provider.ArrivalFlightDate);
					AssertEquals("PortOfDischarge", "NRT", provider.PortOfDischarge);
					AssertEquals("PortOfLoading", "KYU", provider.PortOfLoading);

					var bills = provider.Bills;
					AssertEquals("Bills count", 1, bills.Count());
					var bill = bills.First();
					AssertEquals("HAWB", Bill.ABL_BillNumber, bill.HAWB);
					AssertEquals("TotalCount", Bill.ABL_ManifestQty, bill.TotalCount);
					AssertEquals("TotalWeight.Quantity", Bill.CustomsWeight, bill.TotalWeight.Quantity);
					AssertEquals("TotalWeight.Unit", Bill.CustomsWeightUQ, bill.TotalWeight.Unit);
					AssertEquals("GoodsDescription", Bill.ABL_GoodsDescription, bill.GoodsDescription);
					AssertEquals("SpecialCargoCode", Bill.ABL_SpecialCargoCode, bill.SpecialCargoCode);
					AssertEquals("FinalDestination", "TKU", bill.FinalDestination);
					AssertEquals("GoodsLocation", Bill.ABL_GoodsLocation, bill.GoodsLocation);

					AssertEquals("Shipper.Name", Bill.ABL_ShipperName, bill.Shipper.Name);
					AssertEquals("Shipper.Address", Bill.ShipperAddress, bill.Shipper.Address);
					AssertEquals("Shipper.Phone", Bill.ABL_ShipperPhone, bill.Shipper.Phone);

					AssertEquals("Consignee.Code", Bill.ABL_ConsigneeRegNo, bill.Consignee.Code);
					AssertEquals("Consignee.Name", Bill.ABL_ConsigneeName, bill.Consignee.Name);
					AssertEquals("Consignee.Address", Bill.ConsigneeAddress, bill.Consignee.Address);
					AssertEquals("Consignee.Phone", Bill.ABL_ConsigneePhone, bill.Consignee.Phone);

					header.AMA_GS_NKCustomsAgent = staff.GS_Code;
					header.AMA_CustomsAgentCredentialPK = bthPassword.PK;
					sendingObjectParent = new ManifestMessageSendingObjectParent(Header);
					sendingObject = sendingObjectParent.SendingObjectsCollection.FirstOrDefault() as ManifestMessageSendingObject;
					sendingObject.ShouldSend = true;
					provider = new ManifestHeaderMesssageProvider(new List<ManifestMessageSendingObject>() { sendingObject });
					AssertNull("ConsolidatorCode when mailBoxID is same with NUC code", provider.ConsolidatorCode);
				}
			});
		}

		public void TestHDF01()
		{
			var unloco1 = Factory.New<RefUNLOCO>();
			unloco1.RL_Code = "JP001";
			unloco1.RL_IATA = "TKU";
			var unloco2 = Factory.New<RefUNLOCO>();
			unloco2.RL_Code = "JP002";
			unloco2.RL_IATA = "KYU";

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "CarrierCode";
			var carrierAddress = carrier.Addresses.AddNew();
			carrierAddress.OA_Address1 = "123";

			var airLine = Factory.New<RefAirline>();
			airLine.RM_TwoCharacterCode = "WW";

			var masterBill = Header.MasterBill;
			Header.AMA_RL_NKPortOfDischarge = "JP001";
			masterBill.ABL_BillNumber = "001";
			Header.AMA_RL_NKPortOfLoading = "JP002";
			masterBill.ABL_IsCoLoaded = true;
			Header.AMA_OA_Carrier = carrierAddress.PK;
			Header.AMA_Nature = JPJobMessageTypeList.Codes.Export;
			Header.Carrier.Header.MiscServ.OM_RM_Airline = airLine.PK;

			Bill.ABL_BillNumber = "002";
			Bill.ABL_RL_NKFinalDestination = "JP002";
			Bill.ABL_ManifestQty = 3;
			Bill.ABL_GrossWeight = 123456.890;
			Bill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			Bill.ABL_GoodsDescription = "Test Description";
			Bill.ABL_CargoType = "C";

			Factory.Save();

			CombineAssertions(() =>
			{
				using (Header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.HDF01 }))
				{
					var sendingObjectParent = new ManifestMessageSendingObjectParent(Header);
					var sendingObject = sendingObjectParent.SendingObjectsCollection.FirstOrDefault() as ManifestMessageSendingObject;
					sendingObject.ShouldSend = true;
					sendingObject.Action = "C";
					var provider = new ManifestHeaderMesssageProvider(new List<ManifestMessageSendingObject>() { sendingObject });

					AssertEquals("MAWBDestination", unloco1.RL_IATA, provider.PortOfDischarge);
					AssertEquals("MAWBNumber", masterBill.ABL_BillNumber, provider.MAWB);
					AssertEquals("PortOfLoading", unloco2.RL_IATA, provider.PortOfLoading);
					AssertEquals("IsCoLoaded", "Y", provider.IsCoLoaded);

					AssertEquals("Bills count", 1, provider.Bills.Count());
					var billProvider = (provider as IHDF01).Bills.FirstOrDefault();
					AssertEquals("Action", "C", billProvider.Action);
					AssertEquals("HAWB", Bill.ABL_BillNumber, billProvider.HAWB);
					AssertEquals("FinalDestination", unloco2.RL_IATA, billProvider.FinalDestination);
					AssertEquals("TotalCount", Bill.ABL_ManifestQty, billProvider.TotalCount);
					AssertEquals("Weight", Bill.CustomsWeight, billProvider.Weight);
					AssertEquals("GoodsDescription", Bill.ABL_GoodsDescription, billProvider.GoodsDescription);
					AssertEquals("CargoType", Bill.ABL_CargoType, billProvider.CargoType);
				}
			});
		}

		public void TestNVC01()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";

			var bthPassword = Factory.NewWithValidTestData<GlbExternalPasswordCUS>();
			bthPassword.GP_GS = staff.PK;
			bthPassword.GP_MailBoxID = "12345";
			bthPassword.GP_UserID = "TEST2";
			bthPassword.CurrentDecryptedPassword = "TESTPASS";

			var consolidator = Factory.New<OrgHeader>();
			consolidator.FillWithValidTestData();

			var contractorAddress = consolidator.Addresses.AddNew();
			contractorAddress.OA_Code = "Code1";
			contractorAddress.OA_Address1 = "OA_Address1";
			contractorAddress.OA_City = "Tokyo";
			contractorAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);

			var customsCode = contractorAddress.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Japan;
			customsCode.OK_CodeType = OrgCusCode.JapanCodeTypes.NUC;
			customsCode.OK_CustomsRegNo = "12345";
			Header.AMA_OA_Consolidator = contractorAddress.PK;

			Header.AMA_CustomsOffice = "TY";
			Header.AMA_MasterBill = "MasterBill";
			Header.AMA_AgentType = "H";
			Header.AMA_Voyage = "INAKA";
			Header.MasterBill.ABL_E_DEP = new ZDateTime(2024, 6, 1);
			Header.MasterBill.ABL_GoodsLocation = "Osaka";

			var unloco1 = Factory.New<RefUNLOCO>();
			unloco1.RL_Code = "JP001";
			unloco1.RL_IATA = "TKU";
			var unloco2 = Factory.New<RefUNLOCO>();
			unloco2.RL_Code = "JP002";
			unloco2.RL_IATA = "KYU";
			Header.AMA_RL_NKPortOfFirstArrival = "JP001";
			Header.AMA_RL_NKPortOfLoading = "JP002";
			Header.PortOfDischargeIATACode = "NRT";

			Bill.ABL_BillNumber = "11111";
			Bill.ABL_GoodsDescription = "KURUMA";
			Bill.ABL_RL_NKFinalDestination = "JP001";
			Bill.ABL_RL_NKPortOfDischarge = "JP002";

			Bill.ABL_ShipperRegNo = "13579";
			Bill.ABL_ShipperName = "ABC";
			Bill.ABL_ShipperStreet1 = "KOBAYASHI";
			Bill.ABL_ShipperStreet2 = "KOKODAYO";
			Bill.ABL_ShipperCity = "Fukuoka";
			Bill.ABL_ShipperState = "Kyushu";
			Bill.ABL_ShipperPostcode = "215600";
			Bill.ABL_RN_NKShipperCountry = "CN";
			Bill.ABL_ShipperPhone = "08035288888";

			Bill.ABL_ConsigneeRegNo = "13579";
			Bill.ABL_ConsigneeName = "GINNZA";
			Bill.ABL_ConsigneeStreet1 = "ConsigneeStreet1";
			Bill.ABL_ConsigneeStreet2 = "ConsigneeStreet2";
			Bill.ABL_ConsigneeCity = "SAPO";
			Bill.ABL_ConsigneeState = "ABCDE";
			Bill.ABL_ConsigneePostcode = "365000";
			Bill.ABL_RN_NKConsigneeCountry = "JP";
			Bill.ABL_ConsigneePhone = "08035285555";

			Bill.ABL_NotifyPartyRegNo = "13579";
			Bill.ABL_NotifyPartyName = "GINNZA";
			Bill.ABL_NotifyPartyStreet1 = "ConsigneeStreet1";
			Bill.ABL_NotifyPartyStreet2 = "ConsigneeStreet2";
			Bill.ABL_NotifyPartyCity = "SAPO";
			Bill.ABL_NotifyPartyState = "ABCDE";
			Bill.ABL_NotifyPartyPostcode = "365000";
			Bill.ABL_RN_NKNotifyPartyCountry = "AU";
			Bill.ABL_NotifyPartyPhone = "08035286666";

			Bill.ABL_Tariff = "Osaka";
			Bill.ABL_MarksAndNumbers = "22222";
			Bill.ABL_ManifestQty = 1000;
			Bill.ABL_ManifestUQ = Core.Constants.Weight.Pounds;
			Bill.ABL_GrossWeight = 123456.7891;
			Bill.ABL_GrossWeightUQ = Core.Constants.Weight.Pounds;
			Bill.ABL_NetWeight = 1002m;
			Bill.ABL_NetWeightUQ = Core.Constants.Weight.Grams;
			Bill.ABL_Volume = 1003m;
			Bill.ABL_VolumeUQ = Core.Constants.Volume.CubicDecimetres;

			Bill.ABL_CountryOfOrigin = "JP";
			Bill.ABL_SpecialCargoCode = "CAC";
			Bill.ABL_TransportValue = 200;
			Bill.ABL_RX_NKTransportValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			Bill.ABL_FreightValue = 300;
			Bill.ABL_RX_NKFreightValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;

			Bill.TemporaryLandingInfo.CSI_Code = CusSupportingInfoTypeList.Codes.ApprovalCertificate;
			Bill.TemporaryLandingInfo.CSI_ItemNumber = 10;
			Bill.TemporaryLandingInfo.CSI_DateOfIssue = new ZDateTime(2024, 11, 15, 12, 30, 25);
			Bill.TemporaryLandingInfo.CSI_DateOfExpiry = new ZDateTime(2024, 11, 18, 12, 30, 25);
			Bill.TemporaryLandingInfo.CSI_ReferenceNumber = "30";

			Bill.ABL_GoodsLocation = "Osaka";
			Bill.ABL_LocationInformation = "TEST";
			Bill.ABL_Remarks = string.Empty;

			Bill.OtherLawsandRegulations.AddNew().CFR_Reference = "T";

			Factory.Save();

			CombineAssertions(() =>
			{
				using (Header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.NVC01 }))
				{
					var sendingObjectParent = new ManifestMessageSendingObjectParent(Header);
					var sendingObject = sendingObjectParent.SendingObjectsCollection.FirstOrDefault() as ManifestMessageSendingObject;
					sendingObject.ShouldSend = true;
					sendingObject.Action = "C";
					var provider = new ManifestHeaderMesssageProvider(new List<ManifestMessageSendingObject>() { sendingObject });

					AssertEquals("Action", "C", provider.ActionTypeCode);
					AssertEquals("MAWB", Header.AMA_MasterBill, provider.MAWB);
					AssertEquals("MasterBill GoodsDescription", "Osaka", provider.BondedLocationCode);
					AssertEquals("CustomsOffice", Header.AMA_CustomsOffice, provider.CustomsOffice);

					var bills = provider.HouseBills;
					AssertEquals("Bills count", 1, bills.Count());
					var bill = bills.First();

					AssertEquals("HouseBillNumbers", Bill.ABL_BillNumber, bill.HouseBillNumbers);
					AssertEquals("GoodsDescription", Bill.ABL_GoodsDescription, bill.GoodsDescription);
					AssertEquals("Final Destination Code", Bill.ABL_RL_NKFinalDestination, bill.FinalDestination.Code);
					AssertEquals("Final Destination Name", string.Empty, bill.FinalDestination.Name);
					AssertEquals("Place of Delivery Code", Bill.ABL_RL_NKPortOfDischarge, bill.PlaceOfDelivery.Code);
					AssertEquals("Place of Delivery Name", string.Empty, bill.PlaceOfDelivery.Name);

					AssertEquals("Shipper.Code", Bill.ABL_ShipperRegNo, bill.Shipper.Code);
					AssertEquals("Shipper.Name", Bill.ABL_ShipperName, bill.Shipper.Name);
					AssertEquals("Shipper.Address", string.Empty, bill.Shipper.Address);
					AssertEquals("Shipper.Phone", Bill.ABL_ShipperPhone, bill.Shipper.Phone);
					AssertEquals("Shipper.CountryCode", Bill.ABL_RN_NKShipperCountry, bill.Shipper.CountryCode);

					AssertEquals("Consignee.Code", Bill.ABL_ConsigneeRegNo, bill.Consignee.Code);
					AssertEquals("Consignee.Name", Bill.ABL_ConsigneeName, bill.Consignee.Name);
					AssertEquals("Consignee.Address", string.Empty, bill.Consignee.Address);
					AssertEquals("Consignee.Phone", Bill.ABL_ConsigneePhone, bill.Consignee.Phone);
					AssertEquals("Consignee.CountryCode", Bill.ABL_RN_NKConsigneeCountry, bill.Consignee.CountryCode);

					var notifyParty = bill.Notifier.FirstOrDefault();
					AssertEquals("Notifier.Code", Bill.ABL_NotifyPartyRegNo, notifyParty.Code);
					AssertEquals("Notifier.Name", Bill.ABL_NotifyPartyName, notifyParty.Name);
					AssertEquals("Notifier.Address", string.Empty, notifyParty.Address);
					AssertEquals("Notifier.Phone", Bill.ABL_NotifyPartyPhone, notifyParty.Phone);
					AssertEquals("Notifier.CountryCode", Bill.ABL_RN_NKNotifyPartyCountry, notifyParty.CountryCode);

					AssertEquals("Tariff", Bill.ABL_Tariff, bill.Tariff);
					AssertEquals("MarksAndNumbers", Bill.ABL_MarksAndNumbers, bill.MarksAndNumbers);
					AssertEquals("Quantity.Quantity", (decimal?)Bill.ABL_ManifestQty, bill.Quantity.Quantity);
					AssertEquals("Quantity.Unit", "LB", bill.Quantity.Unit);
					AssertEquals("TotalWeight.Quantity", Bill.CustomsWeight, bill.TotalWeight.Quantity);
					AssertEquals("TotalWeight.Unit", Bill.CustomsWeightUQ, bill.TotalWeight.Unit);
					AssertEquals("NewWeight.Quantity", Bill.CustomsNetWeight, bill.NetWeight.Quantity);
					AssertEquals("NewWeight.Unit", Bill.CustomsNetWeightUQ, bill.NetWeight.Unit);
					AssertEquals("Volume.Quantity", Bill.CustomsVolume, bill.Volume.Quantity);
					AssertEquals("Volume.Unit", Bill.CustomsVolumeUQ, bill.Volume.Unit);

					AssertEquals("Goods Origin Country Code", "JP", bill.GoodsOfOrigin);
					AssertEquals("SpecialCargoCode", "CAC", bill.SpecialCargoCode);
					AssertEquals("FreightValue.Amount", Bill.ABL_TransportValue, bill.FreightValue.Amount);
					AssertEquals("FreightValue.CurrencyCode", Bill.ABL_RX_NKTransportValueCurrency, bill.FreightValue.CurrencyCode);
					AssertEquals("TransportValue.Amount", Bill.ABL_FreightValue, bill.GoodsValue.Amount);
					AssertEquals("TransportValue.CurrencyCode", Bill.ABL_RX_NKFreightValueCurrency, bill.GoodsValue.CurrencyCode);

					AssertEquals("IsTemporaryLanding", "28", bill.IsTemporaryLanding);
					AssertEquals("ReasonCodeForTemporaryLanding", "CER", bill.ReasonCodeForTemporaryLanding);
					AssertEquals("NumberOfDaysForTemporaryLanding", (short)10, bill.NumberOfDaysForTemporaryLanding);
					AssertEquals("StartDateForTemporaryLanding", Bill.TemporaryLandingInfo.CSI_DateOfIssue, bill.StartDateForTemporaryLanding);
					AssertEquals("EndDateForTemporaryLanding", Bill.TemporaryLandingInfo.CSI_DateOfExpiry, bill.EndDateForTemporaryLanding);
					AssertEquals("TransportationCodeDuringTemporaryLanding", Bill.TemporaryLandingInfo.CSI_ReferenceNumber, bill.TransportationCodeDuringTemporaryLanding);

					AssertEquals("PortOfDischarge", Bill.ABL_GoodsLocation, bill.PortOfDischarge);
					AssertEquals("PortOfDischargeName", ZString.Empty, bill.PortOfDischargeName);
					AssertEquals("Remarks", Bill.ABL_Remarks, bill.Notes);

					AssertEquals("CodeForVerification", "T", bill.CodeForVerification.SingleOrDefault());

					header.AMA_GS_NKCustomsAgent = staff.GS_Code;
					header.AMA_CustomsAgentCredentialPK = bthPassword.PK;
					sendingObjectParent = new ManifestMessageSendingObjectParent(Header);
					sendingObject = sendingObjectParent.SendingObjectsCollection.FirstOrDefault() as ManifestMessageSendingObject;
					sendingObject.ShouldSend = true;
					provider = new ManifestHeaderMesssageProvider(new List<ManifestMessageSendingObject>() { sendingObject });
					AssertNull("ConsolidatorCode when mailBoxID is same with NUC code", provider.ConsolidatorCode);
				}
			});
		}

		public void TestHDE()
		{
			Header.AMA_MasterBill = "123456";

			using (Header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.HDE }))
			{
				var sendingObject = new ManifestMessageSendingObject(Header.MasterBill);
				var provider = new ManifestHeaderMesssageProvider(new List<ManifestMessageSendingObject>() { sendingObject });

				AssertContainsExactElementsInExactOrder("MAWB", ["123456"], (provider as IHDE).MAWB);
			}
		}

		public void TestCHA()
		{
			var consolidator = Factory.New<OrgHeader>();
			consolidator.FillWithValidTestData();

			var contractorAddress = consolidator.Addresses.AddNew();
			contractorAddress.OA_Code = "Code1";
			contractorAddress.OA_Address1 = "OA_Address1";
			contractorAddress.OA_City = "Tokyo";
			contractorAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);

			var customsCode = contractorAddress.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Japan;
			customsCode.OK_CodeType = OrgCusCode.JapanCodeTypes.NUC;
			customsCode.OK_CustomsRegNo = "12345";
			Header.AMA_OA_Consolidator = contractorAddress.PK;

			Header.AMA_CustomsOffice = "TY";
			Header.AMA_MasterBill = "MasterBill";
			Header.AMA_AgentType = "H";
			Header.AMA_Voyage = "INAKA";
			Header.MasterBill.ABL_E_DEP = new ZDateTime(2024, 6, 1);

			var unloco1 = Factory.New<RefUNLOCO>();
			unloco1.RL_Code = "JP001";
			unloco1.RL_IATA = "TKU";
			var unloco2 = Factory.New<RefUNLOCO>();
			unloco2.RL_Code = "JP002";
			unloco2.RL_IATA = "KYU";
			Header.AMA_RL_NKPortOfFirstArrival = "JP001";
			Header.AMA_RL_NKPortOfLoading = "JP002";
			Header.PortOfDischargeIATACode = "NRT";

			Bill.ABL_BillNumber = "11111";
			Bill.ABL_ManifestQty = 111;
			Bill.ABL_GrossWeight = 123456.890;
			Bill.ABL_GrossWeightUQ = Core.Constants.Weight.Pounds;
			Bill.ABL_GoodsDescription = "KURUMA";
			Bill.ABL_SpecialCargoCode = "CAC";
			Bill.ABL_RL_NKFinalDestination = "JP001";
			Bill.ABL_GoodsLocation = "Osaka";

			Bill.ABL_ShipperName = "ABC";
			Bill.ABL_ShipperStreet1 = "KOBAYASHI";
			Bill.ABL_ShipperStreet2 = "KOKODAYO";
			Bill.ABL_ShipperCity = "Fukuoka";
			Bill.ABL_ShipperState = "Kyushu";
			Bill.ABL_ShipperPostcode = "215600";
			Bill.ABL_RN_NKShipperCountry = "CN";
			Bill.ABL_ShipperPhone = "08035288888";

			Bill.ABL_ConsigneeRegNo = "13579";
			Bill.ABL_ConsigneeName = "GINNZA";
			Bill.ABL_ConsigneePostcode = "365000";
			Bill.ABL_ConsigneeState = "ABCDE";
			Bill.ABL_ConsigneeCity = "SAPO";
			Bill.ABL_ConsigneeStreet1 = "ConsigneeStreet1";
			Bill.ABL_ConsigneeStreet2 = "ConsigneeStreet2";
			Bill.ABL_ConsigneePhone = "08035286666";

			Factory.Save();

			CombineAssertions(() =>
			{
				using (Header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.CHA }))
				{
					var sendingObjectParent = new ManifestMessageSendingObjectParent(Header);
					var sendingObject = sendingObjectParent.SendingObjectsCollection.FirstOrDefault() as ManifestMessageSendingObject;
					sendingObject.Reason = ReasonList.Codes.ADD;
					sendingObject.ShouldSend = true;
					var provider = new ManifestHeaderMesssageProvider(new List<ManifestMessageSendingObject>() { sendingObject }) as ICHA;

					AssertEquals("MAWB", Header.AMA_MasterBill, provider.MAWB);
					AssertEquals("IsSubConsolidation", "Y", provider.IsSubConsolidation);
					AssertEquals("ConsolidatorCode", customsCode.OK_CustomsRegNo, provider.ConsolidatorCode);
					AssertEquals("ArrivalFlightNumber", Header.AMA_Voyage, provider.ArrivalFlightNumber);
					AssertEquals("ArrivalFlightDate", "01JUN", provider.ArrivalFlightDate);
					AssertEquals("PortOfDischarge", "NRT", provider.PortOfDischarge);
					AssertEquals("PortOfLoading", "KYU", provider.PortOfLoading);

					var bills = provider.Bills;
					AssertEquals("Bills count", 1, bills.Count());
					var bill = bills.FirstOrDefault();
					AssertEquals("HAWB", Bill.ABL_BillNumber, bill.HAWB);
					AssertEquals("TotalCount", Bill.ABL_ManifestQty, bill.TotalCount);
					AssertEquals("TotalWeight.Quantity", Bill.CustomsWeight, bill.TotalWeight.Quantity);
					AssertEquals("TotalWeight.Unit", Bill.CustomsWeightUQ, bill.TotalWeight.Unit);
					AssertEquals("GoodsDescription", Bill.ABL_GoodsDescription, bill.GoodsDescription);
					AssertEquals("SpecialCargoCode", Bill.ABL_SpecialCargoCode, bill.SpecialCargoCode);
					AssertEquals("FinalDestination", "TKU", bill.FinalDestination);
					AssertEquals("GoodsLocation", Bill.ABL_GoodsLocation, bill.GoodsLocation);

					AssertEquals("Shipper.Name", Bill.ABL_ShipperName, bill.Shipper.Name);
					AssertEquals("Shipper.Address", Bill.ShipperAddress, bill.Shipper.Address);
					AssertEquals("Shipper.Phone", Bill.ABL_ShipperPhone, bill.Shipper.Phone);

					AssertEquals("Consignee.Code", Bill.ABL_ConsigneeRegNo, bill.Consignee.Code);
					AssertEquals("Consignee.Name", Bill.ABL_ConsigneeName, bill.Consignee.Name);
					AssertEquals("Consignee.Address", Bill.ConsigneeAddress, bill.Consignee.Address);
					AssertEquals("Consignee.Phone", Bill.ABL_ConsigneePhone, bill.Consignee.Phone);
					AssertEquals("Reason", sendingObject.Reason, bill.Reason);
				}
			});
		}

		AsycudaManifestHeader Header => header ??= Factory.New<AsycudaManifestHeader>();
		AsycudaManifestHeader header;

		AsycudaBill Bill => bill ??= Header.Bills.AddNew();
		AsycudaBill bill;
	}
}
