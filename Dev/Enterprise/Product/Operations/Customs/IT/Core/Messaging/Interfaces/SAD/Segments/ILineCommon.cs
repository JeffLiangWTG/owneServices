using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface ILineCommon
{
	ZInt ItemNumber { get; }
	ZString CombinedNomenclature { get; }
	ZString GoodsDescription { get; }
	IEnumerable<ZString> Containers { get; }
	IEnumerable<ZString> AdditionalCodes { get; }
	ZString CountryOfOrigin { get; }
	ZDecimal GrossMass { get; }
	ZString Procedure { get; }
	IEnumerable<ZString> NationalProcedures { get; }
	ZDecimal? NetMass { get; }
	IPreviousDocument PreviousAdministrativeDocument { get; }
	ZDecimal? SupplementaryUnit { get; }
	IEnumerable<ICertificate> Certificates { get; }
	ZString Notes { get; }
	IEnumerable<IDutyTaxFee> Duties { get; }
	ZDecimal? TotalItemTaxedAmount { get; }
	ZDecimal? GrandTotalTaxedAmount { get; }
	ZDecimal? StatisticalValueAmount { get; }
}
