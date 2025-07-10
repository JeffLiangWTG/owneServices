using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.IO;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(TranslateRegistry))]
	sealed class TranslateRegistryTest : ValueProviderWithLoadControlFactoryTest<TranslateRegistry>
	{
		public override void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match <TranslateRegistry blah blah(registryID)>", !ValueProviderToTest.IsResponsibleForReplacing("<TranslateRegistry blah blah(registryID)>", Passes.FirstPass));
			Assert("should not match <   TranslateRegistry  \t       blah   >", !ValueProviderToTest.IsResponsibleForReplacing("<   TranslateRegistry  \t       blah   >", Passes.FirstPass));
			Assert("should not match <   TranslateRegistry  \t       (, test only)   >", ValueProviderToTest.IsResponsibleForReplacing("<   TranslateRegistry  \t       (, test only)   >", Passes.SecondPass));
			Assert("should not match <   TranslateRegistry  \t       (, )   >", ValueProviderToTest.IsResponsibleForReplacing("<   TranslateRegistry  \t       (, )   >", Passes.SecondPass));
			Assert("should match <   TranslateRegistry  \t       (registryID, test only)   >", ValueProviderToTest.IsResponsibleForReplacing("<   TranslateRegistry  \t       (registryID, test only)   >", Passes.SecondPass));
			Assert("should match <   TranslateRegistry  \t       (registryID, test only)   >", ValueProviderToTest.IsResponsibleForReplacing("<   TranslateRegistry  \t       (registryID, test only, ZH-CN)   >", Passes.SecondPass));
		}

		public void TestGetReplacementCoreWithTrimText()
		{
			const string macro = "<TranslateRegistry(CashFlowActivityConfiguration, UmVjZWlwdHMgRnJvbSBDdXN0b21lcnMgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIA==, ZH-CN)>";
			const string chsString = "收款来自客户";

			var pack = new DocumentPack();
			TestData.CreateLinesTestTable();
			using (Report.TemporarilyUseMainConnection())
			using (var report = new Report(pack, NewStyleTemplate, Guid.NewGuid(), Core.Constants.DataContext.UnitTest))
			{
				var result = typeof(TranslateRegistry).GetMethod("GetReplacementCore", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(GetNewValueProvider(), new object[] { macro, report });
				AssertEquals(chsString, result);
			}
		}

		public void TestGetReplacementWithOldLanguageCode()
		{
			const string macro = "<TranslateRegistry(CashFlowActivityConfiguration, UmVjZWlwdHMgRnJvbSBDdXN0b21lcnMgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIA==, CHS)>";
			const string chsString = "收款来自客户";

			var pack = new DocumentPack();
			TestData.CreateLinesTestTable();
			using (Report.TemporarilyUseMainConnection())
			using (var report = new Report(pack, NewStyleTemplate, Guid.NewGuid(), Core.Constants.DataContext.UnitTest))
			{
				var result = ValueProviderToTest.GetReplacement(macro, report);
				AssertEquals(chsString, result);
				AssertEquals(
				"Error when processing the macro <TranslateRegistry(CashFlowActivityConfiguration, UmVjZWlwdHMgRnJvbSBDdXN0b21lcnMgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIA==, CHS)>: the language code CHS has been depreciated. Please use the ISO language code instead: ZH-CN.",
				((IHaveReportProcessingErrorsForGUI)report.ErrorManager).GetErrors()[0].Message);
			}
		}

		public override void TestReplacement()
		{
			var report = Report.NewForTesting(DocumentPack.EmptyPack);
			var registryValue = AccountingMasterFilesRegistry.Instance.CashFlowActivityConfiguration.DefaultValue.FirstOrDefault();
			var cashflowDescription = ((CashFlowActivityConfiguration)registryValue).Description;
			var encodedCashflowDescription = System.Convert.ToBase64String(Encoding.UTF8.GetBytes(cashflowDescription));
			var macro = string.Format("<TranslateRegistry(CashFlowActivityConfiguration, {0})>", encodedCashflowDescription);
			AssertEquals("Macro should decode correctly", cashflowDescription, ValueProviderToTest.GetReplacement(macro, report));
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			var registryValue = AccountingMasterFilesRegistry.Instance.CashFlowActivityConfiguration.DefaultValue.FirstOrDefault();
			var cashflowDescription = ((CashFlowActivityConfiguration)registryValue).Description;
			var encodedCashflowDescription = System.Convert.ToBase64String(Encoding.UTF8.GetBytes(cashflowDescription));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("EnglishText", encodedCashflowDescription));
		}

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever?.Dispose();
		}

		EmbeddedResourceRetriever embeddedResourceRetriever;

		ExcelTemplateForUnitTesting newStyleTemplate;
		ExcelTemplateForUnitTesting NewStyleTemplate
		{
			get
			{
				if (newStyleTemplate == null)
				{
					embeddedResourceRetriever = new EmbeddedResourceRetriever();
					var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.NewStyleTemplate.xls", "NewStyleTemplate.xls");
					newStyleTemplate = new ExcelTemplateForUnitTesting("NewStyleTemplate.xls", Path.GetFullPath(tempFileName));
				}
				return newStyleTemplate;
			}
		}
	}
}
