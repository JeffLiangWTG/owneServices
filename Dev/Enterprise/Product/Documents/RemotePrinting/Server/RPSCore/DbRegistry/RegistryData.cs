using System;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Integration;

namespace Enterprise.RemotePrinting.Server.RPSCore
{
	public static class RegistryData
	{
		public static string SMTPDefaultReturnEmailAddress(DbConnection connection)
		{
			Argument.NotNull(connection, nameof(connection));

			var result = new SMTPDefaultReturnEmailAddressRegistryItem().LoadValue(connection);

			return result;
		}

		public static string WebServicePassword(DbConnection connection)
		{
			Argument.NotNull(connection, nameof(connection));

			return new WebServicePasswordRegistryItem().LoadValue(connection);
		}

		public static string WebServiceUsername(DbConnection connection)
		{
			Argument.NotNull(connection, nameof(connection));

			return new WebServiceUsernameRegistryItem().LoadValue(connection);
		}

		public static ICodeDescriptionPairList WebServiceAlternativeCredentials(DbConnection connection)
		{
			Argument.NotNull(connection, nameof(connection));

			return new WebServiceAlternativeCredentialsRegistryItem().LoadValue(connection);
		}

		public static Guid WebPrintNotificationGroup(DbConnection connection)
		{
			Argument.NotNull(connection, nameof(connection));

			return new WebPrintNotificationGroupRegistryItem().LoadValue(connection);
		}

		public static int WebPrintDocumentPackMaxSize(DbConnection connection)
		{
			Argument.NotNull(connection, nameof(connection));

			var result = new WebPrintDocumentPackMaxSizeRegistryItem().LoadValue(connection);

			return result;
		}

		public static bool WebPrintAllowDirectPrintPrintPushNotification(DbConnection connection)
		{
			Argument.NotNull(connection, nameof(connection));

			var result = new WebPrintAllowDirectPrintPrintPushNotificationRegistryItem().LoadValue(connection);

			return result;
		}

		public static int WebPrintSignalRIncomingMaxSize(DbConnection connection)
		{
			Argument.NotNull(connection, nameof(connection));

			var result = new WebPrintSignalRIncomingMaxSize().LoadValue(connection);

			return result;
		}

		public static int WebServiceCredentialsCacheTime(DbConnection connection)
		{
			Argument.NotNull(connection, nameof(connection));

			return new WebServiceCredentialsCacheTimeRegistryItem().LoadValue(connection);
		}

		public static bool WebPrintForceToUseHTTPSForWebPrintRequests(DbConnection connection)
		{
			Argument.NotNull(connection, nameof(connection));

			return new WebPrintForceToUseHTTPSForWebPrintRequestsRegistryItem().LoadValue(connection);
		}
	}
}
