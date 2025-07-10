using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Module.Testing
{
	[TestedType(typeof(EUH7BillModule))]
	public class EUH7BillModuleTest : ZModuleBasherTest
	{
		public void TestAdditionalActionsMenuItems()
		{
			using (var module = new EUH7BillModule())
			{
				var menuItem = module.FormActionMenu.FindByText("Convert to Stand Alone Declaration");
				AssertNotNull("Convert To Stand Alone Declaration action menu is added", menuItem);
			}
		}

		[RequiresSTA]
		public void TestConvertToStandAloneDeclarationActionMenu_OpenDeclarationForm()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "Consignee";
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = "ACC";
			bill.ABL_OA_Consignee = orgHeader.MainAddress.PK;
			bill.ABL_OA_Shipper = orgHeader.MainAddress.PK;
			Factory.Save();

			using (var module = new EUH7BillModule())
			using (var form = (ZForm)module.ShowPopup())
			{
				var filterControl = (EUH7BillFilterStripControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.Select(0);

				var toolStripItem = ((ZToolStrip)form.Controls.Find("toolstrip", true)[0])
					.Items.Cast<ToolStripItem>()
					.FirstOrDefault(item => item.Text == "Convert to Stand Alone Declaration");
				toolStripItem.PerformClick();
				Assert(ZFormModaliser.LastFormShownDialogForTest is JobDeclarationForm);
			}
		}

		[RequiresSTA]
		public void TestConvertToStandAloneDeclarationActionMenu_LoadsBillsInNewFactory()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "Consignee";
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = "ACC";
			bill.ABL_OA_Consignee = orgHeader.MainAddress.PK;
			bill.ABL_OA_Shipper = orgHeader.MainAddress.PK;
			Factory.Save();

			using (var module = new EUH7BillModule())
			using (var form = new ZForm())
			{
				var filterControl = (EUH7BillFilterStripControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.Select(0);
				(grid.SelectedElements[0] as AsycudaBill).ABL_GoodsDescription = "new description";

				var convertMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Convert to Stand Alone Declaration");
				convertMenuItem.PerformClick();
				Assert(ZFormModaliser.LastFormShownDialogForTest is JobDeclarationForm);
			}
		}

		[RequiresSTA]
		public void TestConvertToStandAloneDeclarationActionMenu_SavesAddressChangesAutomatically()
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

			using (var module = new EUH7BillModule())
			using (var form = new ZForm())
			{
				var filterControl = (EUH7BillFilterStripControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.Select(0);

				UnitTestUserNotification.Instance.AddYesAnswer();
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(shownForm =>
				{
					if (shownForm is EUH7SimilarAddressesSelectionForm addressSelectionForm)
					{
						AssertNotNull("Should open similar addresses selection form", addressSelectionForm);

						var billGrid = addressSelectionForm.Controls.Find("BillAddressDisplayGrid", true)[0] as ZDisplayGrid;
						billGrid.Select(0);
						var similarAddressGrid = addressSelectionForm.Controls.Find("SimilarOrgsDisplayGrid", true)[0] as ZDisplayGrid;
						similarAddressGrid.Select(0);

						var selectAddressButton = addressSelectionForm.Controls.Find("SelectOrgButton", true).Single() as ZButton;
						selectAddressButton.PerformClick();
					}
				});

				var convertMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Convert to Stand Alone Declaration");
				convertMenuItem.PerformClick();
				CombineAssertions(() =>
				{
					AssertNotEquals("New organization(s) have been linked to the selected bills. Please click 'Convert' again to create Stand Alone Declarations.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(ZFormModaliser.LastFormShownDialogForTest is JobDeclarationForm);
					bill.Reload();
					AssertEquals(orgHeader.MainAddress.PK, bill.ABL_OA_Consignee);
				});
			}
		}

		public virtual void TestGetNewFilterBusinessObject()
		{
			using (var module = new EUH7BillModuleForTest())
			{
				AssertType<EUH7BillFilterBusinessObject>(module.GetNewFilterBusinessObject_Exposed());
			}
		}

		public virtual void TestGetNewFilterControl()
		{
			using (var module = new EUH7BillModuleForTest())
			using (var control = module.GetNewFilterControl_Exposed())
			{
				AssertType<EUH7BillFilterStripControl>(control);
			}
		}

		public virtual void TestGetNewGridCollection()
		{
			using (var module = new EUH7BillModuleForTest())
			{
				AssertType<EUH7BillModuleCollection<AsycudaBill>>(module.GridCollection);
			}
		}

		class EUH7BillModuleForTest : EUH7BillModule
		{
			public IFilterControl GetNewFilterControl_Exposed() => base.GetNewFilterControl();

			public FilterBusinessObject GetNewFilterBusinessObject_Exposed() => base.GetNewFilterBusinessObject();
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.EUH7Bill;

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var header = (AsycudaManifestHeader)base.GetNewBusinessObjectForHelperFilterTests(factory, typeof(AsycudaManifestHeader));
			header.AMA_JobReference = ZString.Empty;
			return header.Bills.AddNew();
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			base.AddTestObjects(collection);
			var bill = Factory.New<AsycudaManifestHeader>().Bills.AddNew();
			collection.Add(bill);
		}
	}
}
