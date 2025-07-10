using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.BE.GUI.Testing
{
	sealed class EntryInstructionGuaranteeGridColumnsBagTest : TestCase
	{
		public void TestBondTypeDropEditColumnCaption()
		{
			var columnInfo = ColumnsBag.BondTypeDropEditColumn.CreateGridColumnInfo();

			AssertEquals("CaptionResourceString", NoResourceStringData.GetData("[UCC 8/2] Type"), columnInfo.CaptionResourceString);
		}

		public void TestBondNumberDropEditColumnCaption()
		{
			var columnInfo = ColumnsBag.BondNumberMultiControlColumn.CreateGridColumnInfo() as ZMultiControlColumnStyleInfo;

			AssertEquals("CaptionResourceString", NoResourceStringData.GetData("[UCC 8/3] Reference"), columnInfo.CaptionResourceString);
		}

		EntryInstructionGuaranteeGridColumnsBag ColumnsBag => EntryInstructionGuaranteeGridColumnsBag.Instance;
	}
}
