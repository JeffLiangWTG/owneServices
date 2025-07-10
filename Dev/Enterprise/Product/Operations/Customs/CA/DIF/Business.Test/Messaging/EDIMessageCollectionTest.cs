using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.CA.DIF.Business.Testing
{
	[TestedType(typeof(EDIMessageCollection))]
	sealed class EDIMessageCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestRelationshipFilter()
		{
			var jobDeclaration = Declaration;

			var requiredDocument = jobDeclaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var requiredDocumentAddInfo = requiredDocument.AddInfos.AddNew();

			var message = Factory.New<EDIMessage>();
			message.EM_LinkedObject = requiredDocumentAddInfo;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;

			var requiredDocument2 = jobDeclaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var requiredDocumentAddInfo2 = requiredDocument2.AddInfos.AddNew();
			var message2 = Factory.New<EDIMessage>();
			message2.EM_LinkedObject = requiredDocumentAddInfo2;
			message2.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;

			var hostWrapper = new DIFHostWrapper(jobDeclaration);
			var disDocument = new DIFDocument(hostWrapper);
			disDocument.RequiredDocumentPK = requiredDocument.PK;
			disDocument.RequiredDocumentAddInfo = requiredDocumentAddInfo;

			var coll = new EDIMessageCollection(disDocument);
			coll.Load();
			AssertEquals(1, coll.Count);
			Assert(coll.Contains(message));

			coll = new EDIMessageCollection(disDocument);
			coll.Load();
			disDocument.RequiredDocumentAddInfo = null;
			AssertEquals(0, disDocument.Messages.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var hostWrapper = new DIFHostWrapper(Declaration);
			return new EDIMessageCollection(new DIFDocument(hostWrapper));
		}

		MasterFiles.Business.DIS.ICADIFHost declaration;
		MasterFiles.Business.DIS.ICADIFHost Declaration => declaration ?? (declaration = (MasterFiles.Business.DIS.ICADIFHost)new TestHelper(Factory).GetJobDeclaration());
	}
}
