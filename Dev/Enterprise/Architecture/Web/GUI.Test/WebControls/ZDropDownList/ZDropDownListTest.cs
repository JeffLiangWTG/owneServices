using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZDropDownListTest : WebControlTest
	{
		#region Setup

		protected DummyWithCodeDescriptionPairList DummyWithList;

		protected override void SetUp()
		{
			base.SetUp();
			DummyWithList = Factory.New<DummyWithCodeDescriptionPairList>();
		}

		protected override Control GetNewControl()
		{
			return new ZDropDownList();
		}

		protected ZDropDownList DropDown
		{
			get { return (ZDropDownList)Control; }
		}

		#endregion

		[ExpectNoExceptions("Should not call Load() on a NonPersistentBusinessObjectCollection")]
		public void TestBindToNonPersistentBizObjCollection()
		{
			DummyNonPersistentBusinessObject bizObj = new DummyNonPersistentBusinessObject();
			AssertEquals("Collection is NOT Loaded", false, bizObj.Collection.IsLoaded);
			DropDown.BindTo = "Code";
			DropDown.BindToList = "Collection";
			DropDown.DataTextField = "Z0_String";
			DropDown.DataValueField = "Z0_String";
			DropDown.Bind(bizObj);
			AssertEquals("Collection is stll NOT Loaded", false, bizObj.Collection.IsLoaded);
		}

		class DummyNonPersistentBusinessObject : NonPersistentBusinessObject
		{
			public ZString Code
			{
				get { return fCode; }
				set { fCode = value; }
			}

			ZString fCode;

			public ZPropertyInfo CodeInfo
			{
				get { return GetZPropertyInfo(nameof(Code)); }
			}

			public DummyNonPersistentBusinessObjectCollection Collection
			{
				get
				{
					if (fCollection == null)
					{
						fCollection = new DummyNonPersistentBusinessObjectCollection();
					}

					return fCollection;
				}
			}

			DummyNonPersistentBusinessObjectCollection fCollection;
		}

		class DummyNonPersistentBusinessObjectCollection : NonPersistentBusinessObjectCollection<DummyNonPersistentBusinessObjectWithRow>
		{
			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				return new DummyNonPersistentBusinessObjectWithRow(Factory);
			}
		}

		public void TestLoadPostDataNoDataSource()
		{
			Assert("Drop down is not bound to a data source", !DropDown.HasDataSource);

			var key = "Key";
			var data = new NameValueCollection();
			data.Add(key, string.Empty);

			var expected = ExpectedLoadPostData();
			foreach (Pair testData in expected.Keys)
			{
				DropDown.CachedSelectedValue = testData.First.ToString();
				data[key] = testData.Second.ToString();
				AssertLoadPostData(testData, key, data, expected[testData]);
			}
		}

		public void TestLoadPostDataNoDataSource_SelectEmptyString()
		{
			var key = "Key";
			var data = new NameValueCollection();
			data.Add(key, string.Empty);

			DropDown.CachedSelectedValue = null;

			Assert("Should be no changes if selecting empty string and no cached value", !DropDown.LoadPostDataInternal(key, data));
		}

		public void TestHasDataSource()
		{
			AssertNull(DropDown.BusinessEntity);
			Assert(!DropDown.HasDataSource);

			BindDropDown();
			Assert(DropDown.HasDataSource);
		}

		public void TestLoadPostDataWithDataSource()
		{
			var dataSource = BindDropDown();
			Assert("Drop down is bound to a data source", DropDown.HasDataSource);

			var key = "Key";
			var data = new NameValueCollection();
			data.Add(key, string.Empty);

			var expected = ExpectedLoadPostData();
			foreach (Pair testData in expected.Keys)
			{
				var localValue = testData.First.ToString();
				var postedValue = testData.Second.ToString();
				var otherValue = $"not '{localValue}'";

				data[key] = postedValue;
				dataSource.Code = localValue;
				DropDown.CachedSelectedValue = otherValue;
				AssertLoadPostData(testData, key, data, expected[testData]);

				dataSource.Code = otherValue;
				DropDown.CachedSelectedValue = localValue;
				AssertLoadPostData(testData, key, data, expected[testData]);
			}
		}

		public void TestLoadPostDataWithDataSource_SelectEmptyString()
		{
			var dataSource = BindDropDown();
			var key = "Key";
			var data = new NameValueCollection();
			data.Add(key, string.Empty);

			DropDown.CachedSelectedValue = null;
			dataSource.Code = "ABC";

			Assert("Should be changes if selecting empty string is different from data bind and no cached value", DropDown.LoadPostDataInternal(key, data));
		}

		DummyNonPersistentBusinessObject BindDropDown()
		{
			var dataSource = new DummyNonPersistentBusinessObject();
			DropDown.BindTo = "Code";
			DropDown.BindToList = "Collection";
			DropDown.DataTextField = "Z0_String";
			DropDown.DataValueField = "Z0_String";
			DropDown.Bind(dataSource);

			return dataSource;
		}

		void AssertLoadPostData(Pair testData, string key, NameValueCollection postData, bool expectedResult)
		{
			AssertEquals(string.Format("Old value: {0} New value: {1}", testData.First, testData.Second), expectedResult, DropDown.LoadPostDataInternal(key, postData));
		}

		public void TestSetCachedSelectedValueOnBind()
		{
			var bizObj = BindDropDown();

			DropDown.Page.IsPostBack = false;
			bizObj.Code = "FST";
			AssertNotEquals("FST", DropDown.CachedSelectedValue);
			DropDown.Bind(bizObj);
			AssertEquals("FST", DropDown.CachedSelectedValue);

			DropDown.Page.IsPostBack = true;
			bizObj.Code = "SEC";
			DropDown.Bind(bizObj);
			AssertEquals("FST", DropDown.CachedSelectedValue);
		}

		protected virtual Dictionary<Pair, bool> ExpectedLoadPostData()
		{
			var result = new Dictionary<Pair, bool>();
			result.Add(new Pair(string.Empty, string.Empty), false);
			result.Add(new Pair(string.Empty, "Test"), true);
			result.Add(new Pair("Test", string.Empty), true);
			result.Add(new Pair("Test", "Test"), false);
			return result;
		}

		public void TestDisplayNotifications()
		{
			TestDisplayNotificationsCore();
		}

		protected virtual void TestDisplayNotificationsCore()
		{
			Assert(DropDown.DisplayNotifications);
		}

		public void TestBindTo()
		{
			DropDown.BindTo = "Z0_Code";
			DropDown.BindToList = "DummyList";
			DummyWithList.Z0_Code = DummyWithList.DummyList[3].Code;
			AssertEquals("Is not ReadOnly", false, DummyWithList.Z0_CodeInfo.ReadOnly);
			DropDown.Bind(DummyWithList);
			AssertEquals(DummyWithList.Z0_Code, DropDown.SelectedValue);
			AssertEquals("DropDown should be enabled", true, DropDown.Enabled);

			DummyWithList.Z0_Code_ReadOnly = true;
			DropDown.Bind(DummyWithList);
			AssertEquals("DropDown should be disabled", false, DropDown.Enabled);
		}

		public void TestDropDownListPostBack()
		{
			DropDown.BindTo = "Z0_Code";
			DropDown.BindToList = "DummyList";
			DummyWithList.Z0_Code = DummyWithList.DummyList[3].Code;
			DropDown.Bind(DummyWithList);

			AssertEquals(DummyWithList.Z0_Code, DropDown.SelectedValue);
			AssertEquals("Number of Items", DropDown.Items.Count, DummyWithList.DummyList.Count);

			Page.IsPostBack = true;

			DropDown.SelectedValue = DummyWithList.DummyList[1].Code;
			DropDown.Bind(DummyWithList);
			AssertEquals(DummyWithList.Z0_Code, DropDown.SelectedValue);

			DropDown.SelectedValue = DummyWithList.DummyList[3].Code;
			DropDown.Bind(DummyWithList);
			AssertEquals(DummyWithList.Z0_Code, DropDown.SelectedValue);
		}

		public void TestBusinessObjectCollectionPopulatesList()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();

			AssertEquals("collection is NOT Loaded", false, bizO.Collection.IsLoaded);
			DropDown.BindTo = DummyBusinessObject.Schema.Z0_Code;
			DropDown.BindToList = "Collection";
			DropDown.DataTextField = "Z0_Description";
			DropDown.DataValueField = "Z0_Code";
			DropDown.Bind(bizO);
			AssertEquals("collection is now Loaded", true, bizO.Collection.IsLoaded);
		}

		public void TestShowEmptyElement()
		{
			DropDown.BindTo = "Z0_Code";
			DropDown.BindToList = "DummyList";
			DropDown.ShowEmptyItem = true;
			DummyWithList.Z0_Code = DummyWithList.DummyList[3].Code;
			DropDown.Bind(DummyWithList);

			AssertEquals(DummyWithList.Z0_Code, DropDown.SelectedValue);
			AssertEquals("Number of Items", DropDown.Items.Count, DummyWithList.DummyList.Count + 1);
			AssertEquals("default empty item value is blank", "", DropDown.Items[0].Value);
			AssertEquals("default empty item text is blank", "", DropDown.Items[0].Text);

			DropDown.EmptyItemText = "Test";
			DropDown.Bind(DummyWithList);
			AssertEquals(DummyWithList.Z0_Code, DropDown.SelectedValue);
			AssertEquals("Number of Items", DropDown.Items.Count, DummyWithList.DummyList.Count + 1);
			AssertEquals("default empty item value is blank", "", DropDown.Items[0].Value);
			AssertEquals("default empty item text is NOT blank", "Test", DropDown.Items[0].Text);
		}

		public void TestShowCustomItem()
		{
			DropDown.BindTo = "Z0_Code";
			DropDown.BindToList = "DummyList";
			DropDown.CustomItemText = "Custom Text";
			DropDown.CustomItemValue = "CVL";
			DropDown.ShowCustomItem = false;
			DummyWithList.Z0_Code = DummyWithList.DummyList[3].Code;
			DropDown.Bind(DummyWithList);

			AssertEquals(DummyWithList.Z0_Code, DropDown.SelectedValue);
			AssertEquals("Number of Items", DropDown.Items.Count, DummyWithList.DummyList.Count);
			AssertNotEquals("default empty item value is blank", "Custom Text", DropDown.Items[0].Value);
			AssertNotEquals("default empty item text is blank", "CVL", DropDown.Items[0].Text);

			DropDown.ShowEmptyItem = false;
			DropDown.ShowCustomItem = true;
			DropDown.Bind(DummyWithList);
			AssertEquals(DummyWithList.Z0_Code, DropDown.SelectedValue);
			AssertEquals("Number of Items", DropDown.Items.Count, DummyWithList.DummyList.Count + 1);
			AssertEquals("default empty item value is blank", "Custom Text", DropDown.Items[0].Text);
			AssertEquals("default empty item text is blank", "CVL", DropDown.Items[0].Value);
		}

		public void TestDataSourceIsAssignedOnBinding()
		{
			AssertNull("Pre-condition", DropDown.BusinessEntity);

			DropDown.BindTo = "Z0_Code";
			DropDown.BindToList = "DummyList";
			DropDown.Bind(DummyWithList);
			AssertEquals("Should be assigned on Binding", DummyWithList, DropDown.BusinessEntity);
		}

		public void TestRebindOnRaisePostDataChangedEvent()
		{
			DropDown.BindTo = "Z0_Code";
			DropDown.BindToList = "DummyList";
			DummyWithList.Z0_Code = DummyWithList.DummyList[1].Code;
			DropDown.Bind(DummyWithList);
			AssertEquals("Should be bound to DummyWithList.Z0_Code", DummyWithList.DummyList[1].Code, DropDown.SelectedValue);

			DummyWithList.Z0_Code = DummyWithList.DummyList[2].Code;
			DropDown.RaisePostDataChangedEventInternal();
			AssertEquals("Should be rebound on RaisePostDataChangedEvent", DummyWithList.DummyList[2].Code, DropDown.SelectedValue);
		}

		[ExpectNoExceptions]
		public void TestRaisePostDataChangedEvent_NullBusinessEntity()
		{
			DropDown.RaisePostDataChangedEventInternal();
		}

		public void TestUnbind()
		{
			DropDown.BindTo = "Z0_Code";
			DropDown.BindToList = "DummyList";
			DummyWithList.Z0_Code = DummyWithList.DummyList[1].Code;
			DropDown.Bind(DummyWithList);
			AssertEquals("Pre-condition", DummyWithList, DropDown.BusinessEntity);
			Assert("Pre-condition", DropDown.Items.Count > 0);
			AssertEquals("DropDown should be enabled", true, DropDown.Enabled);

			DropDown.UnBind();
			AssertNull("Should be set to null", DropDown.BusinessEntity);
			AssertEquals("Item list should be cleared", 0, DropDown.Items.Count);
			AssertEquals("DropDown should be disabled", false, DropDown.Enabled);
		}

		public void TestDisplayStyle()
		{
			AssertEquals(OComboBoxDropDownStyle.DescriptionOnly, DropDown.DisplayStyle);
			DropDown.DisplayStyle = OComboBoxDropDownStyle.CodeAndDescription;
			AssertEquals(OComboBoxDropDownStyle.CodeAndDescription, DropDown.DisplayStyle);

			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();

			DropDown.BindTo = DummyBusinessObject.Schema.Z0_Code;
			DropDown.BindToList = "Collection";
			DropDown.DataTextField = "Z0_Description";
			DropDown.DataValueField = "Z0_Code";
			DropDown.Bind(bizO);
			string collectionFirstItemValue = bizO.Collection[0].Z0_Code;
			string collectionFirstItemText = bizO.Collection[0].Z0_Description;
			AssertEquals(DropDown.Items[0].Value, collectionFirstItemValue);
			AssertEquals(DropDown.Items[0].Text, collectionFirstItemValue + " - " + collectionFirstItemText);
		}

		public void TestText()
		{
			TestTextCore(OComboBoxDropDownStyle.CodeOnly, "c1", "desc1", "c1");
			TestTextCore(OComboBoxDropDownStyle.DescriptionOnly, "c2", "desc2", "desc2");
			TestTextCore(OComboBoxDropDownStyle.CodeAndDescription, "c3", "desc3", "c3 - desc3");
		}

		void TestTextCore(OComboBoxDropDownStyle displayStyle, string code, string description, string expectedDropDownText)
		{
			var bizO = Factory.New<DummyBusinessObject>();
			DropDown.BindTo = DummyBusinessObject.Schema.Z0_Code;
			DropDown.BindToList = "Collection";
			DropDown.DataTextField = "Z0_Description";
			DropDown.DataValueField = "Z0_Code";
			DropDown.DisplayStyle = displayStyle;
			var item = bizO.Collection.AddNew();
			item.Z0_Code = code;
			item.Z0_Description = description;
			DropDown.Bind(bizO);
			DropDown.SelectedIndex = bizO.Collection.ToList().FindIndex(l => l == item);
			bizO.ReadOnly = true;
			AssertEquals("Precondition", true, bizO.Z0_CodeInfo.ReadOnly);
			AssertEquals("Should show desciption based on style: " + displayStyle, expectedDropDownText, DropDown.Text);
			bizO.ReadOnly = false;
			AssertEquals("Precondition", false, bizO.Z0_CodeInfo.ReadOnly);
			AssertEquals("Should show desciption based on style: " + displayStyle, expectedDropDownText, DropDown.Text);
		}
	}
}
