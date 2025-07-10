using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.Dash.Business.Tests
{
	public class DashParseRequestServiceTests : TestCaseWithDocumentFactory
	{
		public void TestCreates_ParseRequest()
		{
			var storageMain = MasterFactory.NewWithValidTestData<StorageMain>();
			var storageDocMock = new Mock<IeDoc>();
			var storageDocPk = ZGuid.NewZGuid();
			storageDocMock.SetupGet(x => x.UniqueKey).Returns(storageDocPk);
			storageDocMock.SetupGet(x => x.ParentMain).Returns(storageMain);

			var dashParseRequestService = new DashParseRequestService(MasterFactory);
			var message = (EDocsShipamaxMessage)dashParseRequestService.Create(storageDocMock.Object);

			AssertEquals(storageDocPk, message.EM_LinkUniqueID);
			AssertEquals(AutoStorageDocs.Schema.TableName, message.EM_LinkTable);
			AssertEquals(storageMain.PK.ToString(), message.EM_ApplicationReference);
		}

		public void TestCancel_Makes_ParseRequest_Inactive()
		{
			var message = MasterFactory.NewWithValidTestData<EDocsShipamaxMessage>();
			var dashParseRequestService = new DashParseRequestService(MasterFactory);
			dashParseRequestService.Cancel(message);

			AssertEquals(false, message.EM_IsActive);
		}

		public void TestGet_Returns_ParseRequest()
		{
			var storageDocPk = ZGuid.NewZGuid();
			var message = MasterFactory.NewWithValidTestData<EDocsShipamaxMessage>();
			message.EM_LinkUniqueID = storageDocPk;
			message.EM_LinkTable = AutoStorageDocs.Schema.TableName;

			MasterFactory.Save();

			var dashParseRequestService = new DashParseRequestService(MasterFactory);
			var parseRequest = dashParseRequestService.Get(storageDocPk.ToGuid());

			AssertNotNull(parseRequest);
		}

		public void TestExists()
		{
			var storageDocPk = ZGuid.NewZGuid();
			var message = MasterFactory.NewWithValidTestData<EDocsShipamaxMessage>();
			message.EM_LinkUniqueID = storageDocPk;
			message.EM_LinkTable = AutoStorageDocs.Schema.TableName;
			message.EM_IsActive = true;

			MasterFactory.Save();

			var dashParseRequestService = new DashParseRequestService(MasterFactory);
			AssertEquals(true, dashParseRequestService.Exists(storageDocPk.ToGuid()));
		}
	}
}
