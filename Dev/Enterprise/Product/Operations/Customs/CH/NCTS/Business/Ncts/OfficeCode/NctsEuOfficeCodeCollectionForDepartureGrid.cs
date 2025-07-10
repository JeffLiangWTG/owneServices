namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsEuOfficeCodeCollectionForDepartureGrid : EU.NCTS.Business.NctsEuOfficeCodeCollectionForDepartureGrid
{
	public NctsEuOfficeCodeCollectionForDepartureGrid(NctsHeader master) : base(master)
	{
	}

	public NctsEuOfficeCodeCollectionForDepartureGrid(NctsDepartureMovementHeader master) : base(master)
	{
	}

	public new NctsEuOfficeCode this[int index] => (NctsEuOfficeCode)base[index];

	public new NctsEuOfficeCode AddNew() => (NctsEuOfficeCode)base.AddNew();
}
