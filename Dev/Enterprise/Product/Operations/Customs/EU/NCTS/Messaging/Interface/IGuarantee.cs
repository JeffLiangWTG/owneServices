using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Messaging
{
	public interface IGuarantee
	{
		ZString GuaranteeType { get; }
		ZString GuaranteeReferenceNumber { get; }
		ZString OtherGuaranteeReference { get; }
		ZString AccessCode { get; }
		ZDecimal TaxAndDutyLiabiltyAmount { get; }
		ZString NotValidForEC { get; }
		IReadOnlyCollection<ZString> NotValidForOtherContractingParties { get; }
	}
}
