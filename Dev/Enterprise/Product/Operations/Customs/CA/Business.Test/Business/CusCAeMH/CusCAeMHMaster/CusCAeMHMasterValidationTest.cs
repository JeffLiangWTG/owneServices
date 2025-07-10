using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHMasterValidationTest : BusinessObjectValidationTestCase
	{
		[TestDate(2013, 8, 15, 10, 30, 25)]
		public void TestCheckBP_AmendReasonCode()
		{
			master.BP_AmendReasonCode = "XX";
			master.BP_ATA = ZDateTime.Now;

			AssertHasMessageErrorContaining(master.BP_AmendReasonCodeInfo, ListValidation.InvalidCodeMessageError);

			master.BP_AmendReasonCode = EManifestAmendmentReasonCodes.Codes.ClientOutage;

			AssertNoMessageErrorContaining(master.BP_AmendReasonCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(master.BP_AmendReasonCodeInfo, MandatoryValidation.DoNotEntered);

			master.BP_AmendReasonCode = ZString.Empty;

			AssertNoMessageErrorContaining(master.BP_AmendReasonCodeInfo, MandatoryValidation.DoNotEntered);
			AssertNoMessageErrorContaining(master.BP_AmendReasonCodeInfo, "an amendment reason will be required");

			var message = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, ZDateTime.Empty, contextNodeXml: "<Context><Type>Status</Type><Value>0002</Value></Context><Context><Type>Status</Type><Value>0011</Value></Context>");
			var stmAlog = Factory.New<StmALog>();
			using (stmAlog.LockForUpdatingKeyFieldsForTesting())
			{
				stmAlog.SL_Parent = master.PK;
				stmAlog.SL_Table = master.TableName;
			}
			var genPivot = Factory.New<GenPivot>();
			genPivot.XX_Relation1ID = stmAlog.PK;
			genPivot.XX_Relation2ID = message.PK;
			genPivot.XX_RelationType = Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage;

			Factory.Save();
			master.MessagesForDisplay.Reload(true);

			master.Validation.ValidateBP_AmendReasonCode();

			AssertNoMessageErrorContaining(master.BP_AmendReasonCodeInfo, "an amendment reason will be required");

			master.BP_CustomsStatus = EManifestForwarderJobStatusList.Codes.Validated;
			master.Validation.ValidateBP_AmendReasonCode();

			AssertHasMessageErrorContaining(master.BP_AmendReasonCodeInfo, "an amendment reason will be required");

			master.BP_AmendReasonCode = EManifestAmendmentReasonCodes.Codes.ClientOutage;

			AssertNoMessageErrorContaining(master.BP_AmendReasonCodeInfo, "an amendment reason will be required");

			master.BP_CustomsStatus = ZString.Empty;
			master.BP_AmendReasonCode = ZString.Empty;
			message = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, ZDateTime.Empty, contextNodeXml: "<Context><Type>Status</Type><Value>0002</Value></Context><Context><Type>Status</Type><Value>0010</Value></Context>");
			stmAlog = Factory.New<StmALog>();
			using (stmAlog.LockForUpdatingKeyFieldsForTesting())
			{
				stmAlog.SL_Parent = master.PK;
				stmAlog.SL_Table = master.TableName;
			}
			genPivot = Factory.New<GenPivot>();
			genPivot.XX_Relation1ID = stmAlog.PK;
			genPivot.XX_Relation2ID = message.PK;

			Factory.Save();

			master.Validation.ValidateBP_AmendReasonCode();

			AssertNoMessageErrorContaining(master.BP_AmendReasonCodeInfo, "an amendment reason will be required");

			master.BP_CustomsStatus = EManifestForwarderJobStatusList.Codes.Validated;
			master.Validation.ValidateBP_AmendReasonCode();

			AssertHasMessageErrorContaining(master.BP_AmendReasonCodeInfo, "an amendment reason will be required");

			master.BP_AmendReasonCode = EManifestAmendmentReasonCodes.Codes.ClientOutage;

			AssertNoMessageErrorContaining(master.BP_AmendReasonCodeInfo, "an amendment reason will be required");
		}

		[TestDate(2013, 8, 15, 10, 30, 25)]
		public void TestCheckBP_AmendReasonCode_PortOrSubLocation()
		{
			var messageError = "There is no Arrival message / event or the arrival date present – hence eManifest cannot be amended, if there is any change in information then please submit Change – not Amendment.";
			master.BP_AmendReasonCode = EManifestAmendmentReasonCodes.Codes.PortOrSubLocation;
			AssertHasMessageErrorContaining(master.BP_AmendReasonCodeInfo, messageError);

			master.BP_ATA = ZDateTime.Now;
			master.Validation.ValidateBP_AmendReasonCode();
			AssertNoMessageErrorContaining(master.BP_AmendReasonCodeInfo, messageError);

			master.BP_ATA = ZDateTime.Empty;
			master.BP_AmendReasonCode = EManifestAmendmentReasonCodes.Codes.ClientOutage;
			AssertHasMessageErrorContaining(master.BP_AmendReasonCodeInfo, messageError);

			master.BP_AmendReasonCode = ZString.Empty;
			AssertNoMessageErrorContaining(master.BP_AmendReasonCodeInfo, messageError);
		}

		public void TestCheckBP_PrimaryCCN()
		{
			master.Validation.ValidateBP_PrimaryCCN();
			AssertHasMessageErrorContaining(master.BP_PrimaryCCNInfo, MandatoryValidation.YouHaveNotEntered);
			master.BP_MasterBill = "555";
			master.BP_MasterHouseCCN = "1234";
			master.BP_PrimaryCCN = "ABCD";
			AssertNoMessageErrorContaining(master.BP_PrimaryCCNInfo, MandatoryValidation.YouHaveNotEntered);
			var master1 = Factory.New<CusCAeMHMaster>();
			master1.BP_MasterHouseCCN = "1234";
			master1.BP_PrimaryCCN = "ABCD";
			AssertHasMessageError(master1.BP_PrimaryCCNInfo, CusCAeMHMasterValidation.CombinationOfPrimaryAndSubCCNMustBeUnique("555"));

			master.BP_MasterHouseCCN = ZString.Empty;
			Factory.Save();
			master.BP_PrimaryCCN = "8123CCN2";
			AssertNoWarningContaining(master.BP_PrimaryCCNInfo, "This is a key field");
			master.BP_CustomsStatus = EManifestForwarderJobStatusList.Codes.Error;
			Factory.Save();
			master.BP_PrimaryCCN = "8123CCN3";
			AssertHasWarningContaining(master.BP_PrimaryCCNInfo, "This is a key field");
			master.BP_MasterHouseCCN = "123456";
			Factory.Save();
			master.BP_PrimaryCCN = "8123CCN4";
			AssertNoWarningContaining(master.BP_PrimaryCCNInfo, "This is a key field");
			master.BP_MasterHouseCCN = ZString.Empty;
			master.BP_CustomsStatus = EManifestForwarderJobStatusList.Codes.Cancelled;
			Factory.Save();
			master.BP_PrimaryCCN = "8123CCN5";
			AssertNoWarningContaining(master.BP_PrimaryCCNInfo, "This is a key field");
		}

		public void TestCheckMasterBillNumberAndPrimaryCCNMatches()
		{
			master.BP_ModeOfTransport = TransportTypeList.Codes.Air;
			master.BP_MasterBill = "12341234";
			master.BP_PrimaryCCN = "12341234";
			AssertNoWarning(master.BP_MasterBillInfo, CusCAeMHMasterValidation.MasterBillNotMatchPrimaryCCNMessage);
			master.BP_PrimaryCCN = "12341-234";
			AssertNoWarning(master.BP_MasterBillInfo, CusCAeMHMasterValidation.MasterBillNotMatchPrimaryCCNMessage);
			master.BP_MasterBill = "123412-34";
			AssertNoWarning(master.BP_MasterBillInfo, CusCAeMHMasterValidation.MasterBillNotMatchPrimaryCCNMessage);
			master.BP_MasterBill = "12341234";
			master.BP_PrimaryCCN = "952712341234";
			AssertHasWarning(master.BP_MasterBillInfo, CusCAeMHMasterValidation.MasterBillNotMatchPrimaryCCNMessage);

			master.BP_ModeOfTransport = TransportTypeList.Codes.Sea;
			master.BP_MasterBill = "123-41234";
			master.BP_PrimaryCCN = "952712341234";
			AssertHasWarning(master.BP_MasterBillInfo, CusCAeMHMasterValidation.MasterBillNotMatchPrimaryCCNMessage);
			master.BP_MasterBill = "12341234";
			master.BP_PrimaryCCN = "43214321";
			AssertHasWarning(master.BP_MasterBillInfo, CusCAeMHMasterValidation.MasterBillNotMatchPrimaryCCNMessage);
			master.BP_MasterBill = "12341234";
			master.BP_PrimaryCCN = "234";
			AssertHasWarning(master.BP_MasterBillInfo, CusCAeMHMasterValidation.MasterBillNotMatchPrimaryCCNMessage);
			master.BP_MasterBill = "12341234";
			master.BP_PrimaryCCN = "952712341234";
			AssertNoWarning(master.BP_MasterBillInfo, CusCAeMHMasterValidation.MasterBillNotMatchPrimaryCCNMessage);
			master.BP_MasterBill = "APLU12341234";
			master.BP_PrimaryCCN = "12341234";
			AssertNoWarning(master.BP_MasterBillInfo, CusCAeMHMasterValidation.MasterBillNotMatchPrimaryCCNMessage);
			master.BP_MasterBill = "12341234";
			master.BP_PrimaryCCN = "9527112341234";
			AssertHasWarning(master.BP_MasterBillInfo, CusCAeMHMasterValidation.MasterBillNotMatchPrimaryCCNMessage);
			master.BP_MasterBill = "APLU112341234";
			master.BP_PrimaryCCN = "12341234";
			AssertHasWarning(master.BP_MasterBillInfo, CusCAeMHMasterValidation.MasterBillNotMatchPrimaryCCNMessage);
			master.BP_MasterBill = "APLU12341234";
			master.BP_PrimaryCCN = "952712341234";
			AssertNoWarning(master.BP_MasterBillInfo, CusCAeMHMasterValidation.MasterBillNotMatchPrimaryCCNMessage);
			master.BP_MasterBill = "APLU12341234";
			master.BP_PrimaryCCN = "952712341266";
			AssertHasWarning(master.BP_MasterBillInfo, CusCAeMHMasterValidation.MasterBillNotMatchPrimaryCCNMessage);
		}

		public void TestCheckBP_RL_NKDiscPort()
		{
			master.BP_RL_NKDiscPort = "XXXXX";
			AssertHasMessageErrorContaining(master.BP_RL_NKDiscPortInfo, ListValidation.InvalidCodeMessageError);
			master.BP_ATA = ZDateTime.Today;
			master.BP_RL_NKDiscPort = ZString.Empty;
			AssertHasMessageErrorContaining(master.BP_RL_NKDiscPortInfo, CusCAeMHMasterValidation.DiscPortIsMissing);
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "ABCDE";
			master.BP_RL_NKDiscPort = "ABCDE";
			AssertNoMessageErrorContaining(master.BP_RL_NKDiscPortInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBP_ModeOfTransport()
		{
			master.BP_ModeOfTransport = "XX";
			AssertHasMessageErrorContaining(master.BP_ModeOfTransportInfo, ListValidation.InvalidCodeMessageError);
			master.BP_ModeOfTransport = ZString.Empty;
			AssertHasMessageErrorContaining(master.BP_ModeOfTransportInfo, MandatoryValidation.YouHaveNotEntered);
			master.BP_ModeOfTransport = TransportTypeList.Codes.Sea;
			AssertNoMessageErrorContaining(master.BP_ModeOfTransportInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBP_MasterBill()
		{
			master.BP_MasterBill = "111";
			master.BP_MasterBill = ZString.Empty;
			AssertHasWarningContaining(master.BP_MasterBillInfo, MandatoryValidation.YouHaveNotEntered);
			master.BP_MasterBill = "111";
			AssertNoWarningContaining(master.BP_MasterBillInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBP_MasterHouseCCN()
		{
			master.BP_MasterHouseBill = "111";
			master.Validation.ValidateBP_MasterHouseCCN();
			AssertHasMessageErrorContaining(master.BP_MasterHouseCCNInfo, MandatoryValidation.YouHaveNotEntered);
			master.BP_MasterHouseCCN = "CCN";
			AssertNoMessageErrorContaining(master.BP_MasterHouseCCNInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(master.BP_MasterHouseCCNInfo, CusCAeMHMasterValidation.PeviousCCNDefaultControlledViaRegistrySetting);
			Factory.Save();

			master.BP_MasterHouseCCN = "AAAA";
			AssertHasMessageErrorContaining(master.BP_MasterHouseCCNInfo, CusCAeMHMasterValidation.PeviousCCNDefaultControlledViaRegistrySetting);

			master.BP_MasterHouseCCN = "8123CCN2";
			AssertNoMessageErrorContaining(master.BP_MasterHouseCCNInfo, CusCAeMHMasterValidation.PeviousCCNDefaultControlledViaRegistrySetting);
			AssertNoWarningContaining(master.BP_MasterHouseCCNInfo, "This is a key field");
			master.BP_CustomsStatus = EManifestForwarderJobStatusList.Codes.Error;
			Factory.Save();
			master.BP_MasterHouseCCN = "8123CCN3";
			AssertHasWarningContaining(master.BP_MasterHouseCCNInfo, "This is a key field");
			master.BP_CustomsStatus = EManifestForwarderJobStatusList.Codes.Cancelled;
			Factory.Save();
			master.BP_MasterHouseCCN = "8123CCN4";
			AssertNoWarningContaining(master.BP_MasterHouseCCNInfo, "This is a key field");
		}

		public void TestCheckBP_CBSACarrierCode()
		{
			var carrier = Factory.New<Universal.ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "CCCC";
			carrier.ZZ4_Description = "CCCC Name";
			carrier.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Canada;
			carrier.ZZ4_IsAir = true;
			carrier.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER);
			Factory.Save();

			master.BP_CBSACarrierCode = "XXXX";
			AssertHasMessageErrorContaining(master.BP_CBSACarrierCodeInfo, ListValidation.InvalidCodeMessageError);
			master.BP_CBSACarrierCode = ZString.Empty;
			AssertHasMessageErrorContaining(master.BP_CBSACarrierCodeInfo, MandatoryValidation.YouHaveNotEntered);
			master.BP_CBSACarrierCode = "CCCC";
			AssertNoMessageErrorContaining(master.BP_CBSACarrierCodeInfo, ListValidation.InvalidCodeMessageError);

			master.BP_CBSACarrierCode = "CCC1";
			AssertNoWarningContaining(master.BP_CBSACarrierCodeInfo, "This is a key field");
			master.BP_CustomsStatus = EManifestForwarderJobStatusList.Codes.Error;
			Factory.Save();
			master.BP_CBSACarrierCode = "CCC2";
			AssertHasWarningContaining(master.BP_CBSACarrierCodeInfo, "This is a key field");
			master.BP_CustomsStatus = EManifestForwarderJobStatusList.Codes.Cancelled;
			Factory.Save();
			master.BP_CBSACarrierCode = "CCC3";
			AssertNoWarningContaining(master.BP_CBSACarrierCodeInfo, "This is a key field");

			master.BP_ModeOfTransport = "SEA";
			master.BP_CBSACarrierCode = "CCCC";
			AssertHasWarningContaining(master.BP_CBSACarrierCodeInfo, "Carrier not allowed for mode of transport 'SEA'");

			master.BP_ModeOfTransport = "AIR";
			master.Validation.ValidateBP_CBSACarrierCode();
			AssertNoWarningContaining(master.BP_CBSACarrierCodeInfo, "Carrier not allowed for mode of transport 'SEA'");

			carrier.ZZ4_IsAir = false;
			carrier.Attributes.AddNew(TransportTypeList.Codes.Air, TransportTypeList.Codes.Air);
			Factory.Save();

			master.BP_ModeOfTransport = "SEA";
			master.BP_CBSACarrierCode = "CCCC";
			AssertHasWarningContaining(master.BP_CBSACarrierCodeInfo, "Carrier not allowed for mode of transport 'SEA'");

			master.BP_ModeOfTransport = "AIR";
			master.Validation.ValidateBP_CBSACarrierCode();
			AssertNoWarningContaining(master.BP_CBSACarrierCodeInfo, "Carrier not allowed for mode of transport 'SEA'");
		}

		public void TestCheckBP_CBSADischargePort()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0OFC", "Nanaimo", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			master.BP_CBSADischargePort = "OFC";
			AssertHasMessageErrorContaining("SubLocation has a messsage error.", master.BP_CBSADischargeSubLocationInfo, "Must be entered if Cust. Port of Discharge is entered.");
			master.BP_CBSADischargePort = ZString.Empty;
			AssertNoMessageErrorContaining("No message error when CBSADischargePort is empty.", master.BP_CBSADischargePortInfo, "Must be entered if Cust. Port of Discharge is entered.");

			master.BP_CBSADischargePort = "XXX";
			AssertHasMessageErrorContaining(master.BP_CBSADischargePortInfo, ListValidation.InvalidCodeMessageError);

			master.BP_CBSADischargePort = "OFC";
			AssertNoMessageErrorContaining(master.BP_CBSADischargePortInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBP_CBSADischargeSubLocation()
		{
			var subLoc = CACSubLocationTest.CreateSubLocation(Factory, "SUL");
			Factory.Save();

			master.BP_CBSADischargeSubLocation = "XXX";
			AssertHasMessageErrorContaining(master.BP_CBSADischargeSubLocationInfo, ListValidation.InvalidCodeMessageError);
			master.BP_CBSADischargeSubLocation = "SUL";
			AssertNoMessageErrorContaining(master.BP_CBSADischargeSubLocationInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBP_CBSADischargeSubLocation_Empty_When_BP_CBSADischargePort_NotEmpty_HasMessageError()
		{
			master.BP_CBSADischargePort = "OFC";

			AssertHasMessageErrorContaining(master.BP_CBSADischargeSubLocationInfo, "Must be entered if Cust. Port of Discharge is entered.");
		}

		public void TestCheckBP_CBSADischargeSubLocation_NotEmpty_When_BP_CBSADischargePort_NotEmpty_NoMessageError()
		{
			var subLoc = CACSubLocationTest.CreateSubLocation(Factory, "SUL");
			Factory.Save();

			AssertNoNotifications(master.BP_CBSADischargeSubLocationInfo);
		}

		CusCAeMHMaster master;
		protected override void SetUp()
		{
			base.SetUp();
			master = Factory.New<CusCAeMHMaster>();
		}
	}
}
