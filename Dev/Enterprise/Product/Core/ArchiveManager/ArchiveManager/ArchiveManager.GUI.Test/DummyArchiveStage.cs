using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.GUI.Test
{
	sealed class DummyArchiveStage : CommonArchiveStageDescriptor
	{
		public override string Name
			=> "Dummy Stage";

		public override SchemaColumn MainArchivePKColumn
			=> DummyBizoSchema.PK;

		public override SchemaColumn MainArchiveNKColumn
			=> DummyBizoSchema.Z0_Code;

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> DummyBizoSchema.Z0_Date;

		public override ZQuery GetMainArchiveableFilter(IArchiveConfiguration config)
			=> new();

		public override IEnumerable<IArchivePreparationAction> GetPreparationAction(IArchiveLogger logger, IArchiveSet set, IArchiveSystemCache cache, IArchiveConfiguration config)
			=> throw new NotImplementedException();

		public override IEnumerable<IArchiveAction> GetArchiveAction(IArchiveLogger logger, IArchiveSet set)
			=> throw new NotImplementedException();

		public override void OnArchiveSetProcessed(IArchiveSet set)
			=> throw new NotImplementedException();
	}
}
