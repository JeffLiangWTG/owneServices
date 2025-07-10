using System;
using CargoWise.Common;
using CargoWise.Definitions;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.ExceptionClasses.Testing
{
	public class ExternalStorageObjectNotFoundExceptionTest : TransactionedTestCase
	{
		public void TestReportExceptionForDeveloper_WhenClientIsHostedWithCW1()
		{
			// Arrange
			var guid = Guid.NewGuid().ToString();
			var exception = new ExternalStorageObjectNotFoundException(guid, "File cannot be found", "S3", null);
			ErrorReporter.Clear();

			SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https:\\\\test.s3.com");
			EnvProxy.SetHostedLocationForTest("SYD");

			// Act
			exception.ReportExceptionForDeveloper();

			// Assert
			AssertEquals("ExternalStorageObjectNotFoundException should be reported back", "ExternalStorageObjectNotFoundExceptionForDeveloper", ErrorReporter.LastKeyReported);
			AssertEquals("Message should contain PK info", $"eDoc with primary key '{guid}' could not be found in external storage 'S3'.", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestReportExceptionForDeveloper_WhenClientIsNotHostedWithCW1()
		{
			// Arrange
			var exception = new ExternalStorageObjectNotFoundException(Guid.NewGuid().ToString(), "File cannot be found", "S3", null);
			ErrorReporter.Clear();

			SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https:\\\\test.s3.com");
			EnvProxy.SetHostedLocationForTest("");

			// Act
			exception.ReportExceptionForDeveloper();

			// Assert
			AssertNullOrEmpty("ExternalStorageNetworkException will not be reported back for the client not hosted with CW1", ErrorReporter.LastKeyReported);
		}

		public void TestReportExceptionForDeveloper_WhenClientIsEDIProd()
		{
			// Arrange
			var guid = Guid.NewGuid().ToString();
			var exception = new ExternalStorageObjectNotFoundException(guid, "File cannot be found", "S3", null);
			ErrorReporter.Clear();

			SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https:\\\\test.s3.com");

			using (ZArchitecture.Modules.ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				// Act
				exception.ReportExceptionForDeveloper();

				// Assert
				AssertEquals(false, EnvProxy.IsHostedWithCargowise);
				AssertEquals("ExternalStorageObjectNotFoundException should be reported back", "ExternalStorageObjectNotFoundExceptionForDeveloper", ErrorReporter.LastKeyReported);
				AssertEquals("Message should contain PK info", $"eDoc with primary key '{guid}' could not be found in external storage 'S3'.", ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
			}
		}
	}
}
