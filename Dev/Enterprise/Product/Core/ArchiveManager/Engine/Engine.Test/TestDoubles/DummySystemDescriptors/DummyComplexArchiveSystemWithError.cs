using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles.DummySystemDescriptors;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

[assembly: ArchiveSystemDescriptorProvider(typeof(DummyComplexArchiveSystemWithErrorDescriptor))]

namespace Enterprise.ArchiveManager.Engine.Test.TestDoubles
{
	class DummyComplexArchiveSystemWithErrorDescriptor : DummySystemDescriptor
	{
		public override string Code
			=> TestArchiveManagerConstants.Codes.DME;

		public override MultilingualString Name
			=> ResString.GetMultilingualString("3897d503-6701-474e-8062-061f2426b08c", TestArchiveManagerConstants.Names.DME);

		public override IEnumerable<IArchiveStageDescriptor> GetArchiveStageDescriptors(IArchiveConfiguration config)
		{
			yield return new DummyComplexArchiveStageDescriptorWithError();
		}

		public class DummyComplexArchiveStageDescriptorWithError : DummyStageDescriptor
		{
			public override ZQuery GetMainArchiveableFilter(IArchiveConfiguration config)
			{
				var query = new ZQuery(DummyBizoSchema.Z0_Date, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, config.ArchiveJobsOnOrBeforeThisDate);
				_ = query.AddToFilter(DummyBizoSchema.Z0_FK_Code, SQLComparisonOperator.Equal, string.Empty);
				return query;
			}

			public override string Name
				=> Res.GetString("8c545d47-8b4e-4ceb-938e-8e838a23c971", "Error Stage");

			public static bool RaiseErrorEnabled;

			public override void Setup(IArchiveSystemSetup systemSetup, IArchiveSchedule schedule, IArchiveConfiguration config)
			{
				if (RaiseErrorEnabled)
				{
					SetupArchiveRelationships(systemSetup, config);
				}
			}

			public override void SetupArchiveRelationships(IArchiveSystemSetup systemSetup, IArchiveConfiguration config)
			{
				systemSetup.AddRelationship(DummyBizoSchema.Z0_Code, DummyBizoSchema.Z0_FK_Code);
				systemSetup.AddRelationship(DummyBizoSchema.Z0_NVarCharMax, DummyBizoSchema.Z0_Code);
			}

			public override IEnumerable<IArchiveAction> GetArchiveAction(IArchiveLogger logger, IArchiveSet set)
			{
				yield break;
			}

			public override IEnumerable<IArchivePreparationAction> GetPreparationAction(IArchiveLogger logger, IArchiveSet set, IArchiveSystemCache cache, IArchiveConfiguration config)
			{
				var action = new ArchiveToImageAction(logger, set, ProviderCacheOrDictionary);
				yield return action;
			}

			public override void OnArchiveSetProcessed(IArchiveSet set)
				=> throw new NotImplementedException();
		}
	}
}
