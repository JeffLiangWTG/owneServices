using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class SealType : SealTypeDataProviderAbstractClass
{
	public SealType(string sealId)
	{
		Identifier = sealId;
	}

	public override int SequenceNumber => default;

	public override string Identifier { get; }
}
