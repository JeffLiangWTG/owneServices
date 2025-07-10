using System;
using System.Collections.Generic;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	public class EntityCollection : Tuple<IEnumerable<INetworkEntity>>, INetworkEntityCollection
	{
		public EntityCollection(IEnumerable<INetworkEntity> entities)
			: base(entities)
		{
		}

		public IEnumerable<INetworkEntity> Entities
		{
			get { return Item1; }
		}

		public bool IsHandledByVisualiser
		{
			get { return true; }
		}
	}
}
