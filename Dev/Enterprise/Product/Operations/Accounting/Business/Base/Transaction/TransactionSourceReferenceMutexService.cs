using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class TransactionSourceReferenceMutexService : IService
	{
		public static TransactionSourceReferenceMutexService GetTransactionSourceReferenceMutexService(BusinessObjectFactory factory)
		{
			var service = factory.ServiceContainer.GetService<TransactionSourceReferenceMutexService>();
			return service ?? factory.ServiceContainer.AddService(new TransactionSourceReferenceMutexService());
		}

		TransactionSourceReferenceMutexService()
		{
			SourceReferenceMutexes = new Dictionary<string, ZGlobalMutex>();
		}

		readonly Dictionary<string, ZGlobalMutex> SourceReferenceMutexes;

		public void UnlockSourceReferenceMutex(string companyCode, string sourceReference)
		{
			var mutex = GetSourceReferenceMutex(companyCode, sourceReference);
			if (mutex.IsLocked && mutex.HasLock)
			{
				mutex.Unlock();
				SourceReferenceMutexes.Remove(sourceReference);
			}
		}

		public void UnlockAllSourceReferenceMutexes()
		{
			foreach (var mutex in SourceReferenceMutexes.Where(x => x.Value.IsLocked && x.Value.HasLock))
			{
				mutex.Value.Unlock();
			}
			SourceReferenceMutexes.Clear();
		}

		public ZGlobalMutex GetSourceReferenceMutex(string companyCode, string sourceReference) => SourceReferenceMutexes.GetOrAdd(sourceReference, () => new ZGlobalMutex(MutexIDs.TransactionSourceReference, companyCode + sourceReference));

#if DEBUG
		public Dictionary<string, ZGlobalMutex> SourceReferenceMutexes_ForTestOnly => SourceReferenceMutexes;
#endif
	}
}
