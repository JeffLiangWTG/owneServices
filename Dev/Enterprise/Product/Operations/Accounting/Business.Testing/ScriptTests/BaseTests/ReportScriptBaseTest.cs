using System;
using System.Collections.Generic;
using Enterprise.Accounting.Utility.Testing;

namespace Enterprise.Accounting.Business.Testing.ScriptTests.BaseTests
{
	abstract class ReportScriptBaseTest : ScriptTest
	{
		#region for subclass to override

		protected abstract List<string> InsertTableList { get; }

		protected abstract string ScriptName { get; }

		protected virtual string ScriptSchemaName => "dbo";

		protected abstract string ScriptDbName { get; }

		protected virtual IDisposable CreateSnapShotForEdwAndTransformData(Action insertAction)
		{
			insertAction();
			return null;
		}

		#endregion
	}
}
