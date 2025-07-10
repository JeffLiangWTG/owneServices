using CargoWise.EntityFramework.Testing;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.REMADV;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class PAYSTDMessageBuilderTest : TestCaseWithFactory
	{
		public void TestPaymentPartyCode()
		{
			messageBuilder = new PAYSTDMessageBuilder(entryHeader, payInfos);

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			AssertEquals("Drawback type", Constants.BankAccountOwnerType.DrawbackClaimant, messageBuilder.PaymentPartyCode);

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			AssertEquals("Broker will pay", Constants.BankAccountOwnerType.Broker, messageBuilder.PaymentPartyCode);

			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Importer;
			AssertEquals("Importer will pay", Constants.BankAccountOwnerType.Importer, messageBuilder.PaymentPartyCode);

			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.SecondBroker;
			AssertEquals("Second Broker will pay", Constants.BankAccountOwnerType.Broker, messageBuilder.PaymentPartyCode);
		}

		public void TestOverridenAbstractFields()
		{
			messageBuilder = new PAYSTDMessageBuilder(entryHeader, payInfos);

			AssertEquals("Message type", CMRMessage.CMRMessageTypes.PAYSTD, messageBuilder.EM_MessageType);
			AssertEquals("TypeOfMessage", typeof(CMRPAYSTDMessage), messageBuilder.TypeOfMessage);
			AssertEquals("Document Name Code", DocumentNameCodeList.RemittanceAdvice, messageBuilder.DocumentNameCode);
			AssertEquals("Document Name", "PAYSTD", messageBuilder.DocumentName);
			_ = messageBuilder.MessageText;
			AssertEquals("Type Of Edifact Message", typeof(REMADVMessage), messageBuilder.EdifactMessage.GetType());
			AssertNotNull("UNH Segment", messageBuilder.UNH);
			Assert("UNHMessageType", MessageTypeList.RemittanceAdviceMessage.Equals(messageBuilder.UNHMessageType));
			AssertNotNull("BGM Segment", messageBuilder.BGM);
			AssertNotNull("UNT segment", messageBuilder.UNT);
		}

		public void TestPopulateMOAs()
		{
			entryHeader.EntryNumber = "AAA111BBB";
			payInfos = new EFTPaymentInformationCollection(testDec);
			payInfos[0].AQISServicePaymentAmountPayableNow = 1000m;
			payInfos[0].CustomsChargeAmountPayableNow = 2000m;
			messageBuilder = new PAYSTDMessageBuilder(entryHeader, payInfos);

			AssertEquals("MOA for total paid exists", true, messageBuilder.MessageText.Contains("MOA+128:2000.00"));
			AssertEquals("MOA for AQIS exists", true, messageBuilder.MessageText.Contains("MOA+206:1000.00"));
		}

		public void TestPopulateUNS()
		{
			messageBuilder = new PAYSTDMessageBuilder(entryHeader, payInfos);

			AssertEquals("UNS", true, messageBuilder.MessageText.Contains("UNS+S"));
		}

		public void TestPopulateNAD()
		{
			messageBuilder = new PAYSTDMessageBuilder(entryHeader, payInfos);

			consignee.CustomsClientID = "AAA";
			var cusCode = consignee.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia);

			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "BBB";

			AssertEquals("NAD Client Code", true, messageBuilder.MessageText.Contains("NAD+IM+AAA::95"));
			AssertEquals("NAD Branch ID", true, messageBuilder.MessageText.Contains("NAD+VT+BBB::95"));

			messageBuilder = new PAYSTDMessageBuilder(entryHeader, payInfos);
			consignee.LocalBusinessRegNo = "12345678901";
			AssertEquals("NAD Client Code", false, messageBuilder.MessageText.Contains("NAD+IM+AAA::95"));
			AssertEquals("NAD ABN", true, messageBuilder.MessageText.Contains("NAD+IM+12345678901::95"));
		}

		public void TestPopulateGIS()
		{
			Env.Registry.RequestOfficialCustomsPaymentReceipt = true;

			messageBuilder = new PAYSTDMessageBuilder(entryHeader, payInfos);

			AssertEquals("GIS segment", true, messageBuilder.MessageText.Contains("GIS+Y:150:95"));
			AssertEquals("GIS segment", true, messageBuilder.MessageText.Contains("GIS+Y:153:95"));
			AssertEquals("GIS segment", true, messageBuilder.MessageText.Contains("GIS+POR:109:95"));
		}

		public void TestGISSegmentWithPORTurnedOffInRegistry()
		{
			Env.Registry.RequestOfficialCustomsPaymentReceipt = false;

			messageBuilder = new PAYSTDMessageBuilder(entryHeader, payInfos);

			AssertEquals("GIS segment", true, messageBuilder.MessageText.Contains("GIS+Y:150:95"));
			AssertEquals("GIS segment", true, messageBuilder.MessageText.Contains("GIS+Y:153:95"));
			AssertEquals("NO POR GIS segment", false, messageBuilder.MessageText.Contains("GIS+POR:109:95"));
		}

		public void TestPopulateFII()
		{
			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Importer;
			consignee.MiscServ.OM_IMEFTBankBSB = "23145";
			consignee.MiscServ.OM_IMEFTBankAccount = "231457895";
			messageBuilder = new PAYSTDMessageBuilder(entryHeader, payInfos);
			AssertEquals("FII segment", true, messageBuilder.MessageText.Contains("FII+COQ+231457895+:::23145"));

			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			AccBankAccount brokerAccount = Factory.New<AccBankAccount>();
			brokerAccount.AB_BSB = "123456";
			brokerAccount.AB_AccountNum = "123456789";
			Env.Registry.SetCustomsPaymentBankAccountForCurrentCompany(brokerAccount.PK.ToGuid());
			messageBuilder = new PAYSTDMessageBuilder(entryHeader, payInfos);
			AssertEquals("FII segment", true, messageBuilder.MessageText.Contains("FII+COQ+123456789+:::123456"));

			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.SecondBroker;
			AccBankAccount secondBrokerAccount = Factory.New<AccBankAccount>();
			secondBrokerAccount.AB_BSB = "222222";
			secondBrokerAccount.AB_AccountNum = "222222222";
			Env.Registry.SetCustomsSecondPaymentBankAccountForCurrentCompany(secondBrokerAccount.PK.ToGuid());
			messageBuilder = new PAYSTDMessageBuilder(entryHeader, payInfos);
			AssertEquals("FII segment", true, messageBuilder.MessageText.Contains("FII+COQ+222222222+:::222222"));
		}

		public void TestPopulateFIISetsPaymentMethodIfDefault()
		{
			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Default;
			entryHeader.EntryNumber = "AAA111BBB";
			payInfos = new EFTPaymentInformationCollection(testDec);

			AccBankAccount brokerAccount = Factory.New<AccBankAccount>();
			brokerAccount.AB_BSB = "123456";
			brokerAccount.AB_AccountNum = "123456789";
			Env.Registry.SetCustomsPaymentBankAccountForCurrentCompany(brokerAccount.PK.ToGuid());

			payInfos[0].CustomsChargeAmountPayableNow = 1000m;
			messageBuilder = new PAYSTDMessageBuilder(entryHeader, payInfos);
			AssertEquals("FII segment", true, messageBuilder.MessageText.Contains("FII+COQ+123456789+:::123456"));
			AssertEquals("Payment Method updated when sending a PAY message. This is necessary otherwise further actions like CUSDSB rating will not know who is paying the charges.", JobDeclaration.PaymentMethods.Broker, testDec.JE_PaymentMethod);

			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Default;
			payInfos = new EFTPaymentInformationCollection(testDec);
			payInfos[0].CustomsChargeAmountPayableNow = 0m;
			payInfos[0].AQISServicePaymentAmountPayableNow = 10m;
			messageBuilder = new PAYSTDMessageBuilder(entryHeader, payInfos);
			AssertEquals("FII segment", true, messageBuilder.MessageText.Contains("FII+COQ+123456789+:::123456"));
			AssertEquals("Payment Method not updated as this is AQIS payment only", JobDeclaration.PaymentMethods.Default, testDec.JE_PaymentMethod);

			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Default;
			payInfos = new EFTPaymentInformationCollection(testDec);
			payInfos[0].CustomsChargeAmountPayableNow = 100m;
			payInfos[0].AQISServicePaymentAmountPayableNow = 0m;
			messageBuilder = new PAYSTDMessageBuilder(entryHeader, payInfos);
			AssertEquals("FII segment", true, messageBuilder.MessageText.Contains("FII+COQ+123456789+:::123456"));
			AssertEquals("Payment Method updated as this is AQIS payment only", JobDeclaration.PaymentMethods.Broker, testDec.JE_PaymentMethod);
		}

		public void TestPopulateRFF()
		{
			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			entryHeader.EntryNumber = "A123456";
			messageBuilder = new PAYSTDMessageBuilder(entryHeader, payInfos);
			AssertEquals("RFF segment for Entry Number", true, messageBuilder.MessageText.Contains("RFF+ABT:A123456"));
			AssertEquals("RFF segment for payment pary", true, messageBuilder.MessageText.Contains("RFF+ANU:B"));
		}

		[NUnit.Framework.TestDate(2005, 6, 8)]
		public void TestPopulateDTM()
		{
			messageBuilder = new PAYSTDMessageBuilder(entryHeader, payInfos);
			AssertEquals("DTM segment", true, messageBuilder.MessageText.Contains("DTM+307:20050608:102"));
		}

		[NUnit.Framework.TestDate(2005, 6, 10)]
		public void TestEndToEnd()
		{
			string message = "UNH+" + EDIMessage.MessageNumberPlaceHolder + @"+REMADV:D:99B:UN
BGM+481:::PAYSTD+" + EDIMessage.SendersReferencePlaceHolder + "/" + Env.Registry.PhysicalServerID + @"1:1+9
DTM+307:20050610:102
RFF+ABT:AAAATWKNS
RFF+ANU:B
GIS+POR:109:95
GIS+Y:150:95
GIS+Y:153:95
NAD+IM+54321::95
NAD+VT+AA33HF::95
UNS+S
MOA+128:102.18
UNT+13+" + EDIMessage.MessageNumberPlaceHolder;

			Env.Registry.RequestOfficialCustomsPaymentReceipt = true;
			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			entryHeader.EntryNumber = "AAAATWKNS";
			payInfos = new EFTPaymentInformationCollection(testDec);

			AccBankAccount brokerAccount = Factory.New<AccBankAccount>();
			brokerAccount.AB_BSB = "034002";
			brokerAccount.AB_AccountNum = "509594";
			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "AA33HF";
			consignee.CustomsClientID = "54321";

			payInfos[0].CustomsChargeAmountPayableNow = 102.18m;
			messageBuilder = new PAYSTDMessageBuilder(entryHeader, payInfos);

			AssertMultilineASCIIEquals("PAY Message", message, messageBuilder.MessageText.Replace("'", "\r\n"));
		}

		JobDeclaration testDec;
		CusEntryHeader entryHeader;
		PAYSTDMessageBuilder messageBuilder;
		OrgHeader consignee;
		EFTPaymentInformationCollection payInfos;

		protected override void SetUp()
		{
			base.SetUp();
			testDec = JobDeclaration.New(Factory);
			entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "AAA111BBB";
			payInfos = new EFTPaymentInformationCollection(testDec);
			consignee = Factory.New<OrgHeader>();
			testDec.JE_OH_Importer = consignee.PK;
		}
	}
}
