using System;
using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZCheckedListBoxTest : ZControlBaseTestCase<ZCheckedListBox>
	{
		public void TestFixHeight()
		{
			using (var form = new ZForm(Bizo))
			{
				CheckedListBox.MultiColumn = true;
				CheckedListBox.ColumnWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
				CheckedListBox.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(19);
				CheckedListBox.Width = CheckedListBox.ColumnWidth * 4;
				CheckedListBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
				form.Controls.Add(CheckedListBox);
				DataBoundControl.Get(CheckedListBox).SetDataBinding(Bizo, "BoolDescriptionPairList");
				form.Show();
				AssertEquals("15", (int)(1 * Math.Ceiling((ControlDpiScalingHelper.DpiY / ControlDpiScalingHelper.BaseDpiY) * CheckedListBox.ItemHeight)), CheckedListBox.Height);
			}

			Bizo.BoolDescriptionPairList.AddNew("a", true);
			Bizo.BoolDescriptionPairList.AddNew("b", true);
			Bizo.BoolDescriptionPairList.AddNew("c", true);
			Bizo.BoolDescriptionPairList.AddNew("d", true);

			using (var form = new ZForm(Bizo))
			{
				checkedListBox = null;
				CheckedListBox.MultiColumn = true;
				CheckedListBox.ColumnWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
				CheckedListBox.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(19);
				CheckedListBox.Width = CheckedListBox.ColumnWidth * 4;
				CheckedListBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
				form.Controls.Add(CheckedListBox);
				DataBoundControl.Get(CheckedListBox).SetDataBinding(Bizo, "BoolDescriptionPairList");
				form.Show();
				AssertEquals("30", (int)(2 * Math.Ceiling((ControlDpiScalingHelper.DpiY / ControlDpiScalingHelper.BaseDpiY) * CheckedListBox.ItemHeight)), CheckedListBox.Height);
			}

			Bizo.BoolDescriptionPairList.AddNew("a", true);
			Bizo.BoolDescriptionPairList.AddNew("b", true);
			Bizo.BoolDescriptionPairList.AddNew("c", true);
			Bizo.BoolDescriptionPairList.AddNew("d", true);

			using (var form = new ZForm(Bizo))
			{
				checkedListBox = null;
				CheckedListBox.MultiColumn = true;
				CheckedListBox.ColumnWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
				CheckedListBox.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(19);
				CheckedListBox.Width = CheckedListBox.ColumnWidth * 4;
				CheckedListBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
				form.Controls.Add(CheckedListBox);
				DataBoundControl.Get(CheckedListBox).SetDataBinding(Bizo, "BoolDescriptionPairList");
				form.Show();
				AssertEquals("45", (int)(3 * Math.Ceiling((ControlDpiScalingHelper.DpiY / ControlDpiScalingHelper.BaseDpiY) * CheckedListBox.ItemHeight)), CheckedListBox.Height);
			}
		}

		public void TestChangingBizoEffectsForm()
		{
			using (var form = new ZForm(Bizo))
			{
				form.Controls.Add(CheckedListBox);
				DataBoundControl.Get(CheckedListBox).SetDataBinding(Bizo, "BoolDescriptionPairList");
				form.Show();
				AssertBizoChangesForm(CheckedListBox);
			}
		}

		public void TestChangingFormEffectsBizo()
		{
			using (var form = new ZForm(Bizo))
			{
				form.Controls.Add(CheckedListBox);
				DataBoundControl.Get(CheckedListBox).SetDataBinding(Bizo, "BoolDescriptionPairList");
				form.Show();
				AssertFormChangesBizo(CheckedListBox);
			}
		}

		public void TestCheckedListBoxShowToolTip()
		{
			using (var form = new ZForm(Bizo))
			{
				form.Controls.Add(CheckedListBox);
				DataBoundControl.Get(CheckedListBox).SetDataBinding(Bizo, "BoolDescriptionPairList");
				form.Show();
				CheckedListBox.OnMouseMoveExposed(new MouseEventArgs(MouseButtons.None, 0, 10, 10, 0), 0);
				Assert(ToolTipService.HasToolTip(CheckedListBox));
				AssertEquals("item1", ToolTipService.GetToolTip(CheckedListBox));

				CheckedListBox.OnMouseMoveExposed(new MouseEventArgs(MouseButtons.None, 0, 10, 20, 0), 1);
				AssertEquals("item2", ToolTipService.GetToolTip(CheckedListBox));
			}
		}

		[ExpectNoExceptions]
		public void TestBindingItemCheckedListBox_IndexOutofRange()
		{
			using (var form = new ZForm(Bizo))
			{
				form.Controls.Add(CheckedListBox);
				DataBoundControl.Get(CheckedListBox).SetDataBinding(Bizo, "BoolDescriptionPairList");
				form.Show();

				var e = new ItemCheckEventArgs(-1, CheckState.Checked, CheckState.Checked);

				CheckedListBox.CheckedListBox_ItemCheck(new object(), e);
			}
		}

		public void TestSingleSelected_OnlyOneOptionCanBeSelected()
		{
			using (var form = new ZForm(Bizo))
			{
				form.Controls.Add(CheckedListBox);
				CheckedListBox.SingleCheckMode = true;
				DataBoundControl.Get(CheckedListBox).SetDataBinding(Bizo, "BoolDescriptionPairList");
				form.Show();

				AssertEquals(false, CheckedListBox.GetItemChecked(0));
				AssertEquals(false, CheckedListBox.GetItemChecked(1));

				Bizo.BoolDescriptionPairList[0].Value = true;
				AssertEquals(true, CheckedListBox.GetItemChecked(0));
				AssertEquals(false, CheckedListBox.GetItemChecked(1));

				Bizo.BoolDescriptionPairList[1].Value = true;
				AssertEquals(false, CheckedListBox.GetItemChecked(0));
				AssertEquals(true, CheckedListBox.GetItemChecked(1));
			}
		}

#if !WINZOR

		public void TestBindingDoesntCauseDoubleItems()
		{
			using (var form = new ZForm())
			{
				const int LB_GETCOUNT = 0x18B;
				Bizo.BoolDescriptionPairList.Clear();
				Bizo.BoolDescriptionPairList.Add(new ZBoolDescriptionPair("Description", false));

				var bindingSource = new ZBindingSource();
				bindingSource.SetBindingMember(CheckedListBox, "BoolDescriptionPairList");
				bindingSource.SetDataBinding(Bizo, "");

				form.Controls.Add(CheckedListBox);
				form.Show();
				Application.DoEvents();

				var count = CargoWise.Interop.NativeMethods.SendMessage(CheckedListBox.Handle, LB_GETCOUNT, 0, 0);
				AssertEquals(1, count);
			}
		}

#endif

		protected override string[] BindablePropertyNames
		{
			get { return new string[] { "BindingItems" }; }
		}

		protected override string InvalidBindablePropertyName
		{
			get { return "Text"; }
		}

		#region Test Classes

		class DummyBusinessObjectWithBoolDescriptionPairList : DummyBusinessObject
		{
			public DummyBusinessObjectWithBoolDescriptionPairList(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			ZBoolDescriptionPairList boolDescriptionPairList;
			public ZBoolDescriptionPairList BoolDescriptionPairList
			{
				get
				{
					if (boolDescriptionPairList == null)
					{
						boolDescriptionPairList = new ZBoolDescriptionPairList();
						boolDescriptionPairList.Add(new ZBoolDescriptionPair("item1", false));
						boolDescriptionPairList.Add(new ZBoolDescriptionPair("item2", false));
					}
					return boolDescriptionPairList;
				}
			}
		}

		#endregion

		#region Implementation

		void AssertBizoChangesForm(ZCheckedListBox listBox)
		{
			AssertEquals(false, Bizo.BoolDescriptionPairList[0].Value);
			AssertEquals(false, Bizo.BoolDescriptionPairList[1].Value);
			listBox.SetItemChecked(1, true);
			AssertEquals(false, Bizo.BoolDescriptionPairList[0].Value);
			AssertEquals(true, Bizo.BoolDescriptionPairList[1].Value);
		}

		void AssertFormChangesBizo(ZCheckedListBox listBox)
		{
			AssertEquals(false, listBox.GetItemChecked(0));
			AssertEquals(false, listBox.GetItemChecked(1));
			Bizo.BoolDescriptionPairList[1].Value = true;
			AssertEquals(false, listBox.GetItemChecked(0));
			AssertEquals(true, listBox.GetItemChecked(1));
		}

		DummyBusinessObjectWithBoolDescriptionPairList Bizo
		{
			get
			{
				if (bizo == null)
				{
					bizo = Factory.New<DummyBusinessObjectWithBoolDescriptionPairList>();
				}
				return bizo;
			}
		}
		DummyBusinessObjectWithBoolDescriptionPairList bizo;

		ZCheckedListBox CheckedListBox
		{
			get { return checkedListBox ?? (checkedListBox = new ZCheckedListBox()); }
		}
		ZCheckedListBox checkedListBox;

		protected override bool UsesControlDataBindings
		{
			get { return false; }
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (checkedListBox != null)
			{
				checkedListBox.Dispose();
			}
		}

		#endregion
	}
}
