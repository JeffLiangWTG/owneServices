using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles.DummySystemDescriptors;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

[assembly: ArchiveSystemDescriptorProvider(typeof(DummyComplexArchiveSystemDescriptor))]

namespace Enterprise.ArchiveManager.Engine.Test.TestDoubles
{
	class DummyComplexArchiveSystemDescriptor : DummySystemDescriptor
	{
		public override string Code
			=> TestArchiveManagerConstants.Codes.DMC;

		public override MultilingualString Name
			=> ResString.GetMultilingualString("094a8b0c-6525-49fe-92b5-c0e1029975b8", TestArchiveManagerConstants.Names.DMC);

		public override IEnumerable<IArchiveStageDescriptor> GetArchiveStageDescriptors(IArchiveConfiguration config)
		{
			yield return new DummyComplexArchiveStageDescriptor();
		}

		public class DummyComplexArchiveStageDescriptor : DummyStageDescriptor
		{
			public override ZQuery GetMainArchiveableFilter(IArchiveConfiguration config)
			{
				var query = new ZQuery(DummyBizoSchema.Z0_Date, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, config.ArchiveJobsOnOrBeforeThisDate);
				_ = query.AddToFilter(DummyBizoSchema.Z0_FK_Code, SQLComparisonOperator.Equal, string.Empty);
				_ = query.AddToFilter(DummyBizoSchema.Z0_Number, SQLComparisonOperator.NotEqual, 0);
				return query;
			}

			public override string Name
				=> Res.GetString("823f4cf6-86c5-47ea-bc89-fd5823a5358d", "Complex Stage");

			public override void Setup(IArchiveSystemSetup systemSetup, IArchiveSchedule schedule, IArchiveConfiguration config)
				=> SetupArchiveRelationships(systemSetup, config);

			public override void SetupArchiveRelationships(IArchiveSystemSetup systemSetup, IArchiveConfiguration config)
			{
				systemSetup.AddRelationship(DummyBizoSchema.Z0_Code.TableName, DummyBizoSchema.Z0_Code, "DummyChild", DummyBizoSchema.Z0_FK_Code);
				systemSetup.AddRelationship("DummyChild", DummyBizoSchema.Z0_Code, "DummyGrandchild", DummyBizoSchema.Z0_FK_Code);
				systemSetup.AddRelationship(DummyBizoSchema.PK.TableName, DummyBizoSchema.PK, "DummyWithDependents", DummyBizoSchema.Z0_Guid);
				systemSetup.AddRelationship("DummyWithDependents", DummyBizoSchema.PK, "DummyDependent", DummyDependentBizoSchema.ZD1_Z0);

				// Recursive:
				systemSetup.AddRelationship("DummyChild", DummyBizoSchema.Z0_AnotherNumber, DummyBizoSchema.Z0_Number.TableName, DummyBizoSchema.Z0_Number, true);
				systemSetup.AddRelationshipToAllPKs(StmNoteSchema.ST_ParentID);
			}

			public override IEnumerable<IArchiveAction> GetArchiveAction(IArchiveLogger logger, IArchiveSet set)
				=> null;

			public override IEnumerable<IArchivePreparationAction> GetPreparationAction(IArchiveLogger logger, IArchiveSet set, IArchiveSystemCache cache, IArchiveConfiguration config)
			{
				var action = new ArchiveToImageAction(logger, set, ProviderCacheOrDictionary);
				yield return action;
			}
		}
	}
}
