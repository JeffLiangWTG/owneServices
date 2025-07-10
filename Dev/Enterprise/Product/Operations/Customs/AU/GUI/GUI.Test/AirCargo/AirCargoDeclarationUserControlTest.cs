using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	sealed class AirCargoDeclarationUserControlTest : TestCaseWithFactory
	{
		public void TestUnderbondPlugin()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			using (TestForm form = new TestForm(hAWB))
			{
				form.Show();
				form.TestControl.SetupPlugins();
				AssertEquals("There should be one plugin", 1, ((CMRAirCargoHouseUserControl)form.TestControl.ShipmentUserControl).MasterTabControl.PlugIns.Instances.Length);
			}
		}

		public void TestInCMR()
		{
			using (AirCargoDeclarationUserControl control = new AirCargoDeclarationUserControl())
			{
				AssertEquals("ShipmentUserControl should be CMR", typeof(CMRAirCargoHouseUserControl), control.ShipmentUserControl.GetType());
			}
		}

		sealed class TestForm : ZForm
		{
			public TestForm(CusHAWB hAWB) : base(hAWB)
			{
				InitialiseComponents();
			}

			void InitialiseComponents()
			{
				TestControl = new AirCargoDeclarationUserControl();
				TestControl.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
				TestControl.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.CusMAWB";
				Controls.Add(TestControl);
				DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
				DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.CusHAWB";
				TabPageNotificationsExposer.ExposeTabPageNotifications(this, BusinessEntity);
			}

			public AirCargoDeclarationUserControl TestControl;
		}
	}
}
