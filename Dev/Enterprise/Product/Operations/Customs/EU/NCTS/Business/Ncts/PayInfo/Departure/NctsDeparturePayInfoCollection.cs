namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsDeparturePayInfoCollection : Customs.Business.CusInBondPayInfoCollection<NctsDeparturePayInfo>
	{
		public NctsDeparturePayInfoCollection(NctsDepartureMovementHeader master) : base(master)
		{
		}
	}
}
