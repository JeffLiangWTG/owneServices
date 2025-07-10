using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(CustomsDeclarationUserControl))]
	class CustomsDeclarationUserControlTest : BaseCustomsDeclarationUserControlAbstractTest<CustomsDeclarationUserControl, JobDeclaration>
	{
		public void TestVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationForm(declaration))
			{
				using (var control = new CustomsDeclarationUserControl())
				{
					control.JobDeclaration = declaration;
					form.Controls.Add(control);
					form.Show();

					Application.DoEvents();
					declaration.JE_TransportMode = "SEA";
					AssertEquals("Container Mode should be visible", true, control.JE_ContainerModeBoundDropDownEdit.Visible);
					AssertEquals("Voyage Textbox Visibility", true, control.JE_VoyageFlightNoBoundTextBox.Visible);
					AssertEquals("Voyage Textbox Visibility", true, control.VesselFindBox.Visible);
					AssertEquals("(RIV LAK SEA)MasterBill Textbox Visibility", true, control.JE_MasterBillForSeaBoundTextBox.Visible);
					AssertEquals("(AIR)MasterBill Textbox Visibility", false, control.JE_MasterBillForAirBoundTextBox.Visible);
					declaration.JE_TransportMode = "RIV";
					AssertEquals("Container Mode should be visible", true, control.JE_ContainerModeBoundDropDownEdit.Visible);
					AssertEquals("Voyage Textbox Visibility", true, control.JE_VoyageFlightNoBoundTextBox.Visible);
					AssertEquals("(RIV LAK SEA)MasterBill Textbox Visibility", true, control.JE_MasterBillForSeaBoundTextBox.Visible);
					AssertEquals("(AIR)MasterBill Textbox Visibility", false, control.JE_MasterBillForAirBoundTextBox.Visible);
					declaration.JE_TransportMode = "LAK";
					AssertEquals("Container Mode should be visible", true, control.JE_ContainerModeBoundDropDownEdit.Visible);
					AssertEquals("Voyage Textbox Visibility", true, control.JE_VoyageFlightNoBoundTextBox.Visible);
					AssertEquals("(RIV LAK SEA)MasterBill Textbox Visibility", true, control.JE_MasterBillForSeaBoundTextBox.Visible);
					AssertEquals("(AIR)MasterBill Textbox Visibility", false, control.JE_MasterBillForAirBoundTextBox.Visible);
					declaration.JE_TransportMode = "ROA";
					AssertEquals("Container Mode should not be visible", false, control.JE_ContainerModeBoundDropDownEdit.Visible);
					AssertEquals("Voyage Textbox Visibility", true, control.JE_VoyageFlightNoBoundTextBox.Visible);
					AssertEquals("(RIV LAK SEA)MasterBill Textbox Visibility", false, control.JE_MasterBillForSeaBoundTextBox.Visible);
					AssertEquals("(AIR)MasterBill Textbox Visibility", false, control.JE_MasterBillForAirBoundTextBox.Visible);
					declaration.JE_TransportMode = "AIR";
					AssertEquals("Container Mode should not be visible", false, control.JE_ContainerModeBoundDropDownEdit.Visible);
					AssertEquals("Voyage Textbox Visibility", true, control.JE_VoyageFlightNoBoundTextBox.Visible);
					AssertEquals("(RIV LAK SEA)MasterBill Textbox Visibility", false, control.JE_MasterBillForSeaBoundTextBox.Visible);
					AssertEquals("(AIR)MasterBill Textbox Visibility", true, control.JE_MasterBillForAirBoundTextBox.Visible);
					declaration.JE_TransportMode = "ZZZ";
					AssertEquals("Container Mode should not be visible", false, control.JE_ContainerModeBoundDropDownEdit.Visible);
					AssertEquals("Voyage Textbox Visibility", false, control.JE_VoyageFlightNoBoundTextBox.Visible);
					AssertEquals("(RIV LAK SEA)MasterBill Textbox Visibility", false, control.JE_MasterBillForSeaBoundTextBox.Visible);
					AssertEquals("(AIR)MasterBill Textbox Visibility", false, control.JE_MasterBillForAirBoundTextBox.Visible);
				}
			}
		}

		public void TestJE_MasterBillForSeaBoundTextBoxVisibleForExpressCourier()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				Env.Registry.IsExpress = false;
				declaration.JE_TransportMode = "SEA";
				form.CustomsBrokerageUserControl.JobDeclaration = declaration;
				AssertEquals("JE_MasterBillForSeaBoundTextBox.Visible", true, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.JE_MasterBillForSeaBoundTextBox.Visible);
				AssertEquals("JE_MasterBillForAirBoundTextBox.Visible", false, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.JE_MasterBillForAirBoundTextBox.Visible);
				declaration.JE_TransportMode = "LAK";
				form.CustomsBrokerageUserControl.JobDeclaration = declaration;
				AssertEquals("JE_MasterBillForSeaBoundTextBox.Visible", true, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.JE_MasterBillForSeaBoundTextBox.Visible);
				AssertEquals("JE_MasterBillForAirBoundTextBox.Visible", false, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.JE_MasterBillForAirBoundTextBox.Visible);
				declaration.JE_TransportMode = "RIV";
				form.CustomsBrokerageUserControl.JobDeclaration = declaration;
				AssertEquals("JE_MasterBillForSeaBoundTextBox.Visible", true, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.JE_MasterBillForSeaBoundTextBox.Visible);
				AssertEquals("JE_MasterBillForAirBoundTextBox.Visible", false, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.JE_MasterBillForAirBoundTextBox.Visible);
				declaration.JE_TransportMode = "AIR";
				form.CustomsBrokerageUserControl.JobDeclaration = declaration;
				AssertEquals("JE_MasterBillForSeaBoundTextBox.Visible", false, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.JE_MasterBillForSeaBoundTextBox.Visible);
				AssertEquals("JE_MasterBillForAirBoundTextBox.Visible", true, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.JE_MasterBillForAirBoundTextBox.Visible);
				Env.Registry.IsExpress = true;
				declaration.JE_TransportMode = "SEA";
				form.CustomsBrokerageUserControl.JobDeclaration = declaration;
				AssertEquals("JE_MasterBillForSeaBoundTextBox.Visible", true, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.JE_MasterBillForSeaBoundTextBox.Visible);
				AssertEquals("JE_MasterBillForAirBoundTextBox.Visible", false, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.JE_MasterBillForAirBoundTextBox.Visible);
				declaration.JE_TransportMode = "LAK";
				form.CustomsBrokerageUserControl.JobDeclaration = declaration;
				AssertEquals("JE_MasterBillForSeaBoundTextBox.Visible", true, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.JE_MasterBillForSeaBoundTextBox.Visible);
				AssertEquals("JE_MasterBillForAirBoundTextBox.Visible", false, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.JE_MasterBillForAirBoundTextBox.Visible);
				declaration.JE_TransportMode = "RIV";
				form.CustomsBrokerageUserControl.JobDeclaration = declaration;
				AssertEquals("JE_MasterBillForSeaBoundTextBox.Visible", true, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.JE_MasterBillForSeaBoundTextBox.Visible);
				AssertEquals("JE_MasterBillForAirBoundTextBox.Visible", false, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.JE_MasterBillForAirBoundTextBox.Visible);
				declaration.JE_TransportMode = "AIR";
				form.CustomsBrokerageUserControl.JobDeclaration = declaration;
				AssertEquals("JE_MasterBillForSeaBoundTextBox.Visible", true, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.JE_MasterBillForSeaBoundTextBox.Visible);
				AssertEquals("JE_MasterBillForAirBoundTextBox.Visible", false, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.JE_MasterBillForAirBoundTextBox.Visible);
			}
		}

		public void TestProcessRelatedTabPageVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				using (var control = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting as CustomsDeclarationUserControl)
				{
					form.Show();
					AssertEquals(false, control.ProcessRelatedTabPage.TabVisible);
					declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
					AssertEquals(true, control.ProcessRelatedTabPage.TabVisible);
					declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
					AssertEquals(false, control.ProcessRelatedTabPage.TabVisible);
				}
			}
		}

		public void TestCustomsOfficesVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			using (var declarationForm = new JobDeclarationForm(declaration))
			{
				using (var declarationUserControl = declarationForm.CustomsBrokerageUserControl.DeclarationUserControlForTesting as CustomsDeclarationUserControl)
				{
					declarationForm.Show();
					AssertEquals("CustomsOfficesGroupBox should be visible for Export", true, declarationUserControl.CustomsOfficesGroupBox.Visible);
					AssertEquals("CustomsOfficesTabControl should be visible for Export", true, declarationUserControl.CustomsOfficesTabControl.Visible);
					AssertEquals("ImportOfficesUserControl should not be visible for Export", false, declarationUserControl.ImportOfficesUserControl.Visible);
					AssertEquals("ImportLicenseOfficesUserControl should not be visible for Export", false, declarationUserControl.ImportLicenseOfficesUserControl.Visible);
					AssertEquals("ImportSiscomexOfficesUserControl should not be visible for Export", false, declarationUserControl.ImportSiscomexOfficesUserControl.Visible);

					declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
					AssertEquals("CustomsOfficesGroupBox should be visible for Import", true, declarationUserControl.CustomsOfficesGroupBox.Visible);
					AssertEquals("CustomsOfficesTabControl should not be visible for Import", false, declarationUserControl.CustomsOfficesTabControl.Visible);
					AssertEquals("ImportOfficesUserControl should be visible for Import", true, declarationUserControl.ImportOfficesUserControl.Visible);
					AssertEquals("ImportLicenseOfficesUserControl should not be visible for Import", false, declarationUserControl.ImportLicenseOfficesUserControl.Visible);
					AssertEquals("ImportSiscomexOfficesUserControl should not be visible for Import", false, declarationUserControl.ImportSiscomexOfficesUserControl.Visible);

					declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
					AssertEquals("CustomsOfficesGroupBox should be visible for Import License", true, declarationUserControl.CustomsOfficesGroupBox.Visible);
					AssertEquals("CustomsOfficesTabControl should not be visible for Import License", false, declarationUserControl.CustomsOfficesTabControl.Visible);
					AssertEquals("ImportOfficesUserControl should not be visible for Import License", false, declarationUserControl.ImportOfficesUserControl.Visible);
					AssertEquals("ImportLicenseOfficesUserControl should be visible for Import License", true, declarationUserControl.ImportLicenseOfficesUserControl.Visible);
					AssertEquals("ImportSiscomexOfficesUserControl should not be visible for License", false, declarationUserControl.ImportSiscomexOfficesUserControl.Visible);

					declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
					AssertEquals("CustomsOfficesGroupBox should be visible for ImportSiscomex", true, declarationUserControl.CustomsOfficesGroupBox.Visible);
					AssertEquals("CustomsOfficesTabControl should not be visible for ImportSiscomex", false, declarationUserControl.CustomsOfficesTabControl.Visible);
					AssertEquals("ImportOfficesUserControl should not be visible for ImportSiscomex", false, declarationUserControl.ImportOfficesUserControl.Visible);
					AssertEquals("ImportLicenseOfficesUserControl should not be visible for ImportSiscomex", false, declarationUserControl.ImportLicenseOfficesUserControl.Visible);
					AssertEquals("ImportSiscomexOfficesUserControl should be visible for ImportSiscomex", true, declarationUserControl.ImportSiscomexOfficesUserControl.Visible);
				}
			}
		}

		public void TestLicensesTabVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			using (var declarationForm = new JobDeclarationForm(declaration))
			{
				using (var declarationUserControl = declarationForm.CustomsBrokerageUserControl.DeclarationUserControlForTesting as CustomsDeclarationUserControl)
				{
					declarationForm.Show();
					AssertEquals(false, declarationUserControl.LicensesTabPage.TabVisible);
					declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
					AssertEquals(false, declarationUserControl.LicensesTabPage.TabVisible);
					declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ImportDeclarationByExternalBroker;
					AssertEquals(false, declarationUserControl.LicensesTabPage.TabVisible);
					declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
					AssertEquals(false, declarationUserControl.LicensesTabPage.TabVisible);
					declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
					AssertEquals(false, declarationUserControl.LicensesTabPage.TabVisible);
					declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
					AssertEquals(true, declarationUserControl.LicensesTabPage.TabVisible);
				}
			}
		}

		public void TestDispatchInstructionTabVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			using (var declarationForm = new JobDeclarationForm(declaration))
			{
				using (var declarationUserControl = declarationForm.CustomsBrokerageUserControl.DeclarationUserControlForTesting as CustomsDeclarationUserControl)
				{
					declarationForm.Show();
					AssertEquals(false, declarationUserControl.DispatchInstructionTabPage.TabVisible);
					declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
					AssertEquals(true, declarationUserControl.DispatchInstructionTabPage.TabVisible);
					declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
					AssertEquals(false, declarationUserControl.DispatchInstructionTabPage.TabVisible);
					declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
					AssertEquals(false, declarationUserControl.DispatchInstructionTabPage.TabVisible);
				}
			}
		}

		public void TestBrokerageUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			using (var declarationForm = new JobDeclarationForm(declaration))
			{
				AssertType<CustomsBrokerageUserControl>(declarationForm.CustomsBrokerageUserControl);
			}

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			using (var declarationForm = new JobDeclarationForm(declaration))
			{
				AssertType<CustomsBrokerageUserControl>(declarationForm.CustomsBrokerageUserControl);
			}

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			using (var declarationForm = new JobDeclarationForm(declaration))
			{
				AssertType<CustomsBrokerageUserControl>(declarationForm.CustomsBrokerageUserControl);
			}
		}

		public void TestVisibilityForImportLicense()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			using (var declarationForm = new JobDeclarationForm(declaration))
			{
				using (var declarationUserControl = declarationForm.CustomsBrokerageUserControl.DeclarationUserControlForTesting as CustomsDeclarationUserControl)
				{
					declarationForm.Show();
					AssertEquals("Details", declarationUserControl.ShipmentDetailsGroupBox.CaptionResourceString.Caption);
					Assert("TransportDetailsGroupBox should not be visible", !declarationUserControl.TransportDetailsGroupBox.Visible);

					Assert("NumbersTabPage should not be visible", !declarationUserControl.NumbersTabPage.TabVisible);
					Assert("OrganisationsTabPage should not be visible", !declarationUserControl.OrganisationsTabPage.TabVisible);
					Assert("DocsTabPage should not be visible", !declarationUserControl.DocsTabPage.TabVisible);
					Assert("ShipmentCustomFieldsPage should be visible", declarationUserControl.ShipmentCustomFieldsPage.TabVisible);
				}
			}
		}

		public void TestVisibilityForLPCO()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			using (var declarationForm = new JobDeclarationForm(declaration))
			{
				using (var declarationUserControl = declarationForm.CustomsBrokerageUserControl.DeclarationUserControlForTesting as CustomsDeclarationUserControl)
				{
					declarationForm.Show();
					AssertEquals("Details", declarationUserControl.ShipmentDetailsGroupBox.CaptionResourceString.Caption);
					Assert("TransportDetailsGroupBox should not be visible", !declarationUserControl.TransportDetailsGroupBox.Visible);

					Assert("NumbersTabPage should not be visible", !declarationUserControl.NumbersTabPage.TabVisible);
					Assert("OrganisationsTabPage should not be visible", !declarationUserControl.OrganisationsTabPage.TabVisible);
					Assert("DocsTabPage should not be visible", !declarationUserControl.DocsTabPage.TabVisible);
					Assert("ShipmentCustomFieldsPage should be visible", declarationUserControl.ShipmentCustomFieldsPage.TabVisible);
				}
			}
		}

		public void TestMultipleKeysToUse()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			using (var declarationForm = new JobDeclarationForm(declaration))
			using (var declarationUserControl = declarationForm.CustomsBrokerageUserControl.DeclarationUserControlForTesting as CustomsDeclarationUserControl)
			{
				AssertSequencesEqual(new[] { BRJobMessageTypeList.Codes.Import }, declarationUserControl.SupportMultipleResourceStringData.MultipleKeysToUse);
			}
		}

		public void TestProcessRelatedNumberGridColumnsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.ProcessRelatedNumbers.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				using (var control = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting as CustomsDeclarationUserControl)
				{
					form.Show();

					var processRelatedUserControl = (ProcessRelatedUserControl)form.Controls.Find("processRelatedUserControl1", true)[0];
					var permitGrid = processRelatedUserControl.NumbersGrid;

					Assert("CE_EntryType should be Visible", permitGrid.GetColumnStyle(ProcessRelatedNumber.Schema.CE_EntryType).IsVisible);
					Assert("CE_EntryNum should be Visible", permitGrid.GetColumnStyle(ProcessRelatedNumber.Schema.CE_EntryNum).IsVisible);
					Assert("CE_EntryLineReference should not be Visible", !permitGrid.GetColumnStyle(ProcessRelatedNumber.Schema.CE_EntryLineReference).IsVisible);
					Assert("CE_IssueDate should not be Visible", !permitGrid.GetColumnStyle(ProcessRelatedNumber.Schema.CE_IssueDate).IsVisible);

					declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
					processRelatedUserControl = (ProcessRelatedUserControl)form.Controls.Find("processRelatedUserControl1", true)[0];
					permitGrid = processRelatedUserControl.NumbersGrid;

					Assert("CE_EntryType should be Visible", permitGrid.GetColumnStyle(ProcessRelatedNumber.Schema.CE_EntryType).IsVisible);
					Assert("CE_EntryNum should be Visible", permitGrid.GetColumnStyle(ProcessRelatedNumber.Schema.CE_EntryNum).IsVisible);
					Assert("CE_EntryLineReference should be Visible", permitGrid.GetColumnStyle(ProcessRelatedNumber.Schema.CE_EntryLineReference).IsVisible);
					Assert("CE_IssueDate should be Visible", permitGrid.GetColumnStyle(ProcessRelatedNumber.Schema.CE_IssueDate).IsVisible);

					declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
					var processRelatedUserControl1 = form.Controls.Find("processRelatedUserControl1", true);
					AssertEquals("processRelatedUserControl should be null", processRelatedUserControl1.IsNullOrEmpty(), true);
				}
			}
		}
	}
}
