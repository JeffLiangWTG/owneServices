using System.Collections.Generic;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface ITransports : IReadOnlyCollection<ITransport>
	{
		ITransport PreCarriage { get; }
		ITransport Main { get; }
		ITransport OnForwarding { get; }
	}
}
