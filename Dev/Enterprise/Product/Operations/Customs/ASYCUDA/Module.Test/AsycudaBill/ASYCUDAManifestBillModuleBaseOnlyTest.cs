using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Module.Testing
{
	[TestedType(typeof(ASYCUDAManifestBillModule))]
	sealed class ASYCUDAManifestBillModuleBaseOnlyTest : ASYCUDAManifestBillModuleAbstractTest
	{
		public void TestAllowNew()
		{
			using (var module = new ASYCUDAManifestBillModule())
			{
				Assert(!module.AllowNew);
			}
		}

		public void TestOverrideMaxRowsToLoad()
		{
			var header1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header1.AMA_JobReference = "JR1";
			var bill1 = header1.Bills.AddNew();
			bill1.ABL_BillNumber = "MB1";
			var header2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header2.AMA_JobReference = "JR2";
			var bill2 = header2.Bills.AddNew();
			bill2.ABL_BillNumber = "MB2";
			Factory.Save();
			ManifestCustomsDataRegistry.Instance.MaximumSearchableManifestBills.SetTemporaryValue(Guid.Empty, Env.CurrentCompany.PK, Guid.Empty, 10);
			using (var module = new ASYCUDAManifestBillModule())
			{
				((IFilterGridModuleInternalsForTesting)module).PerformSearch();
				AssertEquals(2, module.GridCollection.Count);
			}

			ManifestCustomsDataRegistry.Instance.MaximumSearchableManifestBills.SetTemporaryValue(Guid.Empty, Env.CurrentCompany.PK, Guid.Empty, 1);
			using (var module = new ASYCUDAManifestBillModule())
			{
				((IFilterGridModuleInternalsForTesting)module).PerformSearch();
				AssertEquals(0, module.GridCollection.Count);
			}
		}

		public void TestCustomsValuesFetchHintSuspender()
		{
			BusinessObjectFactory factory;
			BusinessObjectFactory factory1;
			using (var module = new ASYCUDAManifestBillModule())
			{
				((IFilterGridModuleInternalsForTesting)module).PerformSearch();
				factory = module.GridCollection.Factory;
				Assert(factory.IsCustomsValuesFetchHintSuspended(typeof(AsycudaBill)));
				Assert(factory.IsCustomsValuesFetchHintSuspended(typeof(AsycudaManifestHeader)));
				((IFilterGridModuleInternalsForTesting)module).PerformSearch();
				factory1 = module.GridCollection.Factory;
				Assert(factory1.IsCustomsValuesFetchHintSuspended(typeof(AsycudaBill)));
				Assert(factory1.IsCustomsValuesFetchHintSuspended(typeof(AsycudaManifestHeader)));
				Assert(factory.IsCustomsValuesFetchHintSuspended(typeof(AsycudaBill)));
				Assert(factory.IsCustomsValuesFetchHintSuspended(typeof(AsycudaManifestHeader)));
			}

			Assert(!factory1.IsCustomsValuesFetchHintSuspended(typeof(AsycudaBill)));
			Assert(!factory1.IsCustomsValuesFetchHintSuspended(typeof(AsycudaManifestHeader)));
			Assert(!factory.IsCustomsValuesFetchHintSuspended(typeof(AsycudaBill)));
			Assert(!factory.IsCustomsValuesFetchHintSuspended(typeof(AsycudaManifestHeader)));
		}

		public void TestViewAndEditMenuItems_StandaloneManifest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_JobReference = "MAN001";
			var bill = header.Bills.AddNew();
			bill.ABL_GoodsLocation = "L1";
			Factory.Save();
			using (ZForm moduleForm = new ZForm())
			using (var module = (ASYCUDAManifestBillModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.ASYCUDA.ManifestBill))
			{
				var filterControl = module.EmbeddedControl;
				moduleForm.Controls.Add(filterControl);
				moduleForm.Show();
				((IFilterGridModuleInternalsForTesting)module).PerformSearch();
				Application.DoEvents();
				var viewCollection = ZModuleResults.Instance.GetPKCollectionForModule(ModuleIDs.Customs.ASYCUDA.ManifestBill);
				AssertEquals(1, viewCollection.Count);
				AssertEquals(bill.PK, viewCollection[0].PK);
				module.DisplayGrid.SelectSingleElement(bill);
				module.EditMenuItem.PerformClick();
				CheckAndCloseForm<AsycudaBillForm>(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifestBill, bill.PK, bill.PK, ODisplayMode.Browse);
				module.EditMenuItem.MenuItems.FindByText("Edit Bill").PerformClick();
				CheckAndCloseForm<AsycudaBillForm>(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifestBill, bill.PK, bill.PK, ODisplayMode.Browse);
				module.EditMenuItem.MenuItems.FindByText("Edit Manifest").PerformClick();
				CheckAndCloseForm<ManifestForm>(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest, header.PK, header.PK, ODisplayMode.Browse);
				module.ViewMenuItem.PerformClick();
				CheckAndCloseForm<AsycudaBillForm>(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifestBill, bill.PK, bill.PK, ODisplayMode.ReadOnly);
				module.ViewMenuItem.MenuItems.FindByText("View Bill").PerformClick();
				CheckAndCloseForm<AsycudaBillForm>(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifestBill, bill.PK, bill.PK, ODisplayMode.ReadOnly);
				module.ViewMenuItem.MenuItems.FindByText("View Manifest").PerformClick();
				CheckAndCloseForm<ManifestForm>(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest, header.PK, header.PK, ODisplayMode.ReadOnly);
			}
		}

		public void TestViewAndEditMenuItems_ConsoleManifest()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SetParent(consol);
			header.AMA_JobReference = "C123457";
			var bill = header.Bills.AddNew();
			bill.ABL_GoodsLocation = "L1";
			Factory.Save();
			using (ZForm moduleForm = new ZForm())
			using (var module = (ASYCUDAManifestBillModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.ASYCUDA.ManifestBill))
			{
				var filterControl = module.EmbeddedControl;
				moduleForm.Controls.Add(filterControl);
				moduleForm.Show();
				((IFilterGridModuleInternalsForTesting)module).PerformSearch();
				Application.DoEvents();
				var viewCollection = ZModuleResults.Instance.GetPKCollectionForModule(ModuleIDs.Customs.ASYCUDA.ManifestBill);
				AssertEquals(1, viewCollection.Count);
				AssertEquals(bill.PK, viewCollection[0].PK);
				module.DisplayGrid.SelectSingleElement(bill);
				module.EditMenuItem.PerformClick();
				CheckAndCloseForm<AsycudaBillForm>(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifestBill, bill.PK, bill.PK, ODisplayMode.Browse);
				module.EditMenuItem.MenuItems.FindByText("Edit Bill").PerformClick();
				CheckAndCloseForm<AsycudaBillForm>(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifestBill, bill.PK, bill.PK, ODisplayMode.Browse);
				module.EditMenuItem.MenuItems.FindByText("Edit Manifest").PerformClick();
				CheckAndCloseForm<ConsolForm>(ControllerIDs.JobConsol, consol.PK, consol.PK, ODisplayMode.Browse);
				module.ViewMenuItem.PerformClick();
				CheckAndCloseForm<AsycudaBillForm>(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifestBill, bill.PK, bill.PK, ODisplayMode.ReadOnly);
				module.ViewMenuItem.MenuItems.FindByText("View Bill").PerformClick();
				CheckAndCloseForm<AsycudaBillForm>(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifestBill, bill.PK, bill.PK, ODisplayMode.ReadOnly);
				module.ViewMenuItem.MenuItems.FindByText("View Manifest").PerformClick();
				CheckAndCloseForm<ConsolForm>(ControllerIDs.JobConsol, consol.PK, consol.PK, ODisplayMode.ReadOnly);
			}
		}

		public override void TestExceptionsFilter()
		{
			Assert("Not available on Manifest Bill module", true);
		}

		public override void TestMilestonesFilter()
		{
			Assert("Not available on Manifest Bill module", true);
		}

		public override void TestAutoAddedMilestoneDateFilter()
		{
			Assert("Not available on Manifest Bill module", true);
		}

		public override void TestAutoAddedTaskStatusFilter()
		{
			Assert("Not available on Manifest Bill module", true);
		}

		public override void TestTasksFilter()
		{
			Assert("Not available on Manifest Bill module", true);
		}

		public override void TestTriggersFilter()
		{
			Assert("Not available on Manifest Bill module", true);
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.ASYCUDA.ManifestBill;
	}
}
