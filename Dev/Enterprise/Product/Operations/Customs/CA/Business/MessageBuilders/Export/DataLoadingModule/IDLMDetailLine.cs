using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	public interface IDLMDetailLine
	{
		ZString CountryOfOrigin { get; }
		ZString ProvinceOfOrigin { get; }
		ZString HarmonizedSystemCode { get; }
		ZString ProductDescription { get; }
		ZString ConveyanceIdentificationNumber { get; }
		ZDecimal Quantity { get; }
		ZString UnitOfMeasure { get; }
		ZDecimal ValueFOBPointOfExit { get; }
	}
}
