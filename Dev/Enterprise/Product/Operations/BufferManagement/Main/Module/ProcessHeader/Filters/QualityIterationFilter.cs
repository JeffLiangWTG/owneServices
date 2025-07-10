using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.BufferManagement.Module
{
	public class QualityIterationFilter : ModuleTextFilter
	{
		public QualityIterationFilter()
			: base(ProcessHeader.ModuleFilterConstants.QualityIteration, QualityIterationQuery, new QualityIterationFilterTypeList())
		{
			MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|QualityIteration", "Quality Iteration");
			Category = FilterCategories.StatusAndFlags;
		}

		static ZQuery QualityIterationQuery(ZString value)
		{
			string sqlText;

			if (value == QualityIterationFilterTypeList.Codes.QualityIteration)
			{
				sqlText = GetFormattedSQL(isNotIn: false);
			}
			else if (value == QualityIterationFilterTypeList.Codes.NonQualityIteration)
			{
				sqlText = GetFormattedSQL(isNotIn: true);
			}
			else
			{
				return new ZQuery();
			}

			return new ZDBOnlyQuery(typeof(ProcessHeader)).AddFilterAndZSQLParameterCollection(sqlText, null);
		}

		static string GetFormattedSQL(bool isNotIn)
		{
			var not = isNotIn ? "NOT" : string.Empty;

			return FormattableString.Invariant($@"
FH_PK {not} IN
(
	SELECT TOP 1
		P9I_FH_IterationWorkflow
	FROM
		dbo.ProcessTaskIterationLink
	WHERE
		P9I_FH_IterationWorkflow = FH_PK AND P9I_LinkType = '{IterationLinkTypeList.Codes.QualityIterationTask}'
)");
		}
	}
}
