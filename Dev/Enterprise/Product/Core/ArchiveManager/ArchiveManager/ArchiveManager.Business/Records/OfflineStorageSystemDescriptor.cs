using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business.Records
{
	public partial class OfflineStorage
	{
		class OfflineStorageSystemDescriptor : IArchiveSystemDescriptor
		{
			public OfflineStorageSystemDescriptor(ArchiveVolumeManager volumeManager)
				=> VolumeManager = volumeManager;

			public OfflineStorageSystemDescriptor()
			{ }

			public ArchiveVolumeManager VolumeManager { get; set; }

			#region IArchiveSystemDescriptor Members

			public string Code
				=> ArchiveManagerConstants.Codes.OFL;

			public IEnumerable<IArchiveStageDescriptor> GetArchiveStageDescriptors(IArchiveConfiguration config)
			{
				yield return new Stage1(VolumeManager);
			}

			public IEnumerable<IArchiveStage> GetArchiveStages(IArchiveConfiguration config)
			{
				foreach (var stageDescriptor in GetArchiveStageDescriptors(config))
				{
					yield return new ArchiveStage(stageDescriptor, this);
				}
			}

			public IEnumerable<string> GetRegistryLogs()
			{
				yield return RegistryHelper.ToLog(SystemDataRegistry.Instance.BatchSizeControl.Name);
			}

			public MultilingualString Name
				=> (NoResString)ArchiveManagerConstants.Names.OFL;

			public MultilingualString Noun
				=> SystemDescriptorStrings.GetNoun(SystemDescriptorType.Offline);

			public MultilingualString PresentTenseVerb
				=> SystemDescriptorStrings.GetPresentTenseVerb(SystemDescriptorType.Offline);

			public MultilingualString PastTenseVerb
				=> SystemDescriptorStrings.GetPastTenseVerb(SystemDescriptorType.Offline);

			public bool AllowShouldArchiveDeclaration
				=> false;

			public bool AllowRecordsWithOrWithoutJobs
				=> false;

			public bool AllowDateParameterSelection
			=> false;

			public bool AllowUsingOnOrBeforeDateWhenWatermarkReset
				=> false;

			public IReportGenerator ReportGenerator
				=> null;

			class Stage1 : IArchiveStageDescriptor
			{
				public Stage1(ArchiveVolumeManager volumeManager)
				{
					this.volumeManager = volumeManager;
				}

				readonly ArchiveVolumeManager volumeManager;

				#region IArchiveStageDescriptor Members

				public IEnumerable<IArchiveAction> GetArchiveAction(IArchiveLogger logger, IArchiveSet set)
				{
					yield return new OfflineStorageArchiveAction(set);
				}

				public ZQuery GetMainArchiveableFilter(IArchiveConfiguration config)
				{
					var query = new ZQuery(StorageMainSchema.SM_Archived, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, config.ArchiveJobsOnOrBeforeThisDate);
					_ = query.AddToFilter(StorageMainSchema.SM_OffLine, null);
					_ = query.AddToFilter(StorageMainSchema.SM_CD2, SQLComparisonOperator.NotEqual, int.MaxValue);
					query.OrderBy = StorageMainSchema.SM_Archived.Name;

					return query;
				}

				public void Finalise(IArchiveSystemDescriptor descriptor, IArchiveLogger logger, IArchiveSchedule schedule, IArchiveConfiguration config)
				{ }

				public SchemaColumn MainArchivePKColumn
					=> StorageMainSchema.PK;

				public SchemaColumn MainArchiveNKColumn
					=> null;

				public SchemaDateTimeColumn MainDateFilterColumn
					=> StorageMainSchema.SM_Archived;

				public string Name
					=> (NoResString)"Offline Storage Stage";

				public bool IsStageUsingTempTables
					=> false;

				public void Setup(IArchiveSystemSetup systemSetup, IArchiveSchedule schedule, IArchiveConfiguration config)
				{ }

				public void SetupArchiveRelationships(IArchiveSystemSetup systemSetup, IArchiveConfiguration config)
				{ }

				public void OnArchiveSetProcessed(IArchiveSet set)
					=> throw new NotImplementedException();

				public IEnumerable<IArchivePreparationAction> GetPreparationAction(IArchiveLogger logger, IArchiveSet set, IArchiveSystemCache cache, IArchiveConfiguration config)
				{
					yield return new OfflineStoragePreparationAction(logger, set, volumeManager);
				}

				#endregion
			}

			#endregion
		}
	}
}
