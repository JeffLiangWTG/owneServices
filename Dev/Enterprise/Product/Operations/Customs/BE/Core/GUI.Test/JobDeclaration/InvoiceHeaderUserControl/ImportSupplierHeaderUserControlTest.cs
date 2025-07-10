using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.BE.GUI.PlugIn;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.EU.GUI.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.BE.GUI.Testing;

[TestedType(typeof(ImportSupplierHeaderUserControl))]
sealed class ImportCustomsSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<ImportSupplierHeaderUserControl, JobDeclaration>
{
	public void TestPreviousDocumentsUserControlType()
	{
		using (var control = new ImportSupplierHeaderUserControlForTest())
		{
			AssertEquals(typeof(SupplierHeaderPreviousDocumentsUserControl), control.GetPreviousDocumentsUserControlTypeExposed());
		}
	}

	public void TestSupportingDocumentsTabPageCaptionAndUserControlType()
	{
		EUDynamicControlTestHelper.AssertEUSupplierHeaderUserControlTabPageCaptionAndUserControlType<ImportSupplierHeaderUserControl>(Factory.New<JobDeclaration>(), "SupportingDocumentsTabPage", "SupportingDocumentsUserControl", "Supporting Documents", typeof(InvoiceHeaderSupportingDocumentsUserControl));
	}

	public void TestColumnsAdded()
	{
		using (var control = new ImportSupplierHeaderUserControl())
		{
			control.InitializeGridLayout();
			var columns = control.JobComInvoiceHeadersBoundGrid.ColumnStyles.OfType<ZGridColumnInfo>().Select(x => x.ColumnName);
			AssertCollectionContains("JZ_UCR", columns);
		}
	}

	public void TestAdditionalInfoTabPageCaptionAndUserControlType()
	{
		EUDynamicControlTestHelper.AssertEUSupplierHeaderUserControlTabPageCaptionAndUserControlType<ImportSupplierHeaderUserControl>(Factory.New<JobDeclaration>(), "AdditionalInfoTabPage", "additionalInfosUserControl1", "[44] Additional Documents", typeof(AdditionalInfosUserControlWithGrid));
	}

	public void TestIntracommunityReceiverFindBox()
	{
		using (var control = new ImportSupplierHeaderUserControl())
		{
			control.InitializeGridLayout();
			var column = control.JobComInvoiceHeadersBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == JobComInvoiceHeader.Schema.ConsigneeOrgPK);
			CombineAssertions(() =>
			{
				AssertEquals("Column Visible", true, column.IsVisible);
				AssertEquals("Caption", "Intra-community Receiver", column.CaptionResourceString.Caption);
				AssertEquals("Group Key", "37BA5EE1-8C5C-4302-9812-76D87D08800A", column.GroupName.Key);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150), column.Width);
			});
		}
	}

	public void TestIntracommunityReceiverDropEdit()
	{
		using (var control = new ImportSupplierHeaderUserControl())
		{
			control.InitializeGridLayout();
			var column = control.JobComInvoiceHeadersBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == JobComInvoiceHeader.Schema.JZ_OA_ConsigneeAddress);
			CombineAssertions(() =>
			{
				AssertEquals("Column Visible", true, column.IsVisible);
				AssertEquals("Caption", "Intra-community Receiver Address", column.CaptionResourceString.Caption);
				AssertEquals("Group Key", "37BA5EE1-8C5C-4302-9812-76D87D08800A", column.GroupName.Key);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180), column.Width);
			});
		}
	}

	public void TestValueIndicatorsTabPage()
	{
		using (var control = new ImportSupplierHeaderUserControl())
		{
			AssertEquals("[UCC 4/13] Value Indicators", control.FindSingle<ZTabPage>("ValueIndicatorsTabPage").CaptionResourceString.Caption);
		}
	}

	public void TestTabPagesOrder()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		using (var form = new ZForm(declaration))
		using (var control = new ImportSupplierHeaderUserControl())
		{
			form.Controls.Add(control);
			control.JobDeclaration = declaration;
			control.SetDataBinding(declaration, "");
			form.Show();
			var tabPages = control.InvoiceTabControl.TabPages;
			AssertArrayEqualsByElements(new[]
			{
				"ComInvoiceDetailsTabPage",
				"SupportingDocumentsTabPage",
				"AdditionalInfoTabPage" ,
				"PreviousDocumentsTabPage",
				"ValueIndicatorsTabPage",
				"CustomFieldsTabPage"
			}, tabPages.Cast<ZTabPage>().Where(x => x.TabVisible).Select(x => x.Name).ToArray());
		}
	}

	protected override IEnumerable<string> ExpectedControlList => DefaultControlList.Except(new[] { "GroupInvoiceDropEdit" }).Union(new[] { "AgreedPlaceCodeFindBox", "TransportChargesMethodOfPaymentDropEdit" });

	sealed class ImportSupplierHeaderUserControlForTest : ImportSupplierHeaderUserControl
	{
		public Type GetPreviousDocumentsUserControlTypeExposed() => GetPreviousDocumentsUserControlType();
	}
}
