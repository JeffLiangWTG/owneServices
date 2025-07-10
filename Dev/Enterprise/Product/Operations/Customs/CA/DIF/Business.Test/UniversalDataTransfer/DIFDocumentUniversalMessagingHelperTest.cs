using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.CA.DIF.Business.Testing;
using Enterprise.Customs.Common.CA.DIF;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.DIF.Business.UniversalDataTransfer.Testing
{
	sealed class DIFDocumentUniversalMessagingHelperTest : TestCaseWithFactory
	{
		[TestDate(2017, 10, 27)]
		public void TestSendEventViaEhub()
		{
			using (Factory.AddDisposableService())
			{
				var resourceRetriever = new EmbeddedResourceRetriever();
				var source = SetupDIFDocumentForEhubTest();
				new DIFDocumentUniversalMessagingHelper().SendEventViaEHub(source, DIFConstants.UniversalEventFunctionCode.Original);
				var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, source.RequiredDocumentAddInfo.PK);
				var queuedMessage = Factory.LoadTop1<EDIMessage>(query);
				AssertNotNull(queuedMessage);
				AssertEquals("UDM", queuedMessage.EM_ApplicationCode);
				AssertEquals("XUE", queuedMessage.EM_MessageType);
				AssertEquals("XUE", queuedMessage.EM_MessageSubType);
				AssertEquals("TRX", queuedMessage.EM_ReceiveTransmit);
				AssertEquals("SNT", queuedMessage.EM_Status);
				AssertEquals("HQU", queuedMessage.Interchange.EI_Status);
				AssertEquals("CACustomsDoc", queuedMessage.Interchange.EI_To);
				var expectedXml = string.Format(resourceRetriever.GetString("Enterprise.Customs.CA.DIF.Business.Testing.UniversalDataTransfer.TestFiles.UniversalEventTest1.xml").Replace("DOCUMENT_ID", source.EDocsDocumentPK.ToString()), queuedMessage.Interchange.EI_SessionGUID, queuedMessage.Interchange.EI_InterchangeNum, queuedMessage.EM_MessageNum);
				AssertMultilineASCIIEquals("queuedMessage.EM_MessageText", expectedXml, queuedMessage.EM_MessageText);
			}
		}

		DIFDocument SetupDIFDocumentForEhubTest()
		{
			var requiredDocument = ((IDISHost)JobDeclaration).RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var addInfo = requiredDocument.AddInfos.AddNew();
			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;
			addInfo.EX_Status = StatusList.Codes.AwaitingOriginal;
			addInfo.EX_ReferenceNumber = "REF001";

			var wrapper = new DIFHostWrapper((ICADIFHost)JobDeclaration);
			var difDocument = wrapper.DISDocuments.AddNew();
			difDocument.RequiredDocumentAddInfo = addInfo;
			difDocument.DocumentType = "1111";
			difDocument.EffectiveDate = new ZDateTime(2017, 10, 25);
			difDocument.ExpiryDate = new ZDateTime(2018, 10, 25);
			var eDoc1 = ((IDocManagerSupport)JobDeclaration).DocManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 1, 2, 3 }), "TestFile1", "INV");
			((IDocManagerSupport)JobDeclaration).DocManagerInfo.MasterFactory.Save();
			difDocument.EDocsDocumentPK = eDoc1.UniqueKey;
			return difDocument;
		}

		BusinessObject jobDeclaration;
		BusinessObject JobDeclaration => jobDeclaration ?? (jobDeclaration = new TestHelper(Factory).GetJobDeclaration());
	}
}
