using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Dash.Integration.Services;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Web.Test
{
	public class ShipamaxServiceTest : TestCaseWithDocumentFactory
	{
		#region ValidateDocToken

		public void TestValidateDocToken_TokenFailedToBeDecrypted()
		{
			AssertValidateDocToken(DocTokenErrorType.InvalidTokenError);
		}

		public void TestValidateDocToken_EDocPKNotMatch()
		{
			AssertValidateDocToken(DocTokenErrorType.DocPKError);
		}

		public void TestValidateDocToken_DocTypeNotMatch()
		{
			AssertValidateDocToken(DocTokenErrorType.DocTypeError);
		}

		public void TestValidateDocToken_DataTypeNotMatch()
		{
			AssertValidateDocToken(DocTokenErrorType.DataTypeError);
		}

		public void TestValidateDocToken_SC_DateNotMatch()
		{
			AssertValidateDocToken(DocTokenErrorType.LastEditTimeError);
		}

		public void TestValidateDocToken_Passed()
		{
			AssertValidateDocToken(null);
		}

		#endregion

		#region GetEDocsShipamaxMessage

		public void TestGetEDocsShipamaxMessage_ValidationFailed()
		{
			var serviceMock = new Mock<ShipamaxServiceForTest>();
			serviceMock.Protected()
				.Setup<bool>("ValidateDocToken", ItExpr.IsAny<Guid>(), ItExpr.IsAny<string>(), ItExpr.Ref<EDocsShipamaxMessage>.IsAny)
				.Returns(false)
				.Verifiable("ValidateDocToken is not called in expectation");
			serviceMock.Protected()
				.Setup("GetEDocsShipamaxMessage", ItExpr.IsAny<Guid>(), ItExpr.IsAny<string>())
				.CallBase()
				.Verifiable("GetEDocsShipamaxMessage is not called in expectation");

			var exception = AssertExceptionThrown<ShipamaxServiceException>(() => serviceMock.Object.GetEDocsShipamaxMessageExposed(Guid.Empty, string.Empty));
			serviceMock.Verify();

			AssertEquals("Error type should be ValidationError", ShipamaxServiceErrorType.ValidationError, exception.ErrorType);
			AssertEquals("ValidationError happens. Error message: The eDoc authorization token is invalid.", exception.Message);
		}

		public void TestGetEDocsShipamaxMessage_InactiveMessage()
		{
			var serviceMock = new Mock<ShipamaxServiceForTest>();
			serviceMock.Protected()
				.Setup<bool>("ValidateDocToken", ItExpr.IsAny<Guid>(), ItExpr.IsAny<string>(), ItExpr.Ref<EDocsShipamaxMessage>.IsAny)
				.Callback(new ValidateDocTokenCallBack((Guid _, string _, out EDocsShipamaxMessage message) =>
				{
					message = MasterFactory.New<EDocsShipamaxMessage>();
					message.EM_IsActive = false;
				}))
				.Returns(true)
				.Verifiable("ValidateDocToken is not called in expectation");
			serviceMock.Protected()
				.Setup("GetEDocsShipamaxMessage", ItExpr.IsAny<Guid>(), ItExpr.IsAny<string>())
				.CallBase()
				.Verifiable("GetEDocsShipamaxMessage is not called in expectation");

			var exception = AssertExceptionThrown<ShipamaxServiceException>(() => serviceMock.Object.GetEDocsShipamaxMessageExposed(Guid.Empty, string.Empty));
			serviceMock.Verify();

			AssertEquals("Error type should be StatusError", ShipamaxServiceErrorType.StatusError, exception.ErrorType);
			AssertEquals("StatusError happens. Error message: The operation cannot be performed on an inactive record.", exception.Message);
		}

		public void TestGetEDocsShipamaxMessage_MessageStatus()
		{
			var testedShipamaxMessage = MasterFactory.New<EDocsShipamaxMessage>();
			var serviceMock = new Mock<ShipamaxServiceForTest>();
			serviceMock.Protected()
				.Setup<bool>("ValidateDocToken", ItExpr.IsAny<Guid>(), ItExpr.IsAny<string>(), ItExpr.Ref<EDocsShipamaxMessage>.IsAny)
				.Callback(new ValidateDocTokenCallBack((Guid _, string _, out EDocsShipamaxMessage message) =>
				{
					message = testedShipamaxMessage;
				}))
				.Returns(true)
				.Verifiable("ValidateDocToken is not called in expectation");
			serviceMock.Protected()
				.Setup("GetEDocsShipamaxMessage", ItExpr.IsAny<Guid>(), ItExpr.IsAny<string>())
				.CallBase()
				.Verifiable("GetEDocsShipamaxMessage is not called in expectation");

			var exception = AssertExceptionThrown<ShipamaxServiceException>(() => serviceMock.Object.GetEDocsShipamaxMessageExposed(Guid.Empty, string.Empty));
			serviceMock.Verify();

			AssertEquals("Error type should be StatusError", ShipamaxServiceErrorType.StatusError, exception.ErrorType);
			AssertEquals("StatusError happens. Error message: The operation cannot be performed on a record in status 'QUE'.", exception.Message);

			testedShipamaxMessage.EM_Status = "FAL";
			exception = AssertExceptionThrown<ShipamaxServiceException>(() => serviceMock.Object.GetEDocsShipamaxMessageExposed(Guid.Empty, string.Empty));
			serviceMock.Verify();

			AssertEquals("Error type should be StatusError", ShipamaxServiceErrorType.StatusError, exception.ErrorType);
			AssertEquals("StatusError happens. Error message: The operation cannot be performed on a record in status 'FAL'.", exception.Message);

			testedShipamaxMessage.EM_Status = "DCD";
			exception = AssertExceptionThrown<ShipamaxServiceException>(() => serviceMock.Object.GetEDocsShipamaxMessageExposed(Guid.Empty, string.Empty));
			serviceMock.Verify();

			AssertEquals("Error type should be StatusError", ShipamaxServiceErrorType.StatusError, exception.ErrorType);
			AssertEquals("StatusError happens. Error message: The operation cannot be performed on a record in status 'DCD'.", exception.Message);

			testedShipamaxMessage.EM_Status = "SNT";
			AssertNoExceptionThrown(() => serviceMock.Object.GetEDocsShipamaxMessageExposed(Guid.Empty, string.Empty));
			serviceMock.Verify();

			testedShipamaxMessage.EM_Status = "PPS";
			AssertNoExceptionThrown(() => serviceMock.Object.GetEDocsShipamaxMessageExposed(Guid.Empty, string.Empty));
			serviceMock.Verify();

			testedShipamaxMessage.EM_Status = "PRS";
			AssertNoExceptionThrown(() => serviceMock.Object.GetEDocsShipamaxMessageExposed(Guid.Empty, string.Empty));
			serviceMock.Verify();
		}

		#endregion

		#region SaveParseResult

		public void TestSaveParseResult_RegistryNotEnabled()
		{
			using (DocManagerRegistry.Instance.SetTemporaryDocParsingRegistryValues(false))
			{
				var exception = AssertExceptionThrown<ShipamaxServiceException>(() => new ShipamaxServiceForTest().SaveParseResult(Guid.Empty, string.Empty, new ShipamaxParseResult()));

				AssertEquals("Error type should be ConfigurationError", ShipamaxServiceErrorType.ConfigurationError, exception.ErrorType);
				AssertEquals("ConfigurationError happens. Error message: Document Ingestion service is not enabled.", exception.Message);
			}
		}

		public void TestSaveParseResult_ExceptionThrownOut_WhenFailedToGetShipamaxMessage()
		{
			var serviceMock = new Mock<ShipamaxServiceForTest>();
			serviceMock.Protected()
				.Setup("GetEDocsShipamaxMessage", ItExpr.IsAny<Guid>(), ItExpr.IsAny<string>())
				.Throws(new ShipamaxServiceException(ShipamaxServiceErrorType.ValidationError, "Invalid Token."))
				.Verifiable("GetEDocsShipamaxMessage is not called in expectation");

			var exception = AssertExceptionThrown<ShipamaxServiceException>(() => serviceMock.Object.SaveParseResult(Guid.Empty, string.Empty, new ShipamaxParseResult()));
			serviceMock.Verify();

			AssertEquals("Error type should be ValidationError", ShipamaxServiceErrorType.ValidationError, exception.ErrorType);
			AssertEquals("ValidationError happens. Error message: Invalid Token.", exception.Message);
		}

		public void TestSaveParseResult_ClearExistingData()
		{
			var eDoc = GetNewStorageDocsBaseWithParent(1);
			var shipamaxMessage = eDoc.ActiveShipamaxMessage;
			var serviceMock = new Mock<ShipamaxServiceForTest>();
			serviceMock.Protected()
				.Setup<EDocsShipamaxMessage>("GetEDocsShipamaxMessage", ItExpr.IsAny<Guid>(), ItExpr.IsAny<string>())
				.Returns(shipamaxMessage)
				.Verifiable("GetEDocsShipamaxMessage is not called in expectation");

			var parseResult = new ShipamaxParseResult
			{
				JsonParseResult = null,
				XmlParseResult = null
			};
			shipamaxMessage.MessageData = new EDocsShipamaxMessageData("Json", "Xml");
			serviceMock.Object.SaveParseResult(Guid.Empty, string.Empty, parseResult);
			serviceMock.Verify();

			AssertNull("Existing json data is cleared", shipamaxMessage.MessageData.ParseDataJson);
			AssertNull("Existing xml data is cleared", shipamaxMessage.MessageData.ParseResultXml);

			var newMasterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var reloadedShipamaxMessage = newMasterFactory.Load<EDocsShipamaxMessage>(shipamaxMessage.PK);

			AssertNull("Existing json data is cleared", reloadedShipamaxMessage.MessageData.ParseDataJson);
			AssertNull("Existing xml data is cleared", reloadedShipamaxMessage.MessageData.ParseResultXml);
		}

		public void TestSaveParseResult_NoChangeOnExistingResultData()
		{
			var eDoc = GetNewStorageDocsBaseWithParent(1);
			var shipamaxMessage = eDoc.ActiveShipamaxMessage;
			var serviceMock = new Mock<ShipamaxServiceForTest>();
			serviceMock.Protected()
				.Setup<EDocsShipamaxMessage>("GetEDocsShipamaxMessage", ItExpr.IsAny<Guid>(), ItExpr.IsAny<string>())
				.Returns(shipamaxMessage)
				.Verifiable("GetEDocsShipamaxMessage is not called in expectation");

			var parseResult = new ShipamaxParseResult
			{
				JsonParseResult = string.Empty,
				XmlParseResult = string.Empty
			};

			serviceMock.Object.SaveParseResult(Guid.Empty, string.Empty, parseResult);
			serviceMock.Verify();

			AssertNull("No change on existing json data", shipamaxMessage.MessageData.ParseDataJson);
			AssertNull("No change on existing xml data", shipamaxMessage.MessageData.ParseResultXml);

			var newMasterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var reloadedShipamaxMessage = newMasterFactory.Load<EDocsShipamaxMessage>(shipamaxMessage.PK);

			AssertNull("No change on existing json data", reloadedShipamaxMessage.MessageData.ParseDataJson);
			AssertNull("No change on existing xml data", reloadedShipamaxMessage.MessageData.ParseResultXml);
		}

		public void TestSaveParseResult_ResultDataIsUpdated()
		{
			var eDoc = GetNewStorageDocsBaseWithParent(1);
			var shipamaxMessage = eDoc.ActiveShipamaxMessage;
			var serviceMock = new Mock<ShipamaxServiceForTest>();
			serviceMock.Protected()
				.Setup<EDocsShipamaxMessage>("GetEDocsShipamaxMessage", ItExpr.IsAny<Guid>(), ItExpr.IsAny<string>())
				.Returns(shipamaxMessage)
				.Verifiable("GetEDocsShipamaxMessage is not called in expectation");

			var parseResult = new ShipamaxParseResult
			{
				JsonParseResult = "New Json",
				XmlParseResult = "New Xml"
			};

			serviceMock.Object.SaveParseResult(Guid.Empty, string.Empty, parseResult);
			serviceMock.Verify();

			AssertEquals("New Json", shipamaxMessage.MessageData.ParseDataJson);
			AssertEquals("New Xml", shipamaxMessage.MessageData.ParseResultXml);

			var newMasterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var reloadedShipamaxMessage = newMasterFactory.Load<EDocsShipamaxMessage>(shipamaxMessage.PK);

			AssertEquals("New Json", reloadedShipamaxMessage.MessageData.ParseDataJson);
			AssertEquals("New Xml", reloadedShipamaxMessage.MessageData.ParseResultXml);
		}

		public void TestSaveParseResult_LogDocumentParseStatsUpdateEvent_Complete()
		{
			var dashUtilsMock = new Mock<IDashUtils>();
			_ = dashUtilsMock.Setup(x => x.GetCorrectionToolUrl(It.IsAny<string>())).Returns("testUrl");
			_ = ObjectFactory.Substitute(dashUtilsMock.Object);

			TestCaseHelper.ClearTable(StmALogSchema.Constants.TableName);
			var log = GetDocumentParseStatsUpdateEventForStatus(ShipamaxParseStatus.Complete);
			AssertEquals($"{log.Item1}|NEW=PRS|OLD=QUE|LINK=testUrl", log.Item2.SL_Reference);
		}

		public void TestSaveParseResult_LogDocumentParseStatsUpdateEvent_NeedReview()
		{
			var dashUtilsMock = new Mock<IDashUtils>();
			_ = dashUtilsMock.Setup(x => x.GetCorrectionToolUrl(It.IsAny<string>())).Returns("testUrl");
			_ = ObjectFactory.Substitute(dashUtilsMock.Object);

			var log = GetDocumentParseStatsUpdateEventForStatus(ShipamaxParseStatus.NeedReview);
			AssertEquals($"{log.Item1}|NEW=PPS|OLD=QUE|LINK=testUrl", log.Item2.SL_Reference);
		}

		public void TestSaveParseResult_LogDocumentParseStatsUpdateEvent_Failed()
		{
			var log = GetDocumentParseStatsUpdateEventForStatus(ShipamaxParseStatus.Failed);
			AssertNull("No event log shold be created for Failed status", log.Item2);
		}

		public void TestSaveParseResult_LogDocumentParseStatsUpdateEvent_SameStatus()
		{
			var log = GetDocumentParseStatsUpdateEventForStatus(ShipamaxParseStatus.Complete, "PRS");
			AssertNull("No event log shold be created if old status is the same to new status", log.Item2);
		}

		public void TestSaveParseResult_FromWebUser()
		{
			using (Env.SetTemporaryUserContext(User.WebUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var eDoc = GetNewStorageDocsBaseWithParent(1);
				var shipamaxMessage = eDoc.ActiveShipamaxMessage;
				var serviceMock = new Mock<ShipamaxServiceForTest>();
				serviceMock.Protected()
					.Setup<EDocsShipamaxMessage>("GetEDocsShipamaxMessage", ItExpr.IsAny<Guid>(), ItExpr.IsAny<string>())
					.Returns(shipamaxMessage)
					.Verifiable("GetEDocsShipamaxMessage is not called in expectation");

				var parseResult = new ShipamaxParseResult
				{
					JsonParseResult = null,
					XmlParseResult = null
				};
				shipamaxMessage.MessageData = new EDocsShipamaxMessageData("Json", "Xml");
				serviceMock.Object.SaveParseResult(Guid.Empty, string.Empty, parseResult);
				serviceMock.Verify();
				AssertEquals("ErrorReporter should not report error for switching context.", string.Empty, ErrorReporter.LastMessageReported);
			}
		}

		(ZGuid, StmALog) GetDocumentParseStatsUpdateEventForStatus(ShipamaxParseStatus status, string oldStatus = "QUE")
		{
			var eDoc = GetNewStorageDocsBaseInShipment();
			var shipamaxMessage = eDoc.ActiveShipamaxMessage;
			shipamaxMessage.EM_Status = oldStatus;
			var serviceMock = new Mock<ShipamaxServiceForTest>();
			serviceMock.Protected()
				.Setup<EDocsShipamaxMessage>("GetEDocsShipamaxMessage", ItExpr.IsAny<Guid>(), ItExpr.IsAny<string>())
				.Returns(shipamaxMessage);

			var parseResult = new ShipamaxParseResult
			{
				JsonParseResult = "New Json",
				XmlParseResult = "New Xml",
				ParseStatus = status
			};

			serviceMock.Object.SaveParseResult(eDoc.PK.ToGuid(), string.Empty, parseResult);
			return (eDoc.PK, MasterFactory.LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_SE_NKEvent, "DPS").AddToFilter(StmALogSchema.SL_Parent, eDoc.ParentMain.SM_ParentFK)));
		}

		#endregion

		#region CheckEDocsChanges

		public void TestCheckEDocsChanges_ChangeProperties()
		{
			var eDoc = GetNewStorageDocsBaseInShipment();
			var shipamaxMessage = eDoc.ActiveShipamaxMessage;
			var tokenManager = new ShipamaxIntegrationTokenManager();
			var docToken = tokenManager.GenerateEDocsAuthToken(shipamaxMessage.PK.ToGuid(), eDoc);

			eDoc.IsParsingEnabled = false;
			eDoc.HasChanges = true;
			MasterFactory.Save();

			var docChanges = ObjectFactory.Get<IShipamaxService>().CheckEDocsChanges(eDoc.PK.ToGuid(), docToken).ToArray();
			AssertEquals(1, docChanges.Length);

			var isParsingEnabledChange = docChanges.SingleOrDefault(x => x.Field == ShipamaxEDocsChangeType.ParsingEnabled);
			AssertNotNull(isParsingEnabledChange);
			AssertEquals(true, isParsingEnabledChange.OldValue);
			AssertEquals(false, isParsingEnabledChange.NewValue);

			var oldDocType = eDoc.SC_DocType;
			var oldDataType = eDoc.SC_DataType;
			var oldDate = eDoc.SC_Date;

			eDoc.SC_DocType = "PKL";
			eDoc.SC_DataType = "JPEG";
			eDoc.SC_Date = new ZDateTime(2024, 1, 1);
			MasterFactory.Save();

			docChanges = ObjectFactory.Get<IShipamaxService>().CheckEDocsChanges(eDoc.PK.ToGuid(), docToken).ToArray();
			AssertEquals(3, docChanges.Length);

			var docTypeChange = docChanges.SingleOrDefault(x => x.Field == ShipamaxEDocsChangeType.DocType);
			AssertNotNull(docTypeChange);
			AssertEquals(oldDocType, docTypeChange.OldValue);
			AssertEquals(eDoc.SC_DocType, docTypeChange.NewValue);

			var dataTypeChange = docChanges.SingleOrDefault(x => x.Field == ShipamaxEDocsChangeType.DocFormat);
			AssertNotNull(dataTypeChange);
			AssertEquals(oldDataType, dataTypeChange.OldValue);
			AssertEquals(eDoc.SC_DataType, dataTypeChange.NewValue);

			var dateChange = docChanges.SingleOrDefault(x => x.Field == ShipamaxEDocsChangeType.DocEditTime);
			AssertNotNull(dateChange);
			AssertEquals(oldDate, dateChange.OldValue);
			AssertEquals(eDoc.SC_Date, dateChange.NewValue);
		}

		public void TestCheckEDocsChanges_SoftDelete()
		{
			var eDoc = GetNewStorageDocsBaseInShipment();
			var shipamaxMessage = eDoc.ActiveShipamaxMessage;
			var tokenManager = new ShipamaxIntegrationTokenManager();
			var docToken = tokenManager.GenerateEDocsAuthToken(shipamaxMessage.PK.ToGuid(), eDoc);

			eDoc.DeleteQuietly();
			MasterFactory.Save();

			var docChanges = ObjectFactory.Get<IShipamaxService>().CheckEDocsChanges(eDoc.PK.ToGuid(), docToken).ToArray();
			AssertEquals(1, docChanges.Length);

			var deleteChange = docChanges.SingleOrDefault(x => x.Field == ShipamaxEDocsChangeType.Deleted);
			AssertNotNull(deleteChange);
			AssertEquals(false, deleteChange.OldValue);
			AssertEquals(true, deleteChange.NewValue);
		}

		public void TestCheckEDocsChanges_PermanentDelete()
		{
			var eDoc = GetNewStorageDocsBaseInShipment();
			var shipamaxMessage = eDoc.ActiveShipamaxMessage;
			var tokenManager = new ShipamaxIntegrationTokenManager();
			var docToken = tokenManager.GenerateEDocsAuthToken(shipamaxMessage.PK.ToGuid(), eDoc);

			eDoc.Delete();
			MasterFactory.Save();

			var docChanges = ObjectFactory.Get<IShipamaxService>().CheckEDocsChanges(eDoc.PK.ToGuid(), docToken).ToArray();
			AssertEquals(1, docChanges.Length);

			var pkChange = docChanges.SingleOrDefault(x => x.Field == ShipamaxEDocsChangeType.DocumentPK);
			AssertNotNull(pkChange);
			AssertEquals(eDoc.PK.ToGuid(), pkChange.OldValue);
			AssertEquals(Guid.Empty, pkChange.NewValue);
		}

		#endregion

		#region Implementation

		void AssertValidateDocToken(DocTokenErrorType? errorType)
		{
			DocManagerRegistry.Instance.EDocsAuthTokenEncryptionKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "EoQNIn4lnxcKPmSH2RPACA==");

			var eDoc = GetNewStorageDocsBaseWithParent(1);
			var tokenDetails = new EDocsAuthTokenDetails(eDoc.ActiveShipamaxMessage.PK.ToGuid(), eDoc.SC_Date.ToDateTime(), eDoc.SC_DocType, eDoc.SC_DataType);
			var pk = eDoc.PK.ToGuid();
			string token = null;

			if (errorType.HasValue)
			{
				switch (errorType.Value)
				{
					case DocTokenErrorType.DocPKError:
						pk = Guid.Empty;
						break;
					case DocTokenErrorType.DataTypeError:
						tokenDetails.DataType = "TIF";
						break;
					case DocTokenErrorType.DocTypeError:
						tokenDetails.DocType = "MBL";
						break;
					case DocTokenErrorType.LastEditTimeError:
						tokenDetails.EDocLastEditTime = DateTime.UtcNow;
						break;
					case DocTokenErrorType.InvalidTokenError:
						token = "h98H1bmxDuGgAkraqyNo4r2S2Gt9xyWvcKl7zS/kxYeWhw9v6qW/jJsii4mpqkINuTguP0TRP8Kmed/WpBGMbFP8/JM5GKUKp1nKf1RRlcwaXArgLUQ08XAiojZYOgu4auWfm+ydy3NxWXjmc/tWyjGvWqo";
						break;
				}
			}

			token ??= new ShipamaxIntegrationTokenManager().GenerateEDocsAuthToken(tokenDetails);
			var result = new ShipamaxServiceForTest().ValidateDocTokenExposed(pk, token, out var shipamaxMessage);
			var expectedResult = !errorType.HasValue;

			AssertEquals($"Validation on doc token should be '{expectedResult}'", expectedResult, result);

			if (expectedResult)
			{
				AssertNotNull("EDosShipamaxMessage should not be null if validation is passed", shipamaxMessage);
			}
		}

		StorageDocsBase GetNewStorageDocsBaseWithParent(int databaseNumber, bool shouldBeSaved = true)
		{
			var factory = MasterFactory.GetFactory(databaseNumber);
			var eDoc = factory.NewWithParent(typeof(StorageFile));
			eDoc.SC_FileName = "test";
			eDoc.SC_DocType = "CIV";
			eDoc.SC_DataType = "PDF";
			eDoc.SC_Date = new ZDateTime(2023, 12, 23);
			eDoc.SC_UncompressedSize = 1222;
			eDoc.ParentMain.SM_DB = databaseNumber;
			eDoc.ParentMain.SM_ParentFK = ZGuid.NewZGuid();
			eDoc.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;

			if (shouldBeSaved)
			{
				MasterFactory.Save();
			}

			return eDoc;
		}

		StorageDocsBase GetNewStorageDocsBaseInShipment()
		{
			var eDoc = GetNewStorageDocsBaseWithParent(1, false);
			var shipment = MasterFactory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>());
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00001000";
			eDoc.ParentMain.SM_ParentFK = shipment.PK;
			MasterFactory.Save();

			return eDoc;
		}

		protected override void SetUp()
		{
			base.SetUp();
			DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

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

			DropDatabase(1);
		}

		void DropDatabase(int dbNumber)
		{
			if (dbHelper.DatabaseExists(dbNumber))
			{
				var dbName = dbHelper.GetDatabaseName(dbNumber);
				dbHelper.DropDatabase(dbName);
			}
		}

		delegate void ValidateDocTokenCallBack(Guid docPK, string docToken, out EDocsShipamaxMessage shipamaxMessage);

		readonly DocManagerDBHelperTestClass dbHelper = new ();

		enum DocTokenErrorType
		{
			DocPKError,
			DocTypeError,
			DataTypeError,
			LastEditTimeError,
			InvalidTokenError
		}

		#endregion
	}
}
