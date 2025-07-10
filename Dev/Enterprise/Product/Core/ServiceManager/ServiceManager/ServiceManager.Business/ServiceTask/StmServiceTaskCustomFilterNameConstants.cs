using System;
using System.Collections.Generic;
using System.Data;

namespace Enterprise.ServiceManager.Business
{
	public static class StmServiceTaskCustomFilterNameConstants
	{
		public const string RegisteredOnHosts = "RegisteredOnHosts";
		public const string BindingTypes = "BindingTypes";
		public const string BindingsCount = "BindingsCount";
		public const string ProcessId = "ProcessID";
		public const string SecondsInQueue = "SecondsInQueue";
		public const string SecondsRunning = "SecondsRunning";
		public const string PlaceInQueue = "PlaceInQueue";
		public const string RunningCount = "RunningCount";
		public const string StatusString = "StatusString";
		public const string NextRunTime = "SST_NextRunTime";
		public const string LastRunTime = "LastRunTime";
		public const string LastErrorTime = "LastErrorTime";
		public const string ErrorCountLast24Hours = "ErrorCountLast24Hours";
		public const string MutuallyExclusiveGroup = "MutuallyExclusiveGroup";
		public const string Description = "Description";
		public const string Category = "Category";

		internal static List<DataColumn> ServiceStatusCustomFilterCollection => new List<DataColumn>
		{
			new DataColumn(RegisteredOnHosts, typeof(string)),
			new DataColumn(ProcessId, typeof(string)),
			new DataColumn(SecondsInQueue, typeof(string)),
			new DataColumn(SecondsRunning, typeof(string)),
			new DataColumn(PlaceInQueue, typeof(string)),
			new DataColumn(RunningCount, typeof(int)),
			new DataColumn(StatusString, typeof(string)),
			new DataColumn(BindingTypes, typeof(string)),
			new DataColumn(BindingsCount, typeof(int)),
			new DataColumn(NextRunTime, typeof(DateTime)),
			new DataColumn(LastRunTime, typeof(DateTime)),
			new DataColumn(LastErrorTime, typeof(DateTime)),
			new DataColumn(ErrorCountLast24Hours, typeof(int)),
		};

		internal static List<DataColumn> MetadataCustomFilterCollection => new List<DataColumn> {
			new DataColumn(MutuallyExclusiveGroup, typeof(string)),
			new DataColumn(Description, typeof(string)),
			new DataColumn(Category, typeof(string)),
		};
	}
}
