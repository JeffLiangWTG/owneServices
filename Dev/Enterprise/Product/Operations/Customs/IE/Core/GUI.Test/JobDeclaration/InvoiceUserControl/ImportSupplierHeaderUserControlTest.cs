using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.EU.GUI.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(ImportSupplierHeaderUserControl))]
	sealed class ImportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<ImportSupplierHeaderUserControl, JobDeclaration>
	{
		public void TestPreviousDocumentsTabPageCaptionAndUserControlType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			using var ucc5 = ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC5Core", true);
			EUDynamicControlTestHelper.AssertEUSupplierHeaderUserControlTabPageCaptionAndUserControlType<ImportSupplierHeaderUserControl>(
				declaration: declaration,
				tabPageName: "PreviousDocumentsTabPage",
				userControlName: "previousDocumentsUserControl1",
				expectedCaption: "[2/1] Previous Documents",
				expectedUserControlType: typeof(PreviousDocumentsUserControl)
			);
		}

		public void TestPreviousDocumentsTabPageCaptionAndUserControlTypeUCC6()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			using var ucc6 = ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true);
			EUDynamicControlTestHelper.AssertEUSupplierHeaderUserControlTabPageCaptionAndUserControlType<ImportSupplierHeaderUserControl>(
				declaration: declaration,
				tabPageName: "PreviousDocumentsTabPage",
				userControlName: "previousDocumentsUserControl1",
				expectedCaption: "Previous Documents",
				expectedUserControlType: typeof(PreviousDocumentsUserControl)
			);
		}

		public void TestAdditionalInfoTabPageCaptionAndUserControlType()
		{
			EUDynamicControlTestHelper.AssertEUSupplierHeaderUserControlTabPageCaptionAndUserControlType<ImportSupplierHeaderUserControl>(Factory.New<JobDeclaration>(), "AdditionalInfoTabPage", "additionalInfosUserControl1", "Additional Documents", typeof(ImportAdditionalInfosUserControlWithGrid));

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			EUDynamicControlTestHelper.AssertEUSupplierHeaderUserControlTabPageCaptionAndUserControlType<ImportSupplierHeaderUserControl>(
				declaration: declaration,
				tabPageName: "AdditionalInfoTabPage",
				userControlName: "additionalInfosUserControl1",
				expectedCaption: "[2/2] Additional Information",
				expectedUserControlType: typeof(ImportAdditionalInfosUserControlWithGrid)
			);
		}

		public void TestSupportingDocumentsTabPageCaptionAndUserControlType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			using var ucc5 = ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC5Core", true);
			EUDynamicControlTestHelper.AssertEUSupplierHeaderUserControlTabPageCaptionAndUserControlType<ImportSupplierHeaderUserControl>(
				declaration: declaration,
				tabPageName: "SupportingDocumentsTabPage",
				userControlName: "SupportingDocumentsUserControl",
				expectedCaption: "[2/3] Supporting Documents",
				expectedUserControlType: typeof(InvoiceLayoutSupportingDocumentsUserControl)
			);
		}

		public void TestSupportingDocumentsTabPageCaptionAndUserControlTypeUCC6()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			using var ucc6 = ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true);
			EUDynamicControlTestHelper.AssertEUSupplierHeaderUserControlTabPageCaptionAndUserControlType<ImportSupplierHeaderUserControl>(
				declaration: declaration,
				tabPageName: "SupportingDocumentsTabPage",
				userControlName: "SupportingDocumentsUserControl",
				expectedCaption: "Supporting Documents",
				expectedUserControlType: typeof(InvoiceLayoutSupportingDocumentsUserControl)
			);
		}

		public void TestValuationIndicatorsTabPageCaptionAndUserControlType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			EUDynamicControlTestHelper.AssertEUSupplierHeaderUserControlTabPageCaptionAndUserControlType<ImportSupplierHeaderUserControl>(declaration, "ValueIndicatorsTabPage", "ValueIndicatorsUserControl", "Valuation Indicators", typeof(ValueIndicatorsUserControl));

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = "IMP";
			declaration2.JE_ApplicationCode = "V1";
			EUDynamicControlTestHelper.AssertEUSupplierHeaderUserControlTabPageCaptionAndUserControlType<ImportSupplierHeaderUserControl>(declaration2, "ValueIndicatorsTabPage", "ValueIndicatorsUserControl", "[4/13] Valuation Indicators", typeof(ValueIndicatorsUserControl));
		}

		public void TestGridColumnsVisibility_JZ_UCR()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;

			using (var form = new ZForm(declaration))
			using (var control = new ImportSupplierHeaderUserControl())
			{
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();

				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
				var style = control.JobComInvoiceHeadersBoundGrid.InnerGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_UCR);
				AssertEquals("JZ_UCR should be available for Import V1", false, style.IsUnavailable);

				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
				style = control.JobComInvoiceHeadersBoundGrid.InnerGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_UCR);
				AssertEquals("JZ_UCR should be available for Import V2", false, style.IsUnavailable);

				declaration.JE_ApplicationCode = "@#@";
				style = control.JobComInvoiceHeadersBoundGrid.InnerGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_UCR);
				AssertEquals("JZ_UCR should not be available otherwise", true, style.IsUnavailable);
			}
		}

		public void TestGridColumnsVisibility_Supplier()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;

			using (var form = new ZForm(declaration))
			using (var control = new ImportSupplierHeaderUserControl())
			{
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();

				AssertEquals("SupplierOrgPK should NOT be available", true, control.JobComInvoiceHeadersBoundGrid.InnerGrid.GetColumnStyle(JobComInvoiceHeader.Schema.SupplierOrgPK).IsUnavailable);
				AssertEquals("JZ_OA_SupplierAddress should NOT be available", true, control.JobComInvoiceHeadersBoundGrid.InnerGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_OA_SupplierAddress).IsUnavailable);
				AssertEquals("SupplierName should NOT be available", true, control.JobComInvoiceHeadersBoundGrid.InnerGrid.GetColumnStyle(JobComInvoiceHeader.Schema.SupplierName).IsUnavailable);
			}
		}

		protected override IEnumerable<string> ExpectedControlList => DefaultControlList.Except(new[] { "GroupInvoiceDropEdit" });

		public void TestGetCalculateFreightForm()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();

			using (var form = new ZForm())
			using (var userControl = new ImportSupplierHeaderUserControl())
			{
				userControl.SetDataBinding(declaration, "");
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				var invoiceChargesCalculateFreightButton = userControl.FindSingle<ZButton>("InvoiceChargesCalculateFreightButton");
				var groupChargesCalculateFreightButton = userControl.FindSingle<ZButton>("GroupChargesCalculateFreightButton");

				CombineAssertions(() =>
				{
					invoiceChargesCalculateFreightButton.PerformClick();
					AssertType<CalculateFreightBizObj>("InvoiceChargesCalculateFreightButton: Should create IE CalculateFreightBizObj.", ((CalculateFreightForm)ZFormModaliser.ActiveForm).BusinessEntity);
					ZFormModaliser.ActiveForm.Close();

					groupChargesCalculateFreightButton.PerformClick();
					AssertType<CalculateFreightBizObj>("GroupChargesCalculateFreightButton: Should create IE CalculateFreightBizObj.", ((CalculateFreightForm)ZFormModaliser.ActiveForm).BusinessEntity);
				});
			}
		}

		public void TestCalculateFreightButtonVisibility_FreightAirTransportModesImport()
		{
			using (var form = new ZForm())
			using (var userControl = new ImportSupplierHeaderUserControl())
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();
				var invoiceChargesButtonPanel = userControl.FindSingle<ZPanel>("InvoiceChargesButtonPanel");
				var groupChargesButton = userControl.FindSingle<ZPanel>("GroupChargesButton");
				var invoiceChargesCalculateFreightButton = userControl.FindSingle<ZButton>("InvoiceChargesCalculateFreightButton");
				var invoiceChargesCalculateInsuranceButton = userControl.FindSingle<ZButton>("InvoiceChargesCalculateInsuranceButton");

				CombineAssertions(() =>
				{
					AssertEquals("InvoiceChargesButtonPanel visible", true, invoiceChargesButtonPanel.Visible);
					AssertEquals("GroupChargesButton visible", true, groupChargesButton.Visible);
					AssertEquals("InvoiceChargesCalculateFreightButton visible", true, invoiceChargesCalculateFreightButton.Visible);
					AssertEquals("InvoiceChargesCalculateFreightButton enabled", true, invoiceChargesCalculateFreightButton.Enabled);
					AssertEquals("InvoiceChargesCalculateInsuranceButton visible", true, invoiceChargesCalculateInsuranceButton.Visible);
					AssertEquals("InvoiceChargesCalculateInsuranceButton enabled", true, invoiceChargesCalculateInsuranceButton.Enabled);
				});
			}
		}

		public void TestCalculateFreightButtonsVisibility_FreightDisabledTransportModesImport()
		{
			using (var form = new ZForm())
			using (var userControl = new ImportSupplierHeaderUserControl())
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				var invoiceChargesButtonPanel = userControl.FindSingle<ZPanel>("InvoiceChargesButtonPanel");
				var groupChargesButton = userControl.FindSingle<ZPanel>("GroupChargesButton");

				var invoiceChargesCalculateFreightButton = userControl.FindSingle<ZButton>("InvoiceChargesCalculateFreightButton");
				var invoiceChargesCalculateInsuranceButton = userControl.FindSingle<ZButton>("InvoiceChargesCalculateInsuranceButton");

				CombineAssertions(() =>
				{
					foreach (var transportMode in FreightDisabledTransportModes)
					{
						declaration.JE_TransportMode = transportMode;
						AssertEquals("InvoiceChargesButtonPanel visible due to insurance", true, invoiceChargesButtonPanel.Visible);
						AssertEquals($"GroupChargesButton -> {transportMode}:", false, groupChargesButton.Visible);

						AssertEquals($"InvoiceChargesCalculateFreightButton -> {transportMode}:", false, invoiceChargesCalculateFreightButton.Visible);
						AssertEquals($"InvoiceChargesCalculateFreightButton -> {transportMode}:", false, invoiceChargesCalculateFreightButton.Enabled);
						AssertEquals($"InvoiceChargesCalculateInsuranceButton -> {transportMode}:", true, invoiceChargesCalculateInsuranceButton.Visible);
						AssertEquals($"InvoiceChargesCalculateInsuranceButton -> {transportMode}:", true, invoiceChargesCalculateInsuranceButton.Enabled);
					}
				});
			}
		}

		string[] freightDisabledTransportModes;
		string[] FreightDisabledTransportModes => freightDisabledTransportModes ?? (freightDisabledTransportModes = new[]
		{
			Core.Constants.TransportModes.Sea,
			Core.Constants.TransportModes.InlandWaterwayTransport,
			Core.Constants.TransportModes.Rail,
			Core.Constants.TransportModes.Road
		});
	}
}
