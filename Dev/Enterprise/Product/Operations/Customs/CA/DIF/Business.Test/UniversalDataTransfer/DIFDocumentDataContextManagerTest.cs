using CargoWise.EntityFramework;
using Enterprise.Customs.CA.DIF.Business.Testing;
using Enterprise.Customs.Common.CA.DIF;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.DIF.Business.UniversalDataTransfer.Testing
{
	[TestedType(typeof(DIFDocumentDataContextManager))]
	sealed class DIFDocumentDataContextManagerTest : DataContextManagerTestCase<DIFDocumentDataContextManager, DIFDocument>
	{
		public void TestOnLogParentFoundFromEDIMessage()
		{
			var eventXmlText = @"
<UniversalEvent>
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Key>REF001</Key>
              <Type>CADIFDocument</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2013-12-01T20:17:41</EventTime>
        <EventType>DDV</EventType>
		<EventParameters>
			<ReferenceNumber>10207000013799</ReferenceNumber>
			<RequestNumber>REF001</RequestNumber>
			<ReceiptNumber>80db27d9-0402-4955-9837-5403c80555df</ReceiptNumber>
		</EventParameters>
	</Event>
</UniversalEvent>
";
			var requiredDocument = ((IDISHost)JobDeclaration).RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var addInfo = requiredDocument.AddInfos.AddNew();
			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;
			addInfo.EX_Status = StatusList.Codes.AwaitingOriginal;
			addInfo.EX_ReferenceNumber = "REF001";

			Factory.SaveForTesting();
			var xmlEvent = new XmlEventDeserializer().Parse(eventXmlText);
			var finder = new DIFDocumentEventParentFinder(Factory.BOFactory, new DIFDocumentDataContextManager(), new XmlSessionTracker(new ServiceTaskLogForTesting()));
			var logParents = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals(1, logParents.Length);
			var relatedObj = logParents[0] as JobRequiredDocumentAddInfo;
			AssertNotNull("Not Null", relatedObj);
			AssertEquals("URN", "REF001", relatedObj.EX_ReferenceNumber);

			var requiredDocumentBO = relatedObj.RequiredDocument;

			var disHostProvider = requiredDocumentBO != null ? requiredDocumentBO.Parent as IDISHostProvider : null;
			var disHost = disHostProvider != null ? disHostProvider.DISHost : null;
			AssertEquals("Declaration Number", "B00001000", disHost.JobNumber);
		}

		public void TestDataContextType()
		{
			var wrapper = new DIFHostWrapper((ICADIFHost)JobDeclaration);
			var difDocument = wrapper.DISDocuments.AddNew();
			var eventManager = difDocument.GetUniversalDataContextManager();
			AssertEquals(DataContextType.CADIFDocument, eventManager.DataContextType);
		}

		public void TestDataContextKey()
		{
			var requiredDocument = ((IDISHost)JobDeclaration).RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var addInfo = requiredDocument.AddInfos.AddNew();
			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;
			addInfo.EX_Status = StatusList.Codes.AwaitingOriginal;
			addInfo.EX_ReferenceNumber = "REF001";
			var wrapper = new DIFHostWrapper((ICADIFHost)JobDeclaration);
			var difDocument = wrapper.DISDocuments.AddNew();
			difDocument.RequiredDocumentAddInfo = addInfo;
			var eventManager = difDocument.GetUniversalDataContextManager();
			AssertEquals("REF001", eventManager.DataContextKey);
		}

		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("DIFDocument doesn't need any unique jobnumber", true);
		}

		protected override DIFDocument GetNewBusinessObjectForTesting()
		{
			var requiredDocument = ((IDISHost)JobDeclaration).RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var addInfo = requiredDocument.AddInfos.AddNew();

			var wrapper = new DIFHostWrapper((ICADIFHost)JobDeclaration);
			var difDocument = wrapper.DISDocuments.AddNew();
			difDocument.RequiredDocumentAddInfo = addInfo;
			return difDocument;
		}

		BusinessObject jobDeclaration;
		BusinessObject JobDeclaration => jobDeclaration ?? (jobDeclaration = new TestHelper(Factory.BOFactory).GetJobDeclaration());
	}
}
