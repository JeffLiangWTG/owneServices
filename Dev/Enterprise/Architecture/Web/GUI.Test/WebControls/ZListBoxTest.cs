using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZListBoxTest : WebControlTest
	{
		#region Setup

		DummyWithCodeDescriptionPairList DummyWithList;

		protected override void SetUp()
		{
			base.SetUp();
			DummyWithList = Factory.New<DummyWithCodeDescriptionPairList>();
		}

		protected override System.Web.UI.Control GetNewControl()
		{
			return new ZListBoxForTest();
		}

		ZListBoxForTest ListBox
		{
			get { return (ZListBoxForTest)Control; }
		}

		class ZListBoxForTest : ZListBox
		{
			#region Test Properties

			public void RaisePostDataChangedEventForTesting() => RaisePostDataChangedEvent();

			#endregion
		}

		#endregion

		public void TestBindTo()
		{
			ListBox.BindTo = "Z0_Code";
			ListBox.BindToList = "DummyList";
			DummyWithList.Z0_Code = DummyWithList.DummyList[3].Code;
			ListBox.Bind(DummyWithList);
			AssertEquals(DummyWithList.Z0_Code, ListBox.SelectedValue);
		}

		public void TestListBoxListPostBack()
		{
			ListBox.BindTo = "Z0_Code";
			ListBox.BindToList = "DummyList";
			DummyWithList.Z0_Code = DummyWithList.DummyList[3].Code;
			ListBox.Bind(DummyWithList);

			AssertEquals(DummyWithList.Z0_Code, ListBox.SelectedValue);
			AssertEquals("Number of Items", ListBox.Items.Count, DummyWithList.DummyList.Count);

			Page.IsPostBack = true;

			ListBox.SelectedValue = DummyWithList.DummyList[1].Code;
			ListBox.Bind(DummyWithList);
			AssertEquals(DummyWithList.Z0_Code, ListBox.SelectedValue);

			ListBox.SelectedValue = DummyWithList.DummyList[3].Code;
			ListBox.Bind(DummyWithList);
			AssertEquals(DummyWithList.Z0_Code, ListBox.SelectedValue);
		}

		public void TestBusinessObjectCollectionPopulatesList()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();

			AssertEquals("collection is NOT Loaded", false, bizO.Collection.IsLoaded);
			ListBox.BindTo = DummyBusinessObject.Schema.Z0_Code;
			ListBox.BindToList = "Collection";
			ListBox.DataTextField = "Z0_Description";
			ListBox.DataValueField = "Z0_Code";
			ListBox.Bind(bizO);
			AssertEquals("collection is now Loaded", true, bizO.Collection.IsLoaded);
		}

		public void TestAllowSortingIs()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			bizO.Collection.Load();
			AssertEquals("Should be three child Business Objects", 3, bizO.Collection.Count);

			bizO.Collection[0].Z0_Description = "Gamma";
			bizO.Collection[1].Z0_Description = "Beta";
			bizO.Collection[2].Z0_Description = "Alpha";

			ListBox.AllowSorting = false;

			ListBox.BindTo = DummyBusinessObject.Schema.Z0_Code;
			ListBox.BindToList = "Collection";
			ListBox.DataTextField = "Z0_Description";
			ListBox.DataValueField = "Z0_Code";
			ListBox.Bind(bizO);
			AssertEquals("Sorting should be not allowed", false, ListBox.AllowSorting);

			AssertEquals("Should be three Items", 3, ListBox.Items.Count);
			AssertEquals("Items should be unsorted", "Gamma", ListBox.Items[0].Text);
			AssertEquals("Items should be unsorted", "Beta", ListBox.Items[1].Text);
			AssertEquals("Items should be unsorted", "Alpha", ListBox.Items[2].Text);

			ListBox.UnBind();
			ListBox.AllowSorting = true;

			ListBox.Bind(bizO);
			AssertEquals("Sorting should be allowed", true, ListBox.AllowSorting);

			AssertEquals("Should be three Items", 3, ListBox.Items.Count);
			AssertEquals("Items should be sorted", "Alpha", ListBox.Items[0].Text);
			AssertEquals("Items should be sorted", "Beta", ListBox.Items[1].Text);
			AssertEquals("Items should be sorted", "Gamma", ListBox.Items[2].Text);
		}

		public void TestDataSourceIsAssignedOnBinding()
		{
			AssertNull("Pre-condition", ListBox.BusinessEntity);

			ListBox.BindTo = "Z0_Code";
			ListBox.BindToList = "DummyList";
			ListBox.Bind(DummyWithList);
			AssertEquals("Should be assigned on Binding", DummyWithList, ListBox.BusinessEntity);
		}

		public void TestRebindOnRaisePostDataChangedEvent()
		{
			ListBox.BindTo = "Z0_Code";
			ListBox.BindToList = "DummyList";
			DummyWithList.Z0_Code = DummyWithList.DummyList[1].Code;
			ListBox.Bind(DummyWithList);
			AssertEquals("Should be bound to DummyWithList.Z0_Code", DummyWithList.DummyList[1].Code, ListBox.SelectedValue);

			DummyWithList.Z0_Code = DummyWithList.DummyList[2].Code;
			ListBox.RaisePostDataChangedEventForTesting();
			AssertEquals("Should be rebound on RaisePostDataChangedEvent", DummyWithList.DummyList[2].Code, ListBox.SelectedValue);
		}

		[ExpectNoExceptions]
		public void TestRaisePostDataChangedEvent_NullBusinessEntity()
		{
			ListBox.RaisePostDataChangedEventForTesting();
		}

		public void TestUnbind()
		{
			ListBox.BindTo = "Z0_Code";
			ListBox.BindToList = "DummyList";
			DummyWithList.Z0_Code = DummyWithList.DummyList[1].Code;
			ListBox.Bind(DummyWithList);
			AssertEquals("Pre-condition", DummyWithList, ListBox.BusinessEntity);
			Assert("Pre-condition", ListBox.Items.Count > 0);

			ListBox.UnBind();
			AssertNull("Should be set to null", ListBox.BusinessEntity);
			AssertEquals("Item list should be cleared", 0, ListBox.Items.Count);
		}
	}
}
