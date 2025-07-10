using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface ISadMessageFixedPart
{
	ZString RecordType { get; }
	ZString MessageCode { get; }
	ZString DeclarantTaxNumber { get; }
	ZString EmptyField { get; }
	ZString AnnualProgressiveNumber { get; }
	ZInt ProgressiveNumber { get; }
}
