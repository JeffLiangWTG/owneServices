using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.BatchProcessor;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.CustomerService.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.EDI.ERequestDocumenting.BatchProcessor.Testing
{
	public class ERequestDocumentProcessorTest : TestCaseWithFactory
	{
		public void TestProcess()
		{
			var request = Factory.NewWithValidTestData<IncidentRequest>();
			request.INC_IncidentNumber = "INC1024";
			request.INC_ReferenceID = ZGuid.NewZGuid();

			Factory.Save();

			var doc1 = new Xsd.ERequestDocument()
			{ ReferenceId = request.INC_ReferenceID.ToString() };
			var attachment1 = doc1.Attachments.AddNew();
			attachment1.FileName = "attachment1";
			attachment1.Data = CreateImage(Color.Red);
			attachment1.IsPublished = true;

			var doc2 = new Xsd.ERequestDocument()
			{ ReferenceId = new ZGuid().ToString() };
			var attachment2 = doc2.Attachments.AddNew();
			attachment2.FileName = "attachment2";
			attachment2.Data = CreateImage(Color.Blue);
			attachment2.IsPublished = true;

			AssertProcess(doc1, true);
			AssertProcess(doc2, false);
		}

		public void TestExceptionHandling()
		{
			var request1 = Factory.NewWithValidTestData<IncidentRequest>();
			request1.INC_IncidentNumber = "INC1024";
			request1.INC_ReferenceID = ZGuid.NewZGuid();
			Factory.Save();
			var doc1 = new Xsd.ERequestDocument()
			{ ReferenceId = request1.INC_ReferenceID.ToString() };
			var attachment1 = doc1.Attachments.AddNew();
			attachment1.FileName = "attachment1";
			attachment1.Data = CreateImage(Color.Red);
			var logger = new LoggerForTest();
			var processor = new ERequestDocumentProcessor(logger);
			//ConcurrencyException
			ProcessWithException(() => processor.Process(doc1), new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(), null, null), null));
			//Duplicate Key
			ProcessWithException(() => processor.Process(doc1), new ZSaveException(new ZDataException(SqlExceptionBuilder.CreateSqlException(SqlExceptionBuilder.CreateSqlErrorCollection(SqlExceptionBuilder.CreateSqlError(2601, 0, 0, "server", "Cannot insert duplicate key row in object 'dbo.StorageMain' with unique index 'NR_UC__SM_ParentFK'. The duplicate key value is (b414c3e4-0e1a-49cb-9180-e257953cedb2). ", "DoSomething", 123))), null, null), null));
			//What Really Matters
			try
			{
				ProcessWithException(() => processor.Process(doc1), new SystemException());
				Fail();
			}
			catch (SystemException)
			{
			}

			var queues = new BusinessObjectFactory().Load<EdiERequestDocumentQueue>(new ZQuery(EdiERequestDocumentQueueSchema.EDQ_INC_ReferenceID, request1.INC_ReferenceID));
			AssertEquals(2, queues.Length);
		}

		void ProcessWithException(Action processAction, Exception ex)
		{
			var actionCount = 0;
			Action action = () =>
			{
				if (actionCount == 0)
				{
					actionCount++;
					throw ex;
				}
			};
			var listenerMock = new Mock<ITransactionParticipantListener>();
			listenerMock.Setup(x => x.FactorySaveBeginning(It.IsAny<ITransactionParticipant[]>())).Callback(action);
			try
			{
				BusinessObjectFactory.RegisterListener(listenerMock.Object);
				processAction();
			}
			finally
			{
				BusinessObjectFactory.UnRegisterListener(listenerMock.Object);
			}
		}

		byte[] CreateImage(Color color)
		{
			using (var bitmap = new Bitmap(1, 1))
			{
				using (var ms = new MemoryStream())
				{
					bitmap.SetPixel(0, 0, color);
					bitmap.Save(ms, ImageFormat.Png);
					return ms.GetBuffer();
				}
			}
		}

		void AssertProcess(Xsd.ERequestDocument requestDoc, bool shouldAttache)
		{
			var logger = new LoggerForTest();
			var processor = new ERequestDocumentProcessor(logger);
			processor.Process(requestDoc);
			var factory = new BusinessObjectFactory();
			var request = factory.LoadTop1<IncidentRequest>(new ZQuery(IncidentRequestSchema.INC_ReferenceID, new ZGuid(requestDoc.ReferenceId)));
			var queue = factory.LoadTop1<EdiERequestDocumentQueue>(new ZQuery(EdiERequestDocumentQueueSchema.EDQ_INC_ReferenceID, new ZGuid(requestDoc.ReferenceId)));
			if (shouldAttache)
			{
				AssertNotNull(request);
				AssertNull(queue);
				AssertEquals(1, request.DocManagerInfo.AllEDocs.Count);
				AssertEquals(requestDoc.Attachments[0].FileName, request.DocManagerInfo.AllEDocs[0].FileName);
				AssertEquals(requestDoc.Attachments[0].IsPublished, request.DocManagerInfo.AllEDocs[0].IsPublished);
				AssertEquals(true, request.DocManagerInfo.AllEDocs[0].IsPublished);
				AssertEquals(requestDoc.Attachments[0].Data, request.DocManagerInfo.AllEDocs[0].GetImageDataReader().ConvertToByteArrayAndCloseStream());
				AssertNull(factory.LoadTop1<EdiERequestDocumentQueue>(new ZQuery(EdiERequestDocumentQueueSchema.EDQ_INC_ReferenceID, new ZGuid(requestDoc.ReferenceId))));
			}
			else
			{
				AssertNull(request);
				AssertNotNull(queue);
				AssertEquals(requestDoc.Attachments[0].FileName, queue.EDQ_FileName);
				AssertEquals(requestDoc.Attachments[0].IsPublished, queue.EDQ_IsPublished);
				AssertEquals(true, queue.EDQ_IsPublished);
				AssertEquals(requestDoc.Attachments[0].Data, queue.EDQ_Data);
			}
		}
	}
}
