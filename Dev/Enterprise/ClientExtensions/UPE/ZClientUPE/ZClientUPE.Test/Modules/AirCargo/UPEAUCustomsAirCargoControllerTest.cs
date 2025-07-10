using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.Module.Testing
{
	internal class UPEAUCustomsAirCargoControllerTest : TestCaseWithFactory
	{
		public void TestID()
		{
			AssertEquals(ClientControllerRegistration.AirCargo, Controller.ID);
		}

		public void TestModuleID()
		{
			AssertEquals(ClientModuleRegistration.HouseAirCargo, Controller.ModuleID);
		}

		public void TestGetForm()
		{
			using (IZForm form = Controller.GetForm(Factory.New(typeof(UPECusHAWB))))
			{
				AssertEquals(typeof(UPEAirCargoHouseForm), form.GetType());
			}
		}

		public void TestSecurityCheckpoints()
		{
			AssertEquals(Env.Security.ACAHouseModify, Controller.CheckPointForDelete);
			AssertEquals(Env.Security.ACAHouseModify, Controller.CheckPointForEdit);
			AssertEquals(Env.Security.ACAHouseModify, Controller.CheckPointForNew);
			AssertEquals(Env.Security.ACAHouse, Controller.CheckPointForView);
		}

		public void TestTwoUsersCannotEditTheSameFormAtTheSameTime()
		{
			UPECusHAWB airCargo = Factory.NewWithValidTestData<UPECusHAWB>(TestBusinessObjectKind.MinimumRequiredToSave);
			airCargo.CS_HAWB = "HBL00001000";
			Factory.Save();
			IZForm form1 = Controller.ShowEditForm(airCargo);
			IZForm form2 = new UPEAUCustomsAirCargoControllerForTest().ShowEditForm(airCargo);
			try
			{
				AssertEquals(typeof(UPEAirCargoHouseForm), form1.GetType());
				AssertNull("Should Not Create second form", form2);
				ZString lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				Assert(lastMessage.StartsWith("Access to Air Cargo House : 'HBL00001000' is denied.\nThe record has been locked since '"));
				Assert(lastMessage.EndsWith("' by '" + Enterprise.MasterFiles.Business.GlbStaff.CurrentUser.GS_FullName + "'.\nPlease wait until the lock has been released before trying to edit the record.\n"));
			}
			finally
			{
				CloseAndDisposeForm(form1);
			}

			try
			{
				form2 = Controller.ShowEditForm(airCargo);
				AssertEquals(typeof(UPEAirCargoHouseForm), form2.GetType());
			}
			finally
			{
				CloseAndDisposeForm(form2);
			}
		}

		public void TestOneUserCanViewFormWhileOtherUserEditsTheSameForm()
		{
			UPECusHAWB airCargo = Factory.NewWithValidTestData<UPECusHAWB>(TestBusinessObjectKind.MinimumRequiredToSave);
			Factory.Save();
			IZForm form1 = Controller.ShowEditForm(airCargo);
			IZForm form2 = Controller.ShowViewForm(airCargo);
			try
			{
				AssertEquals(typeof(UPEAirCargoHouseForm), form1.GetType());
				AssertEquals(typeof(UPEAirCargoHouseForm), form2.GetType());
			}
			finally
			{
				CloseAndDisposeForm(form1);
				CloseAndDisposeForm(form2);
			}
		}

		public void TestOneUserCanEditFormWhileOtherUserViewsTheSameForm()
		{
			UPECusHAWB airCargo = Factory.NewWithValidTestData<UPECusHAWB>(TestBusinessObjectKind.MinimumRequiredToSave);
			Factory.Save();
			IZForm form1 = Controller.ShowViewForm(airCargo);
			IZForm form2 = Controller.ShowEditForm(airCargo);
			try
			{
				AssertEquals(typeof(UPEAirCargoHouseForm), form1.GetType());
				AssertEquals(typeof(UPEAirCargoHouseForm), form2.GetType());
			}
			finally
			{
				CloseAndDisposeForm(form1);
				CloseAndDisposeForm(form2);
			}
		}

		public void TestEditForm_IfRecordCannotBeDisplayed()
		{
			UPECusHAWB airCargo = Factory.NewWithValidTestData<UPECusHAWB>(TestBusinessObjectKind.MinimumRequiredToSave);
			AssertNoExceptionThrown("No NullReferenceException should be thrown", () => Controller.ShowEditForm(airCargo));
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

		UPEAUCustomsAirCargoControllerForTest Controller
		{
			get
			{
				if (fController == null)
				{
					fController = new UPEAUCustomsAirCargoControllerForTest();
				}

				return fController;
			}
		}

		UPEAUCustomsAirCargoControllerForTest fController;
		class UPEAUCustomsAirCargoControllerForTest : UPEAirCargoController
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
