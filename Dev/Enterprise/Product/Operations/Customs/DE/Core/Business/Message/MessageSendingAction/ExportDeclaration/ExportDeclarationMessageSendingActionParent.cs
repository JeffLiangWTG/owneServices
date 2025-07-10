using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public sealed class ExportDeclarationMessageSendingActionParent : MessageSendingActionParent
	{
		public ExportDeclarationMessageSendingActionParent(JobDeclaration declaration) : base(declaration, declaration.ActiveEntryHeaders, x => ((CusEntryHeader)x).MovementReferenceNumber, Env.Security.CustomsDeclarationSendWithMessageErrors)
		{
		}

		public void CopyValuesBackToDeclaration()
		{
			var sendingObjectsList = SendingObjectsCollection.Cast<ExportEntryMessageSendingAction>();
			foreach (var action in sendingObjectsList)
			{
				action.CopyValuesBackToEntry();
			}

			var exitOffices = sendingObjectsList.Where(x => x.ShouldSend && !x.ExitCustomsOffice.IsEmpty).Select(x => x.ExitCustomsOffice).Distinct().ToArray();
			if (exitOffices.Length == 1)
			{
				var exitOffice = exitOffices[0];
				var customsOffices = Declaration.CustomsOffices;
				var exitOfficeCusCodeData = customsOffices.GetFirstElementHaving(EuOfficeCodesTypes.Codes.OfficeOfExit);
				if (exitOffice != (exitOfficeCusCodeData?.CY_Data ?? ZString.Empty))
				{
					if (exitOfficeCusCodeData == null)
					{
						exitOfficeCusCodeData = customsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit);
					}

					exitOfficeCusCodeData.CY_Data = exitOffice;
				}
			}
		}

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => columnDefinitions;

		public new ExportEntryMessageSendingActionCollection SendingObjectsCollection => (ExportEntryMessageSendingActionCollection)base.SendingObjectsCollection;

		protected override NonPersistentBusinessObjectCollection<MessageSendingAction> GetSendingObjectsCollectionCore()
		{
			var result = new ExportEntryMessageSendingActionCollection(Declaration);
			result.PopulateElements();
			return result;
		}

		protected override bool OnlyOneObjectAllowedToBeSent => false;

		protected override IEnumerable<INotification> GetNewMessageErrorCollector()
		{
			return new JobDeclarationMessageSendingNotificationCollector(Declaration, SendingObjectsCollection.Cast<ExportEntryMessageSendingAction>().Where(x => x.ShouldSend).Select(x => x.MessagingObject)).GetMessageErrors();
		}

		readonly IEnumerable<MessageSendingObjectProperty> columnDefinitions = new MessageSendingObjectProperty[]
		{
			new MessageSendingObjectProperty(nameof(ExportEntryMessageSendingAction.MovementReferenceNumber), true, 80, Res.GetData("00916A76-69A1-4B52-9433-FD70B254A03D", "MRN")),
			new MessageSendingObjectProperty(nameof(ExportEntryMessageSendingAction.LocalReferenceNumber), true, 80, Res.GetData("823DE38C-E4CD-4B6E-A100-91821A1EF5FC", "LRN")),
			new MessageSendingObjectProperty(nameof(ExportEntryMessageSendingAction.Variant), true, 80, Res.GetData("34DEE725-AD5B-41FC-B84D-B15F69EF0EF9", "Type (Time)")),
			new MessageSendingObjectProperty(nameof(ExportEntryMessageSendingAction.ProcedureType), true, 110, Res.GetData("3A57D81B-CD50-4557-A5E0-39DB14009BCC", "Type (Procedure)")),
			new MessageSendingObjectProperty(nameof(ExportEntryMessageSendingAction.Description), true, 190, Res.GetData("1E9B9A5D-3345-437A-B5FD-5236835EDCCE", "Description")),
			new MessageSendingObjectProperty(nameof(ExportEntryMessageSendingAction.EntryStatus), true, 80, Res.GetData("F28BFA30-9DDE-432A-911C-4146456FD26C", "Entry Status")),
			new MessageSendingObjectProperty(nameof(ExportEntryMessageSendingAction.EntryType), true, 80, Res.GetData("FF49CCE5-CA6F-404A-8496-6288EE75C6DD", "Entry Type"))
		};

		JobDeclaration Declaration => (JobDeclaration)TopLevelBusinessObject;
	}
}
