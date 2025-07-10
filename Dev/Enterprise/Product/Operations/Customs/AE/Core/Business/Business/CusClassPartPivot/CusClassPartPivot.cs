using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AE.Business;

public class CusClassPartPivot : Customs.Business.BaseCusClassPartPivot, Integration.Customs.AE.ICusClassPartPivot
{
	public CusClassPartPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override ZString DefaultDataGroupingForTariffsCore => AEConstants.DefaultDataGroupingForTariffs;
}
