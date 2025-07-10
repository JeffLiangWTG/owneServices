using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;
using TestDataSetUp = Enterprise.Customs.AU.Declaration.Business.TAndITransmitConditionChecker.TestDataSetUp;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class TAndIEndToEndTest : TestCaseWithFactory
	{
		public void TestTwoInvoicesHaveItsOwnTILV()
		{
			TestDataSetUp setter = new TestDataSetUp();

			setter.SetUpSingleEntryForTwoInvoices(Factory);

			setter.Declaration.TopGroupInvoice.Charges.AddNew("OFT", 400m, "AUD");

			setter.Invoice.AddInfo.ZA_TILV = "200AUD";
			setter.Invoice2.AddInfo.ZA_TILV = "300AUD";

			TAndITransmitConditionChecker checker = new TAndITransmitConditionChecker(setter.EntryHeader);

			AssertEquals("Line T&I should be sent", true, checker.ShouldTransmitTAndIForLine);
			AssertEquals("Header T&I should be sent", true, checker.ShouldTransmitTAndIForHeader);
			setter.Declaration.ResumeApportionment();
			IMDMessageBuilder builder = new IMDMessageBuilder(setter.EntryHeader, CMRMessageTypes.LodgeWithoutPay);
			EDIMessage message = builder.PopulateMessagesReturningResult();
			Assert("Header & Line T&I should be in the message text", DoesSegmentExistInTheMessage(message.EM_MessageText, "MOA+68:500.00:AUD", TAndILevel.Header));
			Assert("Header & Line T&I should be in the message text", DoesSegmentExistInTheMessage(message.EM_MessageText, "MOA+68:300.00:AUD", TAndILevel.Line));
		}

		public void TestOneInvoiceWithItsOwnTILV()
		{
			TestDataSetUp setter = new TestDataSetUp();

			setter.SetUpSingleEntryForSingleInvoice(Factory);

			setter.Declaration.TopGroupInvoice.Charges.AddNew("OFT", 400m, "AUD");

			setter.Invoice.AddInfo.ZA_TILV = "200AUD";

			TAndITransmitConditionChecker checker = new TAndITransmitConditionChecker(setter.EntryHeader);

			AssertEquals("Line T&I should be sent", false, checker.ShouldTransmitTAndIForLine);
			AssertEquals("Header T&I should be sent", true, checker.ShouldTransmitTAndIForHeader);
			setter.Declaration.ResumeApportionment();
			IMDMessageBuilder builder = new IMDMessageBuilder(setter.EntryHeader, CMRMessageTypes.LodgeWithoutPay);
			EDIMessage message = builder.PopulateMessagesReturningResult();
			Assert("Header & Line T&I should be in the message text", DoesSegmentExistInTheMessage(message.EM_MessageText, "MOA+68:200.00:AUD", TAndILevel.Header));
		}

		public void TestWhenInvoicesHaveItsOwnOFTOrONSorFIFT()
		{
			TestDataSetUp setter = new TestDataSetUp();

			setter.SetUpSingleEntryForTwoInvoices(Factory);
			setter.Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, "AUD");

			TAndITransmitConditionChecker checker = new TAndITransmitConditionChecker(setter.EntryHeader);
			AssertEquals("Line T&I should be sent", true, checker.ShouldTransmitTAndIForLine);
			setter.Declaration.ResumeApportionment();
			IMDMessageBuilder builder = new IMDMessageBuilder(setter.EntryHeader, CMRMessageTypes.LodgeWithoutPay);
			EDIMessage message = builder.PopulateMessagesReturningResult();
			Assert("Header & Line T&I should be in the message text", DoesSegmentExistInTheMessage(message.EM_MessageText, "MOA+68:100.00:AUD", TAndILevel.Both));
		}

		public void TestWhenNonDutiableFIFTExistsInGroup()
		{
			TestDataSetUp setter = new TestDataSetUp();

			setter.SetUpSingleEntryForTwoInvoices(Factory);
			BaseJobComInvHeaderCharge fIFT = setter.TopGroup.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 100m, "AUD");
			fIFT.J7_IsDutiable = false;
			setter.Declaration.ResumeApportionment();
			TAndITransmitConditionChecker checker = new TAndITransmitConditionChecker(setter.EntryHeader);
			AssertEquals("Header T&I should be sent", true, checker.ShouldTransmitTAndIForHeader);
			AssertEquals("Line T&I should be sent", false, checker.ShouldTransmitTAndIForLine);

			IMDMessageBuilder builder = new IMDMessageBuilder(setter.EntryHeader, CMRMessageTypes.LodgeWithoutPay);
			EDIMessage message = builder.PopulateMessagesReturningResult();
			Assert("Header T&I should be in the message text", DoesSegmentExistInTheMessage(message.EM_MessageText, "MOA+68:100.00:AUD", TAndILevel.Header));
		}

		public void TestWhenNonDutaibleFIFTExistsInInvoice()
		{
			TestDataSetUp setter = new TestDataSetUp();

			setter.SetUpSingleEntryForSingleInvoice(Factory);
			BaseJobComInvHeaderCharge fIFT = setter.TopGroup.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 100m, "AUD");
			fIFT.J7_IsDutiable = false;
			setter.Declaration.ResumeApportionment();
			TAndITransmitConditionChecker checker = new TAndITransmitConditionChecker(setter.EntryHeader);
			AssertEquals("Header T&I should be sent", true, checker.ShouldTransmitTAndIForHeader);
			AssertEquals("Line T&I should be sent", false, checker.ShouldTransmitTAndIForLine);

			IMDMessageBuilder builder = new IMDMessageBuilder(setter.EntryHeader, CMRMessageTypes.LodgeWithoutPay);
			EDIMessage message = builder.PopulateMessagesReturningResult();
			Assert("Header T&I should be in the message text", DoesSegmentExistInTheMessage(message.EM_MessageText, "MOA+68:100.00:AUD", TAndILevel.Header));
		}

		public void TestTILVInAddInfo()
		{
			TestDataSetUp setter = new TestDataSetUp();

			setter.SetUpSingleEntryForSingleInvoice(Factory);
			setter.InvoiceLine.AddInfo.ZA_TILV = "200AUD";
			setter.InvoiceLine.InvoiceHeader.AddInfo.ZA_TILV = "200AUD";
			TAndITransmitConditionChecker checker = new TAndITransmitConditionChecker(setter.EntryHeader);
			AssertEquals("Line T&I should be sent", true, checker.ShouldTransmitTAndIForLine);

			IMDMessageBuilder builder = new IMDMessageBuilder(setter.EntryHeader, CMRMessageTypes.LodgeWithoutPay);
			EDIMessage message = builder.PopulateMessagesReturningResult();
			Assert("Line T&I should be in the message text", DoesSegmentExistInTheMessage(message.EM_MessageText, "MOA+68:200.00:AUD", TAndILevel.Both));
		}

		public void TestLineLevelCharge()
		{
			TestDataSetUp setter = new TestDataSetUp();

			setter.SetUpSingleEntryForSingleInvoice(Factory);
			setter.InvoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 200m, "AUD");

			TAndITransmitConditionChecker checker = new TAndITransmitConditionChecker(setter.EntryHeader);
			AssertEquals("Line T&I should be sent", true, checker.ShouldTransmitTAndIForLine);

			IMDMessageBuilder builder = new IMDMessageBuilder(setter.EntryHeader, CMRMessageTypes.LodgeWithoutPay);
			EDIMessage message = builder.PopulateMessagesReturningResult();
			Assert("Line & Header T&I should be in the message text", DoesSegmentExistInTheMessage(message.EM_MessageText, "MOA+68:200.00:AUD", TAndILevel.Both));
		}

		public void TestWhenTopGroupHasOFT()
		{
			TestDataSetUp setter = new TestDataSetUp();
			setter.SetUpSingleEntryWithTwoInvoicesUnderTwoGroupInvoices(Factory);
			setter.TopGroup.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, "AUD");

			TAndITransmitConditionChecker checker = new TAndITransmitConditionChecker(setter.EntryHeader);
			AssertEquals("Header/Line T&I should not be sent", false, checker.ShouldTransmitTAndIForLine);
			AssertEquals("Header/Line T&I should not be sent", false, checker.ShouldTransmitTAndIForHeader);

			IMDMessageBuilder builder = new IMDMessageBuilder(setter.EntryHeader, CMRMessageTypes.LodgeWithoutPay);
			EDIMessage message = builder.PopulateMessagesReturningResult();
			Assert("Line & Header T&I should be in the message text", DoesSegmentExistInTheMessage(message.EM_MessageText, "MOA+68:100.00:AUD", TAndILevel.None));
		}

		public void TestWhenSubGroupHasOFT()
		{
			TestDataSetUp setter = new TestDataSetUp();
			setter.SetUpSingleEntryWithTwoInvoicesUnderTwoGroupInvoices(Factory);
			setter.SubGroup1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 200m, "AUD");

			TAndITransmitConditionChecker checker = new TAndITransmitConditionChecker(setter.EntryHeader);
			AssertEquals("Header/Line T&I should be sent", true, checker.ShouldTransmitTAndIForLine);
			setter.Declaration.ResumeApportionment();
			IMDMessageBuilder builder = new IMDMessageBuilder(setter.EntryHeader, CMRMessageTypes.LodgeWithoutPay);
			EDIMessage message = builder.PopulateMessagesReturningResult();
			Assert("Line & Header T&I should be in the message text", DoesSegmentExistInTheMessage(message.EM_MessageText, "MOA+68:200.00:AUD", TAndILevel.Both));
		}

		public void TestWhenThereIsNonDutiableOTH()
		{
			TestDataSetUp setter = new TestDataSetUp();

			setter.SetUpSingleEntryForSingleInvoice(Factory);
			BaseJobComInvHeaderCharge oTH = setter.Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, "USD");
			oTH.J7_IsDutiable = false;

			TAndITransmitConditionChecker checker = new TAndITransmitConditionChecker(setter.EntryHeader);
			AssertEquals("Header/Line T&I should be sent", true, checker.ShouldTransmitTAndIForHeader);
			setter.Declaration.ResumeApportionment();
			IMDMessageBuilder builder = new IMDMessageBuilder(setter.EntryHeader, CMRMessageTypes.LodgeWithoutPay);
			EDIMessage message = builder.PopulateMessagesReturningResult();
			Assert("Header T&I should be in the message text", DoesSegmentExistInTheMessage(message.EM_MessageText, "MOA+68:100.00:USD", TAndILevel.Header));
		}

		public void TestWhenThereIsAdjustment()
		{
			TestDataSetUp setter = new TestDataSetUp();

			setter.SetUpSingleEntryForSingleInvoice(Factory);
			setter.Invoice.Charges.AddNew("OFT", 50m, "AUD");
			setter.InvoiceLine.AddInfo.ZA_ADJ = "10AUD";

			TAndITransmitConditionChecker checker = new TAndITransmitConditionChecker(setter.EntryHeader);
			AssertEquals("Header/Line T&I should be sent", false, checker.ShouldTransmitTAndIForHeader);
			AssertEquals("Header/Line T&I should be sent", false, checker.ShouldTransmitTAndIForLine);
		}

		enum TAndILevel { Header, Line, Both, None }
		bool DoesSegmentExistInTheMessage(ZString messageText, ZString segment, TAndILevel level)
		{
			int lineStartIndex = messageText.IndexOf("CST+1+I");
			bool lineExist = messageText.SubstringSafe(lineStartIndex).Contains(segment);
			bool headerExist = messageText.SubstringSafe(0, lineStartIndex).Contains(segment);

			if (level == TAndILevel.Line)
			{
				return lineExist && !headerExist;
			}
			else if (level == TAndILevel.Header)
			{
				return headerExist && !lineExist;
			}
			else if (level == TAndILevel.Both)
			{
				return lineExist && headerExist;
			}
			else if (level == TAndILevel.None)
			{
				return !lineExist && !headerExist;
			}
			return false;
		}
	}
}
