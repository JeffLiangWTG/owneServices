using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class AutoDocumentDeliveryJob_ConcreteTest : AutoDocumentDeliveryJobTest
	{
		//TODO: WI00738741 - Confirm if serialization is required for AutoDocumentDeliveryJob
		public void TestSerializingActuallySerializesStuff()
		{
			StmPrintQueue queue = Factory.New<StmPrintQueue>();
			BusinessObject bizo = (BusinessObject)BusinessObjectToDeliver;
			DocumentCommand command = DocumentCommand;
			Factory.Save();

			var job = new AutoDocumentDeliveryJob((IDocumentSupportable)bizo, command.PK, queue.PK, true);
			var serializedJob = JsonConverterHelper.Serialize(job);
			var deserializedJob = JsonConverterHelper.Deserialize<AutoDocumentDeliveryJob>(serializedJob);

			AssertEquals("Should remember the BusinessObject", true, BusinessObjectEqualityComparer<BusinessObject>.IgnoreFactoryComparer.Equals(job.BusinessObject, deserializedJob.BusinessObject));
			AssertEquals("Should remember the DocumentCommand", true, BusinessObjectEqualityComparer<BusinessObject>.IgnoreFactoryComparer.Equals(job.DocumentCommand, deserializedJob.DocumentCommand));
			AssertEquals("Should remember the Queue", job.PrinterQueuePK, deserializedJob.PrinterQueuePK);
			AssertEquals("Should remember if it should OnlySendToDocManager", job.OnlySendToDocManager, deserializedJob.OnlySendToDocManager);
		}

		[GuiTest, TestDate(2013, 04, 02, 10, 53, 0, 0)]
		public void TestDeliverCartageAdviceUpdatesCartageAdvised()
		{
			documentQuery = new DocumentZQuery(BusinessContext.Customs, "Cartage Advice");
			documentQuery.AddToFilter(StmMenuItemSchema.SU_DocumentDirection, "ARV");
			BusinessObject shipment = (BusinessObject)BusinessObjectToDeliver;
			shipment[JobShipmentSchema.Constants.JS_UniqueConsignRef] = "S00000100";
			shipment[JobShipmentSchema.Constants.JS_TransportMode] = Core.Constants.TransportModes.Sea;

			BusinessObject jobDeclaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.AU.IJobDeclaration>();
			jobDeclaration[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;

			OrgDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;
			Recipient.OC_Email = "clinton@edi.com.au";

			var docsAndCartage = shipment.GetType().GetProperty("DocsAndCartage",
				(System.Type)shipment.GetType().GetProperty("DocsAndCartageType",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
				.GetValue(shipment, null)).GetValue(shipment, null);
			docsAndCartage.GetType().GetProperty("JP_OA_PickupCartageCoAddr", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).SetValue(
				docsAndCartage, RecipientOrganisation.Addresses[0].PK, null);
			docsAndCartage.GetType().GetProperty("JP_OA_DeliveryCartageCoAddr", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).SetValue(
				docsAndCartage, RecipientOrganisation.Addresses[0].PK, null);
			DocumentCommand.SU_FilterList = ""; //so it's considered applicable

			Factory.Save();
			DocumentDelivery.Deliver(Notifications); //Shipment.DocsAndCartage.DeliveryCartageCo PickupCartageCo

			//delicious copypasta!

			ZDateTime jP_PickupCartageAdvised = (ZDateTime)(docsAndCartage.GetType().GetProperty("JP_PickupCartageAdvised",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).GetValue(docsAndCartage, null));
			ZDateTime jP_DeliveryCartageAdvised = (ZDateTime)(docsAndCartage.GetType().GetProperty("JP_DeliveryCartageAdvised",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).GetValue(docsAndCartage, null));
			Assert("DocumentEventSource_DocumentPrinted was hit", new ZDateTime(2013, 04, 02, 10, 53, 0, 0) == jP_PickupCartageAdvised
				|| new ZDateTime(2013, 04, 02, 10, 53, 0, 0) == jP_DeliveryCartageAdvised);
		}

		public void TestDeliverDocumentPackWithAdditionalDocument()
		{
			DocumentZQuery documentQuery1 = new DocumentZQuery(BusinessContext.Shipment, "Pre-Alert");
			DocumentZQuery documentQuery2 = new DocumentZQuery(BusinessContext.Shipment, "Delay Alert");

			var parentCommand = Factory.LoadTop1<DocumentCommand>(documentQuery1);
			parentCommand.Parent = BusinessObjectToDeliver;
			parentCommand.SU_FilterList = "";
			var childCommand = Factory.LoadTop1<DocumentCommand>(documentQuery2);
			childCommand.Parent = BusinessObjectToDeliver;
			childCommand.SU_PreventAutoDelivery = false;
			childCommand.SU_FilterList = "";

			var pivot = parentCommand.ChildMenus.AddNew();
			pivot.SF_SU_Inward = parentCommand.PK;
			pivot.SF_SU_Outward = childCommand.PK;

			var recipient1 = RecipientOrganisation.Contacts.AddNew();
			recipient1.OC_ContactName = "K";
			recipient1.OC_Email = "kelvin@edi.com.au";
			var orgDocument1 = recipient1.Documents.AddNew();
			orgDocument1.OD_SU_MenuItem = parentCommand.PK;
			orgDocument1.OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;

			var recipient2 = RecipientOrganisation.Contacts.AddNew();
			recipient2.OC_ContactName = "E";
			recipient2.OC_Email = "enguerran@edi.com.au";
			var orgDocument2 = recipient2.Documents.AddNew();
			orgDocument2.OD_SU_MenuItem = childCommand.PK;
			orgDocument2.OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;

			Factory.Save();

			var docDelivery = new AutoDocumentDeliveryJob(BusinessObjectToDeliver, false, parentCommand.PK);
			PrintTask.DocunmentRunCounter = 0;
			docDelivery.Deliver(Notifications);
			AssertEquals("Only 2 documents should have run.", 2, PrintTask.DocunmentRunCounter);
		}

		public void TestDeliver_WithNonApplicableDocumentCommand()
		{
			OrgDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;
			Recipient.OC_Email = "clinton@edi.com.au";
			DocumentCommand.SU_FilterList = "MSGBKRCTY=IMPNZ";
			Factory.Save();

			AutoDocumentDeliveryJob job = new AutoDocumentDeliveryJob(BusinessObjectToDeliver, false, DocumentCommand.PK);
			job.Deliver(new NotificationBuffer());

			ZQuery query = new ZQuery();
			query.AddToFilter(StmPrintJobSchema.SP_RunDateTime, SQLComparisonOperator.GreaterThan, ZDateTime.Now.AddSeconds(-10));
			AssertNull("Non-applicable document not delivered", Factory.LoadTop1<StmPrintJob>(query));
		}

		#region Implementation
		protected override AutoDocumentDeliveryJob NewDocumentDeliveryJob(bool onlySendToDocManager)
		{
			return new AutoDocumentDeliveryJob(BusinessObjectToDeliver, DocumentCommand.PK, onlySendToDocManager);
		}

		protected override DocumentCommand DocumentCommand
		{
			get
			{
				DocumentCommand command = Factory.LoadTop1<DocumentCommand>(documentQuery);
				command.Parent = BusinessObjectToDeliver;
				return command;
			}
		}

		protected override IDocumentSupportable NewBusinessObjectToDeliver()
		{
			BusinessObject result = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			result["ConsigneePK"] = RecipientOrganisation.PK;
			result[JobShipmentSchema.JS_IsForwardRegistered] = false;   // so it can be deleted in unit tests
			return (IDocumentSupportable)result;
		}

		#endregion
	}
}
