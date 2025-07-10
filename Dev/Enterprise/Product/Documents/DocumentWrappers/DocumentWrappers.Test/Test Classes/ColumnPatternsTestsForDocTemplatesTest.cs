using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.BuildTools;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.ExcelTemplates;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class ColumnPatternsTestsForDocTemplatesTest : TestCaseWithFactory
	{
		[DeveloperOnlyTest]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestColumnPatterns()
		{
			string[] templateFiles = Directory.GetFiles(Path.Combine(BuildConstants.LocalEnterprisePath, @"Enterprise\Product\Documents\ExcelTemplates\Documents"));

			Assert("There are no template files", templateFiles.Length > 0);

			StringBuilder columnPatternsResults = new StringBuilder();
			bool hadErrors;

			foreach (string templateFilePath in templateFiles)
			{
				if (!IsNonStandardTemplate(templateFilePath))
				{
					hadErrors = false;
					ExcelTemplate template = new ExcelTemplateReadFromExcelTemplatesSolution(templateFilePath);

					using (ExcelInterface xlInterface = new ExcelInterface())
					{
						Report report = new Report(new DocumentPack(), template, Guid.Empty, Enterprise.Core.Constants.DataContext.None);

						xlInterface.LoadExcelFile(templateFilePath);
						ExcelWorkSheet workSheet = xlInterface.WorkSheets[0];
						xlInterface.Xls.ActiveSheetByName = workSheet.SheetName;
						float widthMultiplier = (float)FlexCel.Core.ExcelMetrics.ColMult(xlInterface.Xls);

						if ((xlInterface.Xls.PrintOptions & FlexCel.Core.TPrintOptions.Orientation) == 0) // landscape
						{
							CheckColumn(workSheet, 1, 5, widthMultiplier, columnPatternsResults, templateFilePath, ref hadErrors);
							CheckColumn(workSheet, 71, 5, widthMultiplier, columnPatternsResults, templateFilePath, ref hadErrors);

							for (int i = 2; i <= 70; i += 2)
							{
								CheckColumn(workSheet, i, 25, widthMultiplier, columnPatternsResults, templateFilePath, ref hadErrors);
							}

							for (int i = 3; i <= 69; i += 2)
							{
								CheckColumn(workSheet, i, 3, widthMultiplier, columnPatternsResults, templateFilePath, ref hadErrors);
							}
						}
						else // portrait
						{
							CheckColumn(workSheet, 1, 5, widthMultiplier, columnPatternsResults, templateFilePath, ref hadErrors);
							CheckColumn(workSheet, 49, 5, widthMultiplier, columnPatternsResults, templateFilePath, ref hadErrors);

							for (int i = 2; i <= 48; i += 2)
							{
								CheckColumn(workSheet, i, 25, widthMultiplier, columnPatternsResults, templateFilePath, ref hadErrors);
							}

							for (int i = 3; i <= 47; i += 2)
							{
								CheckColumn(workSheet, i, 3, widthMultiplier, columnPatternsResults, templateFilePath, ref hadErrors);
							}
						}
					}
				}
			}
			AssertEquals("Should have empty result, but instead was:\r\n" + columnPatternsResults.ToString(), 0, columnPatternsResults);
		}

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "File name")]
		public List<string> NonStandardTemplates
		{
			get
			{
				if (fNonStandardTemplates == null)
				{
					fNonStandardTemplates = new List<string>();
					fNonStandardTemplates.Add("AccountingVoucher.xls");
					fNonStandardTemplates.Add("Air Freight Worksheet.xls");
					fNonStandardTemplates.Add("AUCustomsEntryPrintLandscape.xls");
					fNonStandardTemplates.Add("AUCustomsEntryPrintPortrait.xls");
					fNonStandardTemplates.Add("Authority To Make Entry.xls");
					fNonStandardTemplates.Add("Authorization For Release.xls");
					fNonStandardTemplates.Add("Bill Of Lading CargoWise.xls");
					fNonStandardTemplates.Add("Bill Of Lading Datahawk.xls");
					fNonStandardTemplates.Add("Bill Of Lading Enhanced CargoWise.xls");
					fNonStandardTemplates.Add("Bill Of Lading FIATA.xls");
					fNonStandardTemplates.Add("Bill Of Lading ITC Fiji.xls");
					fNonStandardTemplates.Add("Bill Of Lading ITC NZ.xls");
					fNonStandardTemplates.Add("Bill Of Lading ITC.xls");
					fNonStandardTemplates.Add("Bill Of Lading TAN.xls");
					fNonStandardTemplates.Add("Bill Of Lading TTC US(Letter).xls");
					fNonStandardTemplates.Add("Bill Of Lading TTC.xls");
					fNonStandardTemplates.Add("Cargo Dues Order.xls");
					fNonStandardTemplates.Add("CFS Carters Note AKL NZ.xls");
					fNonStandardTemplates.Add("CFS Export Label.xls");
					fNonStandardTemplates.Add("CFS Import Label.xls");
					fNonStandardTemplates.Add("CFS NZ Dangerous Goods Packing Certificate.xls");
					fNonStandardTemplates.Add("CFS Transhipment Label.xls");
					fNonStandardTemplates.Add("Cheque Template Standard.xls");
					fNonStandardTemplates.Add("Cheque Template HKHSBC.xls");
					fNonStandardTemplates.Add("Cheque Template Singapore.xls");
					fNonStandardTemplates.Add("Cheque Template UKStandard.xls");
					fNonStandardTemplates.Add("Cheque Template USStandard Middle.xls");
					fNonStandardTemplates.Add("Cheque Template USStandard Top.xls");
					fNonStandardTemplates.Add("Cheque Template USStandard.xls");
					fNonStandardTemplates.Add("Cheque Template CanadaStandard Middle.xls");
					fNonStandardTemplates.Add("Class A Invoice Preprinted.xls");
					fNonStandardTemplates.Add("Class A Invoice.xls");
					fNonStandardTemplates.Add("Consultant Review Document.xls");
					fNonStandardTemplates.Add("CostDisection.xls");
					fNonStandardTemplates.Add("Delivery Labels (label printer).xls");
					fNonStandardTemplates.Add("Delivery Labels (side by side).xls");
					fNonStandardTemplates.Add("Dock Receipt Pre Printed.xls");
					fNonStandardTemplates.Add("Dock Receipt.xls");
					fNonStandardTemplates.Add("Export Receival Advice.xls");
					fNonStandardTemplates.Add("Forwarder Ceritificate of Receipt.xls");
					fNonStandardTemplates.Add("Freight Label.xls");
					fNonStandardTemplates.Add("Gate Pass Shipment Singapore.xls");
					fNonStandardTemplates.Add("HAWB Barcode Label 5 Inch.xls");
					fNonStandardTemplates.Add("Hazardous Cargo Label.xls");
					fNonStandardTemplates.Add("IMO Form.xls");
					fNonStandardTemplates.Add("Import Cargo Label.xls");
					fNonStandardTemplates.Add("LandedCosting.xls");
					fNonStandardTemplates.Add("LCEntryDetails.xls");
					fNonStandardTemplates.Add("MY Inward Manifest Master Page.xls");
					fNonStandardTemplates.Add("MY Inward Manifest Shipment List.xls");
					fNonStandardTemplates.Add("NorthPort Container Shipping Note.xls");
					fNonStandardTemplates.Add("NorthPort Delivery Order Follow On Page.xls");
					fNonStandardTemplates.Add("NorthPort Delivery Order.xls");
					fNonStandardTemplates.Add("NorthPort PKDP Delivery Order.xls");
					fNonStandardTemplates.Add("Ocean Bill Of Lading Place Holder.xls");
					fNonStandardTemplates.Add("On Site Time Card.xls");
					fNonStandardTemplates.Add("Penang Delivery Order.xls");
					fNonStandardTemplates.Add("Port Carters Note.xls");
					fNonStandardTemplates.Add("Receipt Matching.xls");
					fNonStandardTemplates.Add("Seaway Booking Confirmation.xls");
					fNonStandardTemplates.Add("Shi Lian Dan.xls");
					fNonStandardTemplates.Add("Short Form Bill Of Lading.xls");
					fNonStandardTemplates.Add("Standard Landed Costing.xls");
					fNonStandardTemplates.Add("Statement Of Account.xls");
					fNonStandardTemplates.Add("Training Course Acceptance Page.xls");
					fNonStandardTemplates.Add("Training Course Schedule One Off.xls");
					fNonStandardTemplates.Add("Training Course Schedule.xls");
					fNonStandardTemplates.Add("Traxon AWB_ForEpsonPrinters1.xls");
					fNonStandardTemplates.Add("Traxon AWB_ForEpsonPrinters2.xls");
					fNonStandardTemplates.Add("Universal Laser MAWB.xls");
					fNonStandardTemplates.Add("Universal Neutral MAWB.xls");
					fNonStandardTemplates.Add("VN ARInvoice.xls");
					fNonStandardTemplates.Add("Warehouse Delivery Label.xls");
					fNonStandardTemplates.Add("Warehouse Neutral Straight Bill Of Lading.xls");
					fNonStandardTemplates.Add("Warehouse Invoice Detail.xls");
					fNonStandardTemplates.Add("Warehouse Inwards Job History With Charges.xls");
					fNonStandardTemplates.Add("Warehouse Orders Job History With Charges.xls");
					fNonStandardTemplates.Add("Warehouse Package Label.xls");
					fNonStandardTemplates.Add("Warehouse Pallet Label.xls");
					fNonStandardTemplates.Add("WestPort Delivery Order.xls");
					fNonStandardTemplates.Add("WorkSheet -Old.xls");
					fNonStandardTemplates.Add("Warehouse Location Label.xls");
					fNonStandardTemplates.Add("Warehouse Job Pallet Label.xls");
				}
				return fNonStandardTemplates;
			}
		}
		List<string> fNonStandardTemplates;

		bool IsNonStandardTemplate(string templateFilePath)
		{
			string[] splitFilePath = templateFilePath.Split('\\');
			string fileName = splitFilePath[splitFilePath.Length - 1];

			return NonStandardTemplates.Contains(fileName);
		}

		void CheckColumn(ExcelWorkSheet workSheet, int columnNumber, int expectedWidth, float widthMultiplier, StringBuilder columnPatternsResults, string templateFilePath, ref bool hadErrors)
		{
			int actualWidth = (int)(workSheet.GetColWidth(columnNumber) / widthMultiplier) + 1;
			if (expectedWidth != actualWidth)
			{
				if (!hadErrors)
				{
					columnPatternsResults.AppendLine(String.Format("Template {0} has the following errors:", templateFilePath));
					hadErrors = true;
				}
				columnPatternsResults.AppendLine(String.Format("        Column {0} should be {1} pixels wide but is {2} pixels wide.", GetExcelColumnName(columnNumber), expectedWidth, actualWidth));
			}
		}

		string GetExcelColumnName(int columnNumber)
		{
			string result = String.Empty;
			if (columnNumber / 26 > 0)
			{
				result = ((IConvertible)(columnNumber / 26 + 64)).ToChar(System.Globalization.CultureInfo.CurrentCulture).ToString();
			}
			result += ((IConvertible)(columnNumber % 26 + 65)).ToChar(System.Globalization.CultureInfo.CurrentCulture).ToString();
			return result;
		}

		#endregion
	}
}
