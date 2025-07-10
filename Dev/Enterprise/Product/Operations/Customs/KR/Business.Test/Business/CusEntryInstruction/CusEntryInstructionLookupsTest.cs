using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class CusEntryInstructionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStatementNumber5WNList()
		{
			var declaration = Factory.New<JobDeclaration>();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			CreateRefundSessionalData("EM123457789KR");
			CreateRefundSessionalData("EM223457789KR");
			CreateRefundSessionalData("EM323457789KR");
			CreateRefundSessionalData("EM423457789KR");
			var amendmentSessionalDataWithoutRefundSessionalData = instruction.AmendmentSessionalDataCollection.AddNew();
			AssertEquals(null, amendmentSessionalDataWithoutRefundSessionalData.RefundSessionalData);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var statement1 = CreateStatementAndLine(StatementHeaderTypeList.Codes.CustomsDisbursementBill, new ZDateTime(2025, 01, 01), StatementHeaderPaymentStatusList.Codes.PYI, ZDateTime.Empty, rmNum: "1", statementNum: "EM123457789KR");
			var statement2 = CreateStatementAndLine(StatementHeaderTypeList.Codes.Invoice, new ZDateTime(2025, 01, 01), StatementHeaderPaymentStatusList.Codes.PYI, ZDateTime.Empty, rmNum: "1", statementNum: "EM223457789KR");
			var statement3 = CreateStatementAndLine(StatementHeaderTypeList.Codes.CustomsDisbursementBill, new ZDateTime(2025, 01, 02), StatementHeaderPaymentStatusList.Codes.PYC, new ZDateTime(2024, 12, 31), statementNum: "EM323457789KR");
			var statement4 = CreateStatementAndLine(StatementHeaderTypeList.Codes.Invoice, new ZDateTime(2025, 01, 03), StatementHeaderPaymentStatusList.Codes.PYC, new ZDateTime(2024, 12, 31), associatedEntry: "EM423457789KR");
			var statement5 = CreateStatementAndLine(StatementHeaderTypeList.Codes.Invoice, new ZDateTime(2025, 01, 03), StatementHeaderPaymentStatusList.Codes.PYC, new ZDateTime(2024, 12, 31), associatedEntry: "EM423457789KR");
			CreateStatementLine(statement4, "EM523457789KR");
			CreateStatementLine(statement4, "EM623457789KR");
			var statementNotIncluded = CreateStatementAndLine(StatementHeaderTypeList.Codes.Invoice, new ZDateTime(2025, 01, 02), StatementHeaderPaymentStatusList.Codes.PYC, new ZDateTime(2024, 12, 31), statementNum: "ZZ123457788KR");

			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_CEI_Instruction = instruction.PK;
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryNum = "1234520000045M";
			entryNum.CE_EntryType = KRJobMessageTypeList.Codes.Import;
			Factory.Save();

			AssertEquals(8, entry.StatementLines.Count());
			AssertEquals("EM523457789KR, EM623457789KR is not in the CSI_ReferenceNumber2", 4, instruction.Lookups.StatementNumber5WNList.Count);
			Assert(instruction.Lookups.StatementNumber5WNList.ContainsCode("EM123457789KR"));
			Assert(instruction.Lookups.StatementNumber5WNList.ContainsCode("EM223457789KR"));
			Assert(instruction.Lookups.StatementNumber5WNList.ContainsCode("EM323457789KR"));
			Assert(instruction.Lookups.StatementNumber5WNList.ContainsCode("EM423457789KR"));
			AssertEquals("Due Date: 2025-01-01, Unpaid", instruction.Lookups.StatementNumber5WNList.GetDescriptionFromCode("EM123457789KR"));
			AssertEquals("Due Date: 2025-01-01, Unpaid", instruction.Lookups.StatementNumber5WNList.GetDescriptionFromCode("EM223457789KR"));
			AssertEquals("Due Date: 2025-01-01, Paid on 2024-12-31", instruction.Lookups.StatementNumber5WNList.GetDescriptionFromCode("EM323457789KR"));
			AssertEquals("Due Date: 2025-01-01, Paid on 2024-12-31", instruction.Lookups.StatementNumber5WNList.GetDescriptionFromCode("EM423457789KR"));

			RefundSessionalData CreateRefundSessionalData(string referenceNumber)
			{
				var amendmentSessionalData = instruction.AmendmentSessionalDataCollection.AddNew();
				var refundSessionalData = new RefundSessionalDataCollection(instruction, amendmentSessionalData.PK).AddNew();
				refundSessionalData.CSI_ReferenceNumber2 = referenceNumber;
				Assert(!amendmentSessionalData.RefundSessionalData.IsNull);

				return refundSessionalData;
			}
			CusStatementHeader CreateStatementAndLine(string statementType, ZDateTime createTime, string paymentStatus, ZDateTime paidDate, string statementNum = "", string rmNum = "", string associatedEntry = "")
			{
				var statement = Factory.New<CusStatementHeader>();
				statement.B2_StatementType = statementType;
				statement.B2_StatementNumber = statementNum;
				statement.B2_RMNumber = rmNum;
				statement.B2_SystemCreateTimeUtc = createTime;
				statement.B2_DueDate = new ZDateTime(2025, 01, 01);
				statement.B2_PaymentAuthorizationDate = paidDate;
				statement.B2_PaymentStatus = paymentStatus;
				var statementLine = statement.StatementLines.AddNew();
				statementLine.B3_EntryNum = "1234520000045M";
				statementLine.B3_EntryType = KRJobMessageTypeList.Codes.Import;
				statementLine.B3_AssociatedEntry = associatedEntry;
				return statement;
			}
			void CreateStatementLine(CusStatementHeader header, string associatedEntry)
			{
				var statementLine = header.StatementLines.AddNew();
				statementLine.B3_EntryNum = "1234520000045M";
				statementLine.B3_EntryType = KRJobMessageTypeList.Codes.Import;
				statementLine.B3_AssociatedEntry = associatedEntry;
			}
		}
	}
}
