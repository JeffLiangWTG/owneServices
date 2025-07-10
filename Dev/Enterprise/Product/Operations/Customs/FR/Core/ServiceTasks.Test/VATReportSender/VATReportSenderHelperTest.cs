using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Registry;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.FR.ServiceTasks.Testing
{
	public class VATReportSenderHelperTest : TestCaseWithFactory
	{
		[TestDate(2022, 6, 1)]
		public void TestConvertReportToFileFRVATReportConfigurationIsNull()
		{
			using (Culture.SetTemporarily(System.Globalization.CultureInfo.GetCultureInfo("fr-FR")))
			{
				PrepareTestData();

				var rows = VATReportSenderHelper.GetRecords(GlbCompany.CurrentCompany.PK.ToGuid(), new DateTime(2022, 5, 31), new DateTime(2022, 6, 5));

				using (var file = Enterprise.ZArchitecture.Core.TempFile.NewWithExtension(".xls"))
				{
					var fees = rows.First(x => x.Key == "IMP1");
					VATReportSenderHelper.ConvertReportToFile(fees, file.Filename, GlbCompany.CurrentCompany, Factory);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(file.Filename);
						AssertContains(@"{A}-[JOB NUMBER]   {B}-[DUCR]   {C}-[ENTRY NUMBER]   {D}-[ENTRY REFERENCE]   {E}-[BAE DATE]   {F}-[CPC]   {G}-[COUNTRY OF SUPPLY]   {H}-[INVOICE NUMBER]   {I}-[INVOICE AMOUNT]   {J}-[CURRENCY]   {K}-[RATE]   {L}-[IMPORTER]   {M}-[SUPPLIER]   {N}-[EORI]   {O}-[VAT NUMBER]   {P}-[VAT BASE AMOUNT]   {Q}-[VAT AMOUNT]   {R}-[VAT PROCEDURE]   {S}-[VAT PROCEDURE DESCRIPTION]   {T}-[ORDER REFS]
{A}-[B0002]   {B}-[UCR0002]   {C}-[ENTRYNUM0002]   {D}-[BGM0002]   {E}-[01/06/2022 00:00:00]   {F}-[70P(IMA/7041)]   {G}-[CN/US]   {H}-[INVOICE NO. 1/INVOICE NO. 2]   {I}-[700,0000]   {J}-[USD]   {K}-[1,390000000]   {L}-[IMPORTER 1;TST]   {M}-[SUPPLIER 1]   {N}-[FR001234]   {O}-[004477]   {P}-[240,000000]   {Q}-[24,0000]   {R}-[2]   {S}-[AI2]   {T}-[ORDERREFS2]
{A}-[B0001]   {B}-[UCR0001]   {C}-[ENTRYNUM0001]   {D}-[BGM0001]   {E}-[02/06/2022 00:00:00]   {F}-[   (IMF/    )]   {G}-[US]   {I}-[0,0000]   {J}-[EUR]   {K}-[1,640000000]   {L}-[IMPORTER 1;TST]   {M}-[SUPPLIER 1]   {N}-[FR001234]   {O}-[004477]   {P}-[40,000000]   {Q}-[4,0000]   {R}-[2]   {S}-[AI2]   {T}-[ORDERREFS1,ORDERREFS3]", excelInterface.WorkSheets[0].ToString().ToUpper());
					}
				}

				using (var file = Enterprise.ZArchitecture.Core.TempFile.NewWithExtension(".xls"))
				{
					var fees = Enumerable.Empty<VATReportFee>();
					VATReportSenderHelper.ConvertReportToFile(fees, file.Filename, GlbCompany.CurrentCompany, Factory);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(file.Filename);
						AssertContains(@"{A}-[JOB NUMBER]   {B}-[DUCR]   {C}-[ENTRY NUMBER]   {D}-[ENTRY REFERENCE]   {E}-[BAE DATE]   {F}-[CPC]   {G}-[COUNTRY OF SUPPLY]   {H}-[INVOICE NUMBER]   {I}-[INVOICE AMOUNT]   {J}-[CURRENCY]   {K}-[RATE]   {L}-[IMPORTER]   {M}-[SUPPLIER]   {N}-[EORI]   {O}-[VAT NUMBER]   {P}-[VAT BASE AMOUNT]   {Q}-[VAT AMOUNT]   {R}-[VAT PROCEDURE]   {S}-[VAT PROCEDURE DESCRIPTION]   {T}-[ORDER REFS]", excelInterface.WorkSheets[0].ToString().ToUpper());
					}
				}
			}
		}

		[TestDate(2022, 6, 1)]
		public void TestConvertReportToFile_FRVATReportConfigurationIsNotNull()
		{
			using (Culture.SetTemporarily(System.Globalization.CultureInfo.GetCultureInfo("fr-FR")))
			{
				PrepareTestData();

				var rows = VATReportSenderHelper.GetRecords(GlbCompany.CurrentCompany.PK.ToGuid(), new DateTime(2022, 5, 31), new DateTime(2022, 6, 5));
				Factory.Save();
				var cleanFactory = new BusinessObjectFactory();

				var columnConfigurationsmanager = new ColumnConfigurationsManager(FRCustomsDataRegistry.FRVATReportPK, false);
				var manager = new CombinedConfigurationManager(columnConfigurationsmanager, "AAA");
				var sheet = manager.HeadingManager.CurrentConfiguration.Worksheets.AddNew("AAA", "AAA");
				sheet.ColumnHeadings.Add(new ColumnHeading("Column1", "Job Number", "Heading", 10, 10, 100, false));
				sheet.ColumnHeadings.Add(new ColumnHeading("Column1", "DUCR", "Heading", 10, 10, 100, false));
				sheet.ColumnHeadings.Add(new ColumnHeading("Column1", "BAE Date", "Heading", 10, 10, 100, false));
				manager.Save();

				using (var file = Enterprise.ZArchitecture.Core.TempFile.NewWithExtension(".xls"))
				{
					FRCustomsDataRegistry.Instance.FRVATReportConfiguration.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "AAA");

					var fees = rows.First(x => x.Key == "IMP1");
					VATReportSenderHelper.ConvertReportToFile(fees, file.Filename, GlbCompany.CurrentCompany, cleanFactory);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(file.Filename);
						var x = excelInterface.WorkSheets[0].ToString().ToUpper();
						AssertEquals("If FRVATReportConfiguration is not null, this configuration will be used to generate report", @"{A}-[JOB NUMBER]   {B}-[DUCR]   {C}-[BAE DATE]
{A}-[B0002]   {B}-[UCR0002]   {C}-[01/06/2022 00:00:00]
{A}-[B0001]   {B}-[UCR0001]   {C}-[02/06/2022 00:00:00]", excelInterface.WorkSheets[0].ToString().ToUpper());
					}
				}

				using (var file = TempFile.NewWithExtension(".xls"))
				{
					FRCustomsDataRegistry.Instance.FRVATReportConfiguration.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "AAA");

					var fees = rows.First(x => x.Key == "IMP2");
					VATReportSenderHelper.ConvertReportToFile(fees, file.Filename, GlbCompany.CurrentCompany, cleanFactory);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(file.Filename);
						AssertEquals("If FRVATReportConfiguration is not null, this configuration will be used to generate report", @"{A}-[JOB NUMBER]   {B}-[DUCR]   {C}-[BAE DATE]
{A}-[B0003]   {B}-[UCR0003]   {C}-[01/06/2022 00:00:00]", excelInterface.WorkSheets[0].ToString().ToUpper());
					}
				}

				var expectedHitCounts = new Dictionary<string, int>
				{
					{ StmDataSchema.Constants.TableName, 1 }
				};
				AssertDbHits(expectedHitCounts, cleanFactory);
			}
		}

		void PrepareTestData()
		{
			var (declaration1, entryInstruction1, invoiceHeader1, invoiceLine1, entryHeader1, entryLine1) = CreateDeclarationAtMinimumRequirement("0001", importer1PK, ZDateTimeOffset.Today.AddDays(1));
			entryInstruction1.CEI_SubStyle = "F";
			var orderItem1 = declaration1.DocsAndCartage.OrderItems.AddNew();
			orderItem1.JT_OrderReference = "OrderRefs1";

			var (declaration2, entryInstruction2, invoiceHeader2, invoiceLine2, entryHeader2, entryLine2) = CreateDeclarationAtMinimumRequirement("0002", importer1PK, ZDateTimeOffset.Today);
			invoiceHeader2.SupportingDocuments.AddNew("N380", "Invoice NO. 1");
			entryLine2.CL_InvoiceAmount = 300m;
			entryLine2.CL_RX_NKInvoiceAmountCurrency = "USD";
			invoiceHeader2.SupportingDocuments.AddNew("N325", "Invoice NO. 2");
			var orderItem2 = declaration2.DocsAndCartage.OrderItems.AddNew();
			orderItem2.JT_OrderReference = "OrderRefs2";
			entryInstruction2.CEI_Style = "70P";
			entryInstruction2.CEI_SubStyle = "A";
			invoiceLine2.JI_Procedure = "7041000";
			invoiceLine2.ZG_CountryOfSupply = "US";
			var invoiceLine2B = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2B.ZG_CountryOfSupply = "CN";
			var entryLine2B = entryHeader2.MergedLines.AddNew();
			invoiceLine2B.JI_CEI = entryInstruction2.PK;
			invoiceLine2B.JI_CL = entryLine2B.PK;
			invoiceLine2B.JI_Procedure = "7041000";
			entryLine2B.CL_InvoiceAmount = 400m;
			entryLine2B.CL_RX_NKInvoiceAmountCurrency = "USD";
			var b00B = entryLine2B.ConfirmedFees.AddNew();
			b00B.CF_ChargeType = "B00";
			b00B.CF_BaseValue = 200m;
			b00B.CF_ChargeAmount = 20m;
			b00B.CF_MethodOfPayment = "6";

			var (declaration3, entryInstruction3, invoiceHeader3, invoiceLine3, entryHeader3, entryLine3) = CreateDeclarationAtMinimumRequirement("0003", importer2PK, ZDateTimeOffset.Today);
			var orderItem3 = declaration1.DocsAndCartage.OrderItems.AddNew();
			orderItem3.JT_OrderReference = "OrderRefs3";

			Factory.Save();
		}

		(JobDeclaration declaration, CusEntryInstruction entryInstruction, JobComInvoiceHeader invoiceHeader, JobComInvoiceLine invoiceLine, CusEntryHeader entryHeader, CusEntryLine entryLine) CreateDeclarationAtMinimumRequirement(string reference, Guid importerPK, ZDateTimeOffset dateTimeOffset)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions[0];
			declaration.JE_OH_Importer = importerPK;
			declaration.JE_OH_Supplier = supplier1PK;
			declaration.JE_MessageType = "IMP";
			declaration.JE_MessageSubType = "IM";
			declaration.JE_DeclarationReference = "B" + reference;
			declaration.JE_UCR = "UCR" + reference;
			declaration.ZG_VATDeferType = "2";

			var vatNumberDocument = declaration.SupportingDocuments.AddNew("1008", "VAT NO.");

			var invoiceHeader = declaration.Invoices.AddNew();

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.ZG_CountryOfSupply = "US";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.CH_EntryReleaseDate = dateTimeOffset.ToZDateTime();
			entryHeader.CH_BGMReference = "BGM" + reference;
			entryHeader.EntryNumber = "EntryNum" + reference;

			var entryLine = entryHeader.MergedLines.AddNew();
			var b00 = entryLine.ConfirmedFees.AddNew();
			b00.CF_ChargeType = "B00";
			b00.CF_BaseValue = 40m;
			b00.CF_ChargeAmount = 4m;
			b00.CF_MethodOfPayment = "6";

			invoiceLine.JI_CL = entryLine.PK;

			return (declaration, entryInstruction, invoiceHeader, invoiceLine, entryHeader, entryLine);
		}

		protected override void SetUp()
		{
			var importer1 = Factory.NewWithValidTestData<OrgHeader>();
			importer1.OH_Code = "IMP1";
			importer1.OH_FullName = "Importer 1;TST";
			importer1.CustomsCodes.AddNew("EOR", "001234", "FR");
			importer1.CustomsCodes.AddNew("TVA", "004477", "FR");
			importer1PK = importer1.PK.ToGuid();

			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			supplier1.OH_Code = "SUP1";
			supplier1.OH_FullName = "Supplier 1";
			supplier1PK = supplier1.PK.ToGuid();

			var importer2 = Factory.NewWithValidTestData<OrgHeader>();
			importer2.OH_Code = "IMP2";
			importer2.OH_FullName = "Importer 2";
			importer2.CustomsCodes.AddNew("EOR", "OCCASIONNEL", "FR");
			importer2.CustomsCodes.AddNew("TVA", "004478", "FR");
			importer2PK = importer2.PK.ToGuid();
		}

		Guid importer1PK;
		Guid supplier1PK;
		Guid importer2PK;
	}
}
