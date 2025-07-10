namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZGuidFindBoxLabelTest : ZFindBoxLabelTest
	{
		protected override ZLabelBase GetNewLabel()
		{
			return new ZGuidFindBoxLabel();
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
			var child = TestBizO.Collection.AddNew();
			child.Z0_Code = "XXX";
			child.Z0_Description = "Description1";

			TestBizO.Z0_Code = child.Z0_Code;

			ZLabelTestO.BindTo = BindToProperty;
			ZLabelTestO.Bind(TestBizO);
			AssertEquals(child.Z0_Code, ZLabelTestO.Text);
		}

		public override void TestReBindModifiedText()
		{
			var child = TestBizO.Collection.AddNew();
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
			var child = TestBizO.Collection.AddNew();
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
	}
}
