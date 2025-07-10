using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.GUI.Testing
{
	sealed class ImportInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestAdditionalInfoPanelLayout()
		{
			using (var control = new ImportInvoiceLineUserControlForTest())
			{
				var layout = control.GetAdditionalInfoPanelLayout_Exposed().Layout;
				var expectedLayout = ((IPanelLayoutProvider)new ImportAdditionalInfoLayout()).Layout;
				AssertContainsExactElementsInAnyOrder("IncludedControls", layout.IncludedControls, expectedLayout.IncludedControls);
			}
		}

		public void TestColumnJI_StatisticalValue()
		{
			using (var control = new ImportInvoiceLineUserControlForTest())
			{
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();
				AssertNull(control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.ZG_StatisticalValue));
			}
		}

		public void TestInwardProcessingTabPage_TabVisible()
		{
			AssertEntryInstruction_CEI_StyleInfo_ValueChanged(ImportDeclarationTypeList.Codes.EAV, (control) =>
			{
				var tabPage = (ZTabPage)(typeof(ImportInvoiceLineUserControl).GetField("InwardProcessingTabPage", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(control));
				AssertEquals("Inward Processing is enabled", true, tabPage.TabVisible);
			});

			AssertEntryInstruction_CEI_StyleInfo_ValueChanged(ImportDeclarationTypeList.Codes.AAV, (control) =>
			{
				var tabPage = (ZTabPage)(typeof(ImportInvoiceLineUserControl).GetField("InwardProcessingTabPage", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(control));
				AssertEquals("Inward Processing is disabled", false, tabPage.TabVisible);
			});
		}

		public void TestBondedWhsQuantityCalcDropEdit()
		{
			using (var form = new ZForm())
			using (var control = new ImportInvoiceLineUserControlForTest())
			{
				form.SetDataBinding(declaration, ZString.Empty);
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();
				var calcDropEdit = control.FindSingle<ZCalcDropEdit>("BondedWhsQuantityCalcDropEdit");
				AssertEquals(3, calcDropEdit.UnitPreBoundMaxLength);
			}
		}

		public void TestBondedWhs_Cleared()
		{
			AssertEntryInstruction_CEI_StyleInfo_ValueChanged(ImportDeclarationTypeList.Codes.AZ, (control) =>
			{
				CombineAssertions(() =>
				{
					AssertEquals("JI_BondedWhsQuantity cleared", ZDecimal.Zero, invoiceLine.JI_BondedWhsQuantity);
					AssertEquals("JI_BondedWhsUnitQty cleared", ZString.Empty, invoiceLine.JI_BondedWhsUnitQty);
				});
			});
		}

		public void TestColumnTypes()
		{
			using (var uc = new ImportInvoiceLineUserControlForTest())
			{
				uc.JobDeclaration = declaration;
				uc.InitializeGridLayout();
				var map = new Dictionary<string, Type>();
				map.Add("JI_NetPrice", typeof(ZCalcEditColumnStyleInfo));
				map.Add("JI_RX_NKNetPriceCurr", typeof(ZCodeFindBoxColumnStyleInfo));

				foreach (var colName in map.Keys)
				{
					var columnStyle = (from ZGridColumnInfo col in uc.CustomsInvoiceLinesBoundGrid.ColumnStyles.ToArray() where col.ColumnName == colName select col).FirstOrDefault();
					Type t = map[colName];
					AssertType(string.Format("Column of name {0} should have a style of type {1}", colName, t.Name), t, columnStyle);
				}
			}
		}

		public void TestFieldVisible()
		{
			using (var form = new ZForm())
			using (var control = new ImportInvoiceLineUserControlForTest())
			{
				form.SetDataBinding(declaration, ZString.Empty);
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals(false, control.ValuationMethodDropEdit.Visible);
					AssertEquals(false, control.ValuationAdjustmentCodeDropEdit.Visible);
					AssertEquals(false, control.ValuationAdjustmentPercentageCalcEdit.Visible);
				});
			}
		}

		public void TestColumnJI_CustomsQuantity()
		{
			using (var form = new ZForm())
			using (var control = new ImportInvoiceLineUserControlForTest())
			{
				form.SetDataBinding(declaration, ZString.Empty);
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals("JI_CustomsQuantity GroupName Caption", "[38] Net Mass Measure", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsQuantity).GroupName.Caption);
					AssertEquals(150, control.CustomsInvoiceLinesBoundGrid.GetColumnWidth(JobComInvoiceLine.Schema.JI_CustomsQuantity));
				});
			}
		}

		public void TestColumnJI_CustomsUnitQtyGroupName()
		{
			using (var form = new ZForm())
			using (var control = new ImportInvoiceLineUserControlForTest())
			{
				form.SetDataBinding(declaration, ZString.Empty);
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();
				form.Show();

				AssertEquals("JI_CustomsUnitQty GroupName Caption", "[38] Net Mass Measure", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsUnitQty).GroupName.Caption);
			}
		}

		public void TestColumnNetPriceGroupName()
		{
			using (var form = new ZForm())
			using (var control = new ImportInvoiceLineUserControlForTest())
			{
				form.SetDataBinding(declaration, ZString.Empty);
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals("JI_NetPrice GroupName Caption", "Net Price", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NetPrice).GroupName.Caption);
					AssertEquals("JI_RX_NKNetPriceCurr GroupName Caption", "Net Price", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_RX_NKNetPriceCurr).GroupName.Caption);
				});
			}
		}

		public void TestVatDropDownInvisible()
		{
			using (var form = new ZForm())
			using (var control = new ImportInvoiceLineUserControlForTest())
			{
				form.SetDataBinding(declaration, ZString.Empty);
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();
				AssertEquals("VAT Dropdown invisible", false, control.VatTypeDropEdit.Visible);
			}
		}

		public void TestColumnJI_Description()
		{
			using (var control = new ImportInvoiceLineUserControlForTest())
			{
				var descColumn = control.CustomsInvoiceLinesBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == JobComInvoiceLine.Schema.JI_Description);
				AssertEquals(System.Windows.Forms.CharacterCasing.Normal, descColumn.CharacterCasing);
			}
		}

		public void TestColumnJI_ZZF_NKTaxTypeNotPresent()
		{
			using (var form = new ZForm())
			using (var control = new ImportInvoiceLineUserControlForTest())
			{
				form.SetDataBinding(declaration, ZString.Empty);
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();

				var grid = control.FindSingle<ZGrid>("CustomsInvoiceLinesBoundGrid");
				AssertEquals(false, grid.Columns.Contains(nameof(JobComInvoiceLine.JI_ZZF_NKTaxType)));
			}
		}

		public void TestAdditionalTariffsGrid()
		{
			(string ColumnName, string ColumnCaption, int columnWidth)[] expectedColumns = new[]
			{
				(CusLineTariffDetailSchema.Constants.BZ_Type, "Part", 43),
				(CusLineTariffDetailSchema.Constants.BZ_Tariff, "Code", 86),
				(CusLineTariffDetailSchema.Constants.BZ_Qty1, "Quantity", 80),
				(CusLineTariffDetailSchema.Constants.BZ_UQ1, "UOM", 46),
				(nameof(CusLineTariffDetail.BZ_PercentAlcohol), "Degree Percentage", 119),
				(nameof(CusLineTariffDetail.Schema.BZ_TobaccoRetailPrice), "Retail Price", 95),
				(nameof(CusLineTariffDetail.ExciseValue), "Excise Value", 95)
			};

			using (var form = new ZForm())
			using (var control = new ImportInvoiceLineUserControlForTest())
			{
				form.SetDataBinding(declaration, ZString.Empty);
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();
				var grid = control.FindSingle<ZGrid>("AdditionalTariffsGrid");

				CombineAssertions(() =>
				{
					foreach (var (columnName, columnCaption, columnWidth) in expectedColumns)
					{
						var column = grid.Columns[columnName].ColumnStyle;
						AssertEquals($"{columnName} caption", columnCaption, column.HeaderText);
						AssertEquals($"{columnName} width", columnWidth, column.Width);
					}
				});
			}
		}

		public void TestInvoiceLineDetailsPanelLayout()
		{
			using (var control = new ImportInvoiceLineUserControlForTest())
			{
				var layout = control.GetNewInvoiceLineDetailsPanelLayout_Exposed().Layout;
				var importLayout = ((IPanelLayoutProvider)new ImportInvoiceLineDetailsLayout()).Layout;
				AssertContainsExactElementsInAnyOrder("IncludedControls", layout.IncludedControls, importLayout.IncludedControls);
			}
		}

		public void TestDynamicLayoutApplied()
		{
			using (var control = new ImportInvoiceLineUserControlForTest())
			{
				AssertEquals(true, control.DynamicLayoutApplied_Exposed);
			}
		}

		public void TestHasDifferentPanelLayout()
		{
			using (var control = new ImportInvoiceLineUserControlForTest())
			{
				AssertEquals(true, control.HasDifferentPanelLayout_Exposed);
			}
		}

		public void TestBottomPanel_MinimumHeight()
		{
			using (var control = new ImportInvoiceLineUserControlForTest())
			{
				var bottomPanel = control.FindSingle<ZPanel>("BottomPanel");
				AssertEquals(462, bottomPanel.MinimumSize.Height);
			}
		}

		public void TestTabPagesVisible_IPR_AVABR()
		{
			using (var form = new ZForm(declaration))
			using (var control = new ImportInvoiceLineUserControlForTest())
			{
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
				invoiceLine.JI_CEI = instruction.PK;

				form.SetDataBinding(declaration, ZString.Empty);
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();

				var lineDetailTabControl = control.FindSingle<ZTabControl>("LineDetailTabControl");
				var lineChargesTabPage = (ZTabPage)lineDetailTabControl.AllTabPages.Single(x => x.Name == "LineChargesTabPage");
				AssertEquals("LineChargesTabPage is not visible", false, lineChargesTabPage.TabVisible);

				var packagesPivotTabPage = (ZTabPage)lineDetailTabControl.AllTabPages.Single(x => x.Name == "PackagesPivotTabPage");
				AssertEquals("PackagesPivotTabPage is not visible", false, packagesPivotTabPage.TabVisible);

				var customFieldsTabPage = (ZTabPage)lineDetailTabControl.AllTabPages.Single(x => x.Name == "CustomFieldsTabPage");
				AssertEquals("CustomFieldsTabPage is not visible", false, customFieldsTabPage.TabVisible);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		}
		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;

		void AssertEntryInstruction_CEI_StyleInfo_ValueChanged(ZString ceiStype, Action<ImportInvoiceLineUserControlForTest> assertAction)
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.EAV;
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_BondedWhsQuantity = 100m;
			invoiceLine.JI_BondedWhsUnitQty = Core.Constants.Weight.Kilograms;

			using (var form = new ZForm())
			using (var control = new ImportInvoiceLineUserControlForTest())
			{
				form.SetDataBinding(declaration, ZString.Empty);
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();
				instruction.CEI_Style = ceiStype;
				assertAction(control);
			}
		}
	}

	class ImportInvoiceLineUserControlForTest : ImportInvoiceLineUserControl
	{
		public new ZDropEdit ValuationMethodDropEdit => base.ValuationMethodDropEdit;

		public new ZDropEdit ValuationAdjustmentCodeDropEdit => base.ValuationAdjustmentCodeDropEdit;

		public new ZCalcEdit ValuationAdjustmentPercentageCalcEdit => base.ValuationAdjustmentPercentageCalcEdit;

		public new ZDropEdit VatTypeDropEdit => base.VatTypeDropEdit;

		public ZBool DynamicLayoutApplied_Exposed => base.DynamicLayoutApplied;

		public ZBool HasDifferentPanelLayout_Exposed => base.HasDifferentPanelLayout;

		public IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout_Exposed() => base.GetNewInvoiceLineDetailsPanelLayout();

		public IPanelLayoutProvider GetAdditionalInfoPanelLayout_Exposed() => base.GetAdditionalInfoPanelLayout();
	}
}
