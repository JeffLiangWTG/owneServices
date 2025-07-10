using System.Windows.Forms;
using Enterprise.Customs.IL.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IL.GUI.Testing
{
	sealed class PreviousDocumentsGridColumnBagTest : TestCase
	{
		public void TestCodeDropEditColumn()
		{
			var columnInfo = ColumnsBag.CodeCodeFindBoxColumn?.CreateGridColumnInfo();
			CombineAssertions("ZDropEditColumnStyleInfo Configuration", () =>
			{
				AssertNotNull(ColumnsBag.CodeCodeFindBoxColumn);
				AssertType<ZCodeFindBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", PreviousDocument.Schema.CSI_Code, columnInfo.ColumnName);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			});
		}

		public void TestReferenceNumberColumn()
		{
			var columnInfo = ColumnsBag.ReferenceNumberColumn?.CreateGridColumnInfo();

			CombineAssertions("ZTextBoxColumnStyleInfo Configuration", () =>
			{
				AssertNotNull(ColumnsBag.ReferenceNumberColumn);
				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", PreviousDocument.Schema.CSI_ReferenceNumber, columnInfo.ColumnName);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			});
		}

		PreviousDocumentsGridColumnBag ColumnsBag => PreviousDocumentsGridColumnBag.Instance;
	}
}
