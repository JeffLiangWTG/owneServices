using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

[assembly: ArchiveSystemDescriptorProvider(typeof(DummyWithExcludeFromPurgeSystem))]

namespace Enterprise.ArchiveManager.Engine.Test.TestDoubles
{
	class DummyWithExcludeFromPurgeSystem : DummyComplexArchiveSystemDescriptor
	{
		public override string Code
			=> TestArchiveManagerConstants.Codes.DEP;

		public override MultilingualString Name
			=> ResString.GetMultilingualString("0babd406-5555-40b3-824a-a68675ca61f5", TestArchiveManagerConstants.Names.DEP);

		public override IEnumerable<IArchiveStageDescriptor> GetArchiveStageDescriptors(IArchiveConfiguration config)
		{
			yield return new DummyWithExcludeFromPurgeStage();
		}

		class DummyWithExcludeFromPurgeStage : DummyComplexArchiveStageDescriptor
		{
			public override IEnumerable<IArchiveAction> GetArchiveAction(IArchiveLogger logger, IArchiveSet set)
			{
				yield return new ExcludeFromPurgeAction(set);
			}

			class ExcludeFromPurgeAction : IArchiveAction
			{
				public ExcludeFromPurgeAction(IArchiveSet set)
				{
					this.set = set;
				}

				#region IArchiveAction Members

				public ITransactionManager BeginTransactionWithManager()
					=> new StubTransactionManager();

				public void Execute()
				{
					var factory = new BusinessObjectFactory();
					var dummiesNotPurgeable = new DynamicBusinessObjectCollection(factory);
					dummiesNotPurgeable.Load("SELECT Z0_PK FROM dbo.DummyBizo WHERE Z0_Code = @Code", new ZSqlParameter[] { ZSqlParameter.New("@Code", "0C2G1", DummyBizoSchema.Z0_Code) });

					foreach (var obj in dummiesNotPurgeable.Cast<DynamicBusinessObject>())
					{
						var pk = (ZGuid)obj[DummyBizoSchema.PK.Name];
						var item = set.GetArchiveItem(pk.ToGuid());
						item.Purgeable = false;
					}
				}

				#endregion

				readonly IArchiveSet set;
			}
		}
	}
}
