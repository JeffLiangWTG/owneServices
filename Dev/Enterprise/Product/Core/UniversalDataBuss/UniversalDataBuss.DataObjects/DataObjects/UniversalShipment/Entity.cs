using System.Collections.Generic;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public partial class Entity : IDataObject
	{
		public Entity()
		{
		}

		public Entity(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		public List<EntityKey> EntityKeyCollection { get; private set; }
		public List<Entity> RelatedEntityCollection { get; private set; }
	}
}