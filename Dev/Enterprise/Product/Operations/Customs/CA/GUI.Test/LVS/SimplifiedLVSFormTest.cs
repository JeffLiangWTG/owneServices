using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(SimplifiedLVSForm))]
	sealed class SimplifiedLVSFormTest : ZFormBasherTest
	{
		public void TestEntryMode()
		{
			var importer = Factory.New<OrgHeader>();
			var simLVS = new SimplifiedLVS(Factory);
			simLVS.JE_OH_Importer = importer.PK;
			simLVS.CA_PortOfClearance = "XXX";
			using (var form = new SimplifiedLVSForm(simLVS))
			{
				form.Show();
				AssertEquals(SimplifiedLVSForm.Mode.CargoListHeader, form.EntryMode);
				AssertEquals("Co&ntinue", form.ContinueAndSaveButton.CaptionResourceString.Caption);
				AssertEquals("Continue", form.continueAndSaveMenuItem.Caption);
				AssertEquals("&Close", form.GaveUpButton.CaptionResourceString.Caption);
				AssertEquals("Close", form.quitMenuItem.Caption);
				Assert("CalculateDutyButton", !form.CalculateDutyButton.Visible);
				Assert("LastJobButton", !form.LastJobButton.Visible);

				form.EntryMode = SimplifiedLVSForm.Mode.ShipmentHeader;
				AssertEquals("Co&ntinue", form.ContinueAndSaveButton.CaptionResourceString.Caption);
				AssertEquals("Continue", form.continueAndSaveMenuItem.Caption);
				AssertEquals("&Close", form.GaveUpButton.CaptionResourceString.Caption);
				AssertEquals("Close", form.quitMenuItem.Caption);
				Assert("CalculateDutyButton", !form.CalculateDutyButton.Visible);
				Assert("LastJobButton", form.LastJobButton.Visible);
				Assert("LastJobButton", !form.LastJobButton.Enabled);

				simLVS.Invoices.AddNew();
				form.EntryMode = SimplifiedLVSForm.Mode.InvoiceDetails;
				AssertEquals("&Save && Continue", form.ContinueAndSaveButton.CaptionResourceString.Caption);
				AssertEquals("Save && Continue", form.continueAndSaveMenuItem.Caption);
				AssertEquals("&Cancel", form.GaveUpButton.CaptionResourceString.Caption);
				AssertEquals("Cancel", form.quitMenuItem.Caption);
				Assert("CalculateDutyButton", form.CalculateDutyButton.Visible);
				Assert("LastJobButton", !form.LastJobButton.Visible);
			}
		}

		public void TestCloseAndMoveNextLVS()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var officeCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0XXB", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "YY");
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var impAddInfo = OrgImpAddInfo.Get(importer);
			impAddInfo.ZO_IsCreateIndividualLVS = true;
			var simLVS = new SimplifiedLVS(Factory);
			simLVS.JE_OH_Importer = importer.PK;
			simLVS.CA_PortOfClearance = "0XXB";
			simLVS.JZ_InvoiceNumber = "11";
			using (var form = new SimplifiedLVSForm(simLVS))
			{
				form.Show();
				UnitTestUserNotification.Instance.AddOKAnswer();
				form.ContinueAndSaveButton.PerformClick();
				form.ContinueAndSaveButton.PerformClick();
				AssertEquals("11", simLVS.Invoices[0].JZ_InvoiceNumber);
				simLVS.Invoices[0].InvoiceLines.AddNew().JI_Tariff = "1111111111";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.ContinueAndSaveButton.PerformClick();
				Assert("Show waring when save with message errors", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("There are message errors"));
				Assert(!form.Visible);

				using (var newForm = form.LastFormShownForTest as SimplifiedLVSForm)
				{
					AssertNotNull(newForm);
					Assert(newForm.Visible);
					var newSimLVS = (SimplifiedLVS)newForm.BusinessEntity;
					AssertEquals(importer.PK, newSimLVS.JE_OH_Importer);
					AssertEquals("0XXB", newSimLVS.CA_PortOfClearance);
					AssertEquals("", newSimLVS.JZ_InvoiceNumber);
				}
			}
		}

		public void TestSaveAndContinueClick()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "OH1";
			var simLVS = new SimplifiedLVS(Factory);
			simLVS.JE_OH_Importer = importer.PK;
			simLVS.CA_PortOfClearance = "XXX";
			using (var form = new SimplifiedLVSForm(simLVS))
			{
				form.Show();
				var headerDetailsGroupBox = form.Controls.Find("headerDetailsGroupBox", true)[0];
				var deliveryAddressTabPage = (ZTabPage)form.Controls.Find("deliveryAddressTabPage", true)[0];
				var shipperDocAddressControl = ((SimplifiedLVSHeaderDetailsUserControl)form.Controls.Find("SimplifiedLVSHeaderDetailsUserControl", true)[0]).ShipperDocAddressControl;
				Assert(!form.simplifiedLVSUserControl.LVSEntryVisible);
				AssertEquals("Header Details", headerDetailsGroupBox.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				form.ContinueAndSaveButton.PerformClick();
				form.ContinueAndSaveButton.PerformClick();
				AssertEquals("The system could not find a matched Type F Consolidation declaration. Do you want to create a new Type F Consolidation declaration now?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(SimplifiedLVSForm.Mode.ShipmentHeader, form.EntryMode);
				AssertEquals("Header Details", headerDetailsGroupBox.Text);

				UnitTestUserNotification.Instance.AddOKAnswer();
				form.ContinueAndSaveButton.PerformClick();
				AssertEquals("The system could not find a matched Type F Consolidation declaration. Do you want to create a new Type F Consolidation declaration now?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(SimplifiedLVSForm.Mode.InvoiceDetails, form.EntryMode);
				AssertEquals("New Type F Consolidation Created", headerDetailsGroupBox.Text);
				Assert("Delivery Address Tab should NOT be visible", !deliveryAddressTabPage.TabVisible);
				Assert("ShipperDocAddressControl should NOT be visible", !shipperDocAddressControl.Visible);
				Assert(simLVS.Declaration.IsConsolidatedLVS);

				var declaration = simLVS.Declaration;
				simLVS.Declaration.JE_DeclarationReference = "B10000001";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.ContinueAndSaveButton.PerformClick();
				Assert(form.Visible);
				AssertEquals("You have not entered LVS Shipment Details.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				simLVS.Invoices[0].InvoiceLines.AddNew().JI_Tariff = "1111111111";
				form.ContinueAndSaveButton.PerformClick();
				Assert(!form.Visible);
				Assert("Apportionment should be resumed", !declaration.ApportionmentDirty);

				UnitTestUserNotification.Instance.ClearMessages();
				using (var newForm = form.LastFormShownForTest as SimplifiedLVSForm)
				{
					newForm.Show();
					headerDetailsGroupBox = newForm.Controls.Find("headerDetailsGroupBox", true)[0];
					AssertEquals(SimplifiedLVSForm.Mode.ShipmentHeader, newForm.EntryMode);
					AssertEquals("Header Details", headerDetailsGroupBox.Text);

					newForm.ContinueAndSaveButton.PerformClick();
					AssertEquals(SimplifiedLVSForm.Mode.InvoiceDetails, newForm.EntryMode);
					AssertEquals("Existing Type F Consolidation Selected (B10000001)", headerDetailsGroupBox.Text);

					newForm.GaveUpButton.PerformClick();
					AssertEquals(SimplifiedLVSForm.Mode.ShipmentHeader, newForm.EntryMode);
					AssertEquals("Header Details", headerDetailsGroupBox.Text);

					Assert("LastJobButton", newForm.LastJobButton.Enabled);
					AssertEquals("LastJobPK", declaration.PK, newForm.LastJobPK);

					newForm.LastJobButton.PerformClick();
					AssertType<LowValueShipmentsUserControl>(((JobDeclarationForm)newForm.LastFormShownForTest).CustomsBrokerageUserControl);
					((JobDeclarationForm)newForm.LastFormShownForTest).Close();
				}
			}
		}

		public void TestSaveAndContinueClick_CreateIndividualLVS()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "OH1";
			var impAddInfo = OrgImpAddInfo.Get(importer);
			impAddInfo.ZO_IsCreateIndividualLVS = true;
			var simLVS = new SimplifiedLVS(Factory);
			simLVS.JE_OH_Importer = importer.PK;
			simLVS.CA_PortOfClearance = "XXX";

			using (var form = new SimplifiedLVSForm(simLVS))
			{
				form.Show();
				var headerDetailsGroupBox = form.Controls.Find("headerDetailsGroupBox", true)[0];
				var deliveryAddressTabPage = (ZTabPage)form.Controls.Find("deliveryAddressTabPage", true)[0];
				var shipperDocAddressControl = ((SimplifiedLVSHeaderDetailsUserControl)form.Controls.Find("SimplifiedLVSHeaderDetailsUserControl", true)[0]).ShipperDocAddressControl;
				AssertEquals(SimplifiedLVSForm.Mode.CargoListHeader, form.EntryMode);
				AssertEquals("Header Details", headerDetailsGroupBox.Text);

				form.ContinueAndSaveButton.PerformClick();
				form.ContinueAndSaveButton.PerformClick();
				AssertEquals(SimplifiedLVSForm.Mode.InvoiceDetails, form.EntryMode);
				AssertEquals("Courier LVS Declaration", headerDetailsGroupBox.Text);
				Assert("Delivery Address Tab should be visible", deliveryAddressTabPage.TabVisible);
				var headerDetailsTabPage = form.simplifiedLVSUserControl.AdditionalHeaderDetailsTabPage;
				((ZTabControl)headerDetailsTabPage.Parent).SelectedTab = headerDetailsTabPage;
				Assert("ShipperDocAddressControl should be visible", shipperDocAddressControl.Visible);
				Assert(simLVS.Declaration.IsLVX);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var invoiceLine = (JobComInvoiceLine)simLVS.Invoices[0].InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "1111111111";
				invoiceLine.JI_LinePrice = 100m;
				invoiceLine.JI_Description = "XX";
				invoiceLine.JI_CountryOfOrigin = "CA";
				invoiceLine.CA_TreatmentCode = "02";
				invoiceLine.JI_FormattedTariff = "";
				invoiceLine.CA_AuthorityNumber = "X";
				var generatedLVXDeclaration = simLVS.Declaration;
				AssertEquals("Merge not done yet", 0, generatedLVXDeclaration.Entries.Count);
				form.ContinueAndSaveButton.PerformClick();
				AssertEquals("JZ_InvoiceAmount should be set", 100m, generatedLVXDeclaration.Invoices[0].JZ_InvoiceAmount);
				Assert("JZ_InvoiceAmount should be set", !generatedLVXDeclaration.Invoices[0].JZ_InvoiceAmountInfo.HasMessageErrors());
				AssertEquals("Merge has occured", 1, generatedLVXDeclaration.Entries.Count);

				UnitTestUserNotification.Instance.ClearMessages();
				using (var newForm = form.LastFormShownForTest as SimplifiedLVSForm)
				{
					newForm.Show();
					Assert("LastJobButton", newForm.LastJobButton.Enabled);
					AssertEquals("LastJobPK", generatedLVXDeclaration.PK, newForm.LastJobPK);

					newForm.LastJobButton.PerformClick();
					AssertType<LVXCustomsBrokerageUserControl>(((JobDeclarationForm)newForm.LastFormShownForTest).CustomsBrokerageUserControl);
					((JobDeclarationForm)newForm.LastFormShownForTest).Close();
				}
			}
		}

		public void TestContinueAndSaveClick()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var simLVS = new SimplifiedLVS(Factory);
			using (var form = new SimplifiedLVSForm(simLVS))
			{
				form.Show();
				AssertEquals(SimplifiedLVSForm.Mode.CargoListHeader, form.EntryMode);

				form.ContinueAndSaveButton.PerformClick();
				AssertEquals(SimplifiedLVSForm.Mode.CargoListHeader, form.EntryMode);
				AssertEquals("Should not allow the user to continue with errors", SimplifiedLVSForm.Mode.CargoListHeader, form.EntryMode);
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				simLVS.CA_PortOfClearance = "XXX";
				form.ContinueAndSaveButton.PerformClick();
				AssertEquals("Allow the user to continue with errors", SimplifiedLVSForm.Mode.ShipmentHeader, form.EntryMode);

				form.ContinueAndSaveButton.PerformClick();
				AssertEquals("Should not allow the user to continue with errors", SimplifiedLVSForm.Mode.ShipmentHeader, form.EntryMode);
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				simLVS.JE_OH_Importer = importer.PK;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				form.ContinueAndSaveButton.PerformClick();
				AssertEquals("Allow the user to continue", SimplifiedLVSForm.Mode.InvoiceDetails, form.EntryMode);
				AssertNull(form.LastFormShownForTest);
			}
		}

		public void TestFormClose()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var simLVS = new SimplifiedLVS(Factory);
			using (var form = new SimplifiedLVSForm(simLVS))
			{
				form.Show();
				form.ContinueAndSaveButton.PerformClick();
				AssertEquals("Should not allow the user to continue with errors", SimplifiedLVSForm.Mode.CargoListHeader, form.EntryMode);
				simLVS.HasChanges = true;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.GaveUpButton.PerformClick();
				Assert("Don't ask for saving changes", UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertNull(form.LastFormShownForTest);
			}
		}

		[TestDate(2015, 12, 1)]
		public void TestDefaultValueForNewSimplifiedLVS()
		{
			var carrier = Factory.New<ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "3001";
			carrier.ZZ4_Description = "3001 Desc";
			carrier.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Canada;
			carrier.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER);
			Factory.Save();

			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest())
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var importer = Factory.New<OrgHeader>();
				importer.OH_Code = "OH1";
				var simLVS = new SimplifiedLVS(Factory);
				simLVS.JE_OH_Importer = importer.PK;
				simLVS.CA_PortOfClearance = "0XXX";
				simLVS.CA_LVSCarrier = "3001";
				simLVS.JZ_InvoiceNumber = "INV001";
				simLVS.JZ_InvoiceDate = new DateTime(2015, 9, 16);
				using (var form = new SimplifiedLVSForm(simLVS))
				{
					form.Show();
					var headerDetailsGroupBox = form.Controls.Find("headerDetailsGroupBox", true)[0];
					AssertEquals(SimplifiedLVSForm.Mode.CargoListHeader, form.EntryMode);
					AssertEquals("Header Details", headerDetailsGroupBox.Text);

					UnitTestUserNotification.Instance.AddOKAnswer();
					form.ContinueAndSaveButton.PerformClick();
					form.ContinueAndSaveButton.PerformClick();

					simLVS.Declaration.JE_DeclarationReference = "B10000001";
					simLVS.Invoices[0].InvoiceLines.AddNew().JI_Tariff = "1111111111";

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					form.ContinueAndSaveButton.PerformClick();

					UnitTestUserNotification.Instance.ClearMessages();
					using (var newForm = form.LastFormShownForTest as SimplifiedLVSForm)
					{
						newForm.Show();

						var newSimplifiedLVS = (SimplifiedLVS)newForm.DataSource;
						AssertEquals(importer.PK, newSimplifiedLVS.JE_OH_Importer);
						AssertEquals("0XXX", newSimplifiedLVS.CA_PortOfClearance);
						AssertEquals("3001", newSimplifiedLVS.CA_LVSCarrier);
						AssertEquals(ZString.Empty, newSimplifiedLVS.JZ_InvoiceNumber);
						AssertEquals(new ZDateTime(2015, 11, 30), newSimplifiedLVS.JZ_InvoiceDate);
					}
				}
			}
		}

		public void TestHideBrokerCodeFindBox()
		{
			var simLVS = new SimplifiedLVS(Factory);
			using (var form = new SimplifiedLVSForm(simLVS))
			{
				form.Show();
				var brokerCodeFindBox = form.simplifiedLVSUserControl.Controls.Find("BrokerCodeFindBox", true)[0];
				Assert("Hide BrokerCodeFindBox for now", !brokerCodeFindBox.Visible);
			}
		}

		protected override Form GetFormToBashCore()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var simplifiedLVS = new SimplifiedLVS(Factory);
			simplifiedLVS.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			simplifiedLVS.CA_PortOfClearance = "0XXX";

			var result = new SimplifiedLVSForm(simplifiedLVS, SimplifiedLVSForm.Mode.InvoiceDetails);
			simplifiedLVS.LoadInvoiceHeader(() => { return true; });
			simplifiedLVS.Invoices[0].InvoiceLines.AddNew();

			Factory.Save();

			return result;
		}

		IDisposable asecSetup;

		protected override void SetUp()
		{
			base.SetUp();
			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest();
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0XXX", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();
		}

		protected override void TearDown()
		{
			asecSetup.Dispose();
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.BorderWiseWeb;
			base.TearDown();
		}
	}
}
