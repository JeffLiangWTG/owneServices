using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.Business
{
	public class UPEElectronicCreditNoteAutoDelivery : UPEDocumentAutoDelivery
	{
		public UPEElectronicCreditNoteAutoDelivery(UPEJobDeclaration documentSupportable) : base(documentSupportable) { }

		public UPEJobDeclaration Declaration
		{
			get { return (UPEJobDeclaration)DocumentSupportable; }
		}

		public override void Deliver()
		{
			PopulateDocummentPacks();
			DeliveryInstructions instructions = GetInstructions();
			instructions.Destination = DeliveryInstructionDestination.TakenFromContact;
			if (HasConfiguredDeliveryRecipients)
			{
				if (instructions.HasErrors)
				{
					SendDocumentDeliveryFailureEmail(instructions);
				}
				else
				{
					using (PrintTask printTask = new PrintTask())
					{
						foreach (DocumentPack pack in Packs)
						{
							printTask.Add(pack);
						}
						printTask.Run(instructions);
					}
					packs = null;
				}
			}

			if (packs != null)
			{
				foreach (DocumentPack pack in packs)
				{
					pack.Dispose();
				}
				packs = null;
			}
		}

		protected virtual DeliveryInstructions GetInstructions()
		{
			return new DeliveryInstructions();
		}

		protected virtual void PopulateDocummentPacks()
		{
			Packs.Add(new UPEDocumentPack(new UPEDocumentMenuItemLoader(Factory).LoadElectronicCreditNote(), Declaration, null, null));
			if (Declaration.Callout != null)
			{
				Packs.Add(new UPEDocumentPack(new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoiceAndCommercialInvoice(), Declaration.Callout, null, null));
			}
		}

		bool HasConfiguredDeliveryRecipients
		{
			get { return new EmailGroupUtility().GetGroupEmailCollection(UPEDataRegistry.Instance.CreditNotificationGroup, false).Count > 0; }
		}

		protected virtual List<DocumentPack> Packs
		{
			get { return packs ?? (packs = new List<DocumentPack>()); }
		}
		List<DocumentPack> packs;

		#region Overriden

		protected override DocumentCommand DocumentCommand
		{
			get { return new UPEDocumentMenuItemLoader(Factory).LoadElectronicCreditNote(); }
		}

		protected override ZString PrintBatchType
		{
			get { return DisableQueueForBatchPrint; }
		}

		protected override string DeliveryFailureEmailSubject
		{
			get { return "Delivery Electronic Credit Note failed for Declaration " + Declaration.JE_DeclarationReference; }
		}

		protected override string DeliveryFailureDocumentDetails
		{
			get { return "Delivery Electronic Credit Note failed for Declaration " + Declaration.JE_DeclarationReference; }
		}

		#endregion
	}
}
