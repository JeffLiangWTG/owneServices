using System;
using System.Linq;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing;

public class ImportInvoiceLineUserControlBaseOnlyTest : EUImportInvoiceLineUserControlTest<EUImportInvoiceLineUserControl>
{
	[RequiresSTA]
	public void TestJI_TaxOrFeeDetail()
	{
		using (var form = new ZForm())
		using (var control = new EUImportInvoiceLineUserControl())
		{
			form.Controls.Add(control);
			form.Show();
			var vatDetailGuidDropEdit = (ZGuidDropEdit)control.Controls.Find("VatDetailGuidDropEdit", true).First();
			AssertNotNull("VatDetailGuidDropEdit", vatDetailGuidDropEdit);
			Assert("vatDetailGuidDropEdit.DescriptionBox should be visible.", vatDetailGuidDropEdit.ShowDescriptionBox);
		}
	}

	[RequiresSTA]
	public void TestTaxOrFeeDropEditMaxLength()
	{
		using (var form = new ZForm())
		using (var control = new EUImportInvoiceLineUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			var vatTypeDropEdit = (ZDropEdit)control.Controls.Find("VatTypeDropEdit", true).First();
			AssertEquals(4, vatTypeDropEdit.PreBoundMaxLength);
		}
	}

	[RequiresSTA]
	public void TestTaxOrFeeDropEditShowDescriptionBox()
	{
		using var form = new ZForm();

		using (var control = new EUImportInvoiceLineUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			var vatTypeDropEdit = (ZDropEdit)control.Controls.Find("VatTypeDropEdit", true).First();
			Assert("ShowDescriptionBox should be true when IsJI_TaxOrFeeDetailVisible is false(by default).", vatTypeDropEdit.ShowDescriptionBox);
		}

		using (var control = new EUImportInvoiceLineUserControl_ShowTaxOrFeeDetailDropEdit())
		{
			form.Controls.Add(control);
			form.Show();

			var vatTypeDropEdit = (ZDropEdit)control.Controls.Find("VatTypeDropEdit", true).First();
			Assert("ShowDescriptionBox should be false when IsJI_TaxOrFeeDetailVisible is overridden to true)", !vatTypeDropEdit.ShowDescriptionBox);
		}
	}

	class EUImportInvoiceLineUserControl_ShowTaxOrFeeDetailDropEdit : EUImportInvoiceLineUserControl
	{
		protected override bool IsJI_TaxOrFeeDetailVisible => true;
	}

