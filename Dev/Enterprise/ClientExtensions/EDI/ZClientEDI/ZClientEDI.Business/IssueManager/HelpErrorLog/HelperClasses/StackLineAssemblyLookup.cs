using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Enterprise.Client.EDI.IncidentManager.Business;
using static Enterprise.Client.EDI.IssueManager.Business.StackLineAssemblyLookupHelper;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	public static class StackLineAssemblyLookup
	{
		public static void UpdateMissingAssemblyInformation(IEnumerable<IStackLine> stackLines)
		{
			var callsWithMissingAssembly = stackLines
				.Where(c => IsWeightCalculableStackLine(c) && string.IsNullOrEmpty(c.Assembly))
				.ToArray();

			if (callsWithMissingAssembly.Length == 0)
			{
				return;
			}

			var assemblies = GetAssemblies(callsWithMissingAssembly);

			for (var i = 0; i < callsWithMissingAssembly.Length; i++)
			{
				var callWithMissingAssembly = callsWithMissingAssembly[i];
				var (stackLine, _) = assemblies.FirstOrDefault(a =>
				{
					var fullMethodName = GetFullMethodNameWithoutParameters(callWithMissingAssembly.FullStackLine);
					var (className, methodName) = ParseFullMethodName(fullMethodName);
					return a.method == TruncateType(className) + "." + TruncateMethod(methodName);
				});

				if (stackLine != null)
				{
					callWithMissingAssembly.Assembly = stackLine.Assembly;
					callWithMissingAssembly.Type = stackLine.Type;
					callWithMissingAssembly.Method = stackLine.Method;
				}
			}
		}

		static List<(StackLine stackLine, string method)> GetAssemblies(IEnumerable<IStackLine> stackLines)
		{
			const string sql = @"
;WITH cte AS
(
	SELECT PC_Assembly, PC_ClassName, PM_MethodName, RANK() OVER(PARTITION BY PC_ClassName, PM_MethodName ORDER BY PM_LastSeen DESC) as rn
	FROM PublishedClasses
	JOIN PublishedMethods ON PC_PK = PM_Class
	JOIN @MethodData on ClassName = PC_ClassName AND MethodName = PM_MethodName
)
SELECT
PA_AssemblyName
, PC_ClassName
, PM_MethodName
, CONCAT(PC_ClassName, '.', PM_MethodName) AS FullMethodName
FROM cte
JOIN PublishedAssemblies ON PA_PK = PC_Assembly
WHERE rn = 1
";
			var assemblies = new List<(StackLine stackLine, string method)>();
			using var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection();
			if (connection != null)
			{
#pragma warning disable CW1107 // Requires direct SQL access
				using var command = connection.Command(sql);
#pragma warning restore CW1107

				using var table = new DataTable();
				table.Locale = CultureInfo.InvariantCulture;
				table.Columns.Add("ClassName", typeof(string));
				table.Columns.Add("MethodName", typeof(string));

				foreach (var stackLine in stackLines)
				{
					var className = stackLine.Type;
					var methodName = stackLine.Method;

					if (className != null)
					{
						table.Rows.Add(className, methodName);
					}
					else
					{
						table.Rows.Add(DBNull.Value, methodName);
					}
				}

				command.AddTableValuedParameter("@MethodData", "dbo.MethodData", table);

				using var reader = command.ExecuteReader();
				while (reader.Read())
				{
					var stackLine = new StackLine(reader["PA_AssemblyName"].ToString(), reader["PC_ClassName"].ToString(), reader["PM_MethodName"].ToString(), string.Empty, string.Empty);
					assemblies.Add((stackLine, reader["FullMethodName"].ToString()));
				}
			}

			return assemblies;
		}
	}
}
