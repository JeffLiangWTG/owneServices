using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.NCTS.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

class NctsTariffFindBoxHelperTest : TestCaseWithFactory
{
	public void TestInitializeFindBox() => CombineAssertions(() =>
	{
		var findBox = new FindBoxForTesting();
		NctsTariffFindBoxHelper.InitializeFindBox(findBox);
		AssertNotNull("GetDataGrouping", findBox.GetDataGrouping);
		AssertNotNull("GetTariffType", findBox.GetTariffType);
	});

	public void TestGetFindBoxListProvider() => CombineAssertions(() =>
	{
		new RefDataTestHelper(Factory).CreateTariffsForTransit();

		AssertListProvider("04069099001", "cheese");
		AssertListProvider("710121", "natural pearls unworked");

		void AssertListProvider(string code, string expectedDescription)
		{
			var findBox = new FindBoxForTesting();
			findBox.Code = code;
			NctsTariffFindBoxHelper.InitializeFindBox(findBox);
			AssertEquals($"Code={findBox.Code}", expectedDescription, NctsTariffFindBoxHelper.GetFindBoxListProvider(findBox).DescriptionFromCode(findBox.Code));
		}
	});

	class FindBoxForTesting : INctsTariffFindBox
	{
		public Func<ZString> GetDataGrouping { get; set; }

		public Func<ZString> GetTariffType { get; set; }

		public string Code { get; set; }

		public Func<bool> GetShouldShowExactDescription => null;

		public ITariffFormatter GetTariffFormatter() => new TariffFormatterCH();

		public Func<ZDateTime> GetEffectiveDate => () => ZDateTime.Today;

		public bool ShowDescriptionFilterOnNonNomenclatureTariffModule => throw new NotImplementedException();

		public string Description { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public IFindBoxListProvider ListProvider => throw new NotImplementedException();

		public IFindBoxPopup PopupForm => throw new NotImplementedException();
	}
}
