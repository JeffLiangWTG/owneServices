using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.AccountingVoucherPrint;
using Enterprise.Accounting.GUI.AccountingVoucherPrinting;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ChinaJournalListingController))]
	public class ChinaJournalListingControllerTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ChinaJournalListing;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new ChinaJournalListingPrintWrapper();
		}

		public void TestGetForm()
		{
			using (IZForm testForm = TestController.GetForm_ForTestOnly(new ChinaJournalListingPrintWrapper(Factory)))
			{
				AssertEquals(typeof(ChinaJournalListingPrintForm), testForm.GetType());
			}
		}

		public void TestControllerID()
		{
			AssertEquals(ControllerIDs.ChinaJournalListing, TestController.ID);
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(ChinaJournalListingPrintWrapper), TestController.TypeOfTopLevelBusinessObject);
		}

		public void TestGetNewBusinessEntityInLocalFactory()
		{
			AssertEquals(typeof(ChinaJournalListingPrintWrapper), TestController.GetNewBusinessEntityInLocalFactory_ForTestOnly().GetType());
		}

		public void TestCheckPointForNew()
		{
			AssertEquals(Env.Security.ChinaJournalListing, TestController.CheckPointForNew_ForTestOnly);
		}

		ChinaJournalListingController fTestController;

		ChinaJournalListingController TestController
		{
			get
			{
				if (fTestController == null)
				{
					fTestController = new ChinaJournalListingController();
				}
				return fTestController;
			}
		}
	}
}
