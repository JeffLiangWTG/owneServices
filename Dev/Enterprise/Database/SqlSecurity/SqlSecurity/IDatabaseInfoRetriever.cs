using System;
using System.Collections.Generic;
using CargoWise.Data;

namespace Enterprise.SqlSecurity
{
	internal interface IDatabaseInfoRetriever : IDisposable
	{
		IEnumerable<string> AllDatabases { get; }
		AdminConnection DataWarehouseServerConnection { get;  }
		AdminConnection AuditServerConnection { get; }
		bool IsHostedInWiseCloud { get; }
		bool IsDedicatedServerInstance(string serverInstanceName);
		bool IsDatabaseSecurityModeOpen { get; }

		IEnumerable<string> StaffPrefixes { get; }

		string SingleRefDbName { get; }

		IEnumerable<string> AllStaffADLoginNames { get; }
	}
}
