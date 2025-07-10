using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class RegexProviderTest : TestCase
	{
		public void TestMacroRegexDouble()
		{
			var sourceString = "select * from dbo.glbcompany where gc_code = '<<IF(<<CompanyCode>>==\"EDI\",\"<<Test1>>\",\"<<Test2>>\")>>'";
			var matches1 = RegexProvider.MacroRegexDouble.Matches(sourceString);
			AssertEquals(3, matches1.Count);
			AssertEquals("CompanyCode", matches1[0].Groups[1].Value);
			AssertEquals("Test1", matches1[1].Groups[1].Value);
			AssertEquals("Test2", matches1[2].Groups[1].Value);

			sourceString = "select * from dbo.glbcompany where gc_code = '<<IF(1==1,\"AB\\<\",\"CD\\>\")>>'";
			var matches2 = RegexProvider.MacroRegexDouble.Matches(sourceString);
			AssertEquals(1, matches2.Count);
			AssertEquals("IF(1==1,\"AB\\<\",\"CD\\>\")", matches2[0].Groups[1].Value);
		}

		public void TestOutermostMacroRegexAgain()
		{
			MatchCollection matches = RegexProvider.OutermostMacroRegex.Matches("BOTHER <OuterMacro(<InnerMacro>)> BUGGER <AnotherOuterMacro> WHATEVER");
			AssertEquals(2, matches.Count);
			AssertEquals("<OuterMacro(<InnerMacro>)>", matches[0].Value);
			AssertEquals("<AnotherOuterMacro>", matches[1].Value);
		}

		public void TestInnermostMacrosRegex()
		{
			MatchCollection matches = RegexProvider.InnermostMacrosRegex.Matches("<OuterMacro(<InnerMacro>)><AnotherInnerMacro>");
			AssertEquals(2, matches.Count);
			AssertEquals("<InnerMacro>", matches[0].Value);
			AssertEquals("<AnotherInnerMacro>", matches[1].Value);
		}

		public void TestSingleMacroOnlyRegex()
		{
			AssertNoMatch(RegexProvider.SingleMacroOnlyRegex, "<>");
			AssertMatch(RegexProvider.SingleMacroOnlyRegex, "<1>");
			AssertNoMatch(RegexProvider.SingleMacroOnlyRegex, " <we>");
			AssertMatch(RegexProvider.SingleMacroOnlyRegex, "<count we>");
			AssertMatch(RegexProvider.SingleMacroOnlyRegex, "<COUNT we>");
			AssertMatch(RegexProvider.SingleMacroOnlyRegex, "<tot we>");
			AssertMatch(RegexProvider.SingleMacroOnlyRegex, "<Total we.ss>");
			AssertNoMatch(RegexProvider.SingleMacroOnlyRegex, "<Total we.ss> <sadhhj>");
		}

		public void TestTotalMacroRegex()
		{
			AssertNoMatch(RegexProvider.TotalMacroRegex, "<>");
			AssertNoMatch(RegexProvider.TotalMacroRegex, "<TotalPages>");
			AssertMatch(RegexProvider.TotalMacroRegex, "<   Total  \t       Tbl.Col   >");
			AssertMatch(RegexProvider.TotalMacroRegex, "<   Total  \t       Lines   >");
			AssertMatch(RegexProvider.TotalMacroRegex, "<Total  Currency(<lines.dummy>, OMR)>");

			MatchCollection matches = RegexProvider.TotalMacroRegex.Matches("<Total  Currency(<lines.dummy>, OMR)>");
			AssertEquals(1, matches.Count);
			AssertEquals("Should match the whole string", "<Total  Currency(<lines.dummy>, OMR)>", matches[0].Value);
		}

		public void TestTotalMacroRegexMatchToFirstEndingBracket()
		{
			MatchCollection matches = RegexProvider.TotalMacroRegexMatchingToFirstEndingBracket.Matches("<Total Packages.Weight.InFreightWeightUnit.Value> <Packages.Weight.InFreightWeightUnit.Unit.Code>");
			AssertEquals(1, matches.Count);
			AssertEquals("<Total Packages.Weight.InFreightWeightUnit.Value>", matches[0].Value);
		}

		public void TestUserRepositoryMacroRegex()
		{
			AssertNoMatch(RegexProvider.UserRepositoryMacroRegex, "<>");
			AssertNoMatch(RegexProvider.UserRepositoryMacroRegex, "<UserRepositoryKelvin>");
			AssertNoMatch(RegexProvider.UserRepositoryMacroRegex, "<UserRepository Alex>");
			AssertNoMatch(RegexProvider.UserRepositoryMacroRegex, "<Dave UserRepository >");
			AssertMatch(RegexProvider.UserRepositoryMacroRegex, "<  UserRepository   >");
			AssertMatch(RegexProvider.UserRepositoryMacroRegex, "<   uSeRrEpOsItOrY   >");
			AssertMatch(RegexProvider.UserRepositoryMacroRegex, "<UserRepository>");
		}

		public void TestBasicMatchesOnInnermostMacrosRegex()
		{
			AssertNoMatch(RegexProvider.InnermostMacrosRegex, "<>");
			AssertMatch(RegexProvider.InnermostMacrosRegex, "<1>");
			AssertMatch(RegexProvider.InnermostMacrosRegex, " <we>");
			AssertMatch(RegexProvider.InnermostMacrosRegex, "<count we>");
			AssertMatch(RegexProvider.InnermostMacrosRegex, "<COUNT we>");
			AssertMatch(RegexProvider.InnermostMacrosRegex, "<tot we><tot we>");
			AssertMatch(RegexProvider.InnermostMacrosRegex, "<Total we.ss>");
			AssertMatch(RegexProvider.InnermostMacrosRegex, "<Total we.ss> <sadhhj>");
		}

		public void TestMacroRegexMatchGroups()
		{
			MatchCollection matches = RegexProvider.OutermostMacroRegex.Matches("blah <Macro> dsjkldjsf <Macro 2>");
			AssertEquals("<Macro>", matches[0].Value);
			AssertEquals("<Macro 2>", matches[1].Value);
			matches = RegexProvider.OutermostMacroRegex.Matches("<Macro(<Param>)>");
			AssertEquals("<Macro(<Param>)>", matches[0].Value);
		}

		public void TestTableNamePlusColumnNameWithOptionalTotalRegex()
		{
			AssertNoMatch(RegexProvider.TableNamePlusColumnNameWithOptionalTotalRegex, "<>");

			AssertNoMatch(RegexProvider.TableNamePlusColumnNameWithOptionalTotalRegex, "<fish>");
			AssertNoMatch(RegexProvider.TableNamePlusColumnNameWithOptionalTotalRegex, "<are>");
			AssertNoMatch(RegexProvider.TableNamePlusColumnNameWithOptionalTotalRegex, "<total fish>");
			AssertNoMatch(RegexProvider.TableNamePlusColumnNameWithOptionalTotalRegex, "<total are>");

			AssertMatch(RegexProvider.TableNamePlusColumnNameWithOptionalTotalRegex, "<go.fish>");
			AssertMatch(RegexProvider.TableNamePlusColumnNameWithOptionalTotalRegex, "<we.are>");
			AssertMatch(RegexProvider.TableNamePlusColumnNameWithOptionalTotalRegex, "<total go.fish>");
			AssertMatch(RegexProvider.TableNamePlusColumnNameWithOptionalTotalRegex, "<total we.are>");

			AssertNoMatch(RegexProvider.TableNamePlusColumnNameWithOptionalTotalRegex, "<dont.go.fish>");
			AssertNoMatch(RegexProvider.TableNamePlusColumnNameWithOptionalTotalRegex, "<here.we.are>");
			AssertNoMatch(RegexProvider.TableNamePlusColumnNameWithOptionalTotalRegex, "<total dont.go.fish>");
			AssertNoMatch(RegexProvider.TableNamePlusColumnNameWithOptionalTotalRegex, "<total here.we.are>");
		}

		public void TestIsSingleMacro()
		{
			AssertEquals(true, RegexProvider.IsSingleMacro("<Now>"));
			AssertEquals(false, RegexProvider.IsSingleMacro("<Now><Then>"));
			AssertEquals(false, RegexProvider.IsSingleMacro("@<Now>%"));
			AssertEquals(false, RegexProvider.IsSingleMacro("A<Now>"));
			AssertEquals(false, RegexProvider.IsSingleMacro("<Now>B"));
			AssertEquals(true, RegexProvider.IsSingleMacro("<Now(1,2)>"));
			AssertEquals(true, RegexProvider.IsSingleMacro("<Now(<p>,<q>)>"));
		}

		public void TestGetInnermostMacroWithValueContainsAngleBracket()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
				@"{A}-[#Config]
{A}-[#EndOfReport]");
			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				MacroTranslator instance = new MacroTranslator(report);
				string macroText1 = "<Upper(\"<SubString(\"aaaa\\<bbbb\",2,5)>\")>";
				string macroText2 = "<Upper(\"<SubString(\"\\<\\>aa\\<\\>\\<bb\\<cc\\>\", 3, 10)>\")>";
				string result1 = instance.GetValue(macroText1, Passes.FirstPass).ToString();
				string result2 = instance.GetValue(macroText2, Passes.FirstPass).ToString();
				AssertEquals("AA\\<BB", result1);
				AssertEquals("A\\<\\>\\<BB\\<CC\\>", result2);
			}
		}
	}
}
