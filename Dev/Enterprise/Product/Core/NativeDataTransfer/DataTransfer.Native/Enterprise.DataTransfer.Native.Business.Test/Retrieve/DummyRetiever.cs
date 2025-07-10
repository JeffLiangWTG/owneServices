using System.Collections.Generic;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;

namespace Enterprise.DataTransfer.Native.Business.Retrieve
{
	public class DummyRetiever : Retriever
	{
		public DummyRetiever()
			: base(new AncillaryImportServices())
		{
		}

		protected override IEnumerable<IEntity> FindEntities(EntitySetDefinition definition, IEnumerable<EntityCriteria> criterias)
		{
			return System.Array.Empty<Entity>();
		}
	}
}
