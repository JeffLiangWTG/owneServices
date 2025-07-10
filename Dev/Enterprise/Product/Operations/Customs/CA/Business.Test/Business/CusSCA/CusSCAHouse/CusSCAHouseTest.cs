using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageProcessors.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusSCAHouse))]
	sealed class CusSCAHouseTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParentWorkflowProviders()
		{
			var house = Factory.New<CusSCAHouse>();
			var prov = ((IWorkflowTriggerEventSource)house).ParentWorkflowProviders;
			AssertNull(house.Consol);
			AssertNotNull(prov);
			AssertEquals(0, prov.Count);

			var helper = new CusSCATestHelper();
			var house2 = helper.House;
			prov = ((IWorkflowTriggerEventSource)house2).ParentWorkflowProviders;
			AssertNotNull(prov);
			AssertNotNull(house2.Consol);
			AssertEquals(1, prov.Count);
			AssertEquals(house2.Consol, prov[0]);
		}

		public void TestOnFactorySavingBeforeTransactionCore()
		{
			var helper = new CusSCATestHelper();
			var house = helper.House;
			helper.HelperFactory.Save();

			var logs = house.Logs;
			AssertEquals("Message Status Change Log Count", logs.Find(o => o.SL_SE_NKEvent == Events.MessageStatusChange.Code).Count(), 0);
			AssertEquals("Status Change Log Count", logs.Find(o => o.SL_SE_NKEvent == Events.StatusChange.Code).Count(), 0);
			house.CA_MessageStatus = "AWO";
			house.CA_ShipmentStatus = "CAN";
			helper.HelperFactory.Save();
			var messagelog = logs.Find(o => o.SL_SE_NKEvent == Events.MessageStatusChange.Code).FirstOrDefault();
			var customlog = logs.Find(o => o.SL_SE_NKEvent == Events.StatusChange.Code).FirstOrDefault();
			AssertNotNull(messagelog);
			AssertNotNull(customlog);
			AssertEquals("Message Status Change Log Reference", "AWO - Awaiting Supplementary Cargo Report Original", messagelog.SL_Reference);
			AssertEquals("Status Change Log Reference", "CAN - Canceled", customlog.SL_Reference);
		}

		public void TestShouldSynchronizeWithShipment()
		{
			var helper = new CusSCATestHelper();
			Assert("Should Sync", helper.House.ShouldSynchronizeWithShipment);
			helper.House.CA_OverrideFreightDefaults = true;
			Assert("Should Not Sync", !helper.House.ShouldSynchronizeWithShipment);
			helper.House.CA_OverrideFreightDefaults = false;
			Assert("Should Sync", helper.House.ShouldSynchronizeWithShipment);
			helper.House.CA_ShipmentStatus = SupplementaryCargoReportJobStatusList.Codes.Clear;
			Assert("Should Not Sync", !helper.House.ShouldSynchronizeWithShipment);
			helper.House.CA_ShipmentStatus = SupplementaryCargoReportJobStatusList.Codes.Cancelled;
			Assert("Should Sync", helper.House.ShouldSynchronizeWithShipment);
		}

		public void TestFieldsReadOnly()
		{
			var helper = new CusSCATestHelper();
			var house = helper.House;
			AssertFieldsReadOnly(house, true);
			house.CA_OverrideFreightDefaults = true;
			AssertFieldsReadOnly(house, false);
			house.CA_OverrideFreightDefaults = false;
			AssertFieldsReadOnly(house, true);
			house.CA_ShipmentStatus = SupplementaryCargoReportJobStatusList.Codes.Clear;
			house.CA_OverrideFreightDefaults = true;
			house.CA_OverrideFreightDefaults = false;
			AssertFieldsReadOnly(house, false);
		}

		void AssertFieldsReadOnly(CusSCAHouse house, bool shouldBeReadOnly)
		{
			AssertEquals("CA_RL_NK_PortOfDestination", shouldBeReadOnly, house.CA_RL_NK_PortOfDestinationInfo.ReadOnly);
			AssertEquals("CA_OH_Consignee is ReadOnly", shouldBeReadOnly, house.CA_OH_ConsigneeInfo.ReadOnly);
			AssertEquals("CA_OH_Consignor is ReadOnly", shouldBeReadOnly, house.CA_OH_ConsignorInfo.ReadOnly);
			AssertEquals("CA_OH_Notify is ReadOnly", shouldBeReadOnly, house.CA_OH_NotifyInfo.ReadOnly);
			AssertEquals("CA_OA_DeliveryAddress is ReadOnly", shouldBeReadOnly, house.CA_OA_DeliveryAddressInfo.ReadOnly);
		}

		public void TestOnOverrideFreightDefaultSet()
		{
			var helper = new CusSCATestHelper();
			var house = helper.House;
			var shipment = house.Shipment;
			AssertEquals("inital value of destination", "CATOR", house.CA_RL_NK_PortOfDestination);
			house.CA_OverrideFreightDefaults = true;
			house.CA_RL_NK_PortOfDestination = "CAVAN";
			AssertEquals("destination has been changed", "CAVAN", house.CA_RL_NK_PortOfDestination);
			house.CA_OverrideFreightDefaults = false;
			AssertEquals("destination should revert to shipment value", "CATOR", house.CA_RL_NK_PortOfDestination);
			house.CA_OverrideFreightDefaults = true;
			house.CA_RL_NK_PortOfDestination = "CAVAN";
			AssertEquals("destination has been changed", "CAVAN", house.CA_RL_NK_PortOfDestination);
			house.CA_ShipmentStatus = SupplementaryCargoReportJobStatusList.Codes.Clear;
			house.CA_OverrideFreightDefaults = false;
			AssertEquals("destination should not revert to shipment value", "CAVAN", house.CA_RL_NK_PortOfDestination);
		}

		public void TestNoLoggingACILicenceInTest()
		{
			var helper = new CusSCATestHelper();
			var count = helper.HelperFactory.Load<StmActivityLog>(new ZQuery(StmActivityLogSchema.S7_FormCaption, Env.Licence.ACIReportingPerTransaction.Name)).Length;
			var house = helper.House;

			var outMessage = house.Messages.AddNew();
			outMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAACI;
			outMessage.EM_MessageType = MessageTypeList.Codes.SupplementaryCargoReport;
			outMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outMessage.EM_Status = EDIMessage.Status.Sent;
			outMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			outMessage.EM_IsTestMessage = ZBool.True;
			outMessage.EM_GE = GlbDepartment.CurrentDepartment.PK;

			helper.HelperFactory.Save();

			var message1 = house.Messages.AddNew();
			message1.EM_ApplicationCode = Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.CAACI;
			message1.EM_MessageType = MessageTypeList.Codes.SupplementaryCargoReport;
			message1.EM_MessageSubType = MessageSubTypeCodes.Codes.Original;
			message1.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			message1.EM_MessageNum = "100";
			message1.EM_MessageText = SupplementaryCargoReportResponseMessageProcessorTest.ContentAcceptedMessageText.Replace("\r\n", "");
			helper.HelperFactory.Save();
			AssertEquals("Should not be any activity logs", 0, helper.HelperFactory.Load<StmActivityLog>(new ZQuery(StmActivityLogSchema.S7_FormCaption, Env.Licence.ACIReportingPerTransaction.Name)).Length - count);
		}

		public void TestLoggingACILicence()
		{
			var helper = new CusSCATestHelper();
			foreach (var log in helper.HelperFactory.Load<StmActivityLog>(new ZQuery(StmActivityLogSchema.S7_FormCaption, Env.Licence.ACIReportingPerTransaction.Name)))
			{
				log.Delete();
			}
			var count = helper.HelperFactory.Load<StmActivityLog>(new ZQuery(StmActivityLogSchema.S7_FormCaption, Env.Licence.ACIReportingPerTransaction.Name)).Length;
			AssertEquals("pre-condition, should not be any licence logs", 0, count);
			var house = helper.House;
			var oceanBill = helper.OceanBill;
			var newCompany = helper.HelperFactory.New<GlbCompany>();
			newCompany.GC_RN_NKCountryCode = Constants.CountryCodes.HongKong;
			newCompany.GC_Code = "HKC";
			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_RL_NKHomePort = helper.HelperFactory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Constants.CountryCodes.HongKong)).RL_Code;
			newBranch.GB_Code = "HKB";
			oceanBill.CB_GB = newBranch.PK;
			var newStaff = helper.HelperFactory.New<GlbStaff>();
			newStaff.GS_Code = "TZT";
			newStaff.GS_LoginName = "BOBB";
			newStaff.GS_FullName = "BOB BUILDER";
			var outMessage = house.Messages.AddNew();
			outMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAACI;
			outMessage.EM_MessageType = MessageTypeList.Codes.SupplementaryCargoReport;
			outMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outMessage.EM_Status = EDIMessage.Status.Sent;
			outMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			outMessage.EM_SystemCreateUser = newStaff.GS_Code;
			outMessage.EM_GE = GlbDepartment.CurrentDepartment.PK;
			helper.HelperFactory.Save();

			house.CA_ShipmentStatus = "XXX";
			var message2 = house.Messages.AddNew();
			message2.EM_ApplicationCode = Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.CAACI;
			message2.EM_MessageType = MessageTypeList.Codes.SupplementaryCargoReport;
			message2.EM_MessageSubType = MessageSubTypeCodes.Codes.Original;
			message2.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			message2.EM_MessageNum = "110";
			message2.EM_MessageText = SupplementaryCargoReportResponseMessageProcessorTest.NotMatchedMessageText.Replace("\r\n", "");
			helper.HelperFactory.Save();
			AssertEquals("Should be one new activity log", 1, helper.HelperFactory.Load<StmActivityLog>(new ZQuery(StmActivityLogSchema.S7_FormCaption, Env.Licence.ACIReportingPerTransaction.Name)).Length);
			var newLog = helper.HelperFactory.LoadTop1<StmActivityLog>(new ZQuery(StmActivityLogSchema.S7_FormCaption, Env.Licence.ACIReportingPerTransaction.Name));
			AssertNotNull(newLog);
			AssertEquals("Licence should have been logged under branch of CusSCAHouse", newBranch.PK, newLog.S7_ParentID);
			AssertEquals("Licence should have been logged under branch of CusSCAHouse", GlbBranchSchema.Constants.Prefix, newLog.S7_ParentTableCode);
			AssertEquals("Licence should have been logged under user of last sent message", newStaff.GS_Code, newLog.S7_GS_NKUser);

			var message3 = house.Messages.AddNew();
			message3.EM_ApplicationCode = Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.CAACI;
			message3.EM_MessageType = MessageTypeList.Codes.SupplementaryCargoReport;
			message3.EM_MessageSubType = MessageSubTypeCodes.Codes.Original;
			message3.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			message3.EM_MessageNum = "120";
			message3.EM_MessageText = SupplementaryCargoReportResponseMessageProcessorTest.RiskAssessmentNoticeMessageText.Replace("\r\n", "");
			helper.HelperFactory.Save();
			AssertEquals("Should still be only one new activity log", 1, helper.HelperFactory.Load<StmActivityLog>(new ZQuery(StmActivityLogSchema.S7_FormCaption, Env.Licence.ACIReportingPerTransaction.Name)).Length);
		}

		public void TestMessageStatusDescription()
		{
			var helper = new CusSCATestHelper();
			helper.House.CA_MessageStatus = MessageStatusList.Codes.AcknowledgedChange;
			AssertEquals("Message status description", "Acknowledged Supplementary Cargo Report Change", helper.House.MessageStatusDescription);
		}

		public void TestShipmentStatusDescription()
		{
			var helper = new CusSCATestHelper();
			helper.House.CA_ShipmentStatus = SupplementaryCargoReportJobStatusList.Codes.RACleared;
			AssertEquals("Message status description", SupplementaryCargoReportJobStatusList.Descriptions.RACleared, helper.House.ShipmentStatusDescription);
		}

		public void TestValidationType()
		{
			var helper = new CusSCATestHelper();
			AssertEquals("Validation Type", typeof(CusSCAHouseValidation), helper.House.Validation.GetType());
		}

		public void TestLookupsType()
		{
			var helper = new CusSCATestHelper();
			AssertEquals("Lookups Type", typeof(CusSCAHouseLookups), helper.House.Lookups.GetType());
		}

		public void TestMessageStatusReadOnly()
		{
			var helper = new CusSCATestHelper();
			Assert("Message status is read only", helper.House.CA_MessageStatusInfo.ReadOnly);
		}

		public void TestShipmentStatusReadOnly()
		{
			var helper = new CusSCATestHelper();
			Assert("Shipment status is read only", helper.House.CA_ShipmentStatusInfo.ReadOnly);
		}

		//the following test, tests most properties of this type
		public void TestSupplementaryCargoReportMessageBuildEndToEnd()
		{
			CACustomsDataRegistry.Instance.SupplementaryNumberSuffixAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "XYZ");

			var helper = new CusSCATestHelper();
			try
			{
				ValidationTestHelper.AddCarrierCodeToCurrentCompany(Factory, "8080");
				CusEntryNumber pCN = helper.Consol.Numbers.AddNew();
				pCN.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.PCN;
				pCN.CE_EntryNum = "ORIGCCN";
				ForwardingContainer c1 = helper.Container1;
				ForwardingContainer c2 = helper.Container2;
				ForwardingContainer c3 = helper.Container3;
				c3.JC_IsEmptyContainer = true;

				// setup stuff on consol and shipment before creating house etc (ie before this point)
				helper.House.CA_CargoFacilityLocation = "TORONTO AIRPORT";
				helper.House.CA_AuthenticationCode = "12345678";
				helper.House.CA_SpecialInstructions = "FRAGILE GLASS HANDLE WITH CAUTION";
				helper.House.CA_FROBTransitImportCode = InTransitCodeList.Codes.FROB;
				helper.PackLine1.CV_AssociatedContainer = CusSCATestHelper.Container2Num;
				helper.PackLine1.CV_PackageCount = 2400;
				helper.PackLine1.CV_PackageType = "PCS";
				helper.PackLine1.CV_GoodsDescription = "CARTRIDGES SMALL ARMS BLANK\n\rAND SOME WHITE DOVES";
				helper.PackLine1.CV_Weight = 12345678.1234m;
				helper.PackLine1.CV_WeightUQ = "KG";
				helper.PackLine1.CV_Volume = 200.075m;
				helper.PackLine1.CV_VolumeUQ = "L";
				helper.PackLine1.CV_MarksAndNumbers = "MARKS LINE 1\r\nMARKS LINE 2\r\nMARKS LINE 3\r\nMARKS LINE 4\r\nMARKS LINE 5\r\nMARKS LINE 6\r\nMARKS LINE 7\r\nMARKS LINE 8\r\nMARKS LINE 9\r\nMARKS LINE 10";
				helper.PackLine1.CV_HarmonisedTariffNums = "6601100000";
				helper.PackLine1.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0327", "A", "IMO").First().PK;
				helper.PackLine2.CV_AssociatedContainer = CusSCATestHelper.Container2Num;
				helper.PackLine2.Container.CN_RN_NKCountryOfRegistration = "";
				helper.PackLine2.CV_PackageCount = 200;
				helper.PackLine2.CV_PackageType = "BOX";
				helper.PackLine2.CV_GoodsDescription = "FRENCH DARK CHOCOLATE\r\nA LONG DESCRIPTION LINE WHICH SHOULD BE SPLIT OVER TWO FTX SEGMENTS";
				helper.PackLine2.CV_Weight = 2000.375m;
				helper.PackLine2.CV_WeightUQ = Core.Constants.Weight.Pounds;
				helper.PackLine2.CV_MarksAndNumbers = "A LONG MARKS LINE THAT SHOULD BE SPLIT INTO TWO SEGMENTS";
				helper.PackLine2.CV_HazardousGoods = true;
				helper.PackLine3.CV_AssociatedContainer = CusSCATestHelper.Container1Num;
				helper.PackLine3.Container.CN_RN_NKCountryOfRegistration = "US";
				helper.PackLine3.Container.CN_ContainerSizeOrISOCode = "22G0";
				helper.PackLine4.CV_AssociatedContainer = CusSCATestHelper.Container3Num;
				helper.PackLine4.Container.CN_RN_NKCountryOfRegistration = "";
				CusSCAPivot packLine5 = helper.House.PackLines.AddNew();
				packLine5.CV_AssociatedContainer = CusSCAHouse.NonContaineriseID;
				packLine5.CV_PackageCount = 200;
				packLine5.CV_PackageType = "PKG";
				packLine5.CV_GoodsDescription = "200 LOOSE PACKAGES";
				packLine5.CV_Weight = 4m;
				packLine5.CV_WeightUQ = "LB";
				packLine5.CV_Volume = 1m;
				packLine5.CV_VolumeUQ = "CY";

				Factory.Save();

				AssertEquals("Pre-condition, No messages on house", 0, helper.House.Messages.Count);

				var builder = new SupplementaryCargoReportMessageBuilder(helper.House, MessageSubTypes.Create);
				foreach (var builderResult in builder.PopulateMessages().GetBuilderResults())
				{
					var message = (EDIMessage)builderResult.Message;
					AssertEquals("One messages on house", 1, helper.House.Messages.Count);
					AssertEquals("Message is in house messages", helper.House.Messages[0], message);
					AssertMultilineASCIIEquals("SupplementaryCargoReportMessageContent", expectedResult, message.EM_FormattedMessageText);
				}
			}
			finally
			{
				ValidationTestHelper.RemoveCarrierCodeFromCurrentCompany(Factory);
			}
		}

		readonly ZString expectedResult = @"UNH+<<MSGNO PLACEHOLDER>>+GSMCAR:D:00A:UN:SUPRPT
