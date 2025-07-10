using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal abstract class CustomLabel : ValueProvider
	{
		protected override object GetReplacementCore(string macro, Report report)
		{
			var match = Regex.Match(macro);
			var customLabelFieldName = match.Groups[1].ToString().Trim();
			var defaultCaption = match.Groups[2].ToString().Trim();

			var result = defaultCaption;
			var customLabel = GetCustomLabel(report, customLabelFieldName);
			if (customLabel != null && !customLabel.OT_UseDefaultCaption)
			{
				result = customLabel.OT_Caption;
			}
			return result;
		}

		protected abstract string GetCustomLabelsType();
		protected abstract OrgHeader GetConfigOrganisation(Report report);

		#region Implementation

		protected OrgCustomLabels GetCustomLabel(Report report, string customLabelFieldName)
		{
			var configOrg = GetConfigOrganisation(report);
			var result = GetCustomLabel(customLabelFieldName, configOrg);
			return result;
		}

		protected OrgCustomLabels GetCustomLabel(string customLabelFieldName, OrgHeader configOrg)
		{
			OrgCustomLabels result = null;
			if (configOrg != null)
			{
				result = new CustomLabelProvider(configOrg.CustomLabels).GetLabelFallbackToCompanyOrgProxy(GlbCompany.CurrentCompany, customLabelFieldName, GetCustomLabelsType());
			}
			return result;
		}

		protected OrgHeader GetDocumentConfigOrganisation(Report report)
		{
			OrgHeader result = null;
			if (report.BODocDataProvider != null)
			{
				ICustomLabelsConfigOrgProvider configOrgProvider = report.BODocDataProvider.ParentBusinessObject as ICustomLabelsConfigOrgProvider;
				if (configOrgProvider != null)
				{
					result = configOrgProvider.ConfigOrg;
				}
			}
			return result;
		}

		protected OrgHeader GetReportConfigOrganisation(Report report)
		{
			return GlbCompany.CurrentCompany.OrgProxy;
		}

		#endregion
	}
}
