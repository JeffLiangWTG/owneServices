namespace Enterprise.DataTransfer.Native.Business.Xml.Serializers
{
	public interface IXmlGenerator<TSource, TElement>
	{
		TElement Generate(TSource source);
	}
}