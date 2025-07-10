using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.Module.Testing
{
	internal class EnquiryControllerTest : TestCaseWithFactory
	{
		public void TestModuleID()
		{
			AssertEquals(ClientModuleRegistration.Enquiry, Controller.ModuleID);
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(Enquiry), Controller.TypeOfTopLevelBusinessObject);
		}

		public void TestControllerID()
		{
			AssertEquals(ClientControllerRegistration.Enquiry, Controller.ID);
		}

		public void TestGetNewForm()
		{
			Enquiry enquiry = Factory.New<Enquiry>();
			using (EnquiryForm form = Controller.GetForm(enquiry) as EnquiryForm)
			{
				AssertNotNull("Form should be of type EnquiryForm", form);
			}
		}

		public void TestSecurityCheckpoints()
		{
			AssertEquals(Env.Security.None, Controller.CheckPointForDelete);
			AssertEquals(Env.Security.None, Controller.CheckPointForEdit);
			AssertEquals(Env.Security.None, Controller.CheckPointForNew);
			AssertEquals(Env.Security.None, Controller.CheckPointForView);
		}

		EnquiryControllerForTest Controller
		{
			get
			{
				if (fController == null)
				{
					fController = new EnquiryControllerForTest();
				}

				return fController;
			}
		}

		EnquiryControllerForTest fController;
		#region EnquiryControllerForTest
		class EnquiryControllerForTest : EnquiryController
		{
			public new IZForm GetForm(IBusiness businessEntity)
			{
				return base.GetForm(businessEntity);
			}

			public new SecurityCheckpoint CheckPointForEdit
			{
				get
				{
					return base.CheckPointForEdit;
				}
			}

			public new SecurityCheckpoint CheckPointForDelete
			{
				get
				{
					return base.CheckPointForDelete;
				}
			}

			public new SecurityCheckpoint CheckPointForNew
			{
				get
				{
					return base.CheckPointForNew;
				}
			}

			public new SecurityCheckpoint CheckPointForView
			{
				get
				{
					return base.CheckPointForView;
				}
			}
		}

		#endregion
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}
	}
}
