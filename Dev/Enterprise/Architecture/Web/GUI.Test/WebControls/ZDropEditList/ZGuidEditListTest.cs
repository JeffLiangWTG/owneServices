using System;
using System.Web.UI;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZGuidEditListTest : WebControlTest
	{
		#region Implementation 

		string TestCode
		{
			get { return "AAA"; }
		}

		protected override Control GetNewControl()
		{
			return new ZGuidEditList();
		}

		ZGuidEditList GuidEditList
		{
			get { return (ZGuidEditList)Control; }
		}

		object DataSource
		{
			get
			{
				if (fDataSource == null)
				{
					fDataSource = Factory.New<DummyBusinessObjectWithCodeDescriptionAttribute>();
				}
				return fDataSource;
			}
		}
		object fDataSource;

		DummyBusinessObject TestDummyObject
		{
			get { return DataSource as DummyBusinessObjectWithCodeDescriptionAttribute; }
		}

		void PopulateTestObject()
		{
			DummyChildBusinessObject newBizO;
			newBizO = TestBizO.Collection.AddNew();
			newBizO.Z0_Code = "AAA";
			newBizO = TestBizO.Collection.AddNew();
			newBizO.Z0_Code = "BBB";
			newBizO = TestBizO.Collection.AddNew();
			newBizO.Z0_Code = "CCC";
			newBizO = TestBizO.Collection.AddNew();
			newBizO.Z0_Code = "DDD";
			newBizO = TestBizO.Collection.AddNew();
			newBizO.Z0_Code = "EEE";
		}

		protected override void SetUp()
		{
			base.SetUp();
			PopulateTestObject();
			GuidEditList.BindTo = DummyBusinessObject.Schema.Z0_Guid;
			GuidEditList.BindToList = "Collection";
			GuidEditList.ShowDescription = false;
			GuidEditList.TextBoxControl.Text = TestCode;
		}

		#endregion Implementation

		public void TestSelectedValue()
		{
			Page.Controls.Add(GuidEditList);
			GuidEditList.Bind(TestDummyObject);
			AssertEquals(null, GuidEditList.SelectedValue);
			AssertEquals("", GuidEditList.TextBoxControl.Text);
		}

		public void TestSetSelectedValueUpdate()
		{
			Page.Controls.Add(GuidEditList);
			GuidEditList.Bind(TestDummyObject);
			Assert(TestDummyObject.Collection.Count > 1);
			AssertEquals(null, GuidEditList.SelectedValue);
			GuidEditList.SelectedValue = TestDummyObject.Collection[1].PK;
			AssertEquals(TestDummyObject.Collection[1].Z0_Code, GuidEditList.TextBoxControl.Text);
			AssertEquals(TestDummyObject.Collection[1].PK, GuidEditList.SelectedValue);
		}

		public void TestDropDownList()
		{
			Page.Controls.Add(GuidEditList);

			GuidEditList.Bind(TestDummyObject);
			AssertEquals(TestDummyObject.Collection.Count, GuidEditList.ListBoxControl.Items.Count);
		}

		public void TestFilter()
		{
			string expectedTextADO = String.Format("{0} = '{1}'", DummyBusinessObject.Schema.Z0_Code, TestCode);
			AssertEquals(expectedTextADO, GuidEditList.GetFilter(typeof(DummyBusinessObject)).LiteralTextADO);
		}
	}
}
