using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.eHubMessaging.Tests.Business.DownloadHandler
{
	public class FailedMessageHandlerFactoryTests : TestCaseWithFactory
	{
		public void TestGetGEIFailedMessageHandler()
		{
			var fullNameOfTheDefaultHandler = "Enterprise.Accounting.ElectronicMessaging.Common.GEIFailedMessageHandler";

			var handler1 = FailedMessageHandlerFactory.GetHandler("GEI", null);
			AssertEquals("Should be GEIFailedMessageHandler", fullNameOfTheDefaultHandler, handler1.GetType().FullName);

			var handler2 = FailedMessageHandlerFactory.GetHandler("GEI", "");
			AssertEquals("Should be GEIFailedMessageHandler", fullNameOfTheDefaultHandler, handler2.GetType().FullName);

			foreach (var countryCode in Country.LicenceKeyBuilderSupportedCountryCodes)
			{
				var handler = FailedMessageHandlerFactory.GetHandler("GEI", countryCode);
				switch (countryCode)
				{
					case CountryCodes.KoreaSouth:
						AssertEquals("Enterprise.Accounting.ElectronicMessaging.KoreaSouth.KoreaSouthGEIFailedMessageHandler", handler.GetType().FullName);
						break;

					default:
						AssertEquals(fullNameOfTheDefaultHandler, handler.GetType().FullName);
						break;
				}
			}
		}

		public void TestGetUnknownHandler()
		{
			var handler = FailedMessageHandlerFactory.GetHandler("Unknown Handler", null);
			AssertNull("Should be null", handler);
		}

		public void TestGetHandlerNameIsEmptyString()
		{
			var handler = FailedMessageHandlerFactory.GetHandler(string.Empty, null);
			AssertNull("Should be null", handler);
		}

		public void TestGetHandlerNameIsNull()
		{
			var handler = FailedMessageHandlerFactory.GetHandler(null, null);
			AssertNull("Should be null", handler);
		}

		public void TestGetObjectThatIsNotAHandler()
		{
			var obj = ObjectFactory.Get("TaskStatusChangeResponders");
			AssertNotNull("Should be able to create an instance of non handler object", obj);

			var handler = FailedMessageHandlerFactory.GetHandler("TaskStatusChangeResponders", null);
			AssertNull("Should be null", handler);
		}
	}
}
