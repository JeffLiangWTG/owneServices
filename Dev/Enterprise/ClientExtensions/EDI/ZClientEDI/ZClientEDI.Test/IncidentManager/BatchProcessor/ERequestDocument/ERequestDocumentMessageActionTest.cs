using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.CustomerService.Business;
using Enterprise.eHubMessaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.EDI.IncidentManager.BatchProcessor.Testing
{
	public class ERequestDocumentMessageActionTest : TestCaseWithFactory
	{
		public void TestExecuteAction()
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
			var doc2 = new Xsd.ERequestDocument()
			{ ReferenceId = new ZGuid().ToString() };
			var attachment2 = doc2.Attachments.AddNew();
			attachment2.FileName = "attachment2";
			attachment2.Data = CreateImage(Color.Blue);
			AssertExecuteAction(doc1, true);
			AssertExecuteAction(doc2, false);
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

		void AssertExecuteAction(Xsd.ERequestDocument requestDoc, bool shouldAttache)
		{
			var action = new ERequestDocumentMessageAction(null);
			var serializer = ZXmlSerializer.New(typeof(Xsd.ERequestDocument));
			var interchange = SystemMessage.CreateSecureInterchange(Factory, SystemMessageList.Descriptions.ERequestDocument, serializer, requestDoc);
			var message = SystemMessage.DebugOnlyCreateDownloadedMessage(interchange);
			((IMessageAction)action).ExecuteAction(message, null, out List<ITransactionParticipant> participant);
			var factory = new BusinessObjectFactory();
			var request = factory.LoadTop1<IncidentRequest>(new ZQuery(IncidentRequestSchema.INC_ReferenceID, new ZGuid(requestDoc.ReferenceId)));
			var queue = factory.LoadTop1<EdiERequestDocumentQueue>(new ZQuery(EdiERequestDocumentQueueSchema.EDQ_INC_ReferenceID, new ZGuid(requestDoc.ReferenceId)));
			if (shouldAttache)
			{
				AssertNotNull(request);
				AssertNull(queue);
				AssertEquals(1, request.DocManagerInfo.AllEDocs.Count);
				AssertEquals(requestDoc.Attachments[0].FileName, request.DocManagerInfo.AllEDocs[0].FileName);
				AssertEquals(false, request.DocManagerInfo.AllEDocs[0].IsPublished);
				AssertEquals(requestDoc.Attachments[0].Data, request.DocManagerInfo.AllEDocs[0].GetImageDataReader().ConvertToByteArrayAndCloseStream());
				AssertNull(factory.LoadTop1<EdiERequestDocumentQueue>(new ZQuery(EdiERequestDocumentQueueSchema.EDQ_INC_ReferenceID, new ZGuid(requestDoc.ReferenceId))));
			}
			else
			{
				AssertNull(request);
				AssertNotNull(queue);
				AssertEquals(requestDoc.Attachments[0].FileName, queue.EDQ_FileName);
				AssertEquals(requestDoc.Attachments[0].Data, queue.EDQ_Data);
			}
		}
	}
}
