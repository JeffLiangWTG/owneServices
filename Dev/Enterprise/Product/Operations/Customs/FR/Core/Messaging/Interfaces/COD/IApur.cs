using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.Interfaces.COD
{
	public interface IApur
	{
		ZBool IndicateurApurement { get; } //Articles/Article/Apur/apurementREC/indicateurApurement
		ZDecimal Mnt { get; } //Articles/Article/Apur/apurementREC/mnt
		ZString Refdecapur { get; } //Articles/Article/Apur/apurementREC/refdecapur

		ZDate ChangementDateLimiteApurement { get; } //Articles/Article/Apur/changementDateLimiteApurement
	}
}
