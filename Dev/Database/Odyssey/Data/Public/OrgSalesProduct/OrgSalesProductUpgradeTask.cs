using System.Collections.Generic;
using System.Data;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(OrgSalesProductSchema))]

namespace Enterprise.DbUpgrader.Data
{
	public class OrgSalesProductUpgradeTask : EmbeddedUpgradeTask
	{
		public OrgSalesProductUpgradeTask()
			: base(new OrgSalesProductDataFile())
		{
		}

		#region Overrides

		protected override void UpdateColumn(string columnName, DataRow targetRow, DataRow sourceRow)
		{
			if (targetRowsWithTransformationPlaceholder.Contains(targetRow))
			{
				base.UpdateColumn(columnName, targetRow, sourceRow);
			}
			else if ((string)targetRow[OrgSalesProductSchema.Constants.MP_FormLayoutData] == TransformationPlaceholderString)
			{
				targetRowsWithTransformationPlaceholder.Add(targetRow);
				base.UpdateColumn(columnName, targetRow, sourceRow);
			}
			else
			{
				var columnsToNotUpdate = new HashSet<string>(
					new[]
					{
						OrgSalesProductSchema.Constants.MP_Name,
						OrgSalesProductSchema.Constants.MP_FormLayoutData
					}
				);

				if (!columnsToNotUpdate.Contains(columnName))
				{
					base.UpdateColumn(columnName, targetRow, sourceRow);
				}
			}
		}

		readonly HashSet<DataRow> targetRowsWithTransformationPlaceholder = new HashSet<DataRow>();

		#endregion

		public const string TransformationPlaceholderString = "<?transformation placeholder?>";
	}
}
