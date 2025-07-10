using CargoWise.PAVE.Common.Interfaces;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class NetworklessScheduleNode : ScheduleNode
	{
		public NetworklessScheduleNode(ILinkEntity entity)
			: base(null, entity)
		{
		}
	}
}
