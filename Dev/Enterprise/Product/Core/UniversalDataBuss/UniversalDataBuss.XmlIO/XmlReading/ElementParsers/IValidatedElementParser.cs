namespace Enterprise.UniversalDataBuss.XmlIO.XmlReading.ElementReaders
{
	public interface IValidatedElementParser : IElementParser
	{
		bool AllMandatoryElementsProvided { get; }
	}
}