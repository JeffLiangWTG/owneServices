using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(ConvertToStandAloneDeclarationMenuItem))]
	sealed class ConvertToStandAloneDeclarationMenuItemTest : TestCaseWithFactory
	{
		public void TestConvertToStandAloneDeclarationMenu_SingleBillSuccessiveConvertClicksOpensJobDeclarationFormOnce()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			var shipperOrg = Factory.NewWithValidTestData<OrgHeader>();
			BuildBillWithoutConvertValidationError(header, consigneeOrg.MainAddress, shipperOrg.MainAddress);
			Factory.Save();

			using (var form = new ManifestForm(header))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");
				billsGrid.SelectAllElements();
				billsGrid.OnPopup_CallForTesting();

				var amountOfTimesFormShown = 0;
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(obj =>
				{
					if (obj is BaseJobDeclarationForm jobDecForm)
					{
						amountOfTimesFormShown++;
					}
				});

				var convertMenu = billsGrid.ContextMenu.MenuItems.Cast<MenuItem>().FindByText("Convert to Stand Alone Declaration");
				convertMenu.PerformClick();
				Assert(ZFormModaliser.LastFormShownDialogForTest is JobDeclarationForm);
				AssertEquals("Only one JobDeclarationForm is opened", 1, amountOfTimesFormShown);

				amountOfTimesFormShown = 0;

				billsGrid.SelectAllElements();
				convertMenu = billsGrid.ContextMenu.MenuItems.Cast<MenuItem>().FindByText("Convert to Stand Alone Declaration");
				convertMenu.PerformClick();
				Assert(ZFormModaliser.LastFormShownDialogForTest is JobDeclarationForm);
				AssertEquals("Only one JobDeclarationForm is opened (Ensure StandAloneDeclarationConverter_OpenForm is cleaned up)", 1, amountOfTimesFormShown);
			}
		}

		[RequiresSTA]
		public void TestConvertToStandAloneDeclarationMenu_ShowSuccessNotifactionIfSelectedMutipleBills()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			var shipperOrg = Factory.NewWithValidTestData<OrgHeader>();
			BuildBillWithoutConvertValidationError(header, consigneeOrg.MainAddress, shipperOrg.MainAddress);
			BuildBillWithoutConvertValidationError(header, consigneeOrg.MainAddress, shipperOrg.MainAddress);
			BuildBillWithoutConvertValidationError(header, consigneeOrg.MainAddress, shipperOrg.MainAddress);
			BuildBillWithoutConvertValidationError(header, consigneeOrg.MainAddress, shipperOrg.MainAddress);
			Factory.Save();

			using (var form = new ManifestForm(header))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");
				billsGrid.SelectAllElements();
				billsGrid.OnPopup_CallForTesting();

				var convertMenu = billsGrid.ContextMenu.MenuItems.Cast<MenuItem>().FindByText("Convert to Stand Alone Declaration");
				convertMenu.PerformClick();
				AssertEquals(0, Application.OpenForms.OfType<JobDeclarationForm>().Count());
				AssertEquals("Expected a prompt to declare conversion success", "Bills have been converted to Stand Alone Declarations.", UnitTestUserNotification.Instance.LastMessage.Text);

				var bill = billsGrid.GetFirstSelectedRow();
				Assert(bill.ReadOnly);
			}
		}

		public void TestConvertToStandAloneDeclarationMenu_ShowErrorWhenNoBillsSelected()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			Factory.Save();

			using (var form = new ManifestForm(header))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");
				billsGrid.OnPopup_CallForTesting();

				var convertMenu = billsGrid.ContextMenu.MenuItems.Cast<MenuItem>().FindByText("Convert to Stand Alone Declaration");
				convertMenu.PerformClick();

				CombineAssertions(() =>
				{
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals("No bills have been selected. Please select bill(s) before creating Stand Alone Declaration.", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		AsycudaBill BuildBillWithoutConvertValidationError(AsycudaManifestHeader header, OrgAddress consignee, OrgAddress shipper)
		{
			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = "ACC";
			bill.ABL_OA_Consignee = consignee.PK;
			bill.ABL_OA_Shipper = shipper.PK;
			return bill;
		}

		public void TestConvertToStandAloneDeclarationMenu_ShowSaveChangeMessage()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			Factory.Save();

			using (var form = new ManifestForm(header))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");

				header.AMA_ManifestDescription = "description";
				AssertSaveChangesErrorShowsUp(billsGrid);

				header.ClearHasChanges();
				bill.ABL_GoodsDescription = "description";
				AssertSaveChangesErrorShowsUp(billsGrid);

				bill.ClearHasChanges();
				var item = bill.PackedItems.AddNew();
				item.API_GoodsDescription = "description";
				AssertSaveChangesErrorShowsUp(billsGrid);

				bill.ClearHasChanges();
				var pack = bill.Packs.AddNew();
				pack.APA_GoodsDescription = "description";
				AssertSaveChangesErrorShowsUp(billsGrid);
			}
		}

		void AssertSaveChangesErrorShowsUp(ZGrid billsGrid)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			billsGrid.SelectAllElements();
			billsGrid.OnPopup_CallForTesting();

			UnitTestUserNotification.Instance.AddOKAnswer();
			var convertMenu = billsGrid.ContextMenu.MenuItems.Cast<MenuItem>().FindByText("Convert to Stand Alone Declaration");
			convertMenu.PerformClick();

			Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertEquals("Please save any changes made before creating Stand Alone Declaration.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[RequiresSTA]
		public void TestConvertToStandAloneMenu_ShowDeclarationExistsMessage()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			header.Bills.AddNew();
			header.Bills.AddNew();
			bill1.EntrySummaryReferenceNumber = "JB001";
			bill1.ABL_IsActive = false;
			bill1.ABL_BillNumber = "BN001";
			Factory.Save();

			using (var form = new ManifestForm(header))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");
				billsGrid.SelectAllElements();
				billsGrid.OnPopup_CallForTesting();

				var convertMenu = billsGrid.ContextMenu.MenuItems.Cast<MenuItem>().FindByText("Convert to Stand Alone Declaration");
				UnitTestUserNotification.Instance.AddOKAnswer();
				convertMenu.PerformClick();

				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("A Stand Alone Declaration cannot be created for a Bill that already has an existing Stand Alone Declaration.\r\nBN001", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConvertToStandAloneMenu_WhenSelectAtLeastABillWithoutImporterOrExporterOrg_ShouldShowOrgNotFoundMessage()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillStatus = "ACC";

			var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			var shipperOrg = Factory.NewWithValidTestData<OrgHeader>();
			BuildBillWithoutConvertValidationError(header, consigneeOrg.MainAddress, shipperOrg.MainAddress);

			Factory.Save();

			ConvertToStandAloneAndAssertMessage(header, "Importer or Exporter organization not found for at least one bill selected, would you like to convert them into organizations?");
		}

		public void TestConvertToStandAloneMenu_WhenSelectSingleBillWithoutImporterAndExporterOrg_ShouldShowOrgNotFoundMessage()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.Bills.AddNew().ABL_BillStatus = "ACC";
			Factory.Save();

			ConvertToStandAloneAndAssertMessage(header, "Importer and Exporter organizations not found, would you like to convert to organizations with details defaulted from 'Importer' and 'Exporter' fields?");
		}

		public void TestConvertToStandAloneMenu_WhenSelectSingleBillWithoutImporterOrg_ShouldShowOrgNotFoundMessage()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var shipperOrg = Factory.NewWithValidTestData<OrgHeader>();

			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = "ACC";
			bill.ABL_OA_Shipper = shipperOrg.MainAddress.PK;

			Factory.Save();

			ConvertToStandAloneAndAssertMessage(header, "Importer organization not found, would you like to convert to organization with details defaulted from 'Importer' fields?");
		}

		public void TestConvertToStandAloneMenu_WhenSelectSingleBillWithoutExporterOrg_ShouldShowOrgNotFoundMessage()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();

			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = "ACC";
			bill.ABL_OA_Consignee = consigneeOrg.MainAddress.PK;
			
			Factory.Save();

			ConvertToStandAloneAndAssertMessage(header, "Exporter organization not found, would you like to convert to organization with details defaulted from 'Exporter' fields?");
		}

		[RequiresSTA]
		public void TestConvertToStandAloneMenu_WhenOrgsAreNotCreated_ShouldAllowConvert()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = "ACC";
			Factory.Save();

			using (var form = new ManifestForm(header))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");
				billsGrid.SelectAllElements();
				billsGrid.OnPopup_CallForTesting();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				var convertMenu = billsGrid.ContextMenu.MenuItems.Cast<MenuItem>().FindByText("Convert to Stand Alone Declaration");
				convertMenu.PerformClick();

				Assert(ZFormModaliser.LastFormShownDialogForTest is JobDeclarationForm);
			}
		}

		public void TestConvertToStandAloneMenu_ShowNewOrgLinkedMessage()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.ABL_ConsigneeName = "COCONUT ENTERTAINMENT";
			bill1.ABL_ConsigneeStreet1 = "Cardigan Street";
			bill1.ABL_BillStatus = "ACC";
			var shipperOrg = Factory.NewWithValidTestData<OrgHeader>();
			bill1.ABL_OA_Shipper = shipperOrg.MainAddress.PK;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "COCONUT ENTERTAINMENT";
			orgHeader.MainAddress.OA_Address1 = "Cardigan Street";
			Factory.Save();

			using (var form = new ManifestForm(header))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");
				billsGrid.SelectAllElements();
				billsGrid.OnPopup_CallForTesting();

				UnitTestUserNotification.Instance.AddYesAnswer();
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(shownForm =>
				{
					var addressSelectionForm = shownForm as EUH7SimilarAddressesSelectionForm;
					AssertNotNull("Should open similar addresses selection form", addressSelectionForm);

					var billGrid = addressSelectionForm.Controls.Find("BillAddressDisplayGrid", true)[0] as ZDisplayGrid;
					billGrid.Select(0);
					var similarAddressGrid = addressSelectionForm.Controls.Find("SimilarOrgsDisplayGrid", true)[0] as ZDisplayGrid;
					similarAddressGrid.Select(0);

					var selectAddressButton = addressSelectionForm.Controls.Find("SelectOrgButton", true).Single() as ZButton;
					selectAddressButton.PerformClick();
				});

				var convertMenu = billsGrid.ContextMenu.MenuItems.Cast<MenuItem>().FindByText("Convert to Stand Alone Declaration");
				convertMenu.PerformClick();

				Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("New organization(s) have been linked to the selected bills. Please click 'Convert' again to create Stand Alone Declarations.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region SaveAddressChangesAutomatically

		public void TestConvertToStandAloneMenu_SaveAddressChangesAutomatically()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_ConsigneeName = "COCONUT ENTERTAINMENT";
			bill.ABL_ConsigneeStreet1 = "Cardigan Street";
			bill.ABL_BillStatus = "ACC";
			var shipperOrg = Factory.NewWithValidTestData<OrgHeader>();
			bill.ABL_OA_Shipper = shipperOrg.MainAddress.PK;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "COCONUT ENTERTAINMENT";
			orgHeader.MainAddress.OA_Address1 = "Cardigan Street";
			Factory.Save();

			var convertMenu = new ConvertToStandAloneDeclarationMenuItem(() => [bill], shouldAutomaticallySaveAddressChanges: true);

			UnitTestUserNotification.Instance.AddYesAnswer();
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormShown(shownForm =>
			{
				if (shownForm is EUH7SimilarAddressesSelectionForm addressSelectionForm)
				{
					var billGrid = addressSelectionForm.Controls.Find("BillAddressDisplayGrid", true)[0] as ZDisplayGrid;
					billGrid.Select(0);
					var similarAddressGrid = addressSelectionForm.Controls.Find("SimilarOrgsDisplayGrid", true)[0] as ZDisplayGrid;
					similarAddressGrid.Select(0);

					var selectAddressButton = addressSelectionForm.Controls.Find("SelectOrgButton", true).Single() as ZButton;
					selectAddressButton.PerformClick();
				}
			});

			convertMenu.MenuAction.Invoke();

			CombineAssertions(() =>
			{
				AssertNotEquals("New organization(s) have been linked to the selected bills. Please click 'Convert' again to create Stand Alone Declarations.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Conversion has proceeded", ZFormModaliser.LastFormShownDialogForTest is JobDeclarationForm);
				bill.Reload();
				AssertEquals(orgHeader.MainAddress.PK, bill.ABL_OA_Consignee);
			});
		}

		public void TestConvertToStandAloneMenu_SaveAddressChangesAutomatically_HandleZSaveConcurrencyException()
		{
			var differentUser = Factory.NewWithValidTestData<GlbStaff>();
			differentUser.GS_FullName = "Different User";
			ZDateTime expectedLastUpdateTime = default;

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_ConsigneeName = "COCONUT ENTERTAINMENT";
			bill.ABL_ConsigneeStreet1 = "Cardigan Street";
			bill.ABL_BillStatus = "ACC";
			var shipperOrg = Factory.NewWithValidTestData<OrgHeader>();
			bill.ABL_OA_Shipper = shipperOrg.MainAddress.PK;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "COCONUT ENTERTAINMENT";
			orgHeader.MainAddress.OA_Address1 = "Cardigan Street";
			Factory.Save();

			var convertMenu = new ConvertToStandAloneDeclarationMenuItem(() => [bill], shouldAutomaticallySaveAddressChanges: true);

			UnitTestUserNotification.Instance.AddYesAnswer();
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormShown(shownForm =>
			{
				if (shownForm is EUH7SimilarAddressesSelectionForm addressSelectionForm)
				{
					modifyBillInNewFactory(bill);

					var billGrid = addressSelectionForm.Controls.Find("BillAddressDisplayGrid", true)[0] as ZDisplayGrid;
					billGrid.Select(0);
					var similarAddressGrid = addressSelectionForm.Controls.Find("SimilarOrgsDisplayGrid", true)[0] as ZDisplayGrid;
					similarAddressGrid.Select(0);

					var selectAddressButton = addressSelectionForm.Controls.Find("SelectOrgButton", true).Single() as ZButton;
					selectAddressButton.PerformClick();
				}
			});

			convertMenu.MenuAction.Invoke();
			var expectedLastUpdateTimeFormatted = expectedLastUpdateTime.ToLocalBranchTime().ToString("dd MMM yyyy HH:mm:ss");
			var expectedMessage = @$"One or more selected bills have been changed in another session. Please click 'Convert' again to create Stand Alone Declarations.

The following objects had changes:
Manifest Bill (Different User @ {expectedLastUpdateTimeFormatted})
	Goods Description (on Bill)
	System Last Edit User
";

			CombineAssertions(() =>
			{
				Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertContainsExactLinesInExactOrder(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				bill.Reload();
				AssertEquals(ZGuid.Empty, bill.ABL_OA_Consignee);
			});

			void modifyBillInNewFactory(AsycudaBill bill)
			{
				using (CurrentUserChanger.SwitchToNewUserTemporarily(differentUser.GS_LoginName))
				{
					var newFactory = NewFactory();
					newFactory.RefreshEnabled = false;
					var billInNewFactory = newFactory.Load<AsycudaBill>(bill.PK);
					billInNewFactory.ABL_GoodsDescription = "New Goods Description";
					newFactory.Save();

					expectedLastUpdateTime = billInNewFactory.ABL_SystemLastEditTimeUtc;
				}
			}
		}

		public void TestConvertToStandAloneMenu_SaveAddressChangesAutomatically_HandleZSaveException()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_ConsigneeName = "COCONUT ENTERTAINMENT";
			bill.ABL_ConsigneeStreet1 = "Cardigan Street";
			bill.ABL_BillStatus = "ACC";
			var shipperOrg = Factory.NewWithValidTestData<OrgHeader>();
			bill.ABL_OA_Shipper = shipperOrg.MainAddress.PK;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "COCONUT ENTERTAINMENT";
			orgHeader.MainAddress.OA_Address1 = "Cardigan Street";
			Factory.Save();

			var convertMenu = new ConvertToStandAloneDeclarationMenuItem(() => [bill], shouldAutomaticallySaveAddressChanges: true);

			UnitTestUserNotification.Instance.AddYesAnswer();
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormShown(shownForm =>
			{
				if (shownForm is EUH7SimilarAddressesSelectionForm addressSelectionForm)
				{
					var testInnerException = new ZDataException(new Exception(), null, Db.Connection);
					testInnerException.SetFriendlyMessageForTest("This is the friendly message");
					Factory.Saving += (f) => throw new ZSaveException(testInnerException, f);
					var billGrid = addressSelectionForm.Controls.Find("BillAddressDisplayGrid", true)[0] as ZDisplayGrid;
					billGrid.Select(0);
					var similarAddressGrid = addressSelectionForm.Controls.Find("SimilarOrgsDisplayGrid", true)[0] as ZDisplayGrid;
					similarAddressGrid.Select(0);

					var selectAddressButton = addressSelectionForm.Controls.Find("SelectOrgButton", true).Single() as ZButton;
					selectAddressButton.PerformClick();
				}
			});

			convertMenu.MenuAction.Invoke();

			CombineAssertions(() =>
			{
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("This is the friendly message", UnitTestUserNotification.Instance.LastMessage.Text);
				bill.Reload();
				AssertEquals(ZGuid.Empty, bill.ABL_OA_Consignee);
			});
		}

		#endregion

		public void TestConvertToStandAloneMenu_NoMessageShowsGivenAllOrgConversionIgnored()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_ConsigneeName = "COCONUT ENTERTAINMENT";
			bill.ABL_ConsigneeStreet1 = "Cardigan Street";
			bill.ABL_BillStatus = "ACC";
			var shipperOrg = Factory.NewWithValidTestData<OrgHeader>();
			bill.ABL_OA_Shipper = shipperOrg.MainAddress.PK;
			Factory.Save();

			using (var form = new ManifestForm(header))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");
				billsGrid.SelectAllElements();
				billsGrid.OnPopup_CallForTesting();

				UnitTestUserNotification.Instance.AddYesAnswer();
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(shownForm =>
				{
					if (shownForm is EUH7SimilarAddressesSelectionForm addressSelectionForm)
					{
						var billGrid = addressSelectionForm.Controls.Find("BillAddressDisplayGrid", true)[0] as ZDisplayGrid;
						billGrid.Select(0);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						var selectAddressButton = addressSelectionForm.Controls.Find("IgnoreButton", true).Single() as ZButton;
						selectAddressButton.PerformClick();
					}
					else if (shownForm is JobDeclarationForm jobDeclarationForm)
					{
						jobDeclarationForm.BusinessEntity.Factory.Save();
					}
				});

				var convertMenu = billsGrid.ContextMenu.MenuItems.Cast<MenuItem>().FindByText("Convert to Stand Alone Declaration");
				convertMenu.PerformClick();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(bill.HasBeenConvertedToStandaloneDeclaration);
			}
		}

		public void TestConvertToStandAloneMenu_ShowCanNotCreateForClearedStatusMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				var bill1 = header.Bills.AddNew();
				bill1.ABL_BillStatus = "CLR";
				bill1.ABL_BillNumber = "BN001";
				Factory.Save();

				using (var form = new ManifestForm(header))
				{
					form.Show();

					var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
					var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
					var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
					mainTabControl.SelectedTab = billsAndPacksTabPage;
					var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");
					billsGrid.SelectAllElements();
					billsGrid.OnPopup_CallForTesting();

					var convertMenu = billsGrid.ContextMenu.MenuItems.Cast<MenuItem>().FindByText("Convert to Stand Alone Declaration");
					UnitTestUserNotification.Instance.AddOKAnswer();
					convertMenu.PerformClick();

					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals("A Stand Alone Declaration cannot be created for a Bill with a 'Cleared' Customs Status.\r\nBN001", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestConvertToStandAloneMenu_ShowCanNotCreateForReleasedStatusMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				var bill1 = header.Bills.AddNew();
				bill1.ABL_BillStatus = "REL";
				bill1.ABL_BillNumber = "BN001";
				Factory.Save();

				using (var form = new ManifestForm(header))
				{
					form.Show();

					var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
					var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
					var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
					mainTabControl.SelectedTab = billsAndPacksTabPage;
					var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");
					billsGrid.SelectAllElements();
					billsGrid.OnPopup_CallForTesting();

					var convertMenu = billsGrid.ContextMenu.MenuItems.Cast<MenuItem>().FindByText("Convert to Stand Alone Declaration");
					UnitTestUserNotification.Instance.AddOKAnswer();
					convertMenu.PerformClick();

					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals("A Stand Alone Declaration cannot be created for a Bill with a 'Released' Customs Status.\r\nBN001", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestConvertToStandAloneMenu_ErrorMessageFormat()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				GenerateBillsWithReleaseStatus(header, 11);
				Factory.Save();

				using (var form = new ManifestForm(header))
				{
					form.Show();

					var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
					var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
					var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
					mainTabControl.SelectedTab = billsAndPacksTabPage;
					var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");
					billsGrid.SelectAllElements();
					billsGrid.OnPopup_CallForTesting();

					var convertMenu = billsGrid.ContextMenu.MenuItems.Cast<MenuItem>().FindByText("Convert to Stand Alone Declaration");
					UnitTestUserNotification.Instance.AddOKAnswer();
					convertMenu.PerformClick();

					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals("A Stand Alone Declaration cannot be created for a Bill with a 'Released' Customs Status.\r\nBN001\r\nBN002\r\nBN003\r\nBN004\r\nBN005\r\nBN006\r\nBN007\r\nBN008\r\n...\r\nBN0011", UnitTestUserNotification.Instance.LastMessage.Text);

					billsGrid.SelectAllElements();
					billsGrid.UnSelect(10);
					billsGrid.OnPopup_CallForTesting();

					UnitTestUserNotification.Instance.AddOKAnswer();
					convertMenu.PerformClick();

					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals("A Stand Alone Declaration cannot be created for a Bill with a 'Released' Customs Status.\r\nBN001\r\nBN002\r\nBN003\r\nBN004\r\nBN005\r\nBN006\r\nBN007\r\nBN008\r\nBN009\r\nBN0010", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		void GenerateBillsWithReleaseStatus(AsycudaManifestHeader header, int number)
		{
			var count = 1;
			while (count <= number)
			{
				var bill = header.Bills.AddNew();
				bill.ABL_BillStatus = "REL";
				bill.ABL_BillNumber = "BN00" + count++;
			}
		}

		void ConvertToStandAloneAndAssertMessage(AsycudaManifestHeader header, string messageText)
		{
			using (var form = new ManifestForm(header))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");
				billsGrid.SelectAllElements();
				billsGrid.OnPopup_CallForTesting();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				var convertMenu = billsGrid.ContextMenu.MenuItems.Cast<MenuItem>().FindByText("Convert to Stand Alone Declaration");
				convertMenu.PerformClick();

				var message = UnitTestUserNotification.Instance.PreviousMessages.FirstOrDefault(x => x.Text == messageText);

				AssertNotNull(message);
				Assert(message.WasQuestion);
			}
		}
	}
}
