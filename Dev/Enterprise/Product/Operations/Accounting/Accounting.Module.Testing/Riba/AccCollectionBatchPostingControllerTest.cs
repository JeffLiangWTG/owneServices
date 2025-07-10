using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Riba;
using Enterprise.Accounting.GUI.Riba;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AccCollectionBatchPostingController))]
	public class AccCollectionBatchPostingControllerTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AccCollectionBatchPosting;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new AccCollectionBatchPoster(Factory);
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(AccCollectionBatchPoster);
		}

		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.AccCollectionBatch, TestController.ModuleID);
		}

		public void TestGetForm()
		{
			using (IZForm testForm = TestController.GetForm_ForTestOnly(new AccCollectionBatchPoster(Factory)))
			{
				AssertEquals(typeof(AccCollectionBatchForm), testForm.GetType());
			}
		}

		public void TestCheckPointForNew()
		{
			AssertEquals(Env.Security.CreateCollectionOrderBatch, TestController.CheckPointForNew_ForTestOnly);
		}

		AccCollectionBatchPostingController fTestController;
		AccCollectionBatchPostingController TestController
		{
			get
			{
				if (fTestController == null)
				{
					fTestController = new AccCollectionBatchPostingController();
				}
				return fTestController;
			}
		}
	}
}
