using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.UniversalDataBuss.ServiceTasks;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Integration.Customs;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	class DocumentTypeEDICodeMappingTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestDocumentTypeEDICodeMapping()
		{
			var declaration = Factory.BOFactory.NewWithValidTestData(ObjectFactory.GetType<IBaseJobDeclaration>());
			Factory.SaveForTesting();
			AssertEquals("Precondition: ", 0, ((IDocManagerSupport)declaration).DocManagerInfo.AllEDocs.Count);

			var message = EDIMessageTestFactory.New(Factory.BOFactory);

			message.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.EM_Status = XmlEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText =
			$@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>CustomsDeclaration</Type>
          <Key>{declaration[JobDeclarationSchema.JE_DeclarationReference]}</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <EventTime>2020-09-08T14:03:08Z</EventTime>
    <EventType>DDI</EventType>
    <AttachedDocumentCollection>
      <AttachedDocument>
        <FileName>CIV-Commercial_Invoice-50010863.pdf</FileName>
        <Type>
          <Code>AAA</Code>
          <Description>Uploaded from Hubtran.</Description>
        </Type>
        <ImageData>AA==</ImageData>
        <IsPublished>true</IsPublished>
      </AttachedDocument>
    </AttachedDocumentCollection>
  </Event>
</UniversalEvent>";

			var org_Proxy = Factory.BOFactory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
			var docType = Factory.BOFactory.NewWithValidTestData<RefDocType>();

			var matchOverride = Factory.BOFactory.New<OrgPatternMatchOverride>();
			matchOverride.OO_OH = org_Proxy.PK;
			matchOverride.OO_ForeignCode = "AAA";
			matchOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.DocumentType;
			matchOverride.OO_LocalGuid = docType.PK;

			Factory.SaveForTesting();

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var localCode = docType.RT_DocType;
			AssertStartsWith("Code mapping should work", string.Format(@"Used code mapping defined in Organization(Code: {0}) > Config > EDI Code Mapping.
Line 17: Mapped Document Type code 'AAA' to '{1}'.
Successfully Added eDoc: CIV-Commercial_Invoice-50010863.pdf.
Adding eDoc with a document type of {1} and name of CIV-Commercial_Invoice-50010863.pdf.
Linked Event to Declaration {2}.
", org_Proxy.OH_Code, localCode, declaration[JobDeclarationSchema.JE_DeclarationReference]), message.GetLogNoteText());

			var newFactory = new BusinessObjectFactory();
			var refreshedDeclaration = newFactory.Load<IBaseJobDeclaration>(declaration.PK);
			AssertEquals(1, ((IDocManagerSupport)refreshedDeclaration).DocManagerInfo.AllEDocs.Count);
			AssertEquals(localCode, ((IDocManagerSupport)refreshedDeclaration).DocManagerInfo.AllEDocs[0].DocType);
		}

		public void TestDocumentTypeEDICodeMapping_HasPrefixInformation()
		{
			var key = "S00001234";
			var factory = new BusinessObjectFactory();
			var shipment = factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = key;
			factory.Save();

			var message = EDIMessageTestFactory.New(Factory.BOFactory);

			message.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.EM_Status = XmlEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText =
			$@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>DocManager</Type>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
    </DataContext>
    <EventTime>2020-09-08T14:03:08Z</EventTime>
    <EventType>DDI</EventType>
    <AttachedDocumentCollection>
      <AttachedDocument>
        <FileName>[SHP AAA {key}]CIV-Commercial_Invoice-50010863.pdf</FileName>
        <Type>
          <Code>AAA</Code>
          <Description>Uploaded from Hubtran.</Description>
        </Type>
        <ImageData>AA==</ImageData>
        <IsPublished>true</IsPublished>
      </AttachedDocument>
    </AttachedDocumentCollection>
  </Event>
</UniversalEvent>";

			var org_Proxy = Factory.BOFactory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
			var docType = Factory.BOFactory.NewWithValidTestData<RefDocType>();

			var matchOverride = Factory.BOFactory.New<OrgPatternMatchOverride>();
			matchOverride.OO_OH = org_Proxy.PK;
			matchOverride.OO_ForeignCode = "AAA";
			matchOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.DocumentType;
			matchOverride.OO_LocalGuid = docType.PK;

			Factory.SaveForTesting();

			var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var localCode = docType.RT_DocType;
			AssertStartsWith("Code mapping should work", string.Format(@"Used code mapping defined in Organization(Code: {0}) > Config > EDI Code Mapping.
Line 24: Mapped Document Type code 'AAA' to '{1}'.
Allocation details were detected in the filename and will be used in place of the Document Type mapping.
Successfully Added eDoc: [SHP AAA {2}]CIV-Commercial_Invoice-50010863.pdf.
Adding eDoc with a document type of AAA and name of [SHP AAA {2}]CIV-Commercial_Invoice-50010863.pdf.
Linked Event to record.
", org_Proxy.OH_Code, localCode, key), message.GetLogNoteText());

			var newFactory = new BusinessObjectFactory();
			var refreshedShipment = newFactory.Load<Forwarding.IForwardingShipment>(shipment.PK);
			AssertEquals(1, ((IDocManagerSupport)refreshedShipment).DocManagerInfo.AllEDocs.Count);
			AssertEquals("AAA", ((IDocManagerSupport)refreshedShipment).DocManagerInfo.AllEDocs[0].DocType);
		}
	}
}
