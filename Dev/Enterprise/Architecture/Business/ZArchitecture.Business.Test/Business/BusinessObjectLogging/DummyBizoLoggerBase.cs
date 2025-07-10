using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	abstract class DummyBizoLoggerBase : IBusinessObjectLogger
	{
		public DummyBizoLoggerBase(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public int CreateSaveLogCount { get; private set; }
		public int FetchForSavingCount { get; private set; }
		public int CreateDeleteLogCount { get; private set; }
		public virtual BusinessObject CreateSaveLog(BusinessObject loggingBizo)
		{
			CreateSaveLogCount++;
			return null;
		}

		public virtual BusinessObject CreateModifiedSaveLog(BusinessObject loggingBizo)
		{
			if (shouldLogIfOnlyChildrenChanged)
			{
				return CreateSaveLog(loggingBizo);
			}
			return null;
		}

		public BusinessObject CreateDeleteLog(BusinessObject loggingBizo)
		{
			CreateDeleteLogCount++;
			return null;
		}

		public void RemoveLog(BusinessObject loggingBizo)
		{
		}

		public void OnSaved(BusinessObject loggingBizo, bool isSavedSucceeded)
		{
		}

		public void FetchForSaving(BusinessObject loggingBizo)
		{
			FetchForSavingCount++;
		}

		public abstract bool RunAfterOnSavingForAllBizos { get; }

		bool shouldLogIfOnlyChildrenChanged;

		public void SetLoggingForChildren(bool isEnabled)
		{
			shouldLogIfOnlyChildrenChanged = isEnabled;
		}

		protected readonly BusinessObjectFactory factory;
	}
}
