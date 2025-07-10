using System.Linq;
using CargoWise.Integration;
using Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting;

namespace Enterprise.Customs.FR.Business.Reports.Testing
{
	partial class ProcedureCodesCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		public override void TestIsReturningCorrectCollection()
		{
			var expectedProcedureCodes = new[] { "00", "40", "71P", "10", "23P" };

			var actualProcedureCodes = CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();

			CombineAssertions(() =>
			{
				foreach (var expectedProcedureCode in expectedProcedureCodes)
				{
					AssertCollectionContains("All Procedure Codes Collection", expectedProcedureCode, actualProcedureCodes.GetAllCodes());
				}
				AssertEquals("No previous procedure code, number of occurences", 1, actualProcedureCodes.Cast<ICodeDescription>().Count(x => x.Code == "00"));
			});
		}

		protected override DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider() => new ProcedureCodesCodeDescriptionPairProvider();
	}
}
