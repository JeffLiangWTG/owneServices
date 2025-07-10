using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public sealed partial class SystemDataRegistry : RegistryItemSet, ISystemDataRegistry
	{
		#region BI = Business Intelligence

		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem BiAuditAPI
		{
			get
			{
				return GetItem("BiAuditAPI", delegate
				{
					return new BooleanRegistryItem(
					"BiAuditAPI",
					Categories.System_BusinessIntelligence,
					(NoResString)"Audit API",
					(NoResString)"Enable Audit Web API",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
					true);
				});
			}
		}

		public BooleanRegistryItem BiReportAPI
		{
			get
			{
				return GetItem("BiReportAPI", delegate
				{
					return new BooleanRegistryItem(
					"BiReportAPI",
					Categories.System_BusinessIntelligence,
					(NoResString)"Report API",
					(NoResString)"Enable Report Web API",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
					false);
				});
			}
		}

		public BooleanRegistryItem BiODataAPI
		{
			get
			{
				return GetItem<BooleanRegistryItem>("BiODataAPI", delegate
				{
					return new BooleanRegistryItem(
					"BiODataAPI",
					Categories.System_BusinessIntelligence,
					(NoResString)"BI OData API",
					(NoResString)"Enable BI OData API",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
					false);
				});
			}
		}

		public BooleanRegistryItem SupportLegacyReportServer
		{
			get
			{
				return GetItem("SupportLegacyReportServer", delegate
				{
					return new BooleanRegistryItem(
						"SupportLegacyReportServer",
						Categories.System_BusinessIntelligence,
						(NoResString)"Support Legacy Report Server",
						(NoResString)"Enable Support Legacy Report Server",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public StringRegistryItem BiAuditServer
		{
			get
			{
				return GetItem("BiAuditServer", delegate
				{
					return new StringRegistryItem(
					"BiAuditServer",
					Categories.System_BusinessIntelligence,
					ResString.GetMultilingualString("e5d84114-446d-444b-a423-34a6c76c6320", "Audit Server"),
					ResString.GetMultilingualString("bb7db7ba-4726-4c9f-b294-a1f64178a4f5", "Name of Audit Server"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
					string.Empty);
				});
			}
		}

		public BooleanRegistryItem BiEnableAuditAccess
		{
			get
			{
				return GetItem("BiEnableAuditAccess", delegate
				{
					return new BooleanRegistryItem(
						"BiEnableAuditAccess",
						Categories.System_BusinessIntelligence,
						ResString.GetMultilingualString("3C46B919-0C34-4F21-A2C0-FA9D856381FE", "Allow Enterprise DB Reader access to the Audit DB"),
						ResString.GetMultilingualString("CD0017AB-B32F-4DC5-AC70-B7886F8BE740", "Controls whether or not Enterprise Reader exists on the Audit DB."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		public StringRegistryItem BiDataWarehouseServer
		{
			get
			{
				return GetItem<StringRegistryItem>("BiDataWarehouseServer", delegate
				{
					var result = new StringRegistryItem(
					"BiDataWarehouseServer",
					Categories.System_BusinessIntelligence,
					(NoResString)"Data Warehouse Server",
					(NoResString)"Name of Data Warehouse Server",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
					string.Empty);
					result.DataType = new DataWarehouseServerDataType();
					return result;
				});
			}
		}

		public StringRegistryItem BiAnalysisServer
		{
			get
			{
				return GetItem<StringRegistryItem>("BiAnalysisServer", delegate
				{
					var result = new StringRegistryItem(
					"BiAnalysisServer",
					Categories.System_BusinessIntelligence,
					(NoResString)"Analysis Services Server",
					(NoResString)"Name of Analysis Services Server",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
					string.Empty);
					result.DataType = new AnalysisServerDataType();
					return result;
				});
			}
		}

		public SupportEdwDataSourceReportRegistryItem ListOfSupportedReportsUsingEdwAsDataSource
		{
			get
			{
				return GetItem<SupportEdwDataSourceReportRegistryItem>("ListOfSupportedReportsUsingEdwAsDataSource", delegate
				{
					var result = new SupportEdwDataSourceReportRegistryItem(
					"ListOfSupportedReportsUsingEdwAsDataSource",
					Categories.System_BusinessIntelligence,
					(NoResString)"List Of Supported Reports Using EDW As Data Source",
					(NoResString)"Below is the list of reports that support the use of EDW as a data source.",
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
					SupportEdwDataSourceReportCollection.GetDefault());
					return result;
				});
			}
		}

		public StringRegistryItem BiPowerBiWebPortalUrl
		{
			get
			{
				return GetItem<StringRegistryItem>("BiPowerBiWebPortalUrl", delegate
				{
					var result = new StringRegistryItem(
					"BiPowerBiWebPortalUrl",
					Categories.System_BusinessIntelligence,
					(NoResString)"Power BI Web Portal URL",
					(NoResString)"Web Portal URL of Power BI Server",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
					string.Empty);
					result.DataType = new PowerBiWebPortalUrlDataType();
					return result;
				});
			}
		}

		public IntRegistryItem AuditRetentionPeriod
		{
			get
			{
				return GetItem<IntRegistryItem>("AuditRetentionPeriod", delegate
				{
					return new IntRegistryItem(
						"AuditRetentionPeriod",
						Categories.System_BusinessIntelligence,
						ResString.GetMultilingualString("9d44Bc08-e90a-4d0d-a356-e5924df1bcd9", "Audit Retention Period"),
						ResString.GetMultilingualString("754f4d45-16cf-4bda-aa10-55d93330ca7a", "Number of months audit data should be kept"),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
						12,
						1,
						24
					);
				});
			}
		}

		public BiReportCredentialRegistryItem BiReportUserCredential
		{
			get
			{
				return GetItem<BiReportCredentialRegistryItem>("BiReportUserCredential", delegate
				{
					return new BiReportCredentialRegistryItem(
						"BiReportUserCredential",
						Categories.System_BusinessIntelligence,
						(NoResString)"Report User Credentials",
						(NoResString)"Credentials to run business intelligence reports",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
						new BiReportCredential()
					);
				});
			}
		}

		public BooleanRegistryItem EnableBiReport
		{
			get
			{
				return GetItem("EnableBiReport", delegate
				{
					return new BooleanRegistryItem(
						"EnableBiReport",
						Categories.System_BusinessIntelligence,
						(NoResString)"Enable Bi Report",
						(NoResString)"Enable Bi Report",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public ImageRegistryItem BiReportCompanyLogo
		{
			get
			{
				return GetItem<ImageRegistryItem>("BiReportCompanyLogo", delegate
				{
					return new ImageRegistryItem(
						"BiReportCompanyLogo",
						Categories.System_BusinessIntelligence_CompanyBranding,
						ResString.GetMultilingualString("99781924-6EC4-4341-AAE1-993F75A59264", "Logo"),
						ResString.GetMultilingualString("088B4FAD-2D2F-4343-9907-5B6EFD3044F8", "A company logo that appears in BI Reports"),
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue
						);
				});
			}
		}

		#region CDC

		public BooleanRegistryItem BiResetChangeDataCapture
		{
			get
			{
				return GetItem("BiResetChangeDataCapture", delegate
				{
					return new BooleanRegistryItem(
					"BiResetChangeDataCapture",
					Categories.System_BusinessIntelligence_Cdc,
					(NoResString)"Reset CDC",
					(NoResString)"This will disable then re-enable Change Data Capture for the database during the next database upgrade. It will also DROP and CREATE the Audit Database.",
					RegistryStorageFlags.System,
					RegistryOptions.NotCached | RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
					false);
				});
			}
		}

		public BooleanRegistryItem BiDisableChangeDataCapture
		{
			get
			{
				return GetItem("BiDisableChangeDataCapture", delegate
				{
					return new BooleanRegistryItem(
					"BiDisableChangeDataCapture",
					Categories.System_BusinessIntelligence_Cdc,
					(NoResString)"Disable CDC",
					(NoResString)"This will disable Change Data Capture for all CDC tables during the next CDN. The database will remain CDC enabled. If GLOW is enabled, then it will enable Change Tracking",
					RegistryStorageFlags.System,
					RegistryOptions.NotCached | RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
					false);
				});
			}
		}

		public IntRegistryItem CdcMaxTransactions
		{
			get
			{
				return GetItem("CdcMaxTransactions", delegate
				{
					return new IntRegistryItem(
						"CdcMaxTransactions",
						Categories.System_BusinessIntelligence_Cdc,
						ResString.GetMultilingualString("037CA29A-19B3-498C-B4BB-63CBA7E070AD", "CDC Maximum Transactions"),
						ResString.GetMultilingualString("BA6C28AD-818C-4452-AE9D-6734C7150944", "Number of maximum transactions to be scanned by CDC service task"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
						100,
						100,
						5000
					);
				});
			}
		}

		public IntRegistryItem CdcMaxScans
		{
			get
			{
				return GetItem("CdcMaxScans", delegate
				{
					return new IntRegistryItem(
						"CdcMaxScans",
						Categories.System_BusinessIntelligence_Cdc,
						ResString.GetMultilingualString("E71DDCDE-0F2C-4379-8400-6634F2354D61", "CDC Maximum Scans"),
						ResString.GetMultilingualString("0CD714AB-97A8-4A98-9514-24715F1AD823", "Number of maximum scans to be executed by CDC service task"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
						5,
						5,
						20
					);
				});
			}
		}

		public IntRegistryItem CdcLatencyProviderAcceptableBacklog
		{
			get
			{
				return GetItem<IntRegistryItem>("CdcLatencyProviderAcceptableBacklog", delegate
				{
					return new IntRegistryItem(
						"CdcLatencyProviderAcceptableBacklog",
						Categories.System_BusinessIntelligence_Cdc,
						ResString.GetMultilingualString("62f2a075-e604-4554-85c7-74f4a22b3f6c", "CDC Acceptable Backlog Threshold"),
						ResString.GetMultilingualString("e82320e5-3505-4520-8a70-d53f744b1ea5", "Specifies the number of pending transactions to use as a threshold for throttling low priority services. When a CDC backlog greater than or equal to the specified number of transactions exists, low priority services will be throttled until the backlog decreases below this value."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
						3000,
						3000,
						100000
					);
				});
			}
		}

		#endregion

		#endregion

		#endregion
	}
}
