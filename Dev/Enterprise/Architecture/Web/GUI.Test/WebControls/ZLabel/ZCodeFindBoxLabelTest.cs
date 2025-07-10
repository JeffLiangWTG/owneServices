using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZCodeFindBoxLabelTest : ZFindBoxLabelTest
	{
		protected override ZLabelBase GetNewLabel()
		{
			return new ZCodeFindBoxLabelForTest();
		}

		protected override string BindToProperty
		{
			get { return "Z0_Code"; }
		}

		#region Overrides

		public void TestNonExistentCodeReturnsEmptyString()
		{
			TestBizO.Z0_FK_Code = "XXX";

			ZLabelTestO.BindTo = "Z0_FK_Code";
			ZLabelTestO.Bind(TestBizO);
			AssertEquals("", ZLabelTestO.Text);
		}

		public override void TestBindToStringProperty()
		{
			DummyChildBusinessObject child = TestBizO.Collection.AddNew();
			child.Z0_Code = "XXX";
			child.Z0_Description = "Description1";

			TestBizO.Z0_Code = child.Z0_Code;

			ZLabelTestO.BindTo = BindToProperty;
			ZLabelTestO.Bind(TestBizO);
			AssertEquals(child.Z0_Code, ZLabelTestO.Text);
		}

		public override void TestReBindModifiedText()
		{
			DummyChildBusinessObject child = TestBizO.Collection.AddNew();
			child.Z0_Code = "XXX";
			child.Z0_Description = "Description1";

			TestBizO.Z0_Code = child.Z0_Code;

			ZLabelTestO.BindTo = BindToProperty;
			ZLabelTestO.Bind(TestBizO);
			AssertEquals(child.Z0_Code, ZLabelTestO.Text);

			string testString = "Modified Description Text";
			Assert(!child.Z0_Code.Equals(testString));
			ZLabelTestO.Text = testString;
			ZLabelTestO.Bind(TestBizO);
			AssertEquals(child.Z0_Code, ZLabelTestO.Text);
		}

		public override void TestUnBind()
		{
			DummyChildBusinessObject child = TestBizO.Collection.AddNew();
			child.Z0_Code = "XXX";
			child.Z0_Description = "Description";

			TestBizO.Z0_Code = child.Z0_Code;
			ZLabelTestO.BindTo = BindToProperty;
			ZLabelTestO.Bind(TestBizO);
			AssertEquals(child.Z0_Code, ZLabelTestO.Text);

			ZLabelTestO.UnBind();
			AssertEquals("", ZLabelTestO.Text);
			AssertEquals(null, ZLabelTestO.BindTo);
		}

		#endregion Overrides

		public void TestGetDescriptionFromKey()
		{
			DummyChildBusinessObject child1 = TestBizO.Collection.AddNew();
			child1.Z0_Code = "XXX";
			child1.Z0_Description = "Description1";

			DummyChildBusinessObject child2 = TestBizO.Collection.AddNew();
			child2.Z0_Code = "ZZZ";
			child2.Z0_Description = "Description2";

			AssertEquals("Expected Description1", "Description1", ((ZCodeFindBoxLabelForTest)ZLabelTestO).GetDescriptionForTesting(TestBizO.Collection, new ZString("XXX")));
		}

		public void TestCodeFromKey()
		{
			DummyChildBusinessObject child1 = TestBizO.Collection.AddNew();
			child1.Z0_Code = "XXX";
			child1.Z0_Description = "Description1";

			DummyChildBusinessObject child2 = TestBizO.Collection.AddNew();
			child2.Z0_Code = "ZZZ";
			child2.Z0_Description = "Description2";

			AssertEquals("Expected Description1", "XXX", ((ZCodeFindBoxLabelForTest)ZLabelTestO).GetCodeForTesting(TestBizO.Collection, new ZString("XXX")));
		}

		class ZCodeFindBoxLabelForTest : ZCodeFindBoxLabel
		{
			#region Test Properties

			public string GetCodeForTesting(object list, IZType value) => GetCode(list, value);
			public string GetDescriptionForTesting(object list, IZType value) => GetDescription(list, value);

			#endregion
		}
	}
}
