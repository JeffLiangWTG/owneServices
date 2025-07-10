using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CO.Manifest.Module.Testing
{
	[TestedType(typeof(DocumentIDsController))]
	class DocumentIDsControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CO.DocumentIDs;

		protected override string CountryCode => Core.Constants.CountryCodes.Colombia;

		public void TestCheckPointForView()
		{
			var header = Factory.NewWithValidTestData<CusTransactionNumber>();
			var controller = new DocumentIDsController();
			AssertEquals(Env.Security.None, controller.GetCheckPointForView(header));
		}

		public void TestCheckPointForEdit()
		{
			var header = Factory.NewWithValidTestData<CusTransactionNumber>();
			var controller = new DocumentIDsController();
			AssertEquals(Env.Security.None, controller.GetCheckPointForEdit(header));
		}

		public void TestCheckPointForNew()
		{
			var header = Factory.NewWithValidTestData<CusTransactionNumber>();
			var controller = new DocumentIDsController();
			AssertEquals(Env.Security.None, controller.GetCheckPointForNew(header));
		}

		public void TestCheckPointForDelete()
		{
			var header = Factory.NewWithValidTestData<CusTransactionNumber>();
			var controller = new DocumentIDsController();
			AssertEquals(Env.Security.None, controller.GetCheckPointForDelete(header));
		}
	}
}
