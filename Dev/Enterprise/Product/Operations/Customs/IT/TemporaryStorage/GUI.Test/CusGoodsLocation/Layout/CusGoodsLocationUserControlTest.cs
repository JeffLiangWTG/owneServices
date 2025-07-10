using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI.Testing;

sealed class CusGoodsLocationUserControlTest : TestCase
{
	public void TestAdditionalIdentifierDropEdit()
	{
		using var control = new CusGoodsLocationUserControl();
		var additionalIdentifierDropEdit = control.AdditionalIdentifierDropEdit;
		CombineAssertions(() =>
		{
			AssertType<ZDropEdit>("Type", additionalIdentifierDropEdit);
			AssertEquals("BindTo", nameof(CusGoodsLocation.AdditionalIdentifier), additionalIdentifierDropEdit.BindTo);
			AssertEquals("UseFullWidthForCodeBox", true, additionalIdentifierDropEdit.UseFullWidthForCodeBox);
		});
	}
}
