using System.Collections.Generic;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

[assembly: ArchiveSystemDescriptorProvider(typeof(DummySimpleCyclicArchiveSystemDescriptor))]

namespace Enterprise.ArchiveManager.Engine.Test.TestDoubles
{
	class DummySimpleCyclicArchiveSystemDescriptor : DummySimpleArchiveSystemDescriptor
	{
		#region IArchiveSystemDescriptor Members

		public override string Code
			=> TestArchiveManagerConstants.Codes.DML;

		public override MultilingualString Name
			=> ResString.GetMultilingualString("71b05c9f-cad5-4186-bac5-806ad100fa1a", TestArchiveManagerConstants.Names.DML);

		public override IEnumerable<IArchiveStageDescriptor> GetArchiveStageDescriptors(IArchiveConfiguration config)
		{
			yield return new DummySimpleCyclicArchiveStageDescriptor();
		}

		#endregion

		public class DummySimpleCyclicArchiveStageDescriptor : DummySimpleArchiveStageDescriptor
		{
			public override string Name
				=> Res.GetString("d313bfd7-d25f-4f7b-ae86-fe0e1ef2f4a7", "Simple Cyclic Stage");

			public override void Setup(IArchiveSystemSetup systemSetup, IArchiveSchedule schedule, IArchiveConfiguration config)
			{
				systemSetup.AddRelationship(DummyBizoSchema.Z0_Code.TableName, DummyBizoSchema.Z0_Code, DummyBizoSchema.Z0_Code.TableName, DummyBizoSchema.Z0_FK_Code);
			}
		}
	}
}
