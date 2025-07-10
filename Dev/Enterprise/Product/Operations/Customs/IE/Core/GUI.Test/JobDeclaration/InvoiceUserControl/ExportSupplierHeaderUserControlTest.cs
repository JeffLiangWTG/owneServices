using System.Collections.Generic;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.GUI.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using InvoiceCharge = Enterprise.Customs.EU.Business.Declaration.InvoiceCharge;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(ExportSupplierHeaderUserControl))]
	sealed class ExportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<ExportSupplierHeaderUserControl, JobDeclaration>
	{
		public void TestPreviousDocumentsTabPageCaptionAndUserControlType()
		{
			EUDynamicControlTestHelper.AssertEUSupplierHeaderUserControlTabPageCaptionAndUserControlType<ExportSupplierHeaderUserControl>(Factory.New<JobDeclaration>(), "PreviousDocumentsTabPage", "previousDocumentsUserControl1", "Previous Docs", typeof(InvoiceHeaderExportPreviousDocumentsUserControl));
		}

		public void TestAdditionalInfoTabPageCaptionAndUserControlType()
		{
			EUDynamicControlTestHelper.AssertEUSupplierHeaderUserControlTabPageCaptionAndUserControlType<ExportSupplierHeaderUserControl>(Factory.New<JobDeclaration>(), "AdditionalInfoTabPage", "additionalInfosUserControl1", "Additional Documents", typeof(AdditionalInfosUserControlWithGrid));
		}

		public void TestAdditionalInfosTabPageIndex()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm())
			using (var control = new ExportSupplierHeaderUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				var tabPageControl = control.FindSingle<ZTemplateTabControl>("InvoiceTabControl");
				var tabPages = tabPageControl.TabPages;

				AssertEquals("AdditionalInfosTabPage.Index", 5, tabPages.IndexOfKey("AdditionalInfoTabPage"));
			}
		}

		public void TestSupportingDocumentsTabPageCaptionAndUserControlType()
		{
			EUDynamicControlTestHelper.AssertEUSupplierHeaderUserControlTabPageCaptionAndUserControlType<ExportSupplierHeaderUserControl>(Factory.New<JobDeclaration>(), "SupportingDocumentsTabPage", "SupportingDocumentsUserControl", "Supporting Documents", typeof(InvoiceLayoutSupportingDocumentsUserControl));
		}

		public void TestRemoveJobComInvoiceHeadersBoundGridColumns()
		{
			using (var control = new ExportSupplierHeaderUserControl())
			{
				control.InitializeGridLayout();

				var columnStyles = control.JobComInvoiceHeadersBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				var namesToRemove = new[] { "Supplier", "Consignee", "Exporter", "Importer", "Intermediate Consignee", "Invoicer", "Seller", "Selling Agent", "Ship to Party", "Sold to Party", "Supplier Name", "FOB Amount", "FOB Curr" };
				AssertEquals(false, columnStyles.Any(x => namesToRemove.Contains(x.GroupName.Caption) || namesToRemove.Contains(x.CaptionResourceString.Caption)));
			}
		}

		public void TestRenameJobComInvoiceHeadersBoundGridColumns()
		{
			using (var control = new ExportSupplierHeaderUserControl())
			{
				control.InitializeGridLayout();

				var columnStyles = control.JobComInvoiceHeadersBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				var oldGroupNames = new[] { "Buyer", "Supplier" };
				AssertEquals(false, columnStyles.Any(x => oldGroupNames.Contains(x.GroupName.Caption)));

				var newGroupNames = new[] { "Buyer/Consignee", "Supplier/Consignor" };
				AssertEquals(true, columnStyles.Any(x => newGroupNames.Contains(x.GroupName.Caption)));

				var oldColumnNames = new[] { "Buyer", "Buyer Address", "Supplier", "Supplier Address" };
				AssertEquals(false, columnStyles.Any(x => oldColumnNames.Contains(x.CaptionResourceString.Caption)));

				var newColumnNames = new[] { "Buyer/Consignee", "Buyer/Consignee Address", "Supplier/Consignor", "Supplier/Consignor Address" };
				AssertEquals(true, columnStyles.Any(x => newColumnNames.Contains(x.CaptionResourceString.Caption)));
			}
		}

		public void TestColumnsAdded()
		{
			using (var control = new ExportSupplierHeaderUserControl())
			{
				control.InitializeGridLayout();
				control.Show();
				var columns = control.JobComInvoiceHeadersBoundGrid.ColumnStyles.OfType<ZGridColumnInfo>().Select(x => x.ColumnName);
				AssertCollectionContains("JZ_UCR", columns);
			}
		}

		public void TestJZ_IncoTermPlaceCaption()
		{
			using (var control = new ExportSupplierHeaderUserControl())
			{
				control.InitializeGridLayout();
				control.Show();
				var jzIncoTermPlaceColumn = control.JobComInvoiceHeadersBoundGrid.ColumnStyles.OfType<ZGridColumnInfo>().First(x => x.ColumnName.Equals("JZ_IncoTermPlace"));
				AssertEquals("Incoterm Location", jzIncoTermPlaceColumn.CaptionResourceString.Caption);
			}
		}

		public void TestFieldsAdded()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var userControl = new ExportSupplierHeaderUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				CombineAssertions(() =>
				{
					AssertNotNull("UCRTextBox", userControl.FindSingleOrDefault<ZTextBox>("UCRTextBox"));
				});
			}
		}

		public void TestChargesGridsColumnVisibility()
		{
			using (var control = new ExportSupplierHeaderUserControl())
			{
				control.InitializeGridLayout();

				var isDutiableColumnName = InvoiceCharge.Schema.J7_IsDutiable;
				var chargesGrids = new[] { control.InvoiceChargesGrid, control.BaseGroupChargesGrid, control.ApportionedChargesGrid };

				CombineAssertions(() =>
				{
					foreach (var grid in chargesGrids)
					{
						var invoiceGridColumnStyle = grid.GetColumnStyle(isDutiableColumnName);
						AssertEquals(string.Format("{0} - 'Add to FOB?' (IsDutiable) not available", grid.Name), true, invoiceGridColumnStyle.IsUnavailable);
					}
				});
			}
		}

		protected override IEnumerable<string> ExpectedControlList => DefaultControlList.Except(new[] { "GroupInvoiceDropEdit", "InvoiceCurrLandedCostExRateCalcEdit" }).Union(new[] { "AgreedPlaceCodeFindBox", "TransportChargesMethodOfPaymentDropEdit", "AdditionalTermsTextBox" });
	}
}
