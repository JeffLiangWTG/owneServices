using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Integration;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Core;

[assembly: ArchiveSystemDescriptorProvider(typeof(DummySimpleDeadlockArchiveSystemDescriptor))]

namespace Enterprise.ArchiveManager.Engine.Test.TestDoubles
{
	class DummySimpleDeadlockArchiveSystemDescriptor : DummySimpleArchiveSystemDescriptor
	{
		#region IArchiveSystemDescriptor Members

		public override string Code
			=> TestArchiveManagerConstants.Codes.DMD;

		public override MultilingualString Name
			=> ResString.GetMultilingualString("ABBDE18A-6278-41F7-80A2-B242055E337A", TestArchiveManagerConstants.Names.DMD);

		public override IEnumerable<IArchiveStageDescriptor> GetArchiveStageDescriptors(IArchiveConfiguration config)
		{
			yield return new DummySimpleDeadlockArchiveStageDescriptor();
		}

		#endregion

		public class DummySimpleDeadlockArchiveStageDescriptor : DummySimpleArchiveStageDescriptor
		{
			public override string Name
				=> ResString.GetMultilingualString("AFD6973A-B724-4E56-BC7C-035780FE6884", "Simple Deadlock Stage");

			public override IEnumerable<IArchiveAction> GetArchiveAction(IArchiveLogger logger, IArchiveSet set)
			{
				yield return new DeadlockingArchiveAction();
			}
		}

		public class DeadlockingArchiveAction : IArchiveAction
		{
			public ITransactionManager BeginTransactionWithManager()
				=> Db.Connection.BeginTransactionWithManager();

			void IArchiveAction.Execute()
			{
				var exception = SqlExceptionBuilder.CreateSqlException(1205, "Transaction (Process ID 123) was deadlocked on lock resources with another process and has been chosen as the deadlock victim. Rerun the transaction.");
				throw exception;
			}
		}
	}
}
