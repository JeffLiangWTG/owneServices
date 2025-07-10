using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using WTG.Data.SqlTests.Utils;

namespace CargoWise.Data.Test
{
	public class DbObjectsTest : TestCase
	{
		[FrequentlyFailing]
		public void TestAllViewsAreAssignedASystem()
		{
			Db.Connection.EnsureIsOpen();
			var allViews = SqlTestsUtils.RunStringSetCommand(((IDbConnectionInternals)Db.Connection).InternalDbConnection, "SELECT name FROM sys.views");
			var unmapped = SqlTestsUtils.TryGetObjectsWithNoSystem(allViews).GetAwaiter().GetResult();
			if (unmapped == null)
			{
				Assert(true);
				return;
			}

			var baseline = GetBaseLine();
			var dump = baseline.Except(unmapped, StringComparer.OrdinalIgnoreCase).Any();

			try
			{
				AssertContainsExactElementsInExactOrder("Add your view mapping at this site : http://wiselake-systemusage-web.wisecloud.zone/WebCPU - see Mappings tab"
					, Array.Empty<string>()
					, unmapped.Except(baseline, StringComparer.OrdinalIgnoreCase));
			}
			catch (AssertionFailedError)
			{
				dump = true;
				throw;
			}
			finally
			{
				DumpToLastRunFolder(unmapped, dump);
			}
		}

		string[] GetBaseLine()
		{
			var type = GetType();
			using (var stream = type.Assembly.GetManifestResourceStream(type, $"BaseLine.{GetBaseLineFileName()}"))
			{
				return SqlTestsUtils.ReadBaseLine(stream);
			}
		}

		string GetBaseLineFileName()
		{
			var method = RunMethod.Name;
			if (method.StartsWith("Test", StringComparison.OrdinalIgnoreCase))
			{
				method = method.Substring("Test".Length);
			}

			return $"{method}.txt";
		}

		void DumpToLastRunFolder(IEnumerable<string> linesToDump, bool dump)
		{
			if (!Directory.Exists(LastRunFolder))
			{
				return;
			}

			var tempFileName = Path.Combine(LastRunFolder, GetBaseLineFileName());
			if (dump)
			{
				File.WriteAllLines(tempFileName, linesToDump.OrderBy(x => x));
			}
			else
			{
				File.Delete(tempFileName);
			}
		}

#pragma warning disable CW1097 // Hard-coded Temp Path for consistency with WTG.Data.SqlTests
		const string LastRunFolder = @"C:\temp\lastrun";
#pragma warning restore CW1097
	}
}
