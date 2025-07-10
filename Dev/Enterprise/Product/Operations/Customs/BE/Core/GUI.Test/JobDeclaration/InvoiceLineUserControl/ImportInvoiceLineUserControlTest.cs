using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.BE.GUI.PlugIn;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI.Testing;

sealed class ImportInvoiceLineUserControlTest : TestCaseWithFactory
{
	public void TestColumnLayoutContextForInvoiceLinesGrid()
	{
		using (var control = new ImportInvoiceLineUserControl())
		{
			AssertEquals("Column layout context should be import", nameof(Customs.GUI.DeclarationType.Import), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
		}
	}

	public void TestDynamicLayout()
	{
		using (var control = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals("Layout should be set to dynamic", true, control.DynamicLayoutAppliedExposed);
		}
	}

	public void TestBottomPanelMinimumSize()
	{
		using (var control = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals(new System.Drawing.Size(750, 350), control.BottomPanelExposed.MinimumSize);
		}
	}

	public void TestContainsRegionOfDestination()
	{
		using (var control = new ImportInvoiceLineUserControl())
		{
			var rodCtl = control.Controls.Find("RegionOfDestinationDropEdit", true);
			AssertEquals("There should be exactly one Region of Destination control", 1, rodCtl.Length);
			AssertEquals("Region of Destination should be visible", typeof(ZDropEdit), rodCtl[0].GetType());
		}
	}

	public void TestAdditionalInfosUserControlUsed()
	{
		using (var control = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals(typeof(InvoiceLineAdditionalInfosUserControlWithGrid), control.GetAdditionalInfosUserControlTypeExposed());
		}
	}

	public void TestGetPreviousDocumentsUserControlType()
	{
		using (var control = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals(typeof(ImportInvoiceLinePreviousDocumentsUserControl), control.GetPreviousDocumentsUserControlTypeExposed());
		}
	}

	public void TestGetPreviousDocumentsUserControlType_UCC6()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		using (var form = new ZForm(declaration))
		using (var control = new ImportInvoiceLineUserControlForTest())
		{
			form.SetDataBinding(declaration, "");
			form.Controls.Add(control);
			control.JobDeclaration = declaration;
			form.Show();

			AssertEquals(typeof(ImportInvoiceLinePreviousDocumentsUserControlUCC6), control.GetPreviousDocumentsUserControlTypeExposed());

			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			AssertEquals(typeof(ImportInvoiceLinePreviousDocumentsUserControl), control.GetPreviousDocumentsUserControlTypeExposed());
		}
	}

	public void TestGetSupportingDocumentsUserControlType()
	{
		using (var control = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals(typeof(InvoiceLineSupportingDocumentsUserControl), control.GetSupportingDocumentsUserControlTypeExposed());
		}
	}

	public void TestValuationIndicatorsUserControlType()
	{
		using (var control = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals(typeof(EU.GUI.InvoiceLineValuationIndicatorDropEditsUserControl), control.GetValuationIndicatorsUserControlTypeExposed());
		}
	}

	public void TestSupportingDocumentsTabPage()
	{
		var declaration = Factory.New<JobDeclaration>();
		using (var form = new ZForm(declaration))
		using (var userControl = new ImportInvoiceLineUserControl())
		{
			form.SetDataBinding(declaration, ZString.Empty);
			form.Controls.Add(userControl);
			userControl.JobDeclaration = declaration;
			form.Show();
			var tabPage = userControl.FindSingle<ZTabPage>("SupportingDocumentsTabPage");
			CombineAssertions(() =>
			{
				AssertEquals("SupportingDocumentsTabPage visible", true, tabPage.TabVisible);
				AssertEquals("Caption", "[UCC 2/3] Supporting documents", tabPage.CaptionResourceString.Caption);
			});
		}
	}

	public void TestAdditionalInfosTabPage()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.Invoices.AddNew().InvoiceLines.AddNew();

