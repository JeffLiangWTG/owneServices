using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IN.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(CGMAsycudaBill))]
sealed class CGMAsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
{
	public void TestABL_OA_BuyerResData()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(Bill.ABL_OA_BuyerInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "Importer", resData?.Caption);
			AssertEquals("MediumCaption", "Importer", resData?.MediumCaption);
			AssertEquals("ShortCaption", "Imp.", resData?.ShortCaption);
		});
	}

	public void TestBuyerDetailsCaptions()
	{
		CombineAssertions(() =>
		{
			AssertCaption(Bill.ABL_BuyerNameInfo, "Importer Name");
			AssertCaption(Bill.ABL_BuyerStreet1Info, "Importer Street 1");
			AssertCaption(Bill.ABL_BuyerStreet2Info, "Importer Street 2");
			AssertCaption(Bill.ABL_BuyerCityInfo, "Importer City");
			AssertCaption(Bill.ABL_BuyerStateInfo, "Importer State");
			AssertCaption(Bill.ABL_BuyerPostcodeInfo, "Importer Postcode");
			AssertCaption(Bill.ABL_BuyerPhoneInfo, "Importer Phone");
			AssertCaption(Bill.ABL_RN_NKBuyerCountryInfo, "Importer Country / Region");
		});

		void AssertCaption(ZPropertyInfo propertyInfo, string expectedCaption)
		{
			var resData = DataBoundResourceStrings.GetDataForProperty(propertyInfo);
			AssertNotNull("Res string data", resData);
			AssertEquals($"{propertyInfo.Name} - Caption", expectedCaption, resData?.Caption);
		}
	}

	public void TestUnoCode()
	{
		AssertEquals("Initial", ZString.Empty, Bill.UnoCode);

		var dgSubstance = CreateAndAddDgSubstance(Bill);
		dgSubstance.DG_UNNO = "XYZ";
		AssertEquals("After DG substance", "XYZ", Bill.UnoCode);
	}

	public void TestImcoCode()
	{
		AssertEquals("Initial", ZString.Empty, Bill.ImcoCode);

		var dgSubstance = CreateAndAddDgSubstance(Bill);
		dgSubstance.DG_Class = "XYZ";
		AssertEquals("After DG substance", "XYZ", Bill.ImcoCode);
	}

	public void TestBondNumber()
	{
		Bill.BondNumber = "bond123";
		CombineAssertions(() =>
		{
			AssertEquals("bond123", Bill.GetSystemDefinedValue<ZString>(CGMAsycudaBill.Schema.BondNumber));
			Bill.SetSystemDefinedValue(CGMAsycudaBill.Schema.BondNumber, new ZString("bond456"));
			AssertEquals("bond456", Bill.BondNumber);
		});
	}

	public void TestBondNumberResData()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(Bill.BondNumberInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Bond Number", resData.Caption);
			AssertEquals("MediumCaption", "Bond No.", resData.MediumCaption);
			AssertEquals("ShortCaption", "Bond No.", resData.ShortCaption);
		});
	}

	public void TestBondNumberMaxLength()
	{
		AssertEquals("MaxLength", CGMAsycudaBill.Schema.BondNumberMaxLength, Bill.BondNumberInfo.MaxLength);
	}

	public void TestLookups() => AssertType<CGMAsycudaBillLookups>(Bill.Lookups);

	public void TestValidationForMasterChild() => AssertType<CGMAsycudaBillValidationForMasterChild>(Header.MasterBill.Validation);

	public void TestValidationForRegularBill() => AssertType<CGMAsycudaBillValidationForRegularBill>(Bill.Validation);

	public void TestABL_CargoStatusCaption()
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(Bill.ABL_CargoStatusInfo);
		CombineAssertions(() =>
		{
			AssertEquals("ABL_CargoStatus Caption", "Cargo Movement", resourceStringData.Caption);
			AssertEquals("ABL_CargoStatus Medium Caption", "Cargo Mov.", resourceStringData.MediumCaption);
			AssertEquals("ABL_CargoStatus Short Caption", "Cargo Mov.", resourceStringData.ShortCaption);
		});
	}

	public void TestABL_CarrierReferenceCaption()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(Bill.ABL_CarrierReferenceInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "Sub Line Number", resData?.Caption);
			AssertEquals("MediumCaption", "Sub-Line No.", resData?.MediumCaption);
			AssertEquals("ShortCaption", "S-Line No.", resData?.ShortCaption);
		});
	}

	public void TestABL_CarrierReferenceMaxLength()
	{
		AssertEquals("MaxLength", 4, Bill.ABL_CarrierReferenceInfo.MaxLength);
	}

	public void TestIsTranshipment()
	{
		CombineAssertions("When ABL_CargoStatus is", () =>
		{
			AssertEquals("Empty", false, Bill.IsTranshipment);

			Bill.ABL_CargoStatus = CargoMovementList.Codes.TranshipmentCargo;
			AssertEquals("TC", true, Bill.IsTranshipment);

			Bill.ABL_CargoStatus = CargoMovementList.Codes.LocalCargo;
			AssertEquals("LC", false, Bill.IsTranshipment);

			Bill.ABL_CargoStatus = CargoMovementList.Codes.TranshipmentToICDSMTP;
			AssertEquals("TI", true, Bill.IsTranshipment);

			Bill.ABL_CargoStatus = "XX";
			AssertEquals("Unknown", false, Bill.IsTranshipment);
		});
	}

	public void TestABL_BillIssueDate()
	{
		var resData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Bill.ABL_BillIssueDateInfo, CGMAsycudaManifestHeader.CaptionKeySea);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "HBL Date", resData.Caption);
			AssertEquals("MediumCaption", "HBL Dt.", resData.MediumCaption);
			AssertEquals("ShortCaption", "Dt.", resData.ShortCaption);
		});

		resData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Bill.ABL_BillIssueDateInfo, CGMAsycudaManifestHeader.CaptionKeyAir);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "HAWB Date", resData.Caption);
			AssertEquals("MediumCaption", "HAWB Dt.", resData.MediumCaption);
			AssertEquals("ShortCaption", "Dt.", resData.ShortCaption);
		});
	}

	public void TestMultipleKeysToUse()
	{
		Header.AMA_TransportMode = "AIR";
		CombineAssertions(() =>
		{
			AssertEquals("AIR", CGMAsycudaManifestHeader.CaptionKeyAir, ((ISupportMultipleResourceStringData)Bill).MultipleKeysToUse.Single());
			Header.AMA_TransportMode = "SEA";
			AssertEquals("SEA", CGMAsycudaManifestHeader.CaptionKeySea, ((ISupportMultipleResourceStringData)Bill).MultipleKeysToUse.Single());
		});
	}

	public void TestABL_SpecialCargoCode()
	{
		var resData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Bill.ABL_SpecialCargoCodeInfo, CGMAsycudaManifestHeader.CaptionKeySea);
		AssertEquals("Caption", "Item Type", resData.Caption);

		resData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Bill.ABL_SpecialCargoCodeInfo, CGMAsycudaManifestHeader.CaptionKeyAir);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Shipment Type", resData.Caption);
			AssertEquals("MediumCaption", "Shipment Type", resData.MediumCaption);
			AssertEquals("ShortCaption", "Ship. Type", resData.ShortCaption);
		});
	}

	public void TestABL_SpecialCargoCode_MaxLength()
	{
		Header.AMA_TransportMode = "AIR";
		CombineAssertions(() =>
		{
			AssertEquals("MaxLength for AIR", 1, Bill.ABL_SpecialCargoCodeInfo.MaxLength);
			Header.AMA_TransportMode = "SEA";
			AssertEquals("MaxLength for SEA", 2, Bill.ABL_SpecialCargoCodeInfo.MaxLength);
		});
	}

	public void TestConsigneeWhenImporterEntered()
	{
		var orgHeader = Factory.New<OrgHeader>();
		var orgAddress1 = orgHeader.MainAddress;
		var bill = Bill;
		bill.ABL_OA_Buyer = orgAddress1.PK;

		CombineAssertions(() =>
		{
			AssertEquals("When Consignee is empty and Importer entered", orgAddress1.PK, bill.ABL_OA_Consignee);
			var orgAddress2 = orgHeader.Addresses.AddNew();
			bill.ABL_OA_Buyer = orgAddress2.PK;
			AssertEquals("When Consignee is filled and Importer entered", orgAddress1.PK, bill.ABL_OA_Consignee);
		});
	}

	public void TestPortOfOriginCode()
	{
		RefDataSetupTestHelper.SetupUNLOCOData(Factory);
		CombineAssertions(() =>
		{
			Header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			Bill.ABL_RL_NKOrigin = "INLMN";
			AssertEquals("Transport AIR and IATA not present", "LMN", Bill.PortOfOriginCode);

			Bill.ABL_RL_NKOrigin = "INABC";
			AssertEquals("Transport AIR and IATA present", "XYZ", Bill.PortOfOriginCode);

			Bill.ABL_RL_NKOrigin = "INDEF";
			AssertEquals("Transport AIR and Port not present", "DEF", Bill.PortOfOriginCode);

			Header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Transport SEA", "INDEF", Bill.PortOfOriginCode);
		});
	}

	public void TestPortOfFinalDestinationCode()
	{
		RefDataSetupTestHelper.SetupUNLOCOData(Factory);
		CombineAssertions(() =>
		{
			Header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			Bill.ABL_RL_NKFinalDestination = "INLMN";
			AssertEquals("Transport AIR and IATA not present", "LMN", Bill.PortOfFinalDestinationCode);

			Bill.ABL_RL_NKFinalDestination = "INABC";
			AssertEquals("Transport AIR and IATA present", "XYZ", Bill.PortOfFinalDestinationCode);

			Bill.ABL_RL_NKFinalDestination = "INDEF";
			AssertEquals("Transport AIR and Port not present", "DEF", Bill.PortOfFinalDestinationCode);

			Header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Transport SEA", "INDEF", Bill.PortOfFinalDestinationCode);
		});
	}

	public void TestABL_RL_NKOriginResData()
	{
		var resData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Bill.ABL_RL_NKOriginInfo, CGMAsycudaManifestHeader.CaptionKeyAir);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Port Of Origin", resData.Caption);
			AssertEquals("MediumCaption", "Origin Port", resData.MediumCaption);
			AssertEquals("ShortCaption", "Origin", resData.ShortCaption);
		});
	}

	public void TestABL_ManifestQtyResData()
	{
		var resData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Bill.ABL_ManifestQtyInfo, CGMAsycudaManifestHeader.CaptionKeyAir);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Packages", resData.Caption);
			AssertEquals("MediumCaption", "Packages", resData.MediumCaption);
			AssertEquals("ShortCaption", "Pack.", resData.ShortCaption);
		});
	}

	public void TestABL_GrossWeightResData()
	{
		var resData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Bill.ABL_GrossWeightInfo, CGMAsycudaManifestHeader.CaptionKeyAir);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Gross Weight", resData.Caption);
			AssertEquals("MediumCaption", "Gross Wt.", resData.MediumCaption);
			AssertEquals("ShortCaption", "Gross Wt.", resData.ShortCaption);
		});
	}

	public void TestABL_VolumeResData()
	{
		var resData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Bill.ABL_VolumeInfo, CGMAsycudaManifestHeader.CaptionKeyAir);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Volume", resData.Caption);
			AssertEquals("MediumCaption", "Volume", resData.MediumCaption);
			AssertEquals("ShortCaption", "Vol.", resData.ShortCaption);
		});
	}

	public void TestABL_BillNumberResData()
	{
		var resData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Bill.ABL_BillNumberInfo, AsycudaManifestHeader.CaptionKeySea);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "HBL Number", resData.Caption);
			AssertEquals("MediumCaption", "HBL Num.", resData.MediumCaption);
			AssertEquals("ShortCaption", "HBL", resData.ShortCaption);
		});

		resData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Bill.ABL_BillNumberInfo, AsycudaManifestHeader.CaptionKeyAir);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "HAWB Number", resData.Caption);
			AssertEquals("MediumCaption", "HAWB Num.", resData.MediumCaption);
			AssertEquals("ShortCaption", "HAWB", resData.ShortCaption);
		});
	}

	public void TestBillNumberMaxLength()
	{
		AssertEquals("MaxLength", CGMAsycudaBill.Schema.BillNumberMaxLength, Bill.ABL_BillNumberInfo.MaxLength);
	}

	public void TestABL_ContainerMode()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(Bill.ABL_ContainerModeInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Nature Of Cargo", resData.Caption);
			AssertEquals("MediumCaption", "Nat. Of Cargo", resData.MediumCaption);
			AssertEquals("ShortCaption", "N.O.C.", resData.ShortCaption);
		});
	}

	public void TestIsContainerModeCorCP()
	{
		CombineAssertions(() =>
		{
			Bill.ABL_ContainerMode = NatureOfCargoList.Codes.C;
			AssertEquals("C", true, Bill.IsContainerModeCorCP());
			Bill.ABL_ContainerMode = NatureOfCargoList.Codes.CP;
			AssertEquals("CP", true, Bill.IsContainerModeCorCP());
			Bill.ABL_ContainerMode = NatureOfCargoList.Codes.LB;
			AssertEquals("other", false, Bill.IsContainerModeCorCP());
		});
	}

	public void TestABL_OH_BondHolder()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(Bill.ABL_OH_BondHolderInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Bond Holder", resData.Caption);
			AssertEquals("MediumCaption", "Bond Holder", resData.MediumCaption);
			AssertEquals("ShortCaption", "Holder", resData.ShortCaption);
		});
	}

	public void TestMLOCode()
	{
		var holder = Factory.NewWithValidTestData<OrgHeader>();
		holder.OH_Code = "ABCD";
		var code = holder.CustomsCodes.AddNew();
		code.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
		code.OK_RN_NKCodeCountry = CountryCodes.India;
		code.OK_CustomsRegNo = "1234";
		Factory.Save();
		var resData = DataBoundResourceStrings.GetDataForProperty(Bill.MLOCodeInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "MLO Code", resData.Caption);
			AssertEquals("MediumCaption", "MLO Code", resData.MediumCaption);
			AssertEquals("ShortCaption", "MLO", resData.ShortCaption);

			AssertEquals("Should be empty when no BondHolder", ZString.Empty, bill.MLOCode);
			Bill.ABL_OH_BondHolder = holder.PK;
			AssertEquals("1234", bill.MLOCode);
		});
	}

	public void TestABL_MarksAndNumbersMaxLength()
	{
		AssertEquals("MaxLength", CGMAsycudaBill.Schema.MarksAndNumbersMaxLength, Bill.ABL_MarksAndNumbersInfo.MaxLength);
	}

	public void TestABL_LocationInformation()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(Bill.ABL_LocationInformationInfo);
		AssertEquals("Caption", "Final Destination", resData.Caption);
	}

	public void TestABL_CustomsFinalDestinationPortMaxLength()
	{
		AssertEquals("MaxLength", CGMAsycudaBill.Schema.CustomsFinalDestinationPortMaxLength, Bill.ABL_CustomsFinalDestinationPortInfo.MaxLength);
	}

	public void TestFinalDestinationIsCustomsHouse()
	{
		Bill.ABL_LocationInformation = DestinationCodeList.Codes.CUS;
		CombineAssertions(() =>
		{
			Assert("CUS", Bill.FinalDestinationIsCustomsHouse);
			Bill.ABL_LocationInformation = DestinationCodeList.Codes.CFS;
			Assert("not CUS", !Bill.FinalDestinationIsCustomsHouse);
		});
	}

	public void TestFinalDestinationIsCFS()
	{
		Bill.ABL_LocationInformation = DestinationCodeList.Codes.CFS;
		CombineAssertions(() =>
		{
			Assert("CFS", Bill.FinalDestinationIsCFS);
			Bill.ABL_LocationInformation = DestinationCodeList.Codes.CUS;
			Assert("not CFS", !Bill.FinalDestinationIsCFS);
		});
	}

	public void TestABL_InlandTransportMode()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(Bill.ABL_InlandTransportModeInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Mode Of Transport", resData.Caption);
			AssertEquals("MediumCaption", "M.O.T", resData.MediumCaption);
			AssertEquals("ShortCaption", "M.O.T", resData.ShortCaption);
		});
	}

	public void TestLocalTransportCarrierCaption()
	{
		Bill.ABL_InlandTransportMode = ModeOfTransportList.Codes.Train;
		var resStr = Bill.LocalTransportCarrierCaption;
		CombineAssertions(() =>
		{
			AssertEquals("Caption - T", "Rail Operator", resStr.FullDescription);
			AssertEquals("MediumCaption - T", "Rail Opt.", resStr.Caption);
			AssertEquals("ShortCaption - T", "Rail", resStr.ShortCaption);

			Bill.ABL_InlandTransportMode = ModeOfTransportList.Codes.Road;
			resStr = Bill.LocalTransportCarrierCaption;
			AssertEquals("Caption - R", "Transporter", resStr.Caption);

			Bill.ABL_InlandTransportMode = ModeOfTransportList.Codes.Ship;
			resStr = Bill.LocalTransportCarrierCaption;
			AssertEquals("Caption - S", "Carrier", resStr.Caption);
		});
	}

	public void TestCarrierCode()
	{
		var carrier = Factory.New<OrgHeader>();
		carrier.OH_Code = "ABCD";
		carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "1234", CountryCodes.India);

		var resData = DataBoundResourceStrings.GetDataForProperty(Bill.CarrierCodeInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Carrier Code", resData.Caption);
			AssertEquals("Should be empty when no LocalTransportCarrier", ZString.Empty, bill.CarrierCode);
			Bill.ABL_OH_LocalTransportCarrier = carrier.PK;
			AssertEquals("Should be empty when not transshipment", ZString.Empty, bill.CarrierCode);
			Bill.ABL_CargoStatus = CargoMovementList.Codes.TranshipmentCargo;
			AssertEquals("When transhipment cargo and Local transport carrier present", "1234", bill.CarrierCode);
		});
	}

	public void TestABL_BillStatus()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(Bill.ABL_BillStatusInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Action", resData.Caption);
			AssertEquals("MediumCaption", "Action", resData.MediumCaption);
			AssertEquals("ShortCaption", "Act.", resData.ShortCaption);
		});
	}

	public void TestMessageStatusDescription()
	{
		var info = Bill.MessageStatusDescriptionInfo;
		var resData = DataBoundResourceStrings.GetDataForProperty(info);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Status", resData.Caption);
			Assert(info.ReadOnly);
		});

		Bill.ABL_MessageStatus = BillMessageStatusList.Codes.Accepted;
		AssertEquals("MessageStatusDescription", BillMessageStatusList.Descriptions.Accepted, Bill.MessageStatusDescription);
		Bill.ABL_MessageStatus = BillMessageStatusList.Codes.Deleted;
		AssertEquals("MessageStatusDescription", BillMessageStatusList.Descriptions.Deleted, Bill.MessageStatusDescription);
	}

	public void TestDataVersionLogs()
	{
		var dataVersionLoggingSupported = (IDataVersionLoggingSupported)Bill;
		CombineAssertions(() =>
		{
			AssertEquals("IsDataVersionsAutoLogged", true, dataVersionLoggingSupported.IsDataVersionsAutoLogged);
			AssertNotNull("DataVersionLogValueFormatter", dataVersionLoggingSupported.DataVersionLogValueFormatter);
		});
	}

	public void TestOnSaving_BillStatusAmendment()
	{
		CombineAssertions(() =>
		{
			AssertEquals("BillMessageStatus empty", BillActionList.Codes.Fresh, Bill.ABL_BillStatus);
			Factory.Save();

			Bill.ABL_MessageStatus = BillMessageStatusList.Codes.Accepted;
			Factory.Save();
			AssertEquals("BillMessageStatus updated to 'ACC'", BillActionList.Codes.Fresh, Bill.ABL_BillStatus);

			AssertSetAmendmentInDifferentTransportMode(TransportTypeList.Codes.Sea, BillActionList.Codes.Fresh);
			AssertSetAmendmentInDifferentTransportMode(TransportTypeList.Codes.Air, BillActionList.Codes.Amendment);

			var bill = Header.Bills.AddNew();
			bill.ABL_BillStatus = BillActionList.Codes.Supplementary;
			Factory.Save();
			AssertEquals("BillMessageStatus empty", BillActionList.Codes.Supplementary, bill.ABL_BillStatus);

			bill.ABL_MessageStatus = BillMessageStatusList.Codes.Accepted;
			Factory.Save();
			AssertEquals("BillMessageStatus updated to 'ACC'", BillActionList.Codes.Supplementary, bill.ABL_BillStatus);

			Header.Action = ManifestMessageTypeList.Codes.Delete;
			Factory.Save();
			AssertEquals("House Bill", BillActionList.Codes.Delete, bill.ABL_BillStatus);
		});

		void AssertSetAmendmentInDifferentTransportMode(ZString transportMode, ZString billStatus)
		{
			Header.AMA_TransportMode = transportMode;
			var bill = Header.Bills.AddNew();
			bill.ABL_MessageStatus = BillMessageStatusList.Codes.Accepted;
			Factory.Save();

			bill.ABL_BillNumber = "1";
			Factory.Save();
			AssertEquals("BillMessageStatus 'ACC', ABL_BillStatus 'F' and changes in ABL_BillNumber", billStatus, bill.ABL_BillStatus);
			bill.ABL_BillStatus = ManifestMessageTypeList.Codes.Fresh;

			bill.ABL_BillIssueDate = new ZDate(2024, 09, 11);
			Factory.Save();
			AssertEquals("BillMessageStatus 'ACC', ABL_BillStatus 'F' and changes in ABL_BillIssueDate", billStatus, bill.ABL_BillStatus);
			bill.ABL_BillStatus = ManifestMessageTypeList.Codes.Fresh;

			bill.ABL_RL_NKOrigin = "XY";
			Factory.Save();
			AssertEquals("BillMessageStatus 'ACC', ABL_BillStatus 'F' and changes in ABL_RL_NKOrigin", billStatus, bill.ABL_BillStatus);
			bill.ABL_BillStatus = ManifestMessageTypeList.Codes.Fresh;

			bill.ABL_RL_NKFinalDestination = "XY";
			Factory.Save();
			AssertEquals("BillMessageStatus 'ACC', ABL_BillStatus 'F' and changes in ABL_RL_NKFinalDestination", billStatus, bill.ABL_BillStatus);
			bill.ABL_BillStatus = ManifestMessageTypeList.Codes.Fresh;

			bill.ABL_SpecialCargoCode = "P";
			Factory.Save();
			AssertEquals("BillMessageStatus 'ACC', ABL_BillStatus 'F' and changes in ABL_SpecialCargoCode", billStatus, bill.ABL_BillStatus);
			bill.ABL_BillStatus = ManifestMessageTypeList.Codes.Fresh;

			bill.ABL_ManifestQty = 12;
			Factory.Save();
			AssertEquals("BillMessageStatus 'ACC', ABL_BillStatus 'F' and changes in ABL_ManifestQty", billStatus, bill.ABL_BillStatus);
			bill.ABL_BillStatus = ManifestMessageTypeList.Codes.Fresh;

			bill.ABL_ManifestUQ = "BAG";
			Factory.Save();
			AssertEquals("BillMessageStatus 'ACC', ABL_BillStatus 'F' and changes in ABL_ManifestUQ", billStatus, bill.ABL_BillStatus);
			bill.ABL_BillStatus = ManifestMessageTypeList.Codes.Fresh;

			bill.ABL_GrossWeight = 12m;
			Factory.Save();
			AssertEquals("BillMessageStatus 'ACC', ABL_BillStatus 'F' and changes in ABL_GrossWeight", billStatus, bill.ABL_BillStatus);
			bill.ABL_BillStatus = ManifestMessageTypeList.Codes.Fresh;

			bill.ABL_GrossWeightUQ = "G";
			Factory.Save();
			AssertEquals("BillMessageStatus 'ACC', ABL_BillStatus 'F' and changes in ABL_GrossWeightUQ", billStatus, bill.ABL_BillStatus);
			bill.ABL_BillStatus = ManifestMessageTypeList.Codes.Fresh;

			bill.ABL_GoodsDescription = "XYZ";
			Factory.Save();
			AssertEquals("BillMessageStatus 'ACC', ABL_BillStatus 'F' and changes in ABL_GoodsDescription", billStatus, bill.ABL_BillStatus);
		}
	}

	public void TestOnSaving_BillStatusSupplementary()
	{
		CombineAssertions(() =>
		{
			Header.AMA_MessageStatus = MessageStatusList.Codes.MessageAccepted;
			Header.RegistrationStatus = RegistrationStatusList.Codes.ManifestRegistered;
			Bill.ABL_MessageStatus = BillMessageStatusList.Codes.Error;
			Factory.Save();
			AssertEquals("BillMessageStatus updated to 'ERR'", BillActionList.Codes.Supplementary, Bill.ABL_BillStatus);

			AssertSetSupplementaryInDifferentTransportMode(TransportTypeList.Codes.Sea, BillActionList.Codes.Fresh, BillMessageStatusList.Codes.Error);
			AssertSetSupplementaryInDifferentTransportMode(TransportTypeList.Codes.Air, BillActionList.Codes.Supplementary, string.Empty);
		});

		void AssertSetSupplementaryInDifferentTransportMode(ZString transportMode, ZString billStatus, ZString messageStatus)
		{
			Header.AMA_TransportMode = transportMode;
			var bill = Header.Bills.AddNew();
			bill.ABL_MessageStatus = BillMessageStatusList.Codes.Error;
			bill.ABL_BillStatus = ManifestMessageTypeList.Codes.Fresh;
			Factory.Save();

			bill.ABL_BillNumber = "1";
			Factory.Save();
			AssertEquals("BillMessageStatus 'ERR', ABL_BillStatus 'F' and changes in ABL_BillNumber", billStatus, bill.ABL_BillStatus);
			AssertEquals("BillMessageStatus 'ERR', ABL_BillStatus 'F' and changes in ABL_BillNumber", messageStatus, bill.ABL_MessageStatus);
			bill.ABL_BillStatus = ManifestMessageTypeList.Codes.Fresh;
			bill.ABL_MessageStatus = BillMessageStatusList.Codes.Error;

			bill.ABL_BillIssueDate = new ZDate(2024, 09, 11);
			Factory.Save();
			AssertEquals("BillMessageStatus 'ERR', ABL_BillStatus 'F' and changes in ABL_BillIssueDate", billStatus, bill.ABL_BillStatus);
			AssertEquals("BillMessageStatus 'ERR', ABL_BillStatus 'F' and changes in ABL_BillIssueDate", messageStatus, bill.ABL_MessageStatus);
			bill.ABL_BillStatus = ManifestMessageTypeList.Codes.Fresh;
			bill.ABL_MessageStatus = BillMessageStatusList.Codes.Error;

			bill.ABL_RL_NKOrigin = "XY";
			Factory.Save();
			AssertEquals("BillMessageStatus 'ERR', ABL_BillStatus 'F' and changes in ABL_RL_NKOrigin", billStatus, bill.ABL_BillStatus);
			AssertEquals("BillMessageStatus 'ERR', ABL_BillStatus 'F' and changes in ABL_RL_NKOrigin", messageStatus, bill.ABL_MessageStatus);
			bill.ABL_BillStatus = ManifestMessageTypeList.Codes.Fresh;
			bill.ABL_MessageStatus = BillMessageStatusList.Codes.Error;

			bill.ABL_RL_NKFinalDestination = "XY";
			Factory.Save();
			AssertEquals("BillMessageStatus 'ERR', ABL_BillStatus 'F' and changes in ABL_RL_NKFinalDestination", billStatus, bill.ABL_BillStatus);
			AssertEquals("BillMessageStatus 'ERR', ABL_BillStatus 'F' and changes in ABL_RL_NKFinalDestination", messageStatus, bill.ABL_MessageStatus);
			bill.ABL_BillStatus = ManifestMessageTypeList.Codes.Fresh;
			bill.ABL_MessageStatus = BillMessageStatusList.Codes.Error;

			bill.ABL_SpecialCargoCode = "P";
			Factory.Save();
			AssertEquals("BillMessageStatus 'ERR', ABL_BillStatus 'F' and changes in ABL_SpecialCargoCode", billStatus, bill.ABL_BillStatus);
			AssertEquals("BillMessageStatus 'ERR', ABL_BillStatus 'F' and changes in ABL_SpecialCargoCode", messageStatus, bill.ABL_MessageStatus);
			bill.ABL_BillStatus = ManifestMessageTypeList.Codes.Fresh;
			bill.ABL_MessageStatus = BillMessageStatusList.Codes.Error;

			bill.ABL_ManifestQty = 12;
			Factory.Save();
			AssertEquals("BillMessageStatus 'ERR', ABL_BillStatus 'F' and changes in ABL_ManifestQty", billStatus, bill.ABL_BillStatus);
			AssertEquals("BillMessageStatus 'ERR', ABL_BillStatus 'F' and changes in ABL_ManifestQty", messageStatus, bill.ABL_MessageStatus);
			bill.ABL_BillStatus = ManifestMessageTypeList.Codes.Fresh;
			bill.ABL_MessageStatus = BillMessageStatusList.Codes.Error;

			bill.ABL_ManifestUQ = "BAG";
			Factory.Save();
			AssertEquals("BillMessageStatus 'ERR', ABL_BillStatus 'F' and changes in ABL_ManifestUQ", billStatus, bill.ABL_BillStatus);
			AssertEquals("BillMessageStatus 'ERR', ABL_BillStatus 'F' and changes in ABL_ManifestUQ", messageStatus, bill.ABL_MessageStatus);
			bill.ABL_BillStatus = ManifestMessageTypeList.Codes.Fresh;
			bill.ABL_MessageStatus = BillMessageStatusList.Codes.Error;

			bill.ABL_GrossWeight = 12m;
			Factory.Save();
			AssertEquals("BillMessageStatus 'ERR', ABL_BillStatus 'F' and changes in ABL_GrossWeight", billStatus, bill.ABL_BillStatus);
			AssertEquals("BillMessageStatus 'ERR', ABL_BillStatus 'F' and changes in ABL_GrossWeight", messageStatus, bill.ABL_MessageStatus);
			bill.ABL_BillStatus = ManifestMessageTypeList.Codes.Fresh;
			bill.ABL_MessageStatus = BillMessageStatusList.Codes.Error;

			bill.ABL_GrossWeightUQ = "G";
			Factory.Save();
			AssertEquals("BillMessageStatus 'ERR', ABL_BillStatus 'F' and changes in ABL_GrossWeightUQ", billStatus, bill.ABL_BillStatus);
			AssertEquals("BillMessageStatus 'ERR', ABL_BillStatus 'F' and changes in ABL_GrossWeightUQ", messageStatus, bill.ABL_MessageStatus);
			bill.ABL_BillStatus = ManifestMessageTypeList.Codes.Fresh;
			bill.ABL_MessageStatus = BillMessageStatusList.Codes.Error;

			bill.ABL_GoodsDescription = "XYZ";
			Factory.Save();
			AssertEquals("BillMessageStatus 'ERR', ABL_BillStatus 'F' and changes in ABL_GoodsDescription", billStatus, bill.ABL_BillStatus);
			AssertEquals("BillMessageStatus 'ERR', ABL_BillStatus 'F' and changes in ABL_GoodsDescription", messageStatus, bill.ABL_MessageStatus);
		}
	}

	public void TestABL_BillStatus_ReadOnly()
	{
		CombineAssertions(() =>
		{
			Bill.ABL_MessageStatus = "ABC";
			AssertEquals("BillMessageStatus not empty", false, Bill.ABL_BillStatusInfo.ReadOnly);

			Bill.ABL_MessageStatus = ZString.Empty;
			AssertEquals("BillMessageStatus, MessageStatus and RegistrationStatus are empty", true, Bill.ABL_BillStatusInfo.ReadOnly);

			Header.AMA_MessageStatus = IN.Business.MessageStatusList.Codes.MessageAccepted;
			AssertEquals("BillMessageStatus, RegistrationStatus are empty and MessageStatus 'ACC'", false, Bill.ABL_BillStatusInfo.ReadOnly);

			Header.RegistrationStatus = RegistrationStatusList.Codes.ManifestRegistered;
			AssertEquals("BillMessageStatus, MessageStatus 'ACC' and RegistrationStatus 'REG", true, Bill.ABL_BillStatusInfo.ReadOnly);

			Header.AMA_MessageStatus = "ABC";
			Header.RegistrationStatus = "XYZ";
			AssertEquals("BillMessageStatus, RegistrationStatus are empty and MessageStatus 'ACC'", false, Bill.ABL_BillStatusInfo.ReadOnly);

			Header.Action = ManifestMessageTypeList.Codes.Delete;
			Factory.Save();

			AssertEquals("Action = Delete in db", true, Bill.ABL_BillStatusInfo.ReadOnly);
			AssertEquals("Master Bill", false, Header.MasterBill.ABL_BillStatusInfo.ReadOnly);
		});
	}

	public void TestPacks()
	{
		AssertType<CGMAsycudaPackCollection>("Type", Bill.Packs);
	}

	public void TestGetPackType()
	{
		AssertEquals("Type", typeof(CGMAsycudaPack), Bill.GetPackType());
	}

	public void TestABL_ManifestUQSetsPackAPA_WeightUQ()
	{
		var pack1 = Bill.Packs.AddNew();
		var pack2 = Bill.Packs.AddNew();
		Bill.ABL_GrossWeightUQ = "KG";
		CombineAssertions("APA_WeightUQ", () =>
		{
			AssertEquals("Pack 1", "KG", pack1.APA_WeightUQ);
			AssertEquals("Pack 2", "KG", pack2.APA_WeightUQ);
		});
	}

	public void TestABL_BillNumber_ReadOnly()
	{
		CombineAssertions(() =>
		{
			Bill.Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			Bill.ABL_MessageStatus = "ACK";
			AssertReadOnlyProperty(Bill.ABL_BillNumberInfo, false);

			Bill.Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			Bill.ABL_MessageStatus = ZString.Empty;
			AssertReadOnlyProperty(Bill.ABL_BillNumberInfo, false);

			Bill.Header.AMA_TransportMode = TransportTypeList.Codes.Air;
			Bill.ABL_MessageStatus = "ACK";
			AssertReadOnlyProperty(Bill.ABL_BillNumberInfo, true);

			Bill.Header.AMA_TransportMode = TransportTypeList.Codes.Air;
			Bill.ABL_MessageStatus = ZString.Empty;
			AssertReadOnlyProperty(Bill.ABL_BillNumberInfo, false);
		});
	}

	public void TestSynchronisePaymentType()
	{
		Assert(Bill.SynchronisePaymentType);
	}

	void AssertReadOnlyProperty(ZPropertyInfo info, bool expectedReadOnly)
	{
		ZString message = $"The readonly of {info.Name} should be {expectedReadOnly}.";
		AssertEquals(message, expectedReadOnly, info.ReadOnly);
	}

	public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
	{
		return new List<ZString>() { AsycudaBill.Schema.ABL_SpecialCargoCode };
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return Bill;
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.New<CGMAsycudaManifestHeader>().Bills.AddNew();

	UNDGSubstance CreateAndAddDgSubstance(CGMAsycudaBill bill)
	{
		var dgSubstance = Factory.New<UNDGSubstance>();
		var undg = bill.Packs.AddNew().UNDGs.AddNew();
		undg.DI_DG = dgSubstance.PK;
		return dgSubstance;
	}

	CGMAsycudaBill Bill => bill ??= Header.Bills.AddNew();
	CGMAsycudaBill bill;

	CGMAsycudaManifestHeader Header => header ??= Factory.New<CGMAsycudaManifestHeader>();
	CGMAsycudaManifestHeader header;
}
