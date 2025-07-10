using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.BE.GUI.PlugIn;
using Enterprise.Customs.EU.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.BE.GUI.Testing;

[TestedType(typeof(ExportInvoiceLineUserControl))]
sealed class ExportInvoiceLineUserControlTest : TestCaseWithFactory
{
	public void TestColumnLayoutContextForInvoiceLinesGrid()
	{
		using (ExportInvoiceLineUserControl control = new ExportInvoiceLineUserControl())
		{
			AssertEquals("Column layout context should be export", nameof(Customs.GUI.DeclarationType.Export), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
		}
	}

	public void TestContainsNoRegionOfDestination()
	{
		using (ExportInvoiceLineUserControl control = new ExportInvoiceLineUserControl())
		{
			var rodCtl = control.Controls.Find("RegionOfDestinationDropEdit", true);
			AssertEquals("There should be no Region of Destination control", 0, rodCtl.Length);
		}
	}

	public void TestDynamicLayout()
	{
		using (var control = new ExportInvoiceLineUserControlForTest())
		{
			AssertEquals("Layout should be set to dynamic", true, control.DynamicLayoutAppliedExposed);
		}
	}

	public void TestBottomPanelMinimumSize()
	{
		using (var control = new ExportInvoiceLineUserControlForTest())
		{
			AssertEquals(new System.Drawing.Size(750, 350), control.BottomPanelExposed.MinimumSize);
		}
	}

	public void TestSupportingDocumentsTabPageCaptionAndUserControlType()
	{
		EUDynamicControlTestHelper.AssertEUInvoiceLineUserControlTabPageCaptionAndUserControlType<ExportInvoiceLineUserControl>(Factory.New<JobDeclaration>(), "SupportingDocumentsTabPage", "SupportingDocumentsUserControl", "[UCC 2/3] Supporting documents", typeof(InvoiceLineSupportingDocumentsUserControl));
	}

	public void TestGetPreviousDocumentsUserControlType()
	{
		using (var control = new ExportInvoiceLineUserControlForTest())
		{
			AssertEquals(typeof(InvoiceLinePreviousDocumentsUserControl), control.GetPreviousDocumentsUserControlTypeExposed());
		}
	}

	public void TestGetPreviousDocumentsUserControlType_UCC6()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		using (var form = new ZForm(declaration))
		using (var control = new ExportInvoiceLineUserControlForTest())
		{
			form.SetDataBinding(declaration, "");
			form.Controls.Add(control);
			control.JobDeclaration = declaration;
			form.Show();

			AssertEquals(typeof(InvoiceLinePreviousDocumentsUserControlUCC6), control.GetPreviousDocumentsUserControlTypeExposed());

			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			AssertEquals(typeof(InvoiceLinePreviousDocumentsUserControl), control.GetPreviousDocumentsUserControlTypeExposed());
		}
	}

	public void TestOrganizationsTabPage()
	{
		var declaration = Factory.New<JobDeclaration>();
		using (var form = new ZForm(declaration))
		using (var control = new ExportInvoiceLineUserControl())
		{
			form.SetDataBinding(declaration, "");
			form.Controls.Add(control);
			control.JobDeclaration = declaration;
			form.Show();

			AssertEquals("OrganizationsTabPage Visible", true, control.FindSingleOrDefault<ZTabPage>("OrganizationsTabPage")?.TabVisible);
		}
	}

	public void TestAdditionalInfosTabPageCaptionAndUserControlType()
	{
		EUDynamicControlTestHelper.AssertEUInvoiceLineUserControlTabPageCaptionAndUserControlType<ExportInvoiceLineUserControl>(Factory.New<JobDeclaration>(), "AdditionalInfosTabPage", "additionalInfosUserControl1", "Additional Documents", typeof(InvoiceLineAdditionalInfosUserControlWithGrid));
	}

	public void TestTabPagesOrder()
	{
		var declaration = Factory.New<JobDeclaration>();
		using (var form = new ZForm(declaration))
		using (var control = new ExportInvoiceLineUserControl())
		{
			form.SetDataBinding(declaration, ZString.Empty);
			form.Controls.Add(control);
			control.JobDeclaration = declaration;
			form.Show();
			var tabPages = control.LineDetailTabControl.TabPages;
			AssertArrayEqualsByElements(new[]
			{
				"NewLineDetailsTabPage",
				"LineChargesTabPage",
				"SupportingDocumentsTabPage" ,
				"AdditionalInfosTabPage",
				"PreviousDocumentsTabPage",
				"PackagesPivotTabPage",
				"OrganizationsTabPage",
				"DangerousGoodsTabPage",
				"AuthorisationsTabPage",
				"ValueIndicatorsTabPage",
				"CustomFieldsTabPage"
			}, tabPages.Cast<ZTabPage>().Where(x => x.TabVisible).Select(x => x.Name).ToArray());
		}
	}

	public void TestGetPreviousDocumentsTabPageCaption()
	{
		using (var control = new ExportInvoiceLineUserControlForTest())
		{
			AssertEquals("[UCC 2/1] Previous documents", control.GetPreviousDocumentsTabPageCaptionExposed());
		}
	}

	public void TestGetSupportingDocumentsTabPageCaption()
	{
		using (var control = new ExportInvoiceLineUserControlForTest())
		{
			AssertEquals("[UCC 2/3] Supporting documents", control.GetSupportingDocumentsTabPageCaptionExposed());
		}
	}

	public void TestGetPackagesTabPageCaption()
	{
		using (var control = new ExportInvoiceLineUserControlForTest())
		{
			AssertEquals("Packages", control.GetPackagesTabPageCaptionExposed());
		}
	}

	public void TestGetLineChargesTabPageCaption()
	{
		using (var control = new ExportInvoiceLineUserControlForTest())
		{
			AssertEquals("[UCC 4/9] Charges", control.GetLineChargesTabPageCaptionExposed());
		}
	}

	class ExportInvoiceLineUserControlForTest : ExportInvoiceLineUserControl
	{
		public ZBool DynamicLayoutAppliedExposed => DynamicLayoutApplied;

		public Type GetPreviousDocumentsUserControlTypeExposed() => base.GetPreviousDocumentsUserControlType();

		public ZPanel BottomPanelExposed => BottomPanel;

		public string GetPreviousDocumentsTabPageCaptionExposed() => base.GetPreviousDocumentsTabPageCaption(null).Caption;

		public string GetSupportingDocumentsTabPageCaptionExposed() => base.GetSupportingDocumentsTabPageCaption(null).Caption;

		public string GetPackagesTabPageCaptionExposed() => base.GetPackagesTabPageCaption(null).Caption;

		public string GetLineChargesTabPageCaptionExposed() => base.GetLineChargesTabPageCaption(null).Caption;
	}
}
