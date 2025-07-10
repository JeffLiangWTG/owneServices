using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Testing;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class KComboBoxTest : TestCase
	{
		public void TestBinding()
		{
			KBindingList<TestDataSource> collection = new KBindingList<TestDataSource>();
			Form.Show();
			Form.Controls.Add(ComboBox);
			Form.SetDataBinding(collection, "");
			Form.BindingSource.SetBindingMember(ComboBox, "PropertyWithListValueAndDisplayMembers");

			collection.AddNew();
			AssertEquals("DataSource", typeof(TestListElementCollection), ComboBox.DataSource.GetType());
			AssertEquals("DisplayMember", "Display", ComboBox.DisplayMember);
		}

		public void TestBoundValue_WhenUserTypesIntoComboBox()
		{
			Form.Controls.Add(ComboBox);
			if (Form.ShowAndCheckFormIsActive())
			{
				ComboBox.DisplayMember = "Display";
				ComboBox.ValueMember = "Value";

				List<TestItem> list = new List<TestItem>();
				list.Add(new TestItem("display1", "value1"));
				list.Add(new TestItem("display2", "value2"));
				ComboBox.DataSource = list;

				KSendKeys.SendWait("display2", ComboBox);
				AssertEquals("The BoundValue should be the one the user typed", "value2", ComboBox.BoundValue);
			}
		}

		public void TestBoundValue_ReturningEmptyValues()
		{
			Form.Controls.Add(ComboBox);
			Form.Show();

			ComboBox.DisplayMember = "Display";
			ComboBox.ValueMember = "Value";

			ComboBox.Items.Add(new TestItem("display1", "value1"));
			ComboBox.Items.Add(new TestItem("display2", "value2"));
			ComboBox.EmptyValue = "empty";

			ComboBox.SelectedItem = ComboBox.Items[0];
			AssertEquals("SelectedItem set for test", ComboBox.Items[0], ComboBox.SelectedItem);

			ComboBox.SelectedItem = null;
			AssertEquals("Empty BoundValue", "empty", ComboBox.BoundValue);
			ComboBox.ValueMember = "";
			AssertEquals("Empty BoundValue", "empty", ComboBox.BoundValue);
		}

		public void TestBoundValue_WhenValueNotInList()
		{
			Form.Controls.Add(ComboBox);
			if (Form.ShowAndCheckFormIsActive())
			{
				ComboBox.DisplayMember = "Display";
				ComboBox.ValueMember = "Value";

				List<TestItem> list = new List<TestItem>();
				list.Add(new TestItem("display1", "value1"));
				list.Add(new TestItem("display2", "value2"));
				ComboBox.DataSource = list;

				ComboBox.BoundValue = null;
				AssertEquals("The BoundValue should be the one set programatically", null, ComboBox.BoundValue);

				ComboBox.SelectedIndex = 0;
				ComboBox.BoundValue = "splaty";
				AssertEquals("The BoundValue should be the one set programatically", "splaty", ComboBox.BoundValue);
				ComboBox.SelectedIndex = 1;
				AssertEquals("The BoundValue should still be the one set programatically", "splaty", ComboBox.BoundValue);

				KSendKeys.SendWait("display2", ComboBox);
				AssertEquals("The BoundValue should be the one the user just selected", "value2", ComboBox.BoundValue);
				KSendKeys.SendWait("{BACKSPACE}", ComboBox);
				AssertEquals("The BoundValue should be the one the user just selected", null, ComboBox.BoundValue);
			}
		}

		public void TestBoundValue_WhenValueMemberSameAsDisplayMemberAndValueNotInList()
		{
			Form.Controls.Add(ComboBox);
			if (Form.ShowAndCheckFormIsActive())
			{
				ComboBox.DisplayMember = "Value";
				ComboBox.ValueMember = "Value";

				List<TestItem> list = new List<TestItem>();
				list.Add(new TestItem("", "value1"));
				list.Add(new TestItem("", "value2"));
				ComboBox.DataSource = list;

				ComboBox.SelectedIndex = 1;
				AssertEquals("BoundValue when the value is in the list", "value2", ComboBox.BoundValue);

				KSendKeys.SendWait("ValueTypedIn", ComboBox);
				AssertEquals("BoundValue when the value is typed into the combo box", "ValueTypedIn", ComboBox.BoundValue);

				ComboBox.BoundValue = "ValueNotInList";
				AssertEquals("Text when value BoundValue set to is not in the list", "ValueNotInList", ComboBox.Text);
			}
		}

		public void TestBoundValue_WhenNoValueDisplayMembersAndValueNotInList()
		{
			Form.Controls.Add(ComboBox);
			if (Form.ShowAndCheckFormIsActive())
			{
				ComboBox.DisplayMember = "";
				ComboBox.ValueMember = "";

				List<string> list = new List<string>();
				list.Add("value1");
				list.Add("value2");
				ComboBox.DataSource = list;

				ComboBox.SelectedIndex = 1;
				AssertEquals("BoundValue when the value is in the list", "value2", ComboBox.BoundValue);

				KSendKeys.SendWait("ValueTypedIn", ComboBox);
				AssertEquals("BoundValue when the value is typed into the combo box", "ValueTypedIn", ComboBox.BoundValue);

				ComboBox.BoundValue = "ValueNotInList";
				AssertEquals("Text when value BoundValue set to is not in the list", "ValueNotInList", ComboBox.Text);
			}
		}

		public void TestBoundValue_SetsTextManually_WhenNoValueMemberWithDisplayMember()
		{
			DataSource.EntityWithListDisplayMemberOnly = new TestListElement();
			DataSource.EntityWithListDisplayMemberOnly.Display = TestListElementCollection.DisplayValueToExcludeFromList;
			AssertEquals(
				"Entity excluded from the list for the test. The display value must be obtained from the entity returned from the property and not the list.",
				false, DataSource.List.Contains(DataSource.EntityWithListDisplayMemberOnly));
			AssertNotNull("Entity available from property for the test", DataSource.EntityWithListDisplayMemberOnly);
			AssertEquals("Entity DisplayValue for the test", TestListElementCollection.DisplayValueToExcludeFromList, DataSource.EntityWithListDisplayMemberOnly.Display);

			Form.BindingSource.SetBindingMember(ComboBox, TestDataSource.Properties.EntityWithListDisplayMemberOnly.Name);
			Form.Controls.Add(ComboBox);
			Form.Show();
			Form.SetDataBinding(DataSource, "");
			AssertEquals(
				"ComboBox.Text set from the display member on the entity returned by the bound property",
				TestListElementCollection.DisplayValueToExcludeFromList, ComboBox.Text);
		}

		[ExpectNoExceptions]
		public void TestSelectedValue_AllowNullWhileSettingDataSource()
		{
			Form.Controls.Add(ComboBox);
			Form.Show();

			ComboBox.DisplayMember = "Display";
			ComboBox.ValueMember = "Value";

			List<TestItem> source = new List<TestItem>();
			ComboBox.DataSource = source;
			source.Add(new TestItem("display1", "value1"));
			source.Add(new TestItem("display2", "value2"));

			ComboBox.BoundValue = null;
			ComboBox.DataSource = source; // expect no exception due to SelectedValue being null
		}

		public void TestDataSource_NullifiedOnDispose()
		{
			ComboBox.DataSource = new List<TestItem>();
			ComboBox.Disposed += delegate
			{
				AssertNull("The data source should be nullified now", ComboBox.DataSource);
			};
			ComboBox.Dispose();
		}

#if !WINZOR

		public void TestComboBoxShowToolTip()
		{
			Form.Controls.Add(ComboBox);
			Form.Show();
			Application.DoEvents();

			ComboBox.DisplayMember = "Display";
			ComboBox.ValueMember = "Value";

			var source = new List<TestItem>();
			source.Add(new TestItem("display1", "value1"));
			source.Add(new TestItem("display2", "value2"));
			ComboBox.DataSource = source;

			ComboBox.SelectedIndex = 0;
			ComboBox.OnMouseHover(EventArgs.Empty);
			AssertEquals(1, ToolTipService.Cache.Count);
			AssertEquals("display1", ToolTipService.GetToolTip(ComboBox));

			ComboBox.SelectedIndex = 1;
			ComboBox.OnMouseHover(EventArgs.Empty);
			AssertEquals("display2", ToolTipService.GetToolTip(ComboBox));
		}

#endif

		public void TestUpdateDropDownWidth()
		{
			Form.Controls.Add(ComboBox);
			Form.Show();

			ComboBox.DisplayMember = "Display";
			ComboBox.ValueMember = "Value";
			ComboBox.Width = 50;

			var source = new List<TestItem>();
			source.Add(new TestItem("display1", "value1"));
			source.Add(new TestItem("display2 with long value", "value2"));
			ComboBox.DataSource = source;
			ComboBox.UpdateDropDownWidth();
			Assert(ComboBox.DropDownWidth > ComboBox.Width);
		}

		#region Binding the List Data Source

		public void TestDataSourceBound_OnFirstBind_WhenValueAndDisplayMembersDifferent()
		{
			Form.BindingSource.SetBindingMember(ComboBox, TestDataSource.Properties.PropertyWithListValueAndDisplayMembers.Name);
			ShowFormAndFocusAndDropDownComboBox();

			AssertEquals(1, ComboBox.FirstBindIndex);
			AssertEquals(2, ComboBox.DataSourceFirstPulledIndex);
		}

		[RequiresSTA]
		public void TestDataSourceBound_OnFocus_ValueMemberAndDisplayMemberSame_WithAutoComplete()
		{
			ComboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
			Form.BindingSource.SetBindingMember(ComboBox, TestDataSource.Properties.PropertyWithSameListValueAndDisplayMembers.Name);
			ShowFormAndFocusAndDropDownComboBox();

			AssertEquals(1, ComboBox.FirstBindIndex);
			AssertEquals(2, ComboBox.FirstGotFocusIndex);
			AssertEquals(3, ComboBox.DataSourceFirstPulledIndex);
		}

		[RequiresSTA]
		public void TestDataSourceBound_OnFocus_NoValueMemberWithDisplayMember_WithAutoComplete()
		{
			ComboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
			Form.BindingSource.SetBindingMember(ComboBox, TestDataSource.Properties.PropertyWithListDisplayMemberOnly.Name);
			ShowFormAndFocusAndDropDownComboBox();

			AssertEquals(1, ComboBox.FirstBindIndex);
			AssertEquals(2, ComboBox.FirstGotFocusIndex);
			AssertEquals(3, ComboBox.DataSourceFirstPulledIndex);
		}

		[RequiresSTA]
		public void TestDataSourceBound_OnDropDown_ValueMemberAndDisplayMemberSame_WithNoAutoComplete()
		{
			ComboBox.AutoCompleteMode = AutoCompleteMode.None;
			Form.BindingSource.SetBindingMember(ComboBox, TestDataSource.Properties.PropertyWithSameListValueAndDisplayMembers.Name);
			ShowFormAndFocusAndDropDownComboBox();

			AssertEquals(1, ComboBox.FirstBindIndex);
			AssertEquals(2, ComboBox.FirstGotFocusIndex);
			AssertEquals(3, ComboBox.DroppedDownIndex);
			AssertEquals(4, ComboBox.DataSourceFirstPulledIndex);
		}

		void ShowFormAndFocusAndDropDownComboBox()
		{
			Form.Controls.Add(new KTextBox()); // don't gain focus initially
			Form.Controls.Add(ComboBox);

			Form.SetDataBinding(DataSource, "");
			Form.Show();

			ComboBox.Focus();
			ComboBox.DroppedDown = true;
		}

		#endregion

		#region Test Classes

		class TestComboBox : KComboBox
		{
			public int FirstBindIndex;
			public override void SetDataBinding(object dataSource, string dataMember)
			{
				FirstBindIndex = eventIndex++;
				base.SetDataBinding(dataSource, dataMember);
			}

			public int DataSourceFirstPulledIndex;
			protected override void OnDataSourceChanged(EventArgs e)
			{
				if (DataSourceFirstPulledIndex == 0 && DataSource != null)
				{
					DataSourceFirstPulledIndex = eventIndex++;
				}
				base.OnDataSourceChanged(e);
			}

			public int DroppedDownIndex;
			protected override void OnDropDown(EventArgs e)
			{
				DroppedDownIndex = eventIndex++;
				base.OnDropDown(e);
			}

			public int FirstGotFocusIndex;
			protected override void OnGotFocus(EventArgs e)
			{
				if (FirstGotFocusIndex == 0)
				{
					FirstGotFocusIndex = eventIndex++;
				}
				base.OnGotFocus(e);
			}

			int eventIndex = 1;

			internal new void OnMouseHover(EventArgs e) => base.OnMouseHover(e);
		}

		class TestForm : KForm
		{
			public new KBindingSource BindingSource
			{ get { return base.BindingSource; } }
		}

		public class TestItem
		{
			public TestItem(object display, string value)
			{
				this.display = display;
				this.value = value;
			}

			public object Display
			{ get { return display; } }
			readonly object display;

			public string Value
			{ get { return value; } }
			readonly string value;

			public override bool Equals(object obj)
			{
				TestItem rhs = obj as TestItem;
				return rhs != null && Display == rhs.Display && Value == rhs.Value;
			}

			public override int GetHashCode()
			{ return Value.GetHashCode(); }
		}

		public class TestDataSource : KComponent
		{
			public abstract class Properties
			{
				public static readonly KPropertyDescriptor PropertyWithListValueAndDisplayMembers = (KPropertyDescriptor)PropertyDescriptorCollectionWithWrappingProperties.FromType(typeof(TestDataSource))["PropertyWithListValueAndDisplayMembers"];
				public static readonly KPropertyDescriptor PropertyWithSameListValueAndDisplayMembers = (KPropertyDescriptor)PropertyDescriptorCollectionWithWrappingProperties.FromType(typeof(TestDataSource))["PropertyWithSameListValueAndDisplayMembers"];
				public static readonly KPropertyDescriptor PropertyWithListDisplayMemberOnly = (KPropertyDescriptor)PropertyDescriptorCollectionWithWrappingProperties.FromType(typeof(TestDataSource))["PropertyWithListDisplayMemberOnly"];
				public static readonly KPropertyDescriptor EntityWithListDisplayMemberOnly = (KPropertyDescriptor)PropertyDescriptorCollectionWithWrappingProperties.FromType(typeof(TestDataSource))["EntityWithListDisplayMemberOnly"];
			}

			public TestDataSource()
			{
				TestListElementCollection list = this.List;
				if (list.Count == 0)
				{
					TestListElement excludedItem = list.AddNew();
					excludedItem.Value = TestListElementCollection.ValueToExcludeFromList;
					excludedItem.Display = TestListElementCollection.DisplayValueToExcludeFromList;
				}
			}

			[List("List", "Value", "Display")]
			public string PropertyWithListValueAndDisplayMembers { get; set; }

			[List("List", "Value", "Value")]
			public string PropertyWithSameListValueAndDisplayMembers { get; set; }

			[List("List", "", "Display"), DefaultValue(TestListElementCollection.ValueToExcludeFromList)]
			public string PropertyWithListDisplayMemberOnly { get; set; }

			[List("List", "", "Display")]
			public TestListElement EntityWithListDisplayMemberOnly { get; set; }

			public TestListElementCollection List
			{
				get { return list ?? (list = new TestListElementCollection()); }
			}
			TestListElementCollection list;
		}

		public class TestListElement : KComponent
		{
			public string Value { get; set; }
			public string Display { get; set; }

			public override bool Equals(object obj)
			{
				TestListElement rhs = obj as TestListElement;
				return rhs != null && Value == rhs.Value && Display == rhs.Display;
			}

			public override int GetHashCode()
			{
				return Value == null ? 0 : Value.GetHashCode();
			}

			public static bool operator ==(TestListElement lhs, TestListElement rhs)
			{
				return object.Equals(lhs, rhs);
			}

			public static bool operator !=(TestListElement lhs, TestListElement rhs)
			{
				return !(lhs == rhs);
			}
		}

		public class TestListElementCollection : KBindingList<TestListElement>
		{
			public const string ValueToExcludeFromList = "ValueToExcludeFromList";
			public const string DisplayValueToExcludeFromList = "DisplayValueToExcludeFromList";
		}

		#endregion

		#region Implementation

		TestForm Form
		{
			get
			{
				if (form == null)
				{
					form = new TestForm();
					form.DataSourceType = typeof(TestDataSource);
				}
				return form;
			}
		}
		TestForm form;

		TestComboBox ComboBox
		{ get { return comboBox ?? (comboBox = new TestComboBox()); } }
		TestComboBox comboBox;

		TestDataSource DataSource
		{ get { return dataSource ?? (dataSource = new TestDataSource()); } }
		TestDataSource dataSource;

		protected override void TearDown()
		{
			base.TearDown();
			if (comboBox != null)
			{
				comboBox.Dispose();
			}
			if (form != null)
			{
				form.Dispose();
			}
		}

		#endregion
	}
}
