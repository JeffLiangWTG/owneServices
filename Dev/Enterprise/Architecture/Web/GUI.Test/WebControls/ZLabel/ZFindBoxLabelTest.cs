using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZFindBoxLabelTest : ZFindBoxLabelBaseTest
	{
		protected override ZLabelBase GetNewLabel()
		{
			return new ZFindBoxLabel();
		}

		public override void TestBindToStringProperty()
		{
			Assert("This test is not relevant to FindBoxLabel because it binds to ZGuids, not strings.", true);
		}

		public override void TestBindToEmptyStringProperty()
		{
			Assert("This test is not relevant to FindBoxLabel because it binds to ZGuids, not strings.", true);
		}

		public override void TestReBindModifiedText()
		{
			DummyChildBusinessObject child = TestBizO.Collection.AddNew();
			child.Z0_Code = "XXX";
			child.Z0_Description = "Description1";

			TestBizO.Z0_Guid = child.PK;

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

			TestBizO.Z0_Guid = child.PK;
			ZLabelTestO.BindTo = BindToProperty;
			ZLabelTestO.Bind(TestBizO);
			AssertEquals(child.Z0_Code, ZLabelTestO.Text);

			ZLabelTestO.UnBind();
			AssertEquals("", ZLabelTestO.Text);
			AssertEquals(null, ZLabelTestO.BindTo);
		}

		protected virtual string BindToProperty
		{
			get { return "Z0_Guid"; }
		}
	}
}
