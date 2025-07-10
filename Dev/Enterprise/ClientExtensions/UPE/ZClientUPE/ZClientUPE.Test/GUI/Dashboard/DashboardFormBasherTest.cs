using System;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	[TestedType(typeof(DashboardForm))]
	class DashboardFormBasherTest : ZFormBasherTest
	{
		public void TestDashboardRefreshTimeByRegistry()
		{
			using (UPEDataRegistry.Instance.DashboardRefreshCycleTimeInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 29))
			{
				using (var form = new DashboardFormForTest(new Dashboard(Factory)))
				{
					form.Show();
					AssertEquals(29 * 60000, form.RefreshTimer.Interval);
				}
			}
		}

#if !WINZOR
		public void TestRefresh_TriesToKeepSessionAlive()
		{
			var keepAliveTracker = new MockKeepSessionAlive();
			using (ObjectFactory.Substitute("IKeepSessionAlive", keepAliveTracker))
			using (DashboardFormForTest form = new DashboardFormForTest(new Dashboard(Factory)))
			{
				form.Show();
				AssertEquals(0, keepAliveTracker.HitTracker);
				((TimerForTest)form.RefreshTimer).OnTick();
				AssertEquals(1, keepAliveTracker.HitTracker);
			}
		}

		class MockKeepSessionAlive : IKeepSessionAlive
		{
			public void KeepAlive() => HitTracker++;
			public int HitTracker { get; private set; }
		}
#endif

		class DashboardFormForTest : DashboardForm
		{
			public DashboardFormForTest(Dashboard businessObject) : base(businessObject)
			{
			}

			public new Timer RefreshTimer
			{
				get
				{
					return base.RefreshTimer;
				}
			}

			protected override Timer NewTimer()
			{
				return new TimerForTest();
			}
		}

		class TimerForTest : Timer
		{
			public void OnTick()
			{
				base.OnTick(EventArgs.Empty);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new DashboardForm(new Dashboard(Factory));
		}

		public void TestFormCaption()
		{
			using (DashboardForm form = new DashboardForm(Dashboard))
			{
				AssertEquals("Dashboard", form.FormCaption);
			}
		}

		public override void TestMinimumSizeNotTooBig()
		{
			Assert(true); //custom form for UPS
		}

		#region Setup
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			Dashboard = new Dashboard(Factory);
		}

		Dashboard Dashboard;
		#endregion
	}
}
