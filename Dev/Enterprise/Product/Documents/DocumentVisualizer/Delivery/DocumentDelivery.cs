using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentVisualizer.Delivery
{
	public sealed class DocumentDelivery
	{
		public DocumentDelivery(IDocumentSupportable documentSupportable, IReadOnlyCollection<IDocumentDelivery> documentDeliveries, IEventBroker broker = null, IStmMenuItem menuItem = null)
		{
			this.documentSupportable = Argument.NotNull(documentSupportable, nameof(documentSupportable));
			this.documentDeliveries = Argument.NotNull(documentDeliveries, nameof(documentDeliveries));
			this.broker = broker;
			this.menuItem = menuItem;
		}

		readonly IEventBroker broker;
		readonly IDocumentSupportable documentSupportable;
		readonly IReadOnlyCollection<IDocumentDelivery> documentDeliveries;
		readonly IStmMenuItem menuItem;

		public void Deliver(bool useDraftWatermark = false)
		{
			var documentSupporter = documentSupportable.DocumentSupporter;

			if (documentSupporter == null
				|| documentDeliveries.Count == 0)
			{
				return;
			}

			using (var pack = new DocumentPack(menuItem as StmMenuItem))
			{
				pack.DocumentSupporter = documentSupporter;
				var instructions = new DocumentDeliveryInstructions(documentSupporter.Factory, pack, documentDeliveries);
				instructions.IsDraft = useDraftWatermark;

				using (var printTask = new DocumentPrintTask(broker: broker))
				{
					if (menuItem != null)
					{
						printTask.DeliveryInstructionsDefaultPK = menuItem.PK; // without this printer choice is not saved
					}
					printTask.RunWithPartialInstructions(AllowedDeliveryOptions.All, instructions, Env.Security.None);
				}
			}

			ObjectFactory.Get<ILicenceConsumptionLogCreator>().CreateLog(Env.Licence.FormBuilder);
		}

		public void Deliver(DocDeliveryContact recipient, string emailSubject, bool useDraftWatermark = false)
		{
			var documentSupporter = documentSupportable.DocumentSupporter;

			if (documentSupporter == null
				|| documentDeliveries.Count == 0)
			{
				return;
			}

			using (var pack = new DocumentPack(menuItem as StmMenuItem))
			{
				pack.DocumentSupporter = documentSupporter;
				var instructions = new DocumentDeliveryInstructions(documentSupporter.Factory, pack, documentDeliveries);
				instructions.Recipients.RemoveAll();
				instructions.Recipients.Add(recipient);
				instructions.Destination = DeliveryInstructionDestination.TakenFromContact;
				instructions.IsDraft = useDraftWatermark;

				using (var printTask = new DocumentPrintTask(emailSubject: emailSubject))
				{
					if (menuItem != null)
					{
						printTask.DeliveryInstructionsDefaultPK = menuItem.PK;
					}
					printTask.Run(instructions);
				}
			}
		}
	}
}
