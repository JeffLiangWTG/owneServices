using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.DE.GUI.Testing
{
	sealed class CreateDeclarationFormTest : TestCaseWithFactory
	{
		public void TestDeclarantsRefTextBox()
		{
			var declarantsRefTextBox = form.DeclarantsRefTextBox;

			CombineAssertions(() =>
			{
				AssertEquals("BindingMember", nameof(CreateDeclarationBizObj.DeclarantsReference), declarantsRefTextBox.GetBindingMember());
				AssertEquals("Placeholder text empty when not creating from Warehouse Order", ZString.Empty, declarantsRefTextBox.PlaceHolderText);
			});
		}

		public void TestDeclarantsRefTextBox_PlaceholderTextWhenCreatingFromWarehouseOrder()
		{
			var bizO = new CreateDeclarationBizObj(createFromWarehouseOrder: true);
			using (var form = new CreateDeclarationForm(bizO))
			{
				var declarantsRefTextBox = form.DeclarantsRefTextBox;
				form.Show();

				AssertEquals("Placeholder text set when creating declarations from Warehouse Order", "Auto-generated from Order", declarantsRefTextBox.PlaceHolderText);
			}
		}

		public void TestCustomsOfficeFindBox()
		{
			var customsOfficeFindBox = form.CustomsOfficeFindBox;

			AssertEquals("BindingMember", nameof(CreateDeclarationBizObj.CustomsOffice), customsOfficeFindBox.GetBindingMember());
		}

		public void TestDeclarationTypeDropEdit()
		{
			var declarationTypeDropEdit = form.DeclarationTypeDropEdit;

			AssertEquals("BindingMember", nameof(CreateDeclarationBizObj.DeclarationType), declarationTypeDropEdit.GetBindingMember());
		}

		public void TestCPCDropEdit()
		{
			var cpcDropEdit = form.CPCDropEdit;

			AssertEquals("BindingMember", nameof(CreateDeclarationBizObj.CPC), cpcDropEdit.GetBindingMember());
		}

		public void TestCustomsDeadlineEdit()
		{
			var customsDeadlineEdit = form.CustomsDeadlineEdit;

			AssertEquals("BindingMember", nameof(CreateDeclarationBizObj.CustomsDeadline), customsDeadlineEdit.GetBindingMember());
		}

		public void TestCustomsDeadlineVisibility()
		{
			var customsDeadlineEdit = form.CustomsDeadlineEdit;

			bizO.DeclarationType = "AVABR";
			AssertEquals("CustomsDeadline is visible only when DeclarationType is AVABR", true, customsDeadlineEdit.Visible);

			bizO.DeclarationType = "AZ";
			AssertEquals("CustomsDeadline is NOT visible", false, customsDeadlineEdit.Visible);
		}

		public void TestFormProperties()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Create Declaration(s)", form.CaptionResourceString.Caption);
				AssertEquals("DataSourceType", typeof(CreateDeclarationBizObj), form.DataSourceType);
				AssertEquals("Border Style", FormBorderStyle.FixedToolWindow, form.FormBorderStyle);
				AssertEquals("Verb", string.Empty, form.FormVerb);
			});
		}

		public void TestOkButtonClicked_SelectFromInventory()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Message errors present on bizObj", false, form.OkButton.Enabled);

				bizO.DeclarantsReference = "REF123";
				bizO.CustomsOffice = "DE004323";

				AssertEquals("Valid data entered, no message errors on bizObj", true, form.OkButton.Enabled);

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(
					x => AssertType<InventorySelectionHeaderForDeclarationCreation>("Business entity is for BWH", ((InventorySelectionForm)x).BusinessEntity)
				);
				form.OkButton.PerformClick();

				AssertType<InventorySelectionForm>("Inventory Selection Form should be shown", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Form should be hidden once 'OK' is clicked", false, form.Visible);

				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			});
		}

		public void TestOkButtonClicked_WarehouseOrder()
		{
			CombineAssertions(() =>
			{
				var bizO = new CreateDeclarationBizObj(createFromWarehouseOrder: true);
				using (var form = new CreateDeclarationForm(bizO))
				{
					form.Show();
					AssertEquals("Message errors present on bizObj", false, form.OkButton.Enabled);

					bizO.CustomsOffice = "DE004323";

					AssertEquals("Valid data entered, no message errors on bizObj", true, form.OkButton.Enabled);

					form.OkButton.PerformClick();

					AssertType<EmbeddedModulePopup>("Warehouse Order Selection Form should be shown", ZFormModaliser.LastFormShownDialogForTest);
					AssertEquals("Form should be hidden once 'OK' is clicked", false, form.Visible);
				}
			});
		}

		public void TestOkButtonClicked_SelectFromInventoryIPR_AVABR()
		{
			CombineAssertions(() =>
			{
				var bizO = new CreateDeclarationIPR();
				using (var form = new CreateDeclarationForm(bizO))
				{
					form.Show();
					AssertEquals("Message errors present on bizObj", false, form.OkButton.Enabled);

					bizO.CustomsDeadline = ZDateTime.Today;
					AssertEquals("Valid data entered, no message errors on bizObj", true, form.OkButton.Enabled);

					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(
						x => AssertType<InventorySelectionHeaderForIPRDeclarationCreation>("Business entity is for IPR", ((InventorySelectionForm)x).BusinessEntity)
					);
					form.OkButton.PerformClick();

					AssertType<InventorySelectionForm>("Inventory Selection Form should be shown", ZFormModaliser.LastFormShownDialogForTest);
					AssertEquals("Form should be hidden once 'OK' is clicked", false, form.Visible);

					ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				}
			});
		}

		public void TestOkButtonClicked_SelectFromInventoryIPR_non_AVABR()
		{
			CombineAssertions(() =>
			{
				var bizO = new CreateDeclarationIPR();
				using (var form = new CreateDeclarationForm(bizO))
				{
					bizO.DeclarationType = "EZA";
					bizO.DeclarantsReference = "REF123";
					bizO.CustomsOffice = "DE004323";
					form.Show();

					AssertEquals("Prerequisite - OK button enabled", true, form.OkButton.Enabled);

					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(
						x => AssertType<InventorySelectionHeaderForIPRDeclarationCreation>("Business entity is for IPR", ((InventorySelectionForm)x).BusinessEntity)
					);
					form.OkButton.PerformClick();
					ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				}
			});
		}

		public void TestShowsCreateDeclarationsForWarehouseOrderErrorToUser()
		{
			var helper = new Customs.Business.Testing.WhsDataTestHelper(Factory);
			var (whsOrder, _) = Enterprise.Customs.EU.Business.Testing.BondedWarehousingHelperTest.CreateOrderWithPick(helper, Factory, createMultiplePickLines: true);
			Factory.Save();

			var bizO = new CreateDeclarationBizObj(createFromWarehouseOrder: true);
			using (var form = new CreateDeclarationForm(bizO))
			{
				form.Show();
				AssertEquals("Message errors present on bizObj", false, form.OkButton.Enabled);

				bizO.CustomsOffice = "DE004323";
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
				{
					((EmbeddedModulePopup)dialog).EmbeddedModulePopupOKButtonStrategy?.HandleFindBoxOKButton(
						new[] { (BusinessObject)whsOrder });
				});

				form.OkButton.PerformClick();

				AssertEquals("You cannot import order REF3 with multiple picks on order line 1", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCreateDeclarationFromWarehouseOrder_CorrectFiltersApplied()
		{
			DECustomsDataRegistry.Instance.BondedWarehouseCreateDeclarationFromWarehouseOrder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var helper = new WhsDataTestHelper(Factory);
			var whsWarehouse = helper.GetNewWhsWarehouse(GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, isVirtualWarehouse: true, "WH");
			Factory.Save();

			var bizO = new CreateDeclarationBizObj(createFromWarehouseOrder: true);
			using (var form = new CreateDeclarationForm(bizO))
			{
				form.Show();
				bizO.CustomsOffice = "DE004323";

				CombineAssertions(() =>
				{
					AssertEquals("Valid data entered, no message errors on bizObj", true, form.OkButton.Enabled);

					form.OkButton.PerformClick();

					var shownForm = ZFormModaliser.LastFormShownDialogForTest as EmbeddedModulePopup;

					var filters = ((IFilterBusinessObjectDefaultsProvider)shownForm.Module_ForTest.ModuleDecisionProvider.List).FilterBusinessObjectDefaults;
					AssertContainsExactElementsInAnyOrder(new[] { "Order Status:Property=DEP", $"Warehouse:Property={whsWarehouse.PK}" }, filters.Cast<FilterBusinessObjectDefault>().Select(x => $"{x.Key}={x.Value}"));
					AssertEquals(false, filters["Order Status:Property"].IsRemovable);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			CreateDataForTest();

			bizO = new CreateDeclarationBizObj(createFromWarehouseOrder: false);
			form = new CreateDeclarationForm(bizO);
			form.Show();
		}

		protected override void TearDown()
		{
			base.TearDown();

			form.Dispose();
		}

		void CreateDataForTest()
		{
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE004323", "GERMAN OFFICE1", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry);

			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Germany, ZString.Empty, "40", "78", "", "DES2", "IMP", group: "EZL");

			Factory.Save();
		}

		CreateDeclarationForm form;
		CreateDeclarationBizObj bizO;
	}
}
