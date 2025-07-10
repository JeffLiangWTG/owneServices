using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformation.DataModification
{
	abstract class RenameModuleFiltersTransformation : DataTransformation
	{
		protected internal abstract IEnumerable<string> ModuleIDs { get; }
		protected abstract IDictionary<string, string> FilterRenames { get; }

		protected override void OfflinePostUpgradeTransform()
		{
			var selectModuleFiltersSql = string.Format(CultureInfo.InvariantCulture, @"SELECT S9_PK, S9_FilterData
																								FROM dbo.StmModuleFilter
																								WHERE
																								S9_FilterData IS NOT NULL
																								AND S9_ModuleID in ({0})", string.Join(",", ModuleIDs.Select(id => string.Format(CultureInfo.InvariantCulture, "'{0}'", id))));
			var moduleFilterUpdates = ModuleTransformationHelper.GetFilterUpdates(selectModuleFiltersSql, FilterRenames);
			ModuleTransformationHelper.SaveChanges(moduleFilterUpdates);
		}
	}
}
