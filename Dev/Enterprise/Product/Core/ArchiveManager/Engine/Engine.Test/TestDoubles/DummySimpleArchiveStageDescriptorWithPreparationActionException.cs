using System;
using System.Collections.Generic;
using Enterprise.ArchiveManager.Integration;
using static Enterprise.ArchiveManager.Engine.Test.TestDoubles.DummySimpleArchiveSystemDescriptor;

namespace Enterprise.ArchiveManager.Engine.Test.TestDoubles
{
	class DummySimpleArchiveStageDescriptorWithPreparationActionException : DummySimpleArchiveStageDescriptor
	{
		public override IEnumerable<IArchivePreparationAction> GetPreparationAction(IArchiveLogger logger, IArchiveSet set, IArchiveSystemCache cache, IArchiveConfiguration config)
		{
			var action = new DummyArchivePreparationActionWithException();
			yield return action;
		}

		class DummyArchivePreparationActionWithException : IArchivePreparationAction
		{
			public void Execute()
			{
				var exception = new Exception();
				exception.Data.Add("ArchiveItemInfo", "Info about the ArchiveItem.");
				throw exception;
			}
		}
	}
}
