using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.EDI.MarketingManager.GUI.Testing
{
	public class EDISalesClientSummaryUserControlTest : TestCaseWithFactory
	{
		#region TestSalesClientSummaryControl
		public class TestEDISalesClientSummaryUserControl : EDISalesClientSummaryUserControl
		{
			public ZCalcEdit cmAcheivableClientRevenueBoundCalcEdit
			{
				get
				{
					return OM_CMAcheivableClientRevenueBoundCalcEdit;
				}
			}

			public ZCalcEdit cmNoOfEmployeesCalcEdit
			{
				get
				{
					return OM_CMNoOfEmployeesCalcEdit;
				}
			}

			public ZCalcEdit cmAmountOfBusinessWonBoundCalcEdit
			{
				get
				{
					return OM_CMAmountOfBusinessWonBoundCalcEdit;
				}
			}

			public ZCalcEdit cmTotalClientRevenueBoundCalcEdit
			{
				get
				{
					return OM_CMTotalClientRevenueBoundCalcEdit;
				}
			}

			public ZCalcEdit cmWarehouseRevenueBoundCalcEdit
			{
				get
				{
					return OM_CMWarehouseRevenueBoundCalcEdit;
				}
			}

			public ZCalcEdit cmConsultingRevenueCalcEdit
			{
				get
				{
					return OM_CMConsultingRevenueCalcEdit;
				}
			}

			public ZCalcEdit cmPaidUpCapitalCalcEdit
			{
				get
				{
					return OM_CMPaidUpCapitalCalcEdit;
				}
			}
		}

		#endregion
		public void TestCustomisedLabels()
		{
			using (EDIDataRegistry.Instance.AchievableBusinessLabel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test Achievable Business Caption"))
			using (EDIDataRegistry.Instance.NumberOfEmployeesLabel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test Number Of Employees Caption"))
			using (EDIDataRegistry.Instance.AmountOfBusinessWonLabel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test Amount Of Business Won Caption"))
			using (EDIDataRegistry.Instance.TotalClientRevenueLabel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test Total Client Revenue Caption"))
			using (EDIDataRegistry.Instance.WarehouseRevenueLabel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test Warehouse Revenue Caption"))
			using (EDIDataRegistry.Instance.ConsultingRevenueLabel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test Consulting Revenue Caption"))
			using (EDIDataRegistry.Instance.PaidUpCapitalLabel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test Paid Up Capital Caption"))
			using (TestEDISalesClientSummaryUserControl control = new TestEDISalesClientSummaryUserControl())
			{
				control.Show();
				AssertEquals("Test Achievable Business Caption", control.cmAcheivableClientRevenueBoundCalcEdit.CaptionResourceString.Caption);
				AssertEquals("Test Number Of Employees Caption", control.cmNoOfEmployeesCalcEdit.CaptionResourceString.Caption);
				AssertEquals("Test Amount Of Business Won Caption", control.cmAmountOfBusinessWonBoundCalcEdit.CaptionResourceString.Caption);
				AssertEquals("Test Total Client Revenue Caption", control.cmTotalClientRevenueBoundCalcEdit.CaptionResourceString.Caption);
				AssertEquals("Test Warehouse Revenue Caption", control.cmWarehouseRevenueBoundCalcEdit.CaptionResourceString.Caption);
				AssertEquals("Test Consulting Revenue Caption", control.cmConsultingRevenueCalcEdit.CaptionResourceString.Caption);
				AssertEquals("Test Paid Up Capital Caption", control.cmPaidUpCapitalCalcEdit.CaptionResourceString.Caption);
			}
		}
	}
}
