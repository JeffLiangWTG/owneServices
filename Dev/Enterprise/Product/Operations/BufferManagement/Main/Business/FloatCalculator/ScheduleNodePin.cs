using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public class ScheduleNodePin
	{
		public ScheduleNodePin(ScheduleGraph scheduleGraph, ILinkEntity ownerEntity, ILinkEntity descendantEntity, decimal offset, int depth)
		{
			graph = scheduleGraph;
			OwnerEntityPk = ownerEntity.PK;
			DescendantEntityPk = descendantEntity.PK;
			Offset = offset;
			Depth = depth;
		}

		readonly ScheduleGraph graph;

		public decimal Offset { get; private set; }

		public int Depth { get; private set; }

		public ZGuid OwnerEntityPk { get; private set; }

		public ZGuid DescendantEntityPk { get; private set; }

		public ScheduleNode Owner
		{
			get
			{
				ScheduleNode node;
				graph.SchedulesByEntity.TryGetValue(OwnerEntityPk, out node);
				return node;
			}
		}

		public ScheduleNode Descendant
		{
			get
			{
				ScheduleNode node;
				graph.SchedulesByEntity.TryGetValue(DescendantEntityPk, out node);
				return node;
			}
		}
	}
}
