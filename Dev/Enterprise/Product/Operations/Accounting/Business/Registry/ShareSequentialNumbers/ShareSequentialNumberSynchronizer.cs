using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public static class ShareSequentialNumberSynchronizer
	{
		public static void SynchronizeNumberGenerators(IEnumerable<INumberFountainProxy> numberGeneratorToSync, BusinessObjectFactory factory)
		{
			using (var transactionManager = ((ITransactionParticipant)factory).BeginTransactionWithManager())
			{
				var maxNum = numberGeneratorToSync.Max(x => x?.PeekPreliminary(factory) ?? 0);
				numberGeneratorToSync.All(x => { x.SetNext(factory, maxNum); return true; } );
				transactionManager.CommitTransaction();
			}
		}

		public static ShareSequentialReferenceNumbers CreateShareSequentialReferenceNumbers(bool value)
		{
			return	new ShareSequentialReferenceNumbers() { Value = value };
		}
	}
}
