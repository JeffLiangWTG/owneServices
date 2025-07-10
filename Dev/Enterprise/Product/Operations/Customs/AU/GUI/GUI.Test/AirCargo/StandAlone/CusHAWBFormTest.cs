using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	sealed class CusHAWBFormTest : TestCaseWithFactory
	{
		public void TestFormCaption()
		{
			using (CusHAWBForm form = new CusHAWBForm(HAWB))
			{
				AssertEquals("Air Cargo House", form.FormCaption);
			}
		}

		public void TestEDocsPlugin()
		{
			using (CusHAWBForm form = new CusHAWBForm(HAWB))
			{
				AssertNotNull("EDocs Plug-in should exist", form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
			}
		}

		public void TestSetupPostingButtonCalledInConstructor()
		{
			using (CusHAWBForm form = new CusHAWBForm(HAWB))
			{
				Assert("SetupPostingButton has to be called in the constructor", ((IPostingButtonsProvider)form).SetupPostingCalled);
			}
		}

		public void TestMinimumSize()
		{
			using (CusHAWBForm form = new CusHAWBForm(HAWB))
			{
				AssertEquals("Please check the form display before you modify this test", 712, form.MinimumSize.Height);
				AssertEquals("Please check the form display before you modify this test", 998, form.MinimumSize.Width);
			}
		}

		public void TestDataContext()
		{
			using (CusHAWBForm form = new CusHAWBForm(HAWB))
			{
				AssertEquals("DataContext should be correct", Core.Constants.DataContext.CusHAWB, form.DataContext);
			}
		}

		public void TestHouseUserControl()
		{
			HAWB.MAWB.ReadOnly = false;
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			using (CusHAWBForm form = new CusHAWBForm(HAWB))
			{
				AssertHouseUserControl(typeof(CMRAirCargoHouseUserControl), form);
			}
		}

		public void TestHouseUserControlGetsHAWBset()
		{
			CusHAWB hAWB1 = Factory.New<CusHAWB>();
			using (CusHAWBForm form = new CusHAWBForm(hAWB1))
			{
				AssertEquals(hAWB1, form.HouseUserControl.HAWB);
			}
		}

		CusHAWB hAWB;
		CusHAWB HAWB
		{
			get
			{
				if (hAWB == null)
				{
					var mawb = Factory.New<CusMAWB>();
					hAWB = mawb.ChildBills.AddNew();
				}

				return hAWB;
			}
		}

		void AssertHouseUserControl(Type expectedType, CusHAWBForm form)
		{
			form.Show();
			Application.DoEvents();
			var houseBill = (CusHAWB)form.DataSource;
			Assert("Master details should be read-only", houseBill.MAWB.CM_MAWBInfo.ReadOnly);
			Assert("Master details should be read-only", houseBill.MAWB.CM_FolioInfo.ReadOnly);
			Assert("Master details should be read-only", houseBill.MAWB.CM_DateOfFirstArrivalInfo.ReadOnly);
			AssertEquals(expectedType, form.HouseUserControl.GetType());
			AssertEquals(DockStyle.Fill, form.HouseUserControl.Dock);
		}
	}
}
