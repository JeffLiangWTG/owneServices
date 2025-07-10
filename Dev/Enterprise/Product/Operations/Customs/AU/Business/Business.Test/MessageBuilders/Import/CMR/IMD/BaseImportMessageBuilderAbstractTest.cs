using System;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSDEC;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class BaseImportMessageBuilderAbstractTest : CMRTestCase
	{
		public void TestSetPaymentParty()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_DeclarationReference = "B00148999";
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Default;

			CusEntryHeaderToTestWith = declaration.CustomsEntryHeaders.AddNew();
			CusEntryHeaderToTestWith.CH_TotalPaid = 1000m;

			_ = GetMessageBuilderToTest(CMRMessageTypes.PreLodge).MessageText;
			AssertEquals("Payment party is set", false, declaration.JE_PaymentMethod == JobDeclaration.PaymentMethods.Default);

			var importer = Factory.New<OrgHeader>();
			importer.MiscServ.OM_IMEftCustomsFromImport = true;
			importer.MiscServ.OM_IMEFTBankBSB = "123456";
			importer.MiscServ.OM_IMEFTBankAccount = "123456";
			importer.MiscServ.OM_IMMaxEFTAmount = 50000m;

			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Default;

			var retriever = new PaymentDetailRetrieverIncludingAQIS(declaration, CMRMessageTypes.LodgeWithPay, null);
			AssertEquals("Importer will pay if it were default", PaymentParty.Importer, retriever.PartyToPayEntry);

			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			_ = GetMessageBuilderToTest(CMRMessageTypes.LodgeWithPay).MessageText;
			AssertEquals("Payment party is not set as it is not Default", JobDeclaration.PaymentMethods.Broker, declaration.JE_PaymentMethod);
		}

		[ExpectNoExceptions]
		public void TestNoExpectionsThrownForCreate()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_DeclarationReference = "B000148999";
			CusEntryHeaderToTestWith = declaration.CustomsEntryHeaders.AddNew();
			var message = GetMessageBuilderToTest(CMRMessageTypes.PreLodge).MessageText;
			Assert("Message is not empty", !message.IsEmpty);
		}

		public void TestIsAmendment()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();

			var builder = GetMessageBuilderToTest(CMRMessageTypes.PreLodge);
			AssertEquals("IsAmendment", false, builder.IsAmendment);

			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
			AssertEquals("IsAmendment", true, builder.IsAmendment);
		}

		public void TestMessageInterpretation()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForAir();
			var dummyBuilder = new DummyImportMessageBuilder(CusEntryHeaderToTestWith, CMRMessageTypes.PreLodge);
			AssertNull(dummyBuilder.AmendmentWithdrawalReason);
			AssertEquals("", dummyBuilder.MessageInterpretation);
			dummyBuilder.AmendmentWithdrawalReason = new CMRAmendmentWithdrawalReason();
			dummyBuilder.AmendmentWithdrawalReason.ReasonText = "Reason";
			AssertEquals("Reason", dummyBuilder.MessageInterpretation);
		}

		public abstract void TestLocationsSegment();

		public abstract void TestBGMSegment();

		public void TestBaseDTMSegment()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			testDec.JE_ExportDate = new ZDateTime(2005, 2, 15);
			testDec.JE_DateOfArrival = new ZDateTime(2005, 2, 16);

			var builder = GetMessageBuilderToTest(CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateDTM();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("DTM Segment is not empty", true, result.Contains("DTM+178:20050216:102'"));
		}

		public void TestDeliveryName()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var builder = GetMessageBuilderToTest(CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateFTX();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Delivery Name", true, result.Contains("FTX+DEL+++NAME OF IMPORTER'"));

			var deliveryAddress = testDec.Importer.Addresses.AddNew();
			deliveryAddress.AddressCapability.SetCapabilityEnabled(nameof(AddressType.PIC));
			testDec.ImporterDeliveryAddress.E2_OA_Address = deliveryAddress.PK;
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateFTX();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Delivery Name not overriden", true, result.Contains("FTX+DEL+++NAME OF IMPORTER'"));

			deliveryAddress.OA_CompanyNameOverride = "DELIVERY NAME";
			testDec.ImporterDeliveryAddress.E2_OA_Address = deliveryAddress.PK;
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateFTX();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Delivery Name overriden", true, result.Contains("FTX+DEL+++DELIVERY NAME'"));

			testDec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			builder = GetMessageBuilderToTest(CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateFTX();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Delivery Name", false, result.Contains("FTX+DEL"));
		}

		public abstract void TestFTXSegment();

		public void TestPopulateFTXWithEmptyString()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var builder = GetMessageBuilderToTest(CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateFTX(TextSubjectCodeQualifierList.Reason, "BLAH", "", "");
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Test", true, result.Contains("FTX+ACD+++BLAH"));
		}

		public void TestPopulateFTXForAmendments()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();

			var builder = GetMessageBuilderToTest(CMRMessageTypes.Amendment);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateFTX();
			builder.AmendmentWithdrawalReason = new CMRAmendmentWithdrawalReason();
			builder.AmendmentWithdrawalReason.ReasonText = "Test Change Reason";

			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Change Reason Code", false, result.Contains("FTX+CHG+++TEST CHANGE REASON"));

			builder = GetMessageBuilderToTest(CMRMessageTypes.Amendment);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
			builder.cUSDEC = new CUSDECMessage();
			builder.AmendmentWithdrawalReason = new CMRAmendmentWithdrawalReason();
			builder.AmendmentWithdrawalReason.ReasonText = "Test Change Reason";
			builder.PopulateFTX();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Change Reason Code", true, result.Contains("FTX+CHG+++TEST CHANGE REASON"));
		}

		public void TestPopulateFTXForWithdrawal()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();

			var builder = GetMessageBuilderToTest(CMRMessageTypes.Amendment);
			var result = builder.MessageText;
			builder.AmendmentWithdrawalReason = new CMRAmendmentWithdrawalReason();
			builder.AmendmentWithdrawalReason.ReasonText = longNoteValue;

			AssertEquals("Change Reason Code", false, result.Contains("FTX+CHG+++"
				+ "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
				"1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
				"12345678901234567890123456789012345678901234567890"));

			builder = GetMessageBuilderToTest(CMRMessageTypes.Withdrawal);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Withdraw;
			builder.AmendmentWithdrawalReason = new CMRAmendmentWithdrawalReason();
			builder.AmendmentWithdrawalReason.ReasonText = longNoteValue;
			result = builder.MessageText;
			AssertEquals("Change Reason Code", true, result.Contains("FTX+CHG+++"
				+ "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
				"1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
				"12345678901234567890123456789012345678901234567890"));
		}

		public void TestFIISegmentWithImporterBankAccount()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Importer;
			SetUpBankAccounts();

			var builder = GetMessageBuilderToTest(CMRMessageTypes.Payment);
			builder.cUSDEC = new CUSDECMessage();

			builder.AmendmentWithdrawalReason = new CMRAmendmentWithdrawalReason();
			builder.PopulateFII();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Bank Account Details - Importer", true, result.Contains("FII+COQ+987654+:::012345::215'"));
		}

		public void TestFIISegmentWithBrokerBankAccount()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;

			SetUpBankAccounts();

			var builder = GetMessageBuilderToTest(CMRMessageTypes.Payment);
			builder.cUSDEC = new CUSDECMessage();
			builder.AmendmentWithdrawalReason = new CMRAmendmentWithdrawalReason();

			builder.PopulateFII();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Bank Account Details - Broker", true, result.Contains("FII+COQ+323232+:::242200::215'"));
		}

		public void TestFIISegmentWithAmendmentWithoutPaymentMadeForBroker()
		{
			FIISegmentForAmendment(false, JobDeclaration.PaymentMethods.Broker);
		}

		public void TestFIISegmentWithAmendmentWithPaymentPendingForBroker()
		{
			FIISegmentForAmendment(false, JobDeclaration.PaymentMethods.Broker);
		}

		public void TestFIISegmentWithAmendmentWithNoAmountDueForBroker()
		{
			FIISegmentForAmendment(false, JobDeclaration.PaymentMethods.Broker);
		}

		public void TestFIISegmentWithAmendmentWithPaymentSentForBroker()
		{
			FIISegmentForAmendment(true, JobDeclaration.PaymentMethods.Broker);
		}

		public void TestFIISegmentWithAmendmentWithPaymentClearedForBroker()
		{
			FIISegmentForAmendment(true, JobDeclaration.PaymentMethods.Broker);
		}

		public void TestFIISegmentWithAmendmentWithPaymentRejectedForBroker()
		{
			FIISegmentForAmendment(false, JobDeclaration.PaymentMethods.Broker);
		}

		public void TestFIISegmentWithAmendmentWithRefundSentForBroker()
		{
			FIISegmentForAmendment(true, JobDeclaration.PaymentMethods.Broker);
		}

		public void TestFIISegmentWithAmendmentWithRefundClearedForBroker()
		{
			FIISegmentForAmendment(true, JobDeclaration.PaymentMethods.Broker);
		}

		public void TestFIISegmentWithAmendmentWithRefundRejectedForBroker()
		{
			FIISegmentForAmendment(true, JobDeclaration.PaymentMethods.Broker);
		}

		public void TestFIISegmentWithAmendmentWithoutPaymentMadeForImporter()
		{
			FIISegmentForAmendment(false, JobDeclaration.PaymentMethods.Importer);
		}

		public void TestFIISegmentWithAmendmentWithPaymentPendingForImporter()
		{
			FIISegmentForAmendment(false, JobDeclaration.PaymentMethods.Importer);
		}

		public void TestFIISegmentWithAmendmentWithNoAmountDueForImporter()
		{
			FIISegmentForAmendment(false, JobDeclaration.PaymentMethods.Importer);
		}

		public void TestFIISegmentWithAmendmentWithPaymentSentForImporter()
		{
			FIISegmentForAmendment(true, JobDeclaration.PaymentMethods.Importer);
		}

		public void TestFIISegmentWithAmendmentWithPaymentClearedForImporter()
		{
			FIISegmentForAmendment(true, JobDeclaration.PaymentMethods.Importer);
		}

		public void TestFIISegmentWithAmendmentWithPaymentRejectedForImporter()
		{
			FIISegmentForAmendment(false, JobDeclaration.PaymentMethods.Importer);
		}

		public void TestFIISegmentWithAmendmentWithRefundSentForImporter()
		{
			FIISegmentForAmendment(true, JobDeclaration.PaymentMethods.Importer);
		}

		public void TestFIISegmentWithAmendmentWithRefundClearedForImporter()
		{
			FIISegmentForAmendment(true, JobDeclaration.PaymentMethods.Importer);
		}

		public void TestFIISegmentWithAmendmentWithRefundRejectedForImporter()
		{
			FIISegmentForAmendment(true, JobDeclaration.PaymentMethods.Importer);
		}

		public void TestFIISegmentWithWithdrawalWithoutPaymentMadeForBroker()
		{
			FIISegmentForWithdrawal(false, JobDeclaration.PaymentMethods.Broker);
		}

		public void TestFIISegmentWithWithdrawalWithPaymentPendingForBroker()
		{
			FIISegmentForWithdrawal(false, JobDeclaration.PaymentMethods.Broker);
		}

		public void TestFIISegmentWithAWithdrawalWithNoAmountDueForBroker()
		{
			FIISegmentForWithdrawal(false, JobDeclaration.PaymentMethods.Broker);
		}

		public void TestFIISegmentWithWithdrawalWithPaymentSentForBroker()
		{
			FIISegmentForWithdrawal(true, JobDeclaration.PaymentMethods.Broker);
		}

		public void TestFIISegmentWithWithdrawalWithPaymentClearedForBroker()
		{
			FIISegmentForWithdrawal(true, JobDeclaration.PaymentMethods.Broker);
		}

		public void TestFIISegmentWithWithdrawalWithPaymentRejectedForBroker()
		{
			FIISegmentForWithdrawal(false, JobDeclaration.PaymentMethods.Broker);
		}

		public void TestFIISegmentWithWithdrawalWithRefundSentForBroker()
		{
			FIISegmentForWithdrawal(true, JobDeclaration.PaymentMethods.Broker);
		}

		public void TestFIISegmentWithWithdrawalWithRefundClearedForBroker()
		{
			FIISegmentForWithdrawal(true, JobDeclaration.PaymentMethods.Broker);
		}

		public void TestFIISegmentWithWithdrawalWithRefundRejectedForBroker()
		{
			FIISegmentForWithdrawal(true, JobDeclaration.PaymentMethods.Broker);
		}

		public void TestFIISegmentWithWithdrawalWithoutPaymentMadeForImporter()
		{
			FIISegmentForWithdrawal(false, JobDeclaration.PaymentMethods.Importer);
		}

		public void TestFIISegmentWithWithdrawalWithPaymentPendingForImporter()
		{
			FIISegmentForWithdrawal(false, JobDeclaration.PaymentMethods.Importer);
		}

		public void TestFIISegmentWithAWithdrawalWithNoAmountDueForImporter()
		{
			FIISegmentForWithdrawal(false, JobDeclaration.PaymentMethods.Importer);
		}

		public void TestFIISegmentWithWithdrawalWithPaymentSentForImporter()
		{
			FIISegmentForWithdrawal(true, JobDeclaration.PaymentMethods.Importer);
		}

		public void TestFIISegmentWithWithdrawalWithPaymentClearedForImporter()
		{
			FIISegmentForWithdrawal(true, JobDeclaration.PaymentMethods.Importer);
		}

		public void TestFIISegmentWithWithdrawalWithPaymentRejectedForImporter()
		{
			FIISegmentForWithdrawal(false, JobDeclaration.PaymentMethods.Importer);
		}

		public void TestFIISegmentWithWithdrawalWithRefundSentForImporter()
		{
			FIISegmentForWithdrawal(true, JobDeclaration.PaymentMethods.Importer);
		}

		public void TestFIISegmentWithWithdrawalWithRefundClearedForImporter()
		{
			FIISegmentForWithdrawal(true, JobDeclaration.PaymentMethods.Importer);
		}

		public void TestFIISegmentWithWithdrawalWithRefundRejectedForImporter()
		{
			FIISegmentForWithdrawal(true, JobDeclaration.PaymentMethods.Importer);
		}

		public void TestFIISegment()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Default;

			BaseImportMessageBuilder builder = GetMessageBuilderToTest(CMRMessageTypes.Payment);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateFII();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Bank Account Details - Empty as EFT not approved", false, result.Contains("FII+COQ"));

			SetUpBankAccounts();

			builder = GetMessageBuilderToTest(CMRMessageTypes.Payment);
			builder.cUSDEC = new CUSDECMessage();
			builder.AmendmentWithdrawalReason = new CMRAmendmentWithdrawalReason();
			builder.PopulateFII();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Bank Account Details - Broker", true, result.Contains("FII+COQ+323232+:::242200::215'"));

			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			builder = GetMessageBuilderToTest(CMRMessageTypes.Payment);
			builder.cUSDEC = new CUSDECMessage();
			builder.AmendmentWithdrawalReason = new CMRAmendmentWithdrawalReason();

			builder.PopulateFII();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Bank Account Details - Broker", true, result.Contains("FII+COQ+323232+:::242200::215'"));

			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Importer;
			builder = GetMessageBuilderToTest(CMRMessageTypes.Payment);
			builder.cUSDEC = new CUSDECMessage();
			builder.AmendmentWithdrawalReason = new CMRAmendmentWithdrawalReason();

			builder.PopulateFII();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Bank Account Details - Importer", true, result.Contains("FII+COQ+987654+:::012345::215'"));
		}

		public abstract void TestGISSegment();

		public void TestPopulateGISSegment()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var expectedResult = "GIS+N:153:95'";

			CusEntryHeaderToTestWith.Declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Cash;
			var builder = GetMessageBuilderToTest(CMRMessageTypes.LodgeWithoutPay);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGIS();

			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("GIS Segment", true, result.Contains(expectedResult));

			builder = GetMessageBuilderToTest(CMRMessageTypes.LodgeWithPay);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGIS();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			expectedResult = "GIS+EPA:109:95'GIS+N:153:95'";
			AssertEquals("GIS Segment", true, result.Contains(expectedResult));

			CusEntryHeaderToTestWith.Declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			builder = GetMessageBuilderToTest(CMRMessageTypes.LodgeWithPay);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGIS();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			expectedResult = "GIS+EPA:109:95'GIS+Y:153:95'";
			AssertEquals("GIS Segment", true, result.Contains(expectedResult));
		}

		public void TestPopulateGISSegmentForAmendment()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			testDec.AddInfo.ZA_EffectDutyDate_Hidden = true;
			var builder = GetMessageBuilderToTest(CMRMessageTypes.Amendment);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGIS();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Re-Calc Effective Duty Date", true, result.Contains("GIS+EFD:109:95'"));

			testDec.AddInfo.ZA_EffectDutyDate_Hidden = false;
			builder = GetMessageBuilderToTest(CMRMessageTypes.Amendment);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGIS();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Re-Calc Effective Duty Date", false, result.Contains("GIS+EFD:109:95'"));
		}

		public abstract void TestSegmentGroup1();

		public void TestBankOwnerTypeInGroup1()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Default;

			var builder = GetMessageBuilderToTest(CMRMessageTypes.LodgeWithoutPay);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Bank Account Owner Type - Empty as not EFT not approved", false, result.Contains("RFF+ANU"));

			builder = GetMessageBuilderToTest(CMRMessageTypes.LodgeWithPay);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Bank Account Owner Type - Broker", true, result.Contains("RFF+ANU:B'"));

			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			builder = GetMessageBuilderToTest(CMRMessageTypes.LodgeWithPay);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Bank Account Owner Type - Broker", true, result.Contains("RFF+ANU:B'"));

			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Importer;
			builder = GetMessageBuilderToTest(CMRMessageTypes.LodgeWithPay);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Bank Account Owner Type - Importer", true, result.Contains("RFF+ANU:I'"));
		}

		public void TestGroup1WithAmendmentWithoutPaymentMade()
		{
			Group1ForAmendment(false);
		}

		public void TestGroup1WithAmendmentWithPaymentPending()
		{
			Group1ForAmendment(false);
		}

		public void TestGroup1WithAmendmentWithNoAmountDue()
		{
			Group1ForAmendment(false);
		}

		public void TestGroup1WithAmendmentWithPaymentSent()
		{
			Group1ForAmendment(true);
		}

		public void TestGroup1WithAmendmentWithPaymentCleared()
		{
			Group1ForAmendment(true);
		}

		public void TestGroup1WithAmendmentWithPaymentRejected()
		{
			Group1ForAmendment(false);
		}

		public void TestGroup1WithAmendmentWithRefundSent()
		{
			Group1ForAmendment(true);
		}

		public void TestGroup1WithAmendmentWithRefundCleared()
		{
			Group1ForAmendment(true);
		}

		public void TestGroup1WithAmendmentWithRefundRejected()
		{
			Group1ForAmendment(true);
		}

		public void TestGroup1WithWithdrawalWithoutPaymentMade()
		{
			Group1ForWithdrawal(false);
		}

		public void TestGroup1WithWithdrawalWithPaymentPending()
		{
			Group1ForWithdrawal(false);
		}

		public void TestGroup1WithWithdrawalWithNoAmountDue()
		{
			Group1ForWithdrawal(false);
		}

		public void TestGroup1WithWithdrawalWithPaymentSent()
		{
			Group1ForWithdrawal(true);
		}

		public void TestGroup1WithWithdrawalWithPaymentCleared()
		{
			Group1ForWithdrawal(true);
		}

		public void TestGroup1WithWithdrawalWithPaymentRejected()
		{
			Group1ForWithdrawal(false);
		}

		public void TestGroup1WithWithdrawalWithRefundSent()
		{
			Group1ForWithdrawal(true);
		}

		public void TestGroup1WithWithdrawalWithRefundCleared()
		{
			Group1ForWithdrawal(true);
		}

		public void TestGroup1WithWithdrawalWithRefundRejected()
		{
			Group1ForWithdrawal(true);
		}

		public void TestPopulateGroup1ForWithdrawal()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			CusEntryHeaderToTestWith.EntryNumber = "Entry Number";
			var builder = GetMessageBuilderToTest(CMRMessageTypes.Withdrawal);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Withdraw;
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1ForWithdrawal();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Import Dec Id", true, result.Contains("RFF+ABT:ENTRY NUMBER'"));
		}

		public void TestAgentAndDeclarationReference()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var builder = GetMessageBuilderToTest(CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Import Dec Id", true, result.Contains("RFF+ADU:<<AGENT REFERENCE PLACE HOLDER>>'"));
		}

		public void TestPopulateAgentReferenceForFAR()
		{
			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.FAR;

			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var builder = GetMessageBuilderToTest(CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Import Dec Id", true, result.Contains("RFF+ADU:<<AGENT REFERENCE PLACE HOLDER>>'"));
		}

		public void TestPopulateAgentReferenceForNSRWhenAgentReferenceIsEmpty()
		{
			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.NSR;

			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var builder = GetMessageBuilderToTest(CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Import Dec Id", false, result.Contains("RFF+ADU:<<AGENT REFERENCE PLACE HOLDER>>'"));
		}

		public void TestPopulateAgentReferenceForNSRWhenAgentReferenceIsNotEmpty()
		{
			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.NSR;

			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			testDec.JE_AgentsReference = "Agent";
			var builder = GetMessageBuilderToTest(CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Import Dec Id", true, result.Contains("RFF+ADU:<<AGENT REFERENCE PLACE HOLDER>>'"));
		}

		public void TestPopulateAgentReferenceForPARWhenAgentReferenceIsEmpty()
		{
			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.PAR;

			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var builder = GetMessageBuilderToTest(CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Import Dec Id", false, result.Contains("RFF+ADU:<<AGENT REFERENCE PLACE HOLDER>>'"));
		}

		public void TestPopulateAgentReferenceForPARWhenAgentReferenceIsNotEmpty()
		{
			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.PAR;

			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			testDec.JE_AgentsReference = "Agent";
			var builder = GetMessageBuilderToTest(CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Import Dec Id", true, result.Contains("RFF+ADU:<<AGENT REFERENCE PLACE HOLDER>>'"));
		}

		public void TestPopulateAgentReferenceForDEF()
		{
			Env.Registry.AUCustoms.AgentsReferenceDefaulting = Core.Constants.AgentsReferenceDefaulting.DEF;

			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var builder = GetMessageBuilderToTest(CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Import Dec Id", true, result.Contains("RFF+ADU:<<AGENT REFERENCE PLACE HOLDER>>'"));
		}

		public void TestAgentReferenceException()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			testDec.JE_DeclarationReference = ZString.Empty;
			var builder = GetMessageBuilderToTest(CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Import Dec Id", true, result.Contains("RFF+ADU:<<AGENT REFERENCE PLACE HOLDER>>'"));
		}

		public void TestOwnersReferenceSupplied()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			PopulateWithValidTestData(testDec);
			testDec.Importer.MiscServ.OM_IMAutoImpJobRefered = false;
			testDec.JE_OwnerRef = "OWNER REF";
			var builder = GetMessageBuilderToTest(CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals(true, result.Contains("RFF+ABQ:<<OWNER REFERENCE PLACE HOLDER>>'"));
		}

		public void TestOwnersReferenceAuto()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			PopulateWithValidTestData(testDec);
			testDec.Importer.MiscServ.OM_IMAutoImpJobRefered = true;
			testDec.JE_OwnerRef = ZString.Empty;
			var builder = GetMessageBuilderToTest(CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals(true, result.Contains("RFF+ABQ:<<OWNER REFERENCE PLACE HOLDER>>'"));
		}

		public void TestNoOwnersReference()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			PopulateWithValidTestData(testDec);
			testDec.Importer.MiscServ.OM_IMAutoImpJobRefered = false;
			testDec.JE_OwnerRef = ZString.Empty;
			var builder = GetMessageBuilderToTest(CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup1();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals(false, result.Contains("RFF+ABQ"));
		}

		public abstract void TestSegmentGroup4();

		public void TestDoNotGenerateTDTWhenVoyageOrLloysNumberIsMissing()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			CusEntryHeaderToTestWith.Declaration.JE_VoyageFlightNo = "112233445X";

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "TESTVESSEL";
			vessel.RV_LloydsNumber = "789445X";
			CusEntryHeaderToTestWith.Declaration.JE_VesselName = vessel.RV_Code;

			var builder = GetMessageBuilderToTest(CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup4();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Has transport details", true, result.Contains("TDT+20+112233445X+S+++++789445X::11"));

			vessel.RV_LloydsNumber = "";
			builder = GetMessageBuilderToTest(CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup4();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Has transport details", false, result.Contains("TDT+20+112233445X+S+++++::11"));

			CusEntryHeaderToTestWith.Declaration.JE_VoyageFlightNo = "";
			vessel.RV_LloydsNumber = "789445X";
			builder = GetMessageBuilderToTest(CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup4();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Has transport details", false, result.Contains("TDT+20++S+++++789445X::11"));
		}

		public void TestCustomsShipNumber()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			CusEntryHeaderToTestWith.Declaration.JE_VoyageFlightNo = "112233445X";
			CusEntryHeaderToTestWith.Declaration.ZA_CustShipNo_Hidden = "X1234567";

			var builder = GetMessageBuilderToTest(CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup4();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Has transport details", true, result.Contains("TDT+20+112233445X+S+++++X1234567::11"));
		}

		public void TestSegmentGroup4ForNature30()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var builder = GetMessageBuilderToTest(CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup4();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Has transport details", true, result.Contains("TDT+20+"));

			testDec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			builder = GetMessageBuilderToTest(CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup4();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Does not have transport details", false, result.Contains("TDT+20+"));
		}

		public abstract void TestSegmentGroup6();

		public void TestDeliveryAddressInSegmentGroup6()
		{
			var declaration = JobDeclaration.New(Factory);

			var importer = OrgHeader.New(Factory);
			var deliveryAddress = importer.Addresses.AddNew();
			deliveryAddress.OA_Address1 = "12345678901234567890123456789012345678901234567890";     // 50 chars
			deliveryAddress.OA_Address2 = "12345678901234567890123456789012345678901234567890";     // 50 chars
			deliveryAddress.AddressCapability.SetCapabilityEnabled(nameof(AddressType.PIC));
			deliveryAddress.OA_City = "1234567890123456789012345";
			deliveryAddress.OA_State = "NSW";
			deliveryAddress.OA_PostCode = "1234567890";

			declaration.JE_OH_Importer = importer.PK;
			declaration.ImporterDeliveryAddress.E2_OA_Address = deliveryAddress.PK;

			CusEntryHeaderToTestWith = declaration.CustomsEntryHeaders.AddNew();

			var result = GetMessageWithGroup6();
			//								  City (25)				     Address 1 (35)
			var expectedResult = "NAD+DP++1234567890123456789012345++12345678901234567890123456789012345:" +
				//	 Address 1 (5)	  Address 2 (35) 	  Address 2 (5)       State Postcode Country
				"67890:12345678901234567890123456789012345:67890++:::NSW+1234567890+AU'";

			AssertContains("NAD Segment", expectedResult, result);
		}

		public void TestDeliveryAddressForNature30()
		{
			var declaration = JobDeclaration.New(Factory);

			var importer = OrgHeader.New(Factory);
			var deliveryAddress = importer.Addresses.AddNew();
			deliveryAddress.OA_Address1 = "12345678901234567890123456789012345678901234567890";
			declaration.JE_OH_Importer = importer.PK;
			declaration.ImporterDeliveryAddress.E2_OA_Address = deliveryAddress.PK;

			CusEntryHeaderToTestWith = declaration.CustomsEntryHeaders.AddNew();
			var result = GetMessageWithGroup6();
			AssertContains("NAD Segment", "NAD+DP++", result);

			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			result = GetMessageWithGroup6();
			AssertNotContains("NAD Segment", "NAD+DP++", result);
		}

		public void TestImporterABNInSegmentGroup6()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			testDec.Importer.LocalBusinessRegNo = "12345678901";

			var result = GetMessageWithGroup6();
			AssertContains("Importer ABN", "NAD+AT+12345678901::95'", result);
		}

		public void TestImporterIDInSegmentGroup6()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			testDec.Importer.LocalBusinessRegNo = ZString.Empty;
			testDec.Importer.CustomsClientID = "12345678901";
			var cusCode = testDec.Importer.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia);
			cusCode.OK_OA_PremisesAddress = Factory.New<OrgAddress>().PK;

			var result = GetMessageWithGroup6();
			AssertNotContains("Importer ID", "NAD+IM+12345678901::95'", result);
			cusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			result = GetMessageWithGroup6();
			AssertContains("Importer ID", "NAD+IM+12345678901::95'", result);

			testDec.Importer.CustomsClientID = "098765432109876543210";
			result = GetMessageWithGroup6();
			AssertContains("Importer ID", "NAD+IM+09876543210::95'", result);
		}

		public void TestBranchIDInSegmentGroup6()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			Env.Registry.AUCustoms.SetLocalCustomsBranchIdentifierForBranch(GlbBranch.CurrentBranch.PK.ToGuid(), "AA33HF");

			var result = GetMessageWithGroup6();
			AssertContains("Branch ID", "NAD+VT+AA33HF::95'", result);
		}

		public void TestImporterCACInSegmentGroup6()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			testDec.Importer.LocalBusinessRegNo = "12345678901CAC ";

			var builder = GetMessageBuilderToTest(CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup6();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertContains("Importer ABN", "NAD+AT+12345678901::95'", result);
			AssertContains("Importer CAC from ABN", "NAD+WP+CAC::95'", result);
			AssertEquals("There are 4 SegmentGroup6", 4, builder.cUSDEC.Group6.Count);

			var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia);
			testDec.Importer.SetCustomsCode(OrgCusCode.CodeTypes.CreditAgencyCode, au, "954 ");
			result = GetMessageWithGroup6();
			AssertContains("Importer ABN", "NAD+AT+12345678901::95'", result);
			AssertContains("Importer CAC in Cus Codes overrides ABN", "NAD+WP+954::95'", result);

			testDec.Importer.LocalBusinessRegNo = "12345678901";
			result = GetMessageWithGroup6();
			AssertContains("Importer ABN", "NAD+AT+12345678901::95'", result);
			AssertContains("Importer CAC from Cus Codes", "NAD+WP+954::95'", result);

			testDec.Importer.SetCustomsCode(OrgCusCode.CodeTypes.CreditAgencyCode, au, "");
			result = GetMessageWithGroup6();
			AssertContains("Importer ABN", "NAD+AT+12345678901::95'", result);
			AssertNotContains("Importer CAC not defined", "NAD+WP+", result);
		}

		public abstract void TestSegmentGroup30();

		public void TestPopulateUNS1()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var expectedSection = "UNS+D'";
			var builder = GetMessageBuilderToTest(CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateUNS1();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Segment UNS 1", expectedSection, result);
		}

		public void TestPopulateUNS2()
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var expectedSection = "UNS+S'";
			var builder = GetMessageBuilderToTest(CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateUNS2();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Segment UNS 2", expectedSection, result);
		}

		protected virtual CusEntryHeader CusEntryHeaderToTestWith
		{
			get => cusEntryHeader;
			set => cusEntryHeader = value;
		}
		CusEntryHeader cusEntryHeader;

		protected abstract BaseImportMessageBuilder GetMessageBuilderToTest(CMRMessageTypes messageType);

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(AutoCMRAqisPremises.Schema.TableName);
		}

		void SetUpBankAccounts()
		{
			var account = Factory.New<AccBankAccount>();
			account.AB_BankName = "Deborah Spagarino Test Account";
			account.AB_BSB = "242200";
			account.AB_AccountNum = "323232";

			Env.Registry.SetCustomsPaymentBankAccountForCurrentCompany(account.PK.ToGuid());

			testDec.Importer.OH_FullName = "Importer Full Name 012345678901234567890NOT";
			testDec.Importer.MiscServ.OM_IMEFTBankAccount = "987654";
			testDec.Importer.MiscServ.OM_IMEFTBankBSB = "012345";
		}

		void FIISegmentForAmendment(bool detailsIncluded, string paymentMethod)
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var builder = GetMessageBuilderToTest(CMRMessageTypes.Amendment);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
			testDec.JE_PaymentMethod = paymentMethod;

			SetUpBankAccounts();
			builder.cUSDEC = new CUSDECMessage();
			builder.AmendmentWithdrawalReason = new CMRAmendmentWithdrawalReason();

			CusEntryHeaderToTestWith.AddInfo.ZA_IsPAYRECAck_Hidden = detailsIncluded;
			builder.PopulateFII();
			ZString result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Bank Account Details included", detailsIncluded && paymentMethod == JobDeclaration.PaymentMethods.Broker, result.Contains("FII+COQ+323232+:::242200::215'"));
			AssertEquals("Bank Account Details included", detailsIncluded && paymentMethod == JobDeclaration.PaymentMethods.Importer, result.Contains("FII+COQ+987654:IMPORTER FULL NAME 0123456789012345:67890+:::012345::215'"));

			CusEntryHeaderToTestWith.AddInfo.ZA_IsPAYRECAck_Hidden = false;
			if (detailsIncluded)
			{
				CusEntryHeaderToTestWith.Charges.AddNew(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 100m);
			}
			builder.PopulateFII();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Bank Account Details included Duty Deferred", detailsIncluded && paymentMethod == JobDeclaration.PaymentMethods.Broker, result.Contains("FII+COQ+323232+:::242200::215'"));
			AssertEquals("Bank Account Details included Duty Deferred", detailsIncluded && paymentMethod == JobDeclaration.PaymentMethods.Importer, result.Contains("FII+COQ+987654:IMPORTER FULL NAME 0123456789012345:67890+:::012345::215'"));
		}

		void FIISegmentForWithdrawal(bool detailsIncluded, string paymentMethod)
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var builder = GetMessageBuilderToTest(CMRMessageTypes.Withdrawal);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Withdraw;
			testDec.JE_PaymentMethod = paymentMethod;

			SetUpBankAccounts();
			builder.cUSDEC = new CUSDECMessage();
			builder.AmendmentWithdrawalReason = new CMRAmendmentWithdrawalReason();

			CusEntryHeaderToTestWith.AddInfo.ZA_IsPAYRECAck_Hidden = detailsIncluded;
			builder.PopulateFII();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Bank Account Details included", detailsIncluded && paymentMethod == JobDeclaration.PaymentMethods.Broker, result.Contains("FII+COQ+323232+:::242200::215'"));
			AssertEquals("Bank Account Details included", detailsIncluded && paymentMethod == JobDeclaration.PaymentMethods.Importer, result.Contains("FII+COQ+987654:IMPORTER FULL NAME 0123456789012345:67890+:::012345::215'"));

			CusEntryHeaderToTestWith.AddInfo.ZA_IsPAYRECAck_Hidden = false;
			if (detailsIncluded)
			{
				CusEntryHeaderToTestWith.Charges.AddNew(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 100m);
			}
			builder.PopulateFII();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Bank Account Details included Duty Deferred", detailsIncluded && paymentMethod == JobDeclaration.PaymentMethods.Broker, result.Contains("FII+COQ+323232+:::242200::215'"));
			AssertEquals("Bank Account Details included Duty Deferred", detailsIncluded && paymentMethod == JobDeclaration.PaymentMethods.Importer, result.Contains("FII+COQ+987654:IMPORTER FULL NAME 0123456789012345:67890+:::012345::215'"));
		}

		void Group1ForWithdrawal(bool detailsIncluded)
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var builder = GetMessageBuilderToTest(CMRMessageTypes.Withdrawal);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;

			SetUpBankAccounts();
			builder.cUSDEC = new CUSDECMessage();
			builder.AmendmentWithdrawalReason = new CMRAmendmentWithdrawalReason();

			CusEntryHeaderToTestWith.AddInfo.ZA_IsPAYRECAck_Hidden = detailsIncluded;
			builder.PopulateGroup1ForWithdrawal();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Bank Account Owner Type - Broker", detailsIncluded, result.Contains("RFF+ANU:B'"));

			CusEntryHeaderToTestWith.AddInfo.ZA_IsPAYRECAck_Hidden = false;
			if (detailsIncluded)
			{
				CusEntryHeaderToTestWith.Charges.AddNew(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 100m);
			}
			builder.PopulateGroup1ForWithdrawal();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Bank Account Owner Type - Broker Duty Deferred", detailsIncluded, result.Contains("RFF+ANU:B'"));

			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Importer;
			builder.PopulateGroup1ForWithdrawal();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Bank Account Owner Type - Importer", detailsIncluded, result.Contains("RFF+ANU:I'"));
		}

		void Group1ForAmendment(bool detailsIncluded)
		{
			CusEntryHeaderToTestWith = GetCusEntryHeaderForSea();
			var builder = GetMessageBuilderToTest(CMRMessageTypes.Amendment);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;

			SetUpBankAccounts();
			builder.cUSDEC = new CUSDECMessage();
			builder.AmendmentWithdrawalReason = new CMRAmendmentWithdrawalReason();

			CusEntryHeaderToTestWith.AddInfo.ZA_IsPAYRECAck_Hidden = detailsIncluded;
			builder.PopulateGroup1();
			var result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Bank Account Owner Type - Broker", detailsIncluded, result.Contains("RFF+ANU:B'"));

			CusEntryHeaderToTestWith.AddInfo.ZA_IsPAYRECAck_Hidden = false;
			if (detailsIncluded)
			{
				CusEntryHeaderToTestWith.Charges.AddNew(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 100m);
			}
			builder.PopulateGroup1();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Bank Account Owner Type - Broker Duty Deferred", detailsIncluded, result.Contains("RFF+ANU:B'"));

			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Importer;
			builder.PopulateGroup1();
			result = builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Bank Account Owner Type - Importer", detailsIncluded, result.Contains("RFF+ANU:I'"));
		}

		ZString GetMessageWithGroup6()
		{
			var builder = GetMessageBuilderToTest(CMRMessageTypes.PreLodge);
			builder.cUSDEC = new CUSDECMessage();
			builder.PopulateGroup6();
			return builder.EdifactMessage.ToString(new Edifact.UNOCCMRCharacterSet());
		}

		sealed class DummyImportMessageBuilder : BaseImportMessageBuilder
		{
			public DummyImportMessageBuilder(CusEntryHeader entryHeader, CMRMessageTypes messageType)
				: base(entryHeader, messageType)
			{
			}

			protected internal override ZString DocumentName => new ZString();

			protected internal override DocumentNameCodeList DocumentNameCode => DocumentNameCodeList.GetFromString("DummyDocumentName");

			protected internal override Type TypeOfMessage => typeof(CMRMessage);

			protected internal override ZString EM_MessageType => new ZString();

			protected override void PopulateAirDetails()
			{
			}

			protected internal override void PopulateGroup8()
			{
			}

			protected internal override void PopulateGroup30()
			{
			}
		}
	}
}
