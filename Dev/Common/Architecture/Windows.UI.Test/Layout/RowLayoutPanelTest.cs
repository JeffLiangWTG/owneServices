using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Layout.Testing
{
	sealed class RowLayoutPanelTest : TestCase
	{
		public void TestExtenderProviderImpl()
		{
			AssertEquals("Implements IExtenderProvider", true, Panel is IExtenderProvider);
			AssertEquals("Extends child controls", true, ((IExtenderProvider)Panel).CanExtend(ChildControl));
			AssertEquals("Doesn't extend child nested controls", false, ((IExtenderProvider)Panel).CanExtend(ChildChildControl));

			ProvidePropertyAttribute[] attrs = (ProvidePropertyAttribute[])typeof(RowLayoutPanel).GetCustomAttributes(typeof(ProvidePropertyAttribute), false);
			AssertEquals("Provides 1 property to its child controls", 1, attrs.Length);

			AssertEquals("PropertyName", "Row", attrs[0].PropertyName);
			AssertEquals("ReceiverType", typeof(Control).AssemblyQualifiedName, attrs[0].ReceiverTypeName);
		}

		public void TestFixedRows()
		{
			AssertEquals("default", false, Panel.FixedRows);

			RowLayout rowLayout = (RowLayout)Panel.LayoutEngine;

			Panel.FixedRows = true;
			AssertEquals("layout.FixedRows", true, rowLayout.FixedRows);

			Panel.FixedRows = false;
			AssertEquals("layout.FixedRows", false, rowLayout.FixedRows);
		}

		public void TestRowHeightDpiState()
		{
			var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.GetProperty;
			var rowHeightPropertyInfo = typeof(RowLayoutPanel).GetProperty("RowHeight", bindingFlags);
			var attrs = rowHeightPropertyInfo.GetCustomAttributes<DpiStateAttribute>().ToList();
			AssertEquals("Should add DpiStateAttribute on RowHeight property", 1, attrs.Count);
			AssertEquals(DpiState.ScaleY, attrs[0].State);
		}

		#region Implementation

		TextBox ChildControl
		{
			get
			{
				if (childControl == null)
				{
					childControl = new TextBox();
					Panel.Controls.Add(childControl);
				}
				return childControl;
			}
		}
		TextBox childControl;

		TextBox ChildChildControl
		{
			get
			{
				if (childChildControl == null)
				{
					childChildControl = new TextBox();
					childChildControl.Controls.Add(childControl);
				}
				return childChildControl;
			}
		}
		TextBox childChildControl;

		RowLayoutPanel Panel
		{
			get { return panel ?? (panel = new RowLayoutPanel()); }
		}
		RowLayoutPanel panel;

		protected override void TearDown()
		{
			base.TearDown();
			if (panel != null)
			{
				panel.Dispose();
			}
			if (childControl != null)
			{
				childControl.Dispose();
			}
			if (childChildControl != null)
			{
				childChildControl.Dispose();
			}
		}

		#endregion
	}
}
