using System;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

public sealed class CancellationMessageWrapper : ICancellation
{
	public CancellationMessageWrapper(CusEntryHeader entryHeader, JobDeclarationMessageSendingObject sendingObject)
	{
		Argument.NotNull(entryHeader, nameof(entryHeader));
		var declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		Argument.NotNull(sendingObject, nameof(sendingObject));

		InitializeLazy(entryHeader, declaration, sendingObject);
	}

	string ICancellation.Mrn => lazyMrn.Value;
	Lazy<string> lazyMrn;

	string ICancellation.CustomsOffice => lazyCustomsOffice.Value;
	Lazy<string> lazyCustomsOffice;

	string ICancellation.Reason => lazyReason.Value;
	Lazy<string> lazyReason;

	string ICancellation.LegislativeReference => lazyLegislativeReference.Value;
	Lazy<string> lazyLegislativeReference;

	void InitializeLazy(CusEntryHeader entryHeader, JobDeclaration declaration, JobDeclarationMessageSendingObject sendingObject)
	{
		lazyMrn = new Lazy<string>(() => entryHeader.MovementReferenceNumber);
		lazyCustomsOffice = new Lazy<string>(() => XmlWrapperHelper.RemoveIsoCode(declaration.JE_CustomsOffice));
		lazyReason = new Lazy<string>(() => sendingObject.VOCReason);
		lazyLegislativeReference = new Lazy<string>(() => sendingObject.CancellationAndAmendmentLegislativeReference);
	}
}
