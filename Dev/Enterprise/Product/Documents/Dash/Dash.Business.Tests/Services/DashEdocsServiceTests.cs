using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Dash.Business.Services;
using Enterprise.Dash.Integration;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.DocumentScanning.Integration;
using Moq;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Dash.Business.Tests
{
	public class DashEdocsServiceTests : TestCaseWithDocumentFactory
	{
		const string DocumentType = "CIV";
		const string DocumentFileType = "PDF";
		const string ShipmentEntityTypeCode = "SHP";

		public void TestGetDashEDocsDetails_Returns_DashEDocsDetails_When_Document_And_ActiveShipamaxMessage_Exist()
		{
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// arrange
				// We want SipamaxEdiMessage to be created automatically when StorageDoc is saved
				var shipment = GetShipment();
				var eDoc = CreateEDoc(DocumentType, DocumentFileType, shipment);

				var docMainId = eDoc.ParentMain.PK;
				var docId = eDoc.PK;
				var docToken = ZGuid.NewZGuid();

				MasterFactory.Save();

				var shipamaxServiceMock = new Mock<IShipamaxService>();
				var dashEdocsService = new DashEDocsService(shipamaxServiceMock.Object);

				// act
				var dashEDocsDetails = dashEdocsService.GetDashEDocsDetails(docMainId, docId, docToken.ToString());

				// assert
				AssertEquals(dashEDocsDetails.DocId, docId);
				AssertEquals(dashEDocsDetails.DocMainId, docMainId);
				AssertEquals(dashEDocsDetails.RelatedEntityId, shipment.PK);
				AssertEquals(dashEDocsDetails.RelatedEntityTypeCode, ShipmentEntityTypeCode);
				AssertEquals(dashEDocsDetails.RelatedBranchId, eDoc.ActiveShipamaxMessage.Branch.PK);
				AssertEquals(dashEDocsDetails.RelatedDepartmentId, eDoc.ActiveShipamaxMessage.Department.PK);
			}
		}

		public void TestGetDashEDocsDetails_Throws_DashException_When_StorageMain_Does_Not_Exist()
		{
			// arrange
			var shipment = GetShipment();
			var eDoc = CreateEDoc(DocumentType, DocumentFileType, shipment);

			var docId = eDoc.PK;
			var docToken = ZGuid.NewZGuid();
			var incorrectStorageMainId = ZGuid.NewZGuid();

			MasterFactory.Save();

			var shipamaxServiceMock = new Mock<IShipamaxService>();
			var dashEdocsService = new DashEDocsService(shipamaxServiceMock.Object);
			var nonExistingStorageMainPk = ZGuid.NewZGuid();

			// act
			var exception = AssertExceptionThrown<DashException>(() => dashEdocsService.GetDashEDocsDetails(incorrectStorageMainId, docId, docToken.ToString()));

			AssertEquals(exception.Message, $"Can't find StorageMain record with ID: {incorrectStorageMainId}");
		}

		public void TestGetDashEDocsDetails_Returns_DashException_When_ActiveShipamaxEdiMessage_Does_Not_Exist()
		{
			// arrange
			// We don't want SipamaxEdiMessage to be created automatically when StorageDoc is saved
			using (DocManagerRegistry.Instance.SetTemporaryDocParsingRegistryValues(false))
			{
				var shipment = GetShipment();
				var eDoc = CreateEDoc(DocumentType, DocumentFileType, shipment);

				var docMainId = eDoc.ParentMain.PK;
				var docId = eDoc.PK;
				var docToken = ZGuid.NewZGuid();

				MasterFactory.Save();

				var shipamaxServiceMock = new Mock<IShipamaxService>();
				var dashEdocsService = new DashEDocsService(shipamaxServiceMock.Object);

				// act
				var exception = AssertExceptionThrown<DashException>(() => dashEdocsService.GetDashEDocsDetails(docMainId, docId, docToken.ToString()));

				AssertEquals(exception.Message, $"Can't find related EDocsShipamaxMessage");
			}
		}

		IForwardingShipment GetShipment() => (IForwardingShipment)MasterFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(IForwardingShipment)));

		StorageDocsBase CreateEDoc(string documentType, string documentFileType, IForwardingShipment shipment)
		{
			var factory = MasterFactory.GetFactory(1);
			var eDoc = factory.NewWithParent(typeof(StorageFile));
			eDoc.SC_DocType = documentType;
			eDoc.SC_DataType = documentFileType;
			eDoc.ParentMain.SM_DB = 1;
			eDoc.ParentMain.SM_ParentFK = shipment.PK;
			eDoc.ParentMain.SM_Type = Enterprise.Core.Constants.DocManagerCodes.Shipment;

			return eDoc;
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

		readonly DocManagerDBHelperTestClass dbHelper = new ();
	}
}
