using System.Threading;
using CargoWise.ComponentModel;

namespace CargoWise.EntityFramework
{
	public interface IProcessor
	{
		void Process(INotifications notifications, CancellationToken token
#if DEBUG // For release you need to supply a value. For tests you get a free pass.
			= new CancellationToken()
#endif
			);
	}
}
