using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

sealed class EntryInstructionPreviousDocumentWrapper : PreviousDocumentWrapper
{
	public EntryInstructionPreviousDocumentWrapper(PreviousDocument previousDocument) : base(previousDocument)
	{
	}

	protected override decimal? GetQuantity() => PreviousDocument.EffectiveNetMass.InKilogramsSafe.NullIfZero();
}
