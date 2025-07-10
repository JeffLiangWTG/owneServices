using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public interface IMovementGuarantee
{
	ZInt GuarantorTypeCode { get; }

	ZString ConsignorCodeGuarantee { get; }

	ZString ConsignorTypeGuarantee { get; }

	ZDecimal ConsignorDepositAmountCommitted { get; }

	ZString ConsigneeCodeGuarantee { get; }

	ZString ConsigneeTypeGuarantee { get; }

	ZDecimal ConsigneeDepositAmountCommitted { get; }

	ZString TransporterCodeGuarantee { get; }

	ZString TransporterTypeGuarantee { get; }

	ZDecimal TransporterDepositAmountCommitted { get; }

	ZString OwnerCodeGuarantee { get; }

	ZString OwnerTypeGuarantee { get; }

	ZDecimal OwnerDepositAmountCommitted { get; }

	IEnumerable<IGuarantorTrader> Guarantors { get; }
}
