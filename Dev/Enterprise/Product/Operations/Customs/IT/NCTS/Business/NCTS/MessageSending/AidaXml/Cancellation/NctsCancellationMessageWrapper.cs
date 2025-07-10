using System;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class NctsCancellationMessageWrapper : ICancellation
{
	public NctsCancellationMessageWrapper(NctsHeader nctsHeader, NctsHeaderMessageSendingObject sendingObject)
	{
		Argument.NotNull(nctsHeader, nameof(nctsHeader));
		var movementHeader = Argument.NotNull(nctsHeader.MovementHeader, nameof(nctsHeader.MovementHeader));
		Argument.NotNull(sendingObject, nameof(sendingObject));

		InitializeLazy(nctsHeader, movementHeader, sendingObject);
	}

	string ICancellation.Mrn => lazyMrn.Value;
	Lazy<string> lazyMrn;

	string ICancellation.CustomsOffice => lazyCustomsOffice.Value;
	Lazy<string> lazyCustomsOffice;

	string ICancellation.Reason => lazyReason.Value;
	Lazy<string> lazyReason;

	string ICancellation.LegislativeReference => lazyLegislativeReference.Value;
	Lazy<string> lazyLegislativeReference;

	void InitializeLazy(NctsHeader nctsHeader, NctsDepartureMovementHeader movementHeader, NctsHeaderMessageSendingObject sendingObject)
	{
		lazyMrn = new Lazy<string>(() => nctsHeader.MovementReferenceNumber);
		lazyCustomsOffice = new Lazy<string>(() => XmlWrapperHelper.RemoveIsoCode(movementHeader.DepartureCustomsOfficeCode));
		lazyReason = new Lazy<string>(() => sendingObject.Reason);
		lazyLegislativeReference = new Lazy<string>(() => sendingObject.LegislativeReference);
	}
}
