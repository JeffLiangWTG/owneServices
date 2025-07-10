using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Messaging;
using Enterprise.Customs.IT.Messaging.MessageStructure;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public sealed class ITMessageChangedStatusDeterminerToDictateWhetherSavingAllowed : MessageChangedStatusDeterminerToDictateWhetherSavingAllowed
{
	public ITMessageChangedStatusDeterminerToDictateWhetherSavingAllowed(JobDeclaration dec) : base(dec)
	{
		declarationLoadedInMainFactory = dec;
	}

	public override EDIMessage[] MakeMessagesOnThisBizoForComparison(BusinessObject bizo)
	{
		if (bizo is JobDeclaration declaration)
		{
			return MakeMessagesOnDeclarationForComparison(declaration).ToArray();
		}

		return Array.Empty<EDIMessage>();
	}

	#region Implementation

	IEnumerable<EDIMessage> MakeMessagesOnDeclarationForComparison(JobDeclaration declaration)
	{
		var sendingObjectParent = GetMessageSendingObjectParent(declaration);
		var lockerForEditingSendingObject = new LockerForEditingSendingObject(sendingObjectParent, declarationLoadedInMainFactory);

		declaration.ResetApportionedPreviousDocuments();
		foreach (var customsMessageValuesProvider in lockerForEditingSendingObject.GetLockedOutgoingMessageGeneratorProviders())
		{
			var message = declaration.Factory.New<EDIMessage>();
			message.EM_MessageText = GetMessageText(customsMessageValuesProvider);
			yield return message;
		}
	}

	ZString GetMessageText(IOutgoingCustomsMessageGeneratorValuesProvider customsMessageValuesProvider)
	{
		switch (customsMessageValuesProvider)
		{
			case ISadOutgoingCustomsMessageGeneratorValuesProvider sadOutgoingCustomsMessageGeneratorValuesProvider:
				var messageObjects = sadOutgoingCustomsMessageGeneratorValuesProvider.GetCustomsMessageObjects() ?? Enumerable.Empty<SadCustomsMessage>();
				return messageObjects.SerializeWithTabSeparator();

			case IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider aidaXmlOutgoingCustomsMessageGeneratorValuesProvider:
				return aidaXmlOutgoingCustomsMessageGeneratorValuesProvider.CustomsMessageText;

			default:
				return ZString.Empty;
		}
	}

	JobDeclarationMessageSendingObjectParent GetMessageSendingObjectParent(JobDeclaration declaration)
	{
		return declaration.IsUCC6
			? new Ucc6JobDeclarationMessageSendingObjectParent(declaration)
			: new JobDeclarationMessageSendingObjectParent(declaration);
	}

	readonly JobDeclaration declarationLoadedInMainFactory;

	#endregion
}
