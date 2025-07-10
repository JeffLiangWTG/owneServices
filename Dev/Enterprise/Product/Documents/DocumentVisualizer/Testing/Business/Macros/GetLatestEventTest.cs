using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Business;
using Constants = CargoWise.EventReference.Constants;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class GetLatestEventTest : TestCaseWithFactory
	{
		public void TestRunMacro_GetMostCurrentLogs()
		{
			var dummy = Factory.New<DummyWithLogs>();

			var log1 = dummy.Logs.AddNew(Events.StatusUpdated, "Status A", ZDateTimeOffset.Now,
				new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Type, "Reset To Original"),
				new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Department, "TPT"));

			var log2 = dummy.Logs.AddNew(Events.StatusUpdated, "Status B", ZDateTimeOffset.Now.AddDays(-2),
				new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Type, "Reset To Original"),
				new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Department, "TPT"));

			var log3 = dummy.Logs.AddNew(Events.StatusUpdated, "Status C", ZDateTimeOffset.Now.AddDays(2),
				new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Type, "Reset To Original"),
				new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Department, "TPT"));

			var log4 = dummy.Logs.AddNew(Events.StatusUpdated, "Status D", ZDateTimeOffset.Now.AddDays(3),
				new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Type, "Reset To Original"),
				new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Department, "OMG"));

			Factory.Save();

			var expr = "@data.GetLatestEvent({Event.Code == \"STU\" && DEP == \"TPT\"});".With<DataLibrary>().CreateExpression();

			var descriptor = new Moq.Mock<IDocumentDescriptor>();
			var document = new DocumentVisualizer.Business.Document(descriptor.Object, dummy);

			var obj = expr.Evaluate(document);
			AssertEquals("Status C", ((Log)expr.Evaluate(document)).Reference);
		}
	}
}
