using System.Linq;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.EU.GUI.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using InvoiceCharge = Enterprise.Customs.EU.Business.Declaration.InvoiceCharge;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(ExportInvoiceLineUserControl))]
	class ExportInvoiceLineUserControlTest : BaseInvoiceLineUserControlForVirtualPropertiesTest<ExportInvoiceLineUserControl>
	{
		public void TestJI_CountryOfOriginColumn()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			using (var form = new ZForm(declaration))
			using (var control = new ExportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();
				form.Controls.Add(control);
				form.Show();
				AssertType<ZCodeFindBoxColumnStyleInfo>("JI_CountryOfOrigin", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CountryOfOrigin));
			}
		}

		public void TestColumnLayoutContextForInvoiceLinesGrid()
		{
			using (var control = new ExportInvoiceLineUserControl())
			{
				AssertEquals("Column layout context should be export", nameof(Customs.GUI.DeclarationType.Export), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestJI_RN_NKCountryOfExportColumn()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			using (var form = new ZForm(declaration))
			using (var control = new ExportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();
				form.Controls.Add(control);
				form.Show();
				AssertEquals("JI_RN_NKCountryOfExport", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(108), control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport).Width);
			}
		}

		public void TestNetWeightInKGColumn_GroupName()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			using (var form = new ZForm(declaration))
			using (var control = new ExportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();
				form.Controls.Add(control);
				form.Show();
				AssertEquals("JI_CustomsQuantity Group Name", "Net Weight in KG", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsQuantity).GroupName.Caption);
				AssertEquals("JI_CustomsUnitQty Group Name", "Net Weight in KG", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsUnitQty).GroupName.Caption);
			}
		}

		public void TestZG_IsMainPackColumn()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			using (var form = new ZForm(declaration))
			using (var control = new ExportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();
				form.Controls.Add(control);
				form.Show();
				AssertEquals("ZG_IsMainPack", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(108), control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.ZG_IsMainPack).Width);
			}
		}

		public void TestZG_IsMainPackColumn_IsVisibleByDefault()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			using (var form = new ZForm(declaration))
			using (var control = new ExportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();
				form.Controls.Add(control);
				form.Show();
				AssertEquals("IsVisible", true, control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.ZG_IsMainPack).IsVisible);
			}
		}
		
		public void TestPreviousDocumentsTabPageCaptionAndUserControlType()
		{
			EUDynamicControlTestHelper.AssertEUInvoiceLineUserControlTabPageCaptionAndUserControlType<ExportInvoiceLineUserControl>(Factory.New<JobDeclaration>(), "PreviousDocumentsTabPage", "PreviousDocumentsUserControl", "Previous Documents", typeof(LayoutPreviousDocumentsUserControl));
		}

		public void TestAdditionalInfosTabPageCaptionAndUserControlType()
		{
			EUDynamicControlTestHelper.AssertEUInvoiceLineUserControlTabPageCaptionAndUserControlType<ExportInvoiceLineUserControl>(Factory.New<JobDeclaration>(), "AdditionalInfosTabPage", "additionalInfosUserControl1", "Additional Documents", typeof(AdditionalInfosUserControlWithGrid));
		}

		public void TestAdditionalInfosTabPageIndex()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm())
			using (var control = new ExportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				var tabPageControl = control.FindSingle<ZTemplateTabControl>("LineDetailTabControl");
				var tabPages = tabPageControl.TabPages;

				AssertEquals("AdditionalInfosTabPage.Index", 10, tabPages.IndexOfKey("AdditionalInfosTabPage"));
			}
		}

		public void TestSupportingDocumentsTabPageCaptionAndUserControlType()
		{
			EUDynamicControlTestHelper.AssertEUInvoiceLineUserControlTabPageCaptionAndUserControlType<ExportInvoiceLineUserControl>(Factory.New<JobDeclaration>(), "SupportingDocumentsTabPage", "SupportingDocumentsUserControl", "Supporting Documents", typeof(InvoiceLineLayoutSupportingDocumentsUserControl));
		}

		public void TestFiscalReferencesTabPage()
		{
			DynamicControlTestHelper.AssertTabPageCaptionAndUserControlType<ExportInvoiceLineUserControl>("FiscalReferencesTabPage", "FiscalReferencesUserControl", "Fiscal References", typeof(InvoiceLineFiscalReferencesUserControl));
		}

		public void TestOrganizationsTabPage()
		{
			DynamicControlTestHelper.AssertTabPageCaptionAndUserControlType<ExportInvoiceLineUserControl>("OrganizationsTabPage", "organizationsUserControl1", "Organizations", typeof(InvoiceLineOrganizationsUserControl));
		}

		public void TestPackagesPivotTabPage()
		{
			AssertTabPageCaption("PackagesPivotTabPage", "Packages");
		}

		public void TestContainersTabPage()
		{
			AssertTabPageCaption("ContainersTabPage", "Containers");
		}

		public void TestCustomsNumberColumn()
		{
			using (var control = new ExportInvoiceLineUserControl())
			{
				control.InitializeGridLayout();
				AssertNotNull("A column ZG_CusNumber should exist.", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.ZG_CusNumber));
			}
		}

		public void TestChargesGridsColumnVisibility()
		{
			using (var control = new ExportInvoiceLineUserControl())
			{
				control.InitializeGridLayout();

				var columnsToHide = new[] { InvoiceCharge.Schema.J7_IsDutiable, InvoiceCharge.Schema.J7_IsGSTApplicable };
				var chargesAndApportionedChargesGrids = new[] { control.InvoiceLineCharges.ChargesGrid, control.InvoiceLineCharges.ApportionedChargesGrid };
				CombineAssertions(() =>
				{
					foreach (var grid in chargesAndApportionedChargesGrids)
					{
						foreach (var columnName in columnsToHide)
						{
							var invoiceLineChargesGridColumnStyle = grid.GetColumnStyle(columnName);
							AssertEquals(string.Format("{0} - {1} not visible", new[] { grid.Name, columnName }), true, invoiceLineChargesGridColumnStyle.IsUnavailable);
						}
					}
				});
			}
		}

		public void TestChargesGridsColumn_Description()
		{
			using (var control = new ExportInvoiceLineUserControl())
			{
				control.InitializeGridLayout();
				var chargesAndApportionedChargesGrids = new[] { control.InvoiceLineCharges.ChargesGrid, control.InvoiceLineCharges.ApportionedChargesGrid };
				CombineAssertions(() =>
				{
					var descriptionColumnName = InvoiceCharge.Schema.ChargeCodeDescription;

					foreach (var grid in chargesAndApportionedChargesGrids)
					{
						var gridColumnStyle = grid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().FirstOrDefault(c => c.ColumnName == descriptionColumnName);
						AssertEquals(string.Format("{0} ChargeCodeDescription Caption", grid.Name), "Description", gridColumnStyle.Caption);
						AssertEquals(string.Format("{0} ChargeCodeDescription Width", grid.Name), ControlDpiScalingHelper.ScaleToCurrentDpiX(138), gridColumnStyle.Width);
					}
				});
			}
		}

		public void TestContainersGridColumnVisibility()
		{
			using (var control = new ExportInvoiceLineUserControl())
			{
				control.InitializeGridLayout();

				var columnToHide = "Mode";
				var containersGrid = control.FindSingle<ZGrid>("CusContainerInvoiceLineGrid");

				var containersGridColumnStyle = containersGrid.GetColumnStyle(columnToHide);
				AssertEquals(string.Format("Container Mode not visible", new[] { containersGrid.Name, columnToHide }), true, containersGridColumnStyle.IsUnavailable);
			}
		}

		protected override ZBool DefaultDynamicLayoutApplied => ZBool.True;

		protected override ZString DefaultUniversalTariffType => Universal.Constants.TariffTypes.Export;

		static void AssertTabPageCaption(string tabPageName, string expectedCaption)
		{
			using (var form = new ZForm())
			using (var control = new ExportInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var tabPage = control.FindSingle<ZTabPage>(tabPageName);
				AssertEquals(expectedCaption, tabPage.CaptionResourceString.Caption);
			}
		}
	}
}
