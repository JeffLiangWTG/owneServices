using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Security;

namespace Enterprise.Customs.DE.Business
{
	public sealed class FinalSumAWithAPreliminaryMessageSendingActionParent : BaseMessageSendingObjectParent<FinalSumAWithAPreliminaryMessageSendingAction>
	{
		public FinalSumAWithAPreliminaryMessageSendingActionParent(BusinessObject topBusinessObject, IEnumerable<CusTempStorageLine> messagingEntities, SecurityCheckpoint securityCheckpointToSendWithMessageError)
			: base(topBusinessObject.Factory)
		{
			this.topBusinessObject = topBusinessObject;
			this.messagingEntities = messagingEntities.Where(l => l.TSL_CustomsStatus != CustomsStatusList.Codes.TST);
			this.securityCheckpointToSendWithMessageError = securityCheckpointToSendWithMessageError;
		}

		readonly BusinessObject topBusinessObject;
		readonly IEnumerable<CusTempStorageLine> messagingEntities;
		readonly SecurityCheckpoint securityCheckpointToSendWithMessageError;

		public override BusinessObject TopLevelBusinessObject => topBusinessObject;

		public new FinalSumAWithAPreliminaryMessageSendingActionCollection SendingObjectsCollection => (FinalSumAWithAPreliminaryMessageSendingActionCollection)base.SendingObjectsCollection;

		public override SecurityCheckpoint SecurityCheckpointToSendWithMessageError => securityCheckpointToSendWithMessageError;

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties
		{
			get
			{
				return new MessageSendingObjectProperty[]
				{
					new MessageSendingObjectProperty(nameof(FinalSumAWithAPreliminaryMessageSendingAction.LineNo), true, 63, Res.GetData("3991A68F-EB39-434D-81FD-7625B6A3F437", "Line No.")),
					new MessageSendingObjectProperty(nameof(FinalSumAWithAPreliminaryMessageSendingAction.Description), true, 150, Res.GetData("8E215767-62FA-4058-8791-4B606DC0EA36", "Description Of Goods")),
					new MessageSendingObjectProperty(nameof(FinalSumAWithAPreliminaryMessageSendingAction.OwnerReferenceType), true, 135, Res.GetData("91B4F989-AC85-4F1C-832B-F79C8D91ACFF", "Owner Reference Type")),
					new MessageSendingObjectProperty(nameof(FinalSumAWithAPreliminaryMessageSendingAction.OwnerReferenceNumber), true, 270, Res.GetData("EE4BE27E-AD9E-4376-B2D1-8227E99F234A", "Owner Reference Number")),
					new MessageSendingObjectProperty(nameof(FinalSumAWithAPreliminaryMessageSendingAction.PackageCount), true, 97, Res.GetData("D84B9653-90CA-4F15-B450-2E964968BAED", "Package Count")),
					new MessageSendingObjectProperty(nameof(FinalSumAWithAPreliminaryMessageSendingAction.PackageType), true, 89, Res.GetData("464B3CA3-2B8D-459F-81C5-9C5857134C84", "Package Type")),
					new MessageSendingObjectProperty(nameof(FinalSumAWithAPreliminaryMessageSendingAction.CustodianEORI), true, 97, Res.GetData("3C9AC8B3-6245-4F3D-9F21-EE372881E812", "Custodian EORI")),
					new MessageSendingObjectProperty(nameof(FinalSumAWithAPreliminaryMessageSendingAction.CustodianBranch), true, 106, Res.GetData("C4A7EC70-2614-40DB-A0DA-E021DBF444AF", "Custodian Branch"))
				};
			}
		}

		protected override NonPersistentBusinessObjectCollection<FinalSumAWithAPreliminaryMessageSendingAction> GetSendingObjectsCollectionCore()
		{
			var collection = new FinalSumAWithAPreliminaryMessageSendingActionCollection(messagingEntities, Factory);
			collection.PopulateElements();
			return collection;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ClearRowNotifications();
			if (!SendingObjectsCollection.Cast<FinalSumAWithAPreliminaryMessageSendingAction>().Any(x => x.ShouldSend))
			{
				AddRowError(Res.GetString("CA21485D-D17B-44DD-AF15-74B147185538", "Please select at least one line to send."));
			}
		}

		protected override IEnumerable<INotification> GetNewMessageErrorCollector() => new EnhancedSelectiveMessageErrorCollector(topBusinessObject
			, new BusinessObject[] { ((CusTempStorageJobHeader)topBusinessObject).CUSPRLCusTempStorageDec }
			, SelectedSendingObjects.Cast<FinalSumAWithAPreliminaryMessageSendingAction>().Select(x => x.MessagingObject).ToArray()).GetMessageErrors();
	}
}
