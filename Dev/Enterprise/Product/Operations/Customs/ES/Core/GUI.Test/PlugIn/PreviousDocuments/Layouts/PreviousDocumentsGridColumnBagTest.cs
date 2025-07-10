using System.Windows.Forms;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing;

public sealed class PreviousDocumentsGridColumnBagTest : TestCase
{
	public void TestCodeDropEditColumn()
	{
		var columnInfo = ColumnsBag.CodeDropEditColumn?.CreateGridColumnInfo();
		CombineAssertions("ZDropEditColumnStyleInfo Configuration", () =>
		{
			AssertNotNull(ColumnsBag.CodeDropEditColumn);
			AssertType<ZDropEditColumnStyleInfo>(columnInfo);
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

	public void TestCountryCodeColumn()
	{
		var columnInfo = ColumnsBag.CountryCodeColumn?.CreateGridColumnInfo();
		CombineAssertions("ZCodeFindBoxColumnStyleInfo Configuration", () =>
		{
			AssertNotNull(ColumnsBag.CountryCodeColumn);
			AssertType<ZCodeFindBoxColumnStyleInfo>(columnInfo);
			AssertEquals("ColumnName", PreviousDocument.Schema.CSI_RN_NKCountryCode, columnInfo.ColumnName);
			AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
		});
	}

	PreviousDocumentsGridColumnBag ColumnsBag => PreviousDocumentsGridColumnBag.Instance;
}
