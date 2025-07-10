using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.GUI;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngine;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.Module.Testing
{
	internal class EntryPrintDocumentHelperTest : TestCaseWithFactory
	{
		public void TestViewEntryPrintDocument()
		{
			Callout callout = Factory.New<Callout>();
			callout.CS_JE_CustomsFormalEntry = Factory.NewWithValidTestData(typeof(BaseJobDeclaration)).PK;
			Helper.Run(callout);
			AssertNotNull("Should have shown the 'deliver documents' form", Helper.LastPrintTask);
		}

		public void TestViewEntryPrintDocument_WhenNoItemsSelected()
		{
			Helper.Run(null);
			AssertNull("Should NOT have shown the 'deliver documents' form", Helper.LastPrintTask);
			AssertEquals("You must select a finance item with a formal declaration in the grid", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestViewEntryPrintDocument_WhenNoDecAttached()
		{
			Callout callout = Factory.New<Callout>();
			callout.CS_JE_CustomsFormalEntry = ZGuid.Empty;
			Helper.Run(callout);
			AssertNull("Should NOT have shown the 'deliver documents' form", Helper.LastPrintTask);
			AssertEquals("The selected finance item doesn't have a declaration attached", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#region Implementation
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		readonly TestEntryPrintDocumentHelper Helper = new TestEntryPrintDocumentHelper();
		class TestEntryPrintDocumentHelper : EntryPrintDocumentHelper
		{
			public PrintTask LastPrintTask;
			protected override void RunPrintTask(PrintTask task)
			{
				LastPrintTask = task;
			}
		}
		#endregion
	}
}
