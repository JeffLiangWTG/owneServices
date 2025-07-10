namespace Enterprise.DataTransfer.Native.Common.Converters
{
	public interface IEntityConverter<TResult> : IConverter<IEntity, TResult>
	{
		TResult Convert(IEntity entity, IEntityContext context);
	}
}