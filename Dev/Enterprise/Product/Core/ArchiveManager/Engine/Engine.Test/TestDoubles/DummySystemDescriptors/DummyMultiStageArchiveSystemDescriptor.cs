using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles.DummySystemDescriptors;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

[assembly: ArchiveSystemDescriptorProvider(typeof(DummyMultiStageArchiveSystemDescriptor))]

namespace Enterprise.ArchiveManager.Engine.Test.TestDoubles
{
	class DummyMultiStageArchiveSystemDescriptor : DummySystemDescriptor
	{
		#region IArchiveSystemDescriptor Members

		public override string Code
			=> TestArchiveManagerConstants.Codes.DMM;

		public override MultilingualString Name
			=> ResString.GetMultilingualString("b40dab1d-27c3-44d0-84b3-9aef1124da62", TestArchiveManagerConstants.Names.DMM);

		public override IEnumerable<IArchiveStageDescriptor> GetArchiveStageDescriptors(IArchiveConfiguration config)
		{
			yield return new StageDescriptor("Stage1");
			yield return new StageDescriptor("Stage2");
			yield return new StageDescriptor("Stage3");
		}

		#endregion

		public class StageDescriptor : DummyStageDescriptor
		{
			public StageDescriptor(string stageText)
			{
				Name = stageText;
			}

			public override string Name { get; }

			public override ZQuery GetMainArchiveableFilter(IArchiveConfiguration config)
			{
				var query = new ZQuery(DummyBizoSchema.Z0_Date, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, config.ArchiveJobsOnOrBeforeThisDate);
				_ = query.AddToFilter(DummyBizoSchema.Z0_Description, Name);
				_ = query.OrderBy = DummyBizoSchema.Z0_Date.Name;
				return query;
			}

			public override void Setup(IArchiveSystemSetup systemSetup, IArchiveSchedule schedule, IArchiveConfiguration config)
			{ }

			public override void SetupArchiveRelationships(IArchiveSystemSetup systemSetup, IArchiveConfiguration config)
			{ }

			public override IEnumerable<IArchiveAction> GetArchiveAction(IArchiveLogger logger, IArchiveSet set)
				=> null;

			public override IEnumerable<IArchivePreparationAction> GetPreparationAction(IArchiveLogger logger, IArchiveSet set, IArchiveSystemCache cache, IArchiveConfiguration config)
			{
				var action = new ArchiveToImageAction(logger, set, ProviderCacheOrDictionary);
				action.StallActionForSeconds = StallActionForSeconds;
				StallActionForSeconds = 0; //reset
				yield return action;
			}

			public override void OnArchiveSetProcessed(IArchiveSet set)
				=> throw new System.NotImplementedException();

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
			public static int StallActionForSeconds;
		}
	}
}
