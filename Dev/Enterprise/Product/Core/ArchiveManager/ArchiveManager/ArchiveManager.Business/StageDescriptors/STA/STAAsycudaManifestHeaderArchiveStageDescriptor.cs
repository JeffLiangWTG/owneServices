using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Business.StageDescriptors.STA;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business
{
	class STAAsycudaManifestHeaderArchiveStageDescriptor : STAArchiveDescriptor
	{
		public override string Name
			=> Res.GetString("5D2F8722-B111-4A0C-BA88-8F333B67E8C7", "Standalone Asycuda Manifest Header Archive");

		public override SchemaColumn MainArchivePKColumn
			=> AsycudaManifestHeaderSchema.PK;

		public override SchemaColumn MainArchiveNKColumn
			=> AsycudaManifestHeaderSchema.AMA_JobReference;

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> AsycudaManifestHeaderSchema.AMA_SystemCreateTimeUtc;

		public override SchemaColumn ParentIDColumn
			=> AsycudaManifestHeaderSchema.AMA_ParentId;

		public override IEnumerable<IArchiveAction> GetArchiveAction(IArchiveLogger logger, IArchiveSet set)
		{
			yield return null;
		}

		public override void SetupArchiveRelationships(IArchiveSystemSetup systemSetup, IArchiveConfiguration config)
			=> CustomsArchiveRelationships.SetupAsycudaRelationships(systemSetup);
	}
}
