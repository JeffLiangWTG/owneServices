namespace Enterprise.UniversalDataBuss.XmlIO.XmlReading.ElementReaders
{
	public interface IElementNavigator
	{
		IElementNavigator ParentElement { get; }
		string CurrentElementName { get; }
	}
}
