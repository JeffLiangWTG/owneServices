using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.Module.Testing
{
	sealed class UPEPrintBatchFilterControlTest : TestCaseWithClientSpecificDocuments
	{
		public void TestPrintSelectedBatchOnDoubleClick()
		{
			using (TestUPEPrintBatchModule module = new TestUPEPrintBatchModule())
			using (UPEPrintBatchFilterControl filterControl = (UPEPrintBatchFilterControl)module.GetNewFilterControl())
			using (ZForm form = new ZForm())
			{
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();

				TestUPEPrintBatch printBatch = CreateNewPrintBatch(Factory);
				module.SetSelectedGridElements(new BusinessObject[] { printBatch });

				MethodInfo onDoubleClickMethod = typeof(Control).GetMethod("OnDoubleClick", BindingFlags.NonPublic | BindingFlags.Instance);
				onDoubleClickMethod.Invoke(filterControl.FilteredGrid, new object[] { EventArgs.Empty });
				AssertNotNull("PrintBatch should be called when the grid is double-clicked", printBatch.PrintCalled);
			}
		}

		#region Implementation

		TestUPEPrintBatch CreateNewPrintBatch(BusinessObjectFactory factory)
		{
			TestUPEPrintBatch printBatch = (TestUPEPrintBatch)new TestUPEPrintBatch.Loader(factory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.TaxInvoice);
			Callout callout = factory.NewWithValidTestData<Callout>();
			printBatch.PrintItems.AddNew(new UPEDocumentMenuItemLoader(factory).LoadUPSTaxInvoice(), callout);
			return printBatch;
		}

		class TestUPEPrintBatchModule : UPEPrintBatchModule
		{
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

			public new IFilterControl GetNewFilterControl()
			{
				return base.GetNewFilterControl();
			}
		}

		#endregion
	}
}


