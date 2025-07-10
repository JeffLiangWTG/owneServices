using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.NCTS.Business;

public class TransitHeaderGuarantee : IETHeaderGuarantee
{
	public TransitHeaderGuarantee(NctsGuarantee guarantee)
	{
		this.guarantee = Argument.NotNull(guarantee, nameof(guarantee));
	}
	readonly NctsGuarantee guarantee;

	public ZString Type => guarantee.PW_BondType;

	public ZString Grn => guarantee.PW_BondNumber;

	public ZString OtherReference => guarantee.PW_BondNumber2;

	public ZString AccessCode => guarantee.PW_Password;

	public ZDecimal? Amount => guarantee.PW_BondNumber.IsEmpty ? guarantee.PW_BondAmount.GetValueOrNullIfZero() : null;

	public ZString NotValidForEC => ZString.Empty;

	public ZString NotValidForOtherContractingParties => ZString.Empty;
}
