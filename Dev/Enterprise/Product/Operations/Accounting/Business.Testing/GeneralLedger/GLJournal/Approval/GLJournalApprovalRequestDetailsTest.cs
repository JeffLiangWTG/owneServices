using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.Business.TransactionApproval.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals.Testing
{
	[TestedType(typeof(GLJournalApprovalRequestDetails))]
	public class GLJournalApprovalRequestDetailsTest : ApprovalRequestDetailsTest<GLJournalApprovalRequestDetails>
	{
		public void TestJournal()
		{
			var request = (GLJournalApprovalRequestDetails)GetNewBusinessObject();
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLReversingJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));

			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);
			Assert("Precondition: request.HasChange", !request.HasChanges);
			Assert("Precondition: journal.HasChange", journal.HasChanges);
			request.SetJournal_ForTestOnly(journal);
			Assert("Journal is not registered as editable child as its HasChanges are not counted in requested HasChanges", !request.HasChanges);

			request.SetJournal_ForTestOnly(journal);
			AssertEquals("LastKeyReported", "Accounting.GLJournalApprovalRequestDetails.Journal", ErrorReporter.LastKeyReported);
			AssertEquals("LastMessageReported", "A try to override live Journal on GLJournalApprovalRequestDetails. This is not allowed as Journal can be bound to a GUI control.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public override void TestOpertorEqual()
		{
			GLJournalApprovalRequestDetails a = null;
			GLJournalApprovalRequestDetails b = null;
			Assert(a == b);
			Assert(b == a);

			a = (GLJournalApprovalRequestDetails)GetNewBusinessObject();
			b = null;
			Assert(a != b);
			Assert(b != a);

			a = null;
			b = (GLJournalApprovalRequestDetails)GetNewBusinessObject();
			Assert(a != b);
			Assert(b != a);

			a = (GLJournalApprovalRequestDetails)GetNewBusinessObject();
			b = (GLJournalApprovalRequestDetails)GetNewBusinessObject();
			Assert(a == b);
			Assert(b == a);
		}

		public void TestOpertorEqualCore()
		{
			var a = (GLJournalApprovalRequestDetails)GetNewBusinessObject();
			var b = (GLJournalApprovalRequestDetails)GetNewBusinessObject();

			Assert(a == b);
			Assert(b == a);

			a.SetJournal_ForTestOnly(Factory.New<GLJournal>());
			Assert(a != b);
			Assert(b != a);

			b.SetJournal_ForTestOnly(Factory.New<GLJournal>());
			Assert(a == b);
			Assert(b == a);

			a.Journal.AH_Desc = "1";
			Assert(a != b);
			Assert(b != a);

			b.Journal.AH_Desc = "1";
			Assert(a == b);
			Assert(b == a);

			var periodManager = new PeriodManager(Factory);
			periodManager.CreateOnePeriod(202005, new ZDateTime(2020, 5, 1), new ZDateTime(2020, 5, 31), Factory);
			periodManager.CreateOnePeriod(202006, new ZDateTime(2020, 6, 1), new ZDateTime(2020, 6, 30), Factory);

			a.Journal.PostPeriod = 202005;
			Assert(a != b);
			Assert(b != a);
			b.Journal.PostPeriod = 202005;
			Assert(a == b);
			Assert(b == a);

			a.Journal.AgePeriod = 202006;
			Assert(a != b);
			Assert(b != a);
			b.Journal.AgePeriod = 202006;
			Assert(a == b);
			Assert(b == a);

			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var newPostDate = new ZDateTime(2020, 5, 15);
				a.Journal.AH_PostDate = newPostDate;
				Assert(a != b);
				Assert(b != a);
				b.Journal.AH_PostDate = newPostDate;
				Assert(a == b);
				Assert(b == a);

				var newDueDate = new ZDateTime(2020, 6, 15);
				a.Journal.AH_DueDate = newDueDate;
				Assert(a != b);
				Assert(b != a);
				b.Journal.AH_DueDate = newDueDate;
				Assert(a == b);
				Assert(b == a);
			}

			var lineA1 = (GLJournalLine)a.Journal.Lines.AddNew();
			var lineA2 = (GLJournalLine)a.Journal.Lines.AddNew();
			var lineB1 = (GLJournalLine)b.Journal.Lines.AddNew();
			var lineB2 = (GLJournalLine)b.Journal.Lines.AddNew();
			Assert(a == b);
			Assert(b == a);

			lineA1.AL_RX_NKTransactionCurrency = "1";
			Assert(a != b);
			Assert(b != a);

			lineB1.AL_RX_NKTransactionCurrency = "1";
			Assert(a == b);
			Assert(b == a);

			lineA2.AL_RX_NKTransactionCurrency = "1";
			Assert(a != b);
			Assert(b != a);

			lineB2.AL_RX_NKTransactionCurrency = "1";
			Assert(a == b);
			Assert(b == a);

			var subAccountAS1 = lineA1.SubAccounts.AddNew();
			var subAccountAS2 = lineA1.SubAccounts.AddNew();
			var subAccountAS3 = lineA2.SubAccounts.AddNew();
			var subAccountAS4 = lineA2.SubAccounts.AddNew();

			var subAccountBS1 = lineB1.SubAccounts.AddNew();
			var subAccountBS2 = lineB1.SubAccounts.AddNew();
			var subAccountBS3 = lineB2.SubAccounts.AddNew();
			var subAccountBS4 = lineB2.SubAccounts.AddNew();

			Assert(a == b);
			Assert(b == a);

			subAccountAS1.AL1_SubClassParentTableCode = "OH";
			Assert(a != b);
			Assert(b != a);

			subAccountBS1.AL1_SubClassParentTableCode = "OH";
			Assert(a == b);
			Assert(b == a);

			subAccountAS2.AL1_SubClassParentId = TestObjectCreator.Creditor1.PK;
			Assert(a != b);
			Assert(b != a);

			subAccountBS2.AL1_SubClassParentId = TestObjectCreator.Creditor1.PK;
			Assert(a == b);
			Assert(b == a);

			subAccountAS3.AL1_SubClassParentTableCode = "GS";
			Assert(a != b);
			Assert(b != a);

			subAccountBS3.AL1_SubClassParentTableCode = "GS";
			Assert(a == b);
			Assert(b == a);

			subAccountAS4.AL1_SubClassParentId = TestObjectCreator.Staff.PK;
			Assert(a != b);
			Assert(b != a);

			subAccountBS4.AL1_SubClassParentId = TestObjectCreator.Staff.PK;
			Assert(a == b);
			Assert(b == a);

			AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var lFOA1 = lineA1.AccTransactionLineDissectionAttributes.AddNew();
			var oRGA1 = lineA1.AccTransactionLineDissectionAttributes.AddNew();
			var lFOA2 = lineA2.AccTransactionLineDissectionAttributes.AddNew();

			var lFOB1 = lineB1.AccTransactionLineDissectionAttributes.AddNew();
			var oRGB1 = lineB1.AccTransactionLineDissectionAttributes.AddNew();
			var lFOB2 = lineB2.AccTransactionLineDissectionAttributes.AddNew();

			AssertAttributeEquals(true);
			AssertAttributeEquals(false);

			void AssertAttributeEquals(bool enableReportingBooksFeature)
			{
				AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enableReportingBooksFeature);

				Assert(a == b);
				Assert(b == a);

				lFOA1.ALD_AttributeValue = AccountingMasterFilesConstants.LFOCodes.LOC;
				AssertEquals(enableReportingBooksFeature, a != b);
				AssertEquals(enableReportingBooksFeature, b != a);

				lFOB1.ALD_AttributeValue = AccountingMasterFilesConstants.LFOCodes.LOC;
				Assert(a == b);
				Assert(b == a);

				var oRGValudID = ZGuid.NewZGuid();
				oRGA1.ALD_AttributeValueID = oRGValudID;
				AssertEquals(enableReportingBooksFeature, a != b);
				AssertEquals(enableReportingBooksFeature, b != a);

				oRGB1.ALD_AttributeValueID = oRGValudID;
				Assert(a == b);
				Assert(b == a);

				lFOA2.ALD_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO;
				AssertEquals(enableReportingBooksFeature, a != b);
				AssertEquals(enableReportingBooksFeature, b != a);

				lFOB2.ALD_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO;
				Assert(a == b);
				Assert(b == a);
			}
		}

		public void TestOperationEqualWhenChargesInDifferentOrder()
		{
			var a = (GLJournalApprovalRequestDetails)GetNewBusinessObject();
			a.SetJournal_ForTestOnly(Factory.New<GLJournal>());
			var b = (GLJournalApprovalRequestDetails)GetNewBusinessObject();
			b.SetJournal_ForTestOnly(Factory.New<GLJournal>());
			var c = (GLJournalApprovalRequestDetails)GetNewBusinessObject();
			c.SetJournal_ForTestOnly(Factory.New<GLJournal>());
			var d = (GLJournalApprovalRequestDetails)GetNewBusinessObject();
			d.SetJournal_ForTestOnly(Factory.New<GLJournal>());
			var e = (GLJournalApprovalRequestDetails)GetNewBusinessObject();
			e.SetJournal_ForTestOnly(Factory.New<GLJournal>());
			var f = (GLJournalApprovalRequestDetails)GetNewBusinessObject();
			f.SetJournal_ForTestOnly(Factory.New<GLJournal>());

			var chargeA1_a = (GLJournalLine)a.Journal.Lines.AddNew();
			var chargeA2_a = (GLJournalLine)a.Journal.Lines.AddNew();
			var chargeA3_b = (GLJournalLine)a.Journal.Lines.AddNew();

			var chargeB1_a = (GLJournalLine)b.Journal.Lines.AddNew();
			var chargeB2_b = (GLJournalLine)b.Journal.Lines.AddNew();
			var chargeB3_a = (GLJournalLine)b.Journal.Lines.AddNew();

			var chargeC1_b = (GLJournalLine)c.Journal.Lines.AddNew();
			var chargeC2_a = (GLJournalLine)c.Journal.Lines.AddNew();
			var chargeC3_a = (GLJournalLine)c.Journal.Lines.AddNew();

			var chargeD1_b = (GLJournalLine)d.Journal.Lines.AddNew();
			var chargeD2_b = (GLJournalLine)d.Journal.Lines.AddNew();
			var chargeD3_a = (GLJournalLine)d.Journal.Lines.AddNew();

			var chargeE1_b = (GLJournalLine)e.Journal.Lines.AddNew();
			var chargeE2_a = (GLJournalLine)e.Journal.Lines.AddNew();
			var chargeE3_b = (GLJournalLine)e.Journal.Lines.AddNew();

			var chargeF1_a = (GLJournalLine)f.Journal.Lines.AddNew();
			var chargeF2_b = (GLJournalLine)f.Journal.Lines.AddNew();
			var chargeF3_b = (GLJournalLine)f.Journal.Lines.AddNew();

			chargeA1_a.AL_Desc = "A";
			chargeA2_a.AL_Desc = chargeA1_a.AL_Desc;
			chargeB1_a.AL_Desc = chargeA1_a.AL_Desc;
			chargeB3_a.AL_Desc = chargeA1_a.AL_Desc;
			chargeC2_a.AL_Desc = chargeA1_a.AL_Desc;
			chargeC3_a.AL_Desc = chargeA1_a.AL_Desc;
			chargeD3_a.AL_Desc = chargeA1_a.AL_Desc;
			chargeE2_a.AL_Desc = chargeA1_a.AL_Desc;
			chargeF1_a.AL_Desc = chargeA1_a.AL_Desc;

			chargeA3_b.AL_Desc = "B";
			chargeB2_b.AL_Desc = chargeA3_b.AL_Desc;
			chargeC1_b.AL_Desc = chargeA3_b.AL_Desc;
			chargeD1_b.AL_Desc = chargeA3_b.AL_Desc;
			chargeD2_b.AL_Desc = chargeA3_b.AL_Desc;
			chargeE1_b.AL_Desc = chargeA3_b.AL_Desc;
			chargeE3_b.AL_Desc = chargeA3_b.AL_Desc;
			chargeF2_b.AL_Desc = chargeA3_b.AL_Desc;
			chargeF3_b.AL_Desc = chargeA3_b.AL_Desc;

#pragma warning disable 1718
			Assert("a == a", a == a);
			Assert("a == b", a == b);
			Assert("a == c", a == c);
			Assert("a != d", a != d);
			Assert("a != e", a != e);
			Assert("a != f", a != f);

			Assert("b == a", b == a);
			Assert("b == b", b == b);
			Assert("b == c", b == c);
			Assert("b != d", b != d);
			Assert("b != e", b != e);
			Assert("b != f", b != f);

			Assert("c == a", c == a);
			Assert("c == b", c == b);
			Assert("c == c", c == c);
			Assert("c != d", c != d);
			Assert("c != e", c != e);
			Assert("c != f", c != f);

			Assert("d != a", d != a);
			Assert("d != b", d != b);
			Assert("d != c", d != c);
			Assert("d == d", d == d);
			Assert("d == e", d == e);
			Assert("d == f", d == f);

			Assert("e != a", e != a);
			Assert("e != b", e != b);
			Assert("e != c", e != c);
			Assert("e == d", e == d);
			Assert("e == e", e == e);
			Assert("e == f", e == f);

			Assert("f != a", f != a);
			Assert("f != b", f != b);
			Assert("f != c", f != c);
			Assert("f == d", f == d);
			Assert("f == e", f == e);
			Assert("f == f", f == f);
#pragma warning restore 1718

			#region Sub Accounts

			a.Journal.Lines.RemoveAndDeleteAll();
			b.Journal.Lines.RemoveAndDeleteAll();
			c.Journal.Lines.RemoveAndDeleteAll();

			var lineA1 = (GLJournalLine)a.Journal.Lines.AddNew();
			var lineA2 = (GLJournalLine)a.Journal.Lines.AddNew();

			var lineB1 = (GLJournalLine)b.Journal.Lines.AddNew();
			var lineB2 = (GLJournalLine)b.Journal.Lines.AddNew();

			c.Journal.Lines.AddNew();
			c.Journal.Lines.AddNew();

			var subAccountA1_OH = lineA1.SubAccounts.AddNew();
			lineA1.SubAccounts.AddNew();

			lineA2.SubAccounts.AddNew();
			lineA2.SubAccounts.AddNew();

			lineB1.SubAccounts.AddNew();
			lineB1.SubAccounts.AddNew();

			lineB2.SubAccounts.AddNew();
			var subAccountB2_OH = lineB2.SubAccounts.AddNew();

			subAccountA1_OH.AL1_SubClassParentTableCode = "OH";
			subAccountB2_OH.AL1_SubClassParentTableCode = "OH";

			Assert("a == b", a == b);
			Assert("a != c", a != c);
			Assert("b == a", b == a);
			Assert("b != c", b != c);
			Assert("c != a", c != a);
			Assert("c != b", c != b);

			#endregion
		}

		public void TestCopyFromCore()
		{
			var request = (GLJournalApprovalRequestDetails)GetNewBusinessObject();
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLReversingJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));

			request.SetJournal_ForTestOnly(journal);

			var newPostingRequest = (GLJournalApprovalRequestDetails)GetNewBusinessObject();

			newPostingRequest.CopyFrom(request);

			AssertEquals("Journal", journal, newPostingRequest.Journal);
		}

		public override void TestIsPostingActionTheSame()
		{
			var a = (GLJournalApprovalRequestDetails)GetNewBusinessObject();
			var b = (GLJournalApprovalRequestDetails)GetNewBusinessObject();

			a.MaxAmountToApprove = 1;
			b.MaxAmountToApprove = 0;
			Assert("Posting action is the same for any details", a.IsPostingActionTheSame(b));
			Assert("Posting action is the same for any details", b.IsPostingActionTheSame(a));
		}

		protected override GLJournalApprovalRequestDetails GetNewFullyPopulatedBusinessObjectForXMLTest()
		{
			var details = base.GetNewFullyPopulatedBusinessObjectForXMLTest();

			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLReversingJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			journal.AH_TransactionNum = "SOME NUMBER";
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);

			details.SetJournal_ForTestOnly(journal);

			return details;
		}

		protected override string GetExpectedXML(bool isReadTest)
		{
			return "\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><GLJournalApprovalRequestDetails><Journal><GLJournal xmlns=\"http://www.edi.com.au/EnterpriseService/\"><GLDetail><JournalType>RJL</JournalType><JournalNumber>SOME NUMBER</JournalNumber><Description>GL REVERSING JOURNAL</Description><InPeriod>201503</InPeriod><OutPeriod>201507</OutPeriod><Branch>BNE</Branch><Department>BRN</Department></GLDetail><JournalLines><JournalLine><Account>2020.10.00</Account><Branch>BNE</Branch><Department>BRN</Department><Description>GL REVERSING JOURNAL</Description><LocalAmount>10</LocalAmount><DRCR>CR</DRCR><PK>Line1PK</PK></JournalLine><JournalLine><Account>2020.20.00</Account><Branch>BNE</Branch><Department>BRN</Department><Description>GL REVERSING JOURNAL</Description><LocalAmount>10</LocalAmount><DRCR>DR</DRCR><PK>Line2PK</PK></JournalLine></JournalLines></GLJournal></Journal></GLJournalApprovalRequestDetails>";
		}

		protected override string GetExpectedEmptyXML()
		{
			return "\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><GLJournalApprovalRequestDetails><Journal /></GLJournalApprovalRequestDetails>";
		}

		protected override string GetLegacyXML()
		{
			return GetExpectedXML(true);
		}

		protected override string PrepareRealXMLForComparison(string xml, GLJournalApprovalRequestDetails details)
		{
			return xml.Replace(details.Journal.Lines[0].PK.ToString(), "Line1PK").Replace(details.Journal.Lines[1].PK.ToString(), "Line2PK");
		}

		protected override void AssertFullyPopulatedBusinessObjectForXMLTest(GLJournalApprovalRequestDetails details, bool withNewFields = true)
		{
			AssertEquals("MaxAmountToApprove is not used.", 0m, details.MaxAmountToApprove);

			var journal = details.Journal;
			AssertNotNull("Journal", journal);
			AssertEquals("TransactionType", TransactionTypes.GLReversingJournal, journal.AH_TransactionType);
			AssertEquals("TransactionNum", "SOME NUMBER", journal.AH_TransactionNum);
			AssertEquals("PostPeriod", 201503, journal.PostPeriod);
			AssertEquals("AgePeriod", 201507, journal.AgePeriod);
			AssertEquals("Line count", 2, journal.Lines.Count);

			var line = (GLJournalLine)journal.Lines[0];
			AssertEquals("UnsignedLocalLineAmount", 10m, line.UnsignedLocalLineAmount);
			AssertEquals("DebitCreditSign", DebitCreditDataEntry.CR, line.DebitCreditSign);
			AssertEquals("GLHeader", TestObjectCreator.ExchangeGainLossControlAccount.PK, line.GLHeader.PK);

			line = (GLJournalLine)journal.Lines[1];
			AssertEquals("UnsignedLocalLineAmount", 10m, line.UnsignedLocalLineAmount);
			AssertEquals("DebitCreditSign", DebitCreditDataEntry.DR, line.DebitCreditSign);
			AssertEquals("GLHeader", TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK, line.GLHeader.PK);
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3));
		}
	}
}
