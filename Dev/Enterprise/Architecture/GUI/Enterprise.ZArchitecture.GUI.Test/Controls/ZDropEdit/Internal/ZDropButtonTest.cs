using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Interop;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class ZDropButtonTest : TestCase
	{
		public void TestDragDrop()
		{
			Assert(!Button.AllowDrop);
		}

		public void TestShowDropDown()
		{
			AssertEquals(false, Button.IsDroppedDown);

			Button.ShowDropDown(false);
			AssertEquals(true, Button.IsDroppedDown);
			AssertEquals(false, Button.DropDownExposed.IsDisposed);

			Button.HideDropDown();
			AssertEquals(false, Button.IsDroppedDown);
			AssertEquals(false, Button.DropDownExposed.Visible);
			AssertEquals(false, Button.DropDownExposed.Enabled);
			AssertEquals(false, Button.DropDownExposed.IsDisposed);

			var dropDownStored = Button.DropDownExposed;

			Button.ShowDropDown(false);
			AssertEquals(true, Button.IsDroppedDown);
			AssertEquals(false, Button.DropDownExposed.IsDisposed);
			AssertEquals(true, dropDownStored.IsDisposed);
		}

		public void TestShowDropDownCalledTwice()
		{
			try
			{
				ErrorReporter.Clear();

				Button.ShowDropDown(false);
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);

				Button.ShowDropDown(false);
				AssertNotNullOrEmpty("Calling it twice without hiding it should report an error", ErrorReporter.LastMessageReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestOnMouseDownCommitsChangedValue()
		{
			var factory = new BusinessObjectFactory();
			var collection = new DummyBusinessObjectCollection(factory);
			var dummy = collection.AddNew();
			dummy.Z0_Code = "";

			using (var form = new DummyZForm(collection))
			{
				form.Show();
				Application.DoEvents();

				var columnStyle = (ZDropEditColumnStyle)form.Grid.Columns[0].ColumnStyle;
				var gridControl = columnStyle.GridControl;
				((ZGridDropEdit)gridControl).Text = "A";

				AssertEquals("Dummy.Z0_Code", "", dummy.Z0_Code);

				var location = form.DropEdit.DropButton.RectangleExposed.Location;
				var mouseEventArgs = new MouseEventArgs(MouseButtons.Left, 1, location.X, location.Y, 0);
				form.DropEdit.DropButton.OnMouseDownExposed(mouseEventArgs);

				AssertEquals("DropDown.Height", 16, form.DropEdit.DropButton.DropDown_Exposed.Height);
				AssertEquals("Dummy.Z0_Code", "A", dummy.Z0_Code);
			}
		}

		public void TestMissingParentSetsToReadOnly()
		{
			var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			var dummy = collection.AddNew();
			dummy.Z0_Code = "Meow";

			using (var form = new DummyZForm(collection))
			{
				form.Show();
				Application.DoEvents();

				var dropButton = form.DropEdit.DropButton;
				var location = dropButton.RectangleExposed.Location;
				var mouseEventArgs = new MouseEventArgs(MouseButtons.Left, 1, location.X, location.Y, 0);
				dropButton.Parent = null;
				dropButton.OnMouseDownExposed(mouseEventArgs);

				AssertEquals("Should be read only to avoid further mishaps", true, form.DropEdit.DropButton.isParentReadOnlyExposed);
			}
		}

		public void TestInvalidItemSelection()
		{
			var filterButton = new ZFilterStripDropButton();

			using (var filterDropEdit = new ZFilterStripDropEdit())
			{
				filterDropEdit.Controls.Add(filterButton);
				filterDropEdit.DisableInvalidation = true;
				filterDropEdit.List = new List<object>();
				filterDropEdit.List.Add(new CategoryCodeDescriptionPair("Dummy", ""));

				filterButton.ShowDropDown(false);
				filterButton.DropDownExposed.HighlightedItem_Exposed = 0;

				var e = new KeyEventArgs(Keys.Tab);
				filterButton.HandleCommandKey(e);

				AssertEquals("The select shouldn't be accepted", string.Empty, filterDropEdit.Text);

				filterButton.HideDropDown();
			}
		}

#if !WINZOR

		public void TestCatchZDropButtonAlreadyDisposedInWndProc()
		{
			using (var form = new ZForm())
			{
				var testDropButton = new DisposedZDropButtonTest { Name = "DropButton" };
				form.Controls.Add(testDropButton);
				form.Show();

				testDropButton.Visible = true;
				UnsafeNativeMethods.SendMessage(new HandleRef(testDropButton, testDropButton.Handle), WindowsMessage.WM_ERASEBKGND, 0, 0);

				AssertNull(ErrorReporter.LastExceptionReported);
				ErrorReporter.Clear();
			}
		}

		class DisposedZDropButtonTest : ZDropButton
		{
			protected override void WndProc(ref Message m)
			{
				if (m.Msg == WindowsMessage.WM_ERASEBKGND)
				{
					Dispose();
				}

				base.WndProc(ref m);
			}
		}

#endif

		#region DummyZForm Class

		class DummyZForm : ZForm
		{
			public DummyZForm(DummyBusinessObjectCollection businessEntity)
				: base(businessEntity)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				var info = new ZDropEditColumnStyleInfo();
				Grid = new ZGrid();
				DropEdit = new ZDropEdit();

				info.BindToList = "Codes";
				info.Caption = "Z0_Code";
				info.ColumnName = "Z0_Code";

				Grid.BindTo = ".";
				Grid.ColumnStyles.Add(info);
				Controls.Add(Grid);

				Controls.Add(DropEdit);
				DropEdit.BindTo = "Z0_Description";
				DropEdit.BindToList = "Collection";
			}

			public ZGrid Grid;
			public ZDropEdit DropEdit;
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Button = new ZDropButton();
			DropEdit = new ZDropEdit();
			DropEdit.Controls.Add(Button);
		}

		protected override void TearDown()
		{
			Button.HideDropDown();
			DropEdit.Dispose();
			base.TearDown();
		}

		ZDropButton Button;
		ZDropEdit DropEdit;

		#endregion
	}
}
