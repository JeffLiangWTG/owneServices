using System;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public abstract class BaseNctsMessageDataProvider<T> : IPassarMessage
	where T : INctsMessageSendingObject
{
	public BaseNctsMessageDataProvider(T sendingObject)
	{
		this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		nctsHeader = this.sendingObject.NctsHeader;
	}
	protected readonly T sendingObject;
	protected readonly NctsHeader nctsHeader;

	public string MessageSender => EnvironmentHelper.GetBusinessPartnerId();

	public DateTime PreparationDateAndTime => (preparationDateAndTime ?? (preparationDateAndTime = ZDateTime.UtcNow.ToDateTime())).Value;
	DateTime? preparationDateAndTime;

	public string CorrelationIdentifier => correlationIdentifier ??= GetCorrelationIdentifier();
	string correlationIdentifier;

	protected virtual string GetCorrelationIdentifier() => null;

	public string MessageIdentification => sendingObject.MessageIdentification;

	public IOppositeInformation OppositeInformation => oppositeInformation ??= GetOppositeInformationData(nctsHeader);
	IOppositeInformation oppositeInformation;

	protected virtual IOppositeInformation GetOppositeInformationData(NctsHeader nctsHeader, ZString? referenceNumberInput = null) => OppositeInformationDataProvider.New(nctsHeader);
}
