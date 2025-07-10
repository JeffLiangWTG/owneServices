using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZCodeLookupLabelTest : ZLookupLabelBaseTest
	{
		protected override ZLabelBase GetNewLabel()
		{
			return new ZCodeLookupLabelForTest();
		}

		public void TestGetDescription()
		{
			TestBizO.Lookups.DummyCodeList.AddPair("XXX", "Description1");
			TestBizO.Lookups.DummyCodeList.AddPair("YYY", "Description2");

			AssertEquals("Expected Description1", "Description1", ((ZCodeLookupLabelForTest)ZLabelTestO).GetDescriptionForTesting(TestBizO.Lookups.DummyCodeList, new ZString("XXX")));
			AssertEquals("Expected Description1", "Description2", ((ZCodeLookupLabelForTest)ZLabelTestO).GetDescriptionForTesting(TestBizO.Lookups.DummyCodeList, new ZString("YYY")));
		}

		public void TestCode()
		{
			TestBizO.Lookups.DummyCodeList.AddPair("XXX", "Description1");
			TestBizO.Lookups.DummyCodeList.AddPair("YYY", "Description2");

			AssertEquals("Expected Description1", "XXX", ((ZCodeLookupLabelForTest)ZLabelTestO).GetCodeForTesting(TestBizO.Lookups.DummyCodeList, new ZString("XXX")));
			AssertEquals("Expected Description1", "YYY", ((ZCodeLookupLabelForTest)ZLabelTestO).GetCodeForTesting(TestBizO.Lookups.DummyCodeList, new ZString("YYY")));
		}

		protected override string BindToListPropertyName
		{
			get { return "Lookups.DummyCodeList"; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestBizO.Z0_Description = TestBizO.Lookups.DummyCodeList[0].Code;
		}

		class ZCodeLookupLabelForTest : ZCodeLookupLabel
		{
			#region Test Properties

			public string GetCodeForTesting(object list, IZType value) => GetCode(list, value);
			public string GetDescriptionForTesting(object list, IZType value) => GetDescription(list, value);

			#endregion
		}
	}
}
