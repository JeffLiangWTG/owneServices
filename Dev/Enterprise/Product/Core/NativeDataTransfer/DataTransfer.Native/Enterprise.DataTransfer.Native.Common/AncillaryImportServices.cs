using System;
using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.DataTransfer.Native.Common.Logging;
using Enterprise.Integration;

namespace Enterprise.DataTransfer.Native.Common
{
	public class AncillaryImportServices
	{
		public AncillaryImportServices(ILogger logger = null)
		{
			Logger = logger ?? new MemoryLogger();
			EntitiesReferencingPK = new Dictionary<Guid, EntityCollection>();
			OrganisationLocalToForeignCodeMappings = new Dictionary<string, string>();
		}

		public bool RequiresAdditionOfActionEqualsMerge { get; set; }
		public ILogger Logger { get; }
		public Dictionary<Guid, EntityCollection> EntitiesReferencingPK { get; private set; }
		public Dictionary<string, string> OrganisationLocalToForeignCodeMappings { get; }

		public IDisposable ImportingEntitySet()
		{
			return new DisposableAction(() => EntitiesReferencingPK = new Dictionary<Guid, EntityCollection>());
		}
	}
}
