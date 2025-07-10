namespace CargoWise.EntityFramework
{
	public interface IWrappingProcessor : IProcessor
	{
		IProcessor WrappedProcessor { get; }
	}
}
