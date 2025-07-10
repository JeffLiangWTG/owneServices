using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal abstract class IsCustomLabelHidden : CustomLabel
	{
		protected override object GetReplacementCore(string macro, Report report)
		{
			var match = Regex.Match(macro);

			var customLabelFieldName = match.Groups[1].ToString().Trim();
			var showByDefaultIfNothingInCustomLabelPrefix = (match.Groups.Count >= 3) && match.Groups[2].ToString().Trim().Equals("Y", StringComparison.OrdinalIgnoreCase);

			var atLeastOneCustomLabelConfiguredWithSamePrefix = false;
			if (showByDefaultIfNothingInCustomLabelPrefix)
			{
				var configOrg = GetConfigOrganisation(report);
				if (configOrg != null)
				{
					atLeastOneCustomLabelConfiguredWithSamePrefix =
						HasCustomLabelWithPrefixWhileFallbackToCompanyOrgProxy(configOrg.CustomLabels, GetCustomLabelPrefix(customLabelFieldName));
				}
			}
			return (ZBool)!((showByDefaultIfNothingInCustomLabelPrefix && !atLeastOneCustomLabelConfiguredWithSamePrefix)
				|| GetCustomLabel(report, customLabelFieldName) != null);
		}

		bool HasCustomLabelWithPrefixWhileFallbackToCompanyOrgProxy(OrgCustomLabelsCollection collection, string labelPrefix)
		{
			var filter = new ZQuery();
			filter.AddToFilter(JoinCondition.And, OrgCustomLabelsSchema.OT_FieldName, SQLComparisonOperator.StartsWith, labelPrefix + ".");
			filter.AddToFilter(JoinCondition.And, OrgCustomLabelsSchema.OT_Type, SQLComparisonOperator.Equal, GetCustomLabelsType());

			var result = collection.Find(filter).Length > 0;
			if (!result)
			{
				var proxy = GlbCompany.CurrentCompany.OrgProxy;
				if (proxy != null)
				{
					result = proxy.CustomLabels.Find(filter).Length > 0;
				}
			}
			return result;
		}

		protected string GetCustomLabelPrefix(string fieldName)
		{
			var index = fieldName.IndexOf(".");
			var result = "";
			if (index != -1)
			{
				result = fieldName.Substring(0, index);
			}
			return result;
		}
	}
}
