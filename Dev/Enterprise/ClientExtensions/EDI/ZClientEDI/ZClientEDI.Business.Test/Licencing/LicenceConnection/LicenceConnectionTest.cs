using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	[TestedType(typeof(LicenceConnection))]
	public class LicenceConnectionTest : SecurityBusinessObjectTestCase
	{
		public void TestLogging()
		{
			var conn = Factory.NewWithValidTestData<LicenceConnection>();
			conn.LK_RemoteAccessMethod = "BLK";
			conn.LK_RemoteAccessAddress = "34234";

			Factory.Save();

			ZString expectedReference = "Connection - Remote Access Method: BLK Address: 34234";
			AssertEquals("Expected log reference", expectedReference, conn.Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem).SL_Reference);

			conn.LK_RemoteAccessMethod = "JJJ";
			conn.LK_RemoteAccessAddress = "1199";

			Factory.Save();

			expectedReference = "Connection - Remote Access Method: JJJ(BLK) Address: 1199(34234)";
			AssertEquals("Expected log reference", expectedReference, conn.Logs.MostRecentLogByEventTime(Events.EditedARecord).SL_Reference);
		}

		public void TestRemoteAccessMethodDescription()
		{
			LicenceConnection conn = Factory.New<LicenceConnection>();
			AssertEquals("No method", ZString.Empty, conn.LK_RemoteAccessMethod);
			AssertEquals("No method description", ZString.Empty, conn.RemoteAccessMethodDescription);

			conn.LK_RemoteAccessMethod = "PCA";
			AssertEquals("Method description set", "PC Anywhere", conn.RemoteAccessMethodDescription);

			conn.LK_RemoteAccessMethod = "RDT";
			AssertEquals("Method description set", "Remote Desktop", conn.RemoteAccessMethodDescription);

			conn.LK_RemoteAccessMethod = "";
			AssertEquals("Method description cleared", ZString.Empty, conn.RemoteAccessMethodDescription);
		}

		public void TestBuildRemoteDesktopBatchCommands()
		{
			LicenceConnection conn = Factory.New<LicenceConnection>();
			string validationErrorText;
			var result = conn.BuildRemoteDesktopBatchCommands(out validationErrorText);
			AssertEquals("You must specify a username, password and connection address before attempting connection using Remote Desktop.", validationErrorText);
			AssertNull(result);

			conn.LK_RemoteAccessAddress = "someserver:aa";
			conn.LK_RemoteAccessUserName = "someuser";
			conn.LK_RemoteAccessPassWord = "somepass";
			result = conn.BuildRemoteDesktopBatchCommands(out validationErrorText);
			AssertEquals("The IP address specified appears to be invalid.", validationErrorText);
			AssertNull(result);

			conn.LK_RemoteAccessAddress = "someserver";
			result = conn.BuildRemoteDesktopBatchCommands(out validationErrorText);
			AssertEquals("@echo off\r\ncmdkey /generic:someserver /user:someuser /pass:somepass > nul\r\nstart mstsc /v:someserver", result);
			AssertNull("error", validationErrorText);

			conn.LK_RemoteAccessAddress = "someserver:1234";
			result = conn.BuildRemoteDesktopBatchCommands(out validationErrorText);
			AssertEquals("@echo off\r\ncmdkey /generic:someserver /user:someuser /pass:somepass > nul\r\nstart mstsc /v:someserver:1234", result);
			AssertNull("error", validationErrorText);

			conn.LK_RemoteAccessAddress = "someserver:1234 /console";
			result = conn.BuildRemoteDesktopBatchCommands(out validationErrorText);
			AssertEquals("@echo off\r\ncmdkey /generic:someserver /user:someuser /pass:somepass > nul\r\nstart mstsc /v:someserver:1234 /admin", result);
			AssertNull("error", validationErrorText);

			conn.LK_RemoteAccessAddress = "someserver:1234 /admin";
			result = conn.BuildRemoteDesktopBatchCommands(out validationErrorText);
			AssertEquals("@echo off\r\ncmdkey /generic:someserver /user:someuser /pass:somepass > nul\r\nstart mstsc /v:someserver:1234 /admin", result);
			AssertNull("error", validationErrorText);

			conn.LK_RemoteAccessAddress = "someserver /admin";
			result = conn.BuildRemoteDesktopBatchCommands(out validationErrorText);
			AssertEquals("@echo off\r\ncmdkey /generic:someserver /user:someuser /pass:somepass > nul\r\nstart mstsc /v:someserver /admin", result);
			AssertNull("error", validationErrorText);
		}

		#region Read Only Security

		public void TestReadOnlySecurity()
		{
			bool oldValue = EDISecurityCheckpoints.OrgLicenceModifyConnectionDetails.IsAllowed;

			LicenceConnection conn = Factory.New<LicenceConnection>();
			try
			{
				string[] propertyNamesToExcept = Array.Empty<string>();
				EDISecurityCheckpoints.OrgLicenceModifyConnectionDetails.IsAllowed = true;
				AssertPropertyInfosReadOnly(conn, false, propertyNamesToExcept);

				propertyNamesToExcept = Array.Empty<string>();
				EDISecurityCheckpoints.OrgLicenceModifyConnectionDetails.IsAllowed = false;
				AssertPropertyInfosReadOnly(conn, true, propertyNamesToExcept);
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceModifyConnectionDetails.IsAllowed = oldValue;
			}
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<EDIOrgHeader>();
			header.OH_Code = "XYZABC";
			header.MainAddress.OA_Address1 = "Address";
			header.CreateAndLoadLicenceForOrg();
			LicenceDatabase licDB = header.LicCompany.LicDatabases.AddNew();
			return licDB.Connections.AddNew();
		}

		#endregion
	}
}
