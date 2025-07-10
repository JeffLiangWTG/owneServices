namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsESOfficeCodeCollectionForDepartureGrid : EU.NCTS.Business.NctsEuOfficeCodeCollectionForDepartureGrid
	{
		public NctsESOfficeCodeCollectionForDepartureGrid(NctsHeader master) : base(master)
		{
		}

		public NctsESOfficeCodeCollectionForDepartureGrid(NctsDepartureMovementHeader master) : base(master)
		{
		}

		public new NctsESOfficeCode this[int index] => (NctsESOfficeCode)Elements[index];

		public new NctsESOfficeCode AddNew() => (NctsESOfficeCode)base.AddNew();
	}
}
