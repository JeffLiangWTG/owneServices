using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business
{
	class IPSInactiveJobDeclarationArchiveStageDescriptor : IPSArchiveDescriptor
	{
		public override string Name
			=> Res.GetString("8FCAAA24-1748-4A04-8113-8F36E58B73FD", "Inactive Job Declaration Archive");

		public override SchemaColumn MainArchivePKColumn
			=> JobDeclarationSchema.PK;

		public override SchemaColumn MainArchiveNKColumn
			=> JobDeclarationSchema.JE_DeclarationReference;

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> JobDeclarationSchema.JE_SystemCreateTimeUtc;

		protected internal override SchemaBoolColumn IsCancelledSchemaColumn
			=> JobDeclarationSchema.JE_IsCancelled;

		public override IEnumerable<IArchivePreparationAction> GetPreparationAction(IArchiveLogger logger, IArchiveSet archiveSet, IArchiveSystemCache cache, IArchiveConfiguration config)
		{
			var imageAction = ObjectFactory.Get<IArchiveImageGenerationAction>();
			imageAction.Setup(logger, archiveSet, BusinessObjectProviderDictionary, cache);
			yield return imageAction;
		}
	}
}
