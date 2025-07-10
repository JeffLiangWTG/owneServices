using System.Collections.Generic;
using System.IO;
using CargoWise.BuildTools;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using FlexCel.Core;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class MarginTestsForDocTemplatesTest : TestCaseWithFactory
	{
		const double MarginUnitLength = 0.3937007874;

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMarginsAreCorrect()
		{
			var templateFiles = Directory.GetFiles(Path.Combine(BuildConstants.LocalEnterprisePath, @"Enterprise\Product\Documents\ExcelTemplates\Documents"));
			Assert("There are no template files", templateFiles.Length > 0);

			using (var templateFile = new ExcelInterface())
			{
				var brokenTemplates = new SortedDictionary<string, IList<string>>();
				var errors = new List<string>();

				foreach (var templateFilePath in templateFiles)
				{
					if (IsAStandardTemplateAndNotExempt(templateFilePath))
					{
						templateFile.LoadExcelFile(templateFilePath);
						var xls = templateFile.Xls;
						for (var sheet = 1; sheet <= xls.SheetCount; sheet++)
						{
							xls.ActiveSheet = sheet;
							if (xls.SheetVisible == TXlsSheetVisible.Visible && DocumentEngine.Report.IsTemplateSheet(xls.SheetName))
							{
								if (templateFile.GetTopMargin() < MarginUnitLength)
								{
									errors.Add("Top margin should be atleast 1");
								}

								if (templateFile.GetBottomMargin() < MarginUnitLength)
								{
									errors.Add("Bottom margin should be atleast 1");
								}

								if (templateFile.GetLeftMargin() < MarginUnitLength)
								{
									errors.Add("Left margin should be atleast 1");
								}

								if (templateFile.GetRightMargin() < MarginUnitLength)
								{
									errors.Add("Right margin should be atleast 1");
								}
							}
						}
					}

					if (errors.Count > 0)
					{
						brokenTemplates.Add(templateFilePath, errors);
						errors = new List<string>();
					}
				}

				AssertGroupedErrorList("The following templates have errors", brokenTemplates);
			}
		}

		ZBool IsAStandardTemplateAndNotExempt(ZString templateFilePath)
		{
			ZString fileName = Path.GetFileName(templateFilePath);

			return
					!templateFilePath.EndsWith(".scc") &&
					!fileName.StartsWith("Bill Of Lading") &&
					!fileName.Contains("AWB") &&
					!fileName.StartsWith("CFS Export Label") &&
					!fileName.StartsWith("CFS Import Label") &&
					!fileName.StartsWith("CFS Transhipment Label") &&
					!fileName.StartsWith("Delivery Labels") &&
					!fileName.StartsWith("Standard Landed Costing") &&
					!fileName.StartsWith("Barcode Cover Sheet") &&
					!fileName.StartsWith("Quotation Terms And Conditions") &&
					!fileName.StartsWith("AUCustomsEntryPrint") &&
					!fileName.StartsWith("Cheque02") &&
					!fileName.StartsWith("Cheque03") &&
					!fileName.StartsWith("Cheque04") &&
					!fileName.StartsWith("Cheque Template Fracht") &&
					!fileName.StartsWith("Cheque Template USStandard Bottom") &&
					!fileName.StartsWith("Cheque Template USStandard MICR") &&
					!fileName.StartsWith("Hazardous Cargo Label") &&
					!fileName.StartsWith("Freight Label") &&
					!fileName.StartsWith("Domestic Freight Label") &&
					!fileName.StartsWith("Import Cargo Label") &&
					!fileName.StartsWith("VAT Register Report.xls") &&
					!fileName.StartsWith("Warehouse Delivery Label.xls") &&
					!fileName.StartsWith("Warehouse Package Label.xls") &&
					!fileName.StartsWith("Warehouse Package Label for BOM") &&
					!fileName.StartsWith("Warehouse Pallet Label.xls") &&
					!fileName.StartsWith("Warehouse Neutral Straight Bill Of Lading.xls") &&
					!fileName.StartsWith("Short Form Bill Of Lading.xls") &&
					!fileName.StartsWith("AUCustoms Authority To Deal") &&
					!fileName.StartsWith("Carrier Booking Request") &&
					!fileName.StartsWith("Cargo Inspection Request") &&
					!fileName.StartsWith("VN ARInvoice") &&
					!fileName.StartsWith("Organisation US IRS 1096 Form") &&
					!fileName.StartsWith("Warehouse Location Label.xls") &&
					!fileName.StartsWith("Warehouse Job Pallet Label.xls") &&
					!fileName.StartsWith("Cash Register.xls") &&
					!fileName.StartsWith("Electronic Credit Note Thermal Paper.xls") &&
					!fileName.StartsWith("Electronic GUI Thermal Paper.xls") &&
					!fileName.StartsWith("Cheque Template Singapore.xls") &&
					!fileName.StartsWith("Class A Invoice Preprinted.xls") &&
					!fileName.StartsWith("Class A Invoice.xls") &&
					!fileName.StartsWith("Dock Receipt Pre Printed.xls") &&
					!fileName.StartsWith("Dock Receipt.xls") &&
					!fileName.StartsWith("EUR.1 - Icelandic.xls") &&
					!fileName.StartsWith("IMO Form.xls") &&
					!fileName.StartsWith("NorthPort Container Shipping Note.xls") &&
					!fileName.StartsWith("NorthPort Delivery Order.xls") &&
					!fileName.StartsWith("NorthPort PKDP Delivery Order.xls") &&
					!fileName.StartsWith("Ocean Bill Of Lading CargoWise.xls") &&
					!fileName.StartsWith("Shi Lian Dan.xls") &&
					!fileName.StartsWith("WestPort Delivery Order.xls") &&
					!fileName.StartsWith("Container Yard EIR In.xls") &&
					!fileName.StartsWith("Container Yard EIR Out.xls");
		}
	}
}
