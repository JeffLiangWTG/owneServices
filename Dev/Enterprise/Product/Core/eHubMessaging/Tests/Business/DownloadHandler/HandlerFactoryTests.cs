using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.eHubMessaging.Business.DownloadHandler;

namespace Enterprise.eHubMessaging.Tests.Business.DownloadHandler
{
	class HandlerFactoryTests : TestCaseWithFactory
	{
		public void TestGetAllHandlers()
		{
			var handlers = HandlerFactory.DebugOnlyGetHandlerTypes();
			Assert("debug handler types should not be empty", handlers.Any());

			foreach (var schemaTypePair in handlers)
			{
				var handler = HandlerFactory.GetHandler(schemaTypePair.Key);
				AssertEquals("Type of handler retrieved should match", schemaTypePair.Value, handler.GetType());
			}
		}

		public void TestGetUnknownHandler()
		{
			var handler = HandlerFactory.GetHandler("Some Unknown eHubMessaging Handler");
			AssertEquals("Should return unknown handler", typeof(UnknownTypeMessageHandler), handler.GetType());
		}

		public void TestGetHandlerNameIsEmptyString()
		{
			var handler = HandlerFactory.GetHandler(string.Empty);
			AssertEquals("Should return unknown handler", typeof(UnknownTypeMessageHandler), handler.GetType());
		}

		public void TestGetHandlerNameIsNull()
		{
			var handler = HandlerFactory.GetHandler(null);
			AssertEquals("Should return unknown handler", typeof(UnknownTypeMessageHandler), handler.GetType());
		}

		public void TestGetObjectThatIsNotAHandler()
		{
			var obj = ObjectFactory.Get("TaskStatusChangeResponders");
			AssertNotNull("Should be able to create an instance of non handler object", obj);

			var handler = HandlerFactory.GetHandler("TaskStatusChangeResponders");
			AssertEquals("Should return unknown handler", typeof(UnknownTypeMessageHandler), handler.GetType());
		}
	}
}
