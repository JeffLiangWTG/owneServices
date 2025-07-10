using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface IOutgoingInterchangeHeaderTextFieldsProvider
{
	ZString Staff { get; }
	ZString Node { get; }
	ZString MessageType { get; }
	ZString AccountNumber { get; }
	ZString CustomsInterchangeHeader { get; }
}