		using (var frm = new ZForm(declaration))
		using (var userControl = new ImportInvoiceLineUserControl())
		{
			frm.Controls.Add(userControl);
			userControl.JobDeclaration = declaration;
			frm.Show();

			var tabPage = userControl.FindSingle<ZTabPage>("AdditionalInfosTabPage");
			tabPage.Show();
			CombineAssertions(() =>
			{
				AssertEquals("AdditionalInfosTabPage visible", true, tabPage.TabVisible);
				AssertEquals("Caption", "Additional Documents", tabPage.CaptionResourceString.Caption);
				var foundUserControl = userControl.FindSingle<ZDynamicControlCreationUserControl>("additionalInfosUserControl1");
				AssertEquals("UserControl type", typeof(InvoiceLineAdditionalInfosUserControlWithGrid), foundUserControl.UserControlType);
			});
		}
	}

	public void TestTabPagesOrder()
	{
		var declaration = Factory.New<JobDeclaration>();
		using (var form = new ZForm(declaration))
		using (var control = new ImportInvoiceLineUserControl())
		{
			form.SetDataBinding(declaration, ZString.Empty);
			form.Controls.Add(control);
			control.JobDeclaration = declaration;
			var tabPages = control.LineDetailTabControl.TabPages;
			AssertArrayEqualsByElements(new[]
			{
				"NewLineDetailsTabPage",
				"LineChargesTabPage",
				"FiscalReferencesTabPage",
				"SupportingDocumentsTabPage",
				"AdditionalInfosTabPage",
				"PreviousDocumentsTabPage",
				"PackagesPivotTabPage",
				"OrganizationsTabPage",
				"AuthorisationsTabPage",
				"ValueIndicatorsTabPage",
				"CustomFieldsTabPage"
			}, tabPages.Cast<ZTabPage>().Where(x => x.TabVisible).Select(x => x.Name).ToArray());
		}
	}

	public void TestGetOrganizationsUserControlType()
	{
		using (var control = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals(typeof(ImportInvoiceLineOrganizationsUserControl), control.GetOrganizationsUserControlTypeExposed());
		}
	}

	public void TestGetPreviousDocumentsTabPageCaption()
	{
		using (var control = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals("[UCC 2/1] Previous documents", control.GetPreviousDocumentsTabPageCaptionExposed());
		}
	}

	public void TestGetSupportingDocumentsTabPageCaption()
	{
		using (var control = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals("[UCC 2/3] Supporting documents", control.GetSupportingDocumentsTabPageCaptionExposed());
		}
	}

	public void TestGetPackagesTabPageCaption()
	{
		using (var control = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals("Packages", control.GetPackagesTabPageCaptionExposed());
		}
	}

	public void TestGetLineChargesTabPageCaption()
	{
		using (var control = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals("[UCC 4/9] Charges", control.GetLineChargesTabPageCaptionExposed());
		}
	}

	public void TestGetValueIndicatorsTabPageCaption()
	{
		using (var control = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals("[UCC 4/13] Value Indicators", control.GetValueIndicatorsTabPageCaptionExposed());
		}
	}

	sealed class ImportInvoiceLineUserControlForTest : ImportInvoiceLineUserControl
	{
		public ZBool DynamicLayoutAppliedExposed => DynamicLayoutApplied;

		public Type GetAdditionalInfosUserControlTypeExposed() => GetAdditionalInfosUserControlType();

		public Type GetPreviousDocumentsUserControlTypeExposed() => GetPreviousDocumentsUserControlType();

		public Type GetSupportingDocumentsUserControlTypeExposed() => GetSupportingDocumentsUserControlType();

		public Type GetOrganizationsUserControlTypeExposed() => GetOrganizationsUserControlType();

		public Type GetValuationIndicatorsUserControlTypeExposed() => GetValuationIndicatorsUserControlType();

		public ZPanel BottomPanelExposed => BottomPanel;

		public string GetPreviousDocumentsTabPageCaptionExposed() => base.GetPreviousDocumentsTabPageCaption(null).Caption;

		public string GetSupportingDocumentsTabPageCaptionExposed() => base.GetSupportingDocumentsTabPageCaption(null).Caption;

		public string GetPackagesTabPageCaptionExposed() => base.GetPackagesTabPageCaption(null).Caption;

		public string GetLineChargesTabPageCaptionExposed() => base.GetLineChargesTabPageCaption(null).Caption;

		public string GetValueIndicatorsTabPageCaptionExposed() => base.GetValueIndicatorsTabPageCaption(null).Caption;
	}
}
