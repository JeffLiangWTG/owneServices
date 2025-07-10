using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business.StageDescriptors;
using Enterprise.ArchiveManager.Business.SystemDescriptors;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.PAL
{
	class PALArchiveStageDescriptorTest : CommonArchiveStageDescriptorTest
	{
		protected override IArchiveStageDescriptor StageDescriptor => new PALArchiveStageDescriptor();

		protected override IArchiveSystemDescriptor SystemDescriptor => new PALArchiveSystemDescriptor();

		protected override string ExpectedName
			=> "Purge Activity Logs";

		protected override SchemaColumn ExpectedMainArchivePKColumn
			=> StmActivityLogSchema.PK;

		protected override SchemaColumn ExpectedMainArchiveNKColumn
			=> null;

		protected override SchemaColumn ExpectedMainArchiveDateFilterColumn { get; set; } = StmActivityLogSchema.S7_OpenDateTimeUtc;

		protected override ZQuery ExpectedMainArchiveFilterWithoutDeclarations
		{
			get
			{
				var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, false);

				var query = new ZDBOnlyQuery(typeof(StmActivityLog));
				_ = query.AddToFilter(StmActivityLogSchema.S7_OpenDateTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, config.ArchiveJobsOnOrBeforeThisDate);
				query.OrderBy = ExpectedMainArchiveDateFilterColumn.Name;
				return query;
			}
		}

		protected override ZQuery ExpectedMainArchiveFilterWithDeclarations
			=> ExpectedMainArchiveFilterWithoutDeclarations;

		protected override Type[] ExpectedPreparationActions
			=> Array.Empty<Type>();

		protected override Type[] ExpectedArchiveActions
			=> Array.Empty<Type>();

		[TestDate(2023, 01, 01, 12, 0, 0, 0)]
		public override void TestMainArchiveableFilter()
		{
			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, false);
			_config = config;
			var mainArchiveableFilter = StageDescriptor.GetMainArchiveableFilter(config).LiteralTextSqlFormatted;
			AssertEquals(ExpectedMainArchiveFilterWithoutDeclarations.LiteralTextSqlFormatted, mainArchiveableFilter);

			config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, true);
			_config = config;
			mainArchiveableFilter = StageDescriptor.GetMainArchiveableFilter(config).LiteralTextSqlFormatted;
			AssertEquals(ExpectedMainArchiveFilterWithDeclarations.LiteralTextSqlFormatted, mainArchiveableFilter);
		}
	}
}
