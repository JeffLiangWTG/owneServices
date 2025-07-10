using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.DataTransfer.ComplianceReport.IL.OpenFormat;
using Enterprise.DocumentEngine.PdfBuilder;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.DataTransfer.Testing.ComplianceReport.IL.OpenFormat
{
	public class OpenFormatFileWriterTest : TestCaseWithFactory
	{
		[TestDate(2024, 3, 20, 15, 22, 48, 789)]
		public void TestExportOpenFormatFile()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Israel))
			{
				var arInvoice1 = Creator.CreateARInvoice<ARInvoice>("INV001", Creator.EUR, 1.78m, Creator.ABIGAS);
				var arInvoice1Line1 = Creator.CreateInvoiceLine(arInvoice1, Creator.EUR, 1.78m, 100m, 10m, 0m);
				arInvoice1Line1.AL_AC = Creator.CC10.PK;
				arInvoice1Line1.AL_AT = Creator.GST1.PK;
				var arInvoice1Line2 = Creator.CreateInvoiceLine(arInvoice1, Creator.EUR, 1.78m, 200m, 0m, 0m);

				var arInvoice2 = Creator.CreateARInvoice<ARInvoice>("INV002", Creator.USD, 1.55m, Creator.Debtor);
				var arInvoice2Line1 = Creator.CreateInvoiceLine(arInvoice2, Creator.USD, 1.55m, 100m, 10m, 0m);
				arInvoice2Line1.AL_AC = Creator.CC10.PK;
				Factory.Save();

				var report = GetOpenFormatReport();
				Creator.CreateComplianceReportTransactionPivot(report, arInvoice1Line1, sequence: 1);
				Creator.CreateComplianceReportTransactionPivot(report, arInvoice1Line2, sequence: 2);
				Creator.CreateComplianceReportTransactionPivot(report, arInvoice2Line1, sequence: 3);

				var expectedResult = @"A100000000001123456789240320152248789&OF1.31&                                                  
C100000000002123456789305            00001000202403201522                                   ABI GAS & TOOLS                               171 ABBOTSFORD ROAD                                  Sydney    4006                     AustraliaAU   +6128001277798765432120240320+00000000031000EUR+00000000016854+00000000000000+00000000016854+00000000000562+00000000017416+00000000000         ABIGAS           20240320    BNECargoWise0000001             
D110000000003123456789305            000010000001000                    1              ZZCC10                     tee he he                                                                                                unit00000000000001+00000000005618+00000000000000+000000000056181000    BNE202403200000001                            
D110000000004123456789305            000010000002000                    1                                         tee he he                                                                                                unit00000000000001+00000000011236+00000000000000+000000000112360000    BNE202403200000001                            
C100000000005123456789305            00001001202403201522                                 Test Company Name                                   184 Bourke Road                              Alexandria                                IsraelIL                        20240320+00000000011000USD+00000000006452+00000000000000+00000000006452+00000000000645+00000000007097+00000000000        ZDebtor           20240320    BNECargoWise0000002             
D110000000006123456789305            000010010001000                    1              ZZCC10                     tee he he                                                                                                unit00000000000001+00000000006452+00000000000000+000000000064521000    BNE202403200000002                            
Z900000000007123456789240320152248789&OF1.31&000000000000007                                                  
";

				var result = GenerateBkmvdata(report);
				AssertEquals(expectedResult, result);
			}
		}

		[TestDate(2024, 3, 20, 15, 22, 48, 789)]
		public void TestExportOpenFormatIniFile()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Israel))
			{
				var arInvoice1 = Creator.CreateARInvoice<ARInvoice>("INV001", Creator.EUR, 1.78m, Creator.ABIGAS);
				var arInvoice1Line1 = Creator.CreateInvoiceLine(arInvoice1, Creator.EUR, 1.78m, 100m, 10m, 0m);
				arInvoice1Line1.AL_AC = Creator.CC10.PK;
				arInvoice1Line1.AL_AT = Creator.GST1.PK;
				var arInvoice1Line2 = Creator.CreateInvoiceLine(arInvoice1, Creator.EUR, 1.78m, 200m, 0m, 0m);

				var arInvoice2 = Creator.CreateARInvoice<ARInvoice>("INV002", Creator.USD, 1.55m, Creator.Debtor);
				var arInvoice2Line1 = Creator.CreateInvoiceLine(arInvoice2, Creator.USD, 1.55m, 100m, 10m, 0m);
				arInvoice2Line1.AL_AC = Creator.CC10.PK;
				Factory.Save();

				var report = GetOpenFormatReport();
				Creator.CreateComplianceReportTransactionPivot(report, arInvoice1Line1, sequence: 1);
				Creator.CreateComplianceReportTransactionPivot(report, arInvoice1Line2, sequence: 2);
				Creator.CreateComplianceReportTransactionPivot(report, arInvoice2Line1, sequence: 3);

				var expectedResult = @"A000     000000000000005123456789240320152248789&OF1.31&00000000CargoWise Accounting                    560038416 WiseTech Global LTD2                 D:\OPENFRMT\560038416.24\0101000021123456789987654321                                         EDI CUSTOMS BROKERS                               10 HUTCHESON STREET                                  ALBION 12345670000202403202024033020240320152221                    ILS1                                              
C100000000000000002
D110000000000000003
";

				var result = GenerateInidata(report);
				AssertEquals(expectedResult, result);
			}
		}

		[TestDate(2024, 3, 20, 15, 22, 48, 789)]
		public void TestExportOpenFormatPdfFile()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Israel))
			{
				// Arrange
				var arInvoice1 = Creator.CreateARInvoice<ARInvoice>("INV001", Creator.EUR, 1.78m, Creator.ABIGAS);
				var arInvoice1Line1 = Creator.CreateInvoiceLine(arInvoice1, Creator.EUR, 1.78m, 100m, 10m, 0m);
				arInvoice1Line1.AL_AC = Creator.CC10.PK;
				arInvoice1Line1.AL_AT = Creator.GST1.PK;
				var arInvoice1Line2 = Creator.CreateInvoiceLine(arInvoice1, Creator.EUR, 1.78m, 200m, 0m, 0m);

				var arInvoice2 = Creator.CreateARInvoice<ARInvoice>("INV002", Creator.USD, 1.55m, Creator.Debtor);
				var arInvoice2Line1 = Creator.CreateInvoiceLine(arInvoice2, Creator.USD, 1.55m, 100m, 10m, 0m);
				arInvoice2Line1.AL_AC = Creator.CC10.PK;
				Factory.Save();

				var report = GetOpenFormatReport();
				Creator.CreateComplianceReportTransactionPivot(report, arInvoice1Line1, sequence: 1);
				Creator.CreateComplianceReportTransactionPivot(report, arInvoice1Line2, sequence: 2);
				Creator.CreateComplianceReportTransactionPivot(report, arInvoice2Line1, sequence: 3);

				var actual = GenerateReportPdf(report);

				using var schemeStream = new MemoryStream(Encoding.UTF8.GetBytes(ComplianceReportPdfSchemeEN));

				// Act
				var expected = PdfBuilderXmlSchemeLoader.Load(schemeStream);

				// Assert
				AssertContainsExactElementsInExactOrder(actual, expected);
			}
		}

		[TestDate(2024, 3, 20, 15, 22, 48, 789)]
		public void TestExportOpenFormatFile_DocumentType_ARInvoice()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Israel))
			{
				var arInv305 = Creator.CreateARInvoice<ARInvoice>("T001", Creator.EUR, 1.78m, Creator.ABIGAS);
				Creator.CreateInvoiceLine(arInv305, Creator.EUR, 1.78m, 100m, 10m, 0m);
				arInv305.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;

				var arInv310 = Creator.CreateARInvoice<ARInvoice>("T002", Creator.EUR, 1.78m, Creator.ABIGAS);
				Creator.CreateInvoiceLine(arInv310, Creator.EUR, 1.78m, 100m, 10m, 0m);
				arInv310.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice_Batching;

				Factory.Save();

				var report = GetOpenFormatReport();
				Creator.CreateComplianceReportTransactionPivot(report, arInv305.Lines[0], sequence: 1);
				Creator.CreateComplianceReportTransactionPivot(report, arInv310.Lines[0], sequence: 2);

				var c100Parts = GetC100Parts(GenerateBkmvdata(report));

				AssertContainsExactElementsInExactOrder("document type", new[] { "C100000000002123456789305", "C100000000004123456789310" }, c100Parts);
			}
		}

		[TestDate(2024, 3, 20, 15, 22, 48, 789)]
		public void TestExportOpenFormatFile_DocumentType_APInvoice()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Israel))
			{
				var apInv700 = Creator.CreateAPInvoice<APInvoice>("T001", Creator.AUD, 1M, 0, 0, 0, 200, 0, 0, Creator.ABIGAS);
				Creator.CreateInvoiceLine(apInv700, Creator.EUR, 1.78m, 100m, 10m, 0m);
				Factory.Save();

				var report = GetOpenFormatReport();
				Creator.CreateComplianceReportTransactionPivot(report, apInv700.Lines[0], sequence: 1);

				var c100Parts = GetC100Parts(GenerateBkmvdata(report));

				AssertContainsExactElementsInExactOrder("document type", new[] { "C100000000002123456789700" }, c100Parts);
			}
		}

		[TestDate(2024, 3, 20, 15, 22, 48, 789)]
		public void TestExportOpenFormatFile_DocumentType_ARCreditNote()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Israel))
			{
				var arCrd330 = Creator.CreateARCreditNoteWithLine("T001", Creator.Debtor, Creator.AUD, 10.0m, "Credit Note", null, Creator.CC1, 20.00m, ZDateTime.Today, false);

				var invoice = Creator.CreateARInvoice<ARInvoice>("0001", Creator.USD, 1m, Creator.AALSHI);
				Factory.Save();

				var arCrd330Cancelled = Creator.CreateARCreditNoteWithLine("T002", Creator.Debtor, Creator.AUD, 1.0m, "Credit Note", null, Creator.CC1, 100.00m, ZDateTime.Today, false);
				arCrd330Cancelled.AH_TransactionBelongsToGroup = invoice.PK;
				arCrd330Cancelled.AH_IsCancelled = true;
				((IMatching)arCrd330Cancelled).CurrentMatchGroup.AddNew().AP_AH = arCrd330Cancelled.PK;
				TestObjectCreator.SetupMatchLinkMatchDate(arCrd330Cancelled);

				Factory.Save();

				var report = GetOpenFormatReport();
				Creator.CreateComplianceReportTransactionPivot(report, arCrd330.Lines[0], sequence: 1);
				Creator.CreateComplianceReportTransactionPivot(report, arCrd330Cancelled.Lines[0], sequence: 2);

				var c100Parts = GetC100Parts(GenerateBkmvdata(report));

				AssertContainsExactElementsInExactOrder("document type", new[] { "C100000000002123456789330", "C100000000004123456789330" }, c100Parts);
			}
		}

		[TestDate(2024, 3, 20, 15, 22, 48, 789)]
		public void TestExportOpenFormatFile_DocumentType_APCreditNote()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Israel))
			{
				var apCrd710 = Creator.CreateAPCreditNoteWithLine("T001", Creator.ABIGAS, Creator.AUD, 1.0m, "Credit Note", null, Creator.NonAccrualChargeCode, 1000.00m, ZDateTime.Today, false);
				Factory.Save();

				var report = GetOpenFormatReport();
				Creator.CreateComplianceReportTransactionPivot(report, apCrd710.Lines[0], sequence: 1);

				var c100Parts = GetC100Parts(GenerateBkmvdata(report));

				AssertContainsExactElementsInExactOrder("document type", new[] { "C100000000002123456789710" }, c100Parts);
			}
		}

		[TestDate(2024, 3, 20, 15, 22, 48, 789)]
		public void TestExportOpenFormatFile_DocumentType_ARAdjustmentNote()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Israel))
			{
				var arAdj305 = Creator.CreateAdjustmentNote<ARAdjustmentNote>("T001", 100.81m, 2.72m, ZDateTime.Now, Creator.ABIGAS.PK);
				Creator.CreateAdjusmentNoteLine(arAdj305, Creator.CC1.PK, 10m, 0m);

				var arAdj305Cancelled = Creator.CreateAdjustmentNote<ARAdjustmentNote>("T002", 100.81m, 2.72m, ZDateTime.Now, Creator.ABIGAS.PK);
				Creator.CreateAdjusmentNoteLine(arAdj305Cancelled, Creator.CC1.PK, 10m, 0m);

				arAdj305Cancelled.GenerateReverseTransaction(true);
				arAdj305Cancelled.SetCancellationFlag(true);
				InvoicingBase reverseInvoice = arAdj305Cancelled.ReverseInvoice;
				reverseInvoice.AH_TransactionNum = "001";
				reverseInvoice.SetCancellationFlag(true);
				arAdj305Cancelled.GenerateMatchLinks();
				reverseInvoice.GenerateMatchLinks();
				TestObjectCreator.SetupMatchLinkMatchDate(arAdj305Cancelled);
				TestObjectCreator.SetupMatchLinkMatchDate(reverseInvoice);

				Factory.Save();

				var report = GetOpenFormatReport();
				Creator.CreateComplianceReportTransactionPivot(report, arAdj305.Lines[0], sequence: 1);
				Creator.CreateComplianceReportTransactionPivot(report, arAdj305Cancelled.Lines[0], sequence: 2);

				var c100Parts = GetC100Parts(GenerateBkmvdata(report));

				AssertContainsExactElementsInExactOrder("document type", new[] { "C100000000002123456789305", "C100000000004123456789305" }, c100Parts);
			}
		}

		[TestDate(2024, 3, 20, 15, 22, 48, 789)]
		public void TestExportOpenFormatFile_DocumentType_APAdjustmentNote()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Israel))
			{
				var apAdj700 = Creator.CreateAdjustmentNote<APAdjustmentNote>("T001", 100.81m, 0m, ZDateTime.Now, Creator.ABIGAS.PK);
				Creator.CreateAdjusmentNoteLine(apAdj700, Creator.NonAccrualChargeCode.PK, 10m, 0m);
				Factory.Save();

				var report = GetOpenFormatReport();
				Creator.CreateComplianceReportTransactionPivot(report, apAdj700.Lines[0], sequence: 1);

				var c100Parts = GetC100Parts(GenerateBkmvdata(report));

				AssertContainsExactElementsInExactOrder("document type", new[] { "C100000000002123456789700" }, c100Parts);
			}
		}

		string[] GetC100Parts(string report)
		{
			return report.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
				.Where(x => x.StartsWith("C100")).Select(x => x.Substring(0, 25)).ToArray();
		}

		[TestDate(2024, 3, 20, 15, 22, 48, 789)]
		public void TestExportOpenFormatFile_MoneyFormat()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Israel))
			{
				var arInv = Creator.CreateARInvoice<ARInvoice>("T001", Creator.EUR, 1.78m, Creator.Debtor);
				Creator.CreateInvoiceLine(arInv, Creator.EUR, 1.78m, 245m, 10m, 0m);
				Creator.CreateInvoiceLine(arInv, Creator.EUR, 1.78m, -300m, 10m, 0m);

				var apInv1 = Creator.CreateAPInvoice<APInvoice>("T002", Creator.AUD, 1M, 0, 0, 0, 1996.78m, 0, 0, Creator.ABIGAS);
				Creator.CreateInvoiceLine(apInv1, Creator.AUD, 1m, 1996.78m, 0m, 0m);

				var ilsCurrency = Creator.GetCurrency("ILS");
				var apInv2 = Creator.CreateAPInvoice<APInvoice>("T003", ilsCurrency, 1M, 0, 0, 0, -542.56m, 0, 0, Creator.ABIGAS);
				Creator.CreateInvoiceLine(apInv2, ilsCurrency, 1m, -542.56m, 0m, 0m);

				Factory.Save();

				var report = GetOpenFormatReport();
				Creator.CreateComplianceReportTransactionPivot(report, arInv.Lines[0], sequence: 1);
				Creator.CreateComplianceReportTransactionPivot(report, arInv.Lines[1], sequence: 2);
				Creator.CreateComplianceReportTransactionPivot(report, apInv1.Lines[0], sequence: 3);
				Creator.CreateComplianceReportTransactionPivot(report, apInv2.Lines[0], sequence: 4);

				var expectedResult = @"A100000000001123456789240320152248789&OF1.31&                                                  
C100000000002123456789305            00001000202403201522                                 Test Company Name                                   184 Bourke Road                              Alexandria                                IsraelIL                        20240320-00000000003500EUR-00000000003090+00000000000000-00000000003090+00000000001124-00000000001966+00000000000        ZDebtor           20240320    BNECargoWise0000001             
D110000000003123456789305            000010000001000                    1                                         tee he he                                                                                                unit00000000000001+00000000013764+00000000000000+000000000137640000    BNE202403200000001                            
D110000000004123456789305            000010000002000                    1                                         tee he he                                                                                                unit00000000000001-00000000016854+00000000000000-000000000168540000    BNE202403200000001                            
C100000000005123456789700                T002202403201522                                   ABI GAS & TOOLS                               171 ABBOTSFORD ROAD                                  Sydney    4006                     AustraliaAU   +6128001277798765432120240320+00000000399356AUD+00000000399356+00000000000000+00000000399356+00000000000000+00000000399356+00000000000         ABIGAS           20240320    BNECargoWise0000002             
D110000000006123456789700                T0020001000                    1                                         tee he he                                                                                                unit00000000000001+00000000199678+00000000000000+000000001996780000    BNE202403200000002                            
C100000000007123456789700                T003202403201522                                   ABI GAS & TOOLS                               171 ABBOTSFORD ROAD                                  Sydney    4006                     AustraliaAU   +6128001277798765432120240320                  -00000000108512+00000000000000-00000000108512+00000000000000-00000000108512+00000000000         ABIGAS           20240320    BNECargoWise0000003             
D110000000008123456789700                T0030001000                    1                                         tee he he                                                                                                unit00000000000001-00000000054256+00000000000000-000000000542560000    BNE202403200000003                            
Z900000000009123456789240320152248789&OF1.31&000000000000009                                                  
";

				var result = GenerateBkmvdata(report);
				AssertEquals(expectedResult, result);
			}
		}

		[TestDate(2024, 3, 20, 15, 22, 48, 789)]
		public void TestExportOpenFormatFile_MaxSize()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Israel))
			{
				var arInvoice = Creator.CreateARInvoice<ARInvoice>("INV001", Creator.USD, 1.55m, Creator.Debtor);
				var arInvoiceLine = Creator.CreateInvoiceLine(arInvoice, Creator.USD, 1.55m, 100m, 10m, 0m);
				arInvoiceLine.AL_AC = Creator.CC10.PK;

				Creator.Debtor.OH_FullName = new string('B', OrgHeaderSchema.OH_FullName.MaxLength);
				Creator.Debtor.MainAddress.OA_Address1 = new string('C', OrgAddressSchema.OA_Address1.MaxLength);
				Creator.Debtor.MainAddress.OA_City = new string('D', OrgAddressSchema.OA_City.MaxLength);
				Creator.Debtor.MainAddress.OA_PostCode = new string('E', OrgAddressSchema.OA_PostCode.MaxLength);
				Creator.Debtor.MainAddress.OA_RN_NKCountryCode = CountryCodes.SaintVincentAndTheGrenadin;
				Creator.Debtor.MainAddress.OA_Phone = new string('1', OrgAddressSchema.OA_Phone.MaxLength);
				Creator.Debtor.OH_Code = new string('F', OrgHeaderSchema.OH_Code.MaxLength);
				arInvoice.Branch.GB_Code = new string('G', GlbBranchSchema.GB_Code.MaxLength);
				GlbStaff.CurrentUser.GS_FullName = new string('H', GlbStaffSchema.GS_FullName.MaxLength);
				arInvoiceLine.AL_Sequence = short.MaxValue;
				arInvoiceLine.ChargeCode.AC_Code = new string('I', AccChargeCodeSchema.AC_Code.MaxLength);
				arInvoiceLine.ChargeCode.AC_Desc = new string('J', AccChargeCodeSchema.AC_Desc.MaxLength);

				Factory.Save();

				var report = GetOpenFormatReport();
				Creator.CreateComplianceReportTransactionPivot(report, arInvoiceLine, sequence: 1);

				var expectedResult = @"A100000000001123456789240320152248789&OF1.31&                                                  
C100000000002123456789305            00001000202403201522BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC          DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDEEEEEEEESaint Vincent and the GrenadinVC111111111111111         20240320+00000000011000USD+00000000006452+00000000000000+00000000006452+00000000000645+00000000007097+00000000000   FFFFFFFFFFFF           20240320    GGGCargoWise0000001             
D110000000003123456789305            000010002767000                    1          IIIIIIIIII                     tee he he                                                                                                unit00000000000001+00000000006452+00000000000000+000000000064521000    GGG202403200000001                            
Z900000000004123456789240320152248789&OF1.31&000000000000004                                                  
";

				var result = GenerateBkmvdata(report);
				AssertEquals(expectedResult, result);
			}
		}

		[TestDate(2024, 3, 20, 15, 22, 48, 789)]
		public void TestExportOpenFormatFile_DealType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Israel))
			{
				var job = Creator.CreateJob(Creator.CreateShipment("00002001"));
				var charge1 = Creator.CreateCharge(job, Creator.GoodsClassChargeCode, "charge", Creator.AUD, 20m, Creator.Creditor1, Creator.AUD, 20m, Creator.ABIGAS);
				var charge2 = Creator.CreateCharge(job, Creator.DSBChargeCode1, "charge", Creator.AUD, 20m, Creator.Creditor1, Creator.AUD, 20m, Creator.ABIGAS);
				Factory.Save();
				var apInvoice = Creator.CreateInvoice(typeof(APInvoice), "1001", Creator.AUD, 1M, Creator.ABIGAS);
				apInvoice.Lines.Add(Creator.CreateCostLine(charge1, apInvoice.PK));
				apInvoice.Lines.Add(Creator.CreateCostLine(charge2, apInvoice.PK));
				Factory.Save();

				AssertEquals("PreCond: line1 charge code type is GDS", GoodServiceTypes.Codes.GDS, apInvoice.Lines[0].ChargeCode.AC_GoodsServiceType);
				AssertEquals("PreCond: line2 charge code type is SRV", GoodServiceTypes.Codes.SRV, apInvoice.Lines[1].ChargeCode.AC_GoodsServiceType);

				Factory.Save();

				var report = GetOpenFormatReport();
				Creator.CreateComplianceReportTransactionPivot(report, apInvoice.Lines[0], sequence: 1);
				Creator.CreateComplianceReportTransactionPivot(report, apInvoice.Lines[1], sequence: 2);

				var expectedResult = @"A100000000001123456789240320152248789&OF1.31&                                                  
C100000000002123456789700                1001202403201522                                   ABI GAS & TOOLS                               171 ABBOTSFORD ROAD                                  Sydney    4006                     AustraliaAU   +6128001277798765432120240320+00000000004400AUD+00000000004000+00000000000000+00000000004000+00000000000400+00000000004400+00000000000         ABIGAS           20240320    BNECargoWise0000001             
D110000000003123456789700                10010001000                    2             ZZCCGDS                        charge                                                                                                unit00000000000001+00000000002000+00000000000000+000000000020001000    BNE202403200000001                            
D110000000004123456789700                10010001000                    1              ZZDSB1                        charge                                                                                                unit00000000000001+00000000002000+00000000000000+000000000020001000    BNE202403200000001                            
Z900000000005123456789240320152248789&OF1.31&000000000000005                                                  
";

				var result = GenerateBkmvdata(report);
				AssertEquals(expectedResult, result);
			}
		}

		string GenerateBkmvdata(AccComplianceReport report)
		{
			using (var stream = new MemoryStream())
			{
				var writer = new OpenFormatFileWriter(report);
				writer.WriteBkmvdataToStream(stream);

				AssertEquals(2, writer.CompletedItems);
				AssertNotEquals(1, writer.TotaItemsToComplete);
				AssertEquals("Open format files export completed.", writer.CurrentStatusText);

				stream.Position = 0;
				string result = null;

				using (var reader = new StreamReader(stream))
				{
					result = reader.ReadToEnd();
				}

				AssertNotNull(result);
				return result;
			}
		}

		string GenerateInidata(AccComplianceReport report)
		{
			using (var stream = new MemoryStream())
			using (var iniStream = new MemoryStream())
			{
				var writer = new OpenFormatFileWriter(report);
				var counters = writer.WriteBkmvdataToStream(stream);
				writer.WriteInidataToStream(iniStream, counters, "D:\\OPENFRMT\\560038416.24\\01010000", ZDateTime.Now);

				AssertEquals(writer.TotaItemsToComplete, writer.CompletedItems);
				AssertNotEquals(1, writer.TotaItemsToComplete);
				AssertEquals("Open format content files export completed.", writer.CurrentStatusText);

				iniStream.Position = 0;
				string result = null;

				using (var reader = new StreamReader(iniStream))
				{
					result = reader.ReadToEnd();
				}
				AssertNotNull(result);
				return result;
			}
		}

		PdfElement[] GenerateReportPdf(AccComplianceReport report)
		{
			using (var stream = new MemoryStream())
			using (var iniStream = new MemoryStream())
			{
				PdfElement[] results = null;

				var mockPdfWriter = new Mock<IPdfBuilder>();
				mockPdfWriter.Setup(x => x.Build(It.IsAny<PdfElement[]>(), It.IsAny<Stream>())).Callback(
					(PdfElement[] elements, Stream stream) => { results = elements; });

				var writer = new OpenFormatFileWriter(report);
				writer.PDFProvider_TestOnly = mockPdfWriter.Object;

				var counters = writer.WriteBkmvdataToStream(stream);

				writer.WritePdfFileToStream(stream, counters, ZDateTime.Now, "D:\\OPENFRMT\\560038416.24\\01010000");

				AssertEquals("Open format files export completed.", writer.CurrentStatusText);

				AssertNotNull(results);

				return results;
			}
		}

		public void TestExportOpenFormatFile_GetReportDataErrorMessage_MissingOrIncorrectOrgProxyVAT()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Israel))
			{
				var report = GetOpenFormatReport();
				report.Company.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
				var writer = new OpenFormatFileWriter(report);
				AssertEquals("Israel Open Format VAT number must be filled with 9 digits. Current value is: .", writer.GetReportDataErrorMessage());

				Creator.CreateCustomsCodes(report.Company.OrgProxy, CountryCodes.Israel, OrgCusCode.CodeTypes.VATCode, "12345");
				AssertEquals("Israel Open Format VAT number must be filled with 9 digits. Current value is: 12345.", writer.GetReportDataErrorMessage());

				report.Company.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
				Creator.CreateCustomsCodes(report.Company.OrgProxy, CountryCodes.Israel, OrgCusCode.CodeTypes.VATCode, "123456789");
				AssertEquals("Israel Tax File Code must be filled with 9 digits. Current value is: .", writer.GetReportDataErrorMessage());
			}
		}

		public void TestExportOpenFormatFile_GetReportDataErrorMessage_MissingOrIncorrectReportUniqueIdentifier()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Israel))
			{
				var report = GetOpenFormatReport();
				report.ACR_ReferenceNumber = string.Empty;
				var writer = new OpenFormatFileWriter(report);
				AssertEquals("Israel Open Format unique identifier must be filled with 15 digits. Current value is: .", writer.GetReportDataErrorMessage());

				report.ACR_ReferenceNumber = "6789";
				AssertEquals("Israel Open Format unique identifier must be filled with 15 digits. Current value is: 6789.", writer.GetReportDataErrorMessage());
			}
		}

		AccComplianceReport GetOpenFormatReport()
		{
			Creator.EnsureComplianceReportConfigInRegistry(AccComplianceReport.ReportTypes.OpenFormatOnlyTransactions, OrgCusCode.CodeTypes.VATCode,
				ReportPeriodicityCodes.DateRange, ReportBaseTablePrefixListCodes.TransactionLine, ReportLineGroupingListCodes.NoGrouping);
			var report = Creator.CreateComplianceReport(AccComplianceReport.ReportTypes.OpenFormatOnlyTransactions, AccComplianceReport.Status.ReportGenerated);
			Creator.CreateCustomsCodes(report.Company.OrgProxy, CountryCodes.Israel, OrgCusCode.CodeTypes.VATCode, "123456789");
			Creator.CreateCustomsCodes(report.Company.OrgProxy, CountryCodes.Israel, OrgCusCode.CodeTypes.TaxFileCode, "987654321");

			report.Company.OrgProxy.Addresses[0].Postcode = "1234567";

			Creator.ABIGAS.MainAddress.OA_City = "Sydney";
			Creator.ABIGAS.MainAddress.OA_Phone = "+61280012777";
			Creator.CreateCustomsCodes(Creator.ABIGAS, CountryCodes.Israel, OrgCusCode.CodeTypes.VATCode, "987654321");

			Factory.Save();
			Assert(report.SupportsExportOpenFormatFile);
			Assert(!report.ACR_ReferenceNumber.IsEmpty);

			return report;
		}

		TestObjectCreator Creator => creator ?? (creator = new TestObjectCreator(Factory));
		TestObjectCreator creator;

		const string ComplianceReportPdfSchemeEN = @"
