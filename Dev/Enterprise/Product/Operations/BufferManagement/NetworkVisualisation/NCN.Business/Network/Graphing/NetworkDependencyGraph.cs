using System.Collections.Generic;
using System.Linq;
using CargoWise.PAVE.Common.Implementation;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class NetworkDependencyGraph : DependencyGraphBase<BMNCNShape, BMNCNAttachment>
	{
		public NetworkDependencyGraph(IJobNetwork network)
			: base(network.DiagramShape)
		{
			this.network = network;
		}

		readonly IJobNetwork network;

		protected override IEnumerable<BMNCNShape> GetVertexes()
		{
			return network.Shapes;
		}

		protected override IEnumerable<BMNCNAttachment> GetLinks()
		{
			return network.Entities.ShapeEntities.SelectMany(e => e.PostRequisiteLinks.Select(l => l.Attachment));
		}
	}
}
