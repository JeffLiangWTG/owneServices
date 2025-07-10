using System;
using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Management
{
	class IndividualImportLogger : SimpleLogger, IImportResult, IEntityIDWithNullableContext
	{
		internal IndividualImportLogger(DataContextType? dataContextType)
		{
			this.DataContextType = dataContextType;
		}

		IEnumerable<IEntityID> IImportResult.LinkedJobs
		{
			get { return LinkedJobs; }
		}

		List<IEntityID> LinkedJobs
		{
			get { return linkedJobs ?? (linkedJobs = new List<IEntityID>()); }
		}

		List<IEntityID> linkedJobs;

		internal void AddLinkedJobs(params IEntityID[] linkedJobs)
		{
			foreach (var linkedJob in linkedJobs)
			{
				LinkedJobs.Add(linkedJob);
			}
		}

		internal void TransferLinkedJobs(IndividualImportLogger logger)
		{
			logger.linkedJobs = linkedJobs;
			linkedJobs = null;
		}

		public DataContextType? DataContextType { get; private set; }
		DataContextType IEntityID.DataContextType => DataContextType.Value;

		public string DataContextKey
		{
			get { return GetDataContextKey != null ? GetDataContextKey.Invoke() : dataContextKey; }
			internal set
			{
				if (dataContextKey != null || GetDataContextKey != null)
				{
					ErrorReporter.ReportOnce("DataContextKey was logged more than once.  Original: [" + dataContextKey + "]  New: [" + value + "]");
				}

				dataContextKey = value;
			}
		}

		public void SetDataContextKey(Func<string> getDataContextKey)
		{
			GetDataContextKey = getDataContextKey;
		}

		Func<string> GetDataContextKey;
		string dataContextKey;

		public bool WasSuccessful
		{
			get { return wasSuccessfullyUsedByAModule && !HasErrors; }
		}

		DataContextType? IEntityIDWithNullableContext.NullableDataContextType => DataContextType;

		bool wasSuccessfullyUsedByAModule = true;
		internal void LogWasNotUsedByAModule()
		{
			wasSuccessfullyUsedByAModule = false;
		}
	}
}
