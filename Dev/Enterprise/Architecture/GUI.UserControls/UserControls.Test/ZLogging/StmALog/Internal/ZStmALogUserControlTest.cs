using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZStmALogUserControlTest : TestCaseWithDummy
	{
		public void TestLogsToShow_ChangeLogsOnly()
		{
			EventControl.LogsToShow = LogsToShow.ChangeLogs;
			Form.Controls.Add(EventControl);
			Form.Show();

			AssertNull("No estimates for ChangeLogs only", EventControl.LogsControl.FilterBusinessObject[ZStmALogFilterBusinessObject.Schema.ShowEstimates]);
			AssertNull("No cancelled for ChangeLogs only", EventControl.LogsControl.FilterBusinessObject[ZStmALogFilterBusinessObject.Schema.ShowCancelled]);
		}

		public void TestLogsToShow_All()
		{
			Form.Controls.Add(EventControl);
			Form.Show();

			foreach (LogsToShow operationsOnlyOrAll in new LogsToShow[] { LogsToShow.Operations, LogsToShow.All })
			{
				EventControl.LogsToShow = operationsOnlyOrAll;
				AssertNotNull("Estimates shown when " + operationsOnlyOrAll + " logs shown", EventControl.LogsControl.FilterBusinessObject[ZStmALogFilterBusinessObject.Schema.ShowEstimates]);
				AssertNotNull("Cancelled shown when " + operationsOnlyOrAll + " logs shown", EventControl.LogsControl.FilterBusinessObject[ZStmALogFilterBusinessObject.Schema.ShowCancelled]);
			}
		}

		#region Implementation

		ZForm Form
		{
			get
			{
				if (form == null)
				{
					form = new ZForm(new LogsView((IStmALogParent)Dummy, LogsToShow.All));
				}
				return form;
			}
		}
		ZForm form;

		ZStmALogUserControl EventControl
		{
			get
			{
				if (eventControl == null)
				{
					eventControl = new ZStmALogUserControl(new BusinessObjectFactory().NewWithValidTestData<DummyEnterpriseBusinessObject>());
				}
				return eventControl;
			}
		}
		ZStmALogUserControl eventControl;

		protected override Type TypeOfDummy
		{
			get { return typeof(DummyEnterpriseBusinessObject); }
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
			if (eventControl != null)
			{
				eventControl.Dispose();
			}
		}

		#endregion
	}
}
