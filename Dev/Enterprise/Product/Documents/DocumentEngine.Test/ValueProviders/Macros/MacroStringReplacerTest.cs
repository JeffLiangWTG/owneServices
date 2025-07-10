using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.ValueProviders;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	sealed class MacroStringReplacerTest : TestCaseWithFactory
	{
		public void TestTheWorldAccordingToWhatTedWants()
		{
			DummyBusinessObject tedsBO = Factory.New<DummyBusinessObject>();
			tedsBO.Z0_VarCharMax = "Clive Peters";
			tedsBO.Collection.AddNew().Z0_VarCharMax = "E";
			tedsBO.Collection.AddNew().Z0_VarCharMax = "e";
			tedsBO.Collection.AddNew().Z0_VarCharMax = "easy";

			MacroStringReplacer replacer = new MacroStringReplacer(new DataProviderList(BODocDataProvider.Get(tedsBO)));

			string sourceString = "<Z0_VarCharMax> - <Collection.Format(\"{Z0_VarCharMax}\", Comma)>.";
			AssertEquals("Clive Peters - E, e, easy."
				, replacer.ReplaceMacros(sourceString));
		}

		public void TestReplaceMacrosWithTopLevelDataSourceSupplied()
		{
			DummyBusinessObject dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_Description = "potato";
			dummyBO.Z0_VarCharMax = "Potato";
			dummyBO.Collection.AddNew().Z0_VarCharMax = "POTATO";

			MacroStringReplacer replacer = new MacroStringReplacer(new DataProviderList(BODocDataProvider.Get(dummyBO)));

			AssertReplacer(replacer
				, "One <Z0_Description> Two <Z0_VarCharMax> Three <Collection.Z0_VarCharMax> Four"
				, "One potato Two Potato Three POTATO Four"
				, "");

			AssertReplacer(replacer
				, "One <Z0_Description> Two <Z0_VarCharMax> Three <Collection.Z0_ImaginaryField> Four"
				, "One potato Two Potato Three  Four"
				, "Field <Collection.Z0_ImaginaryField> not found on DataSource Type [DummyBusinessObject].");

			AssertReplacer(replacer
			, "One <Z0_Description> Two <Z0_VarCharMax> Three <Collection.Z0_VarCharMax> Four"
			, "One potato Two Potato Three POTATO Four"
			, "");
		}

		public void TestReportNameShouldBeBlank()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			var replacer = new MacroStringReplacer(new DataProviderList(BODocDataProvider.Get(dummyBO)));
			AssertEquals(string.Empty, replacer.ReplaceMacros("<ReportName>"));
		}

		static void AssertReplacer(MacroStringReplacer replacer, string macrosAndText, string expectedResult, string expectedErrors)
		{
			CombineAssertions(delegate
			{
				AssertEquals("replacer.ReplaceMacros(\"" + macrosAndText + "\")", expectedResult, replacer.ReplaceMacros(macrosAndText));
				AssertEquals("replacer.GetErrorsFromLastReplace() replacing [" + macrosAndText + "]", expectedErrors, replacer.GetErrorsFromLastReplace());
			});
		}

		public void TestReplaceMacrosWithNullTopLevelDataSource()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{
				var replacer = new MacroStringReplacer(null);
			});
		}
	}
}
