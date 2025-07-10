using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	sealed class FormatNumberUtilTest : TestCaseWithFactory
	{
		public void TestFormatAmountWithCurrentCompanysCulture()
		{
			RefCurrency uSD = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			DocCurrency docUSD = DocCurrency.New(uSD, Factory);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("1,234.56", FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(1234.56, docUSD));
			}
			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				AssertEquals("1.234,56", FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(1234.56, docUSD));
			}
		}

		public void TestFormatRateWithCurrentCompanysCulture()
		{
			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("1,234.56%", FormatNumberUtil.FormatRateWithCurrentCompanysCulture(1234.56));
			}
			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				AssertEquals("1.234,56%", FormatNumberUtil.FormatRateWithCurrentCompanysCulture(1234.56));
			}
		}

		public void TestDocumentEngineRenderCurrencyFormattedMacro()
		{
			var invoicingBase = Factory.New<ARInvoice>();
			var aRInvoiceWrapper = DocARInvoice.New(invoicingBase, Factory);
			var line = (InvoicingLineBase)invoicingBase.Lines.AddNew();
			var testObjectCreator = new TestObjectCreator(Factory);
			line.AL_AG = testObjectCreator.GLHeader1.PK;
			line.AL_OSExTaxAmount = 123456.00M;
			Factory.Save();
			AssertEquals("InvoiceSubTotalFormatted", "123,456.00", aRInvoiceWrapper.InvoiceSubTotalFormatted);

			var originalDecimalSeparator = DataRegistry.Instance.CurrencyDecimalSeparator;
			var originalGroupSeparator = DataRegistry.Instance.CurrencyGroupSeparator;
			var originalGroupSizes = DataRegistry.Instance.CurrencyGroupSizes;
			DataRegistry.Instance.CurrencyDecimalSeparator = ",,";
			DataRegistry.Instance.CurrencyGroupSeparator = "..";
			DataRegistry.Instance.CurrencyGroupSizes = "2";

			try
			{
				using (var templateStream = new MemoryStream())
				{
					using (var creationExcelInterface = new ExcelInterface())
					{
						creationExcelInterface.NewExcelFile(1);
						var workSheet = creationExcelInterface.WorkSheets[0];
						workSheet[0, 0] = "#config";
						workSheet[1, 0] = "Name=TemplateFromStream";
						workSheet[2, 0] = "PageStyle=Portrait";
						workSheet[3, 0] = "#SectionBody";
						workSheet[4, 1] = "<InvoiceSubTotalFormatted>";
						workSheet[5, 0] = "#EndOfReport";
						creationExcelInterface.SaveToStream(templateStream);
					}

					var stmMenuItem = Factory.New<DocumentCommand>();
					var pack = new DocumentPack(stmMenuItem);

					var excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
					using (var report = new Report(pack, excelTemplate, BODocDataProvider.Get(aRInvoiceWrapper), "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
					using (var outputStream = new MemoryStream())
					{
						report.Save(outputStream);
						using (var excelInterface = new ExcelInterface())
						using (Culture.SetTemporarily(Culture.CurrentCompanyCountryCulture))
						{
							excelInterface.LoadExcelFile(outputStream);
							var workSheet = excelInterface.WorkSheets[0];
							AssertMultilineASCIIEquals("Generated Results", @"
{B}-[12..34..56,,00]
".Trim(), workSheet.ToString(new CellFormatterExposingFormulae()));
						}
					}
				}
			}
			finally
			{
				DataRegistry.Instance.CurrencyDecimalSeparator = originalDecimalSeparator;
				DataRegistry.Instance.CurrencyGroupSeparator = originalGroupSeparator;
				DataRegistry.Instance.CurrencyGroupSizes = originalGroupSizes;
			}
		}
	}
}
