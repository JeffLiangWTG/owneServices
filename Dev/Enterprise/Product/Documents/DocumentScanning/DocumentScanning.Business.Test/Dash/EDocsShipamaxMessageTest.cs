using System;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(EDocsShipamaxMessage))]
	public sealed class EDocsShipamaxMessageTest : EDIMessageTest
	{
		public void TestParseResultData()
		{
			var message = Factory.New<EDocsShipamaxMessage>();

			AssertNull("ParseDataJson should be null", message.MessageData.ParseDataJson);
			AssertNull("ParseResultXml should be null", message.MessageData.ParseResultXml);

			message.EM_MessageData = new byte[] { 1, 2, 3 };
			message.MessageData = null;

			AssertEquals("EM_MessageData should be empty when ParseResultData is null", expected: true, message.EM_MessageData.IsEmpty);

			message.EM_MessageData = MessageEncoding.UTF8WithoutBOM.GetBytes("{\"ParseDataJson\":\"Json\", \"ParseResultXml\":\"Xml\"}");

			AssertNotNull("ParseResultData should not be null", message.MessageData);
			AssertEquals("Json", message.MessageData.ParseDataJson);
			AssertEquals("Xml", message.MessageData.ParseResultXml);

			message.EM_MessageData = ZBlob.Empty;

			AssertNull("ParseDataJson should be null as it is reset when EM_MessageData is set", message.MessageData.ParseDataJson);
			AssertNull("ParseResultXml should be null as it is reset when EM_MessageData is set", message.MessageData.ParseResultXml);

			message.EM_MessageData = MessageEncoding.UTF8WithoutBOM.GetBytes("{\"ParseDataJson\":\"Json\"}");

			AssertNotNull("ParseResultData should not be null", message.MessageData);
			AssertEquals("Json", message.MessageData.ParseDataJson);
			AssertNull("ParseResultXml should be null", message.MessageData.ParseResultXml);
		}

		public void TestDefaultValues()
		{
			var message = Factory.New<EDocsShipamaxMessage>();

			AssertEquals("EM_ApplicationCode should be SPM", "SPM", message.EM_ApplicationCode);
			AssertEquals("EM_ReceiveTransmit should be INT", "INT", message.EM_ReceiveTransmit);
			AssertEquals("EM_Status should be QUE", "QUE", message.EM_Status);
		}

		public void TestProperty_LinkedEDoc_StorageMainDoesNotExist()
		{
			var message = Factory.New<EDocsShipamaxMessage>();
			message.EM_ApplicationReference = ZGuid.NewZGuid().ToString();
			message.EM_LinkUniqueID = ZGuid.NewZGuid();
			message.EM_LinkTable = AutoStorageDocs.Schema.TableName;

			StorageDocsBase loadedEDoc = null;
			AssertNoExceptionThrown(() =>
			{
				loadedEDoc = message.LinkedEDoc;
			});

			AssertNull("No eDoc was loaded", loadedEDoc);
		}

		public void TestProperty_LinkedEDoc_EDocLoaded()
		{
			using (var auxConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.CreateDbDropExisting(auxConnection, Db.DatabaseName + "_SD001");
			}

			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var masterFactory = new DocumentFactoryProvider().GetFactory(Factory);
				var docFactory = masterFactory.GetFactory(1);
				var eDoc1 = docFactory.NewWithParent(typeof(StorageFile));
				eDoc1.SC_DocType = "CIV";
				eDoc1.SC_DataType = "PDF";
				eDoc1.ParentMain.SM_DB = 1;
				eDoc1.ParentMain.SM_ParentFK = ZGuid.NewZGuid();
				eDoc1.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;

				var message = Factory.New<EDocsShipamaxMessage>();
				message.EM_ApplicationReference = eDoc1.ParentMain.PK.ToString();
				message.EM_LinkUniqueID = eDoc1.PK;
				message.EM_LinkTable = AutoStorageDocs.Schema.TableName;

				var eDoc2 = docFactory.NewWithParent(typeof(StorageFile));
				eDoc2.SC_DocType = "CIV";
				eDoc2.SC_DataType = "PDF";
				eDoc2.ParentMain.SM_DB = 1;
				eDoc2.ParentMain.SM_ParentFK = ZGuid.NewZGuid();
				eDoc2.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;

				var messageFromDocumentFactory = masterFactory.New<EDocsShipamaxMessage>();
				messageFromDocumentFactory.EM_ApplicationReference = eDoc2.ParentMain.PK.ToString();
				messageFromDocumentFactory.EM_LinkUniqueID = eDoc2.PK;
				messageFromDocumentFactory.EM_LinkTable = AutoStorageDocs.Schema.TableName;

				masterFactory.Save();

				AssertNotNull("There should be one message attached to eDoc1", eDoc1.ActiveShipamaxMessage);
				AssertNotNull("eDoc1 was loaded", eDoc1.ActiveShipamaxMessage.LinkedEDoc);
				AssertNotNull("There should be one message attached to eDoc2", eDoc2.ActiveShipamaxMessage);
				AssertNotNull("eDoc2 was loaded", eDoc2.ActiveShipamaxMessage.LinkedEDoc);
			}
		}

		public void TestProperty_StatusName_When_Status_Is_Queued()
		{
			var message = Factory.New<EDocsShipamaxMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.Queued;

			AssertEquals("Unparsed", message.StatusName);
		}

		public void TestProperty_StatusName_When_Status_Is_Sent()
		{
			var message = Factory.New<EDocsShipamaxMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.Sent;

			AssertEquals("Processing", message.StatusName);
		}

		public void TestProperty_StatusName_When_Status_Is_Failed()
		{
			var message = Factory.New<EDocsShipamaxMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.Failed;

			AssertEquals("Failed", message.StatusName);
		}

		public void TestProperty_StatusName_When_Status_Is_ProcessedOK()
		{
			var message = Factory.New<EDocsShipamaxMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;

			AssertEquals("Complete", message.StatusName);
		}

		public void TestProperty_StatusName_When_Status_Is_PreProcessedOK()
		{
			var message = Factory.New<EDocsShipamaxMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;

			AssertEquals("Need Review", message.StatusName);
		}

		public void TestProperty_StatusName_When_Status_Is_Discarded()
		{
			var message = Factory.New<EDocsShipamaxMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.Discarded;

			AssertEquals("Discarded", message.StatusName);
		}

		public void TestProperty_StatusName_When_Status_Is_Error()
		{
			var message = Factory.New<EDocsShipamaxMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.Error;

			AssertEquals("Error", message.StatusName);
		}

		public void TestProperty_StatusName_When_Status_Is_Cancelled()
		{
			var message = Factory.New<EDocsShipamaxMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.Cancelled;

			AssertEquals("User Canceled", message.StatusName);
		}

		public void TestProperty_StatusName_When_Status_Is_Withdrawn()
		{
			var message = Factory.New<EDocsShipamaxMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.Withdrawn;

			AssertEquals("Disabled", message.StatusName);
		}

		public void TestProperty_StatusCode()
		{
			var message = Factory.New<EDocsShipamaxMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.Queued;

			AssertEquals(EDIMessageStatusList.Codes.Queued, message.StatusCode);
		}

		public void TestProperty_IsRequestCancelled()
		{
			var message = Factory.New<EDocsShipamaxMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.Cancelled;

			AssertEquals(true, message.IsRequestCancelled);

			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			AssertEquals(false, message.IsRequestCancelled);
		}

		public void TestProperty_IsRequestCompleted()
		{
			var message = Factory.New<EDocsShipamaxMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;

			AssertEquals(true, message.IsRequestCompleted);

			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			AssertEquals(false, message.IsRequestCompleted);
		}

		public void TestProperty_IsRequestInProgress()
		{
			var message = Factory.New<EDocsShipamaxMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.Sent;

			AssertEquals(true, message.IsRequestInProgress);

			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			AssertEquals(false, message.IsRequestInProgress);
		}

		public void TestProperty_IsRequestError()
		{
			var message = Factory.New<EDocsShipamaxMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.Error;

			AssertEquals(true, message.IsRequestError);

			message.EM_Status = EDIMessageStatusList.Codes.Failed;
			AssertEquals(true, message.IsRequestError);

			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			AssertEquals(false, message.IsRequestError);
		}

		public void TestProperty_UpdateStatus()
		{
			var message = Factory.New<EDocsShipamaxMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.Error;

			message.UpdateStatus(EDIMessageStatusList.Codes.ProcessedOK);
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
		}

		public void TestProperty_UpdateStatus_Throws_Error_When_Status_Is_Not_Supported()
		{
			var message = Factory.New<EDocsShipamaxMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.Error;

			AssertExceptionThrown($"Status some_status is not supported", typeof(InvalidOperationException), () => message.UpdateStatus("some_status"));
		}

		#region Implementation

		protected override void MasterSetUp()
		{
			base.MasterSetUp();

			if (!dbHelper.DatabaseExists(1))
			{
				dbHelper.CreateDatabase(1);
			}
		}

		protected override void FinalTearDown()
		{
			base.FinalTearDown();

			if (dbHelper.DatabaseExists(1))
			{
				var dbName = dbHelper.GetDatabaseName(1);
				dbHelper.DropDatabase(dbName);
			}
		}

		readonly DocManagerDBHelperTestClass dbHelper = new ();

		#endregion
	}
}
