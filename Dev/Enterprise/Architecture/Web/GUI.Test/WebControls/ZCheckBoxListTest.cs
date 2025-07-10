using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZCheckBoxListTest : WebControlTest
	{
		public void TestRepeatDirection()
		{
			AssertNull("Pre-condition", ListControl.ViewStateInternal["RepeatDirection"]);
			AssertEquals("Pre-condition", RepeatDirection.Vertical, ListControl.RepeatDirection);

			ListControl.RepeatDirection = RepeatDirection.Horizontal;
			AssertEquals(RepeatDirection.Horizontal, ListControl.ViewStateInternal["RepeatDirection"]);
			AssertEquals(RepeatDirection.Horizontal, ListControl.RepeatDirection);
		}

		public void TestRepeatLayout()
		{
			AssertNull("Pre-condition", ListControl.ViewStateInternal["RepeatLayout"]);
			AssertEquals("Pre-condition", RepeatLayout.Flow, ListControl.RepeatLayout);

			ListControl.RepeatLayout = RepeatLayout.Table;
			AssertEquals(RepeatLayout.Table, ListControl.ViewStateInternal["RepeatLayout"]);
			AssertEquals(RepeatLayout.Table, ListControl.RepeatLayout);
		}

		public void TestAutoPostBack()
		{
			Assert("Pre-condition", !ListControl.AutoPostBack);

			ListControl.AutoPostBack = true;
			Assert(ListControl.AutoPostBack);
		}

		public void TestRender()
		{
			AssertRendering(RepeatLayout.Flow, RepeatDirection.Vertical);
			AssertRendering(RepeatLayout.Flow, RepeatDirection.Horizontal);
			AssertRendering(RepeatLayout.Table, RepeatDirection.Vertical);
			AssertRendering(RepeatLayout.Table, RepeatDirection.Horizontal);
		}

		void AssertRendering(RepeatLayout repeatLayout, RepeatDirection repeatDirection)
		{
			StringBuilder expectedString = new StringBuilder();
			RepeatInfo repeatInfo = new RepeatInfo();
			repeatInfo.RepeatLayout = repeatLayout;
			repeatInfo.RepeatDirection = repeatDirection;
			repeatInfo.RenderRepeater(new HtmlTextWriter(new StringWriter(expectedString)), ListControl, ListControl.ControlStyle, ListControl);

			StringBuilder generatedString = new StringBuilder();
			ListControl.RepeatLayout = repeatLayout;
			ListControl.RepeatDirection = repeatDirection;
			ListControl.RenderInternal(new HtmlTextWriter(new StringWriter(generatedString)));
			AssertEquals(expectedString.ToString(), generatedString.ToString());
		}

		#region ISelfBindingWebControl tests

		public void TestIsBindable()
		{
			Assert("Not bindable, TestBizO is not an IEnumerable<IBindableBooleanItem> and BindTo is not specified", !ListControl.IsBindable(TestBizO));

			ListControl.BindTo = "BooleanItems";
			Assert(ListControl.IsBindable(TestBizO));

			ListControl.BindTo = DummyBizoSchema.Constants.Z0_Bool;
			Assert("Property is not an IEnumerable<IBindableBooleanItem>", !ListControl.IsBindable(TestBizO));

			ListControl.BindTo = "";
			List<IBindableBooleanItem> list = TestBizO.BooleanItems;
			Assert(ListControl.IsBindable(list));
		}

		public void TestBind_FromBindTo()
		{
			AssertEquals("Pre-condition", 0, ListControl.ControlsToBeRendered.Count);
			AssertEquals("Pre-condition", 0, ListControl.Controls.Count);

			BindableBooleanItemForTest booleanItem1 = new BindableBooleanItemForTest("option 1", true);
			BindableBooleanItemForTest booleanItem2 = new BindableBooleanItemForTest("option 2", false);
			BindableBooleanItemForTest booleanItem3 = new BindableBooleanItemForTest("option 3", true);
			TestBizO.BooleanItems.Add(booleanItem1);
			TestBizO.BooleanItems.Add(booleanItem2);
			TestBizO.BooleanItems.Add(booleanItem3);
			ListControl.BindTo = "BooleanItems";

			ListControl.Bind(TestBizO);
			AssertEquals(3, ListControl.ControlsToBeRendered.Count);
			AssertEquals(3, ListControl.Controls.Count);
			AssertControlToBeRendered(ListControl.ControlsToBeRendered[0], true, booleanItem1.PK.ToString(), "option 1", false);
			AssertControlToBeRendered(ListControl.ControlsToBeRendered[1], false, booleanItem2.PK.ToString(), "option 2", false);
			AssertControlToBeRendered(ListControl.ControlsToBeRendered[2], true, booleanItem3.PK.ToString(), "option 3", false);
		}

		public void TestBind_DirectlyFromDataSource()
		{
			AssertEquals("Pre-condition", 0, ListControl.ControlsToBeRendered.Count);
			AssertEquals("Pre-condition", 0, ListControl.Controls.Count);

			BindableBooleanItemForTest booleanItem1 = new BindableBooleanItemForTest("option 1", false);
			BindableBooleanItemForTest booleanItem2 = new BindableBooleanItemForTest("option 2", true);
			TestBizO.BooleanItems.Add(booleanItem1);
			TestBizO.BooleanItems.Add(booleanItem2);
			ListControl.AutoPostBack = true;

			ListControl.Bind(TestBizO.BooleanItems);
			AssertEquals(2, ListControl.ControlsToBeRendered.Count);
			AssertEquals(2, ListControl.Controls.Count);
			AssertControlToBeRendered((CheckBox)ListControl.Controls[0], false, booleanItem1.PK.ToString(), "option 1", true);
			AssertControlToBeRendered((CheckBox)ListControl.Controls[1], true, booleanItem2.PK.ToString(), "option 2", true);
		}

		public void TestUnbind()
		{
			ListControl.BindTo = "BooleanItems";
			ListControl.ControlsToBeRendered.Add(new CheckBox());

			ListControl.UnBind();
			AssertEquals("", ListControl.BindTo);
			AssertEquals(0, ListControl.ControlsToBeRendered.Count);
		}

		protected virtual void AssertControlToBeRendered(CheckBox control, bool checkedValue, string id, string text, bool isAutoPostBack)
		{
			AssertEquals(text, control.Text);
			AssertEquals(checkedValue, control.Checked);
			AssertEquals(id, control.ID);
			AssertEquals(isAutoPostBack, control.AutoPostBack);
			AssertEquals(GetExpectedControlToBeRendered().GetType(), control.GetType());
		}

		#endregion

		#region IRepeatInfoUser tests

		public void TestGetItemStyle()
		{
			CheckBox control1 = GetExpectedControlToBeRendered();
			control1.ControlStyle.BackColor = Color.Beige;
			ListControl.ControlsToBeRendered.Add(control1);
			CheckBox control2 = GetExpectedControlToBeRendered();
			control2.ControlStyle.BorderStyle = BorderStyle.Dashed;
			ListControl.ControlsToBeRendered.Add(control2);

			IRepeatInfoUser repeatInfoUser = ListControl;
			AssertEquals(control1.ControlStyle, repeatInfoUser.GetItemStyle(ListItemType.Item, 0));
			AssertEquals(control2.ControlStyle, repeatInfoUser.GetItemStyle(ListItemType.Item, 1));
		}

		public void TestHasFooter()
		{
			IRepeatInfoUser repeatInfoUser = ListControl;
			Assert(!repeatInfoUser.HasFooter);
		}

		public void TestHasHeader()
		{
			IRepeatInfoUser repeatInfoUser = ListControl;
			Assert(!repeatInfoUser.HasHeader);
		}

		public void TestHasSeparators()
		{
			IRepeatInfoUser repeatInfoUser = ListControl;
			Assert(!repeatInfoUser.HasSeparators);
		}

		public void TestRenderItem()
		{
			CheckBox control = GetExpectedControlToBeRendered();
			control.ControlStyle.CssClass = "Meh";
			StringBuilder expectedString = new StringBuilder();
			control.RenderControl(new HtmlTextWriter(new StringWriter(expectedString)));

			StringBuilder generatedString = new StringBuilder();
			ListControl.ControlsToBeRendered.Add(control);
			IRepeatInfoUser repeatInfoUser = ListControl;
			repeatInfoUser.RenderItem(ListItemType.Item, 0, null, new HtmlTextWriter(new StringWriter(generatedString)));
			AssertEquals(expectedString.ToString(), generatedString.ToString());
		}

		public void TestRepeatedItemCount()
		{
			IRepeatInfoUser repeatInfoUser = ListControl;
			AssertEquals("Pre-condition", 0, repeatInfoUser.RepeatedItemCount);

			ListControl.ControlsToBeRendered.Add(new CheckBox());
			AssertEquals(1, repeatInfoUser.RepeatedItemCount);
		}

		#endregion

		protected virtual CheckBox GetExpectedControlToBeRendered()
		{
			return new ZCheckBox();
		}

		protected override Control GetNewControl()
		{
			return new ZCheckBoxList();
		}

		protected override DummyEnterpriseBusinessObject GetNewDataSource()
		{
			return Factory.New<DummyBizOForTest>();
		}

		new DummyBizOForTest TestBizO
		{
			get { return (DummyBizOForTest)base.TestBizO; }
		}

		ZCheckBoxList ListControl
		{
			get { return (ZCheckBoxList)Control; }
		}

		#region DummyBizOForTest

		class DummyBizOForTest : DummyEnterpriseBusinessObject
		{
			public DummyBizOForTest(BusinessObjectFactory factory, DataRow dataRow)
				: base(factory, dataRow)
			{
			}

			public List<IBindableBooleanItem> BooleanItems
			{
				get
				{
					if (fBooleanItems == null)
					{
						fBooleanItems = new List<IBindableBooleanItem>();
					}
					return fBooleanItems;
				}
			}

			List<IBindableBooleanItem> fBooleanItems;
		}

		#endregion

		#region BindableBooleanItemForTest

		class BindableBooleanItemForTest : NonPersistentBusinessObject, IBindableBooleanItem
		{
			public BindableBooleanItemForTest(string text, bool boolValue)
			{
				fText = text;
				fBoolValue = boolValue;
			}

			public ZBool BoolValue
			{
				get { return fBoolValue; }
				set { fBoolValue = value; }
			}

			public ZPropertyInfo BoolValueInfo
			{
				get { return GetZPropertyInfo(nameof(BoolValue)); }
			}

			public ZString Text
			{
				get { return fText; }
			}

			ZBool fBoolValue;
			readonly ZString fText;
		}

		#endregion
	}
}
