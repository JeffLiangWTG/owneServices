using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework
{
	public class RestrictedTableBusinessObjectFactory : BusinessObjectFactory
	{
		public RestrictedTableBusinessObjectFactory(string[] allowedTableNamesToLoad)
		{
			Argument.NotNull(allowedTableNamesToLoad, "allowedTableNamesToLoad");
			this.allowedTableNamesToLoad = allowedTableNamesToLoad.Concat(AlwaysAddedtables()).ToArray();
		}

		readonly string[] allowedTableNamesToLoad;

		public string[] AllowedTableNamesToLoad
		{
			get { return allowedTableNamesToLoad; }
		}

		#region BusinessObjectFactory Overrides

		public override BusinessObjectFactory CreateNewFactory(bool usingMyThreadSentry = false)
		{
			return new RestrictedTableBusinessObjectFactory(AllowedTableNamesToLoad);
		}

		protected override BusinessObject[] LoadCore(string tableOrViewName, Type bizOType, ZQuery effectiveFilter)
		{
			EnsureAllowedTableName(tableOrViewName);

			return base.LoadCore(tableOrViewName, bizOType, effectiveFilter);
		}

		protected override DataRow LoadFromPKCore(string tableOrViewName, ZGuid pk)
		{
			EnsureAllowedTableName(tableOrViewName);

			return base.LoadFromPKCore(tableOrViewName, pk);
		}

		public override bool CanLoadFromTable(string tableOrViewName)
		{
			return allowedTableNamesToLoad.Any(s => string.Equals(s, tableOrViewName, StringComparison.OrdinalIgnoreCase));
		}

		#endregion

		#region Implementation

		void EnsureAllowedTableName(string tableOrViewName)
		{
			if (!CanLoadFromTable(tableOrViewName))
			{
				ErrorReporter.ReportOnce(string.Format("Records from table {0} cannot be loaded with this factory", tableOrViewName));
			}
		}

		IEnumerable<string> AlwaysAddedtables()
		{
			yield return ProcessFieldChangeRuleSchema.Constants.TableName;
			yield return ProcessFieldChangeRuleFieldSchema.Constants.TableName;
		}

		#endregion
	}
}
