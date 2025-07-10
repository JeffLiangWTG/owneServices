using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.PDR
{
	class PDRPurgeStageDescriptorTest : JobHeaderArchiveStageDescriptorTest
	{
		protected override IArchiveStageDescriptor StageDescriptor
			=> new PDRArchiveStageDescriptor();

		protected override IArchiveSystemDescriptor SystemDescriptor
			=> new PDRPurgeSystemDescriptor();

		protected override string ExpectedName
			=> "Purge Documents and Records";

		protected override SchemaColumn ExpectedMainArchivePKColumn
			=> JobHeaderSchema.PK;

		protected override SchemaColumn ExpectedMainArchiveNKColumn
			=> JobHeaderSchema.JH_JobNum;

		protected override SchemaColumn ExpectedMainArchiveDateFilterColumn { get; set; } = JobHeaderSchema.JH_A_JCL;

		protected override ZQuery ExpectedMainArchiveFilterWithoutDeclarations
		{
			get
			{
				var query = new ZQuery();
				ApplyCommonFiltersToQuery(query, includeDeclarations: false);
				query.OrderBy = ExpectedMainArchiveDateFilterColumn.Name;
				return query;
			}
		}

		protected override ZQuery ExpectedMainArchiveFilterWithDeclarations
		{
			get
			{
				var query = new ZQuery();
				ApplyCommonFiltersToQuery(query, includeDeclarations: true);
				query.OrderBy = ExpectedMainArchiveDateFilterColumn.Name;
				return query;
			}
		}

		protected override Type[] ExpectedPreparationActions
			=> new Type[] { typeof(ArchiveImageDeletionAction) };

		protected override Type[] ExpectedArchiveActions
			=> new Type[] { typeof(PeriodArchiveCommencedAction), typeof(NullifyFKAction) };

		public override void TestMainArchiveDateFilterColumn()
		{
			var archiveStage = new PDRArchiveStageDescriptor();

			_config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, false);
			_config.SetIsFilteringByJobOpenDate(false);
			_ = archiveStage.GetMainArchiveableFilter(_config);

			AssertEquals("When IsFilteringByJobOpenDate=false, MainDateFilterColumn should be the default JH_A_JCL column", ExpectedMainArchiveDateFilterColumn.Name, archiveStage.MainDateFilterColumn.Name);

			_config.SetIsFilteringByJobOpenDate(true);
			_ = archiveStage.GetMainArchiveableFilter(_config);

			AssertEquals("When IsFilteringByJobOpenDate=true, MainDateFilterColumn should use the JH_A_JOP column", JobHeaderSchema.JH_A_JOP.Name, archiveStage.MainDateFilterColumn.Name);
		}

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

			ExpectedMainArchiveDateFilterColumn = JobHeaderSchema.JH_A_JOP;
			config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, false);
			config.SetIsFilteringByJobOpenDate(true);
			_config = config;
			mainArchiveableFilter = StageDescriptor.GetMainArchiveableFilter(config).LiteralTextSqlFormatted;
			AssertEquals(ExpectedMainArchiveFilterWithoutDeclarations.LiteralTextSqlFormatted, mainArchiveableFilter);
		}
	}
}
