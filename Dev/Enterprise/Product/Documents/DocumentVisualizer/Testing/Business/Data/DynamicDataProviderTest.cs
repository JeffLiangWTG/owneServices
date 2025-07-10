using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DynamicDataProviderTest : TestCaseWithFactory
	{
		public void TestGetDynamicDataFromUXml()
		{
			var consol = (IBusiness)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();

			var result = consol.TryCreate(DataContext.UXML);

			AssertNotNull("UXml data created", result.Value);
			Assert("UXml data created", result.Value.Value is ITopLevelDataObject);
		}

		public void TestGetDynamicDataFromVisualizableDocumentSupporter()
		{
			var bizObj = (IBusiness)Factory.New<DummyWithUXmlSupport>();

			var tryCreateDynamicData = bizObj.TryCreate("not supported");
			AssertEquals("IsFaulted", true, tryCreateDynamicData.IsFaulted);
			AssertEquals("faulted message", "data context is not supported", tryCreateDynamicData.Message);

			tryCreateDynamicData = bizObj.TryCreate("supported");
			AssertEquals("IsFaulted", false, tryCreateDynamicData.IsFaulted);
			Assert("DynamicData has been created", tryCreateDynamicData.Value is IDynamicData);
		}

		public void TestGetDynamicDataFromUnsupported()
		{
			var bizObj = (IBusiness)Factory.New<DummyWithUXmlSupport>();
			var tryCreateDynamicData = bizObj.TryCreate("unsupported-data-context");

			AssertEquals("IsFaulted", true, tryCreateDynamicData.IsFaulted);
		}

		public void TestReportExceptionsWhenCreatingDocDataObjects()
		{
			const string exceptionMessage = "Kaboom!";
			const string dataContextForTest = "data-context-for-test";

			using (DummyVisualizableDocumentSupporter.TempSetGetDocDataObjectImpl((parent, dataContext, parameters) => throw new NullReferenceException(exceptionMessage)))
			{
				var bizObj = (IBusiness)Factory.New<DummyWithUXmlSupport>();
				var tryCreateDynamicData = bizObj.TryCreate(dataContextForTest);

				AssertEquals("IsFaulted", true, tryCreateDynamicData.IsFaulted);
				AssertEquals("Exception.Message", exceptionMessage, tryCreateDynamicData.Exception.Message);

				Assert("Reported exception", ErrorReporter.LastExceptionReported is NullReferenceException);
				AssertEquals("Reported exception", exceptionMessage, ErrorReporter.LastExceptionReported.Message);
				AssertEquals("Reported exception", dataContextForTest, ErrorReporter.LastExceptionReported.Data["DataContext"]);

				ErrorReporter.Clear();
			}
		}
	}
}
