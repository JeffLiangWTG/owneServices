using System.Collections.Generic;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles.DummySystemDescriptors;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

[assembly: ArchiveSystemDescriptorProvider(typeof(DummyMAIOnlySystemDescriptor))]

namespace Enterprise.ArchiveManager.Engine.Test.TestDoubles.DummySystemDescriptors
{
	public class DummyMAIOnlySystemDescriptor : IArchiveSystemDescriptor
	{
		#region IArchiveSystemDescriptor Members
		public virtual MultilingualString Noun
			=> SystemDescriptorStrings.GetNoun(SystemDescriptorType.Archive);

		public virtual MultilingualString PresentTenseVerb
			=> SystemDescriptorStrings.GetPresentTenseVerb(SystemDescriptorType.Archive);

		public virtual MultilingualString PastTenseVerb
			=> SystemDescriptorStrings.GetPastTenseVerb(SystemDescriptorType.Archive);

		public virtual IReportGenerator ReportGenerator
			=> null;

		public bool AllowShouldArchiveDeclaration
			=> true;

		public bool AllowRecordsWithOrWithoutJobs
			=> false;

		public virtual bool AllowDateParameterSelection
			=> false;

		public bool AllowUsingOnOrBeforeDateWhenWatermarkReset
			=> false;

		public string Code
			=> TestArchiveManagerConstants.Codes.DMA;

		public MultilingualString Name
			=> ResString.GetMultilingualString("a172e2b7-ccd2-4344-b5c6-75f13c82bf1f", TestArchiveManagerConstants.Names.DMA);

		public IEnumerable<IArchiveStageDescriptor> GetArchiveStageDescriptors(IArchiveConfiguration config)
		{
			yield return new DummyMAIOnlyArchiveStageDescriptor();
		}

		public IEnumerable<string> GetRegistryLogs()
		{
			yield return RegistryHelper.ToLog(SystemDataRegistry.Instance.ArchiveRecordsOnOrBeforeMinimum.Name);
			yield return RegistryHelper.ToLog(SystemDataRegistry.Instance.BatchSizeControl.Name);
		}

		#endregion
		public IEnumerable<IArchiveStage> GetArchiveStages(IArchiveConfiguration config)
		{
			foreach (var stageDescriptor in GetArchiveStageDescriptors(config))
			{
				yield return new MAIOnlyArchiveStage(stageDescriptor, this);
			}
		}
	}
	public class DummyMAIOnlyArchiveStageDescriptor : DummyStageDescriptor
	{
		public override string Name
			=> Res.GetString("17e36dcf-16a9-487e-8e5c-1ce5e79f7934", "Main Archiveable Item Only Stage");

		public override void Setup(IArchiveSystemSetup systemSetup, IArchiveSchedule schedule, IArchiveConfiguration config) { }

		public override void SetupArchiveRelationships(IArchiveSystemSetup systemSetup, IArchiveConfiguration config)
		{
			systemSetup.AddRelationship(DummyBizoSchema.Z0_Code.TableName, DummyBizoSchema.Z0_Code, "DummyChild", DummyBizoSchema.Z0_FK_Code);
		}

		public override IEnumerable<IArchiveAction> GetArchiveAction(IArchiveLogger logger, IArchiveSet set)
			=> null;

		public override IEnumerable<IArchivePreparationAction> GetPreparationAction(IArchiveLogger logger, IArchiveSet set, IArchiveSystemCache cache, IArchiveConfiguration config)
			=> null;

		public override void Finalise(IArchiveSystemDescriptor descriptor, IArchiveLogger logger, IArchiveSchedule schedule, IArchiveConfiguration config)
			=> logger.LogInfo(descriptor.Code, "Clean up completed");

		public override bool IsStageUsingTempTables => false;
	}
}
