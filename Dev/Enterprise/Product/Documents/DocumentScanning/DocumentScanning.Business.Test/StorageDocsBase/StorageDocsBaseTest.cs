using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Integration.RemoteDesktopServices;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.Shared.Dash.Common.Services;

namespace Enterprise.DocumentScanning.Business.Test
{
	public abstract class StorageDocsBaseTest : StorageDocsWithS3SupportTest
	{
		public void TestParseStatus()
		{
			StorageDocsBase newDoc;
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				newDoc = CreateEDocForShipamaxIntegration("CIV");

				MasterFactory.Save();

				AssertNotNull("EDocsShipamaxMessage should be created when a new eDoc is saved", newDoc.ActiveShipamaxMessage);
				AssertEquals("Unparsed", newDoc.ParseStatus);
			}

			newDoc.ActiveShipamaxMessage.EM_Status = EDIMessageStatusList.Codes.Sent;
			AssertEquals("Processing", newDoc.ParseStatus);

			newDoc.ActiveShipamaxMessage.EM_Status = EDIMessageStatusList.Codes.Failed;
			AssertEquals("Failed", newDoc.ParseStatus);

			newDoc.ActiveShipamaxMessage.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
			AssertEquals("Complete", newDoc.ParseStatus);

			newDoc.ActiveShipamaxMessage.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;
			AssertEquals("Need Review", newDoc.ParseStatus);

			newDoc.ActiveShipamaxMessage.EM_Status = EDIMessageStatusList.Codes.Discarded;
			AssertEquals("Discarded", newDoc.ParseStatus);

			newDoc.ActiveShipamaxMessage.EM_Status = EDIMessageStatusList.Codes.Error;
			AssertEquals("Error", newDoc.ParseStatus);

			newDoc.ActiveShipamaxMessage.EM_Status = EDIMessageStatusList.Codes.Cancelled;
			AssertEquals("User Canceled", newDoc.ParseStatus);

			newDoc.ActiveShipamaxMessage.EM_Status = EDIMessageStatusList.Codes.Withdrawn;
			AssertEquals("Disabled", newDoc.ParseStatus);

			newDoc.SC_DocType = "BOD";
			MasterFactory.Save();

			AssertEquals("Parse status should be empty", expected: true, newDoc.ParseStatus.IsEmpty);
		}

