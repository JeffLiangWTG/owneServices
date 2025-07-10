using System.Collections.Generic;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles.DummySystemDescriptors;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Core;

[assembly: ArchiveSystemDescriptorProvider(typeof(DummyOPSArchiveSystemDescriptor))]

namespace Enterprise.ArchiveManager.Engine.Test.TestDoubles
{
	class DummyOPSArchiveSystemDescriptor : DummySystemDescriptor
	{
		readonly OPSArchiveSystemDescriptor sysDescriptor = new OPSArchiveSystemDescriptor();

		#region IArchiveSystemDescriptor Members

		public override string Code
			=> TestArchiveManagerConstants.Codes.DMO;

		public override MultilingualString Name
			=> ResString.GetMultilingualString("F7DBF767-3E62-4CA5-A8CD-29A05EA10F28", TestArchiveManagerConstants.Names.DMO);

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
			yield return new DummyOPSArchiveStageDescriptor();
		}

		#endregion

		class DummyOPSArchiveStageDescriptor : DummyStageDescriptor
		{
			readonly IArchiveStageDescriptor descriptor = new OPSArchiveStageDescriptor();

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

			public override void Finalise(IArchiveSystemDescriptor systemdescriptor, IArchiveLogger logger, IArchiveSchedule schedule, IArchiveConfiguration config)
				=> descriptor.Finalise(systemdescriptor, logger, schedule, config);
		}
	}
}
