using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	class EntityCountChangedStrategy
	{
		internal virtual void HandleCountChanged(JobNetwork network, IEnumerable<BMNCNShape> entities)
		{
			SetOwnership(network);
		}

		static void SetOwnership(JobNetwork network)
		{
			foreach (var shape in network.Entities.GetShapesInDepthOrder().WithFetchHints(s => new FetchHint(ProcessHeaderSchema.PK, s.BNS_RelatedEntityID)))
			{
				network.SetupEntityForDiagram(network.Entities.GetInstance(shape), network.Entities.GetInstance(shape.ParentShape));
			}
		}
	}
}
