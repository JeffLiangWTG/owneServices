using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusSCAHouseSynchroniserTest : TestCaseWithFactory
	{
		public void TestSettingAirOriginalCCN()
		{
			var helper = new CusSCATestHelper();
			helper.Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			helper.Consol.JK_MasterBillNum = "08112345678";
			var pCN = helper.Consol.Numbers.AddNew();
			pCN.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.PCN;
			pCN.CE_EntryNum = "ORIGCCN";
			helper.Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Air CCN with airline prefix", "081-ORIGCCN", helper.House.OriginalCCN);
		}

		public void TestNotSettingAirOriginalCCN()
		{
			var helper = new CusSCATestHelper();
			helper.Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			helper.Consol.JK_MasterBillNum = "08112345678";
			var pCN = helper.Consol.Numbers.AddNew();
			pCN.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.PCN;
			pCN.CE_EntryNum = "014-ORIGCCN";
			helper.Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Air CCN with airline prefix", "014-ORIGCCN", helper.House.OriginalCCN);
		}

		public void TestWhenShipmentJobNumberIsEmpty()
		{
			var helper = new CusSCATestHelper();
			try
			{
				ValidationTestHelper.AddCarrierCodeToCurrentCompany(Factory, "8080");
				helper.Shipment.JS_UniqueConsignRef = ZString.Empty;
				AssertEquals("SupplementaryReferenceNumber should not be set when no job number", ZString.Empty, helper.House.SupplementaryReferenceNumber);
				// Note:  SupplementaryCargoReportMessageManager will not allow sending message until supplementary number is set, and gives instructions on how to resolve.
			}
			finally
			{
				ValidationTestHelper.RemoveCarrierCodeFromCurrentCompany(Factory);
			}
		}

		public void TestSupplementaryNoTooLong()
		{
			CACustomsDataRegistry.Instance.SupplementaryNumberSuffixAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "XYZ");
			var helper = new CusSCATestHelper();
			try
			{
				ValidationTestHelper.AddCarrierCodeToCurrentCompany(Factory, "8080");
				helper.Shipment.JS_UniqueConsignRef = "12345678901234567890";
				AssertEquals("SupplementaryReferenceNumber truncated and no exception", "808012345678901234567890X", helper.House.SupplementaryReferenceNumber);
			}
			finally
			{
				ValidationTestHelper.RemoveCarrierCodeFromCurrentCompany(Factory);
			}
		}

		public void TestCusSCAHouseShipmentSynchroniser()
		{
			CACustomsDataRegistry.Instance.SupplementaryNumberSuffixAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "XYZ");

			var helper = new CusSCATestHelper();
			try
			{
				ValidationTestHelper.AddCarrierCodeToCurrentCompany(Factory, "8080");
				var pCN = helper.Consol.Numbers.AddNew();
				pCN.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.PCN;
				pCN.CE_EntryNum = "ORIGCCN";
				var c1 = helper.Container1;
				var c2 = helper.Container2;
				var c3 = helper.Container3;
				c3.JC_IsEmptyContainer = true;
				helper.Shipment.JS_MarksAndNumbersShort = "SHORT MARKS";
				helper.Shipment.JS_GoodsDescription = "SHIPMENT DESC";
				helper.Shipment.DetailedGoodsDescriptionNoteText = "SHIPMENT LONG DESCRIPTION";

				PackLine packLine = helper.Shipment.OuterPackLines[0];
				packLine.SetContainer(c1.PK);
				packLine.JL_PackageCount = new ZInt(5);
				packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.BaleUncompressed;
				packLine.JL_ActualWeight = 250m;
				packLine.JL_ActualWeightUQ = "KG";
				packLine.JL_ActualVolume = 2.5m;
				packLine.JL_ActualVolumeUQ = "M3";
				packLine.JL_HarmonisedCode = "123456";
				packLine.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
				packLine.JL_DetailedDescription = "DETAILED DESCRIPTION";
				packLine.JL_MarksAndNumbers = "MARKS";
				PackLine packLine2 = helper.Shipment.OuterPackLines.AddNew();
				packLine2.JL_PackageCount = new ZInt(1);
				packLine2.JL_F3_NKPackType = Core.Constants.PkgUnit.Unit;
				packLine2.JL_ActualWeight = 10000m;
				packLine2.JL_ActualWeightUQ = "KG";
				packLine2.JL_Description = "SHORT DESCRIPTION";
				packLine2.Containers.RemoveAll();
				PackLine packLine3 = helper.Shipment.OuterPackLines.AddNew();
				packLine3.JL_PackageCount = new ZInt(1);
				packLine3.JL_F3_NKPackType = Core.Constants.PkgUnit.Unit;
				packLine3.JL_ActualWeight = 10000m;
				packLine3.JL_ActualWeightUQ = "KG";
				packLine3.JL_Description = "";
				packLine3.JL_DetailedDescription = "";
				packLine3.Containers.RemoveAll();
				Factory.Save();

				// first touch of House synchronises
				AssertEquals("CA_JS", helper.Shipment.PK, helper.House.CA_JS);
				AssertEquals("Port of destination", "CATOR", helper.House.CA_RL_NK_PortOfDestination);
				AssertEquals("House Bill", "HBL1234", helper.House.CA_HouseBill);
				AssertEquals("Consignee name", "CONSIGNEE NAME WHICH IS MORE THAN 35 CHARACTERS", helper.House.CA_ConsigneeName);
				AssertEquals("Consignee address 1", "CONSIGNEE ADDRESS LINE 1", helper.House.CA_ConsigneeAddress1);
				AssertEquals("Consignee address 2", "CONSIGNEE ADDRESS LINE 2", helper.House.CA_ConsigneeAddress2);
				AssertEquals("Consignee city", "MANHATTAN", helper.House.CA_ConsigneeSuburb);
				AssertEquals("Consignee post code", "12986", helper.House.CA_ConsigneePostcode);
				AssertEquals("Consignee country", "US", helper.House.CA_RN_NKConsigneeCountryCode);
				AssertEquals("Consignee state", "NY", helper.House.CA_ConsigneeState);
				AssertEquals("Consignee state", "2125555212", helper.House.CA_ConsigneePhone);
				AssertEquals("Consignee contact", "FRANK", helper.House.CA_ConsigneeContactName);
				AssertEquals("Consignor name", "CONSIGNOR NAME LINE 1", helper.House.CA_ConsignorName);
				AssertEquals("Consignor address 1", "CONSIGNOR ADDRESS LINE 1", helper.House.CA_ConsignorAddress1);
				AssertEquals("Consignor address 2", "", helper.House.CA_ConsignorAddress2);
				AssertEquals("Consignor city", "PARIS", helper.House.CA_ConsignorSuburb);
				AssertEquals("Consignor post code", "", helper.House.CA_ConsignorPostcode);
				AssertEquals("Consignor country", "FR", helper.House.CA_RN_NKConsignorCountryCode);
				AssertEquals("Consignor state", "", helper.House.CA_ConsignorState);
				AssertEquals("Consignor state", "0129337218", helper.House.CA_ConsignorPhone);
				AssertEquals("Consignor contact", "GILLES", helper.House.CA_ConsignorContactName);
				AssertEquals("Delivery name", "DELIVERED TO 1", helper.House.CA_DeliveryName);
				AssertEquals("Delivery address 1", "DELIVERY 1 ADDRESS LINE 1", helper.House.CA_DeliveryAddress1);
				AssertEquals("Delivery address 2", "", helper.House.CA_DeliveryAddress2);
				AssertEquals("Delivery city", "MANHATTAN", helper.House.CA_DeliverySuburb);
				AssertEquals("Delivery post code", "12783", helper.House.CA_DeliveryPostcode);
				AssertEquals("Delivery country", "US", helper.House.CA_RN_NKDeliveryCountryCode);
				AssertEquals("Delivery state", "NY", helper.House.CA_DeliveryState);
				AssertEquals("Delivery state", "2125551212", helper.House.CA_DeliveryPhone);
				AssertEquals("Delivery contact", "ELIZABETH", helper.House.CA_DeliveryContactName);
				AssertEquals("Notify name", "NOTIFY PARTY 1", helper.House.CA_NotifyName);
				AssertEquals("Notify address 1", "ADDRESS LINE 1", helper.House.CA_NotifyAddress1);
				AssertEquals("Notify address 2", "", helper.House.CA_NotifyAddress2);
				AssertEquals("Notify city", "NEW YORK", helper.House.CA_NotifySuburb);
				AssertEquals("Notify post code", "12345", helper.House.CA_NotifyPostcode);
				AssertEquals("Notify country", "US", helper.House.CA_RN_NKNotifyCountryCode);
				AssertEquals("Notify state", "NY", helper.House.CA_NotifyState);
				AssertEquals("Notify state", "6475551212", helper.House.CA_NotifyPhone);
				AssertEquals("Notify contact", "SUZANNE", helper.House.CA_NotifyContactName);
				AssertEquals("Ocean bill", CusSCATestHelper.MasterBillNum, helper.House.BillOfLading);
				AssertEquals("Application Code", Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACISea, helper.House.OceanBill.CB_ApplicationCode);
				AssertEquals("Original CCN", pCN.CE_EntryNum, helper.House.OriginalCCN);
				AssertEquals("Supplementary Reference Number", "8080S12345678XYZ", helper.House.SupplementaryReferenceNumber);
				AssertEquals("BGM referencer", "S12345678", helper.House.CA_BGMReference);
				AssertEquals("Oceanbill containers", 3, helper.House.OceanBill.Containers.Count);
				var c1Index = -1;
				var c2Index = -1;
				var c3Index = -1;
				for (var i = 0; i < 3; i++)
				{
					if (helper.House.OceanBill.Containers[i].CN_ContainerNumber == c1.JC_ContainerNum)
					{
						c1Index = i;
					}
					else if (helper.House.OceanBill.Containers[i].CN_ContainerNumber == c2.JC_ContainerNum)
					{
						c2Index = i;
					}
					else if (helper.House.OceanBill.Containers[i].CN_ContainerNumber == c3.JC_ContainerNum)
					{
						c3Index = i;
					}
				}
				Assert("C1 not found", c1Index > -1);
				Assert("C2 not found", c2Index > -1);
				Assert("C3 not found", c3Index > -1);
				AssertEquals("Oceanbill container 1 num", c1.JC_ContainerNum, helper.House.OceanBill.Containers[c1Index].CN_ContainerNumber);
				AssertEquals("Oceanbill container 1 type", "20GP", helper.House.OceanBill.Containers[c1Index].CN_RC_NKContainerType);
				AssertEquals("Oceanbill container 1 mode", Core.Constants.ContainerModes.Containerised, helper.House.OceanBill.Containers[c1Index].CN_ContainerMode);
				AssertEquals("Oceanbill container 2 num", c2.JC_ContainerNum, helper.House.OceanBill.Containers[c2Index].CN_ContainerNumber);
				AssertEquals("Oceanbill container 2 type", "20GP", helper.House.OceanBill.Containers[c2Index].CN_RC_NKContainerType);
				AssertEquals("Oceanbill container 2 mode", Core.Constants.ContainerModes.Containerised, helper.House.OceanBill.Containers[c2Index].CN_ContainerMode);
				AssertEquals("Oceanbill container 3 num", c3.JC_ContainerNum, helper.House.OceanBill.Containers[c3Index].CN_ContainerNumber);
				AssertEquals("Oceanbill container 3 type", "40GP", helper.House.OceanBill.Containers[c3Index].CN_RC_NKContainerType);
				AssertEquals("Oceanbill container 3 mode", Core.Constants.ContainerModes.Empty, helper.House.OceanBill.Containers[c3Index].CN_ContainerMode);

				AssertEquals("3 pack lines", 3, helper.House.PackLines.Count);
				AssertEquals("pack line 1 container", c1.JC_ContainerNum, helper.House.PackLines[0].CV_AssociatedContainer);
				AssertEquals("pack line 1 packs", new ZInt(5), helper.House.PackLines[0].CV_PackageCount);
				AssertEquals("pack line 1 UQ", ACROSSPackageTypes.Codes.BALEBLE, helper.House.PackLines[0].CV_PackageType);
				AssertEquals("pack line 1 weight", 250m, helper.House.PackLines[0].CV_Weight);
				AssertEquals("pack line 1 weightUQ", "KG", helper.House.PackLines[0].CV_WeightUQ);
				AssertEquals("pack line 1 volume", 2.5m, helper.House.PackLines[0].CV_Volume);
				AssertEquals("pack line 1 volume UQ", "M3", helper.House.PackLines[0].CV_VolumeUQ);
				AssertEquals("pack line 1 tariff", "123456", helper.House.PackLines[0].CV_HarmonisedTariffNums);
				AssertEquals("pack line 1 description", "DETAILED DESCRIPTION", helper.House.PackLines[0].CV_GoodsDescription);
				AssertEquals("pack line 1 marks", "MARKS", helper.House.PackLines[0].CV_MarksAndNumbers);
				AssertEquals("pack line 1 DG count", 1, helper.House.PackLines[0].UNDGs.Count);
				AssertEquals("pack line 1 DG substance", "0004a", helper.House.PackLines[0].UNDGs[0].UNDGSubstance.DG_Code);
				AssertEquals("pack line 2 container", "NCT", helper.House.PackLines[1].CV_AssociatedContainer);
				AssertEquals("pack line 2 packs", new ZInt(1), helper.House.PackLines[1].CV_PackageCount);
				AssertEquals("pack line 2 UQ", ACROSSPackageTypes.Codes.UNIT, helper.House.PackLines[1].CV_PackageType);
				AssertEquals("pack line 2 weight", 10000m, helper.House.PackLines[1].CV_Weight);
				AssertEquals("pack line 2 weightUQ", "KG", helper.House.PackLines[1].CV_WeightUQ);
				AssertEquals("pack line 2 volume", 0m, helper.House.PackLines[1].CV_Volume);
				AssertEquals("pack line 2 volume UQ", "M3", helper.House.PackLines[1].CV_VolumeUQ);
				AssertEquals("pack line 2 tariff", "", helper.House.PackLines[1].CV_HarmonisedTariffNums);
				AssertEquals("pack line 2 description", "SHORT DESCRIPTION", helper.House.PackLines[1].CV_GoodsDescription);
				AssertEquals("pack line 2 marks", "SHORT MARKS", helper.House.PackLines[1].CV_MarksAndNumbers);
				AssertEquals("pack line 3 description", "SHIPMENT LONG DESCRIPTION", helper.House.PackLines[2].CV_GoodsDescription);
				AssertEquals("pack line 3 volume", 0m, helper.House.PackLines[2].CV_Volume);

				packLine.JL_PackageCount = new ZInt(6);
				packLine2.JL_ActualWeight = 10001m;
				packLine3.JL_ActualVolume = 5m;

				AssertEquals("CA_JS", helper.Shipment.PK, helper.House.CA_JS);
				AssertEquals("Port of destination", "CATOR", helper.House.CA_RL_NK_PortOfDestination);
				AssertEquals("House Bill", "HBL1234", helper.House.CA_HouseBill);
				AssertEquals("Consignee name", "CONSIGNEE NAME WHICH IS MORE THAN 35 CHARACTERS", helper.House.CA_ConsigneeName);
				AssertEquals("Consignee address 1", "CONSIGNEE ADDRESS LINE 1", helper.House.CA_ConsigneeAddress1);
				AssertEquals("Consignee address 2", "CONSIGNEE ADDRESS LINE 2", helper.House.CA_ConsigneeAddress2);
				AssertEquals("Consignee city", "MANHATTAN", helper.House.CA_ConsigneeSuburb);
				AssertEquals("Consignee post code", "12986", helper.House.CA_ConsigneePostcode);
				AssertEquals("Consignee country", "US", helper.House.CA_RN_NKConsigneeCountryCode);
				AssertEquals("Consignee state", "NY", helper.House.CA_ConsigneeState);
				AssertEquals("Consignee state", "2125555212", helper.House.CA_ConsigneePhone);
				AssertEquals("Consignee contact", "FRANK", helper.House.CA_ConsigneeContactName);
				AssertEquals("Consignor name", "CONSIGNOR NAME LINE 1", helper.House.CA_ConsignorName);
				AssertEquals("Consignor address 1", "CONSIGNOR ADDRESS LINE 1", helper.House.CA_ConsignorAddress1);
				AssertEquals("Consignor address 2", "", helper.House.CA_ConsignorAddress2);
				AssertEquals("Consignor city", "PARIS", helper.House.CA_ConsignorSuburb);
				AssertEquals("Consignor post code", "", helper.House.CA_ConsignorPostcode);
				AssertEquals("Consignor country", "FR", helper.House.CA_RN_NKConsignorCountryCode);
				AssertEquals("Consignor state", "", helper.House.CA_ConsignorState);
				AssertEquals("Consignor state", "0129337218", helper.House.CA_ConsignorPhone);
				AssertEquals("Consignor contact", "GILLES", helper.House.CA_ConsignorContactName);
				AssertEquals("Delivery name", "DELIVERED TO 1", helper.House.CA_DeliveryName);
				AssertEquals("Delivery address 1", "DELIVERY 1 ADDRESS LINE 1", helper.House.CA_DeliveryAddress1);
				AssertEquals("Delivery address 2", "", helper.House.CA_DeliveryAddress2);
				AssertEquals("Delivery city", "MANHATTAN", helper.House.CA_DeliverySuburb);
				AssertEquals("Delivery post code", "12783", helper.House.CA_DeliveryPostcode);
				AssertEquals("Delivery country", "US", helper.House.CA_RN_NKDeliveryCountryCode);
				AssertEquals("Delivery state", "NY", helper.House.CA_DeliveryState);
				AssertEquals("Delivery state", "2125551212", helper.House.CA_DeliveryPhone);
				AssertEquals("Delivery contact", "ELIZABETH", helper.House.CA_DeliveryContactName);
				AssertEquals("Notify name", "NOTIFY PARTY 1", helper.House.CA_NotifyName);
				AssertEquals("Notify address 1", "ADDRESS LINE 1", helper.House.CA_NotifyAddress1);
				AssertEquals("Notify address 2", "", helper.House.CA_NotifyAddress2);
				AssertEquals("Notify city", "NEW YORK", helper.House.CA_NotifySuburb);
				AssertEquals("Notify post code", "12345", helper.House.CA_NotifyPostcode);
				AssertEquals("Notify country", "US", helper.House.CA_RN_NKNotifyCountryCode);
				AssertEquals("Notify state", "NY", helper.House.CA_NotifyState);
				AssertEquals("Notify state", "6475551212", helper.House.CA_NotifyPhone);
				AssertEquals("Notify contact", "SUZANNE", helper.House.CA_NotifyContactName);
				AssertEquals("Ocean bill", CusSCATestHelper.MasterBillNum, helper.House.BillOfLading);
				AssertEquals("Application Code", Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACISea, helper.House.OceanBill.CB_ApplicationCode);
				AssertEquals("Original CCN", pCN.CE_EntryNum, helper.House.OriginalCCN);
				AssertEquals("Supplementary Reference Number", "8080S12345678XYZ", helper.House.SupplementaryReferenceNumber);
				AssertEquals("BGM referencer", "S12345678", helper.House.CA_BGMReference);
				AssertEquals("Oceanbill containers", 3, helper.House.OceanBill.Containers.Count);

				AssertEquals("Oceanbill container 1 num", c1.JC_ContainerNum, helper.House.OceanBill.Containers[c1Index].CN_ContainerNumber);
				AssertEquals("Oceanbill container 1 type", "20GP", helper.House.OceanBill.Containers[c1Index].CN_RC_NKContainerType);
				AssertEquals("Oceanbill container 1 mode", Core.Constants.ContainerModes.Containerised, helper.House.OceanBill.Containers[c1Index].CN_ContainerMode);
				AssertEquals("Oceanbill container 2 num", c2.JC_ContainerNum, helper.House.OceanBill.Containers[c2Index].CN_ContainerNumber);
				AssertEquals("Oceanbill container 2 type", "20GP", helper.House.OceanBill.Containers[c2Index].CN_RC_NKContainerType);
				AssertEquals("Oceanbill container 2 mode", Core.Constants.ContainerModes.Containerised, helper.House.OceanBill.Containers[c2Index].CN_ContainerMode);
				AssertEquals("Oceanbill container 3 num", c3.JC_ContainerNum, helper.House.OceanBill.Containers[c3Index].CN_ContainerNumber);
				AssertEquals("Oceanbill container 3 type", "40GP", helper.House.OceanBill.Containers[c3Index].CN_RC_NKContainerType);
				AssertEquals("Oceanbill container 3 mode", Core.Constants.ContainerModes.Empty, helper.House.OceanBill.Containers[c3Index].CN_ContainerMode);

				AssertEquals("3 pack lines", 3, helper.House.PackLines.Count);
				AssertEquals("pack line 1 container", c1.JC_ContainerNum, helper.House.PackLines[0].CV_AssociatedContainer);
				AssertEquals("pack line 1 packs", new ZInt(6), helper.House.PackLines[0].CV_PackageCount);
				AssertEquals("pack line 1 UQ", ACROSSPackageTypes.Codes.BALEBLE, helper.House.PackLines[0].CV_PackageType);
				AssertEquals("pack line 1 weight", 250m, helper.House.PackLines[0].CV_Weight);
				AssertEquals("pack line 1 weightUQ", "KG", helper.House.PackLines[0].CV_WeightUQ);
				AssertEquals("pack line 1 volume", 2.5m, helper.House.PackLines[0].CV_Volume);
				AssertEquals("pack line 1 volume UQ", "M3", helper.House.PackLines[0].CV_VolumeUQ);
				AssertEquals("pack line 1 tariff", "123456", helper.House.PackLines[0].CV_HarmonisedTariffNums);
				AssertEquals("pack line 1 description", "DETAILED DESCRIPTION", helper.House.PackLines[0].CV_GoodsDescription);
				AssertEquals("pack line 1 marks", "MARKS", helper.House.PackLines[0].CV_MarksAndNumbers);
				AssertEquals("pack line 1 DG count", 1, helper.House.PackLines[0].UNDGs.Count);
				AssertEquals("pack line 1 DG substance", "0004a", helper.House.PackLines[0].UNDGs[0].UNDGSubstance.DG_Code);
				AssertEquals("pack line 2 container", "NCT", helper.House.PackLines[1].CV_AssociatedContainer);
				AssertEquals("pack line 2 packs", new ZInt(1), helper.House.PackLines[1].CV_PackageCount);
				AssertEquals("pack line 2 UQ", ACROSSPackageTypes.Codes.UNIT, helper.House.PackLines[1].CV_PackageType);
				AssertEquals("pack line 2 weight", 10001m, helper.House.PackLines[1].CV_Weight);
				AssertEquals("pack line 2 weightUQ", "KG", helper.House.PackLines[1].CV_WeightUQ);
				AssertEquals("pack line 2 volume", 0m, helper.House.PackLines[1].CV_Volume);
				AssertEquals("pack line 2 volume UQ", "M3", helper.House.PackLines[1].CV_VolumeUQ);
				AssertEquals("pack line 2 tariff", "", helper.House.PackLines[1].CV_HarmonisedTariffNums);
				AssertEquals("pack line 2 description", "SHORT DESCRIPTION", helper.House.PackLines[1].CV_GoodsDescription);
				AssertEquals("pack line 2 marks", "SHORT MARKS", helper.House.PackLines[1].CV_MarksAndNumbers);
				AssertEquals("pack line 3 description", "SHIPMENT LONG DESCRIPTION", helper.House.PackLines[2].CV_GoodsDescription);
				AssertEquals("pack line 3 volume", 5m, helper.House.PackLines[2].CV_Volume);

				packLine.JL_PackageCount = new ZInt(7);
				packLine2.JL_F3_NKPackType = Core.Constants.PkgUnit.Bag;

				AssertEquals("pack line 1 packs changes", new ZInt(7), helper.House.PackLines[0].CV_PackageCount);
				AssertEquals("pack line 2 pack type changes", ACROSSPackageTypes.Codes.BAG, helper.House.PackLines[1].CV_PackageType);
				packLine3.SetContainer(c1.PK);
				helper.Shipment.OuterPackLines.RemoveAndDelete(packLine3);// should not cause exception
				AssertEquals("Should now be only 2 lines", 2, helper.House.PackLines.Count);
				helper.House.CA_OverrideFreightDefaults = true;
				packLine.JL_PackageCount = new ZInt(8);
				c2.JC_ContainerNum = "ZZZZ9879870";
				packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Basket;
				AssertEquals("pack line 1 packs should not change", new ZInt(7), helper.House.PackLines[0].CV_PackageCount);
				AssertEquals("Should not be an extra Oceanbill container", 3, helper.House.OceanBill.Containers.Count);
				AssertEquals("Should not be any extra pack lines", 2, helper.House.PackLines.Count);
			}
			finally
			{
				ValidationTestHelper.RemoveCarrierCodeFromCurrentCompany(Factory);
			}
		}

		public void TestSCAHouseShipmentSynchroniserWithOneContainerAndPackLine()
		{
			CACustomsDataRegistry.Instance.SupplementaryNumberSuffixAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "XYZ");

			var helper = new CusSCATestHelper();
			try
			{
				ValidationTestHelper.AddCarrierCodeToCurrentCompany(Factory, "8080");
				var c1 = helper.Container1;
				helper.Shipment.JS_GoodsDescription = "SHIPMENT DESC";

				PackLine packLine = helper.Shipment.OuterPackLines[0];
				packLine.SetContainer(c1.PK);
				packLine.JL_PackageCount = new ZInt(5);
				packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.BaleUncompressed;
				packLine.JL_ActualWeight = 250m;
				packLine.JL_ActualWeightUQ = "KG";
				packLine.JL_ActualVolume = 2.5m;
				packLine.JL_ActualVolumeUQ = "M3";
				packLine.JL_HarmonisedCode = "123456";
				packLine.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
				packLine.JL_DetailedDescription = "DETAILED DESCRIPTION";
				packLine.JL_MarksAndNumbers = "MARKS";
				Factory.Save();

				// first touch of House synchronises
				AssertEquals("CA_JS", helper.Shipment.PK, helper.House.CA_JS);
				AssertEquals("Port of destination", "CATOR", helper.House.CA_RL_NK_PortOfDestination);
				AssertEquals("House Bill", "HBL1234", helper.House.CA_HouseBill);
				AssertEquals("Consignee name", "CONSIGNEE NAME WHICH IS MORE THAN 35 CHARACTERS", helper.House.CA_ConsigneeName);
				AssertEquals("Consignor name", "CONSIGNOR NAME LINE 1", helper.House.CA_ConsignorName);
				AssertEquals("Delivery name", "DELIVERED TO 1", helper.House.CA_DeliveryName);
				AssertEquals("Delivery address 1", "DELIVERY 1 ADDRESS LINE 1", helper.House.CA_DeliveryAddress1);
				AssertEquals("Notify name", "NOTIFY PARTY 1", helper.House.CA_NotifyName);
				AssertEquals("Ocean bill", CusSCATestHelper.MasterBillNum, helper.House.BillOfLading);
				AssertEquals("Application Code", Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACISea, helper.House.OceanBill.CB_ApplicationCode);
				AssertEquals("Supplementary Reference Number", "8080S12345678XYZ", helper.House.SupplementaryReferenceNumber);
				AssertEquals("BGM referencer", "S12345678", helper.House.CA_BGMReference);
				AssertEquals("Oceanbill containers", 1, helper.House.OceanBill.Containers.Count);
				AssertEquals("Oceanbill container 1 num", c1.JC_ContainerNum, helper.House.OceanBill.Containers[0].CN_ContainerNumber);
				AssertEquals("1 pack lines", 1, helper.House.PackLines.Count);
				AssertEquals("pack line 1 container", c1.JC_ContainerNum, helper.House.PackLines[0].CV_AssociatedContainer);
				AssertEquals("mode is containerised", Core.Constants.ContainerModes.Containerised, helper.House.PackLines[0].CN_ContainerMode);
				AssertEquals("pack line 1 packs", new ZInt(5), helper.House.PackLines[0].CV_PackageCount);
				AssertEquals("pack line 1 UQ", ACROSSPackageTypes.Codes.BALEBLE, helper.House.PackLines[0].CV_PackageType);
				AssertEquals("pack line 1 weight", 250m, helper.House.PackLines[0].CV_Weight);
				AssertEquals("pack line 1 weightUQ", "KG", helper.House.PackLines[0].CV_WeightUQ);
				AssertEquals("pack line 1 volume", 2.5m, helper.House.PackLines[0].CV_Volume);
				AssertEquals("pack line 1 volume UQ", "M3", helper.House.PackLines[0].CV_VolumeUQ);
				AssertEquals("pack line 1 tariff", "123456", helper.House.PackLines[0].CV_HarmonisedTariffNums);
				AssertEquals("pack line 1 description", "DETAILED DESCRIPTION", helper.House.PackLines[0].CV_GoodsDescription);
				AssertEquals("pack line 1 marks", "MARKS", helper.House.PackLines[0].CV_MarksAndNumbers);
				AssertEquals("pack line 1 DG count", 1, helper.House.PackLines[0].UNDGs.Count);
				AssertEquals("pack line 1 DG substance", "0004a", helper.House.PackLines[0].UNDGs[0].UNDGSubstance.DG_Code);

				helper.Shipment.JS_RL_NKDestination = "CAVAN";
				packLine.JL_PackageCount = new ZInt(6);
				packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Bottle;
				packLine.UNDGs.DeleteAll();
				c1.JC_ContainerNum = "ZZZZ9879870";

				AssertEquals("Port of destination changes", "CAVAN", helper.House.CA_RL_NK_PortOfDestination);
				AssertEquals("Oceanbill containers not changed", 1, helper.House.OceanBill.Containers.Count);
				AssertEquals("Oceanbill container num", c1.JC_ContainerNum, helper.House.OceanBill.Containers[0].CN_ContainerNumber);
				AssertEquals("still 1 pack lines", 1, helper.House.PackLines.Count);
				AssertEquals("pack line 1 container", c1.JC_ContainerNum, helper.House.PackLines[0].CV_AssociatedContainer);
				AssertEquals("mode is containerised", Core.Constants.ContainerModes.Containerised, helper.House.PackLines[0].CN_ContainerMode);
				AssertEquals("pack line 1 packs", new ZInt(6), helper.House.PackLines[0].CV_PackageCount);
				AssertEquals("pack line 1 UQ", ACROSSPackageTypes.Codes.BOTTLE, helper.House.PackLines[0].CV_PackageType);
				AssertEquals("pack line 1 weight", 250m, helper.House.PackLines[0].CV_Weight);
				AssertEquals("pack line 1 weightUQ", "KG", helper.House.PackLines[0].CV_WeightUQ);
				AssertEquals("pack line 1 volume", 2.5m, helper.House.PackLines[0].CV_Volume);
				AssertEquals("pack line 1 volume UQ", "M3", helper.House.PackLines[0].CV_VolumeUQ);
				AssertEquals("pack line 1 tariff", "123456", helper.House.PackLines[0].CV_HarmonisedTariffNums);
				AssertEquals("pack line 1 description", "DETAILED DESCRIPTION", helper.House.PackLines[0].CV_GoodsDescription);
				AssertEquals("pack line 1 marks", "MARKS", helper.House.PackLines[0].CV_MarksAndNumbers);
				AssertEquals("pack line 1 DG count", 0, helper.House.PackLines[0].UNDGs.Count);

				helper.House.Shipment.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;
				AssertEquals("mode is blank because of inconsistent shipment data", ZString.Empty, helper.House.PackLines[0].CN_ContainerMode);

				helper.House.Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
				AssertEquals("mode reinstated", Core.Constants.ContainerModes.Containerised, helper.House.PackLines[0].CN_ContainerMode);

				helper.House.Shipment.JS_PackingMode = Core.Constants.ContainerModes.Bulk;
				AssertEquals("mode is blank because of inconsistent shipment data", ZString.Empty, helper.House.PackLines[0].CN_ContainerMode);

				packLine.Containers.RemoveAll();
				AssertEquals("mode is now correctly non-containerised", string.Empty, helper.House.PackLines[0].CN_ContainerMode);
			}
			finally
			{
				ValidationTestHelper.RemoveCarrierCodeFromCurrentCompany(Factory);
			}
		}
	}
}
