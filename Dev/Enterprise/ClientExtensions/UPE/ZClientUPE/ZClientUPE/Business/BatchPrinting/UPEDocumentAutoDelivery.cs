using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Business
{
	public abstract class UPEDocumentAutoDelivery
	{
		protected UPEDocumentAutoDelivery(IUPEDocumentSupportable documentSupportable)
		{
			this.DocumentSupportable = documentSupportable;
		}

		public virtual void Deliver()
		{
			if (DocumentCommand != null)
			{
				if (!((BusinessObject)DocumentSupportable).IsInDatabase)
				{
					throw new ArgumentException("IUPEDocumentSupportable record must be saved in the database in order to deliver");
				}

				using (DocumentPack pack = new DocumentPack(DocumentCommand, DocumentSupportable, null, null))
				{
					DeliveryInstructions instructions = new DeliveryInstructions(pack);
					instructions.Destination = DeliveryInstructionDestination.TakenFromContact;

					AutoDeliverOrQueueForPrintDocumentPack(instructions, pack);
				}
			}
		}

		protected readonly IUPEDocumentSupportable DocumentSupportable;

		protected BusinessObjectFactory Factory
		{
			get { return DocumentSupportable.Factory; }
		}

		protected abstract DocumentCommand DocumentCommand { get; }
		protected abstract ZString PrintBatchType { get; }
		protected const string DisableQueueForBatchPrint = "";

		protected abstract string DeliveryFailureEmailSubject { get; }
		protected abstract string DeliveryFailureDocumentDetails { get; }

		#region SendDocumentDeliveryFailureEmail

		protected virtual void SendDocumentDeliveryFailureEmail(DeliveryInstructions instructions)
		{
			NotificationBuffer emailSendingFailureNotification = new NotificationBuffer();
			DocumentAutoDeliveryNotificationBuffer notify = new DocumentAutoDeliveryNotificationBuffer("Delivery instructions incomplete for " + DocumentCommand.SU_MenuName, emailSendingFailureNotification);

			notify.Notify(new InfoNotification(DeliveryFailureDocumentDetails.Trim()));
			notify.Notify(new NewlineNotification());
			BONotification.AddErrorsFromBusinessObjectValidationIncludingChildren(notify, instructions);

			notify.SendEmail(DeliveryFailureEmailSubject, UPEDataRegistry.Instance.DocumentAutoDeliveryNotificationGroup.Value);
		}

		#endregion

		#region AutoDeliverOrQueueForPrintDocumentPack

		void AutoDeliverOrQueueForPrintDocumentPack(DeliveryInstructions instructions, DocumentPack pack)
		{
			bool queueForBatchPrint = RemoveDocumentsThatWillBePrinted(instructions);
			bool hasConfiguredDeliveryRecipients2 = HasConfiguredDeliveryRecipients(instructions);
			if (queueForBatchPrint || !hasConfiguredDeliveryRecipients2)
			{
				if (IsDocumentCommandTaxAndCommercialInvoice)
				{
					QueueForBatchPrintAndSaveInOtherFactory(new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice());
				}
				else
				{
					QueueForBatchPrintAndSaveInOtherFactory(DocumentCommand);
				}
			}

			if (hasConfiguredDeliveryRecipients2)
			{
				instructions.RunPreSaveValidation();
				if (instructions.HasErrors)
				{
					SendDocumentDeliveryFailureEmail(instructions);
				}
				else
				{
					DeliverDocumentPack(instructions, pack);
				}
			}
		}

		bool HasConfiguredDeliveryRecipients(DeliveryInstructions instructions)
		{
			foreach (DocDeliveryContact recipient in instructions.Recipients)
			{
				if (recipient.OrgHeader != null || recipient.Contact != null)
				{
					return true;
				}
			}
			return false;
		}

		bool RemoveDocumentsThatWillBePrinted(DeliveryInstructions instructions)
		{
			bool queueForBatchPrint = instructions.IsDefaultDeliveryRecipient;
			foreach (DocDeliveryContact recipient in new ArrayList(instructions.Recipients))
			{
				if (recipient.DeliveryMethod == nameof(PrintCopyType.PRN))
				{
					queueForBatchPrint = true;
					instructions.Recipients.Remove(recipient);
				}
			}
			return queueForBatchPrint;
		}

		ZBool IsDocumentCommandTaxAndCommercialInvoice
		{
			get
			{
				return (DocumentCommand.SU_MenuName == UPEDocumentMenuItemLoader.UPSTaxInvoiceAndCommercialInvoiceMenuName ||
					DocumentCommand.SU_MenuName == UPEDocumentMenuItemLoader.UPSTaxInvoiceAndCommercialInvoiceQueueForBatchPrintMenuName);
			}
		}

		void QueueForBatchPrintAndSaveInOtherFactory(DocumentCommand documentCommand)
		{
			if (!PrintBatchType.IsEmpty && documentCommand != null)
			{
				BusinessObjectFactory factoryForSave = new BusinessObjectFactory();
				IUPEDocumentSupportable documentSupportableInOtherFactory = (IUPEDocumentSupportable)factoryForSave.Load(DocumentSupportable.GetType(), DocumentSupportable.Identifier);

				UPEPrintBatch currentPrintBatch = new UPEPrintBatch.Loader(factoryForSave).CreateOrLoadLatestBatch(PrintBatchType);
				currentPrintBatch.QueueForBatchPrintAndSave(documentSupportableInOtherFactory, documentCommand.PK);
			}
		}

		void DeliverDocumentPack(DeliveryInstructions instructions, DocumentPack pack)
		{
			PrintTask task = new PrintTask();
			task.Add(pack);
			task.Run(instructions);
		}

		#endregion
	}
}
