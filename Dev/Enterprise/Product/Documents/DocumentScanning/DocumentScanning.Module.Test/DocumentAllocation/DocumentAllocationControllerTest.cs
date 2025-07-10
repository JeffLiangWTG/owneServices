using CargoWise.EntityFramework;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Module.Testing
{
	[TestedType(typeof(DocumentAllocationController))]
	internal sealed class DocumentAllocationControllerTest : ZSingletonControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new AllocateDocumentsManager(new DocumentFactoryProvider().GetFactory(Factory));
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.DocumentAllocation;
		}

		public void TestGetDisplayModeForNew()
		{
			Env.Security.AllocateDocumentsModify.IsAllowed = true;
			AssertEquals("Modify allowed - browse display mode", ODisplayMode.Browse, DocController.GetDisplayModeForNewExposed());

			Env.Security.AllocateDocumentsModify.IsAllowed = false;
			AssertEquals("Modify not allowed - Readonly display mode", ODisplayMode.ReadOnly, DocController.GetDisplayModeForNewExposed());
		}

		public void TestFactory()
		{
			Assert(DocController.Factory is DocumentFactory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DocController = new DocumentAllocationControllerExposed();
		}

		DocumentAllocationControllerExposed DocController;
	}
}
