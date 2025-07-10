using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZGuidFindBoxColumnItemTemplateTest : ZItemTemplateTest
	{
		public void TestGetControl()
		{
			var testZGuidFindBoxColumnWithoutList = new ZGuidFindBoxColumn("Part#", "JO_Partno");
			AssertEquals("BindToList is set, but need not set", "", testZGuidFindBoxColumnWithoutList.BindToList);

			var test = new ZGuidFindBoxColumnItemTemplate(testZGuidFindBoxColumnWithoutList);
			AssertEquals("Type of ZCodeFindBox is not a ZTextLabel", typeof(ZTextLabel), test.GetControl().GetType());

			var testZGuidFindBoxColumnWithList = new ZGuidFindBoxColumn("Part#", "JO_Partno", "JO_PartNo_List");
			AssertEquals("BindToList should be set by constructor", "JO_PartNo_List", testZGuidFindBoxColumnWithList.BindToList);

			var test2 = new ZGuidFindBoxColumnItemTemplate(testZGuidFindBoxColumnWithList);
			AssertEquals("Type of ZGuidFindBox is not a ZGuidFindBoxLabel", typeof(ZGuidFindBoxLabel), test2.GetControl().GetType());
		}

		#region Implementation

		protected override Type ExpectedItemTemplateType
		{
			get { return typeof(ZGuidFindBoxColumnItemTemplate); }
		}

		protected override Type ExpectedColumnType
		{
			get { return typeof(ZGuidFindBoxColumn); }
		}

		public new ZGuidFindBoxColumnItemTemplate TestItemTemplate
		{
			get { return base.TestItemTemplate as ZGuidFindBoxColumnItemTemplate; }
		}

		public new ZGuidFindBoxColumn TestColumn
		{
			get { return base.TestColumn as ZGuidFindBoxColumn; }
		}

		#endregion Implementation
	}
}
