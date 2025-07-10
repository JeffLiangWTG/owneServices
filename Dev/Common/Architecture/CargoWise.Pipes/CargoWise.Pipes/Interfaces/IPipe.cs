using System.Collections.Generic;

namespace CargoWise.Pipes
{
	public interface IPipe<TOutput> : IPipe, IPipeDataSource<TOutput>
	{
		new TOutput Do(object[] inputs);
	}

	public interface IPipe : IPipeDataSource
	{
		PipeType Type { get; }
		IEnumerable<IPipeDataSource> Inputs { get; }

		object Do(object[] inputs);
	}
}
