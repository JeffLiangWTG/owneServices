using System;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Security.Authentication;
using CargoWise.ActiveDirectory;
using CargoWise.Application;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.Test
{
	class DirectoryExceptionHandlerTest : TestCase
	{
		public void TestHandlesExceptionTypes()
		{
			AssertExceptionsHandled(true, new AuthenticationException(), new ActiveDirectoryObjectNotFoundException(), new DirectoryServicesCOMException());
			AssertExceptionsHandled(false, new NullReferenceException(), new OutOfMemoryException(), new IndexOutOfRangeException());
		}

		void AssertExceptionsHandled(bool shouldBeHandled, params Exception[] exceptions)
		{
			foreach (var exception in exceptions)
			{
				string errorMessage = string.Format("Should{0} have handled exception type {1}, but did{2}",
					shouldBeHandled ? "" : " NOT",
					exception.GetType(),
					shouldBeHandled ? " NOT" : "");
				AssertEquals(errorMessage, shouldBeHandled, DirectoryExceptionHandler.TryHandleDirectoryException(exception));
			}
		}

		public void TestShowsExceptionStacktraceForUnattendedUser()
		{
			AssertEquals("Should not be hosted", false, EnvProxy.IsHostedWithCargowise);
			AssertEquals("Should be WTG Internal System", true, ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalSystem());

			// Interactive
			Globals.IsUserInteractive = true;
			DirectoryExceptionHandler.TryHandleDirectoryException(new AuthenticationException("Blah"));
			AssertEquals("Message should not include exception stack trace when it is interactive", ADComExceptionMessageHeader + "Blah", UnitTestUserNotification.Instance.LastMessage.Text);

			// Non-interactive
			Globals.IsUserInteractive = false;
			DirectoryExceptionHandler.TryHandleDirectoryException(new AuthenticationException("Blah"));
			AssertStartsWith("Message should include exception stack trace when it is non-interactive", ADComExceptionMessageHeader + "System.Security.Authentication.AuthenticationException", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShowsExceptionStacktraceForUnattendedUser_Hosted()
		{
			Globals.IsUserInteractive = false;
			var keyMock = new Mock<IProductRegistrationKey>();
			var productRegistrationMock = new Mock<IProductRegistration>();
			productRegistrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(false);
			productRegistrationMock.Setup(r => r.Key).Returns(keyMock.Object);

			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			{
				keyMock.Setup(k => k.HostedLocation).Returns("SYD");
				AssertEquals("Should be hosted", true, EnvProxy.IsHostedWithCargowise);
				AssertEquals("Should not be WTG", false, ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalSystem());
				DirectoryExceptionHandler.TryHandleDirectoryException(new AuthenticationException("Blah"));
				AssertStartsWith("Message should include exception stack trace when it is hosted", ADComExceptionMessageHeader + "System.Security.Authentication.AuthenticationException", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowsExceptionStacktraceForUnattendedUser_WTG()
		{
			Globals.IsUserInteractive = false;
			EnvProxy.SetHostedLocationForTest("NCW");
			AssertEquals("Should not be hosted", false, EnvProxy.IsHostedWithCargowise);
			AssertEquals("Should be WTG Internal System", true, ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalSystem());
			DirectoryExceptionHandler.TryHandleDirectoryException(new AuthenticationException("Blah"));
			AssertStartsWith("Message should include exception stack trace when it is WTG", ADComExceptionMessageHeader + "System.Security.Authentication.AuthenticationException", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestNotShowsExceptionStacktraceForUnattendedUser_NonHosted_NotWTG()
		{
			Globals.IsUserInteractive = false;
			var keyMock = new Mock<IProductRegistrationKey>();
			var productRegistrationMock = new Mock<IProductRegistration>();
			productRegistrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(false);
			productRegistrationMock.Setup(r => r.Key).Returns(keyMock.Object);

			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			{
				keyMock.Setup(k => k.HostedLocation).Returns("NCW");
				AssertEquals("Should not be hosted", false, EnvProxy.IsHostedWithCargowise);
				AssertEquals("Should not be WTG", false, ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalSystem());
				DirectoryExceptionHandler.TryHandleDirectoryException(new AuthenticationException("Blah"));
				AssertStartsWith("Message should not include exception stack trace", ADComExceptionMessageHeader + "Blah", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShouldIncludeIdentityCorrectly()
		{
			var exceptionMessage = "Blah";
			var exceptionToHandle = new DirectoryServicesException(exceptionMessage);
			var identity = "nerd";
			var entityHeader = "Cannot process entity {0}\r\n";

			Globals.IsUserInteractive = true;
			DirectoryExceptionHandler.TryHandleDirectoryException(exceptionToHandle);
			AssertEquals("Message should not include identity when it is interactive", exceptionMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			DirectoryExceptionHandler.TryHandleDirectoryException(exceptionToHandle, identity);
			AssertEquals("Message should not include identity when it is interactive, even it is provided", exceptionMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			Globals.IsUserInteractive = false;
			DirectoryExceptionHandler.TryHandleDirectoryException(exceptionToHandle, identity);
			AssertEquals("Message should include identity when it is non-interactive and it is provided", string.Format(entityHeader, identity) + exceptionMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			DirectoryExceptionHandler.TryHandleDirectoryException(exceptionToHandle);
			AssertEquals("Message should not include identity if it is not provided, even when it is non-interactive", exceptionMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShowADComExceptionMessageHeaderCorrectly()
		{
			var exceptionMessage = "Blah";
			DirectoryExceptionHandler.TryHandleDirectoryException(new DirectoryServicesException(exceptionMessage));
			AssertEquals("Should not have AD COM exception message header for DirectoryServicesException", exceptionMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			DirectoryExceptionHandler.TryHandleDirectoryException(new AuthenticationException(exceptionMessage));
			AssertEquals("Should have AD COM exception message header for AuthenticationException", ADComExceptionMessageHeader + exceptionMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			DirectoryExceptionHandler.TryHandleDirectoryException(new DirectoryServicesCOMException(exceptionMessage));
			AssertEquals("Should have AD COM exception message header for ActiveDirectoryObjectNotFoundException", ADComExceptionMessageHeader + exceptionMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			DirectoryExceptionHandler.TryHandleDirectoryException(new ActiveDirectoryObjectNotFoundException(exceptionMessage));
			AssertEquals("Should have AD COM exception message header for ActiveDirectoryObjectNotFoundException", ADComExceptionMessageHeader + exceptionMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		string ADComExceptionMessageHeader => "An error occurred while communicating with the Active Directory controller. Please contact your System Administrator.\r\n\r\nError message is as follow:\r\n";
	}
}
