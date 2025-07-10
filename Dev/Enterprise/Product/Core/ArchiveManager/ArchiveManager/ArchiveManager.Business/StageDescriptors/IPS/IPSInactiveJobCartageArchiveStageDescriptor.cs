using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business
{
	class IPSInactiveJobCartageArchiveStageDescriptor : IPSArchiveDescriptor
	{
		public override string Name
			=> Res.GetString("D23113DE-055C-4F08-8794-D7AEBBF9E7C1", "Inactive Job Cartage Archive");

		public override SchemaColumn MainArchivePKColumn
			=> JobCartageSchema.PK;

		public override SchemaColumn MainArchiveNKColumn
			=> JobCartageSchema.JJ_ConsignmentID;

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> JobCartageSchema.JJ_SystemCreateTimeUtc;

		protected internal override SchemaBoolColumn IsCancelledSchemaColumn
			=> JobCartageSchema.JJ_IsCancelled;

		public override IEnumerable<IArchivePreparationAction> GetPreparationAction(IArchiveLogger logger, IArchiveSet archiveSet, IArchiveSystemCache cache, IArchiveConfiguration config)
		{
			var imageAction = ObjectFactory.Get<IArchiveImageGenerationAction>();
			imageAction.Setup(logger, archiveSet, BusinessObjectProviderDictionary, cache);
			yield return imageAction;
		}
	}
}
