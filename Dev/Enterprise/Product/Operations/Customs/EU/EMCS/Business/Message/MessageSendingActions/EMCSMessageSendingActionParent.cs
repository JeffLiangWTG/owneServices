using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public abstract class EMCSMessageSendingActionParent<TSendingAction> : BaseMessageSendingObjectParent<TSendingAction> where TSendingAction : EMCSMessageSendingAction
	{
		protected EMCSMessageSendingActionParent(EMCSJobDeclaration declaration) : base(declaration.Factory)
		{
			JobDeclaration = Argument.NotNull(declaration, nameof(declaration));
		}

		public EMCSJobDeclaration JobDeclaration { get; }

		public sealed override BusinessObject TopLevelBusinessObject => JobDeclaration;

		public sealed override Security.SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.CustomsDeclarationSendWithMessageErrors;

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => columns;

		protected override NonPersistentBusinessObjectCollection<TSendingAction> GetSendingObjectsCollectionCore()
		{
			var result = (EMCSMessageSendingActionCollection<TSendingAction>)Activator.CreateInstance(SendingObjectCollectionType, this);
			result.PopulateElements();
			return result;
		}

		protected abstract Type SendingObjectCollectionType { get; }

		readonly IEnumerable<MessageSendingObjectProperty> columns = new MessageSendingObjectProperty[]
		{
			new MessageSendingObjectProperty(BaseMessageSendingObject.SchemaShouldSend, ismandatory: true, columnWidth: 80),
			new MessageSendingObjectProperty(nameof(EMCSMessageSendingAction.DeclarantType), ismandatory: true, columnWidth: 100),
			new MessageSendingObjectProperty(nameof(EMCSMessageSendingAction.EADNumber), ismandatory: true, columnWidth: 80),
			new MessageSendingObjectProperty(nameof(EMCSMessageSendingAction.RegistrationStatus), ismandatory: true, columnWidth: 100),
		};
	}
}
