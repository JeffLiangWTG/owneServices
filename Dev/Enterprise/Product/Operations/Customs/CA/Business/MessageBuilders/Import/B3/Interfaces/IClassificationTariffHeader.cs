namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	using CargoWise.Types;

	public interface IClassificationTariffLine
	{
		ZString ClassificationNumber { get; }
		ZDateTime ClassificationDate { get; }

		ZString TariffCode { get; }
		ZDateTime TariffDate { get; }
	}
}
