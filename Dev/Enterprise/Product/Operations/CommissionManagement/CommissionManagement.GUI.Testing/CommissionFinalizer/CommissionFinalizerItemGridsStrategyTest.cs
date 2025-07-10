using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.CommissionManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CommissionManagement.GUI.Testing
{
	public class CommissionFinalizerItemGridsStrategyTest : TestCaseWithFactory
	{
		public void TestAddDetailsGridAdornments()
		{
			var finalizer = new CommissionFinalizer();

			using (var form = new CommissionFinalizerFormForTest(finalizer))
			{
				form.Show();

				var grid = form.Grid;
				CommissionFinalizerItemGridsStrategy.AddDetailsGridAdornments(grid);

				AssertArrayEqualsByElements(new[] { "&View", "&View Approval Request" }, grid.ContextMenu.MenuItems.Cast<MenuItem>().Select(x => x.Text).Take(2).ToArray());

				var viewMenuItem = grid.ContextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == "&View");
				viewMenuItem.PerformClick();
				AssertEquals("Please select an entity commission to view.", UnitTestUserNotification.Instance.LastMessage.Text);

				var viewApprovalRequestMenuItem = grid.ContextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == "&View Approval Request");
				viewApprovalRequestMenuItem.PerformClick();
				AssertEquals("Please select an entity commission to view approval request.", UnitTestUserNotification.Instance.LastMessage.Text);

				var finalizerLineItem = new CommissionFinalizerLineItem(Factory.New<ViewCommissionLine>());
				var grouping = new CommissionFinalizerLineItemGrouping(Factory);
				grouping.Init(new[] { finalizerLineItem });
				finalizer.CommissionFinalizerLineItemGroupingCollection.Add(grouping);
				viewApprovalRequestMenuItem.PerformClick();
				AssertEquals("Selected entity commission does not have an approval request.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}

	class CommissionFinalizerFormForTest : ZForm
	{
		public CommissionFinalizerFormForTest(CommissionFinalizer commissionFinalizer)
			: base(commissionFinalizer)
		{
		}

		protected override void InitializeComponent()
		{
			base.InitializeComponent();

			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();

			this.Controls.Add(Grid);
			//
			// Grid
			//
			BindingSource.SetBindingMember(Grid, "CommissionFinalizerLineItemGroupingCollection");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommissionFinalizer)(null)).CommissionFinalizerLineItemGroupingCollection);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "StaffCode";
			zCodeFindBoxColumnStyleInfo1.Width = 40;
			this.Grid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);

			this.CaptionRenderingEnabled = true;
			this.Size = new Size(1024, 768);
		}

		public ZGrid Grid = new ZGrid();
	}
}