	public void TestQuotaControlsVisibility()
	{
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		using (var form = new JobDeclarationForm(declaration))
		{
			form.Show();
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
			var control = (EUImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;

			CombineAssertions(() =>
			{
				var quotaTextBox = control.FindSingle<ZTextBox>("QuotaTextBox");
				AssertEquals("QuotaTextBox", false, quotaTextBox.Visible);

				var quotaDropEdit = control.FindSingle<ZDropEdit>("QuotaDropEdit");
				AssertEquals("QuotaDropEdit", true, quotaDropEdit.Visible);

				var secondQuotaTextBox = control.FindSingle<ZTextBox>("SecondQuotaTextBox");
				AssertEquals("SecondQuotaTextBox", false, secondQuotaTextBox.Visible);

				var secondQuotaDropEdit = control.FindSingle<ZDropEdit>("SecondQuotaDropEdit");
				AssertEquals("SecondQuotaDropEdit", false, secondQuotaDropEdit.Visible);

				var checkQuotaLinkLabel = control.FindSingle<ZLinkLabel>("CheckQuotaBalanceLinkLabel");
				AssertEquals("CheckQuotaBalanceLinkLabel", true, checkQuotaLinkLabel.Visible);
			});
		}
	}

	public void TestQuotaBalanceUrlLinkLabel()
	{
		using (var form = new JobDeclarationForm(declaration))
		{
			form.Show();
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
			var control = (EUImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
			var checkQuotaLinkLabel = control.FindSingle<ZLinkLabel>("CheckQuotaBalanceLinkLabel");

			checkQuotaLinkLabel.OnLinkClicked_Exposed(new System.Windows.Forms.LinkLabelLinkClickedEventArgs(null));
			AssertContains("CheckQuotaBalanceLinkLabel", @"https://ec.europa.eu/taxation_customs/dds2/taric/quota_consultation.jsp", WebUrlLauncher.LastUrlLaunched);
		}
	}

	public void TestQuotaBalanceUrlLinkLabelWithQuota()
	{
		using (var form = new JobDeclarationForm(declaration))
		{
			form.Show();
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
			var control = (EUImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
			var checkQuotaLinkLabel = control.FindSingle<ZLinkLabel>("CheckQuotaBalanceLinkLabel");
			var quotaDropEdit = control.FindSingle<ZDropEdit>("QuotaDropEdit");
			quotaDropEdit.Text = "098567";

			checkQuotaLinkLabel.OnLinkClicked_Exposed(new System.Windows.Forms.LinkLabelLinkClickedEventArgs(null));
			AssertContains("CheckQuotaBalanceLinkLabel", @"https://ec.europa.eu/taxation_customs/dds2/taric/quota_consultation.jsp", WebUrlLauncher.LastUrlLaunched);
			AssertContains("CheckQuotaBalanceLinkLabel", "Code=098567", WebUrlLauncher.LastUrlLaunched);
		}
	}

	public void TestMethodOfPaymentDropEdit_LineDetailsTabPageVisibility_Invisible()
	{
		AssertMethodOfPaymentDropEditVisibility(false, ControlDpiScalingHelper.NewScaledSize(440, 330, true));
	}

	public void TestMethodOfPaymentDropEdit_LineDetailsTabPageVisibility_Visible()
	{
		ConfigurationTestHelper.ClearDeclarationConfiguration(declaration);

		var invoiceLineConfigurationMock = new Mock<InvoiceLineConfiguration>();
		invoiceLineConfigurationMock.Protected()
			.Setup<ZBool>("MethodOfPaymentVisibleOnImportControlCore", ItExpr.IsAny<CargoWise.EntityFramework.BusinessObject>())
			.Returns(true);

		using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(declaration.Factory, "GetNewInvoiceLineConfiguration", invoiceLineConfigurationMock.Object, declaration.GetDefaultDataGroupingCode()))
		{
			AssertMethodOfPaymentDropEditVisibility(true, ControlDpiScalingHelper.NewScaledSize(939, 330, true));
		}
	}

	[RequiresSTA]
	public void TestCalculationBoxesVisibilityImportsDJC()
	{
		using (var control = new EUImportInvoiceLineUserControl())
		{
			control.JobDeclaration = declaration;
			control.Show();
			var statValueBox = control.Controls.Find("StatisticalValueLocalCurrencyControl", true)[0];
			Assert(statValueBox != null && statValueBox.Visible);

			var customsValueBox = control.Controls.Find("CustomsValueLocalCurrencyControl", true)[0];
			Assert(customsValueBox != null && customsValueBox.Visible);

			var vatValueBox = control.Controls.Find("ValueForGstVatLocalCurrencyControl", true)[0];
			Assert(vatValueBox != null && vatValueBox.Visible);

			var cifValueBox = control.Controls.Find("JI_Calc_CIFConvertToLocalCurrencyControl", true)[0];
			Assert(cifValueBox != null && cifValueBox.Visible);
		}
	}

	[RequiresSTA]
	public void TestGridLayoutContext()
	{
		using (var control = new EUImportInvoiceLineUserControl())
		{
			AssertEquals("context is set", nameof(Customs.GUI.DeclarationType.Import), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
		}
	}

	[RequiresSTA]
	public void TestDefaultColumns()
	{
		using (var control = new EUImportInvoiceLineUserControl())
		{
			control.JobDeclaration = declaration;
			control.InitializeGridLayout();

			var lineGrid = control.CustomsInvoiceLinesBoundGrid;
			var styles = lineGrid.ColumnStyles;

			AssertEquals(0, styles.IndexOf(lineGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_LineNo)));
			AssertEquals(1, styles.IndexOf(lineGrid.GetColumnStyle(Customs.Business.BaseJobComInvoiceLine.Schema.JI_Calc_Invoice)));
			AssertEquals(2, styles.IndexOf(lineGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_CEI)));
			AssertEquals(3, styles.IndexOf(lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.EntryInstructionDescription)));
			AssertEquals(4, styles.IndexOf(lineGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_PartNo)));
			AssertEquals(5, styles.IndexOf(lineGrid.GetColumnStyle(Customs.Business.BaseJobComInvoiceLine.Schema.JI_FormattedTariff)));
			AssertEquals(6, styles.IndexOf(lineGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_Description)));
			AssertEquals(7, styles.IndexOf(lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_FormattedProcedure)));
			AssertEquals(8, styles.IndexOf(lineGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_InvoiceQuantity)));
			AssertEquals(9, styles.IndexOf(lineGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_CountryOfOrigin)));
			AssertEquals(10, styles.IndexOf(lineGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_PrimaryPreference)));
			AssertEquals(11, styles.IndexOf(lineGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_CustomsQuantity)));
			AssertEquals(12, styles.IndexOf(lineGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_LinePrice)));
			AssertEquals(13, styles.IndexOf(lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_SupplementaryCode1)));
			AssertEquals(14, styles.IndexOf(lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_SupplementaryCode2)));
			AssertEquals(15, styles.IndexOf(lineGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_CustomsSecondQuantity)));
			AssertEquals(16, styles.IndexOf(lineGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_CustomsSecondUnitQty)));
			AssertEquals(17, styles.IndexOf(lineGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_ZZF_NKTaxType)));
			AssertEquals(18, styles.IndexOf(lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.EntryReferenceNumber)));
			AssertEquals(19, styles.IndexOf(lineGrid.GetColumnStyle(Customs.Business.BaseJobComInvoiceLine.Schema.MergedLineNumber)));
			AssertEquals(20, styles.IndexOf(lineGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_BondedWhsQuantity)));
			AssertEquals(21, styles.IndexOf(lineGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_BondedWhsUnitQty)));
			AssertEquals(63, styles.IndexOf(lineGrid.GetColumnStyle(AutoJobComInvoiceLine.Schema.ZG_CountryOfDestination)));
			AssertEquals(56, styles.IndexOf(lineGrid.GetColumnStyle(AutoJobComInvoiceLine.Schema.ZG_CommercialReference)));

			AssertEquals("JI_LineNo is visible.", true, lineGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_LineNo).IsVisible);
			AssertEquals("JI_Calc_Invoice is visible.", true, lineGrid.GetColumnStyle(Customs.Business.BaseJobComInvoiceLine.Schema.JI_Calc_Invoice).IsVisible);
			AssertEquals("JI_CEI is visible.", true, lineGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_CEI).IsVisible);
			AssertEquals("EntryInstructionDescription is visible.", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.EntryInstructionDescription).IsVisible);
			AssertEquals("JI_PartNo is visible.", true, lineGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_PartNo).IsVisible);
			AssertEquals("JI_FormattedTariff is visible.", true, lineGrid.GetColumnStyle(Customs.Business.BaseJobComInvoiceLine.Schema.JI_FormattedTariff).IsVisible);
			AssertEquals("JI_Description is visible.", true, lineGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_Description).IsVisible);
			AssertEquals("JI_FormattedProcedure is visible.", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_FormattedProcedure).IsVisible);
			AssertEquals("JI_InvoiceQuantity is visible.", true, lineGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_InvoiceQuantity).IsVisible);
			AssertEquals("JI_CountryOfOrigin is visible.", true, lineGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_CountryOfOrigin).IsVisible);
			AssertEquals("JI_PrimaryPreference is visible.", true, lineGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_PrimaryPreference).IsVisible);
			AssertEquals("JI_CustomsQuantity is visible.", true, lineGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_CustomsQuantity).IsVisible);
			AssertEquals("JI_LinePrice is visible.", true, lineGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_LinePrice).IsVisible);
			AssertEquals("JI_SupplementaryCode1 is visible.", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_SupplementaryCode1).IsVisible);
			AssertEquals("JI_SupplementaryCode2 is visible.", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_SupplementaryCode2).IsVisible);
			AssertEquals("JI_CustomsSecondQuantity is visible.", true, lineGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_CustomsSecondQuantity).IsVisible);
			AssertEquals("JI_CustomsSecondUnitQty is visible.", true, lineGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_CustomsSecondUnitQty).IsVisible);
			AssertEquals("JI_ZZF_NKTaxType is visible.", true, lineGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_ZZF_NKTaxType).IsVisible);
			AssertEquals("MergedLineNumber is visible.", true, lineGrid.GetColumnStyle(Customs.Business.BaseJobComInvoiceLine.Schema.MergedLineNumber).IsVisible);
			AssertEquals("Bonded whs quantity is visible.", true, lineGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_BondedWhsQuantity).IsVisible);
			AssertEquals("Bonded whs unit quantity is visible.", true, lineGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_BondedWhsUnitQty).IsVisible);
			AssertEquals("Commercial Reference is not visible", false, lineGrid.GetColumnStyle(AutoJobComInvoiceLine.Schema.ZG_CommercialReference).IsVisible);
			AssertEquals("Country of destination is not visible", false, lineGrid.GetColumnStyle(AutoJobComInvoiceLine.Schema.ZG_CountryOfDestination).IsVisible);
		}
	}

	[RequiresSTA]
	public void TestValueAdjustmentCodeVisibility()
	{
		using (var control = new EUImportInvoiceLineUserControl())
		{
			control.JobDeclaration = declaration;
			control.InitializeGridLayout();
			control.Show();
			AssertEquals(true, control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(nameof(JobComInvoiceLine.ZG_ValueAdjustmentCode)).IsUnavailable);
			AssertEquals(false, control.FindSingle<ZDropEdit>("ValuationAdjustmentCodeDropEdit").Visible);
		}
	}

	public void TestCountryOfDestinationVisibility_Invisible()
	{
		AssertCountryOfDestinationCodeFindBox(false, false);
	}

	public void TestCountryOfDestinationVisibility_Visible()
	{
		ConfigurationTestHelper.ClearDeclarationConfiguration(declaration);

		var invoiceLineConfigurationMock = new Mock<InvoiceLineConfiguration>();
		invoiceLineConfigurationMock.Protected()
			.Setup<ZBool>("CountryOfDestinationVisibleOnImportControlCore", ItExpr.IsAny<CargoWise.EntityFramework.BusinessObject>())
			.Returns(true);

		using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(declaration.Factory, "GetNewInvoiceLineConfiguration", invoiceLineConfigurationMock.Object, declaration.GetDefaultDataGroupingCode()))
		{
			AssertCountryOfDestinationCodeFindBox(true, true);
		}
	}

	public void TestCountryOfDispatchVisibility_Invisible()
	{
		var invoice = declaration.Invoices.AddNew();
		invoice.InvoiceLines.AddNew();
		using (var form = new JobDeclarationForm(declaration))
		{
			form.Show();
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
			var control = (EUImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
			var bottomPanel = control.FindSingle<ZPanel>("BottomPanel");
			var lineDetailTabControl = bottomPanel.FindSingle<ZTemplateTabControl>("LineDetailTabControl");
			var lineDetailsTabPage = lineDetailTabControl.FindSingle<ZTabPage>("LineDetailsTabPage");
			lineDetailTabControl.SelectedTab = lineDetailsTabPage;

			var countryOfDispatchCodeFindBox = lineDetailsTabPage.FindSingle<ZCodeFindBox>("CountryOfDispatchCodeFindBox");
			Assert("CountryOfDispatchCodeFindBox is invisible in EU default.", !countryOfDispatchCodeFindBox.Visible);
		}
	}

	[RequiresSTA]
	public void TestCommercialReferenceVisibility()
	{
		using (var control = new EUImportInvoiceLineUserControl())
		{
			control.JobDeclaration = declaration;
			control.Show();
			var invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var commercialReferenceBox = control.Controls.Find("CommercialReferenceBox", true)[0];
			AssertNotNull("For Import Declaration, Commercial Reference must be available on the screen", commercialReferenceBox);
			Assert("Commercial Reference should not be visible by default on Import Declaration", !commercialReferenceBox.Visible);
		}
	}

	[RequiresSTA]
	public void TestOrganizationsUserControlType()
	{
		using (var control = new EUImportInvoiceLineUserControlForTest())
		{
			AssertEquals(typeof(ImportInvoiceLineOrganizationsUserControl), control.GetOrganizationsUserControlTypeExposed());
		}
	}

	[RequiresSTA]
	public void TestAdditionalInfosTabPageCaptionAndUserControlType()
	{
		var declaration = Factory.New<JobDeclaration>();
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
		{
			EUDynamicControlTestHelper.AssertEUInvoiceLineUserControlTabPageCaptionAndUserControlType<EUImportInvoiceLineUserControl>(declaration, "AdditionalInfosTabPage", "additionalInfosUserControl1", "Additional Documents", typeof(AdditionalInfosUserControlWithGrid));
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
		{
			EUDynamicControlTestHelper.AssertEUInvoiceLineUserControlTabPageCaptionAndUserControlType<EUImportInvoiceLineUserControl>(declaration, "AdditionalInfosTabPage", "additionalInfosUserControl1", "[44] Additional Info", typeof(AdditionalInfosUserControl));
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
	}
	JobDeclaration declaration;

	void AssertMethodOfPaymentDropEditVisibility(ZBool visible, System.Drawing.Size autoscrollSize)
	{
		var invoice = declaration.Invoices.AddNew();
		invoice.InvoiceLines.AddNew();
		using (var form = new JobDeclarationForm(declaration))
		{
			form.Show();
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
			var control = (EUImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
			var bottomPanel = control.FindSingle<ZPanel>("BottomPanel");
			var lineDetailTabControl = bottomPanel.FindSingle<ZTemplateTabControl>("LineDetailTabControl");
			var lineDetailsTabPage = lineDetailTabControl.FindSingle<ZTabPage>("LineDetailsTabPage");
			lineDetailTabControl.SelectedTab = lineDetailsTabPage;
			var invoiceDetailsGroupBox = lineDetailsTabPage.FindSingle<ZGroupBox>("InvoiceDetailsGroupBox");
			var methodOfPaymentDropEdit = invoiceDetailsGroupBox.FindSingle<ZDropEdit>("MethodOfPaymentDropEdit");
			CombineAssertions(() =>
			{
				AssertEquals("MethodOfPaymentDropEdit.Visible", visible, methodOfPaymentDropEdit.Visible);
				AssertEquals("lineDetailsTabPage.AutoScrollMinSize", autoscrollSize, lineDetailsTabPage.AutoScrollMinSize);
			});
		}
	}

	void AssertCountryOfDestinationCodeFindBox(ZBool controlVisible, ZBool columnAvailable)
	{
		var invoice = declaration.Invoices.AddNew();
		invoice.InvoiceLines.AddNew();
		using (var form = new JobDeclarationForm(declaration))
		{
			form.Show();
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
			var control = (EUImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
			var bottomPanel = control.FindSingle<ZPanel>("BottomPanel");
			var lineDetailTabControl = bottomPanel.FindSingle<ZTemplateTabControl>("LineDetailTabControl");
			var lineDetailsTabPage = lineDetailTabControl.FindSingle<ZTabPage>("LineDetailsTabPage");
			lineDetailTabControl.SelectedTab = lineDetailsTabPage;
			var countryOfDestinationCodeFindBox = lineDetailsTabPage.FindSingle<ZCodeFindBox>("CountryOfDestinationCodeFindBox");
			CombineAssertions(() =>
			{
				AssertEquals("CountryOfDestinationCodeFindBox.Visible", controlVisible, countryOfDestinationCodeFindBox.Visible);
				AssertEquals("CountryOfDestinationGridColumn.Visible", columnAvailable, !control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(nameof(JobComInvoiceLine.ZG_CountryOfDestination)).IsUnavailable);
			});
		}
	}

	[RequiresSTA]
	public void TestColumnStyles()
	{
		using (var form = new ZForm())
		using (var control = new EUImportInvoiceLineUserControl())
		{
			form.Controls.Add(control);
			form.Show();
			control.InitializeGridLayout();

			var prevEntryNoCol = control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_PreviousEntryNumber);
			AssertNotNull(prevEntryNoCol);
			AssertEquals(false, prevEntryNoCol.IsVisible);
			AssertEquals("Prev. Entry No.", prevEntryNoCol.GroupName.Caption);

			var prevEntryLineNoCol = control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(Customs.Business.AutoJobComInvoiceLine.Schema.JI_PreviousEntryLineNumber);
			AssertNotNull(prevEntryLineNoCol);
			AssertEquals(false, prevEntryLineNoCol.IsVisible);
			AssertEquals("Prev. Entry No.", prevEntryLineNoCol.GroupName.Caption);
		}
	}

	class EUImportInvoiceLineUserControlForTest : EUImportInvoiceLineUserControl
	{
		public Type GetOrganizationsUserControlTypeExposed() => base.GetOrganizationsUserControlType();
	}

	public void TestGDMLink()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		declaration.Invoices.AddNew().InvoiceLines.AddNew();

		using (var form = new JobDeclarationForm(declaration))
		{
			form.Show();
			var brokerageControl = form.CustomsBrokerageUserControl;
			brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
			using (var control = (EUImportInvoiceLineUserControl)brokerageControl.InvoiceLinesUserControl)
			{
				CombineAssertions(() =>
				{
					var link = control.FindSingle<ZLinkLabel>("GDMLink");
					Assert("GDMLink is visible", link.Visible);
					AssertEquals("Text should be GDM", "GDM", link.Text);
					link.PerformClick_ForTest();

					AssertType("LastFormShown type", typeof(GuidedDecisionMakingForm), ZFormModaliser.LastFormShownDialogForTest);
				});

				ZFormModaliser.LastFormShownDialogForTest = null;
				ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
			}
		}
	}
}
