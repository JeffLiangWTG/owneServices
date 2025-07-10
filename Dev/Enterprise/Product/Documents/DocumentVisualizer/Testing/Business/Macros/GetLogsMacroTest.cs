using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class GetLogsMacroTest : TestCaseWithFactory
	{
		public void TestRunMacro_NullParent()
		{
			var expr = "@data.GetLogs()".With<DataLibrary>().CreateExpression();

			var result = (IEnumerable<Log>)expr.Evaluate(null);

			AssertMultilineASCIIEquals("no errors", "", expr.ToFormatString());
			AssertNotNull("prerequisite: macro has returned a value", result);

			AssertContainsExactElementsInAnyOrder("logs",
				System.Array.Empty<string>(),
				result.Select(log => log.Event.Code));
		}

		public void TestRunMacro_ParentWithNoLogs()
		{
			var dummy = Factory.New<DummyWithLogs>();

			Factory.Save();

			var expr = "@data.GetLogs()".With<DataLibrary>().CreateExpression();

			var result = expr.Evaluate(dummy.MakeDynamic(new MetaDataProviderForLogRelatedTest())) as IEnumerable<Log>;

			AssertMultilineASCIIEquals("no errors", "", expr.ToFormatString());
			AssertNotNull("prerequisite: macro has returned a value", result);

			AssertContainsExactElementsInAnyOrder("logs",
				System.Array.Empty<string>(),
				result.Select(log => log.Event.Code));
		}

		public void TestRunMacro_ParentWithLogs()
		{
			var dummy = Factory.New<DummyWithLogs>();
			var log1 = dummy.Logs.AddNew(Events.Authorised);
			var log2 = dummy.Logs.AddNew(Events.Delivered);
			dummy.Logs.AddNew(Events.Attached).Cancel();

			Factory.Save();

			var expr = "@data.GetLogs()".With<DataLibrary>().CreateExpression();

			var result = (IEnumerable<Log>)expr.Evaluate(dummy.MakeDynamic(new MetaDataProviderForLogRelatedTest()));

			AssertMultilineASCIIEquals("no errors", "", expr.ToFormatString());
			AssertNotNull("prerequisite: macro has returned a value", result);

			AssertContainsExactElementsInAnyOrder("logs",
				new[]
				{
					log1.SL_SE_NKEvent,
					log2.SL_SE_NKEvent
				},
				result.Select(log => log.Event.Code));
		}

		public void TestRunMacro_ParentWithLogsAndParameters()
		{
			var dummy = Factory.New<DummyWithLogs>();

			var log1 = dummy.Logs.AddNew(Events.Authorised,
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, "AAA"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Name, "log1"));

			var log2 = dummy.Logs.AddNew(Events.Delivered,
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, "AAA"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Name, "log2"));

			var log3 = dummy.Logs.AddNew(Events.Authorised,
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, "BBB"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Name, "log3"));

			var log4 = dummy.Logs.AddNew(Events.Authorised,
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, "Sydney"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, "AAA"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Name, "log4"));

			Factory.Save();

			var expr = "@data.GetLogs().Where({Event.Code == \"ATH\" && TYP == \"AAA\"})".With<DataLibrary>().And<StandardLibrary>().CreateExpression();

			var result = (IEnumerable<object>)expr.Evaluate(dummy.MakeDynamic(new MetaDataProviderForLogRelatedTest()));

			AssertMultilineASCIIEquals("no errors", "", expr.ToFormatString());
			AssertNotNull("prerequisite: macro has returned a value", result);

			AssertContainsExactElementsInAnyOrder("logs",
				new[]
				{
					log1.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Name],
					log4.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Name]
				},
				result.Cast<dynamic>().Select(log => log.NAM));
		}
	}
}
