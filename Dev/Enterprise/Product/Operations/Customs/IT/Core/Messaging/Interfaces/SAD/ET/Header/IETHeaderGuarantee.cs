using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface IETHeaderGuarantee
{
	ZString Type { get; }
	ZString Grn { get; }
	ZString OtherReference { get; }
	ZString AccessCode { get; }
	ZDecimal? Amount { get; }
	ZString NotValidForEC { get; }
	ZString NotValidForOtherContractingParties { get; }
}
