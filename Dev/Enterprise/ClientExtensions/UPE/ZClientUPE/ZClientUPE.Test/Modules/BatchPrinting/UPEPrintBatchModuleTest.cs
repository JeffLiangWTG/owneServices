using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Business.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	[TestedType(typeof(UPEPrintBatchModule))]
	sealed class UPEPrintBatchModuleTest : ZModuleBasherTest
	{
		public void TestLicenseSecurityCheckpoints()
		{
			AssertEquals("LicenceCheckPoint", Env.Licence.AlwaysAllow, Module.LicenceCheckPoint);
			AssertEquals("SecurityCheckpoint", Env.Security.None, Module.SecurityCheckpoint);
		}

		public void TestGetNewController()
		{
			AssertNull("Controller not required for print batch module", Module.GetNewController(null));
		}

		public void TestGetNewFilterControl()
		{
			using (UPEPrintBatchFilterControl filterControl = Module.GetNewFilterControl() as UPEPrintBatchFilterControl)
			{
				AssertNotNull("Type should be UPEInvoicePrintingFilterControl", filterControl);
			}
		}

		public void TestGetNewGridCollection()
		{
			UPEPrintBatchCollection gridCollection = Module.GetNewGridCollection() as UPEPrintBatchCollection;
			AssertNotNull(gridCollection);
		}

		public void TestGetNewFilterBusinessObject()
		{
			AssertEquals(typeof(UPEPrintBatchFilterBusinessObject), Module.GetNewFilterBusinessObject().GetType());
		}

		#region Disallow New/Edit/View/Delete

		public void TestDontAllowNewEditDelete()
		{
			AssertEquals(false, Module.AllowNew);
			AssertEquals(false, Module.AllowEdit);
			AssertEquals(false, Module.AllowDelete);
		}

		public void TestDontAllowViewRecord()
		{
			TestUPEPrintBatch printBatch = (TestUPEPrintBatch)new TestUPEPrintBatch.Loader(Factory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.TaxInvoice);
			AssertEquals("No view form should be shown", null, Module.ShowViewForm(printBatch));
		}

		public void TestViewMenuItemRemoved()
		{
			AssertNull("View menu should not be available", Module.ViewMenuItem);
		}

		#endregion

		#region Batch Print

		public void TestPrintBatch()
		{
			Cursor originalCursor = Module.EmbeddedControl.Cursor;

			Callout callout = Factory.NewWithValidTestData<Callout>();
			TestUPEPrintBatch printBatch = (TestUPEPrintBatch)new TestUPEPrintBatch.Loader(Factory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.TaxInvoice);
			Module.PrintBatch(printBatch);

			AssertEquals("Invoice batch should be printed", true, printBatch.PrintCalled);
			AssertEquals("PerformSearch is required after print so that last printed date / printed count are updated", true, Module.PerformSearchCalled);
			AssertEquals("Mouse cursor should be restored after", originalCursor, Module.EmbeddedControl.Cursor);
		}

		public void TestBatchPrintActionMenuItem()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			Callout callout1 = (Callout)mAWB.ChildBills.AddNew(typeof(Callout));
			QueueTaxInvoiceForBatchPrintAndSave(callout1);
			Callout callout2 = (Callout)mAWB.ChildBills.AddNew(typeof(Callout));
			QueueTaxInvoiceForBatchPrintAndSave(callout2);
			Factory.Save();

			TestUPEPrintBatch printBatch = (TestUPEPrintBatch)new TestUPEPrintBatch.Loader(Factory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.TaxInvoice);
			Module.SetSelectedGridElements(new BusinessObject[] { printBatch });
			MenuItem printMenuItem = Module.GetNewAdditionalMenuItems().FindByText("Print");
			AssertNotNull("Print button should exist", printMenuItem);

			printMenuItem.PerformClick();
			AssertNotNull("Selected print batch should have been attempted to be printed", printBatch.PrintCalled);
		}

		public void TestBatchPrintActionMenuItem_WhenNoCalloutItemsSelected()
		{
			Module.SetSelectedGridElements(System.Array.Empty<BusinessObject>());
			MenuItem printMenuItem = Module.GetNewAdditionalMenuItems().FindByText("Print");
			AssertNotNull("Print button should exist", printMenuItem);

			printMenuItem.PerformClick();
			AssertEquals("A warning should be shown to the user if no items were selected", true, UnitTestUserNotification.Instance.LastMessage.WasWarning);
		}

		void QueueTaxInvoiceForBatchPrintAndSave(Callout callout)
		{
			UPEPrintBatch currentPrintBatch = new UPEPrintBatch.Loader(Factory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.TaxInvoice);
			currentPrintBatch.QueueForBatchPrintAndSave(callout, new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice().PK);
		}

		#endregion

		#region Test Classes

		class TestUPEPrintBatchModule : UPEPrintBatchModule
		{
			public new ZController GetNewController(BusinessObject selectedBusinessObject)
			{
				return base.GetNewController(selectedBusinessObject);
			}

			public new IZForm ShowViewForm(BusinessObject selectedBusinessObject)
			{
				return base.ShowViewForm(selectedBusinessObject);
			}

			public new MenuItem[] GetNewActionMenuItems()
			{
				return base.GetNewActionMenuItems();
			}

			public new MenuItem[] GetNewAdditionalMenuItems()
			{
				return base.GetNewAdditionalMenuItems();
			}

			public new IFilterControl GetNewFilterControl()
			{
				return base.GetNewFilterControl();
			}

			public new IBusinessObjectCollection GetNewGridCollection()
			{
				return base.GetNewGridCollection();
			}

			public new FilterBusinessObject GetNewFilterBusinessObject()
			{
				return base.GetNewFilterBusinessObject();
			}

			public new BusinessObjectFactory Factory
			{
				get { return base.Factory; }
			}

			public bool PerformSearchCalled => HasSearched;

			#region SelectedGridElements

			BusinessObject[] fSelectedGridElements;

			protected override IReadOnlyList<BusinessObject> SelectedGridElements
			{
				get { return fSelectedGridElements; }
			}

			public void SetSelectedGridElements(BusinessObject[] selectedGridElements)
			{
				this.fSelectedGridElements = selectedGridElements;
			}

			#endregion
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ClientModuleRegistration.BatchPrinting;
		}

		protected override bool HasController()
		{
			return false;
		}

		protected override bool ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_View => true;

		TestUPEPrintBatchModule Module
		{
			get
			{
				if (fModule == null)
				{
					fModule = new TestUPEPrintBatchModule();
				}
				return fModule;
			}
		}
		TestUPEPrintBatchModule fModule;

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;

			base.SetUp();
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
		}

		protected override void TearDown()
		{
			Module.Dispose();
			base.TearDown();
		}

		#endregion
	}
}


