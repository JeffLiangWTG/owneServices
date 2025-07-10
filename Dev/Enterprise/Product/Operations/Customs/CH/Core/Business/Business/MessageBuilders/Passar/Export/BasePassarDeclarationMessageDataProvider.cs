using System;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

public abstract class BasePassarDeclarationMessageDataProvider : IPassarMessage
{
	protected BasePassarDeclarationMessageDataProvider(DeclarationMessageSendingObject sendingObject)
	{
		this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		entryHeader = sendingObject.Header as CusEntryHeader;
		declaration = entryHeader.Declaration;
	}
	protected readonly DeclarationMessageSendingObject sendingObject;
	protected readonly CusEntryHeader entryHeader;
	protected readonly JobDeclaration declaration;

	public string MessageSender => EnvironmentHelper.GetBusinessPartnerId();

	public DateTime PreparationDateAndTime => (preparationDateAndTime ?? (preparationDateAndTime = ZDateTime.UtcNow.ToDateTime())).Value;
	DateTime? preparationDateAndTime;

	public string MessageIdentification => sendingObject.GetApplicationReference();

	public IOppositeInformation OppositeInformation => oppositeInformation ??= OppositeInformationDataProvider.New(declaration);
	IOppositeInformation oppositeInformation;

	public string CorrelationIdentifier => null;
}
