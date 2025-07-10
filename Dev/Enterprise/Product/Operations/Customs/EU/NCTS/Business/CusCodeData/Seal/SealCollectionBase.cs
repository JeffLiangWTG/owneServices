using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business;

public abstract class SealCollection : CusCodeDataCollection<Seal>
{
	protected SealCollection(NctsHeader master)
		: base(master, CusCodeDataTypeList.Codes.Seal)
	{
	}

	protected SealCollection(NctsArrivalMovementHeader master)
		: base(master, CusCodeDataTypeList.Codes.Seal)
	{
	}

	public IEnumerable<ZString> AllSeals => this.Cast<Seal>().Where(seal => !seal.CY_Data.IsEmpty).Select(seal => seal.CY_Data);
}
