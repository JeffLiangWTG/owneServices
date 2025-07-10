using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(NctsTariffColumnStyle))]
class NctsTariffColumnStyleTest : TestCase
{
	public void TestEditControl() => CombineAssertions(() =>
	{
		var columnStyleInfo = new NctsTariffColumnStyleInfo();
		columnStyleInfo.GetCountryCode = () => "XX";
		columnStyleInfo.PartialDescriptionMinLengthForSearch = 4;
		columnStyleInfo.GetEffectiveDate = () => new ZDateTime(2023, 1, 15);
		columnStyleInfo.GetSelectNomenclatureModes = () => new List<SelectionStyle>();
		columnStyleInfo.ShowDescriptionFilterOnNonNomenclatureTariffModule = true;
		columnStyleInfo.NeedLoadParentDataGroup = true;

		using (var columnStyle = new NctsTariffColumnStyle(columnStyleInfo))
		{
			var findBox = columnStyle.EditControl as NctsTariffGridFindBox;
			AssertNotNull("NctsTariffGridFindBox", findBox);
			AssertSame("GetCountryCode", columnStyleInfo.GetCountryCode, findBox.GetCountryCode);
			AssertEquals("PartialDescriptionMinLengthForSearch", 4, findBox.PartialDescriptionMinLengthForSearch);
			AssertSame("GetEffectiveDate", columnStyleInfo.GetEffectiveDate, findBox.GetEffectiveDate);
			AssertSame("GetSelectNomenclatureModes", columnStyleInfo.GetSelectNomenclatureModes, findBox.GetSelectNomenclatureModes);
			AssertEquals("ShowDescriptionFilterOnNonNomenclatureTariffModule", true, findBox.ShowDescriptionFilterOnNonNomenclatureTariffModule);
			AssertEquals("NeedLoadParentDataGroup", true, findBox.NeedLoadParentDataGroup);
		}
	});
}
