using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ImportDeclarationMessageSendingActionParent : MessageSendingActionParent
	{
		public ImportDeclarationMessageSendingActionParent(JobDeclaration declaration)
			: base(declaration, declaration.ActiveEntryHeaders,
				  x => ((CusEntryHeader)x).MovementReferenceNumber, Env.Security.CustomsDeclarationSendWithMessageErrors)
		{
		}

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => columnDefinitions;

		public new ImportEntryMessageSendingActionCollection SendingObjectsCollection => (ImportEntryMessageSendingActionCollection)base.SendingObjectsCollection;

		protected override NonPersistentBusinessObjectCollection<MessageSendingAction> GetSendingObjectsCollectionCore()
		{
			var result = new ImportEntryMessageSendingActionCollection(Declaration, this);
			result.PopulateElements();
			return result;
		}

		protected override bool OnlyOneObjectAllowedToBeSent => false;

		protected override IEnumerable<INotification> GetNewMessageErrorCollector()
		{
			return new JobDeclarationMessageSendingNotificationCollector(Declaration, SendingObjectsCollection.Cast<ImportEntryMessageSendingAction>().Where(x => x.ShouldSend).Select(x => x.MessagingObject)).GetMessageErrors();
		}

		JobDeclaration Declaration => (JobDeclaration)TopLevelBusinessObject;

		readonly IEnumerable<MessageSendingObjectProperty> columnDefinitions = new MessageSendingObjectProperty[]
		{
			new MessageSendingObjectProperty(nameof(ImportEntryMessageSendingAction.DeclarationType), true, 105, Res.GetData("13043F16-71B5-4F9E-8046-EFFA60A44F12", "Declaration Type")),
			new MessageSendingObjectProperty(nameof(ImportEntryMessageSendingAction.SubStyle), true, 80, Res.GetData("BE23FA3D-A774-4845-9D75-89595C489330", "Sub Style")),
			new MessageSendingObjectProperty(nameof(ImportEntryMessageSendingAction.Description), true, 200, Res.GetData("3B36AFD1-9446-4128-A743-2621B7AD29AA", "Description")),
			new MessageSendingObjectProperty(nameof(ImportEntryMessageSendingAction.EntryStatus), true, 80, Res.GetData("60ADC7AA-A28E-4A13-99C9-3CD9B87959C1", "Entry Status")),
			new MessageSendingObjectProperty(nameof(ImportEntryMessageSendingAction.CusCon), true, 61, Res.GetData("E92A844B-9852-4F36-8619-FC4A24E0BC56", "CUSCON?")),
			new MessageSendingObjectProperty(nameof(ImportEntryMessageSendingAction.RegistrationNumber), true, 123, Res.GetData("897B899E-30EE-4F9D-97C2-FCC87440105D", "Registration Number")),
		};
	}
}
