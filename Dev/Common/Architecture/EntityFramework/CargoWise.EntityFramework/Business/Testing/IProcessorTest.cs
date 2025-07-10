#if DEBUG
using System.Threading;
using CargoWise.ComponentModel;

namespace CargoWise.EntityFramework
{
	public interface IProcessorTest : IProcessor
	{
		new void Process(INotifications notifications, CancellationToken token);
	}
}
#endif
