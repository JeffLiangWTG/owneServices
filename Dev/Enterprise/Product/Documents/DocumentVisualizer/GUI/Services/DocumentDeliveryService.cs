using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using DocumentDelivery = Enterprise.DocumentVisualizer.Delivery.DocumentDelivery;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class DocumentDeliveryService : IDocumentDeliveryService
	{
		public DocumentDeliveryService(IEventBroker broker = null, IStmMenuItem menuItem = null)
		{
			this.broker = broker;
			this.menuItem = menuItem;
		}

		readonly IEventBroker broker;
		readonly IStmMenuItem menuItem;

		void IDocumentDeliveryService.Deliver(IDocumentSupportable documentSupportable, IReadOnlyCollection<IDocumentDelivery> documentDeliveries, bool useDraftWatermark)
		{
			if (documentSupportable != null
				&& documentDeliveries != null)
			{
				var documentDelivery = new DocumentDelivery(documentSupportable, documentDeliveries, broker, menuItem);
				documentDelivery.Deliver(useDraftWatermark);
			}
		}

		void IDocumentDeliveryService.Deliver(IDocumentSupportable documentSupportable, string emailSubject, IReadOnlyCollection<IDocumentDelivery> documentDeliveries, DocDeliveryContact recipient, bool useDraftWatermark)
		{
			if (documentSupportable != null
				&& documentDeliveries != null)
			{
				var documentDelivery = new DocumentDelivery(documentSupportable, documentDeliveries, broker, menuItem);
				documentDelivery.Deliver(recipient, emailSubject, useDraftWatermark);
			}
		}

		public IEDICommunicationSettings GetCommunicationSettings(IBusiness parent, INotifications notifications)
		{
			var workflowProvider = parent as IWorkflowProvider;

			if (workflowProvider == null)
			{
				return new EmptyEDICommunicationSettings();
			}

			var workflowDescriptor = WorkflowDescriptors.Instance.TryGetValueSafe(workflowProvider.WorkflowType);

			if (workflowDescriptor == null)
			{
				return new EmptyEDICommunicationSettings();
			}

			CodeDescriptionPair recipient;
			CodeDescriptionPair purpose;

			using (var settingsForm = new MessageSettingsForm(parent.Factory, workflowProvider.WorkflowType))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(settingsForm) != DialogResult.OK)
				{
					return new EmptyEDICommunicationSettings();
				}

				recipient = new CodeDescriptionPair(
					settingsForm.RecipientType,
					settingsForm.RecipientTypeDescription);

				purpose = new CodeDescriptionPair(
					settingsForm.PurposeCode,
					settingsForm.PurposeCodeDescription);
			}

			var recipientParties = workflowDescriptor
				.GetMessageRecipientParty((BusinessObject)workflowProvider, recipient.Code)
				.Select(r => r.Party)
				.ToArray();

			if (!recipientParties.Any())
			{
				var message = Res.GetString("c7f1f08a-faec-4218-b041-6e74c6255800", "Could not find '{0}' ({1}) party on '{2}'.",
					recipient.Code,
					recipient.Description,
					parent.HumanReadableName);

				notifications.Add(NotificationType.Information, message);

				return new EmptyEDICommunicationSettings();
			}

			var failureReasons = new List<MultilingualString>();
			var communicationsModes = new List<IEDICommunicationsMode>();
			foreach (var org in recipientParties)
			{
				var modeQuery = new EDICommunicationModeQuery(
				parent: (BusinessObject)parent,
				descriptor: workflowDescriptor,
				fileFormat: WorkflowTriggerActionTypeConstants.Codes.SendFormBuilderXml,
				purpose: purpose.Code,
				recipientRole: recipient.Code,
				eventCode: ZString.Empty,
				eventReference: ZString.Empty);
				var modes = workflowDescriptor.GetCommunicationModesForRecipient(org, modeQuery);
				communicationsModes.AddRange(modes.communicationModes);
				failureReasons.Add(modes.failureReason);
			}

			if (!communicationsModes.Any())
			{
				var message = Res.GetString("a047ab5a-a4f4-426a-8c94-8b4cd44eaadd", "Could not find '{0}' communication mode with purpose code '{1}' on '{2}' for reasons:\r\n{3}",
					WorkflowTriggerActionTypeConstants.Codes.SendFormBuilderXml,
					purpose.Code,
					recipientParties.First().OH_FullName,
					MultilingualString.Join(System.Environment.NewLine, failureReasons.ToArray()));

				notifications.Add(NotificationType.Information, message);

				return new EmptyEDICommunicationSettings();
			}

			return new EDICommunicationSettings(
				recipient,
				purpose,
				communicationsModes);
		}
	}
}
