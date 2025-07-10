using System.Collections.Generic;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles.DummySystemDescriptors;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

[assembly: ArchiveSystemDescriptorProvider(typeof(DummySimpleArchiveSystemDescriptor))]

namespace Enterprise.ArchiveManager.Engine.Test.TestDoubles
{
	public class DummySimpleArchiveSystemDescriptor : DummySystemDescriptor
	{
		#region IArchiveSystemDescriptor Members

		public override string Code
			=> TestArchiveManagerConstants.Codes.DMS;

		public override MultilingualString Name
			=> ResString.GetMultilingualString("4c65f96e-5650-41ea-aadc-18599e42a059", TestArchiveManagerConstants.Names.DMS);

		public override IEnumerable<IArchiveStageDescriptor> GetArchiveStageDescriptors(IArchiveConfiguration config)
		{
			yield return new DummySimpleArchiveStageDescriptor();
		}

		public override IEnumerable<string> GetRegistryLogs()
		{
			yield return RegistryHelper.ToLog(SystemDataRegistry.Instance.ArchiveRecordsOnOrBeforeMinimum.Name);
			yield return RegistryHelper.ToLog(SystemDataRegistry.Instance.BatchSizeControl.Name);
		}

		#endregion

		public class DummySimpleArchiveStageDescriptor : DummyStageDescriptor
		{
			public override string Name
				=> Res.GetString("27f41d67-d840-416d-9fa1-685a8ae71375", "Simple Stage");

			public override void Setup(IArchiveSystemSetup systemSetup, IArchiveSchedule schedule, IArchiveConfiguration config)
				=> SetupArchiveRelationships(systemSetup, config);

			public override void SetupArchiveRelationships(IArchiveSystemSetup systemSetup, IArchiveConfiguration config)
			{
				systemSetup.AddRelationship(DummyBizoSchema.Z0_Code.TableName, DummyBizoSchema.Z0_Code, "DummyChild", DummyBizoSchema.Z0_FK_Code);
			}

			public override IEnumerable<IArchiveAction> GetArchiveAction(IArchiveLogger logger, IArchiveSet set)
				=> null;

			public override IEnumerable<IArchivePreparationAction> GetPreparationAction(IArchiveLogger logger, IArchiveSet set, IArchiveSystemCache cache, IArchiveConfiguration config)
			{
				var action = new ArchiveToImageAction(logger, set, ProviderCacheOrDictionary);
				action.StallActionForSeconds = StallActionForSeconds;
				StallActionForSeconds = 0; //reset
				yield return action;
			}

			public override void Finalise(IArchiveSystemDescriptor descriptor, IArchiveLogger logger, IArchiveSchedule schedule, IArchiveConfiguration config)
				=> logger.LogInfo(descriptor.Code, "Clean up completed");

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
			public static int StallActionForSeconds;
		}
	}
}
