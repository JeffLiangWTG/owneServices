using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(EUCustomsSupplierHeaderUserControl))]
	class EUCustomsSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<EUCustomsSupplierHeaderUserControl, JobDeclaration>
	{
		protected override IEnumerable<string> ExpectedControlList => DefaultControlList;

		public void TestInvoicePaymentTabPageVisibilityAndUserControlType()
		{
			ConfigurationTestHelper.ClearDeclarationConfiguration(declaration);

			var invoiceHeaderConfigurationMock = new Mock<InvoiceHeaderConfiguration>();
			invoiceHeaderConfigurationMock.CallBase = true;
			invoiceHeaderConfigurationMock.Protected()
				.Setup<ZBool>("InvoicePaymentSupportCore", ItExpr.IsAny<BusinessObject>())
				.Returns(true);

			using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(Factory, "GetNewInvoiceHeaderConfiguration", invoiceHeaderConfigurationMock.Object, null))
			using (var form = new ZForm(declaration))
			using (var control = new EUCustomsSupplierHeaderUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var invoicePaymentTabPage = control.FindSingle<ZTabPage>("InvoicePaymentTabPage");
				AssertEquals("InvoicePaymentTabPage is visible due to the configuration.", true, invoicePaymentTabPage.TabVisible);

				invoicePaymentTabPage.NotifyBindingOrShowing();
				Application.DoEvents();
				var invoicePaymentUserControl1 = control.FindSingle<ZDynamicControlCreationUserControl>("invoicePaymentUserControl1");
				AssertEquals(typeof(InvoicePaymentUserControl), invoicePaymentUserControl1.UserControlType);
			}
		}

		public void TestJZ_OH_SupplierNotRemoved()
		{
			using (var control = new EUCustomsSupplierHeaderUserControl())
			{
				control.InitializeGridLayout();
				var columnStyle = control.JobComInvoiceHeadersBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>().SingleOrDefault(x => x.ColumnName == JobComInvoiceHeader.Schema.JZ_OH_Supplier);
				AssertNotNull("JZ_OH_Supplier Column should exist", columnStyle);
			}
		}

		public void TestJZ_OH_BuyerRemoved()
		{
			using (var control = new EUCustomsSupplierHeaderUserControl())
			{
				AssertColumnIsRemoved(control, "JZ_OH_Buyer");
			}
		}

		[RequiresSTA]
		public void TestInitTabsVisibility()
		{
			using (var form = new ZForm(declaration))
			using (var control = new EUCustomsSupplierHeaderUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var supportingDocument = control.FindSingle<ZTabPage>("SupportingDocumentsTabPage");
				var additionalInfo = control.FindSingle<ZTabPage>("AdditionalInfoTabPage");
				var previousDocument = control.FindSingle<ZTabPage>("PreviousDocumentsTabPage");
				var valueIndicators = control.FindSingleOrDefault<ZTabPage>("ValueIndicatorsTabPage");
				var invoicePaymentTabPage = control.FindSingleOrDefault<ZTabPage>("InvoicePaymentTabPage");
				CombineAssertions(() =>
				{
					AssertEquals("SupportingDocumentsTabPage", true, supportingDocument.TabVisible);
					AssertEquals("AdditionalInfoTabPage", true, additionalInfo.TabVisible);
					AssertEquals("PreviousDocumentsTabPage", true, previousDocument.TabVisible);
					AssertNull("ValueIndicatorsTabPage is not visible when init.", valueIndicators);
					AssertNull("invoicePaymentTabPage is not visible when init.", invoicePaymentTabPage);
				});
			}
		}

		public void TestValueIndicatorsTabPageVisibilityAndCaption()
		{
			ConfigurationTestHelper.ClearDeclarationConfiguration(declaration);

			var invoiceHeaderConfigurationMock = new Mock<InvoiceHeaderConfiguration>();
			invoiceHeaderConfigurationMock.Protected()
				.Setup<ZBool>("ValueIndicatorsSupportCore", ItExpr.IsAny<BusinessObject>())
				.Returns(true);

			using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(Factory, "GetNewInvoiceHeaderConfiguration", invoiceHeaderConfigurationMock.Object, null))
			using (var form = new ZForm(declaration))
			using (var control = new EUCustomsSupplierHeaderUserControl())
			{
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();
				var valueIndicatorsTabPage = control.FindSingle<ZTabPage>("ValueIndicatorsTabPage");
				AssertEquals("ValueIndicatorsTabPage is visible due to the configuration.", true, valueIndicatorsTabPage.TabVisible);
				AssertEquals("ValueIndicatorsTabPage caption", "[UCC 4/13] Valuation Indicators", valueIndicatorsTabPage.Text);
			}
		}

		public void TestValueIndicatorsUserControlType()
		{
			ConfigurationTestHelper.ClearDeclarationConfiguration(declaration);

			var invoiceHeaderConfigurationMock = new Mock<InvoiceHeaderConfiguration>();
			invoiceHeaderConfigurationMock.Protected()
				.Setup<ZBool>("ValueIndicatorsSupportCore", ItExpr.IsAny<BusinessObject>())
				.Returns(true);

			using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(Factory, "GetNewInvoiceHeaderConfiguration", invoiceHeaderConfigurationMock.Object, null))
			using (var form = new ZForm(declaration))
			using (var control = new EUCustomsSupplierHeaderUserControl())
			{
				declaration.Invoices.AddNew();

				form.Controls.Add(control);
				form.Show();
				var valueIndicatorsTabPage = control.FindSingle<ZTabPage>("ValueIndicatorsTabPage");
				valueIndicatorsTabPage.NotifyBindingOrShowing();
				Application.DoEvents();
				var valueIndicatorsUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("ValueIndicatorsUserControl");
				AssertEquals(typeof(ValueIndicatorsUserControl), valueIndicatorsUserControl.UserControlType);
			}
		}

		[RequiresSTA]
		public void TestValueIndicatorsColumns_Available()
		{
			ConfigurationTestHelper.ClearDeclarationConfiguration(declaration);

			var invoiceHeaderConfigurationMock = new Mock<InvoiceHeaderConfiguration>();
			invoiceHeaderConfigurationMock.Protected()
				.Setup<ZBool>("ValueIndicatorsSupportCore", ItExpr.IsAny<BusinessObject>())
				.Returns(true);

			using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(Factory, "GetNewInvoiceHeaderConfiguration", invoiceHeaderConfigurationMock.Object, null))
			{
				AssertValueIndicatorsColumnsAvailable(true);
			}
		}

		public void TestValueIndicatorsColumns_Unavailable()
		{
			ConfigurationTestHelper.ClearDeclarationConfiguration(declaration);

			var invoiceHeaderConfigurationMock = new Mock<InvoiceHeaderConfiguration>();
			invoiceHeaderConfigurationMock.Protected()
				.Setup<ZBool>("ValueIndicatorsSupportCore", ItExpr.IsAny<BusinessObject>())
				.Returns(false);

			using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(Factory, "GetNewInvoiceHeaderConfiguration", invoiceHeaderConfigurationMock.Object, null))
			{
				AssertValueIndicatorsColumnsAvailable(false);
			}
		}

		public void TestInvoiceChargesGrid_HasApplicableForStatisticalValueColumn()
		{
			using (var form = new ZForm())
			using (var userControl = new EUCustomsSupplierHeaderUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				userControl.InitializeGridLayout();

				var column = userControl.InvoiceChargesGrid.ColumnStyles.OfType<ZCheckBoxColumnStyleInfo>().FirstOrDefault(c => c.ColumnName == InvoiceCharge.Schema.J7_IsStatisticalValueApplicable);
				AssertNotNull("Statistical value applicable column must be available", column);
			}
		}

		public void TestBaseGroupChargesGrid_HasApplicableForStatisticalValueColumn()
		{
			using (var form = new ZForm())
			using (var userControl = new EUCustomsSupplierHeaderUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				userControl.InitializeGridLayout();

				var column = userControl.BaseGroupChargesGrid.ColumnStyles.OfType<ZCheckBoxColumnStyleInfo>().FirstOrDefault(c => c.ColumnName == InvoiceCharge.Schema.J7_IsStatisticalValueApplicable);
				AssertNotNull("Statistical value applicable column must be available", column);
			}
		}

		public void TestColumnTypes()
		{
			using (var uc = new EUCustomsSupplierHeaderUserControl())
			{
				uc.InitializeGridLayout();
				var map = new Dictionary<string, Type>();
				map.Add("J7_ExchangeRate", typeof(ZCalcEditColumnStyleInfo));
				map.Add("IsJ7_ExchangeRateUserEnterable", typeof(ZCheckBoxColumnStyleInfo));

				foreach (var colName in map.Keys)
				{
					var columnStyle = (from ZGridColumnInfo col in uc.ApportionedChargesGrid.ColumnStyles.ToArray() where col.ColumnName == colName select col).FirstOrDefault();
					Type t = map[colName];
					AssertType(string.Format("ApportionedChargesGrid column of name {0} should have a style of type {1}", colName, t.Name), t, columnStyle);

					columnStyle = (from ZGridColumnInfo col in uc.InvoiceChargesGrid.ColumnStyles.ToArray() where col.ColumnName == colName select col).FirstOrDefault();
					t = map[colName];
					AssertType(string.Format("InvoiceChargesGrid column of name {0} should have a style of type {1}", colName, t.Name), t, columnStyle);

					columnStyle = (from ZGridColumnInfo col in uc.BaseGroupChargesGrid.ColumnStyles.ToArray() where col.ColumnName == colName select col).FirstOrDefault();
					t = map[colName];
					AssertType(string.Format("BaseGroupChargesGrid column of name {0} should have a style of type {1}", colName, t.Name), t, columnStyle);
				}
			}
		}

		public void TestColumnAndGridVisibilityForCharges()
		{
			using (var form = new ZForm())
			using (var userControl = new EUCustomsSupplierHeaderUserControl())
			{
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				var dutiableColumn = userControl.BaseGroupChargesGrid.ColumnStyles.OfType<ZCheckBoxColumnStyleInfo>().FirstOrDefault(c => c.ColumnName == InvoiceCharge.Schema.J7_IsDutiable);
				AssertNotNull("Dutiable value applicable column must be available", dutiableColumn);

				var vatApplyColumn = userControl.BaseGroupChargesGrid.ColumnStyles.OfType<ZCheckBoxColumnStyleInfo>().FirstOrDefault(c => c.ColumnName == InvoiceCharge.Schema.J7_IsGSTApplicable);
				Assert("VAT apply is not null or invisible", vatApplyColumn != null && vatApplyColumn.IsVisible);
			}
		}

		public void TestColumnVisibility()
		{
			using (var form = new ZForm())
			using (var userControl = new EUCustomsSupplierHeaderUserControl())
			{
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();
				userControl.InitializeGridLayout();

				var invoiceDateColumn = userControl.JobComInvoiceHeadersBoundGrid.ColumnStyles.OfType<ZGridColumnInfo>().FirstOrDefault(c => c.ColumnName == Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_InvoiceDate);
				CombineAssertions("Invoice Date Column ", () =>
				{
					AssertEquals("IsVisible", true, invoiceDateColumn.IsVisible);
					AssertEquals("Order", 1, userControl.JobComInvoiceHeadersBoundGrid.ColumnStyles.IndexOf(invoiceDateColumn));
				});

				var invoiceValuationDateColumn = userControl.JobComInvoiceHeadersBoundGrid.ColumnStyles.OfType<ZGridColumnInfo>().FirstOrDefault(c => c.ColumnName == Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_ValuationDateOverride);
				CombineAssertions("Invoice Valuation Date Column ", () =>
				{
					AssertEquals("IsVisible", true, invoiceValuationDateColumn.IsVisible);
					AssertEquals("Order", 40, userControl.JobComInvoiceHeadersBoundGrid.ColumnStyles.IndexOf(invoiceValuationDateColumn));
				});
			}
		}

		public void TestChargesGridColumnWidth()
		{
			using (var control = new EUCustomsSupplierHeaderUserControl())
			{
				control.InitializeGridLayout();
				var chargesGrid = control.InvoiceChargesGrid;
				CombineAssertions(() =>
				{
					AssertEquals("J7_IsStatisticalValueApplicable", 152, CargoWise.Windows.UI.ControlDpiScalingHelper.UnscaleFromCurrentDpiX(chargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_IsStatisticalValueApplicable).Width));
					AssertEquals("IsJ7_ExchangeRateUserEnterable", 74, CargoWise.Windows.UI.ControlDpiScalingHelper.UnscaleFromCurrentDpiX(chargesGrid.GetColumnStyle(InvoiceCharge.Schema.IsJ7_ExchangeRateUserEnterable).Width));
					AssertEquals("J7_ExchangeRate", 102, CargoWise.Windows.UI.ControlDpiScalingHelper.UnscaleFromCurrentDpiX(chargesGrid.GetColumnStyle(InvoiceCharge.Schema.J7_ExchangeRate).Width));
				});
			}
		}

		public void TestIncoTermTextBoxVisibility()
		{
			using (var invoiceHeaderUserControl = new EUExportSupplierHeaderUserControl())
			{
				var incoTermTextBox = invoiceHeaderUserControl.Controls.Find("IncoTermTextBox", true).FirstOrDefault();
				AssertNotNull(incoTermTextBox);
				Assert(!incoTermTextBox.Visible);
			}
		}

		public void TestCalculateFreightButtonsVisibility_IsAir()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			using (var form = new ZForm())
			using (var userControl = new EUCustomsSupplierHeaderUserControl())
			{
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				var invoiceChargesButtonPanel = userControl.FindSingle<ZPanel>("InvoiceChargesButtonPanel");
				var groupChargesButton = userControl.FindSingle<ZPanel>("GroupChargesButton");

				CombineAssertions(() =>
				{
					AssertEquals("InvoiceChargesButtonPanel Import", true, invoiceChargesButtonPanel.Visible);
					AssertEquals("GroupChargesButton Import", true, groupChargesButton.Visible);
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					AssertEquals("InvoiceChargesButtonPanel Export", true, invoiceChargesButtonPanel.Visible);
					AssertEquals("GroupChargesButton Export", true, groupChargesButton.Visible);
				});
			}
		}

		public void TestInvoiceChargesButtonPanelVisibility_Insurance_Import() => AssertInvoiceChargesButtonPanelVisibility_Insurance(JobMessageTypeList.Codes.Import, true);

		public void TestInvoiceChargesButtonPanelVisibility_Insurance_Export() => AssertInvoiceChargesButtonPanelVisibility_Insurance(JobMessageTypeList.Codes.Export, false);

		public void TestInsuranceAndFreightButtonsVisible_ImportAir()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			jobDeclaration.Invoices.AddNew();

			using (var form = new ZForm())
			using (var userControl = new EUCustomsSupplierHeaderUserControl())
			{
				userControl.JobDeclaration = jobDeclaration;
				form.Controls.Add(userControl);
				form.Show();

				var invoiceChargesButtonPanel = userControl.FindSingle<ZPanel>("InvoiceChargesButtonPanel");
				var freightButton = userControl.FindSingle<ZButton>("InvoiceChargesCalculateFreightButton");
				var insuranceButton = userControl.FindSingle<ZButton>("InvoiceChargesCalculateInsuranceButton");

				CombineAssertions(() =>
				{
					AssertEquals("Panel should be visible", true, invoiceChargesButtonPanel.Visible);
					AssertEquals("Freight button should be visible IMP and TransportMode == AIR", true, freightButton.Visible);
					AssertEquals("Freight button should be enabled IMP and TransportMode == AIR", true, freightButton.Enabled);
					AssertEquals("Insurance Button should be visible, IMP", true, insuranceButton.Visible);
					AssertEquals("Insurance Button should be enabled, IMP", true, insuranceButton.Enabled);
				});
			}
		}

		public void TestCalculateFreightButtonsVisibility_FreightEnabledModesImport()
		{
			using (var form = new ZForm())
			using (var userControl = new EUCustomsSupplierHeaderUserControl())
			{
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				var invoiceChargesButtonPanel = userControl.FindSingle<ZPanel>("InvoiceChargesButtonPanel");
				var groupChargesButton = userControl.FindSingle<ZPanel>("GroupChargesButton");

				CombineAssertions(() =>
				{
					foreach (var transportMode in FreightEnabledTransportModes)
					{
						declaration.JE_TransportMode = transportMode;
						AssertEquals($"InvoiceChargesButtonPanel -> {transportMode}:", true, invoiceChargesButtonPanel.Visible);
						AssertEquals($"GroupChargesButton -> {transportMode}:", true, groupChargesButton.Visible);
					}

					var allEnabledModes = FreightEnabledTransportModes.ToList();
					allEnabledModes.Add(Core.Constants.TransportModes.Air);
					var freightDisabledTransportModes = declaration.Lookups.TransportTypeList.Cast<ICodeDescription>().Select(x => x.Code).Except(allEnabledModes);
					foreach (var transportMode in freightDisabledTransportModes)
					{
						declaration.JE_TransportMode = transportMode;
						AssertEquals("InvoiceChargesButtonPanel visible due to insurance", true, invoiceChargesButtonPanel.Visible);
						AssertEquals($"GroupChargesButton -> {transportMode}:", false, groupChargesButton.Visible);
					}
				});
			}
		}

		public void TestCalculateFreightButtonsVisibility_FreightEnabledModesExport()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new ZForm())
			using (var userControl = new EUCustomsSupplierHeaderUserControl())
			{
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				var invoiceChargesButtonPanel = userControl.FindSingle<ZPanel>("InvoiceChargesButtonPanel");
				var groupChargesButton = userControl.FindSingle<ZPanel>("GroupChargesButton");

				CombineAssertions(() =>
				{
					foreach (var transportMode in FreightEnabledTransportModes)
					{
						declaration.JE_TransportMode = transportMode;
						AssertEquals($"InvoiceChargesButtonPanel -> {transportMode}:", false, invoiceChargesButtonPanel.Visible);
						AssertEquals($"GroupChargesButton -> {transportMode}:", false, groupChargesButton.Visible);
					}
				});
			}
		}

		[RequiresSTA]
		public void TestShowCalculateFreightFormForAir()
		{
			var expectedFormType = typeof(CalculateFreightForm);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.Invoices.AddNew();

			using (var form = new ZForm())
			using (var userControl = new EUCustomsSupplierHeaderUserControl())
			{
				userControl.SetDataBinding(declaration, "");
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				var invoiceChargesCalculateFreightButton = userControl.FindSingle<ZButton>("InvoiceChargesCalculateFreightButton");
				var groupChargesCalculateFreightButton = userControl.FindSingle<ZButton>("GroupChargesCalculateFreightButton");

				CombineAssertions(() =>
				{
					AssertClickingButtonShowsForm("InvoiceChargesCalculateFreightButton Import", invoiceChargesCalculateFreightButton, expectedFormType);
					AssertClickingButtonShowsForm("GroupChargesCalculateFreightButton Import", groupChargesCalculateFreightButton, expectedFormType);
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					AssertClickingButtonShowsForm("InvoiceChargesCalculateFreightButton Export", invoiceChargesCalculateFreightButton, expectedFormType);
					AssertClickingButtonShowsForm("GroupChargesCalculateFreightButton Export", groupChargesCalculateFreightButton, expectedFormType);
				});
			}
		}

		public void TestShowCalculateInsuranceForm()
		{
			var expectedFormType = typeof(CalculateInsuranceForm);
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.Invoices.AddNew();

			using (var form = new ZForm())
			using (var userControl = new EUCustomsSupplierHeaderUserControl())
			{
				userControl.SetDataBinding(jobDeclaration, "");
				userControl.JobDeclaration = jobDeclaration;
				form.Controls.Add(userControl);

				form.Show();

				var calculateInsuranceFormButton = userControl.FindSingle<ZButton>("InvoiceChargesCalculateInsuranceButton");

				AssertClickingButtonShowsForm("Calculate insurance form should be shown", calculateInsuranceFormButton, expectedFormType);
			}
		}

		public void TestShowCalculateFreightFormForEnabledTransportModes()
		{
			var expectedFormType = typeof(CalculateFreightNonAirForm);
			declaration.Invoices.AddNew();

			using (var form = new ZForm())
			using (var userControl = new EUCustomsSupplierHeaderUserControl())
			{
				userControl.SetDataBinding(declaration, "");
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				var invoiceChargesCalculateFreightButton = userControl.FindSingle<ZButton>("InvoiceChargesCalculateFreightButton");
				var groupChargesCalculateFreightButton = userControl.FindSingle<ZButton>("GroupChargesCalculateFreightButton");

				CombineAssertions(() =>
				{
					foreach (var transportMode in FreightEnabledTransportModes)
					{
						declaration.JE_TransportMode = transportMode;
						AssertClickingButtonShowsForm($"InvoiceChargesCalculateFreightButton {transportMode}", invoiceChargesCalculateFreightButton, expectedFormType);
						AssertClickingButtonShowsForm($"GroupChargesCalculateFreightButton {transportMode}", groupChargesCalculateFreightButton, expectedFormType);
					}
				});
			}
		}

		public void TestInitAdditionalInfosUserControl()
		{
			using (var form = new ZForm())
			using (var userControl = new EUCustomsSupplierHeaderUserControl())
			{
				userControl.SetDataBinding(declaration, "");
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				var additionalInfoTab = userControl.FindSingle<ZTabPage>("AdditionalInfoTabPage");
				userControl.InvoiceTabControl.SelectedTab = additionalInfoTab;
				var grid = userControl.Controls.Find("AdditionalInfosGrid", true).First() as ZGrid;
				AssertEquals("DataMember for AdditionalInfosGrid", "Invoices.AdditionalInfos", grid.DataMember);
			}
		}

		public void TestInitPreviousDocumentsUserControl()
		{
			using (var form = new ZForm())
			using (var userControl = new EUCustomsSupplierHeaderUserControl())
			{
				userControl.SetDataBinding(declaration, "");
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				var previousDocumentsTab = userControl.FindSingle<ZTabPage>("PreviousDocumentsTabPage");
				userControl.InvoiceTabControl.SelectedTab = previousDocumentsTab;
				var grid = userControl.Controls.Find("PreviousDocumentsGrid", true).First() as ZGrid;
				AssertEquals("DataMember for PreviousDocumentsGrid", "Invoices.PreviousDocuments", grid.DataMember);
			}
		}

		public void TestInitSupportingDocumentsUserControl()
		{
			using (var form = new ZForm())
			using (var userControl = new EUCustomsSupplierHeaderUserControl())
			{
				userControl.SetDataBinding(declaration, "");
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				var supportingDocumentsTab = userControl.FindSingle<ZTabPage>("SupportingDocumentsTabPage");
				userControl.InvoiceTabControl.SelectedTab = supportingDocumentsTab;
				var grid = userControl.Controls.Find("SupportingDocumentsGrid", true).First() as ZGrid;
				AssertEquals("DataMember for SupportingDocumentsGrid", "Invoices.SupportingDocuments", grid.DataMember);
			}
		}

		public void TestGroupInvoiceColumnStyle_ShouldBeNotAvailable_WhenMessageTypeIsExport()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using var userControl = new EUCustomsSupplierHeaderUserControl();
			userControl.JobDeclaration = declaration;

			userControl.InitializeGridLayout();

			var groupInvoiceColumnStyle = userControl.InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_Calc_GroupInvoice);
			CombineAssertions(() =>
			{
				AssertEquals("JZ_Calc_GroupInvoice ColumnStyle.IsUnavailable", true, groupInvoiceColumnStyle.IsUnavailable);
				AssertEquals("JZ_Calc_GroupInvoice ColumnStyle.IsVisible", false, groupInvoiceColumnStyle.IsVisible);
			});
		}

		public void TestGroupInvoiceColumnStyle_ShouldBetAvailable_WhenMessageTypeIsImport()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using var userControl = new EUCustomsSupplierHeaderUserControl();
			userControl.JobDeclaration = declaration;

			userControl.InitializeGridLayout();

			var groupInvoiceColumnStyle = userControl.InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_Calc_GroupInvoice);
			CombineAssertions(() =>
			{
				AssertEquals("JZ_Calc_GroupInvoice ColumnStyle.IsUnavailable", false, groupInvoiceColumnStyle.IsUnavailable);
				AssertEquals("JZ_Calc_GroupInvoice ColumnStyle.IsVisible", true, groupInvoiceColumnStyle.IsVisible);
			});
		}

		public void TestChargeTotalsVisibility() => CombineAssertions(() =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertVisibility(true, true);
			AssertVisibility(false, false);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertVisibility(false, true);

			void AssertVisibility(bool expectedVisibility, ZBool exportCostCalculationsTotalsUISupport)
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetInvoiceHeaderConfiguration(declaration, nameof(InvoiceHeaderConfiguration.ExportCostCalculationsTotalsUISupport), exportCostCalculationsTotalsUISupport))
				{
					using var form = new ZForm();
					using var userControl = new EUCustomsSupplierHeaderUserControl();
					userControl.JobDeclaration = declaration;
					form.Controls.Add(userControl);
					form.Show();
					userControl.InitializeGridLayout();
					var assertionInfo = $"JE_MessageType={declaration.JE_MessageType} ExportCostCalculationsTotalsUISupport={exportCostCalculationsTotalsUISupport}";
					AssertEquals($"{assertionInfo} InvoiceChargesAddDeductTotal.Visible", expectedVisibility, userControl.InvoiceChargesAddDeductTotal.Visible);
					AssertEquals($"{assertionInfo} GroupChargesAddDeductTotal.Visible", expectedVisibility, userControl.GroupChargesAddDeductTotal.Visible);
					AssertEquals($"{assertionInfo} GroupInvoiceTotal.Visible", expectedVisibility, userControl.GroupInvoiceTotal.Visible);
				}
			}
		});

		public void TestNewLayout() => CombineAssertions(() =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertLayout(true, exportCostCalculationsTotalsUISupport: ZBool.True);
			AssertLayout(false, exportCostCalculationsTotalsUISupport: ZBool.False);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertLayout(false, ZBool.True);

			void AssertLayout(bool expectTotalsLayout, ZBool exportCostCalculationsTotalsUISupport)
			{
				var assertionInfo = $"JE_MessageType={declaration.JE_MessageType} ExportCostCalculationsTotalsUISupport={exportCostCalculationsTotalsUISupport} expectTotalsLayout={expectTotalsLayout}";
				var expectedDock = expectTotalsLayout ? DockStyle.None : DockStyle.Fill;
				var expectedAnchor = expectTotalsLayout ? AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right : AnchorStyles.Top | AnchorStyles.Left;
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetInvoiceHeaderConfiguration(declaration, nameof(InvoiceHeaderConfiguration.ExportCostCalculationsTotalsUISupport), exportCostCalculationsTotalsUISupport))
				{
					using var form = new ZForm();
					using var userControl = new EUCustomsSupplierHeaderUserControl();
					userControl.JobDeclaration = declaration;
					form.Controls.Add(userControl);
					form.Show();
					userControl.InitializeGridLayout();

					AssertEquals($"{assertionInfo} ChargesTabControl.Dock", expectedDock, userControl.ChargesTabControl.Dock);
					AssertEquals($"{assertionInfo} ChargesTabControl.Anchor", expectedAnchor, userControl.ChargesTabControl.Anchor);
					AssertEquals($"{assertionInfo} BaseGroupChargesGrid.Dock", expectedDock, userControl.BaseGroupChargesGrid.Dock);
					AssertEquals($"{assertionInfo} BaseGroupChargesGrid.Anchor", expectedAnchor, userControl.BaseGroupChargesGrid.Anchor);

					var chargesGroupsSplitterContainer = userControl.FindSingle<KSplitContainer>(x => x.Name == "ChargesGroupsSplitterContainer");
					var baseGroupChargesGroupBox = userControl.FindSingle<ZGroupBox>(x => x.Name == "BaseGroupChargesGroupBox");
					Control expectedChargesParent = expectTotalsLayout ? chargesGroupsSplitterContainer.Panel1 : userControl.ChargesGroupBox;
					Control expectedGroupChargesParent = expectTotalsLayout ? chargesGroupsSplitterContainer.Panel2 : baseGroupChargesGroupBox;

					AssertSame($"{assertionInfo} ChargesTabControl.Parent", expectedChargesParent, userControl.ChargesTabControl.Parent);
					AssertSame($"{assertionInfo} BaseGroupChargesGrid.Parent", expectedGroupChargesParent, userControl.BaseGroupChargesGrid.Parent);
					AssertSame($"{assertionInfo} GroupChargesButton.Parent", expectedGroupChargesParent, userControl.GroupChargesButton.Parent);
					if (expectTotalsLayout)
					{
						AssertEquals($"{assertionInfo} ChargesGroupBox.Visible", false, userControl.ChargesGroupBox.Visible);
						AssertEquals($"{assertionInfo} InvoiceChargesGroupLabel.Visible", true, userControl.InvoiceChargesGroupLabel.Visible);
						AssertSame($"{assertionInfo} InvoiceChargesGroupLabel.Parent", expectedChargesParent, userControl.InvoiceChargesGroupLabel.Parent);
						AssertSame($"{assertionInfo} ChargesTabControl.Parent", expectedChargesParent, userControl.InvoiceChargesGroupLabel.Parent);
						AssertSame($"{assertionInfo} InvoiceChargesAddDeductTotal.Parent", expectedChargesParent, userControl.InvoiceChargesAddDeductTotal.Parent);

						AssertEquals($"{assertionInfo} BaseGroupChargesGroupBox.Visible", false, baseGroupChargesGroupBox.Visible);
						AssertEquals($"{assertionInfo} GroupChargesGroupLabel.Visible", true, userControl.GroupChargesGroupLabel.Visible);
						AssertSame($"{assertionInfo} GroupChargesGroupLabel.Parent", expectedGroupChargesParent, userControl.GroupChargesGroupLabel.Parent);
						AssertSame($"{assertionInfo} ChargesTabControl.Parent", expectedGroupChargesParent, userControl.GroupChargesGroupLabel.Parent);
						AssertSame($"{assertionInfo} GroupInvoiceTotal.Parent", expectedGroupChargesParent, userControl.GroupInvoiceTotal.Parent);
						AssertSame($"{assertionInfo} GroupChargesAddDeductTotal.Parent", expectedGroupChargesParent, userControl.GroupChargesAddDeductTotal.Parent);
					}
					else
					{
						AssertEquals($"{assertionInfo} InvoiceChargesGroupLabel.Visible", true, userControl.ChargesGroupBox.Visible);
						AssertEquals($"{assertionInfo} InvoiceChargesGroupLabel.Parent", chargesGroupsSplitterContainer.Panel1, userControl.ChargesGroupBox.Parent);
						AssertEquals($"{assertionInfo} InvoiceChargesGroupLabel.Visible", false, userControl.InvoiceChargesGroupLabel.Visible);

						AssertEquals($"{assertionInfo} BaseGroupChargesGroupBox.Visible", true, baseGroupChargesGroupBox.Visible);
						AssertEquals($"{assertionInfo} BaseGroupChargesGroupBox.Parent", chargesGroupsSplitterContainer.Panel2, baseGroupChargesGroupBox.Parent);
						AssertEquals($"{assertionInfo} GroupChargesGroupLabel.Visible", false, userControl.GroupChargesGroupLabel.Visible);
					}
				}
			}
		});

		public void TestMultipleKeysToUse() => CombineAssertions(() =>
		{
			using var userControl = new EUCustomsSupplierHeaderUserControl();
			var resourceStringDataSupporter = userControl as ISupportMultipleResourceStringDataSupporter;
			userControl.JobDeclaration = declaration;

			AssertMultipleKeysToUse(JobMessageTypeList.Codes.Import, false, [JobDeclaration.CaptionKeySAD]);
			AssertMultipleKeysToUse(JobMessageTypeList.Codes.Export, false, [JobDeclaration.CaptionKeySAD]);
			AssertMultipleKeysToUse(JobMessageTypeList.Codes.Import, true, [JobDeclaration.CaptionKeySAD]);
			AssertMultipleKeysToUse(JobMessageTypeList.Codes.Export, true, [JobDeclaration.CaptionKeyChargesExport]);

			void AssertMultipleKeysToUse(string messageType, ZBool exportCostCalculationsTotalsUISupport, string[] expectedMultipleKeysToUse)
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetInvoiceHeaderConfiguration(declaration, nameof(InvoiceHeaderConfiguration.ExportCostCalculationsTotalsUISupport), exportCostCalculationsTotalsUISupport))
				{
					declaration.JE_MessageType = messageType;
					AssertContainsExactElementsInAnyOrder($"MessageType={messageType} ExportCostCalculationsTotalsUISupport={exportCostCalculationsTotalsUISupport}", expectedMultipleKeysToUse, resourceStringDataSupporter.SupportMultipleResourceStringData.MultipleKeysToUse);
				}
			}
		});

		public static void AssertColumnIsRemoved(EUCustomsSupplierHeaderUserControl control, string columnName)
		{
			control.InitializeGridLayout();
			Assert($"{columnName} must be removed!", control.JobComInvoiceHeadersBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>().All(x => x.ColumnName != columnName || x.IsUnavailable));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		}
		JobDeclaration declaration;

		static void AssertClickingButtonShowsForm(string message, ZButton button, Type formType)
		{
			button.PerformClick();
			AssertEquals(message, formType, ZFormModaliser.ActiveForm.GetType());
		}

		void AssertValueIndicatorsColumnsAvailable(bool available)
		{
			declaration.Invoices.AddNew();
			using (var form = new ZForm(declaration))
			using (var control = new EUCustomsSupplierHeaderUserControl())
			{
				form.Controls.Add(control);
				control.InitializeGridLayout();
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals("RelatedIndicator", available, !control.InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.RelatedIndicator).IsUnavailable);
					AssertEquals("ZG_RelatedIndicator2", available, !control.InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.ZG_RelatedIndicator2).IsUnavailable);
					AssertEquals("ZG_RelatedIndicator3", available, !control.InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.ZG_RelatedIndicator3).IsUnavailable);
					AssertEquals("ZG_RelatedIndicator4", available, !control.InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.ZG_RelatedIndicator4).IsUnavailable);
				});
			}
		}

		void AssertInvoiceChargesButtonPanelVisibility_Insurance(string messageType, bool expectButtonAndPanelVisible)
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = messageType;
			using (var form = new ZForm())
			using (var userControl = new EUCustomsSupplierHeaderUserControl())
			{
				userControl.JobDeclaration = jobDeclaration;
				form.Controls.Add(userControl);
				form.Show();

				var invoiceChargesButtonPanel = userControl.FindSingle<ZPanel>("InvoiceChargesButtonPanel");
				var insuranceButton = userControl.FindSingle<ZButton>("InvoiceChargesCalculateInsuranceButton");
				var freightButton = userControl.FindSingle<ZButton>("InvoiceChargesCalculateFreightButton");

				CombineAssertions(() =>
				{
					AssertEquals($"InvoiceChargesButtonPanel {messageType}", expectButtonAndPanelVisible, invoiceChargesButtonPanel.Visible);
					AssertEquals($"CalculateInsuranceButton enabled {messageType}", expectButtonAndPanelVisible, insuranceButton.Enabled);
					AssertEquals($"CalculateInsuranceButton visible {messageType}", expectButtonAndPanelVisible, insuranceButton.Visible);
					AssertEquals("Freight button not enabled, only Insurance", false, freightButton.Enabled);
					AssertEquals("Freight button not visible, only Insurance", false, freightButton.Visible);
				});
			}
		}

		string[] freightEnabledTransportModes;
		string[] FreightEnabledTransportModes => freightEnabledTransportModes ?? (freightEnabledTransportModes = new[]
		{
			Core.Constants.TransportModes.Sea,
			Core.Constants.TransportModes.InlandWaterwayTransport,
			Core.Constants.TransportModes.Rail,
			Core.Constants.TransportModes.Road
		});
	}
}
