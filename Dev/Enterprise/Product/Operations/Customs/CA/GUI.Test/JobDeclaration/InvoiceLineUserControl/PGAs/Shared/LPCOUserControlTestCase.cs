using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using JobMessageTypeList = Enterprise.Customs.CA.Business.JobMessageTypeList;
using OrgSupplierPart = Enterprise.Customs.CA.Business.OrgSupplierPart;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class LPCOUserControlTestCase : TestCaseWithFactory
	{
		public void TestSetLPCOColumns()
		{
			using (var control = new LPCOGridUserControl())
			{
				control.RemoveExceptAvailableColumns(null);
				CombineAssertions("LPCO do not set anything", () =>
				{
					AssertEquals("Default setting will set unavailable for setting column", false, control.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RefNo).IsUnavailable);
					AssertEquals("Default setting will set unavailable for setting column", false, control.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation).IsUnavailable);
				});

				control.RemoveExceptAvailableColumns(AvailableLPCOFields);
				CombineAssertions("LPCO set some columns", () =>
				{
					AssertEquals("Default setting will set unavailable for setting column", false, control.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RefNo).IsUnavailable);
					AssertEquals("Default setting will set unavailable for setting column", false, control.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation).IsUnavailable);
					AssertEquals("Default setting will set unavailable for setting column", false, control.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RefNo).IsUnavailable);
					AssertEquals("Default setting will set unavailable for setting column", false, control.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation).IsUnavailable);
				});
			}
		}

		public void TestAddOrEditDIFButtonVisibleForDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = "Y";
			var header = invoiceLine.CFIAPGAHeader;
			using (var control = new LPCOGridUserControl())
			{
				control.RemoveExceptAvailableColumns(CFIAPGAHeader.AvailableLPCOFields);
				control.SetDataBinding(header, "");

				Assert("CLP_DIFRefNumberOrLocation should be available for CFIA", !control.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation).IsUnavailable);
				Assert("AddOrEditButton should Visible for CFIA", control.BottomPanel.Visible);
				AssertNotEquals("The Dock of LPCOGrid should not be Fill for CFIA", System.Windows.Forms.DockStyle.Fill, control.LpcoGrid.Dock);
			}
			using (var control = new LPCOGridUserControl())
			{
				var list = new List<string>(CFIAPGAHeader.AvailableLPCOFields);
				list.Remove(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation);
				control.RemoveExceptAvailableColumns(list);
				control.SetDataBinding(header, "");

				Assert("CLP_DIFRefNumberOrLocation should be unavailable, because CLP_DIFRefNumberOrLocation has been removed", control.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation).IsUnavailable);
				Assert("AddOrEditButton should not Visible, because there is no CLP_DIFRefNumberOrLocation", !control.BottomPanel.Visible);
				AssertEquals("The Dock of LPCOGrid should be Fill, because there is no CLP_DIFRefNumberOrLocation", System.Windows.Forms.DockStyle.Fill, control.LpcoGrid.Dock);
			}
		}

		public void TestAddOrEditDIFButtonVisibleForShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = "Y";
			var header = invoiceLine.CFIAPGAHeader;
			using (var control = new LPCOGridUserControl())
			{
				control.RemoveExceptAvailableColumns(CFIAPGAHeader.AvailableLPCOFields);
				control.SetDataBinding(header, "");

				Assert("CLP_DIFRefNumberOrLocation should be available for CFIA", !control.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation).IsUnavailable);
				Assert("AddOrEditButton should Visible for CFIA", control.BottomPanel.Visible);
				AssertNotEquals("The Dock of LPCOGrid should not be Fill for CFIA", System.Windows.Forms.DockStyle.Fill, control.LpcoGrid.Dock);
			}

			using (var control = new LPCOGridUserControl())
			{
				var list = new List<string>(CFIAPGAHeader.AvailableLPCOFields);
				list.Remove(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation);
				control.RemoveExceptAvailableColumns(list);
				control.SetDataBinding(header, "");

				Assert("CLP_DIFRefNumberOrLocation should be unavailable, because CLP_DIFRefNumberOrLocation has been removed", control.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation).IsUnavailable);
				Assert("AddOrEditButton should not Visible, because there is no CLP_DIFRefNumberOrLocation", !control.BottomPanel.Visible);
				AssertEquals("The Dock of LPCOGrid should be Fill, because there is no CLP_DIFRefNumberOrLocation", System.Windows.Forms.DockStyle.Fill, control.LpcoGrid.Dock);
			}
		}

		public void TestAddOrEditDIFButtonVisibleForCommercialInvoice()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			new FakeDeclarationCreatorForInvoice(invoice);
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = "Y";
			var header = invoiceLine.CFIAPGAHeader;
			using (var control = new LPCOGridUserControl())
			{
				control.RemoveExceptAvailableColumns(CFIAPGAHeader.AvailableLPCOFields);
				control.SetDataBinding(header, "");

				Assert("CLP_DIFRefNumberOrLocation should be available for CFIA", !control.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation).IsUnavailable);
				Assert("AddOrEditButton should not Visible, because is Commercial Invoice", !control.BottomPanel.Visible);
				AssertEquals("The Dock of LPCOGrid should be Fill, because is Commercial Invoice", System.Windows.Forms.DockStyle.Fill, control.LpcoGrid.Dock);
			}
		}

		public void TestAddOrEditDIFButtonVisibleForProduct()
		{
			var importer1 = Factory.NewWithValidTestData<OrgHeader>();

			var part = Factory.New<OrgSupplierPart>();
			part.FillWithValidTestData();
			part.OP_PartNum = "1234";
			var part1Relation1 = part.RelatedOrganisations.AddNew();
			part1Relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			part1Relation1.OU_OH = importer1.PK;

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_OP = part.PK;
			pivot.CCA_CFIAIndicator = YesNoList.Codes.Yes;
			var header = pivot.CFIAPGAHeader;
			using (var control = new LPCOGridUserControl())
			{
				control.RemoveExceptAvailableColumns(CFIAPGAHeader.AvailableLPCOFields);
				control.SetDataBinding(header, "");

				Assert("CLP_DIFRefNumberOrLocation should be available for CFIA", !control.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation).IsUnavailable);
				Assert("AddOrEditButton should not Visible, because is product", !control.BottomPanel.Visible);
				AssertEquals("The Dock of LPCOGrid should be Fill, because is product", System.Windows.Forms.DockStyle.Fill, control.LpcoGrid.Dock);
			}
		}

		public void TestAddOrEditDIFButtonVisibleForClassification()
		{
			var classification = Factory.New<CusClassification>();
			var caClassification = Factory.New<CusCAClassification>();
			caClassification.CCA_ParentID = classification.PK;
			caClassification.CCA_ParentTableCode = classification.TablePrefix;
			caClassification.CCA_CFIAIndicator = YesNoList.Codes.Yes;
			var header = classification.CFIAPGAHeader;
			using (var control = new LPCOGridUserControl())
			{
				control.RemoveExceptAvailableColumns(CFIAPGAHeader.AvailableLPCOFields);
				control.SetDataBinding(header, "");

				Assert("CLP_DIFRefNumberOrLocation should be available for CFIA", !control.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation).IsUnavailable);
				Assert("AddOrEditButton should not Visible, because is classification", !control.BottomPanel.Visible);
				AssertEquals("The Dock of LPCOGrid should be Fill, because is classification", System.Windows.Forms.DockStyle.Fill, control.LpcoGrid.Dock);
			}
		}

		public void TestAddOrEditDIFButton_Click()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest())
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
				TransactionNumberSetting setting = new TransactionNumberSetting(Factory, "Test", "32450", null, TransactionNumber.DIFNumberDeclarationType, true);
				setting.NextNumber = 100;
				setting.MaxNumber = 200;
				Factory.Save();

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
				var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
				invoiceLine.CA_CFIAInd = "Y";
				var header = invoiceLine.CFIAPGAHeader;
				var lpcoView = header.LPCOViews.AddNew();
				lpcoView.CLP_DIFRefNumberOrLocation = "123";

				using (var form = new ZForm(declaration))
				using (var control = new LPCOGridUserControl())
				{
					form.Controls.Add(control);
					control.RemoveExceptAvailableColumns(CFIAPGAHeader.AvailableLPCOFields);
					control.SetDataBinding(header, "");

					form.Show();
					control.Show();
					var button = (ZButton)control.Controls.Find("AddOrEditDIFButton", true)[0];

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
					button.PerformClick();
					AssertEquals("There are changes in the main form. Do you want the system to save these changes and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					button.PerformClick();
					AssertEquals("There are no Tracking documents entered in the grid 'Document Tracking'. DIS documents will need to be linked to a document entered in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);

					var requiredDocument = declaration.DocsAndCartage.RequiredDocuments.AddNew();
					requiredDocument.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
					requiredDocument.EQ_DocType = "ABC";
					Factory.Save();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					button.PerformClick();
					AssertEquals("Please select a LPCO first.", UnitTestUserNotification.Instance.LastMessage.Text);

					var lpcoGrid = control.LpcoGrid;
					lpcoGrid.SetDataBinding(header.LPCOViews, "");
					lpcoGrid.Select(0);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					button.PerformClick();
					AssertEquals("A DIF with a matching URN number and PGA could not be found, do you want to create a new DIF?", UnitTestUserNotification.Instance.LastMessage.Text);
					var controller = (ZController)control.GetType().GetField("dIFController", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(control);
					var difForm = controller.LastShownForm;
					AssertEquals("DIFForm", difForm.GetType().Name);
				}
			}
		}

		public void TestFindBoxesBindToList()
		{
			using (var control = new LPCOGridUserControl())
			{
				var authCountryFindBox = control.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RN_NKAuthorizationCountry) as ZCodeFindBoxColumnStyleInfo;
				AssertEquals("BindToList", "LPCO+Lookups.AuthorizationCountries", authCountryFindBox.BindToList);

				var issuanceCountryFindBox = control.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RN_NKIssuanceCountryCode) as ZCodeFindBoxColumnStyleInfo;
				AssertEquals("BindToList", "LPCO+Lookups.IssuanceCountryCodes", issuanceCountryFindBox.BindToList);

				var originCountryFindBox = control.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RN_NKOriginCountryCode) as ZCodeFindBoxColumnStyleInfo;
				AssertEquals("BindToList", "LPCO+Lookups.OriginCountryCodes", originCountryFindBox.BindToList);

				var smeltAndPourCountryFindBox = control.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RN_NKSmeltAndPourCountryCode) as ZCodeFindBoxColumnStyleInfo;
				AssertEquals("BindToList", "LPCO+Lookups.SmeltAndPourCountryCodes", smeltAndPourCountryFindBox.BindToList);
				var smeltAndPourCountryFindBoxCaptionResourceString = smeltAndPourCountryFindBox.CaptionResourceString;
				AssertEquals("CMP", smeltAndPourCountryFindBoxCaptionResourceString.ShortCaption);
				AssertEquals("Ctry. of Melt & Pour", smeltAndPourCountryFindBoxCaptionResourceString.Caption);
				AssertEquals("Country of Melt and Pour", smeltAndPourCountryFindBoxCaptionResourceString.FullDescription);
			}
		}

		static List<string> AvailableLPCOFields
		{
			get
			{
				return new List<string>
				{
					CusCALPCO.Schema.CLP_DIFRefNumberOrLocation,
					CusCALPCO.Schema.CLP_RefNo,
				};
			}
		}
	}
}
