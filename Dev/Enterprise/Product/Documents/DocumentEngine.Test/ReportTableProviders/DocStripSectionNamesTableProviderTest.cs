using System;
using System.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.ReportTableProviders.Testing
{
	sealed class DocStripSectionNamesTableProviderTest : TestCaseWithFactory
	{
		public void TestHandlesSortInternally()
		{
			AssertEquals("HandlesSortInternally", false, Provider.HandlesSortInternally);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetDataTable()
		{
			SetUpData();

			using (Report report = GetNewReport())
			using (DataTable table = Provider.GetDataTable("TableName", null, report, false))
			{
				// This provider doesn't handle sort internally. Let's sort it for test purpose
				DataTable sortedTable;
				DataRow[] rows = table.Select("", "SectionName, TemplateName");
				sortedTable = table.Clone();
				foreach (DataRow row in rows)
				{
					sortedTable.ImportRow(row);
				}

				AssertEquals(9, sortedTable.Rows.Count);
				AssertRow(sortedTable.Rows[0], "System Document Elements", "GEN", "Invoice", "Invoice Local Currency Tax Grand Total", "#ConfigurableSection:GEN:Invoice, Invoice Local Currency Tax Grand Total");
				AssertRow(sortedTable.Rows[1], "System Document Elements", "GEN", "Invoice", "Invoice Tax Messages (English)", "#ConfigurableSection:GEN:Invoice, Invoice Tax Messages (English)");
				AssertRow(sortedTable.Rows[2], "System Document Elements", "GEN", "Invoice", "Invoice Tax Messages (Localized)", "#ConfigurableSection:GEN:Invoice, Invoice Tax Messages (Localized)");
				AssertRow(sortedTable.Rows[3], "System Document Elements", "GEN", "Invoice", "Invoice Tax Total Summary By Rate", "#ConfigurableSection:GEN:Invoice, Invoice Tax Total Summary By Rate");
				AssertRow(sortedTable.Rows[4], "System Document Elements", "GEN", "Invoice", "Invoice Totals Section and Invoice Message", "#ConfigurableSection:GEN:Invoice, Invoice Totals Section and Invoice Message");
				AssertRow(sortedTable.Rows[5], "Customized Document Elements", "GEN", "Warehouse PickingSlip", "PageTotals + Running Totals (Cust)", "#ConfigurableSection:GEN:Warehouse PickingSlip, PageTotals + Running Totals (Cust)");
				AssertRow(sortedTable.Rows[6], "Customized Document Elements [FR-FR]", "GEN", "Warehouse PickingSlip", "PageTotals + Running Totals [FR]", "#ConfigurableSection:GEN:Warehouse PickingSlip, PageTotals + Running Totals [FR]");
				AssertRow(sortedTable.Rows[7], "Customized Document Elements", "GEN", "Warehouse Cartage Advice", "Portrait Docket Info - Shipment (Cust)", "#ConfigurableSection:GEN:Warehouse Cartage Advice, Portrait Docket Info - Shipment (Cust)");
				AssertRow(sortedTable.Rows[8], "Customized Document Elements [FR-FR]", "GEN", "Warehouse Cartage Advice", "Portrait Docket Info - Shipment [FR]", "#ConfigurableSection:GEN:Warehouse Cartage Advice, Portrait Docket Info - Shipment [FR]");
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetDataTableWithMaximumRowNumber()
		{
			SetUpData();

			using (Report report = GetNewReport())
			using (DataTable table = Provider.GetDataTable("TableName", null, report, false, 5))
			{
				// This provider doesn't handle sort internally. Let's sort it for test purpose
				DataTable sortedTable;
				DataRow[] rows = table.Select("", "SectionName, TemplateName");
				sortedTable = table.Clone();
				foreach (DataRow row in rows)
				{
					sortedTable.ImportRow(row);
				}

				AssertEquals(5, sortedTable.Rows.Count);
				AssertRow(sortedTable.Rows[0], "System Document Elements", "GEN", "Invoice", "Invoice Totals Section and Invoice Message", "#ConfigurableSection:GEN:Invoice, Invoice Totals Section and Invoice Message");
				AssertRow(sortedTable.Rows[1], "Customized Document Elements", "GEN", "Warehouse PickingSlip", "PageTotals + Running Totals (Cust)", "#ConfigurableSection:GEN:Warehouse PickingSlip, PageTotals + Running Totals (Cust)");
				AssertRow(sortedTable.Rows[2], "Customized Document Elements [FR-FR]", "GEN", "Warehouse PickingSlip", "PageTotals + Running Totals [FR]", "#ConfigurableSection:GEN:Warehouse PickingSlip, PageTotals + Running Totals [FR]");
				AssertRow(sortedTable.Rows[3], "Customized Document Elements", "GEN", "Warehouse Cartage Advice", "Portrait Docket Info - Shipment (Cust)", "#ConfigurableSection:GEN:Warehouse Cartage Advice, Portrait Docket Info - Shipment (Cust)");
				AssertRow(sortedTable.Rows[4], "Customized Document Elements [FR-FR]", "GEN", "Warehouse Cartage Advice", "Portrait Docket Info - Shipment [FR]", "#ConfigurableSection:GEN:Warehouse Cartage Advice, Portrait Docket Info - Shipment [FR]");
			}
		}

		void AssertRow(DataRow row, string templateName, string sectionType, string sectionCategory, string sectionName, string fullLabel)
		{
			AssertEquals("Template Name", templateName, row[0].ToString());
			AssertEquals("Section Type", sectionType, row[1].ToString());
			AssertEquals("Section Category", sectionCategory, row[2].ToString());
			AssertEquals("Section Name", sectionName, row[3].ToString());
			AssertEquals("Full Label in Template", fullLabel, row[4].ToString());
		}

		Report GetNewReport()
		{
			return new Report(null, null, Guid.Empty, Enterprise.Core.Constants.DataContext.None);
		}

		void SetUpData()
		{
			TestCaseHelper.ClearTable(StmMenuDocumentConfigItemSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmMenuDocumentConfigSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmMenuTemplatePivotSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmTemplateSchema.Constants.TableName);

			GetTemplate("System Document Elements.xls", "System Document Elements", true);
			GetTemplate("Customized Document Elements.xls", "Customized Document Elements", false);
			GetTemplate("Customized Document Elements [FR-FR].xls", "Customized Document Elements [FR-FR]", false);
			GetTemplate("Customized Document Elements - IShouldBeIgnored.xls", "Customized Document Elements - IShouldBeIgnored", false);

			Factory.Save();
		}

		StmTemplateBase GetTemplate(string excelTemplateName, string templateName, bool isSystemDefined)
		{
			var excelTemplate = new ExcelTemplateForUnitTesting(excelTemplateName, TestFilesSubFolder.ReportTestFiles);
			byte[] templateBytes = excelTemplate.GetAsByteArray();
			var template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = nameof(Core.Constants.DataContext.Shipment);
			template.SO_IsSystemDefined = isSystemDefined;
			template.SO_Name = templateName;
			template.SO_Template = new ZBlob(templateBytes);
			return template;
		}

		DocStripSectionNamesTableProvider Provider
		{
			get { return provider ?? (provider = new DocStripSectionNamesTableProvider()); }
		}
		DocStripSectionNamesTableProvider provider;
	}
}
