using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	public class CARMStatementOfAccountMessageProcessorTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessCARMStatementOfAccountMessage_Successed_PTMessage()
		{
			var newGroup1 = Factory.New<GlbGroup>();
			newGroup1.GG_Code = "NG1";
			var newStaff1 = newGroup1.Staff.AddNew();
			newStaff1.GS_Code = "NS1";
			newStaff1.GS_LoginName = "NS1";
			newStaff1.GS_EmailAddress = "ns1@cargowise.com";

			Factory.Save();

			using (CACustomsDataRegistry.Instance.SendK84ReportNotificationsToGroup.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newGroup1.PK.ToGuid()))
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var text = CARMStatementOfAccountMessageTestHelper.GetCARMStatementOfAccount_PTMessageText();
				var message = Factory.New<CARMStatementOfAccountMessage>();
				message.EM_MessageText = text;
				message.EM_MessageSubType = MessageTypeList.Codes.CARMStatementOfAccount;
				var cacMessageProcessor = new CACustomsMessageProcessor(logger);
				AssertNoExceptionThrown(() =>
				{
					cacMessageProcessor.ProcessMessage(message);
					Factory.Save();
				});

				var query = new ZQuery();
				query.AddToFilter(CusStatementHeaderSchema.B2_StatementNumber, "112358145-20231222114854");
				query.AddToFilter(CusStatementHeaderSchema.B2_StatementType, CARMStatementOfAccountStatementTypeList.ShortCodes.ProgramType);
				query.AddToFilter(CusStatementHeaderSchema.B2_IsMonthlyStatement, true);
				var statementHeader = Factory.LoadTop1<CusStatementHeader>(query);
				AssertNotNull(statementHeader);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessCARMStatementOfAccountMessage_Successed()
		{
			var newGroup1 = Factory.New<GlbGroup>();
			newGroup1.GG_Code = "NG1";
			var newStaff1 = newGroup1.Staff.AddNew();
			newStaff1.GS_Code = "NS1";
			newStaff1.GS_LoginName = "NS1";
			newStaff1.GS_EmailAddress = "ns1@cargowise.com";

			Factory.Save();

			using (CACustomsDataRegistry.Instance.SendK84ReportNotificationsToGroup.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newGroup1.PK.ToGuid()))
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var text = CARMStatementOfAccountMessageTestHelper.GetCARMStatementOfAccount_LEMessageText();
				var message = Factory.New<CARMStatementOfAccountMessage>();
				message.EM_MessageText = text;
				message.EM_MessageSubType = MessageTypeList.Codes.CARMStatementOfAccount;
				var cacMessageProcessor = new CACustomsMessageProcessor(logger);
				AssertNoExceptionThrown(() =>
				{
					cacMessageProcessor.ProcessMessage(message);
				});

				var query = new ZQuery();
				query.AddToFilter(CusStatementHeaderSchema.B2_StatementNumber, "112358145-20231222114905");
				query.AddToFilter(CusStatementHeaderSchema.B2_StatementType, CARMStatementOfAccountStatementTypeList.ShortCodes.LegalEntiry);
				query.AddToFilter(CusStatementHeaderSchema.B2_IsMonthlyStatement, true);
				var statementHeader = Factory.LoadTop1<CusStatementHeader>(query);
				AssertNotNull(statementHeader);

				AssertEquals(statementHeader, message.EM_LinkedObject);
				AssertEquals(MessageTypeList.Codes.CARMStatementOfAccount, message.EM_MessageSubType);
				AssertEquals(EDIMessageStatusList.Codes.Received, message.EM_Status);

				AssertEquals("file_Type", CARMStatementOfAccountStatementTypeList.ShortCodes.LegalEntiry, statementHeader.B2_StatementType);
				AssertEquals("file_name", "112358145-20231222114905", statementHeader.B2_StatementNumber);
				AssertEquals("HEADER.PER_START", new ZDateTime(2023, 09, 18), statementHeader.B2_PeriodStartDate);
				AssertEquals("HEADER.PER_END", new ZDateTime(2023, 10, 17), statementHeader.B2_PeriodEndDate);
				AssertEquals("HEADER.SOA_DATE", new ZDateTime(2023, 10, 25), statementHeader.B2_PrintDate);
				AssertEquals("HEADER.PAY_DUE", new ZDateTime(2023, 10, 31), statementHeader.B2_DueDate);
				AssertEquals("HEADER.GRAND_TOT", 27280.0m, statementHeader.B2_StatementAmount);
				AssertEquals("HEADER.PARTY.BN9", "112358145", statementHeader.B2_ImporterCustomsID);

				var noteEN = statementHeader.Notes.FindByDescription(StatementMessageProcessorHelper.EnglishMessageToRecipient).FirstOrDefault();
				AssertEquals("NOTES.Message", "SoA - The CARM Client Portal is now live. Check the CBSA Website for more information.", noteEN.ST_NoteDataAsText);
				var noteFR = statementHeader.Notes.FindByDescription(StatementMessageProcessorHelper.FrenchMessageToRecipient).FirstOrDefault();
				AssertEquals("NOTES.Message", "SoA - Le Portail client de la GCRA est maintenant disponible. Verifier le site Web de lASFC pour plus dinformation.", noteFR.ST_NoteDataAsText);

				AssertCusStatementLineGroupAndFinancialDetail(statementHeader, "112358145", CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.PreviousStatementBalance, 1000.1);
				AssertCusStatementLineGroupAndFinancialDetail(statementHeader, "112358145", CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.CorrectionsToPreviousStatementBalance, 2000.1m);
				AssertCusStatementLineGroupAndFinancialDetail(statementHeader, "112358145", CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.PaymentsReceivedAfterPreviousSoA, 3000.1m);
				AssertCusStatementLineGroupAndFinancialDetail(statementHeader, "112358145", CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.Disbursements, 4000.1m);
				AssertCusStatementLineGroupAndFinancialDetail(statementHeader, "112358145", CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.InterestAndPenaltiesSumTotal, 5000.1m);
				AssertCusStatementLineGroupAndFinancialDetail(statementHeader, "112358145", CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.CurrentPeriodCharges, 6000.1m);
				AssertCusStatementLineGroupAndFinancialDetail(statementHeader, "112358145", CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.CurrentStatementBalance, 21000.6m);

				AssertCusStatementLineGroupAndFinancialDetail(statementHeader, "112358145_DIST", CARMDailyNoticeChargeTypeList.Codes.Duties, 100.1m);
				AssertCusStatementLineGroupAndFinancialDetail(statementHeader, "112358145_DIST", CARMDailyNoticeChargeTypeList.Codes.ExciseTax, 200.1m);
				AssertCusStatementLineGroupAndFinancialDetail(statementHeader, "112358145_DIST", CARMDailyNoticeChargeTypeList.Codes.ExciseDuties, 300.1m);
				AssertCusStatementLineGroupAndFinancialDetail(statementHeader, "112358145_DIST", CARMDailyNoticeChargeTypeList.Codes.SIMA, 400.1m);
				AssertCusStatementLineGroupAndFinancialDetail(statementHeader, "112358145_DIST", CARMDailyNoticeChargeTypeList.Codes.GoodsAndServicesTax, 500.1m);
				AssertCusStatementLineGroupAndFinancialDetail(statementHeader, "112358145_DIST", CARMDailyNoticeChargeTypeList.Codes.HarmonizedSalesTax, 600.1m);
				AssertCusStatementLineGroupAndFinancialDetail(statementHeader, "112358145_DIST", CARMDailyNoticeChargeTypeList.Codes.ProvincialSalesTax, 700.1m);
				AssertCusStatementLineGroupAndFinancialDetail(statementHeader, "112358145_DIST", CARMDailyNoticeChargeTypeList.Codes.Interest, 800.1m);
				AssertCusStatementLineGroupAndFinancialDetail(statementHeader, "112358145_DIST", CARMDailyNoticeChargeTypeList.Codes.Penalties, 900.1m);
				AssertCusStatementLineGroupAndFinancialDetail(statementHeader, "112358145_DIST", CARMDailyNoticeChargeTypeList.Codes.Payments, 1000.1m);
				AssertCusStatementLineGroupAndFinancialDetail(statementHeader, "112358145_DIST", CARMDailyNoticeChargeTypeList.Codes.Totals, 5501.0m);

				AssertCusStatementLineAndChargeAmount(statementHeader, "112358145RM0001", new ZDate(2023, 08, 01), new ZDate(2023, 08, 01), 10.1m, 20.1m, 30.1m, 40.1m, 50.1m, 60.1m, 70.1m, 80.1m, 90.1m, 100.1m, 0m, 551m);
				AssertCusStatementLineAndChargeAmount(statementHeader, "112358145RM0001", new ZDate(2023, 08, 02), new ZDate(2023, 08, 02), 10.2m, 20.2m, 30.2m, 40.2m, 50.2m, 60.2m, 70.2m, 80.2m, 90.2m, 100.2m, 0m, 552m);

				AssertCusStatementLineAndChargeAmount(statementHeader, "112358145RM0002", new ZDate(2023, 08, 01), new ZDate(2023, 08, 01), 10.1m, 20.1m, 30.1m, 40.1m, 50.1m, 60.1m, 70.1m, 80.1m, 90.1m, 100.1m, 0m, 551m);
				AssertCusStatementLineAndChargeAmount(statementHeader, "112358145RM0002", new ZDate(2023, 08, 02), new ZDate(2023, 08, 02), 10.2m, 20.2m, 30.2m, 40.2m, 50.2m, 60.2m, 70.2m, 80.2m, 90.2m, 100.2m, 0m, 552m);

				var expectedHtml = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business.Test\MessageProcessors\ResponseMessageProcessors\CACustoms\TestFiles\ExpectedEmailHtmlForSOA.txt");
				AssertContains("CARM Statement Of Account</a> message has been received from the CBSA.<br />", message.EM_MessageInterpretation);

				var index = message.EM_MessageInterpretation.IndexOf("<table width=\"100%\">");
				if (index > 0)
				{
					AssertMultilineASCIIEquals(expectedHtml, message.EM_MessageInterpretation.Substring(index));
				}

				var email = Env.OutgoingMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "CARM Statement Of Account for 25-Oct-23");
				AssertNotNull(email);
				AssertEquals("One recipient", 1, email.CCRecipients.Count);
				AssertEquals("ns1@cargowise.com", email.CCRecipients[0].Email);

				index = email.Body.IndexOf("<table width=\"100%\">");
				if (index > 0)
				{
					var body = email.Body.Substring(index);
					var endIndex = body.IndexOf("      </td>\r\n    </tr>\r\n    <tr>\r\n      <td><img>");
					if (endIndex > 0)
					{
						AssertMultilineASCIIEquals(expectedHtml, body.Substring(0, endIndex));
					}
				}
			}
		}

		public void TestProcessCARMStatementOfAccountMessage_Failed()
		{
			var text = CARMStatementOfAccountMessageTestHelper.GetCARMStatementOfAccount_PAMessageText();
			text = text.Replace("SOA-112358145-20240711120000", "S-112358145-20240711120000");
			var message = Factory.New<CARMStatementOfAccountMessage>();
			message.EM_MessageText = text;
			message.EM_MessageSubType = MessageTypeList.Codes.CARMStatementOfAccount;
			var cacMessageProcessor = new CACustomsMessageProcessor(logger);
			AssertNoExceptionThrown(() =>
			{
				cacMessageProcessor.ProcessMessage(message);
			});
			AssertEquals(EDIMessageStatusList.Codes.Failed, message.EM_Status);
			AssertEquals(2, logger.Logs.Count());
			AssertContains("The file_name of message does not match the rule, it should start with 'SOA-BN9-'.", logger.Logs.ToArray()[0].Message);
			AssertContains("The file_name of message does not match the rule, it should start with 'SOA-BN9-'.", logger.Logs.ToArray()[1].Message);
			logger.ClearLogs();
		}

		public void TestEmailGroupAndMode()
		{
			var newGroupNN = Factory.New<GlbGroup>();
			newGroupNN.GG_Code = "NN1";
			Factory.Save();

			using (CACustomsDataRegistry.Instance.SendK84ReportNotificationsToGroup.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newGroupNN.PK.ToGuid()))
			{
				var cacMessageProcessor = new CARMStatementOfAccountMessageProcessorForTest(logger);
				AssertEquals(newGroupNN.PK, cacMessageProcessor.AcknowledgementEmailGroup_Exposed);
				AssertEquals(Core.Constants.EmailTo.NominatedGroup, cacMessageProcessor.AcknowledgementEmailMode_Exposed);
				AssertEquals(ZGuid.Empty, cacMessageProcessor.ImpedimentEmailGroup_Exposed);
				AssertEquals(ZString.Empty, cacMessageProcessor.ImpedimentEmailMode_Exposed);
				AssertEquals(ZGuid.Empty, cacMessageProcessor.ErrorEmailGroup_Exposed);
				AssertEquals(ZString.Empty, cacMessageProcessor.ErrorEmailMode_Exposed);
			}
		}

		void AssertCusStatementLineAndChargeAmount(CusStatementHeader statementHeader, ZString importerCustomsID, ZDate dueDate, ZDate scheduledProcessDate,
			ZDecimal duties, ZDecimal exciseTax, ZDecimal exciseDuties, ZDecimal sima, ZDecimal gst, ZDecimal hst, ZDecimal pst, ZDecimal interest,
			ZDecimal penalties, ZDecimal payments, ZDecimal others, ZDecimal total)
		{
			CombineAssertions(() =>
			{
				var statementLine = statementHeader.StatementLines.Cast<CusStatementLine>().First(x => x.B3_ImporterCustomsID == importerCustomsID && x.B3_DueDate == dueDate);
				AssertEquals("RELEASE_DATE", dueDate, statementLine.B3_DueDate);
				AssertEquals("ACCOUNTING_DATE", scheduledProcessDate, statementLine.B3_ScheduledProcessDate);
				AssertEquals("LINEITEM.DUTIES", duties, GetChargeAmount(statementLine, CARMDailyNoticeChargeTypeList.Codes.Duties));
				AssertEquals("LINEITEM.EXCISE", exciseTax, GetChargeAmount(statementLine, CARMDailyNoticeChargeTypeList.Codes.ExciseTax));
				AssertEquals("LINEITEM.EXCISEDUTIES", exciseDuties, GetChargeAmount(statementLine, CARMDailyNoticeChargeTypeList.Codes.ExciseDuties));
				AssertEquals("LINEITEM.SIMA", sima, GetChargeAmount(statementLine, CARMDailyNoticeChargeTypeList.Codes.SIMA));
				AssertEquals("LINEITEM.GST", gst, GetChargeAmount(statementLine, CARMDailyNoticeChargeTypeList.Codes.GoodsAndServicesTax));
				AssertEquals("LINEITEM.HST", hst, GetChargeAmount(statementLine, CARMDailyNoticeChargeTypeList.Codes.HarmonizedSalesTax));
				AssertEquals("LINEITEM.PST", pst, GetChargeAmount(statementLine, CARMDailyNoticeChargeTypeList.Codes.ProvincialSalesTax));
				AssertEquals("LINEITEM.INTEREST", interest, GetChargeAmount(statementLine, CARMDailyNoticeChargeTypeList.Codes.Interest));
				AssertEquals("LINEITEM.PENALTIES", penalties, GetChargeAmount(statementLine, CARMDailyNoticeChargeTypeList.Codes.Penalties));
				AssertEquals("LINEITEM.PAYMENTS", payments, GetChargeAmount(statementLine, CARMDailyNoticeChargeTypeList.Codes.Payments));
				AssertEquals("LINEITEM.OTHERS", others, GetChargeAmount(statementLine, CARMDailyNoticeChargeTypeList.Codes.Others));
				AssertEquals("LINEITEM.TOTALS", others, GetChargeAmount(statementLine, CARMDailyNoticeChargeTypeList.Codes.Totals));
			});
		}

		void AssertCusStatementLineGroupAndFinancialDetail(CusStatementHeader statementHeader, ZString importerCustomsID, ZString type, ZDecimal amount)
		{
			var statementLineGroup = statementHeader.LineGroupCollection.Cast<CusStatementLineGroup>().First(x => x.B10_ImporterCustomsID == importerCustomsID);
			var financialDetail = statementLineGroup.FinancialDetailCollection.Cast<CusStatementLineGroupFinancialDetail>().FirstOrDefault(x => x.B11_Type == type);

			AssertEquals(amount, financialDetail.B11_Amount);
		}

		ZDecimal GetChargeAmount(CusStatementLine line, ZString chargeType)
		{
			var result = ZDecimal.Zero;
			var charge = line.Charges.Cast<CusStatementLineCharge>().FirstOrDefault(x => x.B4_ChargeType == chargeType);
			if (charge != null)
			{
				result = charge.B4_ChargeAmount;
			}
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			logger = new LoggingInformation();
		}

		LoggingInformation logger;

		class CARMStatementOfAccountMessageProcessorForTest : CARMStatementOfAccountMessageProcessor
		{
			public CARMStatementOfAccountMessageProcessorForTest(LoggingInformation logger)
				: base(logger)
			{
			}

			public ZGuid AcknowledgementEmailGroup_Exposed => base.AcknowledgementEmailGroup;

			public ZString AcknowledgementEmailMode_Exposed => base.AcknowledgementEmailMode;

			public ZGuid ImpedimentEmailGroup_Exposed => base.ImpedimentEmailGroup;

			public ZString ImpedimentEmailMode_Exposed => base.ImpedimentEmailMode;

			public ZGuid ErrorEmailGroup_Exposed => base.ErrorEmailGroup;

			public ZString ErrorEmailMode_Exposed => base.ErrorEmailMode;
		}
	}
}
