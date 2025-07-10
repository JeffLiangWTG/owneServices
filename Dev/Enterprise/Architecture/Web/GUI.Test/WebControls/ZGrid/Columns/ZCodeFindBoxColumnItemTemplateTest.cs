using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZCodeFindBoxColumnItemTemplateTest : ZItemTemplateTest
	{
		public void TestGetControl()
		{
			ZCodeFindBoxColumn testZCodeFindBoxColumnWithoutList = new ZCodeFindBoxColumn("Part#", "JO_Partno");
			AssertEquals("BindToList is set, but need not set", "", testZCodeFindBoxColumnWithoutList.BindToList);
			ZCodeFindBoxColumnItemTemplate test = new ZCodeFindBoxColumnItemTemplate(testZCodeFindBoxColumnWithoutList);
			AssertEquals("Type of ZCodeFindBox is not a ZTextLabel", typeof(ZTextLabel), test.GetControl().GetType());

			ZCodeFindBoxColumn testZCodeFindBoxColumnWithList = new ZCodeFindBoxColumn("Part#", "JO_Partno", "JO_PartNo_List");
			AssertEquals("BindToList should be set by constructor", "JO_PartNo_List", testZCodeFindBoxColumnWithList.BindToList);
			ZCodeFindBoxColumnItemTemplate test2 = new ZCodeFindBoxColumnItemTemplate(testZCodeFindBoxColumnWithList);
			AssertEquals("Type of ZCodeFindBox is not a ZCodeFindBoxLabel", typeof(ZCodeFindBoxLabel), test2.GetControl().GetType());
		}

		#region Implementation

		protected override Type ExpectedItemTemplateType
		{
			get { return typeof(ZCodeFindBoxColumnItemTemplate); }
		}

		protected override Type ExpectedColumnType
		{
			get { return typeof(ZCodeFindBoxColumn); }
		}

		public new ZCodeFindBoxColumnItemTemplate TestItemTemplate
		{
			get { return base.TestItemTemplate as ZCodeFindBoxColumnItemTemplate; }
		}

		public new ZCodeFindBoxColumn TestColumn
		{
			get { return base.TestColumn as ZCodeFindBoxColumn; }
		}

		#endregion Implementation
	}
}
