using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.Module.Testing
{
	internal class UPEAUCustomsAirCargoConsolControllerTest : TestCaseWithFactory
	{
		public void TestID()
		{
			AssertEquals(ClientControllerRegistration.AirCargoConsol, Controller.ID);
		}

		public void TestModuleID()
		{
			AssertEquals(ClientModuleRegistration.AirCargo, Controller.ModuleID);
		}

		public void TestGetForm()
		{
			using (IZForm form = Controller.GetForm(Factory.New(typeof(UPECusMAWB))))
			{
				AssertEquals(typeof(UPEAirCargoMasterForm), form.GetType());
			}
		}

		public void TestSecurityCheckpoints()
		{
			AssertEquals(Env.Security.ACAMasterImportDelete, Controller.CheckPointForDelete);
			AssertEquals(Env.Security.ACAMasterImportModify, Controller.CheckPointForNew);
			AssertEquals(Env.Security.ACAMasterImportModify, Controller.CheckPointForEdit);
			AssertEquals(Env.Security.ACAMasterImportView, Controller.CheckPointForView);
		}

		public void TestTwoUsersCannotEditTheSameFormAtTheSameTime()
		{
			UPECusMAWB airCargoConsol = Factory.NewWithValidTestData<UPECusMAWB>(TestBusinessObjectKind.MinimumRequiredToSave);
			airCargoConsol.CM_MAWB = "AML00001000";
			Factory.Save();
			IZForm form1 = Controller.ShowEditForm(airCargoConsol);
			IZForm form2 = new UPEAUCustomsAirCargoConsolControllerForTest().ShowEditForm(airCargoConsol);
			try
			{
				AssertEquals(typeof(UPEAirCargoMasterForm), form1.GetType());
				AssertEquals(form1, form2);
			}
			finally
			{
				CloseAndDisposeForm(form1);
			}

			try
			{
				form2 = Controller.ShowEditForm(airCargoConsol);
				AssertEquals(typeof(UPEAirCargoMasterForm), form2.GetType());
			}
			finally
			{
				CloseAndDisposeForm(form2);
			}
		}

		public void TestOneUserCanViewFormWhileOtherUserEditsTheSameForm()
		{
			UPECusMAWB airCargoConsol = Factory.NewWithValidTestData<UPECusMAWB>(TestBusinessObjectKind.MinimumRequiredToSave);
			Factory.Save();
			IZForm form1 = Controller.ShowEditForm(airCargoConsol);
			IZForm form2 = Controller.ShowViewForm(airCargoConsol);
			try
			{
				AssertEquals(typeof(UPEAirCargoMasterForm), form1.GetType());
				AssertEquals(typeof(UPEAirCargoMasterForm), form2.GetType());
			}
			finally
			{
				CloseAndDisposeForm(form1);
				CloseAndDisposeForm(form2);
			}

			form1 = Controller.ShowViewForm(airCargoConsol);
			form2 = Controller.ShowEditForm(airCargoConsol);
			try
			{
				AssertEquals(typeof(UPEAirCargoMasterForm), form1.GetType());
				AssertEquals(typeof(UPEAirCargoMasterForm), form2.GetType());
			}
			finally
			{
				CloseAndDisposeForm(form1);
				CloseAndDisposeForm(form2);
			}
		}

		#region Implementation
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		void CloseAndDisposeForm(IZForm form)
		{
			if (form != null)
			{
				((ZForm)form).Close();
				form.Dispose();
			}
		}

		UPEAUCustomsAirCargoConsolControllerForTest Controller
		{
			get
			{
				if (fController == null)
				{
					fController = new UPEAUCustomsAirCargoConsolControllerForTest();
				}

				return fController;
			}
		}

		UPEAUCustomsAirCargoConsolControllerForTest fController;
		class UPEAUCustomsAirCargoConsolControllerForTest : UPEAirCargoConsolController
		{
			public new IZForm GetForm(IBusiness businessEntity)
			{
				return base.GetForm(businessEntity);
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

			public new SecurityCheckpoint CheckPointForEdit
			{
				get
				{
					return base.CheckPointForEdit;
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
	}
}
