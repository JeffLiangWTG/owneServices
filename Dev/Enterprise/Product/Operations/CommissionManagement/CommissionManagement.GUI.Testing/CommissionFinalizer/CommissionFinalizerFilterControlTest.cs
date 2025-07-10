using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.CommissionManagement.GUI.Testing
{
	public class CommissionFinalizerFilterControlTest : TestCaseWithFactory
	{
		#region Layout

		public void TestButtons()
		{
			var finalizer = new CommissionFinalizer();
			var filterBizObj = new CommissionFinalizerFilterBusinessObject();

			using (var form = new ZForm())
			using (var control = new CommissionFinalizerFilterControlForTest(finalizer, filterBizObj))
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(false, control.AddStripButton_Exposed.Visible);
				AssertEquals(false, control.ToolStripAddGroupButton_Exposed.Visible);
			}
		}

		#endregion

		#region ItemGrids

		public void TestItemGrids()
		{
			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var finalizer = new CommissionFinalizer();
			using (var form = new ZForm())
			using (var control = new CommissionFinalizerFilterControlForTest(finalizer, new CommissionFinalizerFilterBusinessObject()))
			{
				form.Controls.Add(control);
				form.Show();

				AssertType(typeof(CommissionFinalizerItemGridsControl), control.ControlForLayout_Exposed);
			}

			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (var form = new ZForm())
			using (var control = new CommissionFinalizerFilterControlForTest(finalizer, new CommissionFinalizerFilterBusinessObject()))
			{
				form.Controls.Add(control);
				form.Show();

				AssertType(typeof(GlobalCommissionFinalizerItemGridsControl), control.ControlForLayout_Exposed);
			}
		}

		#endregion

		#region MaximumAllowableQueriesPerSqlStatement

		public void TestMaximumAllowableQueriesPerSqlStatement()
		{
			OrganisationsDataRegistry.Instance.CommissionFinalizerMaxNumberOfRecordsToShowInDisplayGrids.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 500);

			using (var form = new ZForm())
			using (var control = new CommissionFinalizerFilterControlForTest(new CommissionFinalizer(), new CommissionFinalizerFilterBusinessObject()))
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(OrganisationsDataRegistry.Instance.CommissionFinalizerMaxNumberOfRecordsToShowInDisplayGrids.Value, control.MaximumAllowableQueriesPerSqlStatementExposed);
			}
		}

		#endregion

	}

	class CommissionFinalizerFilterControlForTest : CommissionFinalizerFilterControl
	{
		public CommissionFinalizerFilterControlForTest(CommissionFinalizer finalizer, CommissionFinalizerFilterBusinessObject filterBizObj)
			: base(finalizer, filterBizObj)
		{
		}

		public ZFilterStripAddButton AddStripButton_Exposed
		{
			get { return AddStripButton; }
		}

		public ZToolStripButton ToolStripAddGroupButton_Exposed
		{
			get { return ToolStripAddGroupButton; }
		}

		public ZToolStripSplitButton ToolStripManageDropButton_Exposed
		{
			get { return ToolStripManageDropButton; }
		}

		public ZToolStripButton ToolStripSaveLayoutButton_Exposed
		{
			get { return ToolStripSaveLayoutButton; }
		}

		public ZToolStripSplitButton ToolStripFindDropButton_Exposed
		{
			get { return ToolStripFindDropButton; }
		}

		public Control ControlForLayout_Exposed
		{
			get { return ControlForLayout; }
		}

		internal int MaximumAllowableQueriesPerSqlStatementExposed => MaximumAllowableQueriesPerSqlStatement;
	}
}
