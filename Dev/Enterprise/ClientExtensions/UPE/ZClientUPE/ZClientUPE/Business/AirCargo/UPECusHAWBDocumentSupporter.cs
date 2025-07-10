using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Client.UPE.Business
{
	public class UPECusHAWBDocumentSupporter : CusHAWBDocumentSupporter
	{
		public UPECusHAWBDocumentSupporter(CusHAWBBase cusHAWB)
			: base(cusHAWB)
		{
		}

		public new UPECusHAWB CusHAWB
		{
			get { return (UPECusHAWB)base.CusHAWB; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result;
			if (dataContext == Core.Constants.DataContext.CusHAWB)
			{
				result = new DocumentWrapper[] { UPEDocCusHAWB.New(CusHAWB, Factory) };
			}
			else
			{
				result = base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
			}
			return result;
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			IDocumentDeliveryContact result;
			if (menuName == UPEDocumentMenuItemLoader.AlternateBrokerSplitNotificationMenuName)
			{
				OrgHeader contactOrganisation = (CusHAWB.Declaration == null) ? null : CusHAWB.Declaration.AlternateBroker;
				result = new OrgHeaderContact(contactOrganisation, null);
			}
			else if (contact == ContactType.Consignee && CusHAWB.Declaration != null && CusHAWB.Declaration.Importer != null)
			{
				result = new OrgHeaderContact(CusHAWB.Declaration.Importer, null);
			}
			else
			{
				result = base.GetContactOrganisation(menuName, contact, direction);
			}
			return result;
		}

		#region Shipment Held Letter

		protected override void InitialiseCore(IDocumentEvents documentEventSource)
		{
			base.InitialiseCore(documentEventSource);
			documentEventSource.DocumentPrintRequested += new DocumentCancelEventHandler(DocumentEventSource_DocumentPrintRequested);
			documentEventSource.DocumentPrinted += new DocumentPrintedEventHandler(OnDocumentEventSource_DocumentPrinted);
		}

		void DocumentEventSource_DocumentPrintRequested(object sender, DocumentCancelEventArgs e)
		{
			OnDocumentPrintRequested(e);
		}

		void OnDocumentEventSource_DocumentPrinted(object sender, DocumentPrintedEventArgs e)
		{
			OnDocumentPrinted(e);
		}

		protected virtual void OnDocumentPrintRequested(DocumentCancelEventArgs e)
		{
			if (e.MenuItem.SU_MenuName == UPEDocumentMenuItemLoader.ShipmentHeldLetterForConsigneeMenuName)
			{
				e.Cancel = QueryForHeldLetterDetailsAndQueueForBatchPrint(e.MenuItem.PK, ShipmentHeldLetterRecipient.Consignee);
			}
			else if (e.MenuItem.SU_MenuName == UPEDocumentMenuItemLoader.ShipmentHeldLetterForConsignorMenuName)
			{
				e.Cancel = QueryForHeldLetterDetailsAndQueueForBatchPrint(e.MenuItem.PK, ShipmentHeldLetterRecipient.Consignor);
			}
		}

		bool QueryForHeldLetterDetailsAndQueueForBatchPrint(ZGuid menuItemPK, ShipmentHeldLetterRecipient recipient)
		{
			bool userCancelled = CusHAWB.QueryForShipmentHeldLetterDetails(recipient);
			if (!userCancelled && !CusHAWB.ShipmentHeldLetterDetails.DeliverByEmailFax)
			{
				if (CusHAWB.ShipmentHeldLetterDetails.QueueForBatchPrint)
				{
					QueueForBatchPrintAndSave(menuItemPK, NotifyUser);
				}
			}

			bool shouldNotProceedWithDocEngineDelivery = userCancelled || !CusHAWB.ShipmentHeldLetterDetails.DeliverByEmailFax;
			return shouldNotProceedWithDocEngineDelivery;
		}

		protected virtual void OnDocumentPrinted(DocumentPrintedEventArgs e)
		{
			if (e.DeliveryInstructionDestinationType != DeliveryInstructionDestination.UserCancelled &&
				e.DeliveryInstructionDestinationType != DeliveryInstructionDestination.Preview &&
				UPEDocumentMenuItemLoader.IsShipmentHeldLetter(e.MenuItem))
			{
				QueueForBatchPrintAndSave(e.MenuItem.PK, DontNotifyUser);
			}
		}

		void QueueForBatchPrintAndSave(ZGuid menuItemPK, bool shouldNotifyUser)
		{
			if (menuItemPK == DeclarationConsigneeHeldLetterCommand.PK)
			{
				menuItemPK = DocumentLoader.LoadCusHAWBHeldLetter(ShipmentHeldLetterRecipient.Consignee).PK;
			}
			if (menuItemPK == DeclarationConsignorHeldLetterCommand.PK)
			{
				menuItemPK = DocumentLoader.LoadCusHAWBHeldLetter(ShipmentHeldLetterRecipient.Consignor).PK;
			}

			UPEPrintBatch currentPrintBatch = new UPEPrintBatch.Loader(Factory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.ShipmentHeldLetter);
			currentPrintBatch.QueueForBatchPrintAndSave(CusHAWB, menuItemPK, shouldNotifyUser);
		}

		DocumentCommand DeclarationConsigneeHeldLetterCommand
		{
			get { return new UPEDocumentMenuItemLoader(Factory).LoadJobDeclarationHeldLetter(ShipmentHeldLetterRecipient.Consignee); }
		}

		DocumentCommand DeclarationConsignorHeldLetterCommand
		{
			get { return new UPEDocumentMenuItemLoader(Factory).LoadJobDeclarationHeldLetter(ShipmentHeldLetterRecipient.Consignor); }
		}

		UPEDocumentMenuItemLoader DocumentLoader
		{
			get
			{
				if (fDocumentLoader == null)
				{
					fDocumentLoader = new UPEDocumentMenuItemLoader(Factory);
				}
				return fDocumentLoader;
			}
		}
		UPEDocumentMenuItemLoader fDocumentLoader;

		const bool NotifyUser = true;
		const bool DontNotifyUser = false;

		#endregion
	}
}
