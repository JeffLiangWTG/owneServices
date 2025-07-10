using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(JobConsolCostingControllerForTest))]
	public class JobConsolCostingControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.JobConsolCostingForm;
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(DummyBusinessObjectWithNavigationProvider);
		}

		public override void TestNewForm()
		{
			Assert("New form is not supported.", true);
		}

		public override void TestDeleteForm()
		{
			Assert("Delete form is not supported.", true);
		}

		public override void TestViewForm()
		{
			Assert("View form is not supported.", true);
		}

		public void TestSecurityCheckPoint()
		{
			var controller = (JobConsolCostingController)ZControllerFactory.Create(GetControllerID());
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObjectWithNavigationProvider>();

			using ((JobConsolCostingForm)controller.ShowEditForm(dummyBizo))
			{
				CombineAssertions(() =>
				{
					AssertEquals("For View", Env.Security.SystemFeatureTest, controller.CheckPointForViewExposedForTest);
					AssertEquals("For New", Env.Security.SystemFeatureTest, controller.CheckPointForNewExposedForTest);
					AssertEquals("For Edit", Env.Security.SystemFeatureTest, controller.CheckPointForEditExposedForTest);
					AssertEquals("For Delete", Env.Security.SystemFeatureTest, controller.CheckPointForDeleteExposedForTest);

					AssertEquals("For CRM View", Env.Security.SystemFeatureTest, controller.GetCheckPointForView(dummyBizo));
					AssertEquals("For CRM Edit", Env.Security.SystemFeatureTest, controller.GetCheckPointForEdit(dummyBizo));
					AssertEquals("For CRM Delete", Env.Security.SystemFeatureTest, controller.GetCheckPointForDelete(dummyBizo));
				});
			}
		}
	}
}
