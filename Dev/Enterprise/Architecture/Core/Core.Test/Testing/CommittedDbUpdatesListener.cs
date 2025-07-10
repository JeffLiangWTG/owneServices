using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.Data;
using Enterprise.Semaphores.Common;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class CommittedDbUpdatesListener : BaseTestListener
	{
		public static readonly CommittedDbUpdatesListener Instance = new();

		public override void BeforeEachTest(DateTime startTime)
		{
			DbCommitTracker.Reset();
			base.BeforeEachTest(startTime);
		}

		public override void AfterEachTest(DateTime endTime)
		{
			try
			{
				var filteredCommittedUpdates = Filter(DbCommitTracker.CommittedUpdates).ToList();
				if (filteredCommittedUpdates.Count == 0)
				{
					return;
				}

				var testMethod = GetCurrentTestCaseRunMethodSafe(currentTestCase);
				if (!IsExemptedFromSynonymDbCommitValidation(testMethod))
				{
					Assertion.AssertContainsExactElementsInAnyOrder(@"One or more updates were not reverted on the database server.
RECOMMENDED: Use TransactionedTestCase as the standard solution.
ONLY use [UseSnapshotProtection] attribute for specific scenarios requiring multiple database connections.
AVOID use [UseSnapshotProtection] with shared databases like [CW-RefDatabase].",
						[],
						filteredCommittedUpdates);
				}
			}
			finally
			{
				currentTestCase = null;
			}

			base.AfterEachTest(endTime);
		}

		public override void EndTest(TestCase test, DateTime endTime)
		{
			currentTestCase = test;
			base.EndTest(test, endTime);
		}

		TestCase currentTestCase;

		static string NormaliseString(string input)
		{
			var output = RegexRemoveDbPrefix.Replace(input, "");
			return RegexRemoveWhitespace.Replace(output, "");
		}

		IEnumerable<string> Filter(IEnumerable<string> commands)
		{
			return commands.Where(command =>
			{
				var formattedCommand = NormaliseString(command);

				return
					!MatchesSql(formattedCommand, NormKeepHeartbeatAliveProcedure)
					&& !MatchesSql(formattedCommand, NormUpdateUserContextScript)
					&& !MatchesSql(formattedCommand, NormCleanUpOldHeartbeatScript)
					&& !MatchesSql(formattedCommand, NormCleanUpSameUserOldHeartbeatScript)
					&& !MatchesSql(formattedCommand, NormDeleteHeartbeatScript)
					&& !MatchesSql(formattedCommand, NormDeleteSemaphoreHandleScript)
					&& !MatchesSql(formattedCommand, NormCreateHeartbeatScript)
					&& !MatchesSql(formattedCommand, NormCreateSemaphoreHandleScript)
					&& !formattedCommand.StartsWith(NormFountainDeleteStmNums)
					&& !MatchesSql(formattedCommand, NormCleanUpSameUserOldHeartbeatAndUpdateUserContext)
					&& !MatchesSql(formattedCommand, NormCleanUpSameUserOldHeartbeatAndCleanUpOldHeartbeat);
			});
		}

		static bool MatchesSql(string formattedCommand, string sql)
		{
			return formattedCommand == sql || formattedCommand.StartsWith(sql + "Parameters:[");
		}

		static readonly Regex RegexRemoveDbPrefix = new(@"^Database:\s\w+", RegexOptions.Compiled);
		static readonly Regex RegexRemoveWhitespace = new(@"[\n\t\r ]", RegexOptions.Compiled);

		static readonly string NormKeepHeartbeatAliveProcedure = NormaliseString(SemaphoreDbManager.KeepHeartbeatAliveProcedure);
		static readonly string NormUpdateUserContextScript = NormaliseString(SemaphoreDbManager.UpdateUserContextScript);
		static readonly string NormCleanUpOldHeartbeatScript = NormaliseString(SemaphoreDbManager.CleanUpOldHeartbeatScript);
		static readonly string NormCleanUpSameUserOldHeartbeatScript = NormaliseString(SemaphoreDbManager.CleanUpSameUserOldHeartbeatScript);
		static readonly string NormDeleteHeartbeatScript = NormaliseString(SemaphoreDbManager.DeleteHeartbeatScript);
		static readonly string NormDeleteSemaphoreHandleScript = NormaliseString(SemaphoreDbManager.DeleteSemaphoreHandleScript);
		static readonly string NormCreateHeartbeatScript = NormaliseString(SemaphoreDbManager.CreateHeartbeatScript);
		static readonly string NormCreateSemaphoreHandleScript = NormaliseString(SemaphoreDbManager.CreateSemaphoreHandleScript);
		static readonly string NormFountainDeleteStmNums = NormaliseString(FountainTestListener.DeleteStmNumsCreatedInTestScript);
		static readonly string NormCleanUpSameUserOldHeartbeatAndUpdateUserContext = NormaliseString(SemaphoreDbManager.CleanUpSameUserOldHeartbeatScript + SemaphoreDbManager.UpdateUserContextScript);
		static readonly string NormCleanUpSameUserOldHeartbeatAndCleanUpOldHeartbeat = NormaliseString(SemaphoreDbManager.CleanUpSameUserOldHeartbeatScript + SemaphoreDbManager.CleanUpOldHeartbeatScript);

		#region ExemptedFromSynonymDbCommitValidation

		bool IsExemptedFromSynonymDbCommitValidation(MethodInfo methodInfo)
		{
			if (methodInfo is null)
			{
				return false;
			}

			var type = methodInfo.DeclaringType;
			if (type is null)
			{
				return false;
			}

			var typeFullName = $"{type.Namespace}.{type.Name}";
			var fullMethodName = $"{typeFullName}.{methodInfo.Name}";

			var baselineExceptions = dbCommitTrackerExemptionList.Value;
			return baselineExceptions.Contains(typeFullName, StringComparer.OrdinalIgnoreCase)
				|| baselineExceptions.Contains(fullMethodName, StringComparer.OrdinalIgnoreCase);
		}

		static MethodInfo GetCurrentTestCaseRunMethodSafe(TestCase currentTestCase)
		{
			try
			{
				return currentTestCase?.RunMethod;
			}
			catch
			{
				return null;
			}
		}

		static readonly Lazy<HashSet<string>> dbCommitTrackerExemptionList = new(InitializeDbCommitExemptionList, isThreadSafe: true);

		static HashSet<string> InitializeDbCommitExemptionList()
		{
			var assembly = Assembly.GetExecutingAssembly();
			const string resourceName = "Enterprise.ZArchitecture.Core.Test.Testing.DbCommitTrackerExemptionBaseline.txt";

			using var stream = assembly.GetManifestResourceStream(resourceName);
			if (stream is null)
			{
				return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			}

			using var reader = new StreamReader(stream);

			var synonymDbCommitIgnoreList = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			string line;
			while ((line = reader.ReadLine()) != null)
			{
				if (!string.IsNullOrWhiteSpace(line))
				{
					synonymDbCommitIgnoreList.Add(line.Trim());
				}
			}

			return synonymDbCommitIgnoreList;
		}

		#endregion
	}
}