<scheme>
	<TextSection>
		<Texts>
			<Line>Generating files in a open format structure has been completed successfully	EDI CUSTOMS BROKERS</Line>
			<Line>Authorized dealer number:	123456789</Line>
			<Line>name of the business:	EDI CUSTOMS BROKERS</Line>
			<Line>The data was saved in the following path:	D:\OPENFRMT\560038416.24\01010000</Line>
			<Line>The date range according to which the data was produced:	20/03/24 - 30/03/24</Line>
		</Texts>
	</TextSection>
	<TableSection>
		<Widths>
			<Value>100</Value>
			<Value>200</Value>
			<Value>100</Value>
		</Widths>
		<Headers>
			<Line>Record code</Line>
			<Line>Record description</Line>
			<Line>total records</Line>
		</Headers>
		<Rows>
			<Row>
				<Line>A100</Line>
				<Line>Opening Record</Line>
				<Line>1</Line>
			</Row>
			<Row>
				<Line>B100</Line>
				<Line>Transactions in accounting</Line>
				<Line>0</Line>
			</Row>
			<Row>
				<Line>B110</Line>
				<Line>accounts in the file</Line>
				<Line>0</Line>
			</Row>
			<Row>
				<Line>C100</Line>
				<Line>Document title</Line>
				<Line>2</Line>
			</Row>
			<Row>
				<Line>D110</Line>
				<Line>Document details</Line>
				<Line>3</Line>
			</Row>
			<Row>
				<Line>D120</Line>
				<Line>Receipts (ARpayments) details</Line>
				<Line>0</Line>
			</Row>
			<Row>
				<Line>M100</Line>
				<Line>Items in stock</Line>
				<Line>0</Line>
			</Row>
			<Row>
				<Line>Z900</Line>
				<Line>End record</Line>
				<Line>1</Line>
			</Row>
		</Rows>
	</TableSection>
	<TableSection>
		<Widths>
			<Value>100</Value>
			<Value>100</Value>
			<Value>100</Value>
			<Value>100</Value>
		</Widths>
		<Headers>
			<Line>Document type</Line>
			<Line>Description</Line>
			<Line>Amount of documents</Line>
			<Line>Amount in NIS</Line>
		</Headers>
		<Alignments>
			<Alignment>Left</Alignment>
			<Alignment>Left</Alignment>
			<Alignment>Right</Alignment>
			<Alignment>Right</Alignment>
		</Alignments>
		<Rows>
			<Row>
				<Line>305</Line>
				<Line>Tax invoice (ARinvoices)</Line>
				<Line>2</Line>
				<Line>12.07</Line>
			</Row>
			<Row>
				<Line>330</Line>
				<Line>Tax invoice - credit</Line>
				<Line></Line>
				<Line></Line>
			</Row>
			<Row>
				<Line>400</Line>
				<Line>Receipts (ARpayments)</Line>
				<Line>0</Line>
				<Line>0.00</Line>
			</Row>
			<Row>
				<Line>420</Line>
				<Line>Bank deposit</Line>
				<Line>0</Line>
				<Line>0.00</Line>
			</Row>
			<Row>
				<Line>700</Line>
				<Line>Purchase tax invoice (equipment)</Line>
				<Line>0</Line>
				<Line>0.00</Line>
			</Row>
		</Rows>
	</TableSection>
	<TextSection>
		<IsCentered>true</IsCentered>
		<Texts>
			<Line>Produced using software WiseTech Global LTD</Line>
			<Line>Registration certificate number </Line>
			<Line></Line>
			<Line>on Date:20/03/24 at Time: 15:22</Line>
		</Texts>
	</TextSection>
</scheme>";
	}
}
