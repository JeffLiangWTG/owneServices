using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class REFACCInfoProviderTest : D99BCUSRESInfoProviderTest
	{
		public void TestTotalPayableWithNoPayInfo()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();

			CMRSACRMessage sACRMessage = Factory.New<CMRSACRMessage>();
			sACRMessage.EM_LinkedObject = entryHeader;
			sACRMessage.EM_MessageText = TestMessages.SACRMessageTextNegative;
			sACRMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			entryHeader.Messages.Add(sACRMessage);

			AssertEquals("EntryHeader's total payable advised on last clearance message", -16m, entryHeader.TotalPayableAdvisedInLastClearanceMessage);
			AssertEquals("EntryHeader's total payable DUE advised on last clearance message", -16m, entryHeader.TotalPayableDueAdvisedInLastClearanceMessage);

			entryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.Paid;
			AssertEquals("EntryHeader's total payable advised on last clearance message", -16m, entryHeader.TotalPayableAdvisedInLastClearanceMessage);
			AssertEquals("EntryHeader's total payable DUE advised on last clearance message", 0m, entryHeader.TotalPayableDueAdvisedInLastClearanceMessage);

			CUSRESMessage rEFACC = new CUSRESMessage();
			REFACCInfoProvider infoProvider = new REFACCInfoProvider(rEFACC, sACRMessage);
			AssertEquals("TotalAmountToBeRefunded", -16m, infoProvider.TotalAmountToBeRefunded);
		}

		public void TestTotalPayableWithPayInfoAndOtherProperties()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();

			CMRIMDMessage iMDMessage = (CMRIMDMessage)entryHeader.Messages.AddNew(typeof(CMRIMDMessage));
			iMDMessage.EM_MessageText = CMRImportDeclarationTestData.IMDWithPayment;
			iMDMessage.EM_LinkedObject = entryHeader;
			iMDMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			iMDMessage.EM_MessageNum = "1";

			CMRIMDRMessage iMDRmessage = (CMRIMDRMessage)entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			iMDRmessage.EM_MessageText = CMRImportDeclarationTestData.IMDRClear.Replace("RFF+ABO:B00122382/1/MEL2::2", "RFF+ABO:B00122382/1/SYD1::7");
			iMDRmessage.EM_LinkedObject = entryHeader;
			iMDRmessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			iMDRmessage.EM_MessageNum = "2";

			CMRREFACCMessage rEFACCmessage = (CMRREFACCMessage)entryHeader.Messages.AddNew(typeof(CMRREFACCMessage));
			rEFACCmessage.EM_MessageText = CMRImportDeclarationTestData.REFACC;
			iMDMessage.EM_LinkedObject = entryHeader;
			rEFACCmessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			rEFACCmessage.EM_MessageNum = "3";

			CusEntryPayInfo payInfo = entryHeader.EntryPayInfos.AddNew();
			payInfo.C9_IncomingPayResponseNo = iMDRmessage.EM_MessageNum;
			payInfo.C9_PaymentAmount = -123.45m;
			payInfo.C9_PaymentDate = new ZDateTime(2010, 1, 3, 12, 12, 12);

			CUSRESMessage rEFACC = new CUSRESMessage();
			REFACCInfoProvider infoProvider = new REFACCInfoProvider(rEFACC, rEFACCmessage);
			AssertEquals("TotalAmountToBeRefunded", -123.45m, infoProvider.TotalAmountToBeRefunded);
			AssertEquals("BankAccountName", "BROKER", infoProvider.BankAccountName);
			AssertEquals("BankAccountNumber", "323232", infoProvider.BankAccountNumber);
			AssertEquals("BSBNumber", "242200", infoProvider.BSBNumber);
			AssertEquals("PaymentFinalisedDate", payInfo.C9_PaymentDate, infoProvider.PaymentFinalisedDate);
		}

		public override void TestDocumentName()
		{
			CUSRESMessage rEFACC = new CUSRESMessage();
			REFACCInfoProvider infoProvider = new REFACCInfoProvider(rEFACC, message);
			AssertEquals("Document name", "REFACC", infoProvider.DocumentName);
		}

		public void TestEFTRunNumber()
		{
			CUSRESMessage rEFACC = new CUSRESMessage();
			REFACCInfoProvider infoProvider = new REFACCInfoProvider(rEFACC, message);

			SegmentGroup3 group3 = rEFACC.Group3.InstantiateAChildAndAddItToChildrenCollection();
			RFFSegment rFF = group3.RFF.InstantiateAChildAndAddItToChildrenCollection();
			rFF.Reference.ReferenceFunctionCodeQualifier = ReferenceFunctionCodeQualifierList.BanksCommonTransactionReferenceNumber;
			rFF.Reference.ReferenceIdentifier = "BBBXXXYY1";
			AssertEquals("EFTRunNumber", "BBBXXXYY1", infoProvider.EFTRunNumber);
		}

		public void TestClaimNumber()
		{
			CUSRESMessage rEFACC = new CUSRESMessage();
			REFACCInfoProvider infoProvider = new REFACCInfoProvider(rEFACC, message);

			FTXSegment fTX = rEFACC.FTX.InstantiateAChildAndAddItToChildrenCollection();
			fTX.TextLiteral.FreeTextValue1 = "AAABBB111";
			fTX.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.AdditionalInformation;
			AssertEquals("ClaimNumber", "AAABBB111", infoProvider.ClaimNumber);
		}

		protected override ZDateTime ExpectedPaymentFinalisedDate
		{
			get
			{
				return Env.Time.GetLocalTimeFromUtc(new DateTime(2010, 01, 02, 12, 12, 12, 0));
			}
		}

		protected override CUSRESMessage GetCUSRESMessage()
		{
			return new CUSRESMessage();
		}

		protected override D99BCUSRESInfoProvider GetInfoProvider(CUSRESMessage cUSRES)
		{
			return new REFACCInfoProvider(cUSRES, message);
		}

		CMRREFACCMessage message
		{
			get
			{
				if (_message == null)
				{
					JobDeclaration testDec = JobDeclaration.New(Factory);
					CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
					_message = (CMRREFACCMessage)entryHeader.Messages.AddNew(typeof(CMRREFACCMessage));
					_message.EM_SystemCreateTimeUtc = new ZDateTime(2010, 01, 02, 12, 12, 12);
					_message.EM_LinkedObject = entryHeader;
					_message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
				}
				return _message;
			}
		}
		CMRREFACCMessage _message;
	}
}
