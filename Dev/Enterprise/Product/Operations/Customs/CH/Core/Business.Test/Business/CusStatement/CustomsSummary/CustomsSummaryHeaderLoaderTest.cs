using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CustomsSummaryHeaderLoader))]
sealed class CustomsSummaryHeaderLoaderTest : LoaderTestCase
{
	public void TestLoadBordereau() => CombineAssertions(() =>
	{
		var company2 = Factory.New<GlbCompany>();

		var header = Factory.New<CustomsSummaryHeader>();
		header.B2_StatementNumber = "B100";
		header.B2_ProcessDate = new ZDate(2020, 1, 1);
		header.B2_GC = GlbCompany.CurrentCompany.PK;

		var header2 = Factory.New<CustomsSummaryHeader>();
		header2.B2_StatementNumber = "B200";
		header2.B2_ProcessDate = new ZDate(2020, 1, 1);
		header2.B2_GC = company2.PK;

		Factory.Save();

		var loader = new CustomsSummaryHeaderLoader(Factory);
		AssertEquals(" Match", header.PK, loader.LoadBorderau(header.B2_StatementNumber, header.B2_ProcessDate.Date)?.PK);
		AssertNull("Other number", loader.LoadBorderau(header.B2_StatementNumber + "X", header.B2_ProcessDate.Date));
		AssertNull("Other date", loader.LoadBorderau(header.B2_StatementNumber, header.B2_ProcessDate.Date.AddDays(1)));
		AssertNull("Other company", loader.LoadBorderau(header2.B2_StatementNumber, header2.B2_ProcessDate.Date));
	});

	protected override BusinessObject.Loader GetNewLoaderToTest() => new CustomsSummaryHeaderLoader(Factory);
}
