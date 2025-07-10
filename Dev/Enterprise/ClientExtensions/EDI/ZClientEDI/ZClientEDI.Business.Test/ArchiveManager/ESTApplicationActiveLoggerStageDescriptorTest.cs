using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ArchiveManager.Test.StageDescriptors;
using Enterprise.ZArchitecture.Schema;
using ZClientEDI.Business.ArchiveManager;
using ZClientEDI.Business.ArchiveManager.StageDescriptors;

namespace ZClientEDI.Business.Test.ArchiveManager
{
	public class ESTApplicationActiveLoggerStageDescriptorTest : CommonArchiveStageDescriptorTest
	{
		protected override IArchiveSystemDescriptor SystemDescriptor
			=> new ESTPurgeSystemDescriptor();

		protected override IArchiveStageDescriptor StageDescriptor
			=> new ESTApplicationActiveLoggerStageDescriptor();

		protected override string ExpectedName
			=> "Purge Application Active Logger";

		protected override SchemaColumn ExpectedMainArchivePKColumn
			=> ApplicationActiveLoggerSchema.PK;

		protected override SchemaColumn ExpectedMainArchiveNKColumn
			=> null;

		protected override SchemaColumn ExpectedMainArchiveDateFilterColumn { get; set; } = ApplicationActiveLoggerSchema.AAL_SystemLastEditTimeUtc;

		protected override ZQuery ExpectedMainArchiveFilterWithoutDeclarations
			=> new(ExpectedMainArchiveDateFilterColumn, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, new ZDateTime(2023, 1, 1));

		protected override ZQuery ExpectedMainArchiveFilterWithDeclarations
			=> ExpectedMainArchiveFilterWithoutDeclarations;

		protected override Type[] ExpectedPreparationActions
			=> [];

		protected override Type[] ExpectedArchiveActions
			=> [];
	}
}