BGM+85+S12345678+9
CST++687::96
TDT+20++1++8080
CNI+1
DOC+704+ORIGCCN
RFF+ABE:8080S12345678XYZ
LOC+8+CA:::TORONTO+TORONTO AIRPORT
GEI+6+:::26
FTX+SIN+++FRAGILE GLASS HANDLE WITH CAUTION
TDT+12
RFF+AIJ:OBL123456
NAD+CN+++CONSIGNEE NAME WHICH IS MORE THAN 3:5 CHARACTERS+CONSIGNEE ADDRESS LINE 1:CONSIGNEE ADDRESS LINE 2+MANHATTAN+NY+12986+US
CTA+CN+:FRANK
COM+2125555212:TE
NAD+CZ+++CONSIGNOR NAME LINE 1+CONSIGNOR ADDRESS LINE 1+PARIS+++FR
CTA+CO+:GILLES
COM+0129337218:TE
NAD+DP+++DELIVERED TO 1+DELIVERY 1 ADDRESS LINE 1+MANHATTAN+NY+12783+US
CTA+DL+:ELIZABETH
COM+2125551212:TE
NAD+NI+++NOTIFY PARTY 1+ADDRESS LINE 1+NEW YORK+NY+12345+US
CTA+NT+:SUZANNE
COM+6475551212:TE
EQD+CN+OCLU5430029++++5
EQD+CN+OCLU4320011US22G0::5++++5
EQD+CN+OCLU4320032++++4
GID+1
PAC+2400++PCS
FTX+AAA+++CARTRIDGES SMALL ARMS BLANK
FTX+AAA+++AND SOME WHITE DOVES
MEA+WT+AAE+KGM:12345678.123
MEA+VOL+:::V+WSD:200.075
SGP+OCLU5430029
DGS+++0327
PCI++MARKS LINE 1:MARKS LINE 2:MARKS LINE 3:MARKS LINE 4:MARKS LINE 5:MARKS LINE 6:MARKS LINE 7:MARKS LINE 8:MARKS LINE 9
PCI++MARKS LINE 10
CST++6601100000
GID+2
PAC+200++BOX
FTX+AAA+++FRENCH DARK CHOCOLATE
FTX+AAA+++A LONG DESCRIPTION LINE WHICH SHOULD BE SPLIT OVER
FTX+AAA+++TWO FTX SEGMENTS
MEA+WT+AAE+LBR:2000.375
SGP+OCLU5430029
DGS+++MHB
PCI++A LONG MARKS LINE THAT SHOULD BE SP:LIT INTO TWO SEGMENTS
GID+3
PAC+0
MEA+WT+AAE+KGM:0
SGP+OCLU4320011
GID+4
PAC+200++PKG
FTX+AAA+++200 LOOSE PACKAGES
MEA+WT+AAE+LBR:4
MEA+VOL+:::X+WSD:0.7646
AUT+12345678
UNT+58+<<MSGNO PLACEHOLDER>>";

		public void TestShouldMarkAsNeedingValidation()
		{
			var houseBill = Factory.New<CusSCAHouse>();

			houseBill.MarkLightValidationAsValidForTesting();
			Assert(houseBill.LightValidationIsValid);
			houseBill.CA_NotifyName = "123";
			Assert(!houseBill.LightValidationIsValid);

			houseBill.MarkLightValidationAsValidForTesting();
			Assert(houseBill.LightValidationIsValid);
			houseBill.CA_DeliveryName = "123";
			Assert(!houseBill.LightValidationIsValid);
		}
	}
}
