using System;
using System.Data;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocAccComplianceReportLine))]
	sealed class DocAccComplianceReportLineTest : DocumentWrapperTestCase
	{
		[TestDate(2015, 12, 09)]
		public void TestProperties()
		{
			SetComplianceReportLinesWithValidTestData();
			CombineAssertions(() =>
			{
				AssertProperties(WIPSequence, AccountingConstants.DefaultDayBookLineDescriptions.WIP, null, ZDateTime.Empty);
				AssertProperties(AccrualSequence, AccountingConstants.DefaultDayBookLineDescriptions.Accrual, null, ZDateTime.Empty);
				AssertProperties(InvoiceSequence, InvoiceDescription, InvoiceCreateUser, InvoiceCreateTime);
				AssertProperties(LineSequence, InvoiceDescription, InvoiceCreateUser, InvoiceCreateTime);
			});
		}

		public void AssertProperties(ZInt reportLineSequence, ZString description, GlbStaff createUser, ZDateTime createTime)
		{
			var reportLine = Report.ReportLines.Cast<AccComplianceReportLine>().First(x => x.ACL_ReportSequence == reportLineSequence);
			var wrapper = ReportWrapper.ReportLines.Cast<DocAccComplianceReportLine>().First(x => x.ACL_ReportSequence == reportLineSequence);

			AssertEquals("ACL_ReportSequence", reportLine.ACL_ReportSequence, wrapper.ACL_ReportSequence);
			AssertEquals("OH_Code", reportLine.OH_Code, wrapper.OH_Code);
			AssertEquals("OH_FullName", reportLine.OH_FullName, wrapper.OH_FullName);
			AssertEquals("OrgCountryCode", reportLine.OrgCountryCode, wrapper.OrgCountryCode);
			AssertEquals("GC_RN_NKCountryCode", reportLine.GC_RN_NKCountryCode, wrapper.GC_RN_NKCountryCode);
			AssertEquals("OK_CustomsRegNo", reportLine.OK_CustomsRegNo, wrapper.OK_CustomsRegNo);
			AssertEquals("TaxRegistrationNumber", reportLine.OK_CustomsRegNo.IsEmpty ? reportLine.RepCountryRegNo : reportLine.OK_CustomsRegNo, wrapper.TaxRegistrationNumber);
			AssertEquals("GC_RX_NKLocalCurrency", reportLine.GC_RX_NKLocalCurrency, wrapper.GC_RX_NKLocalCurrency);
			AssertEquals("AH_Ledger", reportLine.AH_Ledger, wrapper.AH_Ledger);
			AssertEquals("AH_TransactionType", reportLine.AH_TransactionType, wrapper.AH_TransactionType);
			AssertEquals("PostDate", reportLine.PostDate, wrapper.PostDate);
			AssertEquals("InvoiceDate", reportLine.InvoiceDate, wrapper.InvoiceDate);
			AssertEquals("AH_TransactionNum", reportLine.AH_TransactionNum, wrapper.AH_TransactionNum);
			AssertEquals("AH_TransactionReference", reportLine.AH_TransactionReference, wrapper.AH_TransactionReference);
			AssertEquals("AH_ComplianceSubType", reportLine.AH_ComplianceSubType, wrapper.AH_ComplianceSubType);
			AssertEquals("ReportSubCode", reportLine.ReportSubCode, wrapper.ReportSubCode);
			AssertEquals("AT_Type", reportLine.AT_Type, wrapper.AT_Type);
			AssertEquals("AT_Code", reportLine.AT_Code, wrapper.AT_Code);
			AssertEquals("AT_ExtraTaxRateType", reportLine.AT_ExtraTaxRateType, wrapper.AT_ExtraTaxRateType);
			AssertEquals("TaxMessage", reportLine.TaxMessage, wrapper.TaxMessage);
			AssertEquals("GoodsExTaxAmount", reportLine.GoodsExTaxAmount, wrapper.GoodsExTaxAmount);
			AssertEquals("GoodsTaxAmount", reportLine.GoodsTaxAmount, wrapper.GoodsTaxAmount);
			AssertEquals("ServiceExTaxAmount", reportLine.ServiceExTaxAmount, wrapper.ServiceExTaxAmount);
			AssertEquals("ServiceTaxAmount", reportLine.ServiceTaxAmount, wrapper.ServiceTaxAmount);
			AssertEquals("TotalExTaxAmount", reportLine.TotalExTaxAmount, wrapper.TotalExTaxAmount);
			AssertEquals("TotalTaxAmount", reportLine.TotalTaxAmount, wrapper.TotalTaxAmount);
			AssertEquals("TotalTaxAmountForDisplay", reportLine.SPVTaxAmount, wrapper.TotalTaxAmountForDisplay);
			AssertEquals("SPVTaxAmountForDisplay", -reportLine.SPVTaxAmount, wrapper.SPVTaxAmountForDisplay);
			AssertEquals("TotalInvoiceAmount", reportLine.AH_InvoiceAmount + reportLine.AH_GSTAmount, wrapper.TotalInvoiceAmount);
			AssertEquals("ComplianceSequence", reportLine.ComplianceSequence, wrapper.ComplianceSequence);
			if (reportLine.AH_PK != ZGuid.Empty)
			{
				AssertEquals("IsLastLineOfInvoice", wrapper.ACL_ReportSequence == LineSequence, wrapper.IsLastLineOfInvoice);
			}

			AssertEquals("IsGoods", reportLine.IsGoods, wrapper.IsGoods);
			AssertEquals("IsService", reportLine.IsService, wrapper.IsService);
			AssertEquals("AG_AccountNum", reportLine.AG_AccountNum, wrapper.AG_AccountNum);
			AssertEquals("AG_Description", reportLine.AG_Description, wrapper.AG_Description);
			AssertEquals("GeneralLedgerAmountDR", reportLine.GeneralLedgerAmountDR, wrapper.GeneralLedgerAmountDR);
			AssertEquals("GeneralLedgerAmountCR", reportLine.GeneralLedgerAmountCR, wrapper.GeneralLedgerAmountCR);
			AssertEquals("TaxRecoverableAmount", reportLine.TaxRecoverableAmount, wrapper.TaxRecoverableAmount);
			AssertEquals("TaxNotRecoverableAmount", reportLine.TaxNotRecoverableAmount, wrapper.TaxNotRecoverableAmount);
			AssertEquals("Comment", reportLine.Comment, wrapper.Comment);
			AssertEquals("AccountingPeriod", Period.AM_Period, wrapper.AccountingPeriod);

			AssertEquals("Description", description, wrapper.Description);
			AssertEquals("CreateUserCode", createUser?.GS_Code ?? ZString.Empty, wrapper.CreateUserCode);
			AssertEquals("CreateUserName", createUser?.GS_FullName ?? ZString.Empty, wrapper.CreateUserName);
			AssertEquals("CreateTime", createTime, wrapper.CreateTime);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { CreateDocumentWrapperFromStaticNewMethod() };
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			var docLine = DocAccComplianceReportLine.New(ReportLine, Factory);
			docLine.ReportUniqueID = ReportWrapper.UniqueReportID;
			return docLine;
		}

		AccComplianceReport Report;
		AccComplianceReportLine ReportLine;
		AccPeriodManagement Period;
		TestObjectCreator Creator;
		ZString InvoiceDescription;
		GlbStaff InvoiceCreateUser;
		ZDateTime InvoiceCreateTime;
		readonly ZInt WIPSequence = 1;
		readonly ZInt AccrualSequence = 2;
		readonly ZInt InvoiceSequence = 3;
		readonly ZInt LineSequence = 4;

		DocAccComplianceReport ReportWrapper
		{
			get { return reportWrapper ?? (reportWrapper = DocAccComplianceReport.New(Report, Factory)); }
		}
		DocAccComplianceReport reportWrapper;

		void SetComplianceReportLinesWithValidTestData(ZGuid? lineGlAccountPK = null, ZGuid? accrualGlAccountPK = null, ZGuid? wipGlAccountPK = null)
		{
			var invoice = Creator.CreateAPInvoice<APInvoice>("I0002", Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, Creator.ABIGAS);

			invoice.AH_Desc = InvoiceDescription;
			invoice.AH_SystemCreateUser = InvoiceCreateUser.GS_Code;
			invoice.AH_SystemCreateTimeUtc = InvoiceCreateTime;
			Assert("Has Lines", invoice.Lines.Count > 0);

			var lineCreateUser = Creator.CreateStaff("TU2");
			lineCreateUser.GS_FullName = "Line User";
			var line = invoice.Lines[0];
			line.AL_AG = lineGlAccountPK ?? Creator.GLHeader1.PK;
			line.AL_AT = Creator.VATSPV.PK;

			var wip = Creator.CreateWIP();
			wip.AL_AG = wipGlAccountPK ?? Creator.GLHeader2.PK;
			var accrual = Creator.CreateAccrual();
			accrual.AL_AG = accrualGlAccountPK ?? Creator.CashOnHandAccount.PK;

			foreach (var txnLine in new AccTransactionLines[] { line, wip, accrual })
			{
				txnLine.AL_Desc = InvoiceDescription + "plus difference for line";
				txnLine.AL_SystemCreateUser = lineCreateUser.GS_Code;
				txnLine.AL_SystemCreateTimeUtc = InvoiceCreateTime.AddDays(1);
			}
			Factory.Save();

			Creator.CreateComplianceReportTransactionPivot(Report, wip, WIPSequence, "*JC*WIP*");
			Creator.CreateComplianceReportTransactionPivot(Report, accrual, AccrualSequence, "*JC*ACR*");
			Creator.CreateComplianceReportTransactionPivot(Report, invoice, InvoiceSequence);
			Creator.CreateComplianceReportTransactionPivot(Report, line, LineSequence);
			Creator.CreateConfigurationForComplianceReport(Report, "**", reportLineGrouping: "DAB");

			Assert("Report has ReportLines", Report.ReportLines.Count == 4);
		}

		internal static DataRow GetDataRow(AccComplianceReport report)
		{
			var table = AccComplianceReportLine.GetDataTable(report);
			var result = table.NewRow();

			result[AccComplianceReportLineBase.Schema.PK] = Guid.NewGuid();
			table.Rows.Add(result);
			result.AcceptChanges();

			return result;
		}

		protected override void SetUp()
		{
			Creator = new TestObjectCreator(Factory);
			Report = Factory.NewWithValidTestData<AccComplianceReport>();
			var periodManager = Creator.CreateTestPeriods(Report.ACR_DateFrom.AddDays(-Report.ACR_DateFrom.Day + 1));
			Period = periodManager.Periods[0];

			InvoiceDescription = "Compliance Invoice Description";
			InvoiceCreateUser = Factory.New<GlbStaff>();
			InvoiceCreateUser = Creator.CreateStaff("TU1");
			InvoiceCreateUser.GS_FullName = "Head User";
			InvoiceCreateTime = ZDateTime.Today.AddDays(-5);
			Factory.Save();

			ReportLine = new AccComplianceReportLine(Factory, GetDataRow(Report));
			base.SetUp();
		}
	}
}
