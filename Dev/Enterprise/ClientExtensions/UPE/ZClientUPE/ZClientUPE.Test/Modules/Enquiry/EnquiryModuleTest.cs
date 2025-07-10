using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.Module.Testing
{
	internal class EnquiryModuleTest : TestCaseWithFactory
	{
		public void TestID()
		{
			AssertEquals(ClientModuleRegistration.Enquiry, Module.ID);
		}

		public void TestGetNewGridCollection()
		{
			AssertEquals("Grid collection MUST be Callout so that items in the grid are shown as Callout appropriately", typeof(CalloutCollection), Module.GetNewGridCollection().GetType());
		}

		public void TestController()
		{
			AssertEquals(typeof(EnquiryController), Module.GetNewController(Factory.New(typeof(Enquiry))).GetType());
		}

		public void TestGetNewFilterControl()
		{
			using (EnquiryFilterControl control = Module.GetNewFilterControl() as EnquiryFilterControl)
			{
				AssertNotNull("Should be of type EnquiryFilterControl", control);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			AssertEquals(typeof(EnquiryFilterBusinessObject), Module.GetNewFilterBusinessObject().GetType());
		}

		public void TestBulkStatusUpdatingToolbarButton()
		{
			ZToolBarButton bulkStatusUpdateButton = (ZToolBarButton)Module.ToolBarButtons.FindByText("Force to Finance");
			AssertNotNull("The button should exist", bulkStatusUpdateButton);
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB1 = mAWB.ChildBills.AddNew();
			CusHAWB hAWB2 = mAWB.ChildBills.AddNew();
			Factory.Save();
			Module.SelectedGridItems = new BusinessObject[] { hAWB1, hAWB2 };
			bulkStatusUpdateButton.PerformClick();
			AssertEquals("ShowDialog should have been called on the form", true, Module.LastCreatedBulkStatusUpdatingForm.ShowDialogWasCalled);
			AssertEquals("There should be 2 items selected for bulk update", 2, ((BulkStatusUpdating)Module.LastCreatedBulkStatusUpdatingForm.LastDataSourceForTest).ItemsToBulkUpdate.Length);
		}

		#region Implementation
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected override void TearDown()
		{
			base.TearDown();
			Module.Dispose();
		}

		EnquiryModuleForTest Module
		{
			get
			{
				if (fModule == null)
				{
					fModule = new EnquiryModuleForTest();
				}

				return fModule;
			}
		}

		EnquiryModuleForTest fModule;
		#region EnquiryModuleForTest
		class EnquiryModuleForTest : EnquiryModule
		{
			public new ZController GetNewController(BusinessObject selectedBusinessObject)
			{
				return base.GetNewController(selectedBusinessObject);
			}

			public new IFilterControl GetNewFilterControl()
			{
				return base.GetNewFilterControl();
			}

			public new FilterBusinessObject GetNewFilterBusinessObject()
			{
				return base.GetNewFilterBusinessObject();
			}

			public new IBusinessObjectCollection GetNewGridCollection()
			{
				return base.GetNewGridCollection();
			}

			public BusinessObject[] SelectedGridItems;
			public TestBulkStatusUpdatingForm LastCreatedBulkStatusUpdatingForm;
			protected override BulkStatusUpdatingForm NewBulkStatusUpdatingForm(BusinessObject[] selectedItems)
			{
				LastCreatedBulkStatusUpdatingForm = new TestBulkStatusUpdatingForm(FilterBusinessObject, SelectedGridItems);
				return LastCreatedBulkStatusUpdatingForm;
			}
		}
		#endregion
		#endregion
	}
}
