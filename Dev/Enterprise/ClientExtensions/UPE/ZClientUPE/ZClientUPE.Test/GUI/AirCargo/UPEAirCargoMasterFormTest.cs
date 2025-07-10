using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.AirCargo.GUI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	[TestedType(typeof(UPEAirCargoMasterForm))]
	public class UPEAirCargoMasterFormTest : ZFormBasherTest
	{
		public void TestRequiresColumns()
		{
			CusMAWB cusMAWB = Factory.New<CusMAWB>();
			using (TestUPEAirCargoMasterForm form = new TestUPEAirCargoMasterForm(cusMAWB))
			using (BaseACAStandAloneUserControl userControl = form.NewACAStandAloneUserControl())
			{
				ZCheckBoxColumnStyleInfo column = (ZCheckBoxColumnStyleInfo)userControl.HouseBillsModuleButtonGrid.GetColumnStyle(UPECusHAWB.Schema.RequiresConsigneeMatch);
				AssertEquals("Column should not be visible initially", false, column.IsVisible);
				ZCheckBoxColumnStyleInfo column1 = (ZCheckBoxColumnStyleInfo)userControl.HouseBillsModuleButtonGrid.GetColumnStyle(UPECusHAWB.Schema.CS_IsSurplus);
				AssertNotNull(column1);
				AssertEquals("Column should be visible initially", true, column1.IsVisible);
			}
		}

		public void TestRequiresConsignorMatchColumn()
		{
			CusMAWB cusMAWB = Factory.New<CusMAWB>();
			using (TestUPEAirCargoMasterForm form = new TestUPEAirCargoMasterForm(cusMAWB))
			using (BaseACAStandAloneUserControl userControl = form.NewACAStandAloneUserControl())
			{
				ZCheckBoxColumnStyleInfo column = (ZCheckBoxColumnStyleInfo)userControl.HouseBillsModuleButtonGrid.GetColumnStyle(UPECusHAWB.Schema.RequiresConsignorMatch);
				AssertEquals("Column should not be visible initially", false, column.IsVisible);
			}
		}

		protected override Form GetFormToBashCore()
		{
			CusMAWB cusMAWB = Factory.New<CusMAWB>();
			UPEAirCargoMasterForm result = new UPEAirCargoMasterForm(cusMAWB);
			result.ControllerID = ControllerIDs.Customs.AU.AirCargo;
			return result;
		}

		class TestUPEAirCargoMasterForm : UPEAirCargoMasterForm
		{
			public TestUPEAirCargoMasterForm(CusMAWB businessEntity) : base(businessEntity)
			{
			}

			public new BaseACAStandAloneUserControl NewACAStandAloneUserControl()
			{
				return base.NewACAStandAloneUserControl();
			}
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}
	}
}
