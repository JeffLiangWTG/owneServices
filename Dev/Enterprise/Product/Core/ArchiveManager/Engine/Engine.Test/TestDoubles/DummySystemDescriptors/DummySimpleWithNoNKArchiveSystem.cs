using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Core;

[assembly: ArchiveSystemDescriptorProvider(typeof(DummySimpleWithNoNKArchiveSystemDescriptor))]

namespace Enterprise.ArchiveManager.Engine.Test.TestDoubles
{
	class DummySimpleWithNoNKArchiveSystemDescriptor : DummySimpleArchiveSystemDescriptor
	{
		#region IArchiveSystemDescriptor Members

		public override string Code
			=> TestArchiveManagerConstants.Codes.DNN;

		public override MultilingualString Name
			=> ResString.GetMultilingualString("B450D9C5-FD9F-47A6-AE8C-DE0BAD25B904", TestArchiveManagerConstants.Names.DNN);
		public override IEnumerable<IArchiveStageDescriptor> GetArchiveStageDescriptors(IArchiveConfiguration config)
		{
			yield return new DummySimpleWithNoNKStageDescriptor();
		}

		#endregion

		public class DummySimpleWithNoNKStageDescriptor : DummySimpleArchiveStageDescriptor
		{
			public override string Name
				=> (NoResString)"Simple No NK Stage";

			public override SchemaColumn MainArchiveNKColumn
				=> null;
		}
	}
}
