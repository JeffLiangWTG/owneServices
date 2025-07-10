using System;

namespace CargoWise.Pipes
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface IPipeDataSource<T> : IPipeDataSource
	{
	}

	public interface IPipeDataSource
	{
		Guid Key { get; }
		string DebugName { get; set; }
		Type Output { get; }
	}
}
