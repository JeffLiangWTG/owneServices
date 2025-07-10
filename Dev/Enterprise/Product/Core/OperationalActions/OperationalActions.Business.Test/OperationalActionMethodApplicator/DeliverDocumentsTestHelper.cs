using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	public class DeliverDocumentsTestHelper
	{
		public DeliverDocumentsTestHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		BusinessObjectFactory Factory { get; }

		public int CountPrintJobs(params ZGuid[] parentPK)
		{
			ZQuery filter = new ZQuery(StmPrintJobSchema.SP_ParentGuid, parentPK);
			return Factory.GetDatabaseCount(typeof(StmPrintJob), filter);
		}

		public StmPrintJob[] PrintJobs(params ZGuid[] parentPK)
		{
			ZQuery filter = new ZQuery(StmPrintJobSchema.SP_ParentGuid, parentPK);
			return Factory.Load<StmPrintJob>(filter);
		}

		public StmDeliveryGroup[] DeliveryGroups(params ZGuid[] groupPK)
		{
			var filter = new ZQuery(StmDeliveryGroupSchema.PK, groupPK);
			return Factory.Load<StmDeliveryGroup>(filter);
		}

		public OrgHeader NewConsigneeWithContact(ZString deliverBy)
		{
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_FullName = "consignor" + deliverBy;
			OrgContact contact = consignee.Contacts.AddNew();
			contact.OC_ContactName = deliverBy;
			contact.OC_Fax = "131314";
			contact.OC_Email = "unit.test@cw1.com";
			OrgDocument document = contact.Documents.AddNew();
			document.OD_DeliverBy = deliverBy;
			document.OD_DocumentGroup = ContactType.Consignee.Code;
			return consignee;
		}

		public OrgHeader NewConsigneeWithContactAndBroker(ZString deliverBy, ZString email)
		{
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_FullName = "consignor" + deliverBy;
			consignee.MiscServ.OM_IMSendImportDocsTo = "BRK";
			consignee.MiscServ.OM_IMSendSeaImportDocsTo = "BRK";
			OrgHeader broker = Factory.NewWithValidTestData<OrgHeader>();
			broker.OH_IsBroker = true;
			broker.MainAddress.OA_Email = email;
			OrgContact brokerContact = broker.Contacts.AddNew();
			brokerContact.OC_ContactName = deliverBy;
			brokerContact.OC_Fax = "131314";
			brokerContact.OC_Email = email;
			OrgDocument brokerDocument = brokerContact.Documents.AddNew();
			brokerDocument.OD_DeliverBy = deliverBy;
			brokerDocument.OD_DocumentGroup = ContactType.Consignee.Code;
			consignee.AddRelatedParty(broker.PK, "CAB", "PAD", "ALL", "", null);
			OrgContact contact = consignee.Contacts.AddNew();
			contact.OC_ContactName = deliverBy;
			contact.OC_Fax = "131314";
			contact.OC_Email = email;
			OrgDocument document = contact.Documents.AddNew();
			document.OD_DeliverBy = deliverBy;
			document.OD_DocumentGroup = ContactType.Consignee.Code;
			return consignee;
		}

		public BusinessObject NewShipmentWithConsignee(string name, OrgHeader consignee, string transportMode = Core.Constants.TransportModes.Sea)
		{
			BusinessObject shipment = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = name;
			shipment[JobShipmentSchema.Constants.JS_TransportMode] = transportMode;
			IDocAddresses addresses = (IDocAddresses)shipment;
			addresses.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsigneeDocumentaryAddress).E2_OA_Address = consignee.MainAddress.PK;
			return shipment;
		}

		public DocumentCommand CreateNewDocumentCommand(string name, string menuName, string contactType, bool setSubjectLineMacros = false, bool setMenuDataContext = false, bool setUserDefinedFieldList = false, bool setFields = false, RefDocType docType = null, bool setPrimaryCommand = false)
		{
			var templateContent = new Dictionary<string, string>();
			templateContent.Add("Template", @"{A}-[#Config]
[JerryTest]
{A}-[#EndOfReport]");
			if (setFields)
			{
				templateContent.Add("Fields", @"{A}-[TextFieldName]
{B}-[Type] {C}-[Text]
{B}-[Tab] {C}-[Test]
{A}-[#End]");
			}

			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", "", templateContent);
			var template = Factory.New<StmTemplateBase>();
			template.SO_Name = name;
			template.SO_Template = excelTemplate.GetAsByteArray();
			template.SO_DataContext = "GenericFreightJob";
			var command = Factory.New<DocumentCommand>();
			command.SU_MenuName = menuName;
			command.SU_BusinessContext = "Shipment";
			command.SU_ContactType = contactType;
			if (docType != null)
			{
				var eDocMenu = Factory.New<StmMenuEDocs>();
				eDocMenu.SX_SU = command.PK;
				eDocMenu.SX_RT_DocType = docType.PK;
			}

			var pivot = command.Documents.AddNew();
			pivot.SI_SU = command.PK;
			pivot.SI_SO = template.PK;
			var subjectLinePrefix = "Jerry Test";
			if (setPrimaryCommand)
			{
				subjectLinePrefix += " Primary";
				command.SU_PrimaryDocPackItemId = pivot.PK;
			}

			if (setSubjectLineMacros)
			{
				command.SU_EmailSubjectLine = subjectLinePrefix + " - <JobNumber>";
			}

			if (setMenuDataContext)
			{
				command.SU_MenuDataContext = "Shipment";
			}

			if (setUserDefinedFieldList)
			{
				command.SU_EmailSubjectLine = subjectLinePrefix + " - <TextFieldName>";
				command.SU_MenuDataContext = "Shipment";
			}

			return command;
		}
	}
}
