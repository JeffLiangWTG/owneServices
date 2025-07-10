using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Chief.GenericMessagingHarness;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.Testing
{
	[TestedType(typeof(CustomsExportConsolIntegrationWrapper))]
	sealed class CustomsExportConsolIntegrationWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shutUp = new SendsMessagesToCustomsShutterUpperer();
			return new CustomsExportConsolIntegrationWrapper(consol, shutUp);
		}

		public void TestDoesNotExplodeWhenNonEuDeclarationsAreOnShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "GBLHR";
			consol.JK_RL_NKDischargePort = "ZACPT";
			consol.JK_TransportMode = "AIR";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = consol.JK_RL_NKLoadPort;
			BaseJobDeclaration zaDeclaration = null;
			var zaCompany = Factory.New<GlbCompany>();
			zaCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			zaCompany.GC_Code = "CPT";
			var zaBranch = zaCompany.Branches.AddNew();
			zaBranch.GB_Code = "CPT";
			zaBranch.GB_RL_NKHomePort = consol.JK_RL_NKDischargePort;
			Factory.Save();
			using (DisposableEnvironment.ForBranch(zaBranch.PK.ToGuid())) // Need to wrap creation of first dec in ZA Branch context otherwise setting JE_JS on second declaration fails 
			{
				zaDeclaration = Factory.New<BaseJobDeclaration>();
				zaDeclaration.JE_JS = shipment.PK;
				zaDeclaration.CustomsEntryHeaders.AddNew();
			}
			Factory.Save();
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			var wrapper = new CustomsExportConsolIntegrationWrapper(consol, shutUp);
			AssertEquals("This property should not explode and should find no GB dec", false, wrapper.HasExportEntryForMessaging);
			var gbDeclaration = Factory.New<JobDeclaration>();
			gbDeclaration.JE_JS = shipment.PK;
			gbDeclaration.CustomsEntryHeaders.AddNew();
			AssertEquals("This property should not explode and should find GB export dec", true, wrapper.HasExportEntryForMessaging);
			gbDeclaration.JE_JS = ZGuid.Empty;
			gbDeclaration.Delete();
			BaseJobDeclaration gbDecLoadedAsBase = Factory.New<JobDeclaration>();
			gbDecLoadedAsBase = Factory.Load<BaseJobDeclaration>(gbDecLoadedAsBase.PK);
			gbDecLoadedAsBase.JE_JS = shipment.PK;
			gbDecLoadedAsBase.CustomsEntryHeaders.AddNew();
			AssertEquals("This property should not explode and should find GB export dec even if it were loaded as base", true, wrapper.HasExportEntryForMessaging);
		}

		public void TestMawbProperties()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "GBLHR";
			consol.JK_TransportMode = "AIR";
			var shutUp = new SendsMessagesToCustomsShutterUpperer();
			var wrapper = new CustomsExportConsolIntegrationWrapper(consol, shutUp);
			wrapper.MawbExportHelper.ME_ChiefCustomsReturnCode = "000";
			wrapper.MawbExportHelper.ME_ChiefEntryProcessingUnitID = "EPU1";
			wrapper.MawbExportHelper.ME_ChiefEntryProcessingUnitNumber = "EPU2";
			wrapper.MawbExportHelper.ME_ChiefGoodsArrivalDateTime = ZDateTime.BrettsBirthday;
			wrapper.MawbExportHelper.ME_ChiefGoodsLocation = "LHR";
			wrapper.MawbExportHelper.ME_ChiefMasterRouteOfEntry = "6";
			wrapper.MawbExportHelper.ME_ChiefMasterStyleOfEntry = "7";
			wrapper.MawbExportHelper.ME_ChiefMovementReference = "123456";
			wrapper.MawbExportHelper.ME_ChiefShed = "BAC";
			wrapper.MawbExportHelper.ME_ChiefConsolIsClosed = true;

			AssertEquals("000", wrapper.MawbExportHelper.ME_ChiefCustomsReturnCode);
			AssertEquals("EPU1", wrapper.MawbExportHelper.ME_ChiefEntryProcessingUnitID);
			AssertEquals("EPU2", wrapper.MawbExportHelper.ME_ChiefEntryProcessingUnitNumber);
			AssertEquals(ZDateTime.BrettsBirthday, wrapper.MawbExportHelper.ME_ChiefGoodsArrivalDateTime);
			AssertEquals("LHR", wrapper.MawbExportHelper.ME_ChiefGoodsLocation);
			AssertEquals("6", wrapper.MawbExportHelper.ME_ChiefMasterRouteOfEntry);
			AssertEquals("RT6", wrapper.MawbExportHelper.ME_ChiefMasterRouteOfEntryConvertedToEnterpriseForBinding);
			AssertEquals("7", wrapper.MawbExportHelper.ME_ChiefMasterStyleOfEntry);
			AssertEquals("123456", wrapper.MawbExportHelper.ME_ChiefMovementReference);
			AssertEquals("BAC", wrapper.MawbExportHelper.ME_ChiefShed);
			AssertEquals(true, wrapper.MawbExportHelper.ME_ChiefConsolIsClosed);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			consol = newFactory.Load<ForwardingConsol>(consol.PK);
			wrapper = new CustomsExportConsolIntegrationWrapper(consol, shutUp);
			AssertEquals("000", wrapper.MawbExportHelper.ME_ChiefCustomsReturnCode);
			AssertEquals("EPU1", wrapper.MawbExportHelper.ME_ChiefEntryProcessingUnitID);
			AssertEquals("EPU2", wrapper.MawbExportHelper.ME_ChiefEntryProcessingUnitNumber);
			AssertEquals(ZDateTime.BrettsBirthday, wrapper.MawbExportHelper.ME_ChiefGoodsArrivalDateTime);
			AssertEquals("LHR", wrapper.MawbExportHelper.ME_ChiefGoodsLocation);
			AssertEquals("6", wrapper.MawbExportHelper.ME_ChiefMasterRouteOfEntry);
			AssertEquals("RT6", wrapper.MawbExportHelper.ME_ChiefMasterRouteOfEntryConvertedToEnterpriseForBinding);
			AssertEquals("7", wrapper.MawbExportHelper.ME_ChiefMasterStyleOfEntry);
			AssertEquals("123456", wrapper.MawbExportHelper.ME_ChiefMovementReference);
			AssertEquals("BAC", wrapper.MawbExportHelper.ME_ChiefShed);
			AssertEquals(true, wrapper.MawbExportHelper.ME_ChiefConsolIsClosed);

			wrapper.MawbExportHelper.ME_ChiefMasterRouteOfEntry = "";
			AssertEquals("", wrapper.MawbExportHelper.ME_ChiefMasterRouteOfEntryConvertedToEnterpriseForBinding);
		}

		public void TestMessages()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shutUp = new SendsMessagesToCustomsShutterUpperer();
			var wrapper = new CustomsExportConsolIntegrationWrapper(consol, shutUp);
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_RL_NKOrigin = "GBXXX";
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_JS = shipment1.PK;
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			var message1 = entry1.Messages.AddNew();
			message1.EM_MessageType = "EAC";
			wrapper.Messages.Refresh();
			AssertEquals(1, wrapper.Messages.Count);

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_RL_NKOrigin = "GBXXX";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_JS = shipment2.PK;
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			var message2 = entry2.Messages.AddNew();
			message2.EM_MessageType = "EAC";
			wrapper.Messages.Refresh();
			AssertEquals(2, wrapper.Messages.Count);
		}

		public void TestHasExportEntryForMessaging()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			var wrapper = new CustomsExportConsolIntegrationWrapper(consol, shutUp);
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_RL_NKOrigin = "AUXXX";
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = "IMP";
			declaration1.JE_JS = shipment1.PK;
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			AssertEquals(false, wrapper.HasExportEntryForMessaging);
			shipment1.JS_RL_NKOrigin = "GBXXX";
			AssertEquals(false, wrapper.HasExportEntryForMessaging);
			declaration1.JE_MessageType = "EXP";
			AssertEquals(true, wrapper.HasExportEntryForMessaging);
		}

		public void TestCloseMasterUcrOnChiefUsingExistingEntry()
		{
			var badge = new BadgeCodeSetting();
			badge.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			badge.BadgeCode = "DAN";
			var credential = new CredentialsSetting();
			credential.BadgeCode = badge.BadgeCode;
			credential.PIMA = "CUKFFW98000DAN";
			credential.Company = "DAN";
			var badges = new BadgeCodeSettingCollection();
			badges.Add(badge);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
			var credentials = new CredentialsSettingCollection();
			credentials.Add(credential);
			GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, credentials);
			GBCustomsDataRegistry.Instance.CcsukLocalHostMnemonic.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "DANIEL");

			Factory.Save();

			var wrapper = InitialiseConsolAndWrapper();
			var shipment1 = wrapper.ForwardingConsol.Shipments.AddNew();
			shipment1.JS_RL_NKOrigin = "GBXXX";
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			shipment1.ConsigneeNameOrPK = consignee.PK.ToString();
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			shipment1.ConsignorNameOrPK = consignor.PK.ToString();
			shipment1.ConsigneeDocumentaryAddress.E2_Address1 = "DAN THE MAN STREET";
			shipment1.ConsignorDocumentaryAddress.E2_Address1 = "WABZNASM";
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MasterUCR = "MUCR HERE";
			declaration1.JE_CustomsProfile = "DAN";
			declaration1.JE_JS = shipment1.PK;
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			var invoice = declaration1.Invoices.AddNew();
			var line = invoice.InvoiceLines.AddNew();
			AssertEquals(0, entry1.Messages.Count);
			Factory.Save();
			wrapper.CloseMasterUcrOnChiefUsingExistingEntry();
			var newFactory = new BusinessObjectFactory();
			var message = newFactory.LoadTop1<Enterprise.Messaging.Business.EDIMessage>(new ZQuery());
			AssertContains("UNH+1+UKCINV:D:00A:UN:109001+", message.EM_MessageText);
			AssertContains("BGM+EAC:105:109'RFF+UCN:MUCR HERE'UNS+D'UNS+S'UNT+6+1'", message.EM_MessageText);
			AssertEquals("CUK", message.EM_ApplicationCode);
			AssertEquals("DAN", message.EM_MessageOwner);
			AssertEquals("EACCLS", message.EM_MessageType + message.EM_MessageSubType);
			AssertEquals(entry1.PK, message.EM_LinkUniqueID);
			var ynQs = shutUp.PastYesNoCancelQuestionsAsked;
			AssertEquals(0, ynQs.Count);

			wrapper.IsProfileForCDSForTest = true;
			shutUp.AnswerToContinueWithYNCAction = YesNoCancel.Cancel;
			wrapper.CloseMasterUcrOnChiefUsingExistingEntry();
			ynQs = shutUp.PastYesNoCancelQuestionsAsked;
			AssertEquals(1, ynQs.Count);
			AssertContains("You have asked to create a CHIEF message, but profile/badge ABC is set to use CDS for this consol's load port.", ynQs[0]);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestDirectMessaging_Close()
		{
			var wrapper = InitialiseConsolAndWrapper();
			wrapper.CloseMasterUcrOnChiefDirectlyOnConsol();
			var ynQs = shutUp.PastYesNoCancelQuestionsAsked;
			AssertEquals(0, ynQs.Count);
			AssertEquals(1, wrapper.ForwardingConsol.Messages.Count);
			AssertContains("BGM+EAC:105:109'RFF+UCN:A?:12512345678'UNS+D'UNS+S'UNT", wrapper.ForwardingConsol.Messages[0].EM_MessageText);

			wrapper.IsProfileForCDSForTest = true;
			shutUp.AnswerToContinueWithYNCAction = YesNoCancel.Cancel;
			wrapper.CloseMasterUcrOnChiefDirectlyOnConsol();
			ynQs = shutUp.PastYesNoCancelQuestionsAsked;
			AssertEquals(1, ynQs.Count);
			AssertContains("You have asked to create a CHIEF message, but profile/badge ABC is set to use CDS for this consol's load port.", ynQs[0]);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestDirectMessaging_Arrive()
		{
			var wrapper = InitialiseConsolAndWrapper();
			wrapper.ArriveGoodsOnChief();
			AssertEquals(1, wrapper.ForwardingConsol.Messages.Count);
			AssertContains("BGM+EAL:105:109'RFF+ABO:A?:12512345678'RFF+AES:11DEC0102'LOC+14+LGW:156:109:CAX'DTM+178:198712110102:203'TDT+13++1+++++:::QF123:AU'UNS", wrapper.ForwardingConsol.Messages[0].EM_MessageText);
			var ynQs = shutUp.PastYesNoCancelQuestionsAsked;
			AssertEquals(0, ynQs.Count);

			wrapper.IsProfileForCDSForTest = true;
			shutUp.AnswerToContinueWithYNCAction = YesNoCancel.Cancel;
			wrapper.ArriveGoodsOnChief();
			ynQs = shutUp.PastYesNoCancelQuestionsAsked;
			AssertEquals(1, ynQs.Count);
			AssertContains("You have asked to create a CHIEF message, but profile/badge ABC is set to use CDS for this consol's load port.", ynQs[0]);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestDirectMessaging_ArriveAntiSmugglingWhole()
		{
			var wrapper = InitialiseConsolAndWrapper();
			wrapper.MawbExportHelper.ME_CommunityTransitStatus = "F";
			wrapper.MawbExportHelper.ME_MasterOpt = "X";
			wrapper.MawbExportHelper.ME_PartMovementIndicator = false;
			wrapper.MawbExportHelper.ME_UseAntiSmugglingTrptid = true;
			wrapper.ArriveGoodsOnChief();
			AssertEquals(1, wrapper.ForwardingConsol.Messages.Count);
			AssertContains("BGM+EAL:105:109'GEI+OPT+X'RFF+ABO:A?:12512345678'RFF+AES:11DEC0102'LOC+14+LGW:156:109:CAX'DTM+178:198712110102:203'TDT+13+++++++:::O=LHR/D=SYD/C=AU/T=F'UNS", wrapper.ForwardingConsol.Messages[0].EM_MessageText);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestDirectMessaging_ArriveAntiSmugglingPart()
		{
			var wrapper = InitialiseConsolAndWrapper();
			wrapper.MawbExportHelper.ME_CommunityTransitStatus = "TD";
			wrapper.MawbExportHelper.ME_MasterOpt = "R";
			wrapper.MawbExportHelper.ME_PartMovementIndicator = true;
			wrapper.MawbExportHelper.ME_UseAntiSmugglingTrptid = true;
			wrapper.ArriveGoodsOnChief();
			AssertEquals(1, wrapper.ForwardingConsol.Messages.Count);
			AssertContains("BGM+EAL:105:109'GEI+OPT+R'RFF+ABO:A?:12512345678'RFF+AES:11DEC0102PART'LOC+14+LGW:156:109:CAX'DTM+178:198712110102:203'TDT+13+++++++:::O=LHR/D=SYD/C=AU/T=TD/P'UNS", wrapper.ForwardingConsol.Messages[0].EM_MessageText);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestDirectMessaging_Depart()
		{
			var wrapper = InitialiseConsolAndWrapper();
			wrapper.MawbExportHelper.ME_MasterOpt = "X"; // depart should not send master opt
			wrapper.DepartGoodsOnChief();
			AssertEquals(1, wrapper.ForwardingConsol.Messages.Count);
			AssertContains("BGM+EDL:105:109'RFF+ABO:A?:12512345678'LOC+14+LGW:156:109:CAX'DTM+189:19871211:102'TDT+13++1+++++:::QF123:AU'UNS", wrapper.ForwardingConsol.Messages[0].EM_MessageText);
			var ynQs = shutUp.PastYesNoCancelQuestionsAsked;
			AssertEquals(0, ynQs.Count);

			wrapper.IsProfileForCDSForTest = true;
			shutUp.AnswerToContinueWithYNCAction = YesNoCancel.Cancel;
			wrapper.DepartGoodsOnChief();
			ynQs = shutUp.PastYesNoCancelQuestionsAsked;
			AssertEquals(1, ynQs.Count);
			AssertContains("You have asked to create a CHIEF message, but profile/badge ABC is set to use CDS for this consol's load port.", ynQs[0]);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestDirectMessaging_Anticipate()
		{
			var wrapper = InitialiseConsolAndWrapper();
			wrapper.MawbExportHelper.ME_MasterOpt = "X";
			wrapper.AnticipateArrivalOnChief();
			AssertEquals(1, wrapper.ForwardingConsol.Messages.Count);
			AssertContains("BGM+EAA:105:109'GEI+OPT+X'RFF+ABO:A?:12512345678'RFF+AES:11DEC0102'LOC+14+LGW:156:109:CAX'TDT+13++1+++++:::QF123:AU'UNS+D'UNS+S'UNT", wrapper.ForwardingConsol.Messages[0].EM_MessageText);
			AssertEquals("", shutUp.LastErrorsAsString);
			var ynQs = shutUp.PastYesNoCancelQuestionsAsked;
			AssertEquals(0, ynQs.Count);

			wrapper.IsProfileForCDSForTest = true;
			shutUp.AnswerToContinueWithYNCAction = YesNoCancel.Cancel;
			wrapper.AnticipateArrivalOnChief();
			ynQs = shutUp.PastYesNoCancelQuestionsAsked;
			AssertEquals(1, ynQs.Count);
			AssertContains("You have asked to create a CHIEF message, but profile/badge ABC is set to use CDS for this consol's load port.", ynQs[0]);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestDirectMessaging_MasterUCRError()
		{
			var wrapper = InitialiseConsolAndWrapper();
			wrapper.ForwardingConsol.JK_MasterBillNum = "";
			wrapper.MawbExportHelper.ME_MasterUCR = "";
			wrapper.AnticipateArrivalOnChief();
			AssertEquals(0, wrapper.ForwardingConsol.Messages.Count);
			AssertContains("Master UCR", shutUp.LastErrorsAsString);
			wrapper.ForwardingConsol.JK_MasterBillNum = "98789789";
			wrapper.MawbExportHelper.ME_ExportShed = "";
			wrapper.ArriveGoodsOnChief();
			AssertEquals(0, wrapper.ForwardingConsol.Messages.Count);
			AssertContains("Shed", shutUp.LastErrorsAsString);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestDirectMessaging_ProfileError()
		{
			var wrapper = InitialiseConsolAndWrapper(makeBadge: false);
			wrapper.ForwardingConsol.JK_MasterBillNum = "98789789";
			wrapper.MawbExportHelper.ME_MasterUCR = "98789789";
			wrapper.AnticipateArrivalOnChief();
			var ynQs = shutUp.PastYesNoCancelQuestionsAsked;
			AssertEquals(0, ynQs.Count);
			AssertEquals(0, wrapper.ForwardingConsol.Messages.Count);
			AssertContains("Profile", shutUp.LastErrorsAsString);
			wrapper = InitialiseConsolAndWrapper();
			AssertEquals(0, wrapper.ForwardingConsol.Messages.Count);
			AssertNotContains("Profile", shutUp.LastErrorsAsString);

			wrapper.IsProfileForCDSForTest = true;
			shutUp.AnswerToContinueWithYNCAction = YesNoCancel.Cancel;
			wrapper.AnticipateArrivalOnChief();
			ynQs = shutUp.PastYesNoCancelQuestionsAsked;
			AssertEquals(1, ynQs.Count);
			AssertContains("You have asked to create a CHIEF message, but profile/badge ABC is set to use CDS for this consol's load port.", ynQs[0]);
		}

		public void TestConsolDocumentWrapperBansPrintingADSWhenNoP2P_DEP()
		{
			MawbTestHelper.MakeDepBadge("XYZ", GatewayList.Codes.CCSUKviaNTMsgGW, true);
			var stmMenuItem = Factory.Load<StmMenuItem>(new ZGuid(ForwardingConsolDocumentSupporter.GbAirlineDeliveryScheduleStmMenuItemPK));
			var wrapper = InitialiseConsolAndWrapper();
			var forwarder = Factory.NewWithValidTestData<OrgHeader>();
			var forwarderAddress = forwarder.Addresses.AddNewMainAddress();
			wrapper.ForwardingConsol.JK_OA_SendingForwarderAddress = forwarderAddress.PK;
			wrapper.ForwardingConsol.JK_OA_ReceivingForwarderAddress = forwarderAddress.PK;

			wrapper.MawbExportHelper.ME_Profile = "XYZ";   //DEP
			wrapper.MawbExportHelper.ME_ChiefMasterStyleOfEntry = ExportStyleOfEntries.Codes.Queried;  //3
			var consol = wrapper.ForwardingConsol;

			var baseRunState = consol.DocumentSupporter.GetDataStateBeforeRun(Factory.New<StmMenuItem>());
			AssertEquals(true, baseRunState.IsValid);
			var gbRunState = consol.DocumentSupporter.GetDataStateBeforeRun(stmMenuItem);
			AssertContains("permission to progress", gbRunState.ErrorMessage);
			AssertContains("SoE is: Queried", gbRunState.ErrorMessage);

			wrapper.MawbExportHelper.ME_ChiefMasterStyleOfEntry = ExportStyleOfEntries.Codes.PermittedToProgress;
			gbRunState = consol.DocumentSupporter.GetDataStateBeforeRun(stmMenuItem);
			AssertEquals(true, gbRunState.IsValid);

			var shipment = consol.Shipments.AddNew();
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_JS = shipment.PK;
			declaration.JE_DeclarationReference = "B00069";
			declaration.SingleEntry.CH_StyleOfEntrySOE = ExportStyleOfEntries.Codes.GoodsDepartedInland; //D
			gbRunState = consol.DocumentSupporter.GetDataStateBeforeRun(stmMenuItem);
			AssertEquals(false, gbRunState.IsValid);
			AssertContains("Declaration B00069, SoE D, CT status X, entry status", gbRunState.ErrorMessage);

			declaration.SingleEntry.CH_StyleOfEntrySOE = ExportStyleOfEntries.Codes.PermittedToProgress;
			gbRunState = consol.DocumentSupporter.GetDataStateBeforeRun(stmMenuItem);
			AssertEquals(true, gbRunState.IsValid);
		}

		[ExpectNoExceptions]
		public void TestAdsDocumentOptionDoesNotExplodeWhenSomehowLaunchedFromNonGbExportConsol()
		{
			// issue 00830627, user somehow invoked the Airline Delivery Schedule document option for a consol that was not GB. 
			GlbCompany.CurrentCompany.SetCountry("AU");
			var consolUsToAu = Factory.New<ForwardingConsol>();
			consolUsToAu.JK_RL_NKLoadPort = "USLAX";
			consolUsToAu.JK_RL_NKDischargePort = "AUSYD";
			var adsDocumentMenuItem = Factory.Load<StmMenuItem>(PrinterFromEdiMessageHelper_MenuKeys.Chief.AirlineDeliverySchedule);
			var runState = consolUsToAu.DocumentSupporter.GetDataStateBeforeRun(adsDocumentMenuItem);
			AssertContains("The consol is not an international export from GB", runState.ErrorMessage);
		}

		[TestDate(1986, 3, 12, 1, 2, 3)]
		public void TestConsolDocumentWrapperBansPrintingADSWhenNoP2P_Agent()
		{
			var wrapper = InitialiseConsolAndWrapper();
			wrapper.ForwardingConsol.JK_RL_NKLoadPort = "GBLHR";
			wrapper.ForwardingConsol.JK_RL_NKDischargePort = "AUSYD";
			wrapper.MawbExportHelper.ME_Profile = "ABC";  // agent

			AssertContains("Sending/receiving agents are not set", "sending or receiving agent is not set", wrapper.GetReasonWhyCannotPrintAirlineDeliveryScheduleForConsol());
			var forwarder = Factory.NewWithValidTestData<OrgHeader>();
			var forwarderAddress = forwarder.Addresses.AddNewMainAddress();
			wrapper.ForwardingConsol.JK_OA_SendingForwarderAddress = forwarderAddress.PK;
			wrapper.ForwardingConsol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			AssertContains("Sending/receiving agents are not set", "sending or receiving agent is not set", wrapper.GetReasonWhyCannotPrintAirlineDeliveryScheduleForConsol());
			wrapper.ForwardingConsol.JK_OA_ReceivingForwarderAddress = forwarderAddress.PK;

			wrapper.ForwardingConsol.JK_OA_SendingForwarderAddress = forwarderAddress.PK;
			wrapper.ForwardingConsol.JK_OA_ReceivingForwarderAddress = forwarderAddress.PK;

			wrapper.MawbExportHelper.ME_ChiefConsolIsClosed = false;
			AssertContains("closed", wrapper.GetReasonWhyCannotPrintAirlineDeliveryScheduleForConsol());
			wrapper.MawbExportHelper.ME_ChiefConsolIsClosed = true;

			wrapper.MawbExportHelper.ME_Queried = ZDateTime.Empty;
			AssertContains("not synchronised with Customs", wrapper.GetReasonWhyCannotPrintAirlineDeliveryScheduleForConsol());

			wrapper.MawbExportHelper.ME_Queried = ZDateTime.Now.AddDays(-1);
			wrapper.ForwardingConsol.JK_SystemLastEditTimeUtc = ZDateTime.Now;
			AssertContains("consol has been modified since", wrapper.GetReasonWhyCannotPrintAirlineDeliveryScheduleForConsol());

			var shipment1 = wrapper.ForwardingConsol.Shipments.AddNew();
			wrapper.ForwardingConsol.JK_SystemLastEditTimeUtc = ZDateTime.Now.AddDays(-2);
			wrapper.MawbExportHelper.ME_Queried = ZDateTime.Now.AddDays(-1);
			shipment1.JS_SystemLastEditTimeUtc = ZDateTime.Now;
			AssertContains("shipment has been modified since", wrapper.GetReasonWhyCannotPrintAirlineDeliveryScheduleForConsol());

			wrapper.ForwardingConsol.JK_SystemLastEditTimeUtc = ZDateTime.Now;
			wrapper.MawbExportHelper.ME_Queried = ZDateTime.Now;
			shipment1.JS_SystemLastEditTimeUtc = ZDateTime.Now;

			shipment1.JS_HouseBill = "House1";
			AssertContains("At least one shipment has no internal or external DUCRs and is not C-status", wrapper.GetReasonWhyCannotPrintAirlineDeliveryScheduleForConsol());
			AssertContains("Error should mention which shipment is faulty", "HOUSE1", wrapper.GetReasonWhyCannotPrintAirlineDeliveryScheduleForConsol());

			shipment1.JS_CommunityTransitStatus = ExportCommunityTransitStatusList.Codes.C;
			AssertEquals("All ok to produce ADS, C-status shipment needs no DUCR", "", wrapper.GetReasonWhyCannotPrintAirlineDeliveryScheduleForConsol());

			shipment1.JS_CommunityTransitStatus = ExportCommunityTransitStatusList.Codes.X;
			AssertContains("At least one shipment has no internal or external DUCRs and is not C-status", wrapper.GetReasonWhyCannotPrintAirlineDeliveryScheduleForConsol());

			var externalDucr = shipment1.Numbers.AddNew();
			externalDucr.CE_EntryType = CusEntryNumberTypes.Standard.UniqueConsignementReference;
			externalDucr.CE_EntryNum = "Anything for a DUCR";
			AssertEquals("All ok to produce ADS, external DUCR exists", "", wrapper.GetReasonWhyCannotPrintAirlineDeliveryScheduleForConsol());
			externalDucr.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;  // anything but UCR

			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_JS = shipment1.PK;
			var ceh = jobDeclaration.CustomsEntryHeaders.AddNew();
			ceh.EntryNumber = "123-123456A";
			AssertEquals("All ok to produce ADS, internal export DUCR exists", "", wrapper.GetReasonWhyCannotPrintAirlineDeliveryScheduleForConsol());

			jobDeclaration.JE_MessageType = "IMP";
			AssertContains("no internal or external DUCRs", wrapper.GetReasonWhyCannotPrintAirlineDeliveryScheduleForConsol());

			wrapper.MawbExportHelper.ME_Profile = "";
			AssertContains("PIMA", wrapper.GetReasonWhyCannotPrintAirlineDeliveryScheduleForConsol());
		}

		public void TestCanSendMessages()
		{
			var consol = Factory.New<ForwardingConsol>();
			shutUp = new SendsMessagesToCustomsShutterUpperer();
			var wrapper = new CustomsExportConsolIntegrationWrapperTestHelper(consol, shutUp);

			using (GBCustomsDataRegistry.Instance.GB_CustomsModuleEnabledForShipmentsAndConsols.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true))
			{
				AssertEquals("Cannot send", false, wrapper.CanSendMessages_Exposed);

				consol.JK_TransportMode = "AIR";
				consol.JK_RL_NKLoadPort = "GBLHR";
				consol.JK_RL_NKDischargePort = "AUSYD";

				AssertEquals("Can send", true, wrapper.CanSendMessages_Exposed);
			}

			using (GBCustomsDataRegistry.Instance.GB_CustomsModuleEnabledForShipmentsAndConsols.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false))
			{
				AssertEquals("Should not send", false, wrapper.CanSendMessages_Exposed);
			}
		}

		public void TestYesNoCancelMessages()
		{
			var consol = Factory.New<ForwardingConsol>();
			shutUp = new SendsMessagesToCustomsShutterUpperer();
			var wrapper = new CustomsExportConsolIntegrationWrapperTestHelper(consol, shutUp);

			shutUp.AnswerToContinueWithYNCAction = YesNoCancel.No;
			var result = wrapper.ShowYesNoCancel_Exposed("MSG1", "Msg1 - No is true", "Msg1");
			var msgs = shutUp.PastYesNoCancelQuestionsAsked;
			AssertEquals("Msg Count 1", 1, msgs.Count);
			AssertEquals("Msg Content 1", "Msg1 - No is true" + CustomsExportConsolIntegrationWrapper.YesNoCancelQuestionInfo, msgs[0]);
			AssertEquals("Result 1", true, result);

			shutUp.AnswerToContinueWithYNCAction = YesNoCancel.Yes;
			result = wrapper.ShowYesNoCancel_Exposed("MSG1", "Msg1 - Yes is true", "Msg1");
			AssertEquals("Msg Count 2", 2, msgs.Count);
			AssertEquals("Msg Content 2", "Msg1 - Yes is true" + CustomsExportConsolIntegrationWrapper.YesNoCancelQuestionInfo, msgs[1]);
			AssertEquals("Result 2", true, result);

			result = wrapper.ShowYesNoCancel_Exposed("MSG1", "Msg1 - Suppressed Yes", "Msg1");
			AssertEquals("No New msgs 3", 2, msgs.Count);
			AssertEquals("Result 3", true, result);

			shutUp.AnswerToContinueWithYNCAction = YesNoCancel.Cancel;
			result = wrapper.ShowYesNoCancel_Exposed("MSG2", "Msg2 - New Msg - Cancel is false", "Msg2");
			AssertEquals("New msg 4", 3, msgs.Count);
			AssertEquals("Msg Content 4", "Msg2 - New Msg - Cancel is false" + CustomsExportConsolIntegrationWrapper.YesNoCancelQuestionInfo, msgs[2]);
			AssertEquals("Result 4", false, result);

			result = wrapper.ShowYesNoCancel_Exposed("MSG1", "Msg1 - Still Suppressed (Cancel Ignored)", "Msg1");
			AssertEquals("No New msgs 5", 3, msgs.Count);
			AssertEquals("Result 5", true, result);
		}

		public void TestYesNoMessages()
		{
			var consol = Factory.New<ForwardingConsol>();
			shutUp = new SendsMessagesToCustomsShutterUpperer();
			var wrapper = new CustomsExportConsolIntegrationWrapperTestHelper(consol, shutUp);

			shutUp.AnswerToContinueWithAction = false;
			wrapper.ShowYesNo_Exposed("MSG1", "Msg1 - No suppression", "Msg1");
			var msgs = shutUp.PastYesNoQuestionsAsked;
			AssertEquals("Msg Count 1", 1, msgs.Count);
			AssertEquals("Msg Content 1", "Msg1 - No suppression" + CustomsExportConsolIntegrationWrapper.YesNoQuestionInfo, msgs[0]);

			shutUp.AnswerToContinueWithAction = true;
			wrapper.ShowYesNo_Exposed("MSG1", "Msg1 - Suppression", "Msg1");
			AssertEquals("Msg Count 2", 2, msgs.Count);
			AssertEquals("Msg Content 2", "Msg1 - Suppression" + CustomsExportConsolIntegrationWrapper.YesNoQuestionInfo, msgs[1]);

			wrapper.ShowYesNo_Exposed("MSG1", "Msg1 - Suppressed", "Msg1");
			AssertEquals("No New msgs 3", 2, msgs.Count);

			shutUp.AnswerToContinueWithAction = false;
			wrapper.ShowYesNo_Exposed("MSG2", "Msg2 - New Msg", "Msg2");
			AssertEquals("New msg 4", 3, msgs.Count);
			AssertEquals("Msg Content 4", "Msg2 - New Msg" + CustomsExportConsolIntegrationWrapper.YesNoQuestionInfo, msgs[2]);

			wrapper.ShowYesNo_Exposed("MSG1", "Msg1 - Still Suppressed", "Msg1");
			AssertEquals("No New msgs 5", 3, msgs.Count);
		}

		public void TestMessageText()
		{
			using (GBCustomsDataRegistry.Instance.GB_CustomsModuleEnabledForShipmentsAndConsols.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = "AIR";
				consol.JK_RL_NKLoadPort = "GBLHR";
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_MasterBillNum = "125-12345678";
				shutUp = new SendsMessagesToCustomsShutterUpperer(false);

				shutUp.AnswerToContinueWithAction = false;
				shutUp.AnswerToContinueWithYNCAction = YesNoCancel.Cancel;

				var shipment1 = Factory.New<ForwardingShipment>(); // MsgId - 7CE67EFC-0990-4F5F-95D4-DCDB5C905E2A - ShowYesNo
				var shipment2 = Factory.New<ForwardingShipment>(); // MsgId - D6518863-EE52-4962-8ABF-B7B952D69D5D - ShowWarning
				shipment2.JS_CommunityTransitStatus = "C";
				var shipment3 = Factory.New<ForwardingShipment>(); // MsgId - 64C19DA4-2EE6-47B6-9501-F52EFB33D7EA - ShowWarning
																   // MsdId - EC92C817-5AE7-4428-849E-A524F0AB149A - ShowError HasExportEntryForMessaing
				var shipment4 = Factory.New<ForwardingShipment>(); // MsgId - 319514D9-3F2E-4C29-9122-C41059E0189A - ShowError
				var shipment5 = Factory.New<ForwardingShipment>(); // MsgId - 6371914F-6E6A-41BE-B730-B91B21C17E11 - ShowYesNo
				var shipNum = shipment5.Numbers.AddNew();
				shipNum.CE_EntryNum = "Ship5";
				shipNum.CE_EntryType = Common.CusEntryNumberTypes.Standard.UniqueConsignementReference;
				var shipment6 = Factory.New<ForwardingShipment>(); //MsgId - C976A3CC-D29E-4C7B-AD9B-73B7C3E6AF45 - ShowInfo
				shipNum = shipment6.Numbers.AddNew();
				shipNum.CE_EntryNum = "Ship6";
				shipNum.CE_EntryType = Common.CusEntryNumberTypes.Standard.UniqueConsignementReference;
				var shipment7 = Factory.New<ForwardingShipment>(); //MsgId - 4270343A-FB7D-44C5-AA1C-588631213986 - ShowError
				var dec = Factory.New<JobDeclaration>();
				dec.JE_MessageType = "EXP";
				dec.JE_UCR = "Ship7";
				dec.JE_JS = shipment7.PK;

				Factory.Save();

				var wrapper = new CustomsExportConsolIntegrationWrapperTestHelper(consol, shutUp);
				wrapper.MawbExportHelper.ME_MasterUCR = "ABC";
				wrapper.MawbExportHelper.ME_Profile = "QWE";

				var args = new CollectionCountChangedEventArgs(true, shipment1);
				wrapper.HandleShipmentAddedToOrRemovedFromConsol(args);

				var yncQs = shutUp.PastYesNoCancelQuestionsAsked;
				var ynQs = shutUp.PastYesNoQuestionsAsked;

				AssertEquals(1, yncQs.Count);
				AssertContains("Do you wish to send an EAC message/s to associate the (D)UCR(s) of Shipment S00001000 to consolidation ABC?\r\nWarning: the master consol is not known to be closed." + CustomsExportConsolIntegrationWrapper.YesNoCancelQuestionInfo, yncQs[0]);

				args = new CollectionCountChangedEventArgs(true, shipment2);
				wrapper.HandleShipmentAddedToOrRemovedFromConsol(args);

				AssertEquals(1, ynQs.Count);
				AssertContains("Shipment S00001001 - no EAC message will be sent for this C-status shipment" + CustomsExportConsolIntegrationWrapper.YesNoQuestionInfo, ynQs[0]);

				wrapper.MawbExportHelper.ME_MasterUCR = ZString.Empty;
				args = new CollectionCountChangedEventArgs(true, shipment3);
				wrapper.HandleShipmentAddedToOrRemovedFromConsol(args);

				AssertEquals(2, ynQs.Count);
				AssertContains("There is insufficient data to automatically notify Customs of this attachment/detachment.\r\nTo enable automatic notifications to Customs, ensure that a MAWB number is present and\r\nthat a CCS-UK profile is selected on the 'Electronic Messaging' tab." + CustomsExportConsolIntegrationWrapper.YesNoQuestionInfo, ynQs[1]);

				var bResult = wrapper.HasExportEntryForMessaging;
				AssertEquals(false, bResult);
				AssertEquals(3, ynQs.Count);
				AssertContains("There are no shipments on this consolidation that have an attached declaration that can be used for messaging with Customs.\r\nEnsure shipments have a declaration and that entries have been generated." + CustomsExportConsolIntegrationWrapper.YesNoQuestionInfo, ynQs[2]);

				wrapper.MawbExportHelper.ME_MasterUCR = "ABC";
				shutUp.AnswerToContinueWithYNCAction = YesNoCancel.Yes; //Bypass first YesNoCancel in Future tests
				args = new CollectionCountChangedEventArgs(true, shipment4);
				wrapper.HandleShipmentAddedToOrRemovedFromConsol(args);

				AssertEquals(4, ynQs.Count);
				AssertContains(" has no GB export declaration with a DUCR. No external UCR exists on the shipment. An EAC message could not be sent.\r\nYou should close without saving to undo your attach/detach operation." + CustomsExportConsolIntegrationWrapper.YesNoQuestionInfo, ynQs[3]);

				shutUp.AnswerToContinueWithYNCAction = YesNoCancel.No;
				args = new CollectionCountChangedEventArgs(true, shipment5);
				wrapper.HandleShipmentAddedToOrRemovedFromConsol(args);

				AssertEquals(3, yncQs.Count);
				AssertContains(" has no GB export declaration with a DUCR. Do you wish to use the external UCR(s)\r\nShip5?" + CustomsExportConsolIntegrationWrapper.YesNoCancelQuestionInfo, yncQs[2]);

				shutUp.AnswerToContinueWithYNCAction = YesNoCancel.Cancel;
				args = new CollectionCountChangedEventArgs(true, shipment6);
				wrapper.HandleShipmentAddedToOrRemovedFromConsol(args);

				AssertEquals(4, yncQs.Count);
				AssertEquals(5, ynQs.Count);
				AssertContains("No EAC message was sent.\r\nYou should close without saving to undo your attach/detach operation." + CustomsExportConsolIntegrationWrapper.YesNoQuestionInfo, ynQs[4]);

				args = new CollectionCountChangedEventArgs(true, shipment7);
				wrapper.HandleShipmentAddedToOrRemovedFromConsol(args);
				AssertEquals(6, ynQs.Count);
				AssertContains(" has no GB export declaration with an entry header. An EAC message could not be sent.\r\nYou should close without saving to undo your attach/detach operation." + CustomsExportConsolIntegrationWrapper.YesNoQuestionInfo, ynQs[5]);
			}
		}

		public void TestAdditionalReferenceNumbersConcurrentEdit()
		{
			using (GBCustomsDataRegistry.Instance.GB_CustomsModuleEnabledForShipmentsAndConsols.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true))
			{
				var shipment = Factory.New<ForwardingShipment>();
				var ucrNumber1 = shipment.Numbers.AddNew();
				ucrNumber1.CE_EntryNum = "UCR0001";
				ucrNumber1.CE_EntryType = CusEntryNumberTypes.Standard.UniqueConsignementReference;
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				newFactory.RefreshEnabled = false;
				var reloadedShipment = newFactory.Load<ForwardingShipment>(shipment.PK);
				var ucrNumber2 = reloadedShipment.Numbers.AddNew();
				ucrNumber2.CE_EntryNum = "UCR0002";
				ucrNumber2.CE_EntryType = CusEntryNumberTypes.Standard.UniqueConsignementReference;
				newFactory.Save();

				// Trigger factory to save the shipment
				shipment.JS_HouseBill = "12345678";

				var wrapper = InitialiseConsolAndWrapper();
				wrapper.HandleShipmentAddedToOrRemovedFromConsol(new CollectionCountChangedEventArgs(true, shipment));
				AssertEquals(true, shutUp.SuccessfulSendOccured);
			}
		}

		public void TestGetConsolSender()
		{
			var consol = Factory.New<ForwardingConsol>();
			var wrapper = new CustomsExportConsolIntegrationWrapperTestHelper(consol, null);

			wrapper.IsProfileForCDSForTest = false;
			AssertType<ConsolMessageSender>(wrapper.GetConsolMessgeSender());
			wrapper.IsProfileForCDSForTest = true;
			AssertEquals("resolve from Integration.Customs.GB.GBCDS.ICDSConsolMessageSender", "Enterprise.Customs.GB.CDS.Messaging.CDSConsolMessageSender", wrapper.GetConsolMessgeSender().GetType().FullName);
		}

		public void TestIsProfileForCDS()
		{
			using (SetupCredentials())
			{
				var consol = Factory.New<ForwardingConsol>();
				var wrapper = new CustomsExportConsolIntegrationWrapperTestHelper(consol, null);

				CombineAssertions(() =>
				{
					AssertEquals("Blank Profile is Chief", false, wrapper.IsProfileForCDS);
					wrapper.MawbExportHelper.ME_Profile = "GB123456789.CDS";
					AssertEquals("CDS Profile", true, wrapper.IsProfileForCDS);
					wrapper.MawbExportHelper.ME_Profile = "GB123456789.CHF";
					AssertEquals("Chief Profile", false, wrapper.IsProfileForCDS);
					wrapper.MawbExportHelper.ME_Profile = "GB123456789.CSK";
					AssertEquals("CCSYK CSK Profile", true, wrapper.IsProfileForCDS);
					wrapper.MawbExportHelper.ME_Profile = "CS2";
					AssertEquals("CCSUK CS2 Profile", false, wrapper.IsProfileForCDS);
				});
			}
		}

		public void TestIsProfileForCCSUK()
		{
			using (SetupCredentials())
			{
				var consol = Factory.New<ForwardingConsol>();
				var wrapper = new CustomsExportConsolIntegrationWrapperTestHelper(consol, null);

				CombineAssertions(() =>
				{
					AssertEquals("Blank Profile is not CCSUK", false, wrapper.IsProfileForCCSUK);
					wrapper.MawbExportHelper.ME_Profile = $"GB123456789.CSK";
					AssertEquals("CCSUK CSK Profile", true, wrapper.IsProfileForCCSUK);
					wrapper.MawbExportHelper.ME_Profile = $"GB123456789.CHF";
					AssertEquals("Chief Profile", false, wrapper.IsProfileForCCSUK);
					wrapper.MawbExportHelper.ME_Profile = $"GB123456789.CDS";
					AssertEquals("CDS Profile", false, wrapper.IsProfileForCCSUK);
					wrapper.MawbExportHelper.ME_Profile = $"CS2";
					AssertEquals("CCSUK CS2 Profile", true, wrapper.IsProfileForCCSUK);
				});
			}
		}

		IDisposable SetupCredentials()
		{
			CreateExternalPass("CDS");
			CreateExternalPass("CSK");

			Factory.Save();

			var settingCollection = new BadgeCodeSettingCollection();
			settingCollection.Add(new BadgeCodeSetting(Factory) { BadgeCode = "CDS", CSPCode = "CDS", ApplicationCode = "CDS" });
			settingCollection.Add(new BadgeCodeSetting(Factory) { BadgeCode = "CSK", CSPCode = "CCSUK", ApplicationCode = "CDS" });
			settingCollection.Add(new BadgeCodeSetting(Factory) { BadgeCode = "CHF", CSPCode = "CNS", ApplicationCode = "CHF" });
			settingCollection.Add(new BadgeCodeSetting(Factory) { BadgeCode = "CS2", CSPCode = "CCSUK", ApplicationCode = "" });

			return GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settingCollection);
		}

		void CreateExternalPass(string badgeCode)
		{
			var extPass = Factory.New<GlbExternalPassword_GB>();
			extPass.Badge = badgeCode;
			extPass.EORI = "GB123456789";
			extPass.GP_GC = GlbCompany.CurrentCompany.PK;
		}

		CustomsExportConsolIntegrationWrapperTestHelper InitialiseConsolAndWrapper(string badge = "ABC", bool makeBadge = true)
		{
			if (makeBadge)
			{
				MawbTestHelper.MakeBadge(badge, GatewayList.Codes.CCSUKviaNTMsgGW, "CUKFFW98000", true, "", true, false, BadgeDirectionList.Codes.EXP, "GBLHR", Registry.MucrGenerationStyles.Codes.Air);
			}
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "GBLHR";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_MasterBillNum = "125-12345678";
			shutUp = new SendsMessagesToCustomsShutterUpperer();
			var wrapper = new CustomsExportConsolIntegrationWrapperTestHelper(consol, shutUp);
			wrapper.MawbExportHelper.ME_ExportLocation = "LGW";
			wrapper.MawbExportHelper.ME_ExportShed = "CAX";
			wrapper.MawbExportHelper.ME_MovementDate = ZDateTime.Now;
			wrapper.MawbExportHelper.ME_TransportCountry = "AU";
			wrapper.MawbExportHelper.ME_TransportID = "QF123";
			wrapper.MawbExportHelper.ME_TransportMode = "SEA";
			wrapper.MawbExportHelper.ME_Profile = badge;
			return wrapper;
		}

		SendsMessagesToCustomsShutterUpperer shutUp;
	}

	class CustomsExportConsolIntegrationWrapperTestHelper : CustomsExportConsolIntegrationWrapper
	{
		public CustomsExportConsolIntegrationWrapperTestHelper(ForwardingConsol consol, ISendsMessagesToCustoms sendsMessagesToCustoms) : base(consol, sendsMessagesToCustoms)
		{
		}

		public bool CanSendMessages_Exposed => base.CanSendMessages;
		public bool ShowYesNoCancel_Exposed(string msgId, string message, string caption) => base.ShowYesNoCancel(msgId, message, caption);
		public void ShowYesNo_Exposed(string msgId, string message, string caption) => base.ShowYesNo(msgId, message, caption);

		public bool? IsProfileForCCSUKForTest { get; set; }
		protected override bool IsProfileForCCSUKCore => IsProfileForCCSUKForTest ?? base.IsProfileForCCSUKCore;

		public bool? IsProfileForCDSForTest { get; set; }
		protected override bool IsProfileForCDSCore => IsProfileForCDSForTest ?? base.IsProfileForCDSCore;
	}
}
