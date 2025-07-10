using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Module.Testing
{
	[TestedType(typeof(ASYCUDAManifestBillController))]
	sealed class ASYCUDAManifestBillControllerTest : ZControllerBasherTest
	{
		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(AsycudaBill), new ASYCUDAManifestBillController().TypeOfTopLevelBusinessObject);
		}

		public void TestPreviousNextBillSwitching()
		{
			var header1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header1.AMA_JobReference = "MAN001";
			var bill1 = header1.Bills.AddNew();
			bill1.ABL_GoodsLocation = "L1";
			var header2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header2.AMA_JobReference = "MAN002";
			var bill2 = header2.Bills.AddNew();
			bill2.ABL_GoodsLocation = "L2";
			Factory.Save();
			using (ZForm moduleForm = new ZForm())
			using (var module = (ASYCUDAManifestBillModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.ASYCUDA.ManifestBill))
			{
				var filterControl = module.EmbeddedControl;
				moduleForm.Controls.Add(filterControl);
				moduleForm.Show();
				((IFilterGridModuleInternalsForTesting)module).PerformSearch();
				((IFilterGridModuleInternalsForTesting)module).GridCollection.ApplySort(new SortInfo(AsycudaBill.Schema.ABL_GoodsLocation, ListSortDirection.Ascending));
				Application.DoEvents();
				var viewCollection = ZModuleResults.Instance.GetPKCollectionForModule(ModuleIDs.Customs.ASYCUDA.ManifestBill);
				AssertEquals(2, viewCollection.Count);
				AssertEquals(bill1.PK, viewCollection[0].PK);
				AssertEquals(bill2.PK, viewCollection[1].PK);
				module.DisplayGrid.SelectSingleElement(bill1);
				var menuItem = module.FormActionMenu.Single(mi => mi.Text == "&View");
				menuItem.PerformClick();
				using (var form = (AsycudaBillForm)OpenedFormCache.GetInstance().GetForm(bill1.PK.ToGuid(), ControllerIDs.Customs.ASYCUDA.ASYCUDAManifestBill.ToString()))
				{
					Application.DoEvents();
					AssertEquals(bill1.PK, form.BusinessEntity.PK);
					var previousNextControlProvider = (IPreviousNextControlProvider)form;
					var previousNextControl = previousNextControlProvider.PreviousNextControlForTesting;
					AssertNotNull("previousNextControl", previousNextControl);
					AssertEquals(true, previousNextControl.Visible);
					previousNextControl.FireNextButtonForTesting();
					Application.DoEvents();
					using (var form2 = (AsycudaBillForm)OpenedFormCache.GetInstance().GetForm(bill2.PK.ToGuid(), ControllerIDs.Customs.ASYCUDA.ASYCUDAManifestBill.ToString()))
					{
						AssertEquals(bill2.PK, form2.BusinessEntity.PK);
					}
				}
			}
		}

		public override Type ControllerToBashType => typeof(ASYCUDAManifestBillController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.ASYCUDA.ASYCUDAManifestBill;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			Factory.Save();
			return bill;
		}
	}
}
