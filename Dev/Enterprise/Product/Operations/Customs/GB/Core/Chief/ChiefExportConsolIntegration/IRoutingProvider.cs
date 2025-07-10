using CargoWise.Types;

namespace Enterprise.Customs.GB.Chief.EdiFact.UKCINV
{
	public interface IRoutingProvider
	{
		ZString StyleOfEntry { get; }  //SOE
		ZString RouteOfEntry { get; }  //ROE		
	}
}
