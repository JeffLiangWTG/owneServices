namespace Enterprise.Customs.ASYCUDA.Business
{
	public class CusPersonCountryCollection<TCusPersonCountry> : Customs.Business.CusPersonCountryCollection<TCusPersonCountry>
		where TCusPersonCountry : CusPersonCountry
	{
		public CusPersonCountryCollection(CusPerson master)
			: base(master)
		{
		}
	}
}
