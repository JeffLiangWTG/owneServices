using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

[assembly: ArchiveSystemDescriptorProvider(typeof(DummyAdditionalActionArchiveSystemDescriptor))]

namespace Enterprise.ArchiveManager.Engine.Test.TestDoubles
{
	class DummyAdditionalActionArchiveSystemDescriptor : DummySimpleArchiveSystemDescriptor, IArchiveSystemDescriptor
	{
		public override string Code
			=> TestArchiveManagerConstants.Codes.DMR;

		public override MultilingualString Name
			=> ResString.GetMultilingualString("A45350FA-E51A-4DF3-8722-EF6D705B62B6", TestArchiveManagerConstants.Names.DMR);

		public override IEnumerable<IArchiveStageDescriptor> GetArchiveStageDescriptors(IArchiveConfiguration config)
		{
			yield return new DummyAdditionalArchiveStageDescriptor();
		}

		public class DummyAdditionalArchiveStageDescriptor : DummySimpleArchiveStageDescriptor
		{
			public override IEnumerable<IArchiveAction> GetArchiveAction(IArchiveLogger logger, IArchiveSet set)
			{
				foreach (var item in set.GetArchiveItems())
				{
					if (item.PKColumn == DummyBizoSchema.PK)
					{
						yield return new NullifyFKAction(item, logger, set);
					}
				}
			}

			class NullifyFKAction : IArchiveAction
			{
				public NullifyFKAction(IArchiveItem relatedItem, IArchiveLogger logger, IArchiveSet set)
				{
					this.logger = logger;
					factory = new BusinessObjectFactory();
					item = relatedItem;
					this.set = set;
				}

				readonly IArchiveSet set;
				readonly IArchiveItem item;
				readonly IArchiveLogger logger;
				readonly BusinessObjectFactory factory;
				DummyDependantBusinessObject[] bizOs;
				Dictionary<ZGuid, ZGuid> rollbackInfo;

				#region IArchiveAction Members

				public ITransactionManager BeginTransactionWithManager()
				{
					bizOs = factory.Load<DummyDependantBusinessObject>(new ZQuery(DummyDependentBizoSchema.ZD1_Z0, item.PK));
					rollbackInfo = new Dictionary<ZGuid, ZGuid>();
					return new TransactionManager(this);
				}

				public void Execute()
				{
					var atLeastOneTriggered = false;

					foreach (var biz in bizOs)
					{
						if (biz.ZD1_Code.StartsWith(TriggerCode))
						{
							atLeastOneTriggered = true;
						}
					}

					if (atLeastOneTriggered)
					{
						logger.LogInfo(set.SystemDescriptor.Code, "Nullifying ZD1_Z0 Foreign Key on DummyDependentBusinessObject");
					}

					var shouldThrow = false;

					foreach (var biz in bizOs)
					{
						rollbackInfo.Add(biz.PK, biz.ZD1_Z0);

						if (biz.ZD1_Number == 666)
						{
							shouldThrow = true;
						}
						else if (biz.ZD1_Code.StartsWith(TriggerCode))
						{
							biz.ZD1_Z0 = ZGuid.Empty;
						}
					}

					if (shouldThrow)
					{
						throw new Exception();
					}

					factory.Save();
				}

				#endregion

				class TransactionManager : BaseTransactionManager<NullifyFKAction>
				{
					public TransactionManager(NullifyFKAction owner, Action rollbackAction = null) : base(owner, rollbackAction) { }

					protected override void Commit() { }

					protected override void Rollback()
					{
						foreach (var biz in owner.bizOs)
						{
							biz.ZD1_Z0 = owner.rollbackInfo[biz.PK]; // TestWithFactory already has a transaction, so need to implement this way for testing the roll back value.
						}

						owner.factory.Save();
					}
				}
			}

			public const string TriggerCode = "TRIG";
		}
	}
}
