namespace Enterprise.DataTransfer.Native.Common
{
	public interface IConverter<TSource, TResult>
	{
		TResult Convert(TSource source);
	}
}