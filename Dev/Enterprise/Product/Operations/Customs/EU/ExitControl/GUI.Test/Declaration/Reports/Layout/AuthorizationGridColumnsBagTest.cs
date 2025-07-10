using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing;

sealed class AuthorizationGridColumnsBagTest : TestCase
{
	public void TestAGC_CodeDropEditColumn()
	{
		AssertNotNull(ColumnsBag.AGC_CodeDropEditColumn);

		var columnInfo = ColumnsBag.AGC_CodeDropEditColumn.CreateGridColumnInfo();
		CombineAssertions("AGC_CodeDropEditColumn Configuration", () =>
		{
			AssertType<ZDropEditColumnStyleInfo>(columnInfo);
			AssertEquals("ColumnName", "AGC_Code", columnInfo.ColumnName);
		});
	}

	public void TestCustomsCodeTextBoxColumn()
	{
		AssertNotNull(ColumnsBag.CustomsCodeTextBoxColumn);

		var columnInfo = ColumnsBag.CustomsCodeTextBoxColumn.CreateGridColumnInfo();
		CombineAssertions("CustomsCodeTextBoxColumn Configuration", () =>
		{
			AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
			AssertEquals("ColumnName", "CustomsCode", columnInfo.ColumnName);
		});
	}

	public void TestEffectiveReferenceNumberCodeFindBoxColumn()
	{
		AssertNotNull(ColumnsBag.EffectiveReferenceNumberCodeFindBoxColumn);

		var columnInfo = ColumnsBag.EffectiveReferenceNumberCodeFindBoxColumn.CreateGridColumnInfo();
		CombineAssertions("EffectiveReferenceNumberCodeFindBoxColumn Configuration", () =>
		{
			AssertType<ZCodeFindBoxColumnStyleInfo>(columnInfo);
			AssertEquals("ColumnName", "EffectiveReferenceNumber", columnInfo.ColumnName);
		});
	}

	public void TestAGC_OH_OwnerOrganisationFindBoxColumn()
	{
		AssertNotNull(ColumnsBag.AGC_OH_OwnerOrganisationFindBoxColumn);

		var columnInfo = ColumnsBag.AGC_OH_OwnerOrganisationFindBoxColumn.CreateGridColumnInfo();
		CombineAssertions("AGC_OH_OwnerOrganisationFindBoxColumn Configuration", () =>
		{
			AssertType<ZOrganisationFindBoxColumnStyleInfo>(columnInfo);
			AssertEquals("ColumnName", "AGC_OH_Owner", columnInfo.ColumnName);
		});
	}

	AuthorizationGridColumnsBag ColumnsBag => AuthorizationGridColumnsBag.Instance;
}
