using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.GUI.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(ImportInvoiceLineUserControl))]
sealed class ImportInvoiceLineUserControlTest : CommonInvoiceLineUserControlTest<ImportInvoiceLineUserControlForTest>
{
	public void TestVatDetailGuidDropEditVisibility()
	{
		using (var form = new ZForm())
		using (var control = new ImportInvoiceLineUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			var vatDetailGuidDropEdit = control.FindSingle<ZGuidDropEdit>(x => x.Name == "VatDetailGuidDropEdit");
			AssertEquals("VatDetailGuidDropEdit should be visible", true, vatDetailGuidDropEdit.Visible);
		}
	}

	public void TestSupportingDocumentsUserControlType()
	{
		using (var form = new ZForm())
		using (var control = new ImportInvoiceLineUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			control.Controls.Find("SupportingDocumentsTabPage", true).First().Show();
			var supportingDocument = control.Controls.Find("SupportingDocumentsUserControl", true).First() as ZDynamicControlCreationUserControl;
			AssertEquals(typeof(InvoiceLineLayoutSupportingDocumentsUserControl), supportingDocument.UserControlType);
		}
	}

	public void TestCountryOfSupplyVisibility()
	{
		using (var form = new ZForm())
		using (var control = new ImportInvoiceLineUserControl())
		{
			form.Controls.Add(control);
			form.Show();
			var countryOfSupply = control.Controls.Find("CountryOfSupplyCodeFindBox", true).FirstOrDefault();
			AssertEquals("Country of supply must be hidden in Import", false, countryOfSupply.Visible);
		}
	}

	public void TestValuationAdjustmentVisibility()
	{
		using (var form = new ZForm())
		using (var control = new ImportInvoiceLineUserControl())
		{
			var adjCode = (ZDropEdit)control.Controls.Find("ValuationAdjustmentCodeDropEdit", true).FirstOrDefault();
			AssertEquals(false, adjCode.Visible);
			var adjPercent = (ZCalcEdit)control.Controls.Find("ValuationAdjustmentPercentageCalcEdit", true).FirstOrDefault();
			AssertEquals(false, adjPercent.Visible);
		}
	}

	public void TestVatTypeDropEdit()
	{
		using (var form = new ZForm())
		using (var control = new ImportInvoiceLineUserControl())
		{
			form.Controls.Add(control);
			form.Show();
			var vatType = (ZDropEdit)control.Controls.Find("VatTypeDropEdit", true).FirstOrDefault();
			AssertEquals(false, vatType.Visible);
			var classGroupbox = (ZGroupBox)control.Controls.Find("ClassificationDetailsGroupBox", true).FirstOrDefault();
			AssertEquals(false, classGroupbox.Controls.Contains(vatType));
		}
	}

	public void TestAdditionalInfoTabPageExists()
	{
		using (var userControl = new ImportInvoiceLineUserControl())
		{
			var additionalInfosTabPage = userControl.FindSingleOrDefault<ZTabPage>("AdditionalInfosTabPage");
			AssertNotNull(additionalInfosTabPage);

			var lineDetailTabControl = userControl.FindSingleOrDefault<ZTemplateTabControl>("LineDetailTabControl");
			AssertNotNull(lineDetailTabControl);

			AssertNotEquals("In Import InvoiceLine user control, Additional Info tab page index", -1, lineDetailTabControl.TabPages.IndexOf(additionalInfosTabPage));
		}
	}

	public void TestAdditionalInfosTabPageCaptionAndUserControlType()
	{
		EUDynamicControlTestHelper.AssertEUInvoiceLineUserControlTabPageCaptionAndUserControlType<ImportInvoiceLineUserControl>(Factory.New<JobDeclaration>(), "AdditionalInfosTabPage", "additionalInfosUserControl1", "[44] Additional Info", typeof(AdditionalInfosUserControl));
	}

	public void TestProductCodeControlTextLength()
	{
		using (var control = new ImportInvoiceLineUserControl())
		{
			var lineDetailTabControl = control.FindSingleOrDefault<ZTemplateTabControl>("LineDetailTabControl");
			var box = lineDetailTabControl.FindSingle<ZCodeFindBox>("PartNoCodeFindBox");
			AssertEquals(35, box.PreBoundMaxLength);
		}
	}

	public void TestGetOrganizationsUserControlType()
	{
		using (var userControl = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals("OrganizationsUserControlType", typeof(ImportInvoiceLineOrganizationsUserControl), userControl.GetOrganizationsUserControlTypeExposed());
		}
	}

	public void TestGetValuationIndicatorsUserControlType()
	{
		using (var userControl = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals("ValuationIndicatorsUserControlType", typeof(EU.GUI.InvoiceLineValuationIndicatorDropEditsUserControl), userControl.GetValuationIndicatorsUserControlTypeExposed());
		}
	}

	public void TestGetPreviousDocumentUserControlType()
	{
		using var userControl = new ImportInvoiceLineUserControlForTest();
		AssertEquals("PreviousDocumentUserControlType", typeof(InvoiceLineImportLayoutPreviousDocumentsUserControl), userControl.GetPreviousDocumentUserControlTypeExposed());
	}

	protected override Type InvoiceLineDetailsPanelLayoutType => typeof(ImportInvoiceLineDetailsLayout);

	protected override ImportInvoiceLineUserControlForTest GetNewInvoiceLineUserControl() => new ImportInvoiceLineUserControlForTest();

	protected override Type ExpectedPreviousDocumentsUserControlType => typeof(InvoiceLineImportLayoutPreviousDocumentsUserControl);
}

sealed class ImportInvoiceLineUserControlForTest : ImportInvoiceLineUserControl, IHasTaxTabPageExposedForTest, IHasPanelLayoutMembersExposedForTest
{
	public ImportInvoiceLineUserControlForTest()
	{
	}

	public Type GetOrganizationsUserControlTypeExposed() => GetOrganizationsUserControlType();

	public Type GetValuationIndicatorsUserControlTypeExposed() => GetValuationIndicatorsUserControlType();

	public Type GetPreviousDocumentUserControlTypeExposed() => GetPreviousDocumentsUserControlType();

	public ZTabPage TaxTabPageExposed => TaxTabPage;

	ZBool IHasPanelLayoutMembersExposedForTest.DynamicLayoutAppliedExposed => DynamicLayoutApplied;
	ZBool IHasPanelLayoutMembersExposedForTest.HasDifferentPanelLayoutExposed => HasDifferentPanelLayout;
	IPanelLayoutProvider IHasPanelLayoutMembersExposedForTest.GetNewInvoiceLineDetailsPanelLayoutExposed() => GetNewInvoiceLineDetailsPanelLayout();
}
