using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Enterprise.DbUpgrader.Transformation.Common;

public static class ModuleFilterTransformationHelper
{
	public static bool UpdateFilterParameter(XDocument filterData, XDocument filterParameters, string targetFilterDescription, XElement parameterToAdd)
	{
		var updated = false;

		var filterStrips = filterData.XPathSelectElements("//FilterStrip").ToList();
		var filterParams = filterParameters.XPathSelectElements("//ModuleFilter").ToList();

		var index = 0;

		foreach (var filter in filterStrips)
		{
			var filterDescription = filter.Element("FilterDescription")?.Value;

			if (filterDescription == targetFilterDescription)
			{
				var toModify = filterParams.ElementAtOrDefault(index);

				if (toModify != null && toModify.Elements().All(x => x.Name != parameterToAdd.Name))
				{
					toModify.Add(parameterToAdd);
					updated = true;
				}
			}

			index++;
		}
		return updated;
	}

	public static string GetFilterDataByModuleAndDescriptionQuery(string filterDescription, string module) => @$"
SELECT
	S0_PK,
	S0_FilterDataValues AS ModuleData,
	S9_FilterData AS FilterData
FROM
	dbo.StmModuleFilter
JOIN
	dbo.StmModuleFilterUserData ON S9_PK = S0_S9
WHERE
	S9_ModuleID IN ('{module}', '{module}_CT')
AND
	dbo.CLRUncompressAsString(S9_FilterData) LIKE '%>{filterDescription}<%'
";
}
