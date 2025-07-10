using System.Threading;

namespace Enterprise.ServiceManager.Host;

public interface IBackgroundThreadActionQueueFactory
{
	IBackgroundThreadActionQueue BackgroundThreadActionQueue { get; }
}
