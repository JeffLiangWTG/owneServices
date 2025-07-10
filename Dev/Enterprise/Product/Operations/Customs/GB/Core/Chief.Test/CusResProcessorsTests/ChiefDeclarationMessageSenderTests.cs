using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.MessageBuilders;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.Testing;
using Enterprise.Customs.GB.Chief.CusDec.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.GB.Chief.Testing
{
	class ChiefDeclarationMessageSenderTests : TestCaseWithFactory
	{
		public void TestCheckTheFunctionCodeIsValidOrNotForSending()
		{
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
			refDataHelper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FUNCS, "Customs Functionality", Core.Constants.CountryCodes.UnitedKingdom, allowCreationOfFuncsOrPFunc: true);

			var chiefDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var sender = new ChiefDeclarationMessageSenderForTest();
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);

			var cudecMessageFunction = new CusdecMessageFunction.New();
			var result = sender.CheckTheFunctionCodeIsValidOrNotForSending_Exposed(shutUp, cudecMessageFunction, chiefDeclaration);
			Assert(result);

			var des242MessageFunction = new GbDes242MessageFunction.MucrAssociate();
			result = sender.CheckTheFunctionCodeIsValidOrNotForSending_Exposed(shutUp, des242MessageFunction, chiefDeclaration);
			Assert(result);

			using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.CHIEF_CDS_EXPORT_DUAL_RUN, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDateTime.Today, true))
			{
				shutUp.AnswerToContinueWithAction = false;
				result = sender.CheckTheFunctionCodeIsValidOrNotForSending_Exposed(shutUp, des242MessageFunction, chiefDeclaration);
				Assert(!result);
				result = sender.CheckTheFunctionCodeIsValidOrNotForSending_Exposed(shutUp, cudecMessageFunction, chiefDeclaration);
				Assert(result);

				shutUp.AnswerToContinueWithAction = true;
				result = sender.CheckTheFunctionCodeIsValidOrNotForSending_Exposed(shutUp, des242MessageFunction, chiefDeclaration);
				Assert(result);
				result = sender.CheckTheFunctionCodeIsValidOrNotForSending_Exposed(shutUp, cudecMessageFunction, chiefDeclaration);
				Assert(result);
			}

			var importCode = refDataHelper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FUNCS, FunctionalityTypes.CHIEF_SUNSET_IMP, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			chiefDeclaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			chiefDeclaration.JE_MessageType = MessageTypeList.Codes.Import;

			shutUp.AnswerToContinueWithAction = false;
			result = sender.CheckTheFunctionCodeIsValidOrNotForSending_Exposed(shutUp, cudecMessageFunction, chiefDeclaration);
			Assert(!result);
			result = sender.CheckTheFunctionCodeIsValidOrNotForSending_Exposed(shutUp, des242MessageFunction, chiefDeclaration);
			Assert(!result);

			shutUp.AnswerToContinueWithAction = true;
			result = sender.CheckTheFunctionCodeIsValidOrNotForSending_Exposed(shutUp, des242MessageFunction, chiefDeclaration);
			Assert(result);
			result = sender.CheckTheFunctionCodeIsValidOrNotForSending_Exposed(shutUp, cudecMessageFunction, chiefDeclaration);
			Assert(result);

			var exportCode = refDataHelper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FUNCS, FunctionalityTypes.CHIEF_SUNSET_EXP, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			chiefDeclaration.JE_MessageType = MessageTypeList.Codes.Export;

			shutUp.AnswerToContinueWithAction = false;
			result = sender.CheckTheFunctionCodeIsValidOrNotForSending_Exposed(shutUp, cudecMessageFunction, chiefDeclaration);
			Assert(!result);
			result = sender.CheckTheFunctionCodeIsValidOrNotForSending_Exposed(shutUp, des242MessageFunction, chiefDeclaration);
			Assert(!result);

			shutUp.AnswerToContinueWithAction = true;
			result = sender.CheckTheFunctionCodeIsValidOrNotForSending_Exposed(shutUp, des242MessageFunction, chiefDeclaration);
			Assert(result);
			result = sender.CheckTheFunctionCodeIsValidOrNotForSending_Exposed(shutUp, cudecMessageFunction, chiefDeclaration);
			Assert(result);

			// Testing without warning factor
			importCode.ZZD_StartDate = ZDateTime.Today.AddDays(5);
			Factory.Save();
			chiefDeclaration.JE_MessageType = MessageTypeList.Codes.Import;

			shutUp.AnswerToContinueWithAction = false;
			result = sender.CheckTheFunctionCodeIsValidOrNotForSending_Exposed(shutUp, cudecMessageFunction, chiefDeclaration);
			Assert(result);
			result = sender.CheckTheFunctionCodeIsValidOrNotForSending_Exposed(shutUp, des242MessageFunction, chiefDeclaration);
			Assert(result);

			shutUp.AnswerToContinueWithAction = true;
			result = sender.CheckTheFunctionCodeIsValidOrNotForSending_Exposed(shutUp, des242MessageFunction, chiefDeclaration);
			Assert(result);
			result = sender.CheckTheFunctionCodeIsValidOrNotForSending_Exposed(shutUp, cudecMessageFunction, chiefDeclaration);
			Assert(result);

			exportCode.ZZD_StartDate = ZDateTime.Today.AddDays(5);
			Factory.Save();
			chiefDeclaration.JE_MessageType = MessageTypeList.Codes.Export;

			shutUp.AnswerToContinueWithAction = false;
			result = sender.CheckTheFunctionCodeIsValidOrNotForSending_Exposed(shutUp, cudecMessageFunction, chiefDeclaration);
			Assert(result);
			result = sender.CheckTheFunctionCodeIsValidOrNotForSending_Exposed(shutUp, des242MessageFunction, chiefDeclaration);
			Assert(result);

			shutUp.AnswerToContinueWithAction = true;
			result = sender.CheckTheFunctionCodeIsValidOrNotForSending_Exposed(shutUp, des242MessageFunction, chiefDeclaration);
			Assert(result);
			result = sender.CheckTheFunctionCodeIsValidOrNotForSending_Exposed(shutUp, cudecMessageFunction, chiefDeclaration);
			Assert(result);

			// Testing with warning factor
			refDataHelper.CreateCusCodeListAttribute(importCode.PK, "WarningFactor", "2.0");
			Factory.Save();
			chiefDeclaration.JE_MessageType = MessageTypeList.Codes.Import;

			shutUp.AnswerToContinueWithYNCAction = YesNoCancel.Yes;
			result = sender.CheckTheFunctionCodeIsValidOrNotForSending_Exposed(shutUp, cudecMessageFunction, chiefDeclaration);
			Assert(result);
			AssertEquals("User should be nagged", true, shutUp.PastYesNoQuestionsAsked.Contains(ZString.Format(sunsetNagWarning, "imports", importCode.ZZD_StartDate.ToString("dd/MM/yyyy"))));
			JobDeclarationMessageManagerFrontEnd.CountOfImportDeclarationsThisSession++;
			shutUp.AnswerToContinueWithYNCAction = YesNoCancel.Yes;
			shutUp.PastYesNoQuestionsAsked.Clear();
			result = sender.CheckTheFunctionCodeIsValidOrNotForSending_Exposed(shutUp, des242MessageFunction, chiefDeclaration);
			Assert(result);
			AssertEquals("User shouldn't be nagged", false, shutUp.PastYesNoQuestionsAsked.Contains(ZString.Format(sunsetNagWarning, "imports", importCode.ZZD_StartDate.ToString("dd/MM/yyyy"))));
			JobDeclarationMessageManagerFrontEnd.CountOfImportDeclarationsThisSession++;
			result = sender.CheckTheFunctionCodeIsValidOrNotForSending_Exposed(shutUp, cudecMessageFunction, chiefDeclaration);
			Assert(result);
			AssertEquals("User should be nagged", true, shutUp.PastYesNoQuestionsAsked.Contains(ZString.Format(sunsetNagWarning, "imports", importCode.ZZD_StartDate.ToString("dd/MM/yyyy"))));

			refDataHelper.CreateCusCodeListAttribute(exportCode.PK, "WarningFactor", "2.0");
			Factory.Save();
			chiefDeclaration.JE_MessageType = MessageTypeList.Codes.Export;

			shutUp.AnswerToContinueWithYNCAction = YesNoCancel.Yes;
			result = sender.CheckTheFunctionCodeIsValidOrNotForSending_Exposed(shutUp, cudecMessageFunction, chiefDeclaration);
			Assert(result);
			AssertEquals("User should be nagged", true, shutUp.PastYesNoQuestionsAsked.Contains(ZString.Format(sunsetNagWarning, "exports", exportCode.ZZD_StartDate.ToString("dd/MM/yyyy"))));
			JobDeclarationMessageManagerFrontEnd.CountOfExportDeclarationsThisSession++;
			shutUp.AnswerToContinueWithYNCAction = YesNoCancel.Yes;
			shutUp.PastYesNoQuestionsAsked.Clear();
			result = sender.CheckTheFunctionCodeIsValidOrNotForSending_Exposed(shutUp, des242MessageFunction, chiefDeclaration);
			Assert(result);
			AssertEquals("User shouldn't be nagged", false, shutUp.PastYesNoQuestionsAsked.Contains(ZString.Format(sunsetNagWarning, "exports", exportCode.ZZD_StartDate.ToString("dd/MM/yyyy"))));
			JobDeclarationMessageManagerFrontEnd.CountOfExportDeclarationsThisSession++;
			result = sender.CheckTheFunctionCodeIsValidOrNotForSending_Exposed(shutUp, cudecMessageFunction, chiefDeclaration);
			Assert(result);
			AssertEquals("User should be nagged", true, shutUp.PastYesNoQuestionsAsked.Contains(ZString.Format(sunsetNagWarning, "exports", exportCode.ZZD_StartDate.ToString("dd/MM/yyyy"))));
		}

		const string sunsetNagWarning = "CHIEF for {0} is due to be retired on {1}.\r\nPlease migrate to CDS as soon as possible.  Would you like to send to CHIEF now?";

		public void TestCheckCcsukAirWaybillIsAcceptable()
		{
			EU.Business.Testing.ShedTest.CreateShed(Factory, "GB", "LHRABC", "BRITISH AIRWAYS at Heathrow", acpCode: "H");
			Factory.Save();

			var declaration = DeclarationChosererTester.CreateGemsDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(this.Factory);
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportFullDeclaration;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			var sender = new ChiefDeclarationMessageSenderForTest();
			AssertEquals(true, sender.CheckCcsukAirWaybillIsAcceptable_Exposed(declaration, shutUp));

			var mawb = Factory.New<Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB>();
			mawb.MasterLevelHouseHelper.CS_JE_CustomsFormalEntry = declaration.PK;
			mawb.CM_MAWB = "125-12345678";
			mawb.CargoTerminalOperatorAirport = "LHR";
			mawb.CargoTerminalOperator = "ABC";
			declaration.JE_MasterUCR = "HABC12512345678";

			mawb.DescriptionOfGoods = "CONSOL";
			mawb.ShipmentDescriptionCode = "M";
			AssertEquals(false, sender.CheckCcsukAirWaybillIsAcceptable_Exposed(declaration, shutUp));
			AssertContains("'Consol' is not allowed", shutUp.Warning);
			AssertContains("the bill's Shipment Description Code is M and Status 1 is unset and no splits exist", shutUp.Warning);

			mawb.DescriptionOfGoods = "Stuff";
			mawb.ShipmentDescriptionCode = "T";
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			AssertEquals(true, sender.CheckCcsukAirWaybillIsAcceptable_Exposed(declaration, shutUp));
			Assert(string.IsNullOrEmpty(shutUp.Warning));

			mawb.ShipmentDescriptionCode = "M";
			mawb.Splits.AddNew();
			Assert("Pre req - mawb HasSplits is true", ((Integration.Customs.GB.CCSUK.ICcsukCusAwbBase)mawb).HasSplits);
			Assert("Pre req - mawb has NO status 1", ((Integration.Customs.GB.CCSUK.ICcsukCusAwbBase)mawb).Status1Date.IsEmpty);
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			AssertEquals(true, sender.CheckCcsukAirWaybillIsAcceptable_Exposed(declaration, shutUp));
			Assert(string.IsNullOrEmpty(shutUp.Warning));

			mawb.MasterLevelHouseHelper.Logs.AddNew(Events.StatusChange, "ST1", ZDateTime.BrettsBirthday.ToOffset());
			mawb.Splits.DeleteAll();
			Assert("Pre req - mawb HasSplits is false", !((Integration.Customs.GB.CCSUK.ICcsukCusAwbBase)mawb).HasSplits);
			Assert("Pre req - mawb has status 1", !((Integration.Customs.GB.CCSUK.ICcsukCusAwbBase)mawb).Status1Date.IsEmpty);
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			AssertEquals(true, sender.CheckCcsukAirWaybillIsAcceptable_Exposed(declaration, shutUp));
			Assert(string.IsNullOrEmpty(shutUp.Warning));

			var baseHouse = (CusHAWB)Factory.New<Integration.Customs.GB.CCSUK.ICusHAWB>();
			// Neutral scenario
			baseHouse.CS_JE_CustomsFormalEntry = declaration.PK;
			AssertEquals(true, sender.CheckCcsukAirWaybillIsAcceptable_Exposed(declaration, shutUp));
			Assert(string.IsNullOrEmpty(shutUp.Warning));

			// Set up negative scenario
			baseHouse.CS_CM = mawb.PK;
			baseHouse.CS_HAWB = "11111111";
			baseHouse.CS_GoodsDescription = "CONSOL";
			baseHouse.CS_ShipmentType = "M";
			declaration.JE_MasterUCR = "HABC1251234567811111111";
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			AssertEquals(false, sender.CheckCcsukAirWaybillIsAcceptable_Exposed(declaration, shutUp));
			AssertContains("'Consol' is not allowed", shutUp.Warning);
			AssertContains("Shipment Description Code is M", shutUp.Warning);

			// Positive 1
			baseHouse.CS_GoodsDescription = "Stuff";
			baseHouse.CS_ShipmentType = "T";
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			AssertEquals(true, sender.CheckCcsukAirWaybillIsAcceptable_Exposed(declaration, shutUp));
			Assert(string.IsNullOrEmpty(shutUp.Warning));

			// Positive 2 - SDC M, splits
			baseHouse.CS_ShipmentType = "M";
			var split = Factory.New<CusPartShip>();
			split.CG_CS = baseHouse.PK;
			Assert("Pre req - house HasSplits is always false", !((Integration.Customs.GB.CCSUK.ICcsukCusAwbBase)baseHouse).HasSplits);
			Assert("Pre req - house has NO status 1", ((Integration.Customs.GB.CCSUK.ICcsukCusAwbBase)baseHouse).Status1Date.IsEmpty);
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			AssertEquals(false, sender.CheckCcsukAirWaybillIsAcceptable_Exposed(declaration, shutUp));
			AssertContains("the bill's Shipment Description Code is M and Status 1 is unset and no splits exist", shutUp.Warning);

			// Positive 3 - SDC=M, status 1 
			baseHouse.Logs.AddNew(Events.StatusChange, "ST1", ZDateTime.BrettsBirthday.ToOffset());
			split.CG_CS = Guid.Empty;
			Assert("Pre req - house HasSplits is always false", !((Integration.Customs.GB.CCSUK.ICcsukCusAwbBase)baseHouse).HasSplits);
			Assert("Pre req - house has status 1", !((Integration.Customs.GB.CCSUK.ICcsukCusAwbBase)baseHouse).Status1Date.IsEmpty);
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			AssertEquals(true, sender.CheckCcsukAirWaybillIsAcceptable_Exposed(declaration, shutUp));
			Assert(string.IsNullOrEmpty(shutUp.Warning));

			// Weird scenario where somehow (?!) there are two CusHAWBs linked to a declaration, one for a true house and one for its master bill. Ensure we only look to the true house. 
			var workerForMawb = mawb.ChildBills.AddNew();
			workerForMawb.CS_IsMasterHouse = true;
			workerForMawb.CS_GoodsDescription = "CONSOL";
			Assert("We look to the house which CS_IsMasterHouse is false", sender.CheckCcsukAirWaybillIsAcceptable_Exposed(declaration, shutUp));
			baseHouse.CS_GoodsDescription = "CONSOL";
			Assert(!sender.CheckCcsukAirWaybillIsAcceptable_Exposed(declaration, shutUp));
		}

		public void TestCcsukFallbackGivesWarning()
		{
			var declaration = DeclarationChosererTester.CreateGemsDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(this.Factory);
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportFullDeclaration;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;
			var sender = new ChiefDeclarationMessageSenderForTest();

			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			shutUp.AnswerToContinueWithAction = true;
			GBCustomsDataRegistry.Instance.ChiefFallbackExports.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Import job, exports fallback, no warning", 0, shutUp.PastYesNoQuestionsAsked.Count);
			AssertEquals(true, sender.CheckCcsukFallbackIsInOperation_Exposed(declaration, shutUp));

			shutUp.AnswerToContinueWithAction = true;
			GBCustomsDataRegistry.Instance.ChiefFallbackImports.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, sender.CheckCcsukFallbackIsInOperation_Exposed(declaration, shutUp));
			AssertContains("fallback is in effect", shutUp.PastYesNoQuestionsAsked[0]);

			shutUp = new SendsMessagesToCustomsShutterUpperer();
			shutUp.AnswerToContinueWithAction = false;
			AssertEquals(false, sender.CheckCcsukFallbackIsInOperation_Exposed(declaration, shutUp));
			AssertContains("fallback is in effect", shutUp.PastYesNoQuestionsAsked[0]);

			shutUp = new SendsMessagesToCustomsShutterUpperer();
			shutUp.AnswerToContinueWithAction = false;
			declaration.ZG_Gateway = GatewayList.Codes.CNS_CUSDECOnly;
			AssertEquals(true, sender.CheckCcsukFallbackIsInOperation_Exposed(declaration, shutUp));
			AssertEquals(0, shutUp.PastYesNoQuestionsAsked.Count);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;
			shutUp = new SendsMessagesToCustomsShutterUpperer();
			shutUp.AnswerToContinueWithAction = false;
			AssertEquals(false, sender.CheckCcsukFallbackIsInOperation_Exposed(declaration, shutUp));
			AssertContains("fallback is in effect", shutUp.PastYesNoQuestionsAsked[0]);

			GBCustomsDataRegistry.Instance.ChiefFallbackExports.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			shutUp = new SendsMessagesToCustomsShutterUpperer();
			AssertEquals(true, sender.CheckCcsukFallbackIsInOperation_Exposed(declaration, shutUp));
			AssertEquals(0, shutUp.PastYesNoQuestionsAsked.Count);
		}

		public void TestCheckBusinessObjectLevelValidationCount()
		{
			MessageSendingValidationForTest.Register();
			CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			GBCustomsDataRegistry.Instance.ChiefValidationCreditCheck_AlwaysCheck.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			GBCustomsDataRegistry.Instance.CcsukLocalHostMnemonic.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "FOO");
			MawbTestHelper.MakeBadge("XYZ", GatewayList.Codes.CCSUKviaNTMsgGW, false);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsProfile = "XYZ";
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			Factory.Save();
			var newFunction = new CusdecMessageFunction.New();
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			var messageSender = (IDeclarationMessageSender)Activator.CreateInstance(ObjectFactory.GetType<Integration.Customs.GB.IDeclarationMessageSenderChooser>());

			importer.OH_IsDebtor = true;
			importer.CompanyData.OB_AROnCreditHold = true;
			importer.OH_Code = "DJC123";
			Factory.Save();
			importer.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			messageSender.Send(declaration, shutUp, newFunction);
			var cnt = MessageSendingValidationForTest.ValidationCount;
			AssertEquals("CheckBusinessObjectLevelValidation should only be called once", 1, cnt);
		}

		class MessageSendingValidationForTest : MessageSendingValidation
		{
			public static int ValidationCount;
			protected MessageSendingValidationForTest(BusinessObject topLevelBusinessObjectForValidation, IEnumerable<INotification> messageErrors, bool refreshValidation = true) : base(topLevelBusinessObjectForValidation, messageErrors, refreshValidation)
			{
			}

			protected MessageSendingValidationForTest(BusinessObject topLevelBusinessObjectForValidation, IEnumerable<INotification> messageErrors, SecurityCheckpoint sendMessageWithErrorsSecurityCheckpoint, bool refreshValidation = true) : base(topLevelBusinessObjectForValidation, messageErrors, sendMessageWithErrorsSecurityCheckpoint, refreshValidation)
			{
			}

			protected override MessageSendingNotificationCollection CheckBusinessObjectLevelValidationCore()
			{
				ValidationCount++;
				return base.CheckBusinessObjectLevelValidationCore();
			}

			public static void Register()
			{
				OverridableNewDelegate.Value = new NewDelegate(GetInstance);
			}

			static MessageSendingValidation GetInstance(BusinessObject topLevelBusinessObjectForValidation, IEnumerable<INotification> messageErrors)
			{
				return new MessageSendingValidationForTest(topLevelBusinessObjectForValidation, messageErrors);
			}
		}

		class ChiefDeclarationMessageSenderForTest : ChiefDeclarationMessageSender
		{
			public bool CheckTheFunctionCodeIsValidOrNotForSending_Exposed(ISendsMessagesToCustoms sendMessagesToCustoms, CusdecMessageFunction how, JobDeclaration declaration)
			{
				return base.CheckTheFunctionCodeIsValidOrNotForSending(true, sendMessagesToCustoms, how, declaration);
			}

			public bool CheckCcsukAirWaybillIsAcceptable_Exposed(JobDeclaration declaration, ISendsMessagesToCustoms sendMessagesToCustoms)
			{
				return base.CheckCcsukAirWaybillIsAcceptable(declaration, sendMessagesToCustoms);
			}

			public bool CheckCcsukFallbackIsInOperation_Exposed(JobDeclaration declaration, ISendsMessagesToCustoms sendMessagesToCustoms)
			{
				return base.CheckCcsukFallbackIsInOperation(declaration, sendMessagesToCustoms);
			}

			public override string[] GetCodesOfRelevantServiceTasksThatShouldBeCheckedBeforeSending()
			{
				return Array.Empty<string>();
			}
			public override IMessageGenerator<EU.Business.Declaration.CusEntryHeader> GetTransmissionGenerator(CusdecMessageFunction declarationMessageFunction)
			{
				throw new NotImplementedException();
			}
		}
	}
}
