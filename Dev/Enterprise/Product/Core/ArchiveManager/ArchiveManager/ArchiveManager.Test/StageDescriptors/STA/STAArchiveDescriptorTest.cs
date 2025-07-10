using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions;
using Enterprise.ArchiveManager.Business.StageDescriptors.STA;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Engine.ArchiveStages;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.STA
{
	abstract class STAArchiveDescriptorTest : CommonArchiveStageDescriptorTest
	{
		protected override IArchiveSystemDescriptor SystemDescriptor
			=> new STAArchiveSystemDescriptor();

		protected override ZQuery ExpectedMainArchiveFilterWithoutDeclarations
		{
			get
			{
				var query = new ZQuery();
				_ = query.AddToFilter(ExpectedMainArchiveDateFilterColumn, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, TestDate);

				if (ExpectedParentIDColumn != null)
				{
					_ = query.AddToFilter(ExpectedParentIDColumn, SQLComparisonOperator.Equal, null);
				}

				query.OrderBy = ExpectedMainArchiveDateFilterColumn.Name;
				return query;
			}
		}

		protected override ZQuery ExpectedMainArchiveFilterWithDeclarations
			=> ExpectedMainArchiveFilterWithoutDeclarations;

		protected abstract SchemaColumn ExpectedParentIDColumn { get; }

		protected override Type[] ExpectedPreparationActions
			=> new Type[] { typeof(ArchiveImageGenerationAction) };

		protected override Type[] ExpectedArchiveActions
			=> new Type[] { typeof(PeriodArchiveCommencedAction), typeof(NullifyFKAction) };

		public void TestParentIdColumn()
		{
			var staDescriptor = StageDescriptor as STAArchiveDescriptor;
			AssertNotNull(staDescriptor);
			AssertEquals(ExpectedParentIDColumn, staDescriptor.ParentIDColumn);
		}

		protected struct ArchiveRelationshipForSTATest
		{
			public string ParentNameOverride { get; }
			public SchemaColumn ParentKeyColumnReferenceByChild { get; }
			public string ChildNameOverride { get; }
			public SchemaColumn ChildFKColumn { get; }
			public bool IsReversed { get; }

			public ArchiveRelationshipForSTATest(string parentNameOverride, SchemaColumn parentKeyColumnReferenceByChild, string childNameOverride, SchemaColumn childFKColumn, bool isReversed)
			{
				ParentNameOverride = parentNameOverride;
				ParentKeyColumnReferenceByChild = parentKeyColumnReferenceByChild;
				ChildNameOverride = childNameOverride;
				ChildFKColumn = childFKColumn;
				IsReversed = isReversed;
			}
		}

		protected abstract List<ArchiveRelationshipForSTATest> ExpectedRelationships { get; }

		public void TestArchiveRelationships()
		{
			var stage = new STAArchiveStage(StageDescriptor, SystemDescriptor);
			StageDescriptor.SetupArchiveRelationships(stage, new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, false));
			AssertEquals(ExpectedRelationships.Count, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			foreach (var relation in ExpectedRelationships)
			{
				Assert(TestHelpers.ArchiveStageHasRelationship(stage,
					relation.ParentNameOverride,
					relation.ParentKeyColumnReferenceByChild,
					relation.ChildNameOverride,
					relation.ChildFKColumn,
					isReversed: relation.IsReversed));
			}
		}
	}
}
