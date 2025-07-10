using System;
using System.Linq;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	[System.Diagnostics.DebuggerDisplay("{Name}")]
	public class BusinessObjectFactoryStatistic : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema
		public static class Schema
		{
			public const string Name = "Name";
			public const string BusinessObjectCount = "BusinessObjectCount";
			public const string DataRowCount = "DataRowCount";
			public const string ActiveFetchHintsCount = "ActiveFetchHintsCount";
			public const string DatabaseLoadCount = "DatabaseLoadCount";
			public const string ChildFactoriesCount = "ChildFactoriesCount";
			public const string ChildFactoryIDs = "ChildFactoryIDs";
			public const string FactoryCreationTime = "FactoryCreationTime";
			public const string FactoryIsDeactivated = "FactoryIsDeactivated";
		}

		#endregion

		#region Constructor

		public BusinessObjectFactoryStatistic(BusinessObjectFactory factoryToMonitor)
		{
			if (factoryToMonitor == null)
			{
				throw new ArgumentNullException(nameof(factoryToMonitor));
			}
			this.factoryToMonitor = factoryToMonitor;
		}

		#endregion

		#region Properties

		public ZString Name
		{
			get { return factoryToMonitor.NameForDebugging; }
		}

		public ZPropertyInfo NameInfo
		{
			get { return GetZPropertyInfo(Schema.Name); }
		}

		public ZDateTime FactoryCreationTime
		{
			get { return factoryToMonitor.InstantiationTime; }
		}

		public ZPropertyInfo FactoryCreationTimeInfo
		{
			get { return GetZPropertyInfo(Schema.FactoryCreationTime); }
		}

		public ZInt BusinessObjectCount
		{
			get { return FactoryInternals.NumberOfBusinessObjects; }
		}

		public ZPropertyInfo BusinessObjectCountInfo
		{
			get { return GetZPropertyInfo(Schema.BusinessObjectCount); }
		}

		public ZInt DataRowCount
		{
			get
			{
				if (factoryToMonitor.IsOwnedByCurrentThread)
				{
					return factoryToMonitor.RowFactory.GetDataRowCount();
				}
				return -1;
			}
		}

		public ZPropertyInfo DataRowCountInfo
		{
			get { return GetZPropertyInfo(Schema.DataRowCount); }
		}

		public ZInt ActiveFetchHintsCount
		{
			get
			{
				if (factoryToMonitor.IsOwnedByCurrentThread)
				{
					return factoryToMonitor.ActiveTableFetchHints;
				}
				return -1;
			}
		}

		public ZPropertyInfo ActiveFetchHintsCountInfo
		{
			get { return GetZPropertyInfo(Schema.ActiveFetchHintsCount); }
		}

		public ZInt DatabaseLoadCount
		{
			get { return factoryToMonitor.DatabaseLoadCount; }
		}

		public ZBool IncludeWithOtherFactoriesForIssueReport
		{
			get { return FactoryInternals.IncludeWithOtherFactoriesForIssueReport; }
		}

		public ZPropertyInfo DatabaseLoadCountInfo
		{
			get { return GetZPropertyInfo(Schema.DatabaseLoadCount); }
		}

		public ZInt ChildFactoriesCount
		{
			get { return factoryToMonitor.ChildFactories.Count; }
		}

		public ZPropertyInfo ChildFactoriesCountInfo
		{
			get { return GetZPropertyInfo(Schema.ChildFactoriesCount); }
		}

		public ZString ChildFactoryIDs
		{
			get { return String.Join(", ", factoryToMonitor.ChildFactories.ToArray().Take(100).Select(x => ((BusinessObjectFactory)x)._Instance.ToString())); }
		}

		public ZPropertyInfo ChildFactoriesIDInfo
		{
			get { return GetZPropertyInfo(Schema.ChildFactoryIDs); }
		}

		public ZBool FactoryIsDeactivated
		{
			get { return factoryToMonitor.IsDeactivated; }
		}

		public ZPropertyInfo FactoryIsDeactivatedInfo
		{
			get { return GetZPropertyInfo(Schema.FactoryIsDeactivated); }
		}

		public ZString AllocationPath
		{
			get { return factoryToMonitor.AllocationPath; }
		}

		public ZInt? CreationThreadID
		{
			get { return factoryToMonitor.ThreadSentry.CreationThread.ThreadID; }
		}

		public ZString BusinessObjectsInformation
		{
			get { return factoryToMonitor.BusinessObjectsInformation; }
		}

		public bool IsOwnedByCurrentThread
		{
			get { return factoryToMonitor.IsOwnedByCurrentThread; }
		}

		#endregion

		#region Overrides

		public override void Delete()
		{
			throw new NotSupportedException();
		}

		#endregion

		#region Implementation

		readonly BusinessObjectFactory factoryToMonitor;

		public IBusinessObjectFactoryInternals FactoryInternals
		{
			get { return factoryToMonitor; }
		}

		#endregion
	}
}
