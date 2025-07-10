using System;
using System.Collections.Generic;
using System.Reflection;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.DataProviders
{
	public sealed class TableProviderFactory
	{
		public TableProviderFactory()
		{
			// Used by GL Reports and overridden for UTI
			TableProviderTypes["GLTransactionList"] = (NoResString)"Enterprise.Accounting.ReportTableProviders.GLAccountDocumentDataProvider, Enterprise.Accounting.ReportTableProviders";
			TableProviderTypes["GLMultilingualTransactionList"] = (NoResString)"Enterprise.Accounting.ReportTableProviders.GLAccountMultiLingualDataProvider, Enterprise.Accounting.ReportTableProviders";

			// Used by Import Invoice Lines Product Audit Report (US)
			TableProviderTypes["USInvoiceLineProductMatches"] = (NoResString)"Enterprise.Customs.US.Business.Reports.InvoiceLineProductMatchTableProvider, Enterprise.Customs.US.Business";

			// Used by New FR Delta G regularization report
			TableProviderTypes["FRDeltaGRegularizationMatches"] = (NoResString)"Enterprise.Customs.FR.Business.Reports.FRDeltaGRegularizationMatchTableProvider, Enterprise.Customs.FR.Business";

			// Used by Similar Organisations report
			TableProviderTypes["SimilarOrganisations"] = (NoResString)"Enterprise.MasterFiles.ReportTableProviders.SimilarOrganisationsTableProvider, Enterprise.MasterFiles.ReportTableProviders";

			// Used by Staff Security report
			TableProviderTypes["StaffSecurity"] = (NoResString)"Enterprise.MasterFiles.ReportTableProviders.StaffSecurityTableProvider, Enterprise.MasterFiles.ReportTableProviders";

			// Used by Group Staff Security report
			TableProviderTypes["GroupStaffSecurity"] = (NoResString)"Enterprise.MasterFiles.ReportTableProviders.GroupStaffSecurityTableProvider, Enterprise.MasterFiles.ReportTableProviders";

			// Used by Listing of DocStrip Section Names report
			TableProviderTypes["DocStripSectionNames"] = (NoResString)"Enterprise.DocumentEngine.ReportTableProviders.DocStripSectionNamesTableProvider, Enterprise.DocumentEngine";

			// Used by Listing of System DocBuilder Document report
			TableProviderTypes["SystemDocBuilderDocuments"] = (NoResString)"Enterprise.MasterFiles.ReportTableProviders.DocBuilderDocumentTableProvider, Enterprise.MasterFiles.ReportTableProviders";
		}

		public bool IsProviderAvailable(string providerName)
		{
			return TableProviderTypes.ContainsKey(providerName);
		}

		public TableProvider GetProvider(string providerName)
		{
			string tableProviderTypeName;
			if (TableProviderTypes.TryGetValue(providerName, out tableProviderTypeName))
			{
				Type tableProviderType = null;

#if DEBUG
				if (Globals.IsTest && tableProviderTypeName.Contains("DocumentEngine") && tableProviderTypeName.Contains("Test"))
				{
					tableProviderType = Assembly.Load("Enterprise.DocumentEngine.Test").GetType(tableProviderTypeName, true);
				}

				else
#endif
				{
					tableProviderType = Type.GetType(tableProviderTypeName, true);
				}

				MethodInfo staticNew = tableProviderType.GetMethod("New", BindingFlags.Public | BindingFlags.Static);
				if (staticNew != null)
				{
					return (TableProvider)staticNew.Invoke(null, null);
				}
				else
				{
					return (TableProvider)Activator.CreateInstance(tableProviderType);
				}
			}
			return null;
		}

		readonly Dictionary<string, string> TableProviderTypes = new Dictionary<string, string>();

#if DEBUG
		internal Dictionary<string, string> TableProviderTypesForTesting
		{
			get { return TableProviderTypes; }
		}
#endif
	}
}
