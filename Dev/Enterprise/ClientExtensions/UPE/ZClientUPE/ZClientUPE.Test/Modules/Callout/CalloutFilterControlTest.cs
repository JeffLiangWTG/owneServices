using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	public class CalloutFilterControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLoad()
		{
			using (TestCalloutModule module = new TestCalloutModule())
			using (CalloutFilterControl filterControl = (CalloutFilterControl)module.GetNewFilterControl())
			using (ZForm form = new ZForm())
			{
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();
			}
		}

		public void TestIsCODCustomerColumnAdded()
		{
			using (TestCalloutModule module = new TestCalloutModule())
			using (CalloutFilterControl filterControl = (CalloutFilterControl)module.GetNewFilterControl())
			using (ZForm form = new ZForm())
			{
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();
				ZGridColumn column = filterControl.FilteredGrid.Columns["Payment+IsCheque"];
				column.IsVisible = true;
				filterControl.FilteredGrid.RefreshTableStyles();
				AssertNotNull("Column should be visible and bound", column.ColumnStyle.PropertyDescriptor);
			}
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		class TestCalloutModule : CalloutModule
		{
			public new IFilterControl GetNewFilterControl()
			{
				return base.GetNewFilterControl();
			}
		}
	}
}
