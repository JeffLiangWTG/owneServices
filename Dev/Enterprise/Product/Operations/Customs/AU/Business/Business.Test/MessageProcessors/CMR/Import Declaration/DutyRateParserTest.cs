using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class DutyRateParserTest : TestCaseWithFactory
	{
		public void TestParseAndGetResultForPercentageOnly()
		{
			DutyResult result = DutyRateParser.ParseAndGetResult(Factory, "5%");
			AssertEquals("Result.Percent", 5m, result.Percent);
			AssertEquals("Result.FlatAmount", 0m, result.FlatRateAmount);
			AssertEquals("Result.FlatUQ", "", result.FlatRateUQ);
		}

		public void TestParseAndGetResultForCompositeRate()
		{
			CMRCodeLists codeForLA = CMRCodeLists.New(Factory);
			codeForLA.CI_Code = "LA";
			codeForLA.CI_CodeType = CMRCodeLists.CodeTypes.QUANUNIT;
			codeForLA.CI_Name = "LITRE ALCOHOL";

			DutyResult result = DutyRateParser.ParseAndGetResult(Factory, "5% ?+ $61.71/LITRE ALCOHOL");
			AssertEquals("Result.Percent", 5m, result.Percent);
			AssertEquals("Result.FlatAmount", 61.71m, result.FlatRateAmount);
			AssertEquals("Result.FlatUQ", "LA", result.FlatRateUQ);
		}

		public void TestParseForTheFlatRateOnly()
		{
			CMRCodeLists codeForLA = CMRCodeLists.New(Factory);
			codeForLA.CI_Code = "LA";
			codeForLA.CI_CodeType = CMRCodeLists.CodeTypes.QUANUNIT;
			codeForLA.CI_Name = "LITRE ALCOHOL";

			DutyResult result = DutyRateParser.ParseAndGetResult(Factory, "$61.71/LITRE ALCOHOL");
			AssertEquals("Result.Percent", 0m, result.Percent);
			AssertEquals("Result.FlatAmount", 61.71m, result.FlatRateAmount);
			AssertEquals("Result.FlatUQ", "LA", result.FlatRateUQ);
		}

		public void TestParseAndGetResultForFree()
		{
			DutyResult result = DutyRateParser.ParseAndGetResult(Factory, "FREE");
			AssertEquals("Result.Percent", 0m, result.Percent);
			AssertEquals("Result.FlatAmount", 0m, result.FlatRateAmount);
			AssertEquals("Result.FlatUQ", "", result.FlatRateUQ);
		}

		public void TestParseForTruncatedCompositeRate()
		{
			var uqQuery = new ZQuery(CMRCodeListsSchema.CI_Code, "LA");
			uqQuery.AddToFilter(CMRCodeListsSchema.CI_CodeType, CMRCodeLists.CodeTypes.QUANUNIT);
			var codeForLA = Factory.LoadTop1<CMRCodeLists>(uqQuery);
			if (codeForLA == null)
			{
				CreateQuantityCode("LA", "LITRE ALCOHOL");
			}

			uqQuery = new ZQuery(CMRCodeListsSchema.CI_Code, "L");
			uqQuery.AddToFilter(CMRCodeListsSchema.CI_CodeType, CMRCodeLists.CodeTypes.QUANUNIT);
			var codeForL = Factory.LoadTop1<CMRCodeLists>(uqQuery);
			if (codeForL == null)
			{
				CreateQuantityCode("L", "LITRE");
			}

			var result = DutyRateParser.ParseAndGetResult(Factory, "5% ?+ $100.05/LITRE ALCOHO");
			AssertEquals("Result.Percent", 5m, result.Percent);
			AssertEquals("Result.FlatAmount", 100.05m, result.FlatRateAmount);
			AssertEquals("Result.FlatUQ", "LA", result.FlatRateUQ);
			AssertEquals("Result.FlatUQ", "5.00%+100.05/LA", result.DutyRateDescription);

			uqQuery = new ZQuery(CMRCodeListsSchema.CI_Code, "CC");
			uqQuery.AddToFilter(CMRCodeListsSchema.CI_CodeType, CMRCodeLists.CodeTypes.QUANUNIT);
			var codeForCC = Factory.LoadTop1<CMRCodeLists>(uqQuery);
			if (codeForCC == null)
			{
				CreateQuantityCode("CC", "CUBIC CENTIMETRE");
			}

			result = DutyRateParser.ParseAndGetResult(Factory, "2% ?+ $0.07c/CUBIC CENTIME");
			AssertEquals("Result.Percent", 2m, result.Percent);
			AssertEquals("Result.FlatAmount", 0.07m, result.FlatRateAmount);
			AssertEquals("Result.FlatUQ", "CC", result.FlatRateUQ);
			AssertEquals("Result.FlatUQ", "2.00%+0.07/CC", result.DutyRateDescription);
		}

		public void TestParseForTruncatedCompositeRate_ExactMatchFound()
		{
			var codeForLA = CMRCodeLists.New(Factory);
			codeForLA.CI_Code = "LA";
			codeForLA.CI_CodeType = CMRCodeLists.CodeTypes.QUANUNIT;
			codeForLA.CI_Name = "LITRE ALCOHOL";

			var codeForL = CMRCodeLists.New(Factory);
			codeForL.CI_Code = "L";
			codeForL.CI_CodeType = CMRCodeLists.CodeTypes.QUANUNIT;
			codeForL.CI_Name = "LITRE";

			var result = DutyRateParser.ParseAndGetResult(Factory, "5% ?+ $61.71/LITRE ALCOHOL");
			AssertEquals("Result.Percent", 5m, result.Percent);
			AssertEquals("Result.FlatAmount", 61.71m, result.FlatRateAmount);
			AssertEquals("Result.FlatUQ", "LA", result.FlatRateUQ);
			AssertEquals("Result.FlatUQ", "5.00%+61.71/LA", result.DutyRateDescription);
		}

		public void TestParseForTruncatedCompositeRate_PartialMatchFound()
		{
			var uqQuery = new ZQuery(CMRCodeListsSchema.CI_Code, "LA");
			uqQuery.AddToFilter(CMRCodeListsSchema.CI_CodeType, CMRCodeLists.CodeTypes.QUANUNIT);
			var codeForLA = Factory.LoadTop1<CMRCodeLists>(uqQuery);
			if (codeForLA == null)
			{
				CreateQuantityCode("LA", "LITRE ALCOHOL");
			}

			uqQuery = new ZQuery(CMRCodeListsSchema.CI_Code, "L");
			uqQuery.AddToFilter(CMRCodeListsSchema.CI_CodeType, CMRCodeLists.CodeTypes.QUANUNIT);
			var codeForL = Factory.LoadTop1<CMRCodeLists>(uqQuery);
			if (codeForL == null)
			{
				CreateQuantityCode("L", "LITRE");
			}

			var result = DutyRateParser.ParseAndGetResult(Factory, "5% ?+ $100.05/LITRE ALCOHO");
			AssertEquals("Result.Percent", 5m, result.Percent);
			AssertEquals("Result.FlatAmount", 100.05m, result.FlatRateAmount);
			AssertEquals("Result.FlatUQ", "LA", result.FlatRateUQ);
			AssertEquals("Result.FlatUQ", "5.00%+100.05/LA", result.DutyRateDescription);
		}

		public void TestParseForTruncatedCompositeRate_MultiplePartialMatchesFound()
		{
			var codeForPD = CMRCodeLists.New(Factory);
			codeForPD.CI_Code = "PD";
			codeForPD.CI_CodeType = CMRCodeLists.CodeTypes.QUANUNIT;
			codeForPD.CI_Name = "DOZEN PACKS";

			var codeForDP = CMRCodeLists.New(Factory);
			codeForDP.CI_Code = "DP";
			codeForDP.CI_CodeType = CMRCodeLists.CodeTypes.QUANUNIT;
			codeForDP.CI_Name = "DOZEN PAIR";

			ExceptionReporterTestListener.Instance.Clear();
			try
			{
				var result = DutyRateParser.ParseAndGetResult(Factory, "5% ?+ $61.71/DOZEN PA");
				AssertEquals(
					"Customs have truncated the duty per quantity UQ code and multiple potential matches have been found: 5% ?+ $61.71/DOZEN PA",
					ExceptionReporterTestListener.Instance[0].Message);
				AssertEquals("Result.Percent", 5m, result.Percent);
				AssertEquals("Result.FlatAmount", 61.71m, result.FlatRateAmount);
				AssertEquals("Result.FlatUQ", "PD", result.FlatRateUQ);
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		void CreateQuantityCode(string uqCode, string uqDesc)
		{
			var codeForUQ = CMRCodeLists.New(Factory);
			CMRCodeLists.New(Factory);
			codeForUQ.CI_Code = uqCode;
			codeForUQ.CI_CodeType = CMRCodeLists.CodeTypes.QUANUNIT;
			codeForUQ.CI_Name = uqDesc;
		}
	}
}
