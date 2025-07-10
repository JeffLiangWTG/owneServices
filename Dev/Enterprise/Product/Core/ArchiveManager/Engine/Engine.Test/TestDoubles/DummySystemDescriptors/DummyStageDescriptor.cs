using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Engine.Test.TestDoubles.DummySystemDescriptors
{
	public abstract class DummyStageDescriptor : IArchiveStageDescriptor
	{
		readonly object providerCacheOrDictionaryLock = new ();

		public abstract string Name { get; }

		public SchemaColumn MainArchivePKColumn
			=> DummyBizoSchema.PK;

		public virtual SchemaColumn MainArchiveNKColumn
			=> DummyBizoSchema.Z0_Code;

		public SchemaDateTimeColumn MainDateFilterColumn
			=> DummyBizoSchema.Z0_Date;

		public virtual bool IsStageUsingTempTables
			=> true;

		public virtual void Finalise(IArchiveSystemDescriptor descriptor, IArchiveLogger logger, IArchiveSchedule schedule, IArchiveConfiguration config)
		{ }

		public abstract IEnumerable<IArchiveAction> GetArchiveAction(IArchiveLogger logger, IArchiveSet set);

		public virtual ZQuery GetMainArchiveableFilter(IArchiveConfiguration config)
		{
			var query = new ZQuery(DummyBizoSchema.Z0_Date, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, config.ArchiveJobsOnOrBeforeThisDate);
			query.OrderBy = DummyBizoSchema.Z0_Code.Name;
			return query;
		}

		public abstract IEnumerable<IArchivePreparationAction> GetPreparationAction(IArchiveLogger logger, IArchiveSet set, IArchiveSystemCache cache, IArchiveConfiguration config);

		public virtual void OnArchiveSetProcessed(IArchiveSet set)
		{ }

		public abstract void Setup(IArchiveSystemSetup systemSetup, IArchiveSchedule schedule, IArchiveConfiguration config);

		public abstract void SetupArchiveRelationships(IArchiveSystemSetup systemSetup, IArchiveConfiguration config);

		readonly ArchiveableBusinessObjectProviderCache providerCacheOrDictionary;

		protected ArchiveableBusinessObjectProviderCache ProviderCacheOrDictionary
		{
			get
			{
				if (providerCacheOrDictionary == null)
				{
					lock (providerCacheOrDictionaryLock)
					{
						if (providerCacheOrDictionary == null)
						{
							var helper = new DummyProviderHelper();
							return helper.GetAndRegisterProviderCache(providerCacheOrDictionary);
						}
					}
				}
				return providerCacheOrDictionary;
			}
		}
	}
}
