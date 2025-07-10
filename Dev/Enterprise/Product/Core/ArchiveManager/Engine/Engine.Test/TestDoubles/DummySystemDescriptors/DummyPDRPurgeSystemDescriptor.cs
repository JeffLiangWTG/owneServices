using System.Collections.Generic;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles.DummySystemDescriptors;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Core;

[assembly: ArchiveSystemDescriptorProvider(typeof(DummyPDRPurgeSystemDescriptor))]

namespace Enterprise.ArchiveManager.Engine.Test.TestDoubles
{
	class DummyPDRPurgeSystemDescriptor : DummySystemDescriptor
	{
		readonly PDRPurgeSystemDescriptor sysDescriptor = new PDRPurgeSystemDescriptor();

		#region IArchiveSystemDescriptor Members

		public override string Code
			=> TestArchiveManagerConstants.Codes.DMP;

		public override MultilingualString Name
			=> ResString.GetMultilingualString("E2C946C8-0111-4C95-8769-16D26265A130", TestArchiveManagerConstants.Names.DMP);

		public override MultilingualString Noun
			=> sysDescriptor.Noun;

		public override MultilingualString PresentTenseVerb
			=> sysDescriptor.PresentTenseVerb;

		public override MultilingualString PastTenseVerb
			=> sysDescriptor.PastTenseVerb;

		public override bool AllowDateParameterSelection
			=> true;

		public override IReportGenerator ReportGenerator
			=> sysDescriptor.ReportGenerator;

		public override IEnumerable<IArchiveStageDescriptor> GetArchiveStageDescriptors(IArchiveConfiguration config)
		{
			yield return new DummyPDRPurgeStageDescriptor();
		}

		#endregion

		class DummyPDRPurgeStageDescriptor : DummyStageDescriptor
		{
			readonly IArchiveStageDescriptor descriptor = new PDRArchiveStageDescriptor();

			public override string Name
				=> descriptor.Name;

			public override void Setup(IArchiveSystemSetup systemSetup, IArchiveSchedule schedule, IArchiveConfiguration config)
				=> descriptor.Setup(systemSetup, schedule, config);

			public override void SetupArchiveRelationships(IArchiveSystemSetup systemSetup, IArchiveConfiguration config)
				=> descriptor.SetupArchiveRelationships(systemSetup, config);

			public override IEnumerable<IArchiveAction> GetArchiveAction(IArchiveLogger logger, IArchiveSet set)
				=> null;

			public override IEnumerable<IArchivePreparationAction> GetPreparationAction(IArchiveLogger logger, IArchiveSet set, IArchiveSystemCache cache, IArchiveConfiguration config)
			{
				var action = new ArchiveToImageAction(logger, set, ProviderCacheOrDictionary);
				action.StallActionForSeconds = 0;
				yield return action;
			}

			public override void Finalise(IArchiveSystemDescriptor systemDescriptor, IArchiveLogger logger, IArchiveSchedule schedule, IArchiveConfiguration config)
				=> descriptor.Finalise(systemDescriptor, logger, schedule, config);
		}
	}
}
