using System;
#if NETFRAMEWORK
using System.Data.EntityClient;
#else
using System.Data.Entity.Core.EntityClient;
#endif
using CargoWise.Data.Providers.Common;
using NUnit.Framework;

namespace CargoWise.Services.Common
{
	sealed class ConnectionProviderTest : TestCase
	{
		public void TestGetCommonConnectionString()
		{
			string expectedString = String.Format("metadata=res://*/Model.CommonModel.csdl|res://*/Model.CommonModel.ssdl|res://*/Model.CommonModel.msl;provider=System.Data.SqlClient;provider connection string=\"Data Source={0};Initial Catalog=eHubTransactions;Integrated Security=True;MultipleActiveResultSets=True;Connect Timeout=60;Encrypt=False;TrustServerCertificate=True;Application Name=eServices\"", Environment.MachineName);
			expectedString = AdjustString(expectedString);
			AssertEquals(expectedString, ConnectionProvider.GetCommonConnectionString());

			expectedString = String.Format("Data Source={0};Initial Catalog=eHubTransactions;Integrated Security=True;MultipleActiveResultSets=True;Connect Timeout=60;Encrypt=False;TrustServerCertificate=True;Application Name=eServices", Environment.MachineName);
			expectedString = AdjustString(expectedString);
			AssertEquals(expectedString, ConnectionProvider.GetCommonProviderConnectionString());
		}

		public void TestGetConnectionString()
		{
			string expectedString = String.Format("metadata=res://*/Model.ContentModel.csdl|res://*/Model.ContentModel.ssdl|res://*/Model.ContentModel.msl;provider=System.Data.SqlClient;provider connection string=\"Data Source={0};Initial Catalog=DeniedPartyContent;Integrated Security=True;MultipleActiveResultSets=True;Connect Timeout=60;Encrypt=False;TrustServerCertificate=True;Application Name=eServices\"", Environment.MachineName);
			expectedString = AdjustString(expectedString);
			AssertEquals(expectedString, ConnectionProvider.GetConnectionString("DeniedPartyContent"));

			expectedString = String.Format("metadata=res://*/Model.TransactionModel.csdl|res://*/Model.TransactionModel.ssdl|res://*/Model.TransactionModel.msl;provider=System.Data.SqlClient;provider connection string=\"Data Source={0};Initial Catalog=DeniedPartyTransactions;Integrated Security=True;MultipleActiveResultSets=True;Connect Timeout=60;Encrypt=False;TrustServerCertificate=True;Application Name=eServices\"", Environment.MachineName);
			expectedString = AdjustString(expectedString);
			AssertEquals(expectedString, ConnectionProvider.GetConnectionString("DeniedPartyTransactions"));
		}

		public void TestEnabledMultiSubnetFailover()
		{
			// Arrange
			using (SqlFailoverSettingsTestHelper.SetMockWindowsRegistry(serversThatAreEnabled: new[] { Environment.MachineName }))
			{
				// Act
				var connectionString = ConnectionProvider.GetCommonConnectionString();

				// Assert
				var sqlConnectionString = new EntityConnectionStringBuilder(connectionString).ProviderConnectionString;
				AssertEquals(true, new SqlConnectionStringBuilder(sqlConnectionString).MultiSubnetFailover);
			}
		}

		static string AdjustString(string s)
		{
#if NETFRAMEWORK
			return s;
#else
			s = s.Replace(";MultipleActiveResultSets=", ";Multiple Active Result Sets=", StringComparison.Ordinal);
			s = s.Replace(";TrustServerCertificate=", ";Trust Server Certificate=", StringComparison.Ordinal);
			return s;
#endif
		}
	}
}
