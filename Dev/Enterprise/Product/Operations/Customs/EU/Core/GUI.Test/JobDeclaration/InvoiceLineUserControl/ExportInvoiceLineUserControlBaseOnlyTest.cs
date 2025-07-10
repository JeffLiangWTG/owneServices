using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	public abstract class ExportInvoiceLineUserControlTest<T> : Customs.GUI.Testing.BaseInvoiceLineUserControlForVirtualPropertiesTest<T>
		where T : EUExportInvoiceLineUserControl, new()
	{
		protected override ZString DefaultUniversalTariffType => Universal.Constants.TariffTypes.Export;
	}

	sealed class ExportInvoiceLineUserControlBaseOnlyTest : ExportInvoiceLineUserControlTest<EUExportInvoiceLineUserControl>
	{
		[RequiresSTA]
		public void TestGDMLinkVisibleByRegistry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();

			using (var form = new ZForm(declaration))
			using (var control = new EUExportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				var gdmLink = control.FindSingle<ZLinkLabel>("GDMLink");
				Assert("GDM Link should be visible if GDM registry is turned on", gdmLink.Visible);
				gdmLink.PerformClick_ForTest();

				var gDMForm = ZFormModaliser.LastFormShownDialogForTest;
				CombineAssertions("GDM Form should be created and shown", () =>
				{
					AssertNotNull("GDM Form created", gDMForm);
					AssertType<GuidedDecisionMakingForm>(gDMForm);
				});
			}
		}

		[RequiresSTA]
		public void TestCalculationBoxesVisibilityExportsDJC()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Business.MessageTypeList.Codes.Export;
			using (var control = new EUExportInvoiceLineUserControl())
			{
				control.JobDeclaration = dec;
				control.Show();
				var statValueBox = control.Controls.Find("StatisticalValueLocalCurrencyControl", true)[0];
				Assert(statValueBox != null && statValueBox.Visible);

				var statManualValue = control.Controls.Find("StatisticalValueCalcEdit", true)[0];
				Assert(statManualValue == null || !statManualValue.Visible);
				var statManualOverrideValue = control.Controls.Find("StatValueManualOverrideCheckBox", true)[0];
				Assert(statManualOverrideValue == null || !statManualValue.Visible);
			}
		}

		[RequiresSTA]
		public void TestGridLayoutContext()
		{
			using (EUExportInvoiceLineUserControl control = new EUExportInvoiceLineUserControl())
			{
				AssertEquals("context is set", nameof(Customs.GUI.DeclarationType.Export), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestNoInvoiceLinesPressingButtonGivesWarning()
		{
			using (ZGrid grid = new ZGrid())
			{
				AssertEquals(-1, grid.CurrentRowIndex);
				EUExportInvoiceLineUserControl.SetSpoffOnLine(grid);
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("You need to enter an invoice line before you can assign a Supervising Office to it"));
			}
		}

		[RequiresSTA]
		public void TestCommercialReferenceVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var control = new EUExportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				control.Show();
				var commercialReferenceBox = control.Controls.Find("CommercialReferenceBox", true)[0];
				AssertNotNull("For Export Declaration, Commercial Reference must be available on the screen", commercialReferenceBox);
				Assert("Commercial Reference should not be visible by default on Export Declaration", !commercialReferenceBox.Visible);
			}
		}

		[RequiresSTA]
		public void TestDefaultColumns()
		{
			using (var control = new EUExportInvoiceLineUserControl())
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();

				var lineGrid = control.CustomsInvoiceLinesBoundGrid;
				var styles = lineGrid.ColumnStyles;

				AssertEquals(18, styles.IndexOf(lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.ZG_CountryOfDestination)));
				AssertEquals(53, styles.IndexOf(lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.ZG_CommercialReference)));
				AssertEquals("Country of destination should be visible in grid for Export Declarations", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.ZG_CountryOfDestination).IsVisible);
				AssertEquals("Commercial Reference should not be visible by default in grid for Export Declarations", false, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.ZG_CommercialReference).IsVisible);
				AssertEquals("JI_CEI is visible.", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CEI).IsVisible);
				AssertEquals("entry instruction is visible.", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.EntryInstructionDescription).IsVisible);
			}
		}

		[RequiresSTA]
		public void TestColumnStyles()
		{
			using (var form = new ZForm())
			using (var control = new EUExportInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.InitializeGridLayout();

				var prevEntryNoCol = control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_PreviousEntryNumber);
				AssertNotNull(prevEntryNoCol);
				AssertEquals(false, prevEntryNoCol.IsVisible);
				AssertEquals("Prev. Entry No.", prevEntryNoCol.GroupName.Caption);

				var prevEntryLineNoCol = control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_PreviousEntryLineNumber);
				AssertNotNull(prevEntryLineNoCol);
				AssertEquals(false, prevEntryLineNoCol.IsVisible);
				AssertEquals("Prev. Entry No.", prevEntryLineNoCol.GroupName.Caption);
			}
		}

		[RequiresSTA]
		public void TestCusNumberVisibility_IsUcc6() => AssertCusNumberVisibility(true, true);

		[RequiresSTA]
		public void TestCusNumberVisibility_IsNotUcc6() => AssertCusNumberVisibility(false, false);

		public void TestCountryOfDestinationVisibility_Visible()
		{
			AssertCountryOfDestinationColumn(true);
		}

		public void TestCountryOfDestinationVisibility_Invisible()
		{
			var invoiceLineConfigurationMock = new Mock<InvoiceLineConfiguration>();
			invoiceLineConfigurationMock.Protected()
				.Setup<ZBool>("CountryOfDestinationVisibleOnExportControlCore", ItExpr.IsAny<CargoWise.EntityFramework.BusinessObject>())
				.Returns(false);

			using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(Factory, "GetNewInvoiceLineConfiguration", invoiceLineConfigurationMock.Object, null))
			{
				AssertCountryOfDestinationColumn(false);
			}
		}

		[RequiresSTA]
		public void TestAdditionalInfosTabPageCaptionAndUserControlType()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				EUDynamicControlTestHelper.AssertEUInvoiceLineUserControlTabPageCaptionAndUserControlType<EUExportInvoiceLineUserControl>(declaration, "AdditionalInfosTabPage", "additionalInfosUserControl1", "Additional Documents", typeof(AdditionalInfosUserControlWithGrid));
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
			{
				EUDynamicControlTestHelper.AssertEUInvoiceLineUserControlTabPageCaptionAndUserControlType<EUExportInvoiceLineUserControl>(declaration, "AdditionalInfosTabPage", "additionalInfosUserControl1", "[44] Additional Info", typeof(AdditionalInfosUserControl));
			}
		}

		[RequiresSTA]
		public void TestDangerousGoodsTabPage_Controls()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			using (var control = new EUExportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();
				control.FindSingle<ZTabPage>("DangerousGoodsTabPage").Show();

				DynamicLayoutPanelTest.AssertControlsOrder(control.UNDGPanel,
					nameof(UNDGUserControlBag.DGGuidFindBox),
					nameof(UNDGUserControlBag.FlashpointUserControl),
					nameof(UNDGUserControlBag.DGLinkLabel));
			}
		}

		void AssertCountryOfDestinationColumn(ZBool controlVisible)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Business.MessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var control = (EUExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var bottomPanel = control.FindSingle<ZPanel>("BottomPanel");
				var lineDetailTabControl = bottomPanel.FindSingle<ZTemplateTabControl>("LineDetailTabControl");
				var lineDetailsTabPage = lineDetailTabControl.FindSingle<ZTabPage>("LineDetailsTabPage");
				lineDetailTabControl.SelectedTab = lineDetailsTabPage;
				var countryOfDestinationColumn = control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(nameof(JobComInvoiceLine.ZG_CountryOfDestination));
				var countryOfDestinationCodeFindBox = lineDetailsTabPage.FindSingle<ZCodeFindBox>("DestinationCodeFindBox");
				CombineAssertions(() =>
				{
					AssertEquals("CountryOfDestination Column - IsVisible", controlVisible, countryOfDestinationColumn.IsVisible);
					AssertEquals("CountryOfDestination Column - IsAvailable", controlVisible, !countryOfDestinationColumn.IsUnavailable);
					AssertEquals("CountryOfDestination CodeFindBox - Visible", controlVisible, countryOfDestinationCodeFindBox.Visible);
				});
			}
		}

		void AssertCusNumberVisibility(bool isUcc6, bool visible)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			declaration.Invoices.AddNew();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUcc6))
			{
				using (var form = new ZForm(declaration))
				{
					using (var control = new EUExportInvoiceLineUserControl())
					{
						control.JobDeclaration = declaration;
						form.Controls.Add(control);
						control.InitializeGridLayout();
						form.Show();
						var columnStyle = control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.ZG_CusNumber);
						CombineAssertions(() =>
						{
							AssertEquals("IsVisible", visible, columnStyle.IsVisible);
							AssertEquals("IsUnavailable", !visible, columnStyle.IsUnavailable);
						});
					}
				}
			}
		}
	}
}
