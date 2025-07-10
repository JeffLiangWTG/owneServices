using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class K84MonthlyReportDocumentWrapperTest : K84ReportDocumentWrapperTestCase
	{
		#region TestSourceIdentifierProvider

		public void TestISourceIdentifierProvider()
		{
			var message = CreateMessageFromInterchangeString(Factory, interchangeString);
			var wrapper = new K84MonthlyReportDocumentWrapper(message);

			var supporter = wrapper as ISourceIdentifierProvider;
			AssertNotNull("K84MonthlyReportDocumentWrapper should implement ISourceIdentifierProvider", supporter);
			AssertEquals("supporter.SourceIdentifier", message.PK, supporter?.SourceIdentifier);
		}

		#endregion
		#region Interchange String

		const string interchangeString = @"UNB+UNOA:3+INETCECPT+YUSAIRXPN+110330:0607+567++++++1'
UNG+CUSDEC+K84++110330:0607+567+UN+S:99B'
UNH+1+CUSDEC:S:99B:UN'
BGM+++9'
LOC+127+:::0497'
DTM+130:20110330:102'
RFF+ABP:10207'
UNS+D'
DMS+K50'
DTM+137:20110303:102'
MOA+155:113703'
MOA+1:202532'
MOA+161:316235'
DMS+K50'
DTM+137:20110304:102'
MOA+155:7366'
MOA+1:25496'
MOA+161:32862'
DMS+K50'
DTM+137:20110307:102'
MOA+155:45668'
MOA+105:2000'
MOA+4:3000'
MOA+1:96257'
MOA+161:146925'
DMS+K50'
DTM+137:20110308:102'
MOA+155:11'
MOA+1:1861623'
MOA+161:1861634'
DMS+K51'
MOA+155:166748'
MOA+105:2000'
MOA+4:3000'
MOA+1:2185908'
MOA+161:2357656'
DMS+K52++000000395'
DTM+137:20110330:102'
MOA+155:1000'
MOA+105:2000'
MOA+4:3000'
MOA+1:4000'
MOA+161:10000'
DMS+K52++000000396'
DTM+137:20110331:102'
MOA+155:1000'
MOA+105:2000'
MOA+4:3000'
MOA+1:4000'
MOA+161:10000'
DMS+K53'
MOA+155:2000'
MOA+105:4000'
MOA+4:6000'
MOA+1:8000'
MOA+161:20000'
DMS+K54'
DTM+137:20110330:102'
MOA+155:1000'
MOA+105:2000'
MOA+4:3000'
MOA+1:4000'
MOA+161:10000'
MOA+201:5000'
MOA+128:15000'
DMS+K54'
DTM+137:20110331:102'
MOA+155:1000'
MOA+105:2000'
MOA+4:3000'
MOA+1:4000'
MOA+161:10000'
MOA+201:5000'
MOA+128:15000'
DMS+K56++000000395'
DTM+137:20110330:102'
MOA+155:1000'
MOA+105:2000'
MOA+4:3000'
MOA+1:4000'
MOA+161:10000'
MOA+201:5000'
MOA+128:15000'
DMS+K56++000000396'
DTM+137:20110331:102'
MOA+155:1000'
MOA+105:2000'
MOA+4:3000'
MOA+1:4000'
MOA+161:10000'
MOA+201:5000'
MOA+128:15000'
DMS+K60++000000395'
DTM+137:20110330:102'
MOA+201:5000'
DMS+K60++000000396'
DTM+137:20110331:102'
MOA+201:5000'
DMS+K61++000000395'
DTM+137:20110330:102'
MOA+201:10000'
DMS+K61++000000396'
DTM+137:20110331:102'
MOA+201:10000'
DMS+K62'
DTM+137:20110330:102'
MOA+201:15000'
DMS+K62'
DTM+137:20110331:102'
MOA+201:15000'
DMS+K65'
MOA+201:10000'
MOA+202:20000'
MOA+208:30000'
MOA+128:60000'
DMS+K66'
PAT+1+2011033099999999'
PCD+16:.375'
PAT+7+2011033199999999'
PCD+15:001.002'
DMS+K70'
DTM+140:20110331:102'
MOA+155:166748'
MOA+105:2000'
MOA+4:3000'
MOA+1:2185908'
MOA+161:2357656'
MOA+128:2357656'
UNS+S'
UNT+38+1'
UNE+1+567'
UNZ+1+567'";

		#endregion
		public override void TestProperties()
		{
			var message = CreateMessageFromInterchangeString(Factory, interchangeString);
			var wrapper = new K84MonthlyReportDocumentWrapper(message);
			AssertEquals("AccountingOfficeNumber", "0497", wrapper.AccountingOffice);
			AssertEquals("CurrentDate", new ZDateTime(2011, 03, 30), wrapper.StatementDate);
			AssertEquals("AccountSecurityNumber", "10207", wrapper.AccountSecurityNumber);

			//K50
			AssertEquals("DailyAccountingTotals.Count", 4, wrapper.DailyAccountingTotals.Count);

			var dailyAccountingTotal = wrapper.DailyAccountingTotals[0];
			AssertEquals("Accounting NoticeDate 1", new ZDateTime(2011, 03, 03), dailyAccountingTotal.NoticeDate);
			AssertAmounts(dailyAccountingTotal.Amounts, 1137.03m, 0, 0, 2025.32m, 3162.35m);

			dailyAccountingTotal = wrapper.DailyAccountingTotals[2];
			AssertEquals("Accounting NoticeDate 1", new ZDateTime(2011, 03, 07), dailyAccountingTotal.NoticeDate);
			AssertAmounts(dailyAccountingTotal.Amounts, 456.68m, 20m, 30m, 962.57m, 1469.25m);

			//K51
			AssertNotNull("MonthlyUnAdjustedTotals", wrapper.MonthlyUnAdjustedTotals);
			AssertAmounts(wrapper.MonthlyUnAdjustedTotals.Amounts, 1667.48m, 20m, 30m, 21859.08m, 23576.56m);

			//K52
			AssertEquals("TransactionCorrections.Count", 2, wrapper.TransactionCorrections.Count);

			var correction = wrapper.TransactionCorrections[0];
			AssertEquals("Correction TransactionNumber 1", "000000395", correction.TransactionNumber);
			AssertEquals("Correction NoticeDate 1", new ZDateTime(2011, 03, 30), correction.NoticeDate);
			AssertAmounts(correction.Amounts, 10m, 20m, 30m, 40m, 100m);

			correction = wrapper.TransactionCorrections[1];
			AssertEquals("Correction TransactionNumber 2", "000000396", correction.TransactionNumber);
			AssertEquals("Correction NoticeDate 2", new ZDateTime(2011, 03, 31), correction.NoticeDate);
			AssertAmounts(correction.Amounts, 10m, 20m, 30m, 40m, 100m);

			//K53
			AssertNotNull("TotalTransactionCorrections", wrapper.TotalTransactionCorrections);
			AssertAmounts(wrapper.TotalTransactionCorrections.Amounts, 20m, 40m, 60m, 80m, 200m);

			//K54
			AssertEquals("PeriodicInterimPaymentsByNotice.Count", 2, wrapper.PeriodicInterimPaymentsByNotice.Count);

			var payment = wrapper.PeriodicInterimPaymentsByNotice[0];
			AssertEquals("Payment By Notice NoticeDate 1", new ZDateTime(2011, 03, 30), payment.NoticeDate);
			AssertAmounts(payment.Amounts, 10m, 20m, 30m, 40m, 100m, 50m, 150m);

			payment = wrapper.PeriodicInterimPaymentsByNotice[1];
			AssertEquals("Payment By Notice NoticeDate 2", new ZDateTime(2011, 03, 31), payment.NoticeDate);
			AssertAmounts(payment.Amounts, 10m, 20m, 30m, 40m, 100m, 50m, 150m);

			//K56
			AssertEquals("PeriodicInterimPaymentsByTransaction.Count", 2, wrapper.PeriodicInterimPaymentsByTransaction.Count);

			payment = wrapper.PeriodicInterimPaymentsByTransaction[0];
			AssertEquals("Payment By Transaction TransactionNumber 1", "000000395", payment.TransactionNumber);
			AssertEquals("Payment By Transaction NoticeDate 1", new ZDateTime(2011, 03, 30), payment.NoticeDate);
			AssertAmounts(payment.Amounts, 10m, 20m, 30m, 40m, 100m, 50m, 150m);

			payment = wrapper.PeriodicInterimPaymentsByTransaction[1];
			AssertEquals("Payment By Transaction TransactionNumber 2", "000000396", payment.TransactionNumber);
			AssertEquals("Payment By Transaction NoticeDate 2", new ZDateTime(2011, 03, 31), payment.NoticeDate);
			AssertAmounts(payment.Amounts, 10m, 20m, 30m, 40m, 100m, 50m, 150m);

			//K60
			AssertEquals("LateFilingPenalties.Count", 2, wrapper.LateFilingPenalties.Count);

			var penalty = wrapper.LateFilingPenalties[0];
			AssertEquals("Penalty TransactionNumber 1", "000000395", penalty.TransactionNumber);
			AssertEquals("Penalty StatementDate 1", new ZDateTime(2011, 03, 30), penalty.StatementDate);
			AssertEquals("Penalty Amount 1", 50m, penalty.Amount);

			penalty = wrapper.LateFilingPenalties[1];
			AssertEquals("Penalty TransactionNumber 2", "000000396", penalty.TransactionNumber);
			AssertEquals("Penalty StatementDate 2", new ZDateTime(2011, 03, 31), penalty.StatementDate);
			AssertEquals("Penalty Amount 2", 50m, penalty.Amount);

			//K61
			AssertEquals("LateAccountingInterestCharges.Count", 2, wrapper.LateAccountingInterestCharges.Count);

			var accountingInterestCharge = wrapper.LateAccountingInterestCharges[0];
			AssertEquals("Accounting Interest Charge TransactionNumber 1", "000000395", accountingInterestCharge.TransactionNumber);
			AssertEquals("Accounting Interest Charge StatementDate 1", new ZDateTime(2011, 03, 30), accountingInterestCharge.StatementDate);
			AssertEquals("Accounting Interest Charge Amount 1", 100m, accountingInterestCharge.Amount);

			accountingInterestCharge = wrapper.LateAccountingInterestCharges[1];
			AssertEquals("Accounting Interest Charge TransactionNumber 2", "000000396", accountingInterestCharge.TransactionNumber);
			AssertEquals("Accounting Interest Charge StatementDate 2", new ZDateTime(2011, 03, 31), accountingInterestCharge.StatementDate);
			AssertEquals("Accounting Interest Charge Amount 2", 100m, accountingInterestCharge.Amount);

			//K62
			AssertEquals("LateK84InterestCharges.Count", 2, wrapper.LateK84InterestCharges.Count);

			var k84InterestCharge = wrapper.LateK84InterestCharges[0];
			AssertEquals("K84 Interest Charge StatementDate 1", new ZDateTime(2011, 03, 30), k84InterestCharge.StatementDate);
			AssertEquals("K84 Interest Charge Amount 1", 150m, k84InterestCharge.Amount);

			k84InterestCharge = wrapper.LateK84InterestCharges[1];
			AssertEquals("K84 Interest Charge StatementDate 2", new ZDateTime(2011, 03, 31), k84InterestCharge.StatementDate);
			AssertEquals("K84 Interest Charge Amount 2", 150m, k84InterestCharge.Amount);

			//K65
			var details = wrapper.TotalLateFilingAndInterestDetails;
			AssertNotNull("TotalLateFilingAndInterestDetails", details);
			AssertEquals("TotalLateFilingPenaltyAmount", 100m, details.LateFilingPenalty);
			AssertEquals("TotalLateAccountInterest", 200m, details.LateAccountInterest);
			AssertEquals("TotalLateK84Interest", 300m, details.LateK84Interest);
			AssertEquals("TotalPenaltiesAndInterest", 600m, details.TotalPenaltiesAndInterest);

			//K66
			AssertNotNull("InterestRates", wrapper.InterestRates);

			AssertEquals("InterestEffectiveDate", new ZDateTime(2011, 03, 30), wrapper.InterestRates.InterestEffectiveDate);
			AssertEquals("InterestExpiryDate", new ZDateTime(2079, 6, 5), wrapper.InterestRates.InterestExpiryDate);
			AssertEquals("InterestRate", 0.375m, wrapper.InterestRates.InterestPercentage);

			AssertEquals("InterestEffectiveDate", new ZDateTime(2011, 03, 31), wrapper.InterestRates.PenaltyEffectiveDate);
			AssertEquals("InterestExpiryDate", new ZDateTime(2079, 6, 5), wrapper.InterestRates.PenaltyExpiryDate);
			AssertEquals("InterestRate", 1.002m, wrapper.InterestRates.PenaltyPercentage);

			//K70
			AssertNotNull("MonthlyAdjustedTotals", wrapper.MonthlyAdjustedTotals);
			AssertEquals("NoticeDate", new ZDateTime(2011, 03, 31), wrapper.MonthlyAdjustedTotals.DueDate);
			AssertAmounts(wrapper.MonthlyAdjustedTotals.Amounts, 1667.48m, 20m, 30m, 21859.08m, 23576.56m, 0, 23576.56m);
		}

		public void TestLoadLatestDeclaration()
		{
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			importer.OH_Code = "IMPORTERX";
			importer.OH_FullName = "IMPORTERX NAME";

			var dec1 = CreatedDeclaration("25AE414E-13C1-4A36-9562-94E460CC8EDE", Factory, importer, "B0000111X", "10207000000396", ZDateTime.UtcNow.AddDays(-1));
			var dec2 = CreatedDeclaration("223E27F8-45F6-459A-B9E7-705381E54B2E", Factory, importer, "B0000111Y", "10207000000396", ZDateTime.UtcNow);
			Factory.Save();

			#region Interchange String

			const string interchangeString = @"UNB+UNOA:3+INETCECPT+YUSAIRXPN+110330:0607+567++++++1'
UNG+CUSDEC+K84++110330:0607+567+UN+S:99B'
UNH+1+CUSDEC:S:99B:UN'
BGM+++9'
LOC+127+:::0497'
DTM+130:20110330:102'
RFF+ABP:10207'
UNS+D'
DMS+K50'
DTM+137:20110303:102'
MOA+155:113703'
MOA+1:202532'
MOA+161:316235'
DMS+K50'
DTM+137:20110304:102'
MOA+155:7366'
MOA+1:25496'
MOA+161:32862'
DMS+K50'
DTM+137:20110307:102'
MOA+155:45668'
MOA+105:2000'
MOA+4:3000'
MOA+1:96257'
MOA+161:146925'
DMS+K50'
DTM+137:20110308:102'
MOA+155:11'
MOA+1:1861623'
MOA+161:1861634'
DMS+K51'
MOA+155:166748'
MOA+105:2000'
MOA+4:3000'
MOA+1:2185908'
MOA+161:2357656'
DMS+K52++000000395'
DTM+137:20110330:102'
MOA+155:1000'
MOA+105:2000'
MOA+4:3000'
MOA+1:4000'
MOA+161:10000'
DMS+K52++000000396'
DTM+137:20110331:102'
MOA+155:1000'
MOA+105:2000'
MOA+4:3000'
MOA+1:4000'
MOA+161:10000'
UNS+S'
UNT+38+1'
UNE+1+567'
UNZ+1+567'";

			#endregion

			var message = CreateMessageFromInterchangeString(Factory, interchangeString);
			var wrapper = new K84MonthlyReportDocumentWrapper(message);

			var correction = wrapper.TransactionCorrections[1];
			AssertEquals("Correction TransactionNumber 2", "000000396", correction.TransactionNumber);
			AssertContains("Correction JobNumber", "B0000111Y", correction.JobNumber);
		}

		static JobDeclaration CreatedDeclaration(string pk, BusinessObjectFactory factory, OrgHeader importer, ZString jobNumber, ZString transactionNo, ZDateTime createTime)
		{
			var declaration = factory.NewWithPrimaryKey<JobDeclaration>(new Guid(pk));

			declaration.JE_GB = GlbBranch.CurrentBranch.PK;

			if (importer != null)
			{
				declaration.JE_OH_Importer = importer.PK;
			}

			declaration.JE_DeclarationReference = jobNumber;
			var documentNumber = CusEntryNumber.LoadOrCreate(declaration, CusEntryNumber.EntryType.CATransactionNumber, "CA");
			documentNumber.CE_EntryNum = transactionNo;
			declaration.JE_PaymentMethod = Customs.Business.PaymentPartyCodeDescriptionList.Codes.Broker;

			var topGroupInvoice = declaration.JobComInvoiceGroupHeaders.AddNew();
			topGroupInvoice.JZ_InvoiceNumber = JobComInvoiceGroupHeader.AllInvoices;
			declaration.JE_SystemCreateTimeUtc = createTime;

			return declaration;
		}
	}
}