		public void TestShipamaxIntegration_NewEDocIsAdded()
		{
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var newDoc = CreateEDocForShipamaxIntegration("CIV");

				MasterFactory.Save();

				AssertNotNull("EDocsShipamaxMessage should be created when a new eDoc is saved", newDoc.ActiveShipamaxMessage);
			}
		}

		public void TestShipamaxIntegration_ChangeDocTypeToNotAcceptedFromAccepted()
		{
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var newDoc = CreateEDocForShipamaxIntegration("CIV");

				MasterFactory.Save();

				AssertNotNull("EDocsShipamaxMessage record was created during save process", newDoc.ActiveShipamaxMessage);

				newDoc.SC_DocType = "BOD";

				MasterFactory.Save();

				AssertNull("There should be no active EDocsShipamaxMessage", newDoc.ActiveShipamaxMessage);

				var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, newDoc.PK);
				var inactiveShipamaxMessage = Factory.LoadTop1<EDocsShipamaxMessage>(query);

				AssertNotNull("There should be one EDocsShipamaxMessage record in the database", inactiveShipamaxMessage);
				AssertEquals("The existing EDocsShipamaxMessage record should be inactive", expected: false, inactiveShipamaxMessage.EM_IsActive);
			}
		}

		public void TestShipamaxIntegration_ChangeDocTypeToAcceptedFromNotAccepted()
		{
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var newDoc = CreateEDocForShipamaxIntegration("BOD");

				MasterFactory.Save();

				AssertNull("No EDocsShipamaxMessage record was created during save process", newDoc.ActiveShipamaxMessage);

				newDoc.SC_DocType = "civ";

				MasterFactory.Save();

				AssertNotNull("An EDocsShipamaxMessage record was created", newDoc.ActiveShipamaxMessage);
			}
		}

		public void TestShipamaxIntegration_ChangeDocTypeToAcceptedFromAccepted()
		{
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DocManagerRegistry.Instance.EnableAccountsPayableInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var newDoc = CreateEDocForShipamaxIntegration("PIN", dataTypeAcceptedByShipamax: true, "PIN");

				MasterFactory.Save();

				AssertNotNull("EDocsShipamaxMessage record was created during save process", newDoc.ActiveShipamaxMessage);

				newDoc.ParentMain.SM_Type = "SHP";
				newDoc.SC_DocType = "CIV";

				MasterFactory.Save();

				AssertShipamaxMessages(newDoc.PK);
			}
		}

		public void TestShipamaxIntegration_ChangeDataTypeToNotAcceptedFromAccepted()
		{
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var newDoc = CreateEDocForShipamaxIntegration("CIV");

				MasterFactory.Save();

				AssertNotNull("EDocsShipamaxMessage record was created during save process", newDoc.ActiveShipamaxMessage);

				newDoc.SC_DataType = "XML";

				MasterFactory.Save();

				AssertNull("There should be no active EDocsShipamaxMessage", newDoc.ActiveShipamaxMessage);

				var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, newDoc.PK);
				var inactiveShipamaxMessage = Factory.LoadTop1<EDocsShipamaxMessage>(query);

				AssertNotNull("There should be one EDocsShipamaxMessage record in the database", inactiveShipamaxMessage);
				AssertEquals("The existing EDocsShipamaxMessage record should be inactive", expected: false, inactiveShipamaxMessage.EM_IsActive);
			}
		}

		public void TestShipamaxIntegration_ChangeDataTypeToAcceptedFromNotAccepted()
		{
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				SetUpShipamaxSupportedDataTypes();

				var newDoc = CreateEDocForShipamaxIntegration("CIV", dataTypeAcceptedByShipamax: false);

				MasterFactory.Save();

				AssertNull("No EDocsShipamaxMessage record was created during save process", newDoc.ActiveShipamaxMessage);

				newDoc.SC_DataType = "pdf";

				MasterFactory.Save();

				AssertNotNull("An EDocsShipamaxMessage record was created", newDoc.ActiveShipamaxMessage);
			}
		}

		public void TestShipamaxIntegration_SC_ImageIsUpdated()
		{
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var newDoc = CreateEDocForShipamaxIntegration("CIV");

				MasterFactory.Save();

				AssertNotNull("EDocsShipamaxMessage record was created during save process.", newDoc.ActiveShipamaxMessage);

				newDoc.SC_ImageData = FivePagesTifBytes;

				MasterFactory.Save();

				AssertShipamaxMessages(newDoc.PK);
			}
		}

		public void TestShipamaxIntegration_NoNewEDIMessageWhenEnableShipamaxRegistryIsOff()
		{
			using (DocManagerRegistry.Instance.SetTemporaryDocParsingRegistryValues(false))
			{
				var newDoc = CreateEDocForShipamaxIntegration("CIV");

				MasterFactory.Save();

				AssertNull("EDocsShipamaxMessage record was not created because registry EnableShipamaxIntegration is off", newDoc.ActiveShipamaxMessage);
			}
		}

		public void TestShipamaxIntegration_ExistingEDIMessageIsDeactivatedWhenEnableShipamaxRegistryIsOff()
		{
			StorageDocsBase newDoc;

			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				newDoc = CreateEDocForShipamaxIntegration("CIV");
				MasterFactory.Save();

				AssertNotNull("EDocsShipamaxMessage record was created during save process", newDoc.ActiveShipamaxMessage);
			}

			using (DocManagerRegistry.Instance.SetTemporaryDocParsingRegistryValues(false))
			{
				newDoc.SC_ImageData = FivePagesTifBytes;
				newDoc.SC_DocType = "MBL";
				newDoc.SC_DataType = "JPG";

				MasterFactory.Save();

				AssertNull("There should be no active EDocsShipamaxMessage because registry EnableShipamaxIntegration is off", newDoc.ActiveShipamaxMessage);

				var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, newDoc.PK);
				var inactiveShipamaxMessage = Factory.LoadTop1<EDocsShipamaxMessage>(query);

				AssertNotNull("There should be one EDocsShipamaxMessage record in the database", inactiveShipamaxMessage);
				AssertEquals("The existing EDocsShipamaxMessage record should be inactive", expected: false, inactiveShipamaxMessage.EM_IsActive);
			}
		}

		public void TestShipamaxIntegration_WhenQualifiedEDocIsDeletedWhenAdding()
		{
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var newDoc = CreateEDocForShipamaxIntegration("CIV");

				newDoc.SC_IsDeleted = true;

				MasterFactory.Save();

				AssertNull("No EDocsShipamaxMessage record was created during save process because the eDoc is marked as deleted", newDoc.ActiveShipamaxMessage);
			}
		}

		public void TestShipamaxIntegration_WhenExistingEDocIsDeletedAndRestored()
		{
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var newDoc = CreateEDocForShipamaxIntegration("CIV");
				MasterFactory.Save();

				AssertNotNull("EDocsShipamaxMessage record was created during save process.", newDoc.ActiveShipamaxMessage);

				newDoc.DeleteQuietly();
				MasterFactory.Save();

				var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, newDoc.PK);
				var inactiveShipamaxMessage = Factory.LoadTop1<EDocsShipamaxMessage>(query);

				AssertNotNull("There should be one EDocsShipamaxMessage record in the database", inactiveShipamaxMessage);
				AssertEquals("The existing EDocsShipamaxMessage record should be inactive", expected: false, inactiveShipamaxMessage.EM_IsActive);

				newDoc.Restore();
				MasterFactory.Save();

				AssertEquals("There should be 1 EDocsShipamaxMessage record", 1, Factory.GetDatabaseCount(typeof(EDIMessage), new ZQuery(EDIMessageSchema.EM_LinkUniqueID, newDoc.PK)));
				AssertEquals("The inactive message should be reactivated again", expected: true, inactiveShipamaxMessage.EM_IsActive);
			}
		}

		public void TestShipamaxIntegration_WhenQualifiedEDocIsPermanentlyDeletedWhenInDatabase()
		{
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var newDoc = CreateEDocForShipamaxIntegration("CIV");

				MasterFactory.Save();

				AssertNotNull("EDocsShipamaxMessage record was created during save process", newDoc.ActiveShipamaxMessage);

				newDoc.Delete();

				MasterFactory.Save();

				var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, newDoc.PK);
				var inactiveShipamaxMessage = Factory.LoadTop1<EDocsShipamaxMessage>(query);

				AssertNotNull("There should be one EDocsShipamaxMessage record in the database", inactiveShipamaxMessage);
				AssertEquals("The existing EDocsShipamaxMessage record should be inactive", expected: false, inactiveShipamaxMessage.EM_IsActive);
			}
		}

		public void TestValidateSC_ImageDataLength_NoValidationIfNoBinaryDataChange()
		{
			SystemDataRegistry.Instance.eDocsMaximumFilesize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);

			var doc = GetNewTestBizOForSave(MasterFactory);
			doc.SC_FileName = "good";
			doc.SC_DataType = "doc";
			doc.SC_ImageData = StorageDocsEncryptionHelper.GetRandomBytes(2000000);

			MasterFactory.Save();

			SystemDataRegistry.Instance.eDocsMaximumFilesize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			doc.SC_FileName = "good still";
			doc.Validation.ValidateSC_ImageData();

			AssertNoErrors("There should be no error for SC_ImageData as validation will not be triggered", doc.SC_ImageDataInfo);
		}

		public void TestShipamaxIntegration_DoNotParseSystemGeneratedFiles()
		{
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var newDoc = CreateEDocForShipamaxIntegration("CIV");
				newDoc.SC_IsSystemGenerated = true;

				MasterFactory.Save();

				AssertNull("No EDocsShipamaxMessage record was created during save process", newDoc.ActiveShipamaxMessage);
			}
		}

		public void TestShipamaxIntegration_DoNotParseIfDenySendForParsing()
		{
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var newDoc = CreateEDocForShipamaxIntegration("CIV");
				var eDocsParsingSupportMock = new Mock<IEDocsParsingSupport>();
				var invoked = false;
				eDocsParsingSupportMock.Setup(x => x.DenySendForParsing(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Callback((Guid pk, string docType, string fileName) =>
				{
					invoked = true;
					AssertEquals(newDoc.PK.ToGuid(), pk);
					AssertEquals(newDoc.SC_FileNameWithExtension, fileName);
					AssertEquals(newDoc.SC_DocType, docType);
				}).Returns(true);
				newDoc.EDocsParsingSupport = eDocsParsingSupportMock.Object;

				MasterFactory.Save();

				AssertEquals("DenySendForParsing must be invoked", true, invoked);
				AssertEquals(EDIMessageStatusList.Codes.Withdrawn, newDoc.ActiveShipamaxMessage.EM_Status);
			}
		}

		public void TestShipamaxIntegration_ParseDisabledIfParsingDeniedInEDocsParsingSupport()
		{
			using var civDisposible = DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var newDoc = CreateEDocForShipamaxIntegration("CIV");
			AssertEquals(true, newDoc.IsParsingEnabled);

			// Set SC_DocType to empty, this will stop initializing IsParsingEnabled when the eDoc is loaded again from DB in the test below.
			newDoc.SC_DocType = string.Empty;
			MasterFactory.Save();

			var eDocsParsingSupportMock = new Mock<IEDocsParsingSupport>();
			eDocsParsingSupportMock.Setup(x => x.DenySendForParsing(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

			var newStorageDoc = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()).GetFactory(newDoc.ParentMain.SM_DB).Load<StorageDocsBase>(newDoc.PK);
			newStorageDoc.EDocsParsingSupport = eDocsParsingSupportMock.Object;
			newStorageDoc.SC_DocType = "CIV";
			AssertEquals("IsParsingEnabled should be false because parsing is denied.", false, newStorageDoc.IsParsingEnabled);

			// Set IsParsingEnabled to true(e.g. users click on the Parsing Enabled checkbox), then change the Doc Type to something not supporting parsing,
			// this will trigger validation to IsParsingEnabled
			newStorageDoc.IsParsingEnabled = true;
			newStorageDoc.SC_DocType = "CAD";
			AssertEquals("IsParsingEnabled should be false because doc type doesn't support parsing.", false, newStorageDoc.IsParsingEnabled);
			AssertNoErrors("IsParsingEnabled should not have validation error because doc type doesn't support parsing.", newStorageDoc.IsParsingEnabledInfo);

			// Change data type to another supported type, which will update and validate IsParsingEnabled while doc type still doesn't support parsing
			newStorageDoc.IsParsingEnabled = true;
			newStorageDoc.SC_DataType = "JPEG";
			AssertEquals("IsParsingEnabled should be false because doc type doesn't support parsing.", false, newStorageDoc.IsParsingEnabled);
			AssertNoErrors("IsParsingEnabled should not have validation error because doc type doesn't support parsing.", newStorageDoc.IsParsingEnabledInfo);

			// Set Doc Type back to CIV, but set data type to something not supporting parsing, this will trigger validation to IsParsingEnabled
			newStorageDoc.SC_DocType = "CIV";
			newStorageDoc.IsParsingEnabled = true;
			newStorageDoc.SC_DataType = "TXT";
			AssertEquals("IsParsingEnabled should be false because data type doesn't support parsing.", false, newStorageDoc.IsParsingEnabled);
			AssertNoErrors("IsParsingEnabled should not have validation error because data type doesn't support parsing.", newStorageDoc.IsParsingEnabledInfo);

			// Change doc type to another supported type, which will update and validate IsParsingEnabled while data type still doesn't support parsing
			newStorageDoc.IsParsingEnabled = true;
			newStorageDoc.SC_DocType = "PKL";
			AssertEquals("IsParsingEnabled should be false because data type doesn't support parsing.", false, newStorageDoc.IsParsingEnabled);
			AssertNoErrors("IsParsingEnabled should not have validation error because data type doesn't support parsing.", newStorageDoc.IsParsingEnabledInfo);
		}

		public void TestShipamaxIntegration_UpdateDocTypeCanUpdateParseType()
		{
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var doc = CreateEDocForShipamaxIntegration("CIV");
				AssertEquals("Parse type should be empty for doc type which supports parsing but its parsing type is disabled", ZString.Empty, doc.ParseType);
			}

			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DocManagerRegistry.Instance.EnableAccountsPayableInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var doc = CreateEDocForShipamaxIntegration("CIV");
				AssertEquals("Parse type should NOT be empty for doc type which supports parsing and its parsing type is enabled", "CIV", doc.ParseType);

				doc.ParentMain.SM_Type = "PIN";
				doc.SC_DocType = "PIN";
				AssertEquals("Setting Doc Type to PIN shold change Parse Type to PIN", "PIN", doc.ParseType);

				doc.SC_DocType = "ACV";
				AssertEquals("Parse type should be empty for doc type which doesn't support parsing", ZString.Empty, doc.ParseType);
			}
		}

		public void TestShipamaxIntegration_UpdateDocTypeCanUpdateIsParsingEnabled()
		{
			SetUpShipamaxSupportedDataTypes();

			using (DocManagerRegistry.Instance.SetTemporaryDocParsingRegistryValues(false))
			{
				var doc = CreateEDocForShipamaxIntegration("CIV");
				AssertEquals("Parse should be disabled when all parse types are disabled", false, doc.IsParsingEnabled);
			}

			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var doc = CreateEDocForShipamaxIntegration("CIV");
				AssertEquals("Parse should be enabled when doc type is enabled for parsing", true, doc.IsParsingEnabled);
				AssertEquals("Parse read only should be false when parse type is not empty", false, doc.IsParsingEnabled_ReadOnly);

				doc.SC_DocType = "ACV";
				AssertEquals("Parse should NOT be enabled when doc type doesn't support parsing", false, doc.IsParsingEnabled);
				AssertEquals("Parse read only should be true when parse type is empty", true, doc.IsParsingEnabled_ReadOnly);
			}
		}

		public void TestShipamaxIntegration_IsParsingEnabledOnNewDocument()
		{
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var doc = CreateEDocForShipamaxIntegration("CIV");
				AssertEquals("Parse should be enabled for a new doc which supports parsing", true, doc.IsParsingEnabled);

				MasterFactory.Save();
				AssertEquals("Parse should still be enabled after Shipamax message is saved", true, doc.IsParsingEnabled);

				doc.IsParsingEnabled = false;
				AssertEquals("Setting IsParsingEnabled shouldn't set HasChanges", false, doc.HasChanges);
				doc.HasChanges = true;
				MasterFactory.Save();
				AssertEquals("The Shipamax message status should be CAN when IsParsingEnabled set to false", "CAN", doc.ActiveShipamaxMessage.EM_Status);

				doc.IsParsingEnabled = true;
				AssertEquals("Setting IsParsingEnabled shouldn't set HasChanges", false, doc.HasChanges);
				doc.HasChanges = true;
				MasterFactory.Save();
				AssertEquals("The Shipamax message status should be QUE when IsParsingEnabled set to true", "QUE", doc.ActiveShipamaxMessage.EM_Status);
			}
		}

		public void TestShipamaxIntegration_InitializeParseTypeAndIsParsingEnabled()
		{
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var doc = CreateEDocForShipamaxIntegration("CIV");
				MasterFactory.Save();
				AssertEquals("HasChanges should be false after save", false, doc.HasChanges);

				doc.ActiveShipamaxMessage.EM_Status = "CAN";
				MasterFactory.Save();

				var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
				var doc1 = documentFactory.GetFactory(doc.ParentMain.SM_DB).Load<StorageDocsBase>(doc.PK);
				AssertEquals("HasChanges should be false after load from DB", false, doc1.HasChanges);

				_ = doc1.ParseType;
				AssertEquals("IsParsingEnabled should be false because EM status is Canceled ", false, doc1.IsParsingEnabled);
			}
		}

		public void TestShipamaxIntegration_ParseType_OnValuedChangedEvent()
		{
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
				var parent = documentFactory.New<StorageMain>();
				parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;

				var file = parent.Documents.AddNew();
				file.SC_FileName = "Test";
				file.SC_ImageData = new byte[] { 1, 2, 3 };
				file.SC_DocType = "MBL";
				file.SC_DataType = "JPG";

				var parseTypeValueChangedCounter = 0;
				file.ParseTypeInfo.ValueChanged += delegate
				{
					parseTypeValueChangedCounter++;
				};

				file.UpdateParseType();
				AssertEquals("ParseType RefreshBinding should not be called because DocType has been initialised to MBL", 0, parseTypeValueChangedCounter);

				file.SC_DocType = "CIV";
				AssertEquals("ParseType RefreshBinding should be called when ParseType is changed", 1, parseTypeValueChangedCounter);

				file.UpdateParseType();
				AssertEquals("ParseType RefreshBinding should not be called when ParseType is not changed", 1, parseTypeValueChangedCounter);
			}
		}

		public void TestShipamaxIntegration_ParseType_OnValuedChangedFromNullToEmpty()
		{
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
				var parent = documentFactory.New<StorageMain>();
				parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;

				var file = parent.Documents.AddNew();
				file.SC_FileName = "Test";
				file.SC_ImageData = new byte[] { 1, 2, 3 };
				file.SC_DataType = "JPG";

				var parseTypeValueChangedCounter = 0;
				file.ParseTypeInfo.ValueChanged += delegate
				{
					parseTypeValueChangedCounter++;
				};

				file.SC_DocType = "MSC";
				AssertEquals("ParseType RefreshBinding should not be called because DocType was changed from null to empty string when setting SC_DocType to 'MSC'", 0, parseTypeValueChangedCounter);
			}
		}

		public void TestShipamaxIntegration_DisableUpdateIsParseEnabledWhenParsingInProgressOrComplete()
		{
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DocManagerRegistry.Instance.EnableAccountsPayableInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var doc = CreateEDocForShipamaxIntegration("CIV");
				MasterFactory.Save();
				AssertEquals("Parse read only should be false if parsing is QUE status", false, doc.IsParsingEnabled_ReadOnly);
				AssertNoExceptionThrown("Setting IsParsEnabled should not throw exception", () => doc.IsParsingEnabled = false);

				doc.ActiveShipamaxMessage.EM_Status = EDIMessageStatusList.Codes.Sent;
				AssertEquals("Parse read only should be true if no changes to parsing related properties and status is in progress", true, doc.IsParsingEnabled_ReadOnly);

				doc.ActiveShipamaxMessage.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;
				AssertEquals("Parse read only should be true if no changes to parsing related properties and status is need review", true, doc.IsParsingEnabled_ReadOnly);

				doc.ActiveShipamaxMessage.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
				AssertEquals("Parse read only should be true if no changes to parsing related properties and status is complete", true, doc.IsParsingEnabled_ReadOnly);

				doc.ParentMain.SM_Type = "PIN";
				doc.SC_DocType = "PIN";
				AssertEquals("Parse read only should be false if parsing is Doc Type changes", false, doc.IsParsingEnabled_ReadOnly);

				var oldDataType = doc.SC_DataType;
				doc.SC_DataType = "ABC";
				AssertEquals("Parse read only should be true if parsing is Data Type changes to unsupported type", true, doc.IsParsingEnabled_ReadOnly);
				doc.SC_DataType = oldDataType;

				doc.SC_ImageData = new byte[] { 1, 2, 3 };
				AssertEquals("Parse read only should be false if parsing is file content changes", false, doc.IsParsingEnabled_ReadOnly);
			}
		}

		public void TestShipamaxIntegration_NotSupportedDocManagerCodes()
		{
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var doc = CreateEDocForShipamaxIntegration("CIV", dataTypeAcceptedByShipamax: true, "IAT");
				doc.ParseType = "CIV";
				MasterFactory.Save();

				AssertNull("Should not create parsing message because doc owner is EDIInterChange", doc.ActiveShipamaxMessage);
			}
		}

		public void TestShipamaxIntegration_SupportedDataTypes()
		{
			var dashServiceMock = new Mock<IDashParametersService>();
			dashServiceMock.SetupGet(x => x.SupportedDataTypes).Returns(new List<String>() { "XYZ", "ABC" });
			ObjectFactory.Substitute(dashServiceMock.Object);

			var doc = GetNewTestBizOForSave(MasterFactory);
			AssertContainsExactElementsInAnyOrder("Supported data types should be XYZ and ABC", new List<string>() { "XYZ", "ABC" }, doc.SupportedDataTypes);
		}

		public void TestShipamaxIntegration_DoNotRecreateParseRequestWhenUnpublishOlderVersionDocs()
		{
			SystemDataRegistry.Instance.UnpublishOlderVersionDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var doc = CreateEDocForShipamaxIntegration("CIV");
			MasterFactory.Save();
			var activeShipamaxMessage = doc.ActiveShipamaxMessage;

			AssertEquals("CIV doc is published by default", true, doc.SC_IsPublished);
			AssertEquals("isParseEnabled field should be set to true after doc is created", true, doc.IsParsingEnabledValue);
			AssertEquals("the shipamax message should be active after created", true, activeShipamaxMessage.EM_IsActive);

			var docFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var doc1 = docFactory.GetFactory(1).Load<StorageDocsBase>(doc.PK);
			AssertEquals("isParseEnabled filed should be still set to true after it's loaded in another document factory", true, doc1.IsParsingEnabledValue);

			StorageDocsBase newDoc = doc1 is StorageFile ? doc1.ParentMain.Files.AddNew() : doc1.ParentMain.Documents.AddNew();
			newDoc.SC_DataType = doc1 is StorageFile ? "PDF" : "JPG";
			newDoc.SC_SM = doc1.SC_SM;
			newDoc.SC_DocType = "CIV";
			newDoc.SC_FileName = "test[2]";
			newDoc.ParentMain.SupersedeOlderVersionDocs(newDoc, doc1.SC_FileNameWithExtension);

			AssertEquals("The old doc should be unpublished by new doc", false, doc1.SC_IsPublished);
			AssertEquals("The old doc has changes as it's unpublished", true, doc1.HasChanges);

			docFactory.Save();

			AssertEquals("The active message created originally should still be active", true, activeShipamaxMessage.EM_IsActive);
			AssertEquals("There should be only 1 EDIMessage for first doc", 1, MasterFactory.GetDatabaseCount(typeof(EDIMessage), new ZQuery(EDIMessageSchema.EM_LinkUniqueID, doc.PK)));
		}

		public void TestValidateSC_ImageDataLength()
		{
			SystemDataRegistry.Instance.eDocsMaximumFilesize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);

			var doc = GetNewTestBizOForSave(MasterFactory);
			doc.SC_FileName = "good";
			doc.SC_DataType = "doc";

			// Limit 1MB but add a 2MB file, it should show error
			doc.SC_ImageData = StorageDocsEncryptionHelper.GetRandomBytes(2000000);
			AssertHasError("Document should have error because blob file size is too big", doc.SC_ImageDataInfo, "The file 'good.doc' is larger than the maximum file size specified by the system (1 MB)");

			// Change limit to 5MB and it should not have error
			SystemDataRegistry.Instance.eDocsMaximumFilesize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
			doc.Validation.ValidateSC_ImageData();

			AssertNoErrors("Document shouldn't have error because blob file size is under registry specified limit", doc.SC_ImageDataInfo);

			MasterFactory.Save();

			// Add a 6MB bytes
			doc.SC_ImageData = StorageDocsEncryptionHelper.GetRandomBytes(6000000);
			doc.Validation.ValidateSC_ImageData();
			AssertHasError("Document should have error because blob file size is too big", doc.SC_ImageDataInfo, "The file 'good.doc' is larger than the maximum file size specified by the system (5 MB)");

			// Change limit to 10MB and it should not have error
			SystemDataRegistry.Instance.eDocsMaximumFilesize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			doc.Validation.ValidateSC_ImageData();
			AssertNoErrors("Document shouldn't have error because blob file size is under registry specified limit", doc.SC_ImageDataInfo);
		}

		public void TestValidateSC_ImageDataLength_S3Enabled_ShouldIgnoreExternalStorageException()
		{
			SystemDataRegistry.Instance.eDocsMaximumFilesize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);

			var doc = GetNewTestBizOForSave(MasterFactory);
			doc.SC_FileName = "good";
			doc.SC_DataType = "doc";

			doc.SC_ImageData = ZBlob.Empty;
			MasterFactory.Save();

			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3WithThrowAmazonS3Exception())
			{
				// to be able to overwrite SC_ImageData, the data should be already loaded from external storage
				doc.currentExternalImageData = StorageDocsEncryptionHelper.GetRandomBytes(100000);
				// now overwrite SC_ImageData with some large data
				AssertNoExceptionThrown(() => doc.SC_ImageData = StorageDocsEncryptionHelper.GetRandomBytes(6000000));

				doc.currentExternalImageData = null;
				doc.Validation.ValidateSC_ImageData();
				AssertHasError("Document should have error because blob file size is too big", doc.SC_ImageDataInfo, "The file 'good.doc' is larger than the maximum file size specified by the system (1 MB)");
			}
		}

		[TestDate(2000, 01, 01, 01, 01, 01)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1055:DoNotUseProcessGetProcesses", Justification = "Testing")]
		public void TestSetImageData_AfterOpenForEdit_WhenS3Enabled()
		{
			using (var syncContext = SynchronizationContextForTest.Enable())
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			{
				var testFile = Path.Combine(Temp.TempPath, "TestFileForPostChange.txt");

				var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
				var doc = documentFactory.NewWithParent(typeof(StorageDocs));
				doc.ParentMain.SM_ParentFK = Factory.NewWithValidTestData<OrgHeader>().PK;
				doc.ParentMain.SM_DB = 1;
				doc.SC_FileName = "TestForPostChange";
				doc.SC_DataType = "txt";
				doc.SC_ImageData = ZBlob.Empty;
				doc.SC_SCK_MasterKey = StorageDocsMasterKeyProvider.GetCurrentMasterKey().PK;
				var dataKey = StorageDocsEncryptionHelper.NewDataKey();
				doc.SC_EncryptedDataKey = StorageDocsEncryptionHelper.Encrypt(dataKey, StorageDocsMasterKeyProvider.GetMasterKey(doc.SC_SCK_MasterKey).SCK_KeyValue);
				doc.ReadOnly = false;
				documentFactory.Save();

				var originalText = "Seek first to understand, then to be understood.";
				var originalRetrievedStream = new MemoryStream(StorageDocsEncryptionHelper.Encrypt(Encoding.ASCII.GetBytes(originalText), dataKey));

				var appendedText = "Put first things first.";

				var persisterMock = new Mock<IExternalPersister>(MockBehavior.Strict);
				persisterMock.Setup(x => x.RetrieveStream(doc.PK)).Returns((originalRetrievedStream, string.Empty));
				var editedAndUploaded = Array.Empty<byte>();
				persisterMock.Setup(x => x.SaveStream(It.IsAny<Stream>(), doc.PK)).Callback((Stream stream, ZGuid guid) => editedAndUploaded = stream.ToByteArray()).Returns((true, null));

				var persisterProviderMock = new Mock<IExternalPersisterProvider>();
				persisterProviderMock.Setup(p => p.GetExternalPersister(Core.Constants.EDocsStorageProviders.Code.S3)).Returns(persisterMock.Object);
				using (ObjectFactory.Substitute(persisterProviderMock.Object))
				{
					try
					{
						using (doc.OpenForEdit())
						{
							persisterMock.Verify(x => x.RetrieveStream(doc.PK), Times.Once);

							Thread.Sleep(1000);
							var tempFileName = doc.TempFileName;
							var openProcess = Process.GetProcesses().FirstOrDefault(p => p.MainWindowTitle.Contains(Path.GetFileNameWithoutExtension(doc.TempFileName)));
							try
							{
								File.AppendAllText(doc.TempFileName, appendedText);
								Assert("Expected watcher_changed event to be raised", syncContext.WaitAndExecuteCallbacks(TimeSpan.FromSeconds(1)));
								AssertEquals("doc.HasChanges should be true", true, doc.HasChanges);

								AssertEquals("Data key already exists before SetImageData", true, doc.HasDataKey);
								doc.SetImageData();
								AssertEquals("MasterKey should not be changed", StorageDocsMasterKeyProvider.GetCurrentMasterKey().PK, doc.SC_SCK_MasterKey);
								AssertEquals("DataKey should not be changed", dataKey, StorageDocsEncryptionHelper.DecryptDataKey(doc.SC_EncryptedDataKey, StorageDocsMasterKeyProvider.GetMasterKey(doc.SC_SCK_MasterKey).SCK_KeyValue));
							}
							finally
							{
								openProcess?.Kill();

								if (File.Exists(tempFileName))
								{
									File.Delete(tempFileName);
								}
							}
						}

						// Add 61 days to rotate master key
						TestDateAttribute.AddDays(DocManagerRegistry.Instance.EDocsEncryptionMasterKeyRotationPeriod.Value + 1);
						var newMasterKey = StorageDocsMasterKeyProvider.GetCurrentMasterKey();

						var previousEncryptedDataKey = doc.SC_EncryptedDataKey;
						doc.SaveToExternalStorage();
						AssertEquals("MasterKey should be updated to the new one", newMasterKey.PK, doc.SC_SCK_MasterKey);
						AssertNotEquals("DataKey should be re-encrypted with the new master key", previousEncryptedDataKey, doc.SC_EncryptedDataKey);
						AssertEquals("Datakey should be the same", dataKey, StorageDocsEncryptionHelper.DecryptDataKey(doc.SC_EncryptedDataKey, newMasterKey.SCK_KeyValue));

						// Try decrypt with the original datakey and verified uploaded after edit
						var decryptedData = StorageDocsEncryptionHelper.DecryptDocument(editedAndUploaded, dataKey, doc.PK.ToGuid());
						var finalData = (byte[])ZCompressor.GetUncompressedVersion(decryptedData, StorageDocsSchema.Constants.SC_ImageData);

						AssertEquals("Uploaded text is edited text", originalText + appendedText, Encoding.ASCII.GetString(finalData));
					}
					finally
					{
						if (File.Exists(testFile))
						{
							File.Delete(testFile);
						}
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1055:DoNotUseProcessGetProcesses", Justification = "Testing")]
		public void TestEditEdocsShouldTriggerChangesWhenEnableS3Storage()
		{
			using (var syncContext = SynchronizationContextForTest.Enable())
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "S3"))
			using (SystemDataRegistry.Instance.DocManagerStorageBucketName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test"))
			using (SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://s3.test.local"))
			using (SystemDataRegistry.Instance.EDocsStorageAccess.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "KeyId=dude;Secret=top"))
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(false, true))
			{
				var testFile = Path.Combine(Temp.TempPath, "TestFileForPostChange.txt");

				var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
				var doc = documentFactory.NewWithParent(typeof(StorageDocs));
				doc.ParentMain.SM_ParentFK = Factory.NewWithValidTestData<OrgHeader>().PK;
				doc.ParentMain.SM_DB = 1;
				doc.SC_FileName = "TestForPostChange";
				doc.SC_DataType = "txt";
				doc.SC_ImageData = ZBlob.Empty;
				doc.ReadOnly = false;
				documentFactory.Save();
				Globals.IsTest_ForTest.Value = false;

				try
				{
					using (doc.OpenForEdit())
					{
						Thread.Sleep(1000);
						var tempFileName = doc.TempFileName;
						var openProcess = Process.GetProcesses().FirstOrDefault(p => p.MainWindowTitle.Contains(Path.GetFileNameWithoutExtension(doc.TempFileName)));
						try
						{
							File.AppendAllText(doc.TempFileName, "123");
							Assert("Expected watcher_changed event to be raised", syncContext.WaitAndExecuteCallbacks(TimeSpan.FromSeconds(1)));
							AssertEquals("doc.HasChanges should be true", true, doc.HasChanges);
						}
						finally
						{
							openProcess?.Kill();

							if (File.Exists(tempFileName))
							{
								File.Delete(tempFileName);
							}
						}
					}
				}
				finally
				{
					Globals.IsTest_ForTest.Value = true;
					if (File.Exists(testFile))
					{
						File.Delete(testFile);
					}
				}
			}
		}

		public void TestMessageShouldBeCorrectWhenIOErrorInWinzor()
		{
			var testFile = Path.Combine(Temp.TempPath, "TestFileForPostChange.txt");

			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var doc = GetNewTestBizOForSave(documentFactory);
			doc.ParentMain.SM_ParentFK = Factory.NewWithValidTestData<OrgHeader>().PK;
			doc.ParentMain.SM_DB = 1;
			doc.SC_FileName = "TestForPostChange";
			doc.SC_DataType = "txt";
			doc.SC_ImageData = ZBlob.Empty;
			doc.SC_SCK_MasterKey = StorageDocsMasterKeyProvider.GetCurrentMasterKey().PK;

			var dataKey = StorageDocsEncryptionHelper.NewDataKey();
			doc.SC_EncryptedDataKey = StorageDocsEncryptionHelper.Encrypt(dataKey, StorageDocsMasterKeyProvider.GetMasterKey(doc.SC_SCK_MasterKey).SCK_KeyValue);
			doc.ReadOnly = false;
			documentFactory.Save();

			var remoteFileMock = new Mock<IRemoteFile>();
			remoteFileMock.Setup(mock => mock.FetchFileData()).Returns(() => null);
			remoteFileMock.Setup(mock => mock.RemoteFilesSupported).Returns(true);
			remoteFileMock.Setup(mock => mock.GetIsOpenStatus()).Returns(false);
			remoteFileMock.Setup(mock => mock.GetDoesExistStatus()).Returns(false);
			remoteFileMock.Setup(mock => mock.FileName).Returns("TestForPostChange");
			remoteFileMock.Setup(mock => mock.Open()).Returns(true);
			ObjectFactory.Substitute(remoteFileMock.Object);

			var remoteChannelMock = new Mock<IRemoteChannel>();
			remoteChannelMock.Setup(mock => mock.RemoteVersion).Returns(Version.Parse("0.0.0.0"));
			ObjectFactory.Substitute(remoteChannelMock.Object);

			var isWinzorHistoryValue = Globals.IsWinzor;
			var isTestForTestHistoryValue = Globals.IsTest_ForTest.Value;
			try
			{
				Globals.IsTest_ForTest.Value = false;
				Globals.IsWinzor = true;
				using (doc.OpenForEdit())
				{
					var success = doc.SetImageData();
					Assert("Set image data should be false when there is an IO error！", !success);
					var message = string.Format("Failed to save document {0} to database because it is empty. This could happen because of network or IO failure. Please check and try to modify and save again.", doc.SC_FileName);
					var messageEqual = message.Equals(UnitTestUserNotification.Instance.LastMessage.Text);
					Assert("SetEmptyValue pop up message should be correct!", messageEqual);
				}
			}
			finally
			{
				Globals.IsTest_ForTest.Value = isTestForTestHistoryValue;
				Globals.IsWinzor = isWinzorHistoryValue;
			}
		}

		public void TestStorageDocsBase_PrinterDetails()
		{
			var doc = (StorageDocsBase)MasterFactory.New(GetExpectedBusinessObjectType());
			var iDeliverable = doc as IDeliverable;
			AssertNotNull(iDeliverable);

			AssertNull("edocs doesn't support printerdetails.", iDeliverable.PrinterDetails);
		}

		public void TestInvalidSC_ParentID()
		{
			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var doc = documentFactory.NewWithParent(typeof(StorageDocs));
			doc.ParentMain.SM_ParentFK = Factory.NewWithValidTestData<OrgHeader>().PK;
			doc.ParentMain.SM_DB = 2;
			doc.SC_FileName = "TestForViewLocalFile";
			doc.SC_DataType = "xls";
			doc.SC_ImageData = TestXlsBytes;
			doc.ReadOnly = false;
			doc.SC_ParentID = ZGuid.Invalid;
			AssertEquals(ZGuid.Invalid, doc.SC_ParentID);

			documentFactory.Save();

			AssertEquals(ZGuid.Empty, doc.SC_ParentID);
		}

		public void TestFileByteSize_WhenNewEDocIsAdded()
		{
			// Arrange
			var storageDoc = GetBusinessObjectForTestingDeliveryInfo(null);
			storageDoc.SC_ImageData = Encoding.Default.GetBytes(new string('A', 1024 * 300));

			// Assert
			AssertEquals(307_200, storageDoc.SC_UncompressedSize);
			AssertEquals("300KB", storageDoc.HumanReadableAttachmentSize);
		}

		public void TestFileByteSize_WhenSC_UncompressedIsEmptyAndSC_ImageDataHasData()
		{
			// Arrange
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";

			var storageDoc = GetBusinessObjectForTestingDeliveryInfo(org);
			storageDoc.SC_ImageData = Encoding.Default.GetBytes(new string('A', 1024 * 300));
			storageDoc.SC_UncompressedSize = 0;
			MasterFactory.Save();

			// Assert
			AssertEquals(0, storageDoc.SC_UncompressedSize);
			AssertEquals("300KB", storageDoc.HumanReadableAttachmentSize);
			AssertEquals(307_200, storageDoc.SC_UncompressedSize);
			AssertEquals(false, storageDoc.HasChanges);
		}

		public void TestFileByteSize_WhenSC_UncompressedIsEmptyAndSC_ImageDataIsEmpty()
		{
			// Arrange
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";
			var storageDoc = GetBusinessObjectForTestingDeliveryInfo(org);
			storageDoc.SC_ImageData = ZBlob.Empty;
			storageDoc.SC_UncompressedSize = 0;
			MasterFactory.Save();

			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			{
				var persisterProviderMock = new Mock<IExternalPersisterProvider>();
				var persisterMock = new Mock<IExternalPersister>(MockBehavior.Strict);

				persisterProviderMock.Setup(p => p.GetExternalPersister(Core.Constants.EDocsStorageProviders.Code.S3)).Returns(persisterMock.Object);
				persisterMock.Setup(x => x.GetObjectSize(storageDoc.PK)).Returns(307_200);

				using (ObjectFactory.Substitute(persisterProviderMock.Object))
				{
					// Assert
					AssertEquals(0, storageDoc.SC_UncompressedSize);
					AssertEquals("300KB", storageDoc.HumanReadableAttachmentSize);
					AssertEquals(0, storageDoc.SC_UncompressedSize);
				}
			}
		}

		public void TestFileByteSize_WhenSC_UncompressedIsNotEmpty()
		{
			// Arrange
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";
			var storageDoc = GetBusinessObjectForTestingDeliveryInfo(org);
			storageDoc.SC_ImageData = ZBlob.Empty;
			storageDoc.SC_UncompressedSize = 307_200;
			MasterFactory.Save();

			// Act
			var newStorageDoc = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()).Load<StorageDocs>(storageDoc.PK);

			// Assert
			AssertNotNull(newStorageDoc);
			AssertEquals(307_200, newStorageDoc.SC_UncompressedSize);
			AssertEquals("300KB", newStorageDoc.HumanReadableAttachmentSize);
		}

		[DeveloperOnlyTest]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1055:DoNotUseProcessGetProcesses", Justification = "Testing")]
		public void TestDoNotPostChangeWhenOpenFileForLocalView()
		{
			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var doc = documentFactory.NewWithParent(typeof(StorageDocs));
			doc.ParentMain.SM_ParentFK = Factory.NewWithValidTestData<OrgHeader>().PK;
			doc.ParentMain.SM_DB = 2;
			doc.SC_FileName = "TestForViewLocalFile";
			doc.SC_DataType = "xls";
			doc.SC_ImageData = TestXlsBytes;
			doc.ReadOnly = false;

			documentFactory.Save();

			Globals.IsTest_ForTest.Value = false;
			try
			{
				using (doc.OpenForEdit())
				{
					Thread.Sleep(2000);
					var openProcess = Process.GetProcesses().FirstOrDefault(p => p.MainWindowTitle.Contains(Path.GetFileNameWithoutExtension(doc.TempFileName)));
					try
					{
						AssertNotNull(openProcess);
						AssertEquals(false, doc.HasChanges);
					}
					finally
					{
						openProcess.Kill();
						Thread.Sleep(2000);

						AssertEquals(false, doc.HasChanges);
					}
				}
			}
			finally
			{
				Globals.IsTest_ForTest.Value = true;
				if (File.Exists(doc.TempFileName))
				{
					File.Delete(doc.TempFileName);
				}
			}
		}

		public void TestFileChangedActionForTempFile()
		{
			using var syncContext = SynchronizationContextForTest.Enable();
			var doc = MasterFactory.NewWithParent(typeof(StorageFile));
			doc.ParentMain.SM_ParentFK = Factory.NewWithValidTestData<OrgHeader>().PK;
			doc.ParentMain.SM_DB = 1;
			doc.SC_FileName = "TestForPostChange";
			doc.SC_DataType = "txt";
			doc.SC_ImageData = ZBlob.Empty;
			MasterFactory.Save();

			using (Globals.TemporaryOverrideForIsTest(false))
			using (doc.OpenForEdit())
			{
				Thread.Sleep(1000);
				var tempFileName = doc.TempFileName;
				var openProcess = ProcessLocator.Instance.GetCurrentUserVisibleProcesses().FirstOrDefault(p => p.MainWindowTitle.Contains(Path.GetFileNameWithoutExtension(tempFileName)));

				try
				{
					File.AppendAllText(tempFileName, "123");

					AssertEquals("Expected watcher_changed event to be raised", expected: true, syncContext.WaitAndExecuteCallbacks(TimeSpan.FromSeconds(1)));
					AssertEquals("doc.HasChanges should be true", true, doc.HasChanges);
					AssertEquals("Property IsSC_ImageDataOutOfSync should be set true", expected: true, doc.IsSC_ImageDataOutOfSync);
					AssertEquals("Property ShouldSetImageDataForAll should be set true", expected: true, doc.ParentMain.ShouldSetImageDataForAll);

					doc.SetImageData();

					AssertEquals("Property IsSC_ImageDataOutOfSync should be set false", expected: false, doc.IsSC_ImageDataOutOfSync);
				}
				finally
				{
					openProcess?.Kill();

					if (File.Exists(tempFileName))
					{
						File.Delete(tempFileName);
					}
				}
			}
		}

		public void TestSaveFileWhenOpenWithoutException()
		{
			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var doc = GetNewTestBizOForSave(documentFactory);
			doc.ParentMain.SM_ParentFK = Factory.NewWithValidTestData<OrgHeader>().PK;
			doc.ParentMain.SM_DB = 1;
			doc.SC_FileName = "TestFile";
			doc.SC_DataType = "txt";
			doc.SC_ImageData = ZBlob.Empty;
			doc.SC_SCK_MasterKey = StorageDocsMasterKeyProvider.GetCurrentMasterKey().PK;

			var dataKey = StorageDocsEncryptionHelper.NewDataKey();
			doc.SC_EncryptedDataKey = StorageDocsEncryptionHelper.Encrypt(dataKey, StorageDocsMasterKeyProvider.GetMasterKey(doc.SC_SCK_MasterKey).SCK_KeyValue);
			doc.ReadOnly = false;
			documentFactory.Save();

			var remoteFileMock = new Mock<IRemoteFile>();
			remoteFileMock.Setup(mock => mock.RemoteFilesSupported).Returns(true);
			remoteFileMock.Setup(mock => mock.GetIsOpenStatus()).Returns(true);
			remoteFileMock.Setup(mock => mock.GetDoesExistStatus()).Returns(true);
			remoteFileMock.Setup(mock => mock.Open()).Returns(true);
			ObjectFactory.Substitute(remoteFileMock.Object);
			var remoteChannelMock = new Mock<IRemoteChannel>();
			remoteChannelMock.Setup(mock => mock.RemoteVersion).Returns(Version.Parse("99.99.0.0"));
			ObjectFactory.Substitute(remoteChannelMock.Object);

			using (doc.OpenForEdit())
			{
				var success = false;
				AssertNoExceptionThrown(() => success = doc.SetImageData());
				Assert("Set image data should be true when there is no error!", success);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1055:DoNotUseProcessGetProcesses", Justification = "Testing")]
		public void TestPostChangeWhenChangeTextFile()
		{
			var testFile = Path.Combine(Temp.TempPath, "TestFileForPostChange.txt");

			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var doc = documentFactory.NewWithParent(typeof(StorageDocs));
			doc.ParentMain.SM_ParentFK = Factory.NewWithValidTestData<OrgHeader>().PK;
			doc.ParentMain.SM_DB = 2;
			doc.SC_FileName = "TestForPostChange";
			doc.SC_DataType = "txt";
			doc.SC_ImageData = ZBlob.Empty;
			doc.ReadOnly = false;
			documentFactory.Save();
			Globals.IsTest_ForTest.Value = false;

			try
			{
				using (var syncContext = SynchronizationContextForTest.Enable())
				using (doc.OpenForEdit())
				{
					Thread.Sleep(1000);
					var tempFileName = doc.TempFileName;
					var openProcess = Process.GetProcesses().FirstOrDefault(p => p.MainWindowTitle.Contains(Path.GetFileNameWithoutExtension(doc.TempFileName)));
					try
					{
						File.AppendAllText(doc.TempFileName, "123");
						Assert("Expected watcher_changed event to be raised", syncContext.WaitAndExecuteCallbacks(TimeSpan.FromSeconds(1)));
						AssertEquals("doc.HasChanges should be true", true, doc.HasChanges);
						((StorageDocs)doc).WasOpenInExternalEditor = true;
						doc.SC_ImageData = ZBlob.Empty;
						AssertNullOrEmpty(doc.TempFileName);

						AssertNoExceptionThrown("Save text files twice will not throw exception.",
						() =>
						{
							File.AppendAllText(tempFileName, "123");
							Thread.Sleep(1000);
						});
					}
					finally
					{
						openProcess?.Kill();

						if (File.Exists(tempFileName))
						{
							File.Delete(tempFileName);
						}
					}
				}
			}
			finally
			{
				Globals.IsTest_ForTest.Value = true;
				if (File.Exists(testFile))
				{
					File.Delete(testFile);
				}
			}
		}

		public void TestDocumentEventFailedInTransactionSaving()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";

			var docType = Factory.NewWithValidTestData<RefDocType>();
			docType.RT_DocType = "EVE";
			docType.RT_Desc = "Test Doc Type";
			docType.RT_ReferenceType = "ALL";
			docType.RT_SE_NKDocumentReceivedEvent = AutoEvents.MiscellaneousEventCode;
			docType.RT_LogMacro = "Hello <@data.Name>|FOO=<@data.Foo>|BAR=<@data.Bar>";
			Factory.Save();

			var document = GetNewTestBizO(MasterFactory, Core.Constants.DocManagerCodes.Organisation);
			document.ParentMain.SM_ParentFK = org.PK;
			document.SC_DocType = docType.RT_DocType;
			MasterFactory.Save();

			var shouldHasWarningMessage = UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("There was a problem creating the reference for the event which was added when adding this document. This may effect any associated triggers. You may need to amend the event's reference manually.");

			Assert("there should not have warning", !shouldHasWarningMessage);
		}

		public void TestCustomEventOnDocType()
		{
			const string arbitraryEventCode = AutoEvents.MiscellaneousEventCode;

			var docType = Factory.NewWithValidTestData<RefDocType>();
			docType.RT_SE_NKDocumentReceivedEvent = arbitraryEventCode;
			docType.RT_LogMacro = "FOUR=<1 + 3>|BLAH=SOMETHING";
			docType.RT_ReferenceType = "ALL";

			var parent = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			var document = GetNewTestBizO(MasterFactory.GetFactory(1), Core.Constants.DocManagerCodes.Organisation);
			document.ParentMain.SM_ParentFK = parent.PK;
			document.SC_DocType = docType.RT_DocType;

			MasterFactory.Save();
			MasterFactory.Save();

			var customLog = parent.GetLogs().Find(log => log.SL_SE_NKEvent == arbitraryEventCode).Single();
			AssertEquals(arbitraryEventCode, customLog.SL_SE_NKEvent); // I know its redundant, but it shows intent
			AssertEquals(document.PK + "|FOUR=4|BLAH=SOMETHING", customLog.SL_Reference);
		}

		public void TestNoDocumentReceivedEventWasCreatedWhenDeleteDocument()
		{
			const string eventCode = "DCA";

			var docType = Factory.NewWithValidTestData<RefDocType>();
			docType.RT_DocType = "DOR";
			docType.RT_ReferenceType = "ALL";

			var parent = Factory.NewWithValidTestData<OrgHeader>();

			var document = GetNewTestBizO(MasterFactory, Core.Constants.DocManagerCodes.Organisation);
			document.ParentMain.SM_ParentFK = parent.PK;
			document.SC_DocType = docType.RT_DocType;
			Factory.Save();

			Assert("Should be no DCA event", !parent.GetLogs().Find(log => log.SL_SE_NKEvent == eventCode).Any());

			docType.RT_SE_NKDocumentReceivedEvent = eventCode;
			document.SC_IsDeleted = true;
			Factory.Save();

			Assert("Should be no DCA event", !parent.GetLogs().Find(log => log.SL_SE_NKEvent == eventCode).Any());
		}

		public void TestCustomEventOnDocType_DocTypeIsChangedIntoOneWithoutCustomEvent()
		{
			const string arbitraryEvent = AutoEvents.BookedCode;

			var firstDocType = Factory.NewWithValidTestData<RefDocType>();
			firstDocType.RT_SE_NKDocumentReceivedEvent = arbitraryEvent;
			firstDocType.RT_ReferenceType = "ALL";

			var secondDocType = Factory.NewWithValidTestData<RefDocType>();
			secondDocType.RT_ReferenceType = "ALL";

			var parent = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			var document = GetNewTestBizO(MasterFactory, Core.Constants.DocManagerCodes.Organisation);
			document.ParentMain.SM_ParentFK = parent.PK;
			document.SC_DocType = firstDocType.RT_DocType;
			document.SC_DocType = secondDocType.RT_DocType;

			MasterFactory.Save();

			Assert("The custom event should no longer exist", !parent.GetLogs().Find(log => log.SL_SE_NKEvent == arbitraryEvent).Any());
		}

		public void TestCustomEventOnDocType_CancelAddingEDocRemovesTheEvent()
		{
			var docType = CreateDocTypeWithLog(AutoEvents.MiscellaneousEventCode);
			var parent = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var document = GetNewTestBizO(MasterFactory.GetFactory(1), Core.Constants.DocManagerCodes.Organisation);
			document.ParentMain.SM_ParentFK = parent.PK;
			document.SC_DocType = docType.RT_DocType;

			document.Delete(); // Delete before it save

			MasterFactory.Save();

			AssertEquals("Custom event should no longer exist", false, parent.GetLogs().Find(log => log.SL_SE_NKEvent == AutoEvents.MiscellaneousEventCode).Any());
		}

		public void TestCustomEventOnDocType_DeleteEDocsCancelTheEvent()
		{
			var docType = CreateDocTypeWithLog(AutoEvents.MiscellaneousEventCode);
			var parent = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var document = GetNewTestBizO(MasterFactory.GetFactory(1), Core.Constants.DocManagerCodes.Organisation);
			document.ParentMain.SM_ParentFK = parent.PK;
			document.SC_DocType = docType.RT_DocType;

			MasterFactory.Save();
			Assert("Custom event should exist", parent.GetLogs().Find(log => log.SL_SE_NKEvent == AutoEvents.MiscellaneousEventCode).Any());

			document.Delete();
			MasterFactory.Save();

			var customLog = parent.GetLogs().Find(log => log.SL_SE_NKEvent == AutoEvents.MiscellaneousEventCode).Single();
			AssertEquals("Previous event should be cancelled", true, customLog.IsCancelled);
		}

		public void TestCustomEventOnDocType_DeleteDuplicatedOrphanEDocsNotCancelTheEvent()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.CreateDbDropExisting(adminConnection, Db.DatabaseName + "_SD002");
			}
			var docType = CreateDocTypeWithLog(AutoEvents.MiscellaneousEventCode);
			var parent = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var document = GetNewTestBizO(MasterFactory.GetFactory(2), Core.Constants.DocManagerCodes.Organisation);
			document.ParentMain.SM_ParentFK = parent.PK;
			document.SC_DocType = docType.RT_DocType;

			MasterFactory.Save();
			Assert("Custom event should exist", parent.GetLogs().Find(log => log.SL_SE_NKEvent == AutoEvents.MiscellaneousEventCode).Any());

			// Add duplicated orphan document to DB 1
			var testDbHelper = new DocManagerDBHelperTestClass();
			var insertDuplicatedStorageDocsSql = $@"
INSERT [{testDbHelper.GetDatabaseName(1)}]..StorageDocs (SC_PK, SC_SM, SC_DocType, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc, SC_ImageData)
VALUES ('{document.PK}', '{document.ParentMain.PK}', '{document.SC_DocType}',  sysutcdatetime(), sysutcdatetime(), sysutcdatetime(), 0x010203);";
			Db.Connection.ExecuteNonQuery(insertDuplicatedStorageDocsSql);

			var orphanDoc = MasterFactory.GetFactory(1).Load<StorageDocsBase>(document.PK);
			AssertEquals("Orpha Doc should be in DB 1", 1, ((NumberedBusinessObjectFactory)orphanDoc.Factory).DBNumber);
			AssertNotEquals("Orphan Doc is an orphan", orphanDoc.ParentMain.SM_DB, ((NumberedBusinessObjectFactory)orphanDoc.Factory).DBNumber);

			// delete duplicated orphan in DB 1 (and turn off Refresh first so it won't broadcast the deletion to other factories)
			orphanDoc.Factory.RefreshEnabled = false;
			orphanDoc.Delete();
			MasterFactory.Save();

			var customLog = parent.GetLogs().Find(log => log.SL_SE_NKEvent == AutoEvents.MiscellaneousEventCode).Single();
			AssertEquals("Previous event should not be cancelled when deleting orphan doc", false, customLog.IsCancelled);

			// Verify with loaded docs and existing factory (with RefreshEnabled off, these should pass)
			AssertEquals("Orphan should be deleted", true, orphanDoc.IsDeleted);
			AssertEquals("Non-orphan should not be deleted", false, document.IsDeleted);
			document = MasterFactory.GetFactory(2).Load<StorageDocsBase>(document.PK);
			AssertEquals("Non-orphan (reloaded) should not be deleted", false, document.IsDeleted);

			// Verify again with a new factory
			var newMasterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			AssertNull("Orphan can't be reload from DB 1", newMasterFactory.GetFactory(1).Load<StorageDocsBase>(document.PK));
			var reloadDoc = newMasterFactory.GetFactory(2).Load<StorageDocsBase>(document.PK);
			AssertNotNull("Non-orphan doc can be reloaded from DB 1", reloadDoc);
			AssertEquals("Non-orphan doc is not deleted", false, reloadDoc.IsDeleted);
		}

		public void TestCustomEventOnDocType_DeleteOrphanEDocsNotCancelTheEvent()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.CreateDbDropExisting(adminConnection, Db.DatabaseName + "_SD002");
			}
			var docType = CreateDocTypeWithLog(AutoEvents.MiscellaneousEventCode);
			var parent = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var document = GetNewTestBizO(MasterFactory.GetFactory(2), Core.Constants.DocManagerCodes.Organisation);
			document.ParentMain.SM_DB = 1; // parent not pointing to DB2
			document.ParentMain.SM_ParentFK = parent.PK;
			document.SC_DocType = docType.RT_DocType;

			MasterFactory.Save();
			Assert("Custom event should exist", parent.GetLogs().Find(log => log.SL_SE_NKEvent == AutoEvents.MiscellaneousEventCode).Any());

			AssertNotEquals("Document is an orphan", document.ParentMain.SM_DB, ((NumberedBusinessObjectFactory)document.Factory).DBNumber);
			document.Delete();
			MasterFactory.Save();

			var customLog = parent.GetLogs().Find(log => log.SL_SE_NKEvent == AutoEvents.MiscellaneousEventCode).Single();
			AssertEquals("Previous event should not be cancelled when deleting orphan document", false, customLog.IsCancelled);
		}

		RefDocType CreateDocTypeWithLog(string eventCode)
		{
			var docType = Factory.NewWithValidTestData<RefDocType>();
			docType.RT_SE_NKDocumentReceivedEvent = eventCode;
			docType.RT_LogMacro = "FOUR=<1 + 3>|BLAH=SOMETHING";
			docType.RT_ReferenceType = "ALL";
			return docType;
		}

		public void TestBadMacroWillNotifyPostMasters()
		{
			const string arbitraryEventCode = AutoEvents.MiscellaneousEventCode;

			var postMasters = Factory.LoadFromNaturalKey<GlbGroup>(GlbGroupSchema.GG_Code, "PMG");
			var staff = postMasters.Staff.AddNew();
			staff.FillWithValidTestData();
			staff.GS_EmailAddress = "not@blank.com";

			var docType = Factory.NewWithValidTestData<RefDocType>();
			docType.RT_DocType = "BLA";
			docType.RT_SE_NKDocumentReceivedEvent = arbitraryEventCode;
			docType.RT_LogMacro = "FOUR=<@data.PK * @data.PK>"; // Some invalid macro that compiles fine
			docType.RT_ReferenceType = "ALL";

			var parent = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			var document = GetNewTestBizO(MasterFactory, Core.Constants.DocManagerCodes.Organisation);
			document.ParentMain.SM_ParentFK = parent.PK;
			document.SC_DocType = docType.RT_DocType;

			MasterFactory.Save();
			MasterFactory.Save();

			var customLog = parent.GetLogs().Find(log => log.SL_SE_NKEvent == arbitraryEventCode).Single();
			AssertEquals("Even though the macro was bad, we should still add the event", arbitraryEventCode, customLog.SL_SE_NKEvent); // I know its redundant, but it shows intent

			var email = Env.OutgoingMailManager.EmailsCreated.Single();
			CombineAssertions(() =>
			{
				AssertContains("Subject", "Error evaluating macro for document type " + docType.RT_DocType, email.Subject);

				var errorMessage =
	@"There was a problem evaluating the Log Macro for the document type 'BLA' with the category of 'ALL'.
The macro is: 'FOUR=<@data.PK * @data.PK>'
The errors are: 
Runtime: Cannot perform binary 'Multiply' operation between 'ZGuid' and 'ZGuid'.

The MIS event has been created, but the reference may not be what is expected. This may effect any triggers you have for that event. Please repair the problematic macro.";

				AssertContains("Should tell the PMG what happened", errorMessage, email.Body);
			});
		}

		public void TestCustomEventOnDocType_ChangingDocTypeCancelsEvent()
		{
			const string firstArbitraryEvent = AutoEvents.BookedCode;
			const string secondArbitraryEvent = AutoEvents.MiscellaneousEventCode;

			var firstDocType = Factory.NewWithValidTestData<RefDocType>();
			firstDocType.RT_SE_NKDocumentReceivedEvent = firstArbitraryEvent;
			firstDocType.RT_ReferenceType = "ALL";

			var secondDocType = Factory.NewWithValidTestData<RefDocType>();
			secondDocType.RT_SE_NKDocumentReceivedEvent = secondArbitraryEvent;
			secondDocType.RT_ReferenceType = "ALL";

			var parent = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			var document = GetNewTestBizO(MasterFactory, Core.Constants.DocManagerCodes.Organisation);
			document.ParentMain.SM_ParentFK = parent.PK;
			document.SC_DocType = firstDocType.RT_DocType;

			MasterFactory.Save();
			MasterFactory.Save();

			var secondFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var reloaded = (StorageDocsBase)secondFactory.Load(document.GetType(), document.PK);

			reloaded.SC_DocType = secondDocType.RT_DocType;

			secondFactory.Save();
			secondFactory.Save();

			var parentLogs = secondFactory.Load<OrgHeader>(parent.PK).GetLogs();
			var customLog = parentLogs.Find(log => log.SL_SE_NKEvent == secondArbitraryEvent).Single();
			Assert("When the new doc type has a custom event we should add that new event", !customLog.IsCancelled);

			customLog = parentLogs.Find(log => log.SL_SE_NKEvent == firstArbitraryEvent).Single();
			Assert("Since we changed the doc type we should cancel our original custom event", customLog.IsCancelled);
		}

		public void TestDocumentTypeSecurity()
		{
			var org = MasterFactory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Address1 = "1111111111";
			org.MainAddress.OA_Code = "zxcfg";
			var org2 = MasterFactory.NewWithValidTestData<OrgHeader>();
			org2.MainAddress.OA_Address1 = "1111111112";
			org2.MainAddress.OA_Code = "zxcfh";

			Env.Security.GetDocumentTypeUploadCheckPoint("ACV").IsAllowed = false;
			var storageDoc = GetNewTestBizO(MasterFactory, Core.Constants.DocManagerCodes.Organisation);
			storageDoc.ParentMain.SM_ParentFK = org.PK;
			storageDoc.ParentMain.SM_DB = 1;
			storageDoc.SC_DocType = "ACV";
			storageDoc.SC_FileName = "File name";
			Assert(!storageDoc.SC_DocType_List.GetAllCodes().Contains("ACV"));
			AssertHasErrors(storageDoc.SC_DocTypeInfo);
			storageDoc.SC_DocType = "1RM";
			AssertNoErrors(storageDoc.SC_DocTypeInfo);

			MasterFactory.Save();

			Env.Security.GetDocumentTypeUploadCheckPoint("ACV").IsAllowed = true;
			Env.Security.GetDocumentTypeUploadCheckPoint("1RM").IsAllowed = false;
			var storageDoc2 = GetNewTestBizO(MasterFactory, Core.Constants.DocManagerCodes.Organisation);
			storageDoc2.ParentMain.SM_ParentFK = org2.PK;
			storageDoc2.ParentMain.SM_DB = 1;
			storageDoc2.SC_DocType = "ACV";
			storageDoc2.SC_FileName = "File name 2";
			AssertNoErrors(storageDoc2.SC_DocTypeInfo);

			MasterFactory.Save();

			var masterFactory2 = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var storageDocReloaded = (StorageDocsBase)masterFactory2.Load(storageDoc.GetType(), storageDoc.PK);
			var storageDoc2Reloaded = (StorageDocsBase)masterFactory2.Load(storageDoc2.GetType(), storageDoc2.PK);
			Assert(storageDocReloaded.ReadOnly);
			Assert(!storageDoc2Reloaded.ReadOnly);
			Assert(storageDocReloaded.SC_DocType_List.GetAllCodes().Contains("ACV"));
			Assert(storageDocReloaded.SC_DocType_List.GetAllCodes().Contains("1RM"));
			Assert(storageDoc2Reloaded.SC_DocType_List.GetAllCodes().Contains("ACV"));
			Assert(!storageDoc2Reloaded.SC_DocType_List.GetAllCodes().Contains("1RM"));
			storageDocReloaded.RunPreSaveValidation();
			storageDoc2Reloaded.RunPreSaveValidation();
			AssertNoErrors(storageDocReloaded);
			AssertNoErrors(storageDoc2Reloaded);

			masterFactory2.Save();
		}

		[ExpectNoExceptions]
		public void TestSC_SMIsNull()
		{
			var org = MasterFactory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Address1 = "1111111111";
			org.MainAddress.OA_Code = "zxcfg";
			var main = MasterFactory.New<StorageMain>();
			main.SM_ParentFK = org.PK;
			main.SM_DB = 1;
			var doc = (StorageDocsBase)MasterFactory.New(GetExpectedBusinessObjectType());
			doc.SC_SM = main.PK;
			MasterFactory.Save();

			doc.SC_SM = ZGuid.Empty;
			doc.SC_IsDeleted = true;
			var docPK = doc.PK;
			MasterFactory.Save();

			var fac = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			main = fac.Load<StorageMain>(main.PK);
			main.eDocs.Load();
			AssertEquals(0, main.eDocs.Count);
		}

		[ExpectNoExceptions]
		public void TestCreateNewFactoryDoesNotThrow()
		{
			var doc = (StorageDocsBase)MasterFactory.New(GetExpectedBusinessObjectType());
			doc.SC_SM = ZGuid.Empty;
			var factory = doc.CreateNewFactory() as NumberedBusinessObjectFactory;
			AssertNotNull(factory);
			AssertEquals(0, factory.DBNumber);

			var main = MasterFactory.NewWithValidTestData<StorageMain>();
			main.SM_DB = 2;
			doc.SC_SM = main.PK;
			factory = doc.CreateNewFactory() as NumberedBusinessObjectFactory;
			AssertNotNull(factory);
			AssertEquals(2, factory.DBNumber);
		}

		public void TestSC_ParentIDForDeleted()
		{
			var doc = (StorageDocsBase)MasterFactory.New(GetExpectedBusinessObjectType());
			doc.SC_IsDeleted = false;
			AssertNoErrors(doc);

			doc.Validation.ValidateSC_Date();
			AssertNoErrors(doc);

			doc.SC_Date = ZDateTime.Invalid; // What the? Part 1:  Here it assigns SC_Date an "Invalid" value
			AssertHasErrors(doc.SC_DateInfo);

			doc.SC_IsDeleted = true;
			doc.SC_Date = ZDateTime.Today;
			doc.Validation.ValidateSC_Date(); // What the? Part 2: Validation will surely add an error to an Invalid SC_Date
			AssertNoErrors(doc); // What the? Part 3: Expecting no errors after validating an invalid date?  What the...?
		}

		public void TestSC_DateForDeleted()
		{
			var doc = (StorageDocsBase)MasterFactory.New(GetExpectedBusinessObjectType());
			doc.SC_IsDeleted = false;
			AssertNoErrors(doc);

			doc.Validation.ValidateSC_ParentID();
			AssertNoErrors(doc);

			doc.SC_ParentID = ZGuid.Invalid; // What the? Part 1:  Here it assigns ParentID an "Invalid" value
			AssertHasErrors(doc.SC_ParentIDInfo);

			doc.SC_IsDeleted = true;
			doc.SC_ParentID = ZGuid.NewZGuid();  // This line I added to make it work
			doc.Validation.ValidateSC_ParentID(); // What the? Part 2: Validation will surely add an error to an Invalid ParentID
			AssertNoErrors(doc); // What the? Part 3: Expecting no errors after validating an invalid ParentID?  What the...?
		}

		public void TestCanDelete()
		{
			var newFile = StorageFile.NewWithParent_DEBUG(MasterFactory);
			Assert(newFile.CanDelete);

			var attachment = MasterFactory.New<EDIMessageAttach>();
			attachment.EG_StorageDocsGuid = newFile.PK;
			Assert(!newFile.CanDelete);
			AssertEquals("There are EDI Messages that have this eDocs file as attachment. You cannot delete this eDocs now.", newFile.ReasonForNotAbleToDelete);
		}

		public void TestDeleteParentIfUnallocated()
		{
			var doc = (StorageDocsBase)MasterFactory.New(GetExpectedBusinessObjectType());
			var main = MasterFactory.NewWithValidTestData<StorageMain>();
			AssertNoExceptionThrown("Calling storageDoc.DeleteParentIfUnallocated", doc.DeleteParentIfUnallocated);

			doc.SC_SM = main.PK;
			var unallocated = doc.ParentMain == null;
			AssertNoExceptionThrown("Calling storageDoc.DeleteParentIfUnallocated", doc.DeleteParentIfUnallocated);
			if (!unallocated)
			{
				Assert(doc.ParentMain.IsDeleted);
			}
		}

		public void TestSaveWhenDeleteParent()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var storageMain = Factory.NewWithValidTestData<StorageMain>();
			storageMain.SM_ParentFK = orgHeader.PK;
			Factory.Save();
			var contents = new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 };
			storageMain.AddFileOrDocument(contents, new AddFileOrDocumentDto { FileName = "document.pdf", DocumentType = "ABC" });

			storageMain.Delete();
			AssertNoExceptionThrown(() => { Factory.Save(); });
		}

		public void TestNotifyReadByUserHandlesNullParent()
		{
			var storageDoc = (StorageDocsBase)Factory.New(GetExpectedBusinessObjectType());
			AssertNoExceptionThrown("Calling storageDoc.NotifyReadByUser()", storageDoc.NotifyReadByUser);
		}

		public void TestNotifyReadByUserDoesNotSaveReadRelaltedDocumentLogForUnsavedParent()
		{
			var fDocTypeWithForceUserToRead = Factory.New<RefDocType>();
			fDocTypeWithForceUserToRead.RT_ForceUserToRead = true;
			fDocTypeWithForceUserToRead.RT_ReferenceType = Core.Constants.ReferenceTypes.All;
			fDocTypeWithForceUserToRead.RT_DocType = "FUR";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var storageMain = Factory.NewWithValidTestData<StorageMain>();
			storageMain.SM_ParentFK = orgHeader.PK;
			storageMain.SM_Type = "ORG";
			var contents = new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 };
			var storageDoc = storageMain.AddFileOrDocument(contents, new AddFileOrDocumentDto { FileName = "document.pdf", DocumentType = "FUR" });
			AssertNoExceptionThrown("Calling storageDoc.NotifyReadByUser() should not write ReadRelatedDocs log", storageDoc.NotifyReadByUser);
		}

		public void TestSC_DocType_List_WithoutInactive()
		{
			var newDocType = MasterFactory.New(typeof(RefDocType)) as RefDocType;
			newDocType.RT_DocType = "GGG";
			newDocType.RT_ReferenceType = Core.Constants.DocManagerCodes.Organisation;
			newDocType.RT_Desc = "Test Description";
			newDocType.RT_IsActive = false;
			newDocType.RT_IsPublished = true;
			newDocType.RT_SaveVersions = true;

			var storageDoc = GetNewTestBizO(MasterFactory, Core.Constants.DocManagerCodes.Organisation);
			Assert("Should contain some doctypes", storageDoc.SC_DocType_List.Count > 0);
			AssertCollectionNotContains("Should not contain inactive doctype", storageDoc.SC_DocType_List);
		}

		public void TestSC_DocType_List_ShouldContainPrivates()
		{
			var storageDoc = GetNewTestBizO(MasterFactory, Core.Constants.DocManagerCodes.Shipment);
			var list = storageDoc.SC_DocType_List;
			AssertCollectionContains("SC_DocType_List should contain internal doc types otherwise validation fails in StorageFileValidation.CheckSC_DocType",
				Core.Constants.RefDocTypes.InternallyCreatedPrivateDocument, list.ToArray().Select(d => d.Code));
		}

		public void TestSupportsNotes()
		{
			AssertEquals("Should always return false", false, DocumentOrFile.SupportsNotes);
		}

		public void TestIsObsolete()
		{
			var storageDoc = GetNewTestBizOForSave(MasterFactory, Core.Constants.DocManagerCodes.Organisation);
			Assert("should be false for newly created doc", !storageDoc.IsObsolete);
			storageDoc.SC_DocType = "GGG";
			storageDoc.SC_Desc = "";
			Assert("should be false for changed doc", !storageDoc.IsObsolete);

			MasterFactory.Save();

			Assert("shoule be true for doc in database e.g obsolete", storageDoc.IsObsolete);
		}

		public void TestIsAutoLogged()
		{
			Assert("Should always return false since we shouldn't add logs for StorageDocs to StmALog table", !DocumentOrFile.IsAutoLogged);
		}

		public void TestLogsFactory()
		{
			var logsFactory = (BusinessObjectFactory)typeof(StorageDocsBase).GetProperty("LogsFactory", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(DocumentOrFile, null);
			AssertEquals("Should always be the same instance as the MasterFactory passed in", MasterFactory, logsFactory);
		}

		public void TestSaveToTempFile()
		{
			var testDocument = GetNewTestBizO(MasterFactory);
			testDocument.SC_ImageData = FivePagesTifBytes;

			var tempFile = string.Empty;
			try
			{
				tempFile = testDocument.SaveToTempFile();
				AssertEquals("File should exist", true, File.Exists(tempFile));
				Assert("File length should be greater than 0", new FileInfo(tempFile).Length > 0);
				AssertEquals("File extension should be SC_DataType or the default of 'TIF'", testDocument.SC_DataType.IsEmpty ? "TIF" : (string)testDocument.SC_DataType, new FileInfo(tempFile).Extension.Trim('.').ToUpper());
				AssertEquals("TempFileName property on the BizO should have the filename that was created", tempFile, testDocument.TempFileName);
			}
			finally
			{
				DeleteFileIfExists(tempFile);
			}
		}

		public void TestSaveToTempFileWithPDF()
		{
			var testDocument = GetNewTestBizO(MasterFactory);
			testDocument.SC_ImageData = resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF");
			testDocument.SC_DataType = "PDF";

			var tempFile = string.Empty;
			try
			{
				tempFile = testDocument.SaveToTempFile();
				AssertEquals("File should exist", true, File.Exists(tempFile));
				Assert("File length should be greater than 0", new FileInfo(tempFile).Length > 0);
				AssertEquals("File extension should be PDF", "pdf", new FileInfo(tempFile).Extension.Trim('.'));
				AssertEquals("TempFileName property on the BizO should have the filename that was created", tempFile, testDocument.TempFileName);
			}
			finally
			{
				DeleteFileIfExists(tempFile);
			}
		}

		public void TestSaveToTempFileWithGivenFile()
		{
			var testDocument = GetNewTestBizO(MasterFactory);
			testDocument.SC_ImageData = FivePagesTifBytes;

			var tempFile = Temp.GetTempFileName();
			try
			{
				AssertEquals("FileLength is 0", 0, new FileInfo(tempFile).Length);
				testDocument.SaveToTempFile(tempFile);
				AssertEquals("File should exist now", true, File.Exists(tempFile));
				Assert("File should have contents", new FileInfo(tempFile).Length > 0);
				AssertEquals("TempFileName property on the bizO should have the filename passed in", tempFile, testDocument.TempFileName);
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

		public void TestSaveToStream()
		{
			var testDocument = GetNewTestBizO(MasterFactory);
			testDocument.SC_ImageData = FivePagesTifBytes;

			var tempFile = Temp.GetTempFileName();
			using (var stream = new FileStream(tempFile, FileMode.Append))
			{
				AssertEquals("Precondition: File stream should be empty before calling SaveToStream", 0, stream.Length);
				testDocument.SaveToStream(stream);
				Assert("File Stream should not be empty after calling SaveToStream", stream.Length > 0);
			}
			File.Delete(tempFile);
		}

		void AssertIsMalwareRun(StorageDocsBase testDocument)
		{
			AssertEquals("IsMalware run", true, isMalwareRun);
			AssertEquals("Scanned payload", scanningBytes, testDocument.SC_ImageData);
			AssertEquals("Scanned file name", scanningFile, testDocument.SC_FileNameWithExtension);
		}

		void AssertIsMalwareDoesNotRun()
		{
			AssertEquals("IsMalware run", false, isMalwareRun);
		}

		void AssertNotDetectedVirusScanStatus(StorageDocsBase testDocument)
		{
			AssertGreaterThan("Document ImageData should be not empty", testDocument.SC_ImageData.Length, 0);
			AssertEquals("Document scan result should be NotDetected", Constants.VirusScanResult.NotDetected, testDocument.LastVirusScanResult);
		}

		void AssertNotScanVirusScanStatus(StorageDocsBase testDocument)
		{
			AssertGreaterThan("Document ImageData should be not empty", testDocument.SC_ImageData.Length, 0);
			AssertEquals("Document scan result should be NotScan", Constants.VirusScanResult.NotScan, testDocument.LastVirusScanResult);
		}

		void AssertDetectedVirusScanStatus(StorageDocsBase testDocument)
		{
			AssertExceptionThrown(typeof(VirusDetectedException), expectedMessage,
			() =>
			{
				_ = testDocument.SC_ImageData;
			});
			AssertEquals("Document scan result should be Detected", Constants.VirusScanResult.Detected, testDocument.LastVirusScanResult);
		}

		bool isMalwareRun;
		byte[] scanningBytes;
		string scanningFile;
		int scanningCounter;
		public void TestNormalFile_WithScanning()
		{
			using (DocManagerRegistry.Instance.EnableEDocsVirusScanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var testDocument = GetNewTestBizO(MasterFactory);
				TestWithMockVirusNotDetected(
				() =>
				{
					AssertNoExceptionThrown(() => testDocument.SC_ImageData = FivePagesTifBytes);

					AssertNotDetectedVirusScanStatus(testDocument);
					AssertIsMalwareRun(testDocument);

					MasterFactory.Save();
				});

				TestDocumentReloadAndDoNotRescan(testDocument);
			}
		}

		public void TestNotScannedFile_WithScanning()
		{
			using (DocManagerRegistry.Instance.EnableEDocsVirusScanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var testDocument = GetNewTestBizO(MasterFactory);
				TestWithMockVirusNotDetected(
				() =>
				{
					AssertNoExceptionThrown(() => testDocument.SC_ImageData = FivePagesTifBytes);
					MasterFactory.Save();

					AssertNotScanVirusScanStatus(testDocument);
					AssertIsMalwareDoesNotRun();
				});

				using (DocManagerRegistry.Instance.EnableEDocsVirusScanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					TestWithMockVirusNotDetected(
					() =>
					{
						testDocument.Reload();
						AssertNotDetectedVirusScanStatus(testDocument);
						AssertIsMalwareRun(testDocument);
						MasterFactory.Save();
					});

					TestDocumentReloadAndDoNotRescan(testDocument);
				}
			}
		}

		public void TestNormalFile_WithScanning_SuspendScanningWhenGetter()
		{
			using (DocManagerRegistry.Instance.EnableEDocsVirusScanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var testDocument = GetNewTestBizO(MasterFactory);
				TestWithMockVirusNotDetected(
				() =>
				{
					AssertNoExceptionThrown(() => testDocument.SC_ImageData = FivePagesTifBytes);
					AssertEquals("IsMalware run", true, isMalwareRun);
					AssertEquals("Scanning Counter", 1, scanningCounter);

					MasterFactory.Save();

					testDocument.Reload();
					// to simulate the close and open the eDocs module again
					testDocument.LastVirusScanResult = Constants.VirusScanResult.NotScan;
					isMalwareRun = false;
					AssertNoExceptionThrown(() => testDocument.SC_ImageData = TestXlsBytes);

					AssertEquals("IsMalware run", true, isMalwareRun);
					AssertEquals("Scanning Counter", 2, scanningCounter);

					MasterFactory.Save();
				});
			}
		}

		void TestWithMockVirusNotDetected(AnonymousMethod codeToRun)
		{
			var moqAmsiContext = new Mock<IAmsiContext>();
			var moqAmsiSession = new Mock<IAmsiSession>();

			isMalwareRun = false;
			scanningBytes = null;
			scanningFile = null;
			scanningCounter = 0;

			moqAmsiSession.Setup(session => session.IsMalware(It.IsAny<byte[]>(), It.IsAny<string>()))
				.Callback((byte[] payload, string fileName) =>
				{
					scanningBytes = payload;
					scanningFile = fileName;
					isMalwareRun = true;
					scanningCounter++;
				})
				.Returns(() =>
				{
					return false;
				});
			moqAmsiContext.Setup(context => context.CreateSession()).Returns(moqAmsiSession.Object);
			using (ObjectFactory.Substitute(moqAmsiContext.Object))
			{
				codeToRun();
			}
		}

		void TestDocumentReloadAndDoNotRescan(StorageDocsBase testDocument)
		{
			TestWithMockVirusNotDetected(
			() =>
			{
				testDocument.Reload();

				AssertNotDetectedVirusScanStatus(testDocument);
				AssertIsMalwareDoesNotRun();
			});
		}

		void TestWithMockVirusDetected(AnonymousMethod codeToRun)
		{
			var moqAmsiContext = new Mock<IAmsiContext>();
			var moqAmsiSession = new Mock<IAmsiSession>();

			isMalwareRun = false;
			scanningBytes = null;
			scanningFile = null;
			scanningCounter = 0;

			moqAmsiSession.Setup(session => session.IsMalware(It.IsAny<byte[]>(), It.IsAny<string>()))
				.Callback((byte[] payload, string fileName) =>
				{
					scanningBytes = payload;
					scanningFile = fileName;
					isMalwareRun = true;
					scanningCounter++;
				})
				.Returns(() =>
				{
					return true;
				});
			moqAmsiContext.Setup(context => context.CreateSession()).Returns(moqAmsiSession.Object);
			using (ObjectFactory.Substitute(moqAmsiContext.Object))
			{
				codeToRun();
				AssertEquals("IsMalware run", true, isMalwareRun);
			}
		}

		public void TestSaveVirusFile_WithScanning_Moq()
		{
			using (DocManagerRegistry.Instance.EnableEDocsVirusScanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var testDocument = GetNewTestBizO(MasterFactory);
				TestWithMockVirusDetected(
				() =>
				{
					AssertExceptionThrown(typeof(VirusDetectedException), expectedMessage,
					() =>
					{
						testDocument.SC_ImageData = ZBlob.FromUTF8("Moq Test Virus");
					});
				});

				MasterFactory.Save();

				var eDocs = MasterFactory.Load(typeof(StorageDocs), testDocument.PK);
				AssertEquals("should not saved -- return null", null, eDocs);
			}
		}

		public void TestSaveVirusFile_WithoutScanning_ThenWithScanning_Moq()
		{
			using (DocManagerRegistry.Instance.EnableEDocsVirusScanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var testDocument = GetNewTestBizO(MasterFactory);
				TestWithMockVirusDetected(
				() =>
				{
					testDocument.SC_FileName = "VirusTest";
					testDocument.SC_DataType = "txt";
					AssertNoExceptionThrown(() => testDocument.SC_ImageData = ZBlob.FromUTF8("Moq Test Virus"));

					AssertNotScanVirusScanStatus(testDocument);

					MasterFactory.Save();
					testDocument.Reload();

					AssertNotScanVirusScanStatus(testDocument);

					using (DocManagerRegistry.Instance.EnableEDocsVirusScanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
					{
						testDocument.Reload();
						testDocument.SC_FileName = "VirusTest-Rename";
						AssertDetectedVirusScanStatus(testDocument);
						AssertEquals("Document FileName should not be loaded from DB", "VirusTest-Rename", testDocument.SC_FileName);

						testDocument.Reload();
						AssertDetectedVirusScanStatus(testDocument);
						AssertEquals("Document FileName from DB should not be updated", "VirusTest", testDocument.SC_FileName);
						AssertGreaterThan("Document ImageDataFromDb should be not empty", testDocument.SC_ImageDataFromDb.Length, 0);
					}
				});
			}
		}

		public void TestUpdateWithVirusFile_WithScanning_Moq()
		{
			using (DocManagerRegistry.Instance.EnableEDocsVirusScanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var testDocument = GetNewTestBizO(MasterFactory);
				testDocument.SC_FileName = "VirusTest";
				testDocument.SC_DataType = "png";
				testDocument.SC_ImageData = FivePagesTifBytes;
				MasterFactory.Save();

				AssertNotDetectedVirusScanStatus(testDocument);

				TestWithMockVirusDetected(
				() =>
				{
					var ex = AssertExceptionThrown<VirusDetectedException>("Virus Detected Exception should be thrown", expectedMessage,
					() =>
					{
						testDocument.SC_ImageData = ZBlob.FromUTF8("Moq Test Virus");
					});
					AssertEquals("The file \"VirusTest.png\" has been detected with virus and therefore cannot be saved or opened.", ex.VirusDetectedFriendlyMessage);
				});

				testDocument.Reload();
				AssertNotDetectedVirusScanStatus(testDocument);
			}
		}

		public void TestSetStreamVirusFile_WithScanning_Moq()
		{
			using (DocManagerRegistry.Instance.EnableEDocsVirusScanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var testDocument = GetNewTestBizO(MasterFactory);
				testDocument.SC_FileName = "VirusTest";
				testDocument.SC_DataType = "txt";
				TestWithMockVirusDetected(
				() =>
				{
					var ex = AssertExceptionThrown<VirusDetectedException>("Virus Detected Exception should be thrown", expectedMessage,
					() =>
					{
						testDocument.SetSC_ImageDataSource(new StreamSource(new MemoryStream(new byte[] { 1, 1, 1, 1, 1, 1, 1 })));
					});
					AssertEquals("The file \"VirusTest.txt\" has been detected with virus and therefore cannot be saved or opened.", ex.VirusDetectedFriendlyMessage);
				});
			}
		}

		public void TestGetStreamVirusFile_WithScanning_Moq()
		{
			using (DocManagerRegistry.Instance.EnableEDocsVirusScanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var testDocument = GetNewTestBizO(MasterFactory);
				testDocument.SC_ImageData = ZBlob.FromUTF8("Moq Test Virus");
				MasterFactory.Save();

				TestWithMockVirusDetected(
				() =>
				{
					AssertExceptionThrown(typeof(VirusDetectedException), expectedMessage,
					() =>
					{
						var newFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
						testDocument = (StorageDocsBase)newFactory.Load(testDocument.GetType(), testDocument.PK);
						_ = testDocument.GetSC_ImageDataReader();
					});
				});
			}
		}

		public void TestScanningVirusWithLargeFile()
		{
			using (DocManagerRegistry.Instance.EnableEDocsVirusScanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var testDocument = GetNewTestBizO(MasterFactory);

				const long k = 1024;
				const long m = 1024 * k;
				const long fileSize = 50 * m;
				var c = new Random(Guid.NewGuid().GetHashCode());
				var fileBuffer = new byte[fileSize];

				c.NextBytes(fileBuffer);

				AssertNoExceptionThrown(() => testDocument.SC_ImageData = fileBuffer);
			}
		}

		const string expectedMessage = "The file has been detected with virus and therefore cannot be saved or opened.";

		byte[] EicarString()
		{
			// this is WTG virus string
			var eicarString = "eijB8fm_gpDCFii0))7%rJ~=BzDiWTGVirusTest!YRF]7@uHUZ+]knv!*PMxrD:";
			var eicar = new UTF8Encoding(true).GetBytes(eicarString);
			return eicar;
		}

		public void TestSaveVirusFile_WithScanning()
		{
			using (DocManagerRegistry.Instance.EnableEDocsVirusScanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var testDocument = GetNewTestBizO(MasterFactory);

				AssertExceptionThrown(typeof(VirusDetectedException), expectedMessage,
				() =>
				{
					testDocument.SC_ImageData = EicarString();
				});
			}
		}

		public void TestSaveVirusFile_WithoutScanning_ThenWithScanning()
		{
			using (DocManagerRegistry.Instance.EnableEDocsVirusScanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var testDocument = GetNewTestBizO(MasterFactory);
				testDocument.SC_ImageData = EicarString();

				AssertNotScanVirusScanStatus(testDocument);

				MasterFactory.Save();
				testDocument.Reload();

				AssertNotScanVirusScanStatus(testDocument);

				using (DocManagerRegistry.Instance.EnableEDocsVirusScanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					testDocument.Reload();
					AssertDetectedVirusScanStatus(testDocument);
				}
			}
		}

		public void TestUpdateWithVirusFile_WithScanning()
		{
			using (DocManagerRegistry.Instance.EnableEDocsVirusScanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var testDocument = GetNewTestBizO(MasterFactory);
				testDocument.SC_ImageData = FivePagesTifBytes;
				MasterFactory.Save();

				AssertNotDetectedVirusScanStatus(testDocument);

				AssertExceptionThrown(typeof(VirusDetectedException), expectedMessage,
				() =>
				{
					testDocument.SC_ImageData = EicarString();
				});

				testDocument.Reload();
				AssertNotDetectedVirusScanStatus(testDocument);
			}
		}

		public void TestParentMain()
		{
			DocumentOrFile.SC_SM = ZGuid.Empty;
			AssertNull("ParentMain should be null when the Foreign Key hasn't been set", DocumentOrFile.ParentMain);

			var main = MasterFactory.New(typeof(StorageMain)) as StorageMain;
			DocumentOrFile.SC_SM = main.PK;
			AssertEquals("Once FK has been set, ParentMain should be the same instance", main, DocumentOrFile.ParentMain);

			var anotherMain = MasterFactory.New(typeof(StorageMain)) as StorageMain;
			DocumentOrFile.SC_SM = anotherMain.PK;
			AssertEquals("Changed Parent PK, should have a different ParentMain object", anotherMain, DocumentOrFile.ParentMain);
		}

		[TestDate(2013, 09, 21, 16, 00, 00)]
		public void TestParentMain_LastActivityIsUpdatedOnStorageDocInsertUpdateDelete()
		{
			var anotherDocumentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var testDocument = GetNewTestBizOForSave(MasterFactory.GetFactory(1));
			var storageMainPK = testDocument.ParentMain.PK;

			TestDateAttribute.Date = new DateTime(2013, 09, 21, 16, 01, 00);
			MasterFactory.Save();
			AssertEquals(TestDateAttribute.Date, anotherDocumentFactory.Load<StorageMain>(storageMainPK).SM_LastActivity);

			TestDateAttribute.Date = new DateTime(2013, 09, 21, 16, 02, 00);
			testDocument.SC_Desc = "phew~~~~";
			MasterFactory.Save();
			AssertEquals(TestDateAttribute.Date, anotherDocumentFactory.Load<StorageMain>(storageMainPK).SM_LastActivity);

			TestDateAttribute.Date = new DateTime(2013, 09, 21, 16, 03, 00);
			testDocument.DeleteQuietly();
			MasterFactory.Save();
			AssertEquals(TestDateAttribute.Date, anotherDocumentFactory.Load<StorageMain>(storageMainPK).SM_LastActivity);

			TestDateAttribute.Date = new DateTime(2013, 09, 21, 16, 04, 00);
			testDocument.Restore();
			MasterFactory.Save();
			AssertEquals(TestDateAttribute.Date, anotherDocumentFactory.Load<StorageMain>(storageMainPK).SM_LastActivity);
		}

		public void TestMasterFactory()
		{
			var masterFactory = DocumentOrFile.MasterFactory;

			AssertNotNull("Master Factory should never be null", masterFactory);

			var factoryOne = MasterFactory.GetFactory(2);
			var anotherTestObject = GetNewTestBizO(factoryOne);
			var anotherMasterFactory = anotherTestObject.MasterFactory;
			AssertEquals("MasterFactory should be the correct DocumentFactory instance even when created with a child factory", MasterFactory, anotherMasterFactory);

			AssertEquals("Factory 1", 1, ((NumberedBusinessObjectFactory)DocumentOrFile.Factory).DBNumber);
			AssertEquals("Factory 2", 2, ((NumberedBusinessObjectFactory)anotherTestObject.Factory).DBNumber);
		}

		public void TestSC_AddingUser()
		{
			var parent1 = Factory.NewWithValidTestData<OrgHeader>();
			var parent2 = Factory.NewWithValidTestData<OrgHeader>();
			var originalStaffInitials = GlbStaff.CurrentUser.GS_Code.ToString();
			var testObject = GetNewTestBizOForSave(MasterFactory);
			testObject.ParentMain.SM_ParentFK = parent1.PK;
			MasterFactory.Save();
			AssertEquals("Object's adding user should be the currently logged in user", GlbStaff.CurrentUser.GS_Code, testObject.SC_AddingUser.Trim());

			var newStaff = MasterFactory.New<GlbStaff>();
			newStaff.GS_FullName = "New Staff";
			newStaff.GS_LoginName = "NewStaff";
			newStaff.GS_Code = "NST";
			MasterFactory.Save();

			using (Env.SetTemporaryUserContext("NewStaff", GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("currently logged in staff member changed", "NST", GlbStaff.CurrentUser.GS_Code);

				var testObject2 = GetNewTestBizO(MasterFactory);
				testObject2.ParentMain.SM_ParentFK = parent2.PK;
				MasterFactory.Save();
				AssertEquals("new document should have new user's initials", GlbStaff.CurrentUser.GS_Code, testObject2.SC_AddingUser);
				AssertEquals("new document should have new user's initials", "NST", testObject2.SC_AddingUser);

				AssertEquals("Original document kept old Login details", originalStaffInitials.Trim(), testObject.SC_AddingUser.Trim());
			}
		}

		public void TestIsAutologged()
		{
			var testObject = GetNewTestBizOForSave(MasterFactory);
			Assert("New object is not autologged", !testObject.IsAutoAdminBusinessObjectLoggerEnabled);

			testObject.HasChanges = true;
			MasterFactory.Save();
			Assert("Saved object is not autologged", !testObject.IsAutoAdminBusinessObjectLoggerEnabled);
		}

		public void TestAddedLogIsNullWhenIsAutologgedIsFalse()
		{
			var testObject = GetNewTestBizOForSave(MasterFactory);
			MasterFactory.Save();

			var docInOtherFactory = (new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory())).Load<StorageDocsBase>(testObject.PK);
			Assert(docInOtherFactory.Logs.AddedLog == null);
		}

		public void TestSC_LastEditingUser()
		{
			var orgQuery = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "f");
			var org1 = MasterFactory.LoadTop1(typeof(OrgHeader), orgQuery) as OrgHeader;

			orgQuery = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "R");
			var org2 = MasterFactory.LoadTop1(typeof(OrgHeader), orgQuery) as OrgHeader;

			var originalStaffInitials = GlbStaff.CurrentUser.GS_Code.ToString();

			var testObject = GetNewTestBizO(MasterFactory);
			testObject.ParentMain.SM_ParentFK = org1.PK;
			MasterFactory.Save();

			AssertEquals("Object's adding user should be the currently logged in user", GlbStaff.CurrentUser.GS_Code, testObject.SC_LastEditingUser.Trim());

			var newStaff = MasterFactory.New<GlbStaff>();
			newStaff.GS_FullName = "New Staff";
			newStaff.GS_LoginName = "NewStaff";
			newStaff.GS_Code = "NST";
			MasterFactory.Save();

			using (Env.SetTemporaryUserContext("NewStaff", GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("currently logged in staff member changed", "NST", GlbStaff.CurrentUser.GS_Code);

				var testObject2 = GetNewTestBizO(MasterFactory);
				testObject2.ParentMain.SM_ParentFK = org2.PK;
				MasterFactory.Save();
				AssertEquals("new document should have new user's initials", GlbStaff.CurrentUser.GS_Code, testObject2.SC_LastEditingUser);
				AssertEquals("new document should have new user's initials", "NST", testObject2.SC_LastEditingUser);

				AssertEquals("Original document kept old Login details", originalStaffInitials, testObject.SC_LastEditingUser);
				testObject.SC_Date = ZDateTime.Now;
				MasterFactory.Save();
				AssertEquals("After edit, the original document should have the new user's staff initials", "NST", testObject.SC_LastEditingUser);
			}
		}

		public void TestSetDefaults()
		{
			var newBranch = SetupUkrainianCompanyAndBranch();
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, newBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var testDoc = GetNewTestBizO(MasterFactory);
				var utcTime = new TimeFactory().CurrentUtcDateTime;

				//https://docs.microsoft.com/en-us/dotnet/api/system.datetime?view=net-5.0#comparing-for-equality-within-tolerance
				var timeSpanInSeconds = Math.Abs((utcTime - testDoc.SC_Date).TotalSeconds);
				Assert("Attaching UTC time", timeSpanInSeconds < 1);

				timeSpanInSeconds = Math.Abs((new TimeFactory().GetLocalTimeFromUtc(utcTime) - testDoc.SC_DateLocalBranchTime).TotalSeconds);
				Assert("Attaching Local time should depend on a branch", timeSpanInSeconds < 1);
			}
		}

		GlbBranch SetupUkrainianCompanyAndBranch()
		{
			var ukraine = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Ukraine));
			var ukraiainCompany = Factory.NewWithValidTestData<GlbCompany>();
			ukraiainCompany.GC_RN_NKCountryCode = ukraine.Code;
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_GC = ukraiainCompany.PK;
			Factory.Save();
			return newBranch;
		}

		public void TestIdentifier()
		{
			var testDoc = GetNewTestBizO(MasterFactory);
			AssertEquals(testDoc.PK.ToString(), testDoc.Identifier);
		}

		public void TestDocType()
		{
			var testDoc = GetNewTestBizO(MasterFactory);
			testDoc.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			testDoc.SC_DocType = "CIV";

			var query = new ZQuery(RefDocTypeSchema.RT_DocType, "CIV");
			query.AddToFilter(JoinCondition.And, RefDocTypeSchema.RT_ReferenceType, SQLComparisonOperator.Equal, Core.Constants.ReferenceTypes.SupplyChainLogistics);
			var loadedDocType = MasterFactory.LoadTop1<RefDocType>(query);
			AssertEquals("Correct DocType should be on loaded up", loadedDocType, testDoc.DocType);
			testDoc.SC_IsPublished = false;
			testDoc.SC_IsPublished = true;
			AssertEquals(testDoc.SC_DocTypeInfo.HasWarnings(), !loadedDocType.RT_IsPublished);
			testDoc.SC_IsPublished = false;
			AssertEquals(testDoc.SC_DocTypeInfo.HasWarnings(), loadedDocType.RT_IsPublished);

			testDoc.SC_DocType = "DDR";
			AssertNull("invalid doc type for that reference type code means null returned", testDoc.DocType);

			testDoc.SC_IsPublished = true;
			Assert(!testDoc.SC_DocTypeInfo.HasWarnings());
			testDoc.SC_IsPublished = false;
			Assert(!testDoc.SC_DocTypeInfo.HasWarnings());
		}

		public void TestSC_DocType()
		{
			var newDocType = MasterFactory.New<RefDocType>();
			newDocType.RT_DocType = "ZZZ";
			newDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			newDocType.RT_Desc = "Test Description";
			newDocType.RT_IsActive = true;
			newDocType.RT_IsPublished = true;
			newDocType.RT_SaveVersions = true;

			var testDocument = GetNewTestBizO(MasterFactory, Core.Constants.DocManagerCodes.Organisation);

			testDocument.SC_DocType = "LLL";
			Assert("Invalid doctype code, description will update to empty", testDocument.SC_Desc.IsEmpty);
			Assert("Invalid doctype code, IsPublished stays default", !testDocument.SC_IsPublished);
			Assert("Invalid doctype code, SaveVersions stays default", !testDocument.SC_SaveVersions);

			testDocument.SC_DocType = "ZZZ";
			AssertEquals("Valid doctype, description should update properly", testDocument.SC_Desc, newDocType.RT_Desc);
			Assert("Valid doctype, IsPublished should update properly", testDocument.SC_IsPublished);
			Assert("Valid doctype, SaveVersions should update properly", testDocument.SC_SaveVersions);
		}

		public void TestSC_DocType_Description()
		{
			var newDocType = MasterFactory.New(typeof(RefDocType)) as RefDocType;
			newDocType.RT_DocType = "GGG";
			newDocType.RT_ReferenceType = "ALL";

			newDocType.RT_Desc = "Test Description";
			newDocType.RT_IsActive = true;
			newDocType.RT_IsPublished = true;
			newDocType.RT_SaveVersions = true;
			MasterFactory.Save();

			var storageDoc = GetNewTestBizO(new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()), AssemblyDataLookup.GetReferenceTypeFromDocManagerCode(Core.Constants.DocManagerCodes.Organisation));
			storageDoc.SC_DocType = "GGG";

			AssertEquals("Should show proper doctype description", "Test Description", storageDoc.SC_DocType_Description);

			AssertEquals("Should equal to SC_DocType", "GGG", storageDoc.DocumentTypeCode);
			AssertEquals("Should equal to SC_DocType_Description", "Test Description", storageDoc.DocumentTypeDescription);
		}

		public void TestDocTypeDescriptionMultilingual()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				var newDocType = MasterFactory.New(typeof(RefDocType)) as RefDocType;
				newDocType.RT_DocType = "GGG";
				newDocType.RT_ReferenceType = "ALL";
				newDocType.RT_Desc = "Test Description";
				newDocType.RT_IsActive = true;
				newDocType.RT_IsPublished = true;
				newDocType.RT_SaveVersions = true;
				MasterFactory.Save();

				var key = ((ResourceString)newDocType.RT_DescMultilingual).ResourceKey;
				mockRes.Put(key, new ResourceStringData(key, "测试说明"));

				var storageDoc = GetNewTestBizO(new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()), AssemblyDataLookup.GetReferenceTypeFromDocManagerCode(Core.Constants.DocManagerCodes.Organisation));
				storageDoc.SC_DocType = "GGG";

				AssertEquals("Test Description", storageDoc.SC_Desc);
				AssertEquals("测试说明", storageDoc.SC_DescMultilingual);

				AssertEquals(true, storageDoc.SC_DescInfo.ReadOnly);
				AssertEquals(true, storageDoc.SC_DescMultilingual_ReadOnly);

				storageDoc.SC_DocType = "MSC";
				AssertEquals(false, storageDoc.SC_DescInfo.ReadOnly);
				AssertEquals(false, storageDoc.SC_DescMultilingual_ReadOnly);

				storageDoc.SC_DescMultilingual = (NoResString)"Custom Description";
				AssertEquals("Custom Description", storageDoc.SC_Desc);
			}
		}

		public void TestSM_Type()
		{
			var storageDoc = MasterFactory.New<StorageDocs>();
			storageDoc.SC_DocType = Core.Constants.RefDocTypes.AgentsInstruction;

			AssertEquals("Doc type should be invalid because the document does not have a parent", true, storageDoc.SC_DocTypeInfo.HasErrors());
			Assert("Description should be empty because it can't look up the doc type without a parent", storageDoc.SC_Desc.IsEmpty);

			var storageMain = MasterFactory.New<StorageMain>();
			storageDoc.SC_SM = storageMain.PK;
			storageDoc.SM_Type = Core.Constants.DocManagerCodes.Shipment;

			AssertEquals("Doc type should now be valid because the document has a parent, and the doc type is valid for that parent", false, storageDoc.SC_DocTypeInfo.HasErrors());
			Assert("Description should be populated because it can look up the doc type with a parent", !storageDoc.SC_Desc.IsEmpty);
			AssertEquals("Description should be correct for the doc type", Core.Constants.RefDocTypeDescriptions.AgentsInstruction, storageDoc.SC_Desc);
		}

		public void TestIsPublishedInfo()
		{
			var bizO = GetNewTestBizO(MasterFactory);

			var newDocType = MasterFactory.New<RefDocType>();
			newDocType.RT_DocType = "ZZZ";
			newDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			newDocType.RT_Desc = "Test Description";
			newDocType.RT_IsActive = true;
			newDocType.RT_IsPublished = true;
			newDocType.RT_SaveVersions = true;
			newDocType.RT_IsPublishUpdatable = false;

			AssertEquals(
				"Readonly takes default of object (IsPublishedReadonlyDefault) if it doesn't have a doctype",
				bizO.SC_IsPublishedReadonlyDefault,
				bizO.SC_IsPublishedInfo.ReadOnly);

			bizO.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;

			bizO.SC_DocType = "ZZZ";
			Assert("readonly if doctype supplied, but the doctype isn't publish updatable", bizO.SC_IsPublishedInfo.ReadOnly);

			newDocType.RT_IsPublishUpdatable = true;
			Assert("Not readonly if the doc type is publish updatable", !bizO.SC_IsPublishedInfo.ReadOnly);
		}

		public void TestDocTypeAndEDocIsPublishedNotMatchingShowsWarning()
		{
			var testDocument = GetNewTestBizO(MasterFactory, Core.Constants.DocManagerCodes.Organisation);
			testDocument.SC_IsPublished = false;

			var docTypeNotPublished = MasterFactory.New<RefDocType>();
			docTypeNotPublished.RT_DocType = "ZZZ";
			docTypeNotPublished.RT_IsActive = true;
			docTypeNotPublished.RT_IsPublished = false;
			docTypeNotPublished.RT_ReferenceType = "ALL";
			testDocument.SC_DocType_List.Add(docTypeNotPublished);

			var docTypePublished = MasterFactory.New<RefDocType>();
			docTypePublished.RT_DocType = "YYY";
			docTypePublished.RT_IsPublished = true;
			docTypePublished.RT_ReferenceType = "ALL";
			testDocument.SC_DocType_List.Add(docTypePublished);

			testDocument.SC_DocType = "ZZZ";
			AssertNoWarnings(testDocument.SC_DocTypeInfo);

			testDocument.SC_IsPublished = true;
			AssertHasWarning(testDocument.SC_DocTypeInfo, "The Published status for the document has been changed from the ZZZ Document Type's published value.");

			testDocument.SC_DocType = "YYY";
			testDocument.SC_IsPublished = true;
			AssertNoWarnings(testDocument.SC_DocTypeInfo);

			testDocument.SC_IsPublished = false;
			AssertHasWarning(testDocument.SC_DocTypeInfo, "The Published status for the document has been changed from the YYY Document Type's published value.");
		}

		public void TestUnpublishOlderVersionDocsShowsWarning()
		{
			var testDocument = GetNewTestBizO(MasterFactory, Core.Constants.DocManagerCodes.Organisation);
			testDocument.SC_IsPublished = true;
			AssertNoWarnings(testDocument.SC_DocTypeInfo);

			testDocument.IsSupersededByNewVersion = true;
			testDocument.SC_IsPublished = false;
			AssertHasWarning(testDocument.SC_DocTypeInfo, "This document has been superseded by a newer version and has been unpublished.");
		}

		public void TestRequiresUserToRead()
		{
			DocTypeWithForceUserToRead.Factory.Save();
			DocumentOrFile.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;

			BusinessObject parent = MasterFactory.NewWithValidTestData<OrgHeader>();
			DocumentOrFile.ParentMain.SM_ParentFK = parent.PK;
			MasterFactory.Save();
			Factory.Save();
			AssertEquals("Should not be unread if RT_ForceUserToRead='N'", false, DocumentOrFile.RequiresUserToRead);

			DocumentOrFile.SC_DocType = DocTypeWithForceUserToRead.RT_DocType;
			AssertEquals("Should not be unread if the current user added the document", false, DocumentOrFile.RequiresUserToRead);

			MasterFactory.Save();
			using (Enterprise.MasterFiles.Business.Testing.CurrentUserInitialsChanger.ChangeCurrentUserInitials("XXX"))
			{
				AssertEquals("Should be unread if RT_ForceUserToRead='Y'", true, DocumentOrFile.RequiresUserToRead);
				DocumentOrFile.NotifyReadByUser();
				AssertEquals("After the document has been read", false, DocumentOrFile.RequiresUserToRead);
			}

			using (Enterprise.MasterFiles.Business.Testing.CurrentUserInitialsChanger.ChangeCurrentUserInitials("YYY"))
			{
				AssertEquals("Should be unread if RT_ForceUserToRead='Y'", true, DocumentOrFile.RequiresUserToRead);
				DocumentOrFile.NotifyReadByUser();
				AssertEquals("After the document has been read", false, DocumentOrFile.RequiresUserToRead);
			}
		}

		public void TestIsUnallocated()
		{
			var org = Factory.New<OrgHeader>();
			Assert(DocumentOrFile.IsUnallocated);
			DocumentOrFile.ParentMain.SM_ParentFK = org.PK;
			Assert(!DocumentOrFile.IsUnallocated);
		}

		public void TestIsAllocated()
		{
			var org = Factory.New<OrgHeader>();
			Assert(!DocumentOrFile.IsAllocated);
			DocumentOrFile.ParentMain.SM_ParentFK = org.PK;
			Assert(DocumentOrFile.IsAllocated);

			var bizO = GetNewTestBizO(MasterFactory);
			bizO.SC_SM = ZGuid.Empty;
			Assert("no parent, document should be recognised as unallocated", !bizO.IsAllocated);
		}

		public void TestSC_FriendlyFileDescription()
		{
			DocumentOrFile.SC_FileName = "hello";
			DocumentOrFile.SC_DataType = "DOC";
			var fileAssoc = new FileAssociationRetriever();

			AssertEquals("Friendly file description", fileAssoc.GetFriendlyDocumentName("DOC"), DocumentOrFile.SC_FriendlyFileDescription);

			DocumentOrFile.SC_DataType = "XLS";
			AssertEquals("Friendly file description", fileAssoc.GetFriendlyDocumentName("XLS"), DocumentOrFile.SC_FriendlyFileDescription);
		}

		public void TestSC_FilenameWithExtension()
		{
			DocumentOrFile.SC_FileName = "hello";
			DocumentOrFile.SC_DataType = "PDF";
			AssertEquals("FilenameWithExtension combines SC_FileName and DataType", "hello.pdf", DocumentOrFile.SC_FileNameWithExtension);

			DocumentOrFile.SC_DataType = "doc";
			AssertEquals("FilenameWithExtension should now end in doc", "hello.doc", DocumentOrFile.SC_FileNameWithExtension);

			DocumentOrFile.SC_DataType = string.Empty;
			AssertEquals("Filename with extension should be just the filename", "hello", DocumentOrFile.SC_FileNameWithExtension);

			DocumentOrFile.SC_FileName = "hello  ";
			DocumentOrFile.SC_DataType = "PDF";
			AssertEquals("Filename with extension should trim extra whitespace", "hello.pdf", DocumentOrFile.SC_FileNameWithExtension);
		}

		public void TestGetFileNameOnlyWithExtension()
		{
			DocumentOrFile.SC_FileName = "hello";
			DocumentOrFile.SC_DataType = "PDF";
			AssertEquals("FilenameWithExtension combines SC_FileName and DataType", "hello.pdf", DocumentOrFile.GetFileNameOnlyWithExtension());

			DocumentOrFile.SC_DataType = "doc";
			AssertEquals("FilenameWithExtension should now end in doc", "hello.doc", DocumentOrFile.GetFileNameOnlyWithExtension());

			DocumentOrFile.SC_DataType = string.Empty;
			AssertEquals("Filename with extension should be just the filename", "hello", DocumentOrFile.GetFileNameOnlyWithExtension());

			DocumentOrFile.SC_FileName = "hello  ";
			DocumentOrFile.SC_DataType = "PDF";
			AssertEquals("Filename with extension should trim extra whitespace", "hello.pdf", DocumentOrFile.GetFileNameOnlyWithExtension());
		}

		public void TestGetFileNameOnlyWithoutExtension()
		{
			DocumentOrFile.SC_FileName = "hello";
			DocumentOrFile.SC_DataType = "PDF";
			AssertEquals("Filename without extension should be just the filename", "hello", DocumentOrFile.GetFileNameOnlyWithoutExtension());

			DocumentOrFile.SC_DataType = string.Empty;
			AssertEquals("Filename without extension should be just the filename", "hello", DocumentOrFile.GetFileNameOnlyWithoutExtension());

			DocumentOrFile.SC_FileName = "hello  ";
			AssertEquals("Filename without extension should trim extra whitespace", "hello", DocumentOrFile.GetFileNameOnlyWithoutExtension());
		}

		public void TestRestore()
		{
			DocumentOrFile.SC_IsDeleted = true;
			DocumentOrFile.Restore();
			AssertEquals("Test Object should not be marked deleted any more", false, DocumentOrFile.SC_IsDeleted);
		}

		public void TestDeleteQuietly()
		{
			DocumentOrFile.SC_IsDeleted = false;
			DocumentOrFile.DeleteQuietly();
			AssertEquals("Test Object should be marked deleted", true, DocumentOrFile.SC_IsDeleted);
		}

		public void TestDeleteQuietlyWhenNotInDB()
		{
			var newInDB = GetNewTestBizOForSave(MasterFactory);
			MasterFactory.Save();

			var newNotInDB = GetNewTestBizO(MasterFactory);

			Assert("Document saved in database", newInDB.IsInDatabase);
			Assert("Document not saved in database", !newNotInDB.IsInDatabase);

			Assert("Document is not deleted, has no deleted flag set", !newInDB.SC_IsDeleted);
			Assert("Document is not deleted, has no deleted flag set", !newNotInDB.SC_IsDeleted);

			newInDB.DeleteQuietly();
			newNotInDB.DeleteQuietly();

			Assert("New document in DB, flagged deleted", newInDB.SC_IsDeleted);
			Assert("New document not in DB, flagged deleted", newNotInDB.SC_IsDeleted);

			MasterFactory.Save();

			Assert("After call to OnFactorySave, New document originally in DB is still in DB", !newInDB.IsDeleted);
			Assert("After call to OnFactorySave, New document NOT in DB is deleted completely from DB", newNotInDB.IsDeleted);
		}

		public void TestIfDocIsDeleted()
		{
			var doc = GetNewTestBizOForSave(MasterFactory);
			doc.SetSC_ImageDataSource(new FileStreamSource(SmallTifPath));
			MasterFactory.Save();

			var newFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			newFactory.RefreshEnabled = false;
			doc = (StorageDocsBase)newFactory.Load(doc.GetType(), doc.PK);
			AssertEquals("Precondition: SC_ImageData needs loading", true, doc.BlobFieldsNeedLoadingExposedForTest(StorageDocsSchema.SC_ImageData));
			using (var tempFile = TempFile.New())
			{
				doc.Delete();
				doc.SaveToFilesystem(tempFile.Filename);

				Assert(doc.HasRowErrors);
				AssertHasRowError(doc, "The file you are trying to save has been deleted or does not exist.");
			}
		}

		public void TestNotifyReadByUser()
		{
			var document = GetNewTestBizOForSave(MasterFactory);
			MasterFactory.Save();

			AssertEquals("IsReadByUserInThisSession should be false initially", false, document.IsReadByUserInThisSession);
			document.NotifyReadByUser();
			AssertEquals("IsReadByUserInThisSession should be true after the user has read the document", true, document.IsReadByUserInThisSession);

			var newFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var loadedDocument = newFactory.Load<StorageDocsBase>(document.PK);
			AssertEquals("When the document is loaded from a different factory, IsReadByUserInThisSession is false again", false, loadedDocument.IsReadByUserInThisSession);
		}

		[ExpectNoExceptions]
		public void TestParentMainDoesNotThrowShouldNotBeAccessingPropertyOnDeletedBizO()
		{
			var document = GetNewTestBizOForSave(MasterFactory);
			document.Delete();

			var storageMain = document.ParentMain;
		}

		[ExpectNoExceptions()]
		public void TestOnFactorySavingDoesNotThrowNullReferenceException()
		{
			var bizO = GetNewTestBizOForSave(MasterFactory);
			bizO.SC_SM = ZGuid.NewZGuid();
			MasterFactory.Save(); // will call OnFactorySaving on the document
		}

		public void TestSaveToFileDoesNotKeepDataInMemory()
		{
			var doc = GetNewTestBizOForSave(MasterFactory.GetFactory(1));
			doc.SetSC_ImageDataSource(new FileStreamSource(SmallTifPath));
			MasterFactory.Save();

			var newFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()).GetFactory(1);
			newFactory.RefreshEnabled = false;
			doc = (StorageDocsBase)newFactory.Load(doc.GetType(), doc.PK);
			AssertEquals("Precondition: SC_ImageData needs loading", true, doc.BlobFieldsNeedLoadingExposedForTest(StorageDocsSchema.SC_ImageData));
			using (var tempFile = TempFile.New())
			{
				doc.SaveToFilesystem(tempFile.Filename);
				AssertEquals("SC_ImageData needs loading", true, doc.BlobFieldsNeedLoadingExposedForTest(StorageDocsSchema.SC_ImageData));
			}
		}

		public void TestCompanyBranchDepartmentSpecific()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "C1";
			company1.GC_Name = "C1";

			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "C2";
			company1.GC_Name = "C2";

			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_Code = "B1";
			branch1.GB_BranchName = "Branch 1";
			branch1.GB_GC = company1.PK;

			var department1 = Factory.New<GlbDepartment>();
			department1.GE_Code = "D1";
			department1.GE_Desc = "Department 1";

			var department2 = Factory.New<GlbDepartment>();
			department2.GE_Code = "D2";
			department2.GE_Desc = "department 2";

			Factory.Save();

			var doc = GetNewTestBizOForSave(MasterFactory);
			doc.SC_GC_Company = company1.PK;
			doc.SC_GB_Branch = branch1.PK;
			doc.SC_GE_Department = department1.PK;

			Assert(doc.IsCompanySpecific);
			Assert(doc.IsBranchSpecific);
			Assert(doc.IsDepartmentSpecific);
			Assert(doc.IsCompanyBranchDepartmentSpecific);
			Assert(!doc.CompanyCode_ReadOnly);
			Assert(!doc.BranchCode_ReadOnly);
			Assert(!doc.DepartmentCode_ReadOnly);
			Assert(!doc.IsBelongingToCurrentLoginCompany);
			Assert(!doc.IsBelongingToCurrentLoginBranch);
			Assert(!doc.IsBelongingToCurrentLoginDepartment);

			AssertEquals(company1.GC_Code, doc.CompanyCode);
			AssertEquals(branch1.GB_Code, doc.BranchCode);
			AssertEquals(department1.GE_Code, doc.DepartmentCode);
			AssertEquals(company1.GC_Code, ((IeDoc)doc).VisibleCompanyCode);
			AssertEquals(branch1.GB_Code, ((IeDoc)doc).VisibleBranchCode);
			AssertEquals(department1.GE_Code, ((IeDoc)doc).VisibleDepartmentCode);

			doc.SC_GC_Company = GlbCompany.CurrentCompany.PK;
			doc.SC_GB_Branch = GlbBranch.CurrentBranch.PK;
			doc.SC_GE_Department = GlbDepartment.CurrentDepartment.PK;

			Assert(doc.IsBelongingToCurrentLoginCompany);
			Assert(doc.IsBelongingToCurrentLoginBranch);
			Assert(doc.IsBelongingToCurrentLoginDepartment);
			AssertEquals(GlbCompany.CurrentCompany.GC_Code, doc.CompanyCode);
			AssertEquals(GlbBranch.CurrentBranch.GB_Code, doc.BranchCode);
			AssertEquals(GlbDepartment.CurrentDepartment.GE_Code, doc.DepartmentCode);
			AssertEquals(GlbCompany.CurrentCompany.GC_Code, ((IeDoc)doc).VisibleCompanyCode);
			AssertEquals(GlbBranch.CurrentBranch.GB_Code, ((IeDoc)doc).VisibleBranchCode);
			AssertEquals(GlbDepartment.CurrentDepartment.GE_Code, ((IeDoc)doc).VisibleDepartmentCode);

			doc.SC_GC_Company = ZGuid.Empty;
			doc.SC_GB_Branch = ZGuid.Empty;
			doc.SC_GE_Department = ZGuid.Empty;

			Assert(!doc.IsCompanySpecific);
			Assert(!doc.IsBranchSpecific);
			Assert(!doc.IsDepartmentSpecific);
			Assert(!doc.IsCompanyBranchDepartmentSpecific);
			Assert(doc.CompanyCode_ReadOnly);
			Assert(doc.BranchCode_ReadOnly);
			Assert(doc.DepartmentCode_ReadOnly);
			Assert(doc.IsBelongingToCurrentLoginCompany);
			Assert(doc.IsBelongingToCurrentLoginBranch);
			Assert(doc.IsBelongingToCurrentLoginDepartment);

			AssertEquals("", doc.CompanyCode);
			AssertEquals("", doc.BranchCode);
			AssertEquals("", doc.DepartmentCode);
			AssertEquals("", ((IeDoc)doc).VisibleCompanyCode);
			AssertEquals("", ((IeDoc)doc).VisibleBranchCode);
			AssertEquals("", ((IeDoc)doc).VisibleDepartmentCode);
		}

		public void TestSetDefaultCompanyBranchDepartmentSpecificValueFromDocType()
		{
			var docType1 = Factory.LoadTop1<RefDocType>(new ZQuery());
			Assert(!docType1.RT_IsBranchSpecific);
			Assert(!docType1.RT_IsCompanySpecific);
			Assert(!docType1.RT_IsDepartmentSpecific);

			var doc = GetNewTestBizOForSave(MasterFactory);
			doc.SC_DocType = docType1.RT_DocType;
			Assert(doc.SC_GC_Company.IsEmpty);
			Assert(doc.SC_GB_Branch.IsEmpty);
			Assert(doc.SC_GE_Department.IsEmpty);

			var docType2 = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.PK, SQLComparisonOperator.NotEqual, docType1.PK));
			docType2.RT_IsCompanySpecific = true;
			docType2.RT_IsBranchSpecific = true;
			docType2.RT_IsDepartmentSpecific = true;
			docType2.RT_ReferenceType = Core.Constants.ReferenceTypes.All;
			Factory.Save();

			doc.SC_DocType = docType2.RT_DocType;
			AssertEquals(GlbCompany.CurrentCompany.PK, doc.SC_GC_Company);
			AssertEquals(GlbBranch.CurrentBranch.PK, doc.SC_GB_Branch);
			AssertEquals(GlbDepartment.CurrentDepartment.PK, doc.SC_GE_Department);
		}

		public void TestNoValidationErrorOnExistingCompanySpecificEDoc()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "XC1";
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company1.PK;
			branch1.GB_Code = "XB1";

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "XC2";
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = company2.PK;
			branch2.GB_Code = "XB2";

			var localStaff = Factory.NewWithValidTestData<GlbStaff>();
			localStaff.GS_Code = "LS1";
			localStaff.GS_LoginName = "local1";
			localStaff.GS_IsController = false;

			var denyAllLoginSecurity = Factory.NewWithValidTestData<GlbSecurity>();
			denyAllLoginSecurity.GU_SecurityRight = "Login";
			denyAllLoginSecurity.GU_GS = localStaff.PK;
			denyAllLoginSecurity.GU_SecurityItemIsAllowed = false;

			var allowCompany1Security = Factory.NewWithValidTestData<GlbSecurity>();
			allowCompany1Security.GU_SecurityRight = "Login";
			allowCompany1Security.GU_GS = localStaff.PK;
			allowCompany1Security.GU_GC = company1.PK;
			allowCompany1Security.GU_SecurityItemIsAllowed = true;

			var docType = Factory.NewWithValidTestData<RefDocType>();
			docType.RT_ReferenceType = "ALL";
			docType.RT_IsCompanySpecific = true;

			var parent1 = Factory.NewWithValidTestData<OrgHeader>();
			var parent2 = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			using (Env.SetTemporaryUserContext(localStaff.GS_LoginName, branch1.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var savedDoc1 = GetNewTestBizOForSave(MasterFactory);
				savedDoc1.SC_DocType = docType.RT_DocType;
				savedDoc1.SC_GC_Company = company1.PK;
				savedDoc1.SC_GB_Branch = ZGuid.Empty;
				savedDoc1.SC_GE_Department = ZGuid.Empty;
				savedDoc1.ParentMain.SM_ParentFK = parent1.PK;

				var savedDoc2 = GetNewTestBizOForSave(MasterFactory);
				savedDoc2.SC_DocType = docType.RT_DocType;
				savedDoc2.SC_GC_Company = company2.PK;
				savedDoc2.SC_GB_Branch = ZGuid.Empty;
				savedDoc2.SC_GE_Department = ZGuid.Empty;
				savedDoc2.ParentMain.SM_ParentFK = parent2.PK;

				MasterFactory.Save();

				var newDoc = GetNewTestBizOForSave(MasterFactory);
				newDoc.SC_DocType = docType.RT_DocType;
				newDoc.SC_GC_Company = company2.PK;
				newDoc.SC_GB_Branch = ZGuid.Empty;
				newDoc.SC_GE_Department = ZGuid.Empty;

				((StorageDocsBaseValidation)savedDoc1.Validation).ValidateCompanyCode();
				AssertNoErrors("Should not have errors on saved record", savedDoc1.CompanyCodeInfo);

				((StorageDocsBaseValidation)savedDoc2.Validation).ValidateCompanyCode();
				AssertNoErrors("Should not have errors on saved record", savedDoc2.CompanyCodeInfo);

				((StorageDocsBaseValidation)newDoc.Validation).ValidateCompanyCode();
				AssertHasError("Should have error for new record", newDoc.CompanyCodeInfo, "Enter a valid Specific to Company.");

				savedDoc1.SC_GC_Company = company2.PK;
				((StorageDocsBaseValidation)savedDoc1.Validation).ValidateCompanyCode();
				AssertHasError("Should have error for changed company property", savedDoc1.CompanyCodeInfo, "Enter a valid Specific to Company.");
			}
		}

		public void TestImageDataIsNotWipedOffWhenS3Enabled()
		{
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.EDocsStorageProviders.Code.S3))
			{
				SetupForFileOpen();
				using (var document = GetObjectForTestingOpeningFile())
				{
					document.SC_FileName = "happy birthday";
					document.SC_ImageData = new byte[] { 1, 2, 3 };
					document.SC_IsPublished = false;
					document.SC_Date = new ZDateTime(2015, 5, 20);
					document.ReadOnly = false;
					MasterFactory.Save();

					var newByte = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
					document.SC_ImageData = newByte;
					MasterFactory.Save();

					var docReloaded = MasterFactory.Load<StorageDocsBase>(document.PK);
					AssertEquals("SC_ImageData should not be wiped off", newByte, docReloaded.SC_ImageData);
				}
			}
		}

		public void TestLoginObj()
		{
			var staff = MasterFactory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsController = true;
			var doc = GetNewTestBizOForSave(MasterFactory);
			doc.LoginObj.CurrentUserForTesting = staff;

			var companies = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_IsActive, true));
			AssertEquals("Companies count should be equal", companies.Length, doc.LoginObj.Companies.Count);

			doc.LoginObj.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			AssertEquals("Branches count should be equal", GlbCompany.CurrentCompany.Branches.Count, doc.LoginObj.Branches.Count);

			var departments = Factory.Load<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_IsActive, true));
			AssertEquals("Departments count should be equal", departments.Length, doc.LoginObj.Departments.Count);
		}

		public void TestIsAvailableForCurrentEnvContext()
		{
			var testCases = from company in new char[] { 'E', 'C', 'N', 'A' }
							from branch in new char[] { 'E', 'C', 'N', 'A' }
							from department in new char[] { 'E', 'C', 'N', 'A' }
							from security in new char[] { 'Y', 'N' }
							select new string(new char[] { company, branch, department, security });

			Env.Security.GetDocumentTypeViewCheckPoint("YYY").IsAllowed = true;
			Env.Security.GetDocumentTypeViewCheckPoint("NNN").IsAllowed = false;

			var doc = GetNewTestBizO(MasterFactory);

			foreach (var testCase in testCases)
			{
				AssertEDocIsAvailableForCurrentEnvContext(testCase);
			}

			void AssertEDocIsAvailableForCurrentEnvContext(string description)
			{
				var showForAllCompanies = false;
				switch (description[0])
				{
					case 'E':
						doc.SC_GC_Company = Guid.Empty;
						break;
					case 'C':
						doc.SC_GC_Company = Env.Instance.CurrentCompanyPK;
						break;
					case 'N':
						doc.SC_GC_Company = Guid.NewGuid();
						break;
					case 'A':
						showForAllCompanies = true;
						break;
				}

				var showForAllBranches = false;
				switch (description[1])
				{
					case 'E':
						doc.SC_GB_Branch = Guid.Empty;
						break;
					case 'C':
						doc.SC_GB_Branch = Env.Instance.CurrentBranchPK;
						break;
					case 'N':
						doc.SC_GB_Branch = Guid.NewGuid();
						break;
					case 'A':
						showForAllBranches = true;
						break;
				}

				var showForAllDepartments = false;
				switch (description[2])
				{
					case 'E':
						doc.SC_GE_Department = Guid.Empty;
						break;
					case 'C':
						doc.SC_GE_Department = Env.Instance.CurrentDepartmentPK;
						break;
					case 'N':
						doc.SC_GE_Department = Guid.NewGuid();
						break;
					case 'A':
						showForAllDepartments = true;
						break;
				}

				doc.SC_DocType = description[3] == 'Y' ? "YYY" : "NNN";

				var expected = !description.Contains('N');
				AssertEquals($"EDoc[{description}] should be {(expected ? "available" : "unavailable")} for CurrentEnvContext", expected, doc.IsAvailableForCurrentEnvContext(showForAllCompanies, showForAllBranches, showForAllDepartments));
			}
		}

		#region IDeliverable Members

		protected abstract void AssertDeliveryInfo(StorageDocsBase doc, DeliveryInfo info);

		StorageDocsBase GetBusinessObjectForTestingDeliveryInfo(OrgHeader org)
		{
			var result = (StorageDocsBase)GetNewBusinessObject();

			result.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			result.SC_FileName = "Winning Lottery Numbers";
			result.SC_DataType = result is StorageDocs ? "tif" : "txt";
			result.SC_ImageData = result is StorageDocs ? SmallTifBytes : [1, 2, 3];

			if (org != null)
			{
				result.ParentMain.SM_ParentFK = org.PK;
				result.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			}

			return result;
		}

		public abstract void TestDeliveryMode();

		public abstract void TestGetSupportedDeliveryMethodsCore();

		public abstract void TestSupportsDeliveryMethod();

		public abstract void TestGetSupportedDeliveryMethodDispiteOfPrintCopyType();

		public abstract void TestFileExtension();

		public abstract void TestAllAvailableDeliveryModes();

		public void TestGetMenuEDocs_WhenRefDocTypeIsNull()
		{
			var doc = (StorageDocsBase)GetNewBusinessObject();
			var menu = Factory.NewWithValidTestData<StmMenuItemBase>();
			var eDoc = menu.EDocsView.AddNew();
			eDoc.SX_PrintCopyType = nameof(PrintCopyType.EML);
			((IDeliverable)doc).MenuItem = menu;
			doc.SC_DocType = "ZZZ";

			AssertNoExceptionThrown("NRE exception should not be thrown out", () => doc.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Email));
		}

		[ExpectNoExceptions]
		public void TestGetDeliveryInfo()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var subject = GlbCompany.CurrentCompany.GC_Name + " - " + GlbBranch.CurrentBranch.GB_BranchName + " - A Test Description";
			var formatter = new EmailFormatter(GlbStaff.CurrentUser);
			var signature = formatter.GetEmailSignature(DocumentsDataRegistry.Instance.EmailFormat.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).EmailSignatureFields);
			var doc = GetBusinessObjectForTestingDeliveryInfo(org);
			AssertGetDeliveryInfo(doc, org, OrgHeaderSchema.Constants.TableName, org.PK, subject + " - " + org.OH_Code, signature);
			doc.SC_SM = ZGuid.Empty;
			AssertGetDeliveryInfo(doc, org, StorageDocsSchema.Constants.TableName, ZGuid.Empty, subject, signature);
		}

		public void AssertGetDeliveryInfo(StorageDocsBase doc, OrgHeader org, string parentTable, ZGuid parent, string subject, string signature)
		{
			doc.SC_Desc = "A Test Description";

			var deliverable = (IDeliverable)doc;
			var info = deliverable.GetDeliveryInfo(false);

			AssertDeliveryInfo(doc, info);
			AssertEquals("ParentTableName", parentTable, info.ParentTableName);
			AssertEquals("ParentGuid", parent, info.ParentGuid);
			AssertEquals("EmailSubjectLine", subject, info.EmailSubjectLine);
			AssertEquals("EmailSubjectLine", signature, info.EmailSignature);
			AssertEquals("FileFormat", deliverable.FileExtension, info.FileFormat);
			AssertEquals("Winning Lottery Numbers." + (doc is StorageDocs ? "tif" : "txt"), info.Name);
			using (var stream = (MemoryStream)info.FileContents)
			{
				AssertEquals("FileContents", doc.SC_ImageData, stream.ToArray());
			}
		}

		public void TestGetDeliveryInfoWithSystemGeneratedDocument()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var doc = GetBusinessObjectForTestingDeliveryInfo(org);

			var expectedDescription = GlbCompany.CurrentCompany.GC_Name + " (" + GlbBranch.CurrentBranch.GB_BranchName + ") - Notes - " + org.OH_Code;
			doc.SC_Desc = expectedDescription;
			doc.SC_IsSystemGenerated = true;

			var info = ((IDeliverable)doc).GetDeliveryInfo(false);
			AssertDeliveryInfo(doc, info);
			AssertEquals("ParentTableName", OrgHeaderSchema.Constants.TableName, info.ParentTableName);
			AssertEquals("ParentGuid", org.PK, info.ParentGuid);
			AssertEquals("EmailSubjectLine", expectedDescription, info.EmailSubjectLine);
			AssertEquals("FileFormat", ((IDeliverable)doc).FileExtension, info.FileFormat);
			using (var stream = (MemoryStream)info.FileContents)
			{
				AssertEquals("FileContents", doc.SC_ImageData, stream.ToArray());
			}
		}

		public void TestGetDeliveryInfoWithUnallocatedUnsavedDocument()
		{
			var doc = GetBusinessObjectForTestingDeliveryInfo(null);
			var info = ((IDeliverable)doc).GetDeliveryInfo(false);
			AssertDeliveryInfo(doc, info);
			AssertEquals("ParentTableName", StorageDocsSchema.Constants.TableName, info.ParentTableName);
			AssertEquals("ParentGuid", ZGuid.Empty, info.ParentGuid);
			AssertEquals("EmailSubjectLine", GlbCompany.CurrentCompany.GC_Name + " - " + GlbBranch.CurrentBranch.GB_BranchName + " - Document", info.EmailSubjectLine);
			using (var stream = (MemoryStream)info.FileContents)
			{
				AssertEquals("FileContents", doc.SC_ImageData, stream.ToArray());
			}
		}

		public void TestGetDeliveryInfoEmailSubjectLine()
		{
			var emailFormat = new EmailFormat();
			emailFormat.EmailSubjectFields.RemoveAndDeleteAll();
			emailFormat.EmailSubjectFields.Add(new EmailSubjectField("1", Core.Constants.EmailFormat.EmailFieldCodes.CompanyBrandName));
			emailFormat.EmailSubjectFields.Add(new EmailSubjectField("2", Core.Constants.EmailFormat.EmailFieldCodes.BranchName));
			DocumentsDataRegistry.Instance.EmailFormat.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, emailFormat);

			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var doc = GetBusinessObjectForTestingDeliveryInfo(org);
			var info = ((IDeliverable)doc).GetDeliveryInfo(false);

			AssertDeliveryInfo(doc, info);
			var expectedEmailSubject = GlbCompany.CurrentCompany.GC_Name + " - " + GlbBranch.CurrentBranch.GB_BranchName + " - " + org.OH_Code;
			AssertEquals("EmailSubjectLine", expectedEmailSubject, info.EmailSubjectLine);
		}

		public void TestIncludedInPrint()
		{
			var doc = (StorageDocsBase)GetNewBusinessObject();
			AssertEquals("IncludedInPrint should not trigger HasChanges", false, doc.HasChanges);
			AssertEquals("IncludedInPrint", true, doc.IncludedInPrint);
			doc.IncludedInPrint = false;
			AssertEquals("IncludedInPrint should not trigger HasChanges", false, doc.HasChanges);
			AssertEquals("IncludedInPrint", false, doc.IncludedInPrint);
			doc.IncludedInPrint = true;
			AssertEquals("IncludedInPrint should not trigger HasChanges", false, doc.HasChanges);
			AssertEquals("IncludedInPrint", true, doc.IncludedInPrint);
		}

		public void TestValidateIncludedInPrint_ShouldNotHaveWarningAfterValidateAll()
		{
			var doc = (StorageDocsBase)GetNewBusinessObject();
			doc.SC_FileName = "Jerry Test.txt";
			doc.SC_IsPublished = false;

			var baseValidation = (StorageDocsBaseValidation)doc.Validation;
			baseValidation.ValidateAll();
			AssertNoWarning(doc.IncludedInPrintInfo, "The eDoc file Jerry Test.txt was not published.");

			doc.ShouldPrintByDefault = true;
			baseValidation.ValidateIncludedInPrint();
			AssertNoWarning(doc.IncludedInPrintInfo, "The eDoc file Jerry Test.txt was not published.");

			doc.ShouldPrintByDefault = false;
			baseValidation.ValidateIncludedInPrint();
			AssertHasWarning(doc.IncludedInPrintInfo, "The eDoc file Jerry Test.txt was not published.");
		}

		public void TestValidateIncludedInPrint_ShouldNotHaveErrorAfterValidateAll()
		{
			var doc = (StorageDocsBase)GetNewBusinessObject();
			doc.SC_FileName = "Jerry Test.txt";
			doc.SC_IsPublished = true;
			doc.SC_IsDeleted = true;

			var baseValidation = (StorageDocsBaseValidation)doc.Validation;
			baseValidation.ValidateAll();
			AssertNoError(doc.IncludedInPrintInfo, "The eDoc file Jerry Test.txt has been deleted.");

			doc.ShouldPrintByDefault = true;
			baseValidation.ValidateIncludedInPrint();
			AssertNoError(doc.IncludedInPrintInfo, "The eDoc file Jerry Test.txt has been deleted.");

			doc.ShouldPrintByDefault = false;
			baseValidation.ValidateIncludedInPrint();
			AssertHasError(doc.IncludedInPrintInfo, "The eDoc file Jerry Test.txt has been deleted.");
		}

		public void TestValidateIsParsingEnabledForDenyParsing()
		{
			var doc = (StorageDocsBase)GetNewBusinessObject();
			doc.SC_FileName = "Test.txt";

			var parsingSupportMock = new Mock<IEDocsParsingSupport>();
			parsingSupportMock.Setup(x => x.DenySendForParsing(doc.PK.ToGuid(), It.IsAny<string>(), It.IsAny<string>())).Returns(false);
			doc.EDocsParsingSupport = parsingSupportMock.Object;

			doc.IsParsingEnabled = true;
			AssertNoError("There is no validation error when parsing is not denied.", doc.IsParsingEnabledInfo, "The document is not eligible for parsing.");

			parsingSupportMock.Setup(x => x.DenySendForParsing(doc.PK.ToGuid(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
			doc.IsParsingEnabled = false;
			AssertNoError("There is no validation error when parsing is not enabled.", doc.IsParsingEnabledInfo, "The document is not eligible for parsing.");

			doc.IsParsingEnabled = true;
			AssertHasError("There is an error when parsing is denied but IsParseEnable is set true.", doc.IsParsingEnabledInfo, "The document is not eligible for parsing.");
		}

		public void TestValidateIsParsingEnabledForDataType()
		{
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{ 
				SetUpShipamaxSupportedDataTypes();
				var shipment = MasterFactory.NewWithValidTestData<ForwardingShipment>();
				var doc = GetNewTestBizOForSave(MasterFactory.GetFactory(1));
				doc.ParentMain.SM_ParentFK = shipment.PK;
				doc.ParentMain.SM_Type = "SHP";
				doc.SC_FileName = "Test";
				doc.SC_DataType = "JPG";
				doc.SC_DocType = "CIV";

				AssertEquals(true, doc.IsParsingEnabled);
				AssertNoWarning("No data type warning for parsing as parsing is enabled", doc.IsParsingEnabledInfo, "The document’s file type is not supported for parsing.");

				doc.SC_DataType = "PNG";
				AssertEquals(false, doc.IsParsingEnabled);
				AssertHasWarning("Changing DataType to PNG should trigger the warning", doc.IsParsingEnabledInfo, "The document’s file type is not supported for parsing.");

				doc.SC_DocType = "MSC";
				AssertEquals(false, doc.IsParsingEnabled);
				AssertNoWarning("No data type warning for parsing as doc type is not supported for parsing", doc.IsParsingEnabledInfo, "The document’s file type is not supported for parsing.");

				doc.SC_DocType = "CIV";
				AssertEquals(false, doc.IsParsingEnabled);
				AssertHasWarning("Changing DocType back to CIV, which supports parsing, should trigger the warning again", doc.IsParsingEnabledInfo, "The document’s file type is not supported for parsing.");
			}
		}

		public void TestMenuItem()
		{
			var doc = (IDeliverable)GetNewBusinessObject();
			AssertNull("MenuItem", doc.MenuItem);
			var menuItem = Factory.New<StmMenuItem>();
			doc.MenuItem = menuItem;
			AssertEquals("MenuItem", menuItem, doc.MenuItem);
		}

		public abstract void TestName();

		public abstract void TestBindingName();

		public void TestSave()
		{
			var doc = (StorageDocsBase)GetNewBusinessObject();
			var originalTestFile = SmallTifPath;
			var originalFileInfo = new FileInfo(originalTestFile);

			doc.SC_ImageData = DocumentUtilities.GetFileAsBytes(originalTestFile);

			using (var tempFile = TempFile.NewWithExtension("TIF"))
			using (var fileContent = new MemoryStream())
			{
				AssertEquals("fileContent.Length", 0, fileContent.Length);
				((IDeliverable)doc).Save(new DocDeliveryContact(Factory), new DocDeliveryContact(Factory), fileContent);
				AssertEquals("fileContent.Length", originalFileInfo.Length, fileContent.Length);
			}
		}

		#endregion

		#region IDocument Members

		public void TestDocumentDeliveryMethodAndName()
		{
			var storageDocs = GetBusinessObjectForTestingDeliveryInfo(null);
			IDocument document = storageDocs;
			AssertEquals("DocumentDeliveryMethod", storageDocs.DeliveryMode, document.DocumentDeliveryMethod);
			AssertEquals("DocumentName", storageDocs.Name, document.DocumentName);
			AssertEquals("IncludeInPrint", storageDocs.IncludedInPrint, document.IncludeInPrint);
		}

		public void TestCanIncludeInPrint_ShouldDefaultToTrue()
		{
			IDocument storageDocs = GetBusinessObjectForTestingDeliveryInfo(null);
			Assert(storageDocs.CanIncludeInPrint);
		}

		public void TestIncludeInPrint_ConcurrentCollectionModification()
		{
			//this is a pretty egregious way to do it, because I don't know how it really happened.
			var imageData = TwoMbDatBytes;
			var storageDocs = GetBusinessObjectForTestingDeliveryInfo(null);
			storageDocs.SC_ImageData = imageData;
			var storageDocs2 = GetBusinessObjectForTestingDeliveryInfo(null);
			storageDocs2.SC_ImageData = imageData;
			var command = Factory.New<DocumentCommand>();
			var pack = new DocumentPack(command);
			var instructions = new DeliveryInstructions(pack);
			instructions.DeliverablesToBePrinted.Add(storageDocs);
			instructions.DeliverablesToBePrinted.Add(storageDocs2);

			instructions.Recipients.RemoveAndDeleteAll();
			var contact = instructions.Recipients.AddNew();
			//contact.DeliveryMethod = ""; //to trigger notification error
			contact.AttachmentType = "PDF";
			contact.DeliveryAddress = "test@wgt.com";
			var contact2 = instructions.Recipients.AddNew();
			contact2.DeliveryMethod = "EML";
			contact2.AttachmentType = "PDF";
			contact2.DeliveryAddress = "test2@wgt.com";

			contact.NotificationsChanged += (o, e) => { instructions.Recipients.Remove(contact2); };
			contact2.NotificationsChanged += (o, e) => { instructions.Recipients.Remove(contact); };

			AssertNoExceptionThrown(() => { storageDocs.IncludedInPrint = true; });
		}

		public void TestNoErrorsAfterResetEDocsToBeDeliveredIfNeeded()
		{
			var imageData = TwoMbDatBytes;
			var storageDocs = GetBusinessObjectForTestingDeliveryInfo(null);
			storageDocs.SC_ImageData = imageData;
			var storageDocs2 = GetBusinessObjectForTestingDeliveryInfo(null);
			storageDocs2.SC_ImageData = imageData;

			var command = Factory.New<DocumentCommand>();
			var pack = new DocumentPack(command);
			var instructions = new DeliveryInstructions(pack);
			_ = instructions.EDocsToBeDelivered;
			instructions.DeliverablesToBePrinted.Add(storageDocs);
			instructions.DeliverablesToBePrinted.Add(storageDocs2);

			using (instructions.ResetEDocsToBeDeliveredIfNeeded())
			{
				storageDocs.IncludedInPrint = true;
				storageDocs.Index = 1;
				storageDocs2.IncludedInPrint = true;
				storageDocs2.Index = 1;

				AssertHasWarnings(storageDocs.IndexInfo);
				AssertHasWarnings(storageDocs2.IndexInfo);
			}

			AssertNoErrors(storageDocs.IndexInfo);
			AssertNoWarnings(storageDocs.IndexInfo);
			AssertNoErrors(storageDocs2.IndexInfo);
			AssertNoWarnings(storageDocs2.IndexInfo);
		}

		public void TestIncludeInPrintShouldTriggerEmailDeliveryValidation()
		{
			var storageDocs = GetBusinessObjectForTestingDeliveryInfo(null);
			storageDocs.SC_ImageData = TwoMbDatBytes;
			var command = Factory.New<DocumentCommand>();
			var pack = new DocumentPack(command);
			var instructions = new DeliveryInstructions(pack);
			instructions.DeliverablesToBePrinted.Add(storageDocs);

			instructions.Recipients.RemoveAndDeleteAll();
			var contact = instructions.Recipients.AddNew();
			contact.DeliveryMethod = "EML";
			contact.AttachmentType = "PDF";
			contact.DeliveryAddress = "test@wgt.com";

			using (SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				storageDocs.IncludedInPrint = true;
				AssertHasError(contact.DeliveryMethodInfo,
					$"One or more eDoc files exceeds the 1MB attachment limit and cannot be sent: {((IDeliveryEmailAttachment)storageDocs).FileName}. The limit is defined in the Registry at {((IRegistryItemInternals)SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB).Location}.");

				storageDocs.IncludedInPrint = false;
				AssertNoErrors(contact.DeliveryMethodInfo);
			}

			//999 999 999 MB when converted to Bytes will overflow int.MaxValue. This makes sure we cater for the max value this registry supports
			using (SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 999_999_999))
			{
				storageDocs.IncludedInPrint = true;
				AssertNoErrors(contact.DeliveryMethodInfo);
			}
		}

		#endregion

		#region IDeliveryEmailAttachmentMembers

		public void TestIDeliveryEmailAttachmentMembers()
		{
			var storageDocs = GetBusinessObjectForTestingDeliveryInfo(null);
			IDeliveryEmailAttachment attachment = storageDocs;
			AssertEquals("IDeliveryEmailAttachment.FileName", storageDocs.Name, attachment.FileName);
			AssertEquals("IDeliveryEmailAttachment.FileSizeInBytes", storageDocs.SC_ImageData.Length, attachment.FileSizeInBytes);

			storageDocs.IncludedInPrint = true;
			Assert(attachment.ShouldBeAttached);
			storageDocs.IncludedInPrint = false;
			Assert(!attachment.ShouldBeAttached);
		}

		public void TestAttachmentSize()
		{
			AssertEquals("0B", StorageDocsBase.AttachmentSize(0));
			AssertEquals("1B", StorageDocsBase.AttachmentSize(1));
			AssertEquals("1023B", StorageDocsBase.AttachmentSize(1024 - 1));
			AssertEquals("1KB", StorageDocsBase.AttachmentSize(1024));
			AssertEquals("1.5KB", StorageDocsBase.AttachmentSize(1024.0 * 1.5));
			AssertEquals("1023KB", StorageDocsBase.AttachmentSize(1024 * 1023));
			AssertEquals("1MB", StorageDocsBase.AttachmentSize(1024 * 1024));
			AssertEquals("1.5MB", StorageDocsBase.AttachmentSize(1024.0 * 1024.0 * 1.5));
			AssertEquals("1023MB", StorageDocsBase.AttachmentSize(1024 * 1024 * 1023));
			AssertEquals("1GB", StorageDocsBase.AttachmentSize(1024 * 1024 * 1024 + 1));
			AssertEquals("1.5GB", StorageDocsBase.AttachmentSize(1024.0 * 1024.0 * 1024.0 * 1.5));
			AssertEquals("1023GB", StorageDocsBase.AttachmentSize(1024L * 1024L * 1024L * 1023L));
			AssertEquals("1024GB", StorageDocsBase.AttachmentSize(1024L * 1024L * 1024L * 1024L));
			AssertEquals("1536GB", StorageDocsBase.AttachmentSize((1024.0 * 1024.0 * 1024.0 * 1024.0 + 1.0) * 1.5));
		}

		#endregion

		#region IeDoc Members

		public void TestIEDocLastEdited()
		{
			var newElement = GetNewTestBizOForSave(MasterFactory);
			MasterFactory.Save();
			AssertEquals("Last edited date should be same", ((IeDoc)newElement).LastEdited, newElement.SC_SystemLastEditTimeUtc);
		}

		public void TestIEDocDocType()
		{
			var newElement = GetNewTestBizO(MasterFactory);
			newElement.SC_DocType = "ABC";
			AssertEquals("DocType should return DocType on Document", ((IeDoc)newElement).DocType, newElement.SC_DocType);

			newElement.SC_DocType = "123";
			AssertEquals("DocType should return DocType on Document", ((IeDoc)newElement).DocType, newElement.SC_DocType);
		}

		public void TestIEDocImageData()
		{
			var newElement = GetNewTestBizO(MasterFactory);
			newElement.SC_ImageData = new byte[] { 1, 2, 3 };
			AssertEquals("DocType should return Image Data on Document", ((IeDoc)newElement).ImageData, newElement.SC_ImageData);

			newElement.SC_ImageData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
			AssertEquals("DocType should return Image Data on Document", ((IeDoc)newElement).ImageData, newElement.SC_ImageData);
		}

		public void TestIEDocIsSystemGenerated()
		{
			var newElement = GetNewTestBizO(MasterFactory);
			newElement.SC_IsSystemGenerated = true;
			AssertEquals("DocType should return IsSystemGenerated on Document", ((IeDoc)newElement).IsSystemGenerated, newElement.SC_IsSystemGenerated);

			newElement.SC_IsSystemGenerated = false;
			AssertEquals("DocType should return IsSystemGenerated on Document", ((IeDoc)newElement).IsSystemGenerated, newElement.SC_IsSystemGenerated);
		}

		public void TestIEDocIsDeleted()
		{
			var newElement = GetNewTestBizO(MasterFactory);
			newElement.SC_IsDeleted = true;
			AssertEquals("DocType should return IsDeleted on Document", ((IeDoc)newElement).IsDeleted, newElement.SC_IsDeleted);

			newElement.SC_IsDeleted = false;
			AssertEquals("DocType should return IsDeleted on Document", ((IeDoc)newElement).IsDeleted, newElement.SC_IsDeleted);
		}

		public void TestIEDocIsPublished()
		{
			var newElement = GetNewTestBizO(MasterFactory);
			newElement.SC_IsPublished = true;
			AssertEquals("DocType should return IsPublished on Document", ((IeDoc)newElement).IsPublished, newElement.SC_IsPublished);

			newElement.SC_IsPublished = false;
			AssertEquals("DocType should return IsPublished on Document", ((IeDoc)newElement).IsPublished, newElement.SC_IsPublished);
		}

		public void TestIEDocFileName()
		{
			var newElement = GetNewTestBizO(MasterFactory);

			if (newElement.IsImageFile)
			{
				newElement.SC_Desc = "tiffile";
				AssertEquals("tiffile.tif", ((IeDoc)newElement).FileName);
			}
			else
			{
				newElement.SC_FileName = "pdffile";
				newElement.SC_DataType = "pdf";
				AssertEquals("pdffile.pdf", ((IeDoc)newElement).FileName);
			}
		}

		public void TestIEDocGetImageDataReader()
		{
			var newElement = GetNewTestBizO(MasterFactory);
			var bytes = new byte[] { 1, 2, 3 };
			newElement.SC_ImageData = bytes;

			using (var reader = ((IeDoc)newElement).GetImageDataReader())
			{
				AssertArrayEqualsByElements("ImageDataReader should have Image Data on Document", bytes, reader.ConvertToByteArrayAndCloseStream());
			}
		}

		public virtual void TestEDocsFormat()
		{
			var doc = (StorageDocsBase)GetNewBusinessObject();
			AssertEquals("Should be equal", doc.SC_DataType, doc.EDocFormat);

			doc.SC_DataType = "XXX";
			AssertEquals("Should still be equal", doc.SC_DataType, doc.EDocFormat);
		}

		public void TestFileSizeInMB()
		{
			var newElement = GetNewTestBizO(MasterFactory);
			newElement.SC_ImageData = new byte[] { 1, 2, 3 };
			AssertEquals(new ZDecimal(0.00000286102294921875), ((IeDoc)newElement).FileSizeInMB);
			newElement.SC_ImageData = new byte[1024 * 1024];
			AssertEquals(new ZDecimal(1), ((IeDoc)newElement).FileSizeInMB);
		}

		public abstract bool ExpectedTIF { get; }
		public abstract bool ExpectedJPG { get; }
		public abstract bool ExpectedTXT { get; }

		#endregion

		#region Open in Filesystem

		protected interface IIsLaunched
		{
			bool IsLaunched { get; }
		}

		public void TestOpenForEditAndOnSaving()
		{
			SetupForFileOpen();
			using (var syncContext = SynchronizationContextForTest.Enable())
			using (var document = GetObjectForTestingOpeningFile())
			{
				var originalDate = new ZDateTime(2004, 12, 10);

				document.SC_FileName = "hello";
				document.SC_ImageData = SmallTifBytes;
				document.SC_IsPublished = false;
				document.SC_Date = originalDate;
				document.ReadOnly = false;

				MasterFactory.Save();
				Assert("No temp file should be stored yet", document.TempFileName.IsEmpty);
				AssertEquals("HasChanges should be false", false, document.HasChanges);

				document.ReadOnly = true;
				using (document.OpenForEdit())
				{
					AssertEquals("SC_Date should not have changed", originalDate, document.SC_Date);
					Assert("HasChanges should be false on the object", !document.HasChanges);
					Assert("Should now be a temp file stored", !document.TempFileName.IsEmpty);

					document.ReadOnly = false;
					using (document.OpenForEdit())
					{
						AssertEquals("SC_Date should not have changed", originalDate, document.SC_Date);
						Assert("HasChanges should be false on the object", !document.HasChanges);
						Assert("Should now be a temp file stored", !document.TempFileName.IsEmpty);

						using (var wr = File.AppendText(document.TempFileName))
						{
							wr.Write("Anton");
							wr.Flush();
						}
						Assert("Expected watcher_changed event to be raised", syncContext.WaitAndExecuteCallbacks(TimeSpan.FromSeconds(1)));
						Assert("HasChanges should be true on the object", document.HasChanges);
						File.SetAttributes(document.TempFileName, FileAttributes.Normal);
						var attributes = File.GetAttributes(document.TempFileName);
						AssertEquals("FileAttributes should be normal for a file opened for edit", attributes, FileAttributes.Normal);
						Assert("NewFile process should have been launched", (document as IIsLaunched).IsLaunched);

						MasterFactory.Save();
						AssertEquals("SC_Date should not have changed", originalDate, document.SC_Date);
						AssertEquals("SC_IsPublished should have reverted back to original value", false, document.SC_IsPublished);

						if (document is StorageFile)
						{
							using (document.OpenForEdit())
							{
								document.SC_ImageData = ZBlob.Empty;

								MasterFactory.Save();
								var expectedTime = ZDateTime.UtcNow;
								var actualTime = document.SC_Date;
								Assert("SC_Date should be the current time if the file has changed. Expected " + expectedTime + " but was " + actualTime,
									expectedTime - actualTime < new TimeSpan(0, 0, 2));
							}
						}
					}
				}
			}
		}

		protected virtual void SetupForFileOpen()
		{
		}

		protected abstract StorageDocsBase GetObjectForTestingOpeningFile();

		public void TestIsTempFileOpen()
		{
			SetupForFileOpen();
			var newFile = GetObjectForTestingOpeningFile();
			try
			{
				AssertEquals("With empty temp file should return false", false, newFile.IsTempFileOpen);

				newFile.SC_FileName = "blah";
				newFile.SC_ImageData = SmallTifBytes;
				newFile.SaveToTempFile();

				AssertEquals("With temp file existing, but not open, should return false", false, newFile.IsTempFileOpen);

				using (var stream = File.OpenWrite(newFile.TempFileName))
				{
					AssertEquals("With temp file open, should return true", true, newFile.IsTempFileOpen);
				}

				File.SetAttributes(newFile.TempFileName, FileAttributes.ReadOnly);

				AssertEquals("With temp (read only) file existing, but not open, should return false", false, newFile.IsTempFileOpen);

				using (var stream = File.OpenRead(newFile.TempFileName))
				{
					AssertEquals("With temp (read only) file open, should return true", true, newFile.IsTempFileOpen);
				}
			}
			finally
			{
				try
				{
					File.SetAttributes(newFile.TempFileName, FileAttributes.Normal);
				}
				finally
				{
					newFile.Dispose();
				}
			}
		}

		#endregion

		#region SC_Desc

		public void TestSC_Desc_IsNotReadonlyForMiscDocType()
		{
			var storageDocs = GetNewTestBizO(MasterFactory);
			storageDocs.SC_DocType = "MSC";
			AssertEquals("SC_Desc should NOT be readonly for MSC doc type", false, storageDocs.SC_DescInfo.ReadOnly);

			storageDocs.SC_DocType = string.Empty;
			AssertEquals("SC_Desc should be readonly for other doc types", true, storageDocs.SC_DescInfo.ReadOnly);
		}

		#endregion

		#region Implementation

		StorageDocsBase DocumentOrFile => document ?? (document = GetNewTestBizO(MasterFactory.GetFactory(1)));

		StorageDocsBase document;

		RefDocType DocTypeWithForceUserToRead
		{
			get
			{
				if (docTypeWithForceUserToRead == null)
				{
					docTypeWithForceUserToRead = MasterFactory.New<RefDocType>();
					docTypeWithForceUserToRead.RT_ForceUserToRead = true;
					docTypeWithForceUserToRead.RT_ReferenceType = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
					docTypeWithForceUserToRead.RT_DocType = "FCE";
				}

				return docTypeWithForceUserToRead;
			}
		}

		RefDocType docTypeWithForceUserToRead;

		void DeleteFileIfExists(string filename)
		{
			if (File.Exists(filename))
			{
				File.Delete(filename);
			}
		}

		protected abstract StorageDocsBase GetNewTestBizOForSave(NumberedBusinessObjectFactory factory);

		protected StorageDocsBase GetNewTestBizO(NumberedBusinessObjectFactory factory, string referenceType)
		{
			var result = GetNewTestBizO(factory);
			result.ParentMain.SM_Type = referenceType;
			return result;
		}

		protected StorageDocsBase GetNewTestBizOForSave(NumberedBusinessObjectFactory factory, string referenceType)
		{
			var result = GetNewTestBizOForSave(factory);
			result.ParentMain.SM_Type = referenceType;
			return result;
		}

		void AssertShipamaxMessages(ZGuid pk)
		{
			var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, pk)
			{
				OrderBy = AutoEDIMessage.Schema.EM_SystemCreateTimeUtc + OrderByClause.Descending
			};
			var messages = Factory.Load<EDocsShipamaxMessage>(query);

			AssertEquals("There should be 2 EDocsShipamaxMessage records", 2, messages.Length);
			AssertEquals("The first one should be active", expected: true, messages[0].EM_IsActive);
			AssertEquals("The first one should be in QUE status", expected: "QUE", messages[0].EM_Status);
			AssertEquals("The second one should be inactive", expected: false, messages[1].EM_IsActive);
		}

		StorageDocsBase CreateEDocForShipamaxIntegration(ZString docType, bool dataTypeAcceptedByShipamax = true, string docManagerCode = "SHP")
		{
			var shipment = MasterFactory.NewWithValidTestData<ForwardingShipment>();
			var newDoc = GetNewTestBizOForSave(MasterFactory.GetFactory(1));
			newDoc.ParentMain.SM_ParentFK = shipment.PK;
			newDoc.ParentMain.SM_Type = docManagerCode;
			if (newDoc is StorageFile)
			{
				newDoc.SC_ImageData = dataTypeAcceptedByShipamax ? SamplePdfBytes : TestXlsBytes;
				newDoc.SC_DataType = dataTypeAcceptedByShipamax ? "PDF" : "XSL";
			}
			else
			{
				newDoc.SC_ImageData = dataTypeAcceptedByShipamax ? SmallJpgBytes : SmallTifBytes;
				newDoc.SC_DataType = dataTypeAcceptedByShipamax ? "JPG" : "TIF";
			}
			newDoc.SC_FileName = "test";
			newDoc.SC_DocType = docType;
			newDoc.SC_Desc = "Doc Type for Shipamax Integration ";

			return newDoc;
		}

		#endregion

		void SetUpShipamaxSupportedDataTypes()
		{
			if (!ObjectFactory.HasBeenSubstituted<IDashParametersService>())
			{
				var dashServiceMock = new Mock<IDashParametersService>();
				dashServiceMock.SetupGet(x => x.SupportedDataTypes).Returns(new List<String>() { "PDF", "JPG" });
				ObjectFactory.Substitute(dashServiceMock.Object);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			SystemDataRegistry.Instance.EDocsStorageProvider.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, null);
			using (var adminConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.CreateDbDropExisting(adminConnection, Db.DatabaseName + "_SD001");
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		protected override void OnAfterBaseTestCaseRunBare()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(adminConnection, Db.DatabaseName + "_SD001");
				AdoTestUtils.DropDbIfExists(adminConnection, Db.DatabaseName + "_SD002");
			}
			base.OnAfterBaseTestCaseRunBare();
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		byte[] TestXlsBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Test.xls");

		byte[] FivePagesTifBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.5pages.tif");

		byte[] SmallTifBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif");

		byte[] TwoMbDatBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.2MB.dat");

		byte[] SamplePdfBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF");

		byte[] SmallJpgBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.JPG");

		string SmallTifPath
		{
			get
			{
				if (string.IsNullOrEmpty(smallTifPath))
				{
					smallTifPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif");
				}
				return smallTifPath;
			}
		}
		string smallTifPath;
	}

	class StorageDocsBaseNonTransactionedTest : TestCase
	{
		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestConcurrencyFailure()
		{
			var edocsDB = 1;

			var masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());

			var org = masterFactory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");

			var parent = masterFactory.New<StorageMain>();
			parent.SM_DB = edocsDB;
			parent.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			parent.SM_ParentFK = org.PK;

			var document = (StorageDocsBase)parent.Documents.AddNew();
			document.SC_FileName = "FileName";
			document.SC_ImageData = System.Text.Encoding.ASCII.GetBytes("Test Text");
			document.SC_Date = ZDateTime.Now;
			document.SC_SM = parent.PK;
			masterFactory.Save();

			document.SC_FileName = "NewFileName";

			using (new TemporaryUserContext() { StaffLoginName = User.SupportUserName, BranchPK = Env.CurrentBranch.PK, DepartmentPK = Env.CurrentDepartment.PK }.Set())
			{
				var newMasterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				var documentInNewFactory = newMasterFactory.GetFactory(edocsDB).Load<StorageDocsBase>(document.PK);
				documentInNewFactory.SC_FileName = "AltNewFileName";

				using (TestCaseWithFactory.GetFactoryIsolater(document.Factory))
				using (TestCaseWithFactory.GetFactoryIsolater(document.ParentMain.Factory))
				using (TestCaseWithFactory.GetFactoryIsolater(documentInNewFactory.Factory))
				using (TestCaseWithFactory.GetFactoryIsolater(documentInNewFactory.ParentMain.Factory))
				{
					newMasterFactory.Save();
				}
			}

			try
			{
				masterFactory.Save();
				Fail("Expecting a ZSaveConcurrencyException to be thrown");
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}

			// Concurrency error should be resolved now.
			masterFactory.Save();
		}
	}

	sealed class StorageDocsBaseRemoteDesktopServicesTest : RemoteDesktopServicesTest
	{
		public void TestFileNameWithInvalidCharacters()
		{
			var doc = DocumentFactory.NewWithParent(typeof(StorageDocs));
			doc.ParentMain.SM_ParentFK = Factory.NewWithValidTestData<OrgHeader>().PK;
			doc.SC_FileName = "\"Sample\"";
			doc.SC_DataType = "pdf";
			AssertEquals("UseRemoteFile", true, doc.UseRemoteFile);
		}

		public void TestReloadOnStorageMainDisposeRemoteFile()
		{
			var doc = DocumentFactory.NewWithParent(typeof(StorageDocs));
			doc.ParentMain.SM_ParentFK = Factory.NewWithValidTestData<OrgHeader>().PK;
			doc.SC_FileName = "Sample";
			doc.SC_DataType = "pdf";

			var disposeInvoked = false;
			var remoteFileMock = new Mock<IRemoteFile>();
			remoteFileMock.Setup(x => x.RemoteFilesSupported).Returns(true);
			remoteFileMock.Setup(x => x.Dispose()).Callback(() => { disposeInvoked = true; });
			ObjectFactory.Substitute(remoteFileMock.Object);
			AssertEquals("UseRemoteFile", true, doc.UseRemoteFile);

			doc.ParentMain.RequireReload();
			AssertEquals("Remote file is disposed after reload", true, disposeInvoked);
		}

		public void TestNoExceptionOnDeleteRemoteFileWhenOpenForEdit()
		{
			var doc = DocumentFactory.NewWithParent(typeof(StorageDocs));
			doc.ParentMain.SM_ParentFK = Factory.NewWithValidTestData<OrgHeader>().PK;
			doc.SC_FileName = "Sample";
			doc.SC_DataType = "pdf";

			var remoteFileMock = new Mock<IRemoteFile>();
			remoteFileMock.Setup(x => x.RemoteFilesSupported).Returns(true);
			remoteFileMock.Setup(x => x.Open()).Callback(() => { Thread.Sleep(3000); }).Returns(true);
			ObjectFactory.Substitute(remoteFileMock.Object);

			var threadForViewingFile = new Thread(() =>
			{
				using var syncContext = SynchronizationContextForTest.Enable();
				AssertNoExceptionThrown(() => doc.OpenForEdit());
			});
			threadForViewingFile.Start();

			// Wait for 1 sec so the open of remote file is invoked, then delete remote file.
			Thread.Sleep(1000);
			doc.DeleteTempFile();

			threadForViewingFile.Join();
		}

		public void TestNoExceptionOnDeleteRemoteFileWhenSetImageData()
		{
			var doc = DocumentFactory.NewWithParent(typeof(StorageDocs));
			doc.ParentMain.SM_ParentFK = Factory.NewWithValidTestData<OrgHeader>().PK;
			doc.SC_FileName = "Sample";
			doc.SC_DataType = "pdf";

			var remoteFileMock = new Mock<IRemoteFile>();
			remoteFileMock.Setup(x => x.RemoteFilesSupported).Returns(true);
			remoteFileMock.Setup(x => x.Open()).Returns(true);
			remoteFileMock.Setup(x => x.FetchFileData()).Callback(() => { Thread.Sleep(3000); }).Returns(ZBlob.Empty);
			ObjectFactory.Substitute(remoteFileMock.Object);

			var threadForViewingFile = new Thread(() =>
			{
				using var syncContext = SynchronizationContextForTest.Enable();
				using var disposable = doc.OpenForEdit();
				AssertNoExceptionThrown(() => doc.SetImageData());
			});
			threadForViewingFile.Start();

			// Wait for 1 sec so SetImageData is invoked, then delete remote file.
			Thread.Sleep(1000);
			doc.DeleteTempFile();

			threadForViewingFile.Join();
		}

		[GuiTest]
		[TestRequiresAdministrativePrivileges("Monitoring other processes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1055:DoNotUseProcessGetProcesses", Justification = "Testing")]
		public void TestOpenForEditAndSave()
		{
			using (var syncContext = SynchronizationContextForTest.Enable())
			{
				var doc = DocumentFactory.NewWithParent(typeof(StorageDocs));
				doc.ParentMain.SM_ParentFK = Factory.NewWithValidTestData<OrgHeader>().PK;
				doc.ParentMain.SM_DB = 1;
				doc.SC_FileName = "small";
				var testFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif", "small.tif");
				doc.SC_ImageData = DocumentUtilities.GetFileAsBytes(testFile);
				doc.ReadOnly = false;
				DocumentFactory.Save();
				AssertEquals("UseRemoteFile", true, doc.UseRemoteFile);
				string remoteFileName;
				using (doc.OpenForEdit())
				{
					Thread.Sleep(5000);
					var fileName = Path.GetFileNameWithoutExtension(testFile);
					Process openProcess = null;
					foreach (var process in Process.GetProcesses())
					{
						if (process.MainWindowTitle.Contains(fileName))
						{
							openProcess = process;
							break;
						}
					}
					AssertNotNull(openProcess);
					try
					{
						remoteFileName = GetRemoteFilePath(testFile);
						AssertFileSameAsBytes(testFile, File.ReadAllBytes(remoteFileName));

						AssertEquals(false, doc.HasChanges);
						var newFileData = resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.colored.tif");
						File.WriteAllBytes(remoteFileName, newFileData);
						Assert("Expected remoteFile_FileChanged event to be raised", syncContext.WaitAndExecuteCallbacks(TimeSpan.FromSeconds(10)));
						AssertEquals(true, doc.HasChanges);

						doc.SetImageData();
						DocumentFactory.Save();
						AssertEquals(newFileData, doc.SC_ImageData);
					}
					finally
					{
						openProcess.Kill();
					}

					using (File.OpenWrite(remoteFileName))
					{
						AssertEquals(true, doc.IsTempFileOpen);
					}
					AssertEquals(false, doc.IsTempFileOpen);
				}
			}
		}

		[GuiTest]
		[TestRequiresAdministrativePrivileges("Monitoring other processes")]
		public void TestFileChangedActionForRemoteFile()
		{
			using var syncContext = SynchronizationContextForTest.Enable();
			var doc = DocumentFactory.NewWithParent(typeof(StorageDocs));
			doc.ParentMain.SM_ParentFK = Factory.NewWithValidTestData<OrgHeader>().PK;
			doc.ParentMain.SM_DB = 1;
			doc.SC_FileName = "small";

			var testFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif", "small.tif");
			doc.SC_ImageData = DocumentUtilities.GetFileAsBytes(testFile);
			doc.ReadOnly = false;
			DocumentFactory.Save();

			AssertEquals("UseRemoteFile", true, doc.UseRemoteFile);

			using (doc.OpenForEdit())
			{
				Thread.Sleep(5000);

				try
				{
					var remoteFileName = GetRemoteFilePath(testFile);
					AssertFileSameAsBytes(testFile, File.ReadAllBytes(remoteFileName));
					AssertEquals(false, doc.HasChanges);

					var newFileData = resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.colored.tif");
					File.WriteAllBytes(remoteFileName, newFileData);
					AssertEquals("Expected remoteFile_FileChanged event to be raised", expected: true, syncContext.WaitAndExecuteCallbacks(TimeSpan.FromSeconds(10)));
					AssertEquals("HasChanges should be set true", true, doc.HasChanges);
					AssertEquals("Property IsSC_ImageDataOutOfSync should be set true", expected: true, doc.IsSC_ImageDataOutOfSync);
					AssertEquals("Property ShouldSetImageDataForAll should be set true", expected: true, doc.ParentMain.ShouldSetImageDataForAll);

					doc.SetImageData();
					AssertEquals("Property IsSC_ImageDataOutOfSync should be set false", expected: false, doc.IsSC_ImageDataOutOfSync);
				}
				finally
				{
					var openProcess = ProcessLocator.Instance.GetCurrentUserVisibleProcesses().FirstOrDefault(p => p.MainWindowTitle.Contains(Path.GetFileNameWithoutExtension(testFile)));
					openProcess?.Kill();
				}
			}
		}

		public void TestOpenForEditCanHandleExceptionThrownWhenRetrieveFromS3()
		{
			var factory = new DbBackendDocumentFactory(new BusinessObjectFactory());
			var doc = StorageDocs.NewWithParent_DEBUG(factory);
			doc.SC_UncompressedSize = 10;
			factory.Save();
			Assert("PRE", doc.SC_ImageDataFromDb.IsEmpty);

			var persisterMock = new Mock<IExternalPersister>(MockBehavior.Strict);
			persisterMock.Setup(x => x.RetrieveStream(doc.PK)).Throws(new ExternalStorageObjectNotFoundException(doc.PK.ToString(), "Retrieve fail", null, null));
			var persisterProviderMock = new Mock<IExternalPersisterProvider>();
			persisterProviderMock.Setup(p => p.GetExternalPersister(Core.Constants.EDocsStorageProviders.Code.S3)).Returns(persisterMock.Object);
			ObjectFactory.Substitute(persisterProviderMock.Object);

			StorageDocsWithS3SupportTest.SetUpS3Registries();

			var exceptionThrown = false;
			try
			{
				doc.OpenForEdit();
			}
			catch (Exception ex)
			{
				exceptionThrown = true;
				AssertEquals("Exception should be thrown from OpenForEdit() so it can be handled correctly by the calling thread", "OpenForEdit", ex.TargetSite.Name);
			}

			Assert("Exception is not thrown", exceptionThrown);
		}

		protected override void SetUp()
		{
			ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest());
			base.SetUp();
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		DocumentFactory DocumentFactory
		{
			get
			{
				if (documentFactory == null)
				{
					documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
				}
				return documentFactory;
			}
		}
		DocumentFactory documentFactory;
	}
}
