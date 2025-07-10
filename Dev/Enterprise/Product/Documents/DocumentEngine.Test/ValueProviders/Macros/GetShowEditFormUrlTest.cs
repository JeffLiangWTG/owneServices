using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(GetShowEditFormUrl))]
	sealed class GetShowEditFormUrlTest : ValueProviderTest
	{
		public void TestReplacementWithInvalidParameters()
		{
			AssertEquals("Precondition - report.ErrorManager.HasErrors is false", false, Report.ErrorManager.HasErrors);
			ValueProviderToTest.GetReplacement("<GetShowEditFormUrl(JobConsol, 123456789)>", Report);
			AssertEquals("report.ErrorManager.HasErrors is true", true, Report.ErrorManager.HasErrors);
			AssertEquals("report.ErrorManager.IsWarningOnly is true", true, Report.ErrorManager.HasWarningsOnly);
			AssertStartsWith("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in GetShowEditFormUrl Macro: Invalid PK '123456789';",
				Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
			Report.ErrorManager.ClearErrors();

			AssertEquals("Precondition - report.ErrorManager.HasErrors is false", false, Report.ErrorManager.HasErrors);
			ValueProviderToTest.GetReplacement("<GetShowEditFormUrl(InvalidControllerID, 123456789)>", Report);
			AssertEquals("report.ErrorManager.HasErrors is true", true, Report.ErrorManager.HasErrors);
			AssertEquals("report.ErrorManager.IsWarningOnly is true", true, Report.ErrorManager.HasWarningsOnly);
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in GetShowEditFormUrl Macro: No such ControllerID 'InvalidControllerID']",
							Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			Report.ErrorManager.ClearErrors();

			AssertEquals("Precondition - report.ErrorManager.HasErrors is false", false, Report.ErrorManager.HasErrors);
			ValueProviderToTest.GetReplacement("<GetShowEditFormUrl(JobInvoicing,01-02-03-04)>", Report);
			AssertEquals("report.ErrorManager.HasErrors is true", true, Report.ErrorManager.HasErrors);
			AssertEquals("report.ErrorManager.IsWarningOnly is true", true, Report.ErrorManager.HasWarningsOnly);
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in GetShowEditFormUrl Macro: The Controller ID 'JobInvoicing' cannot be used with this macro.]",
				Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			Report.ErrorManager.ClearErrors();
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("Should not pass", !ValueProviderToTest.IsResponsibleForReplacing("GetShowEditFormUrl", Passes.FirstPass));
			Assert("Should not pass", !ValueProviderToTest.IsResponsibleForReplacing("<GetShowEditFormUrl meh>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<GetShowEditFormUrl(JobConsol,6E449683-C509-11CF-AAFA-00AA00B6015C)>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<GetShowEditFormUrl(JobConsol,6e449683-c509-11cf-aafa-00aa00b6015c)>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<GetShowEditFormUrl(JobConsol, bf51f612-4d56-48fb-ab7a-79e764f1819d)>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("< GETSHOWEDITFORMURL ( JobConsol , 6E449683-C509-11CF-AAFA-00AA00B6015C ) >", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			ZGuid pK = ZGuid.NewZGuid();
			AssertEquals(ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.JobConsol, pK.ToGuid()), ValueProviderToTest.GetReplacement("<GetShowEditFormUrl(JobConsol, " + pK + ")>", Report));
		}

		public void TestReplacement_GreaterThanTenThousandsTimes_NoError()
		{
			ZGuid pK = ZGuid.NewZGuid();
			for (int i = 0; i < 10002; i++)
			{
				ValueProviderToTest.GetReplacement("<GetShowEditFormUrl(JobConsol," + pK + ")>", Report);
			}

			AssertNotEquals("Should not contains the specific error.", "TooManyIDCountryFactories", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		[ExpectNoExceptions]
		public void TestReplacement_WithInvalidPK()
		{
			ValueProviderToTest.GetReplacement("<GetShowEditFormUrl(JobConsol,01-02-03-04)>", Report);
		}

		[ExpectNoExceptions]
		public void TestReplacement_WithInvalidControllerID()
		{
			ZGuid pK = ZGuid.NewZGuid();
			ValueProviderToTest.GetReplacement("<GetShowEditFormUrl(InvalidControllerID," + pK + ")>", Report);
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new GetShowEditFormUrl();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("PK", new Guid("6E449683-C509-11CF-AAFA-00AA00B6015C")));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Creditor.PK", new Guid("bf51f612-4d56-48fb-ab7a-79e764f1819d")));
		}
	}
}
