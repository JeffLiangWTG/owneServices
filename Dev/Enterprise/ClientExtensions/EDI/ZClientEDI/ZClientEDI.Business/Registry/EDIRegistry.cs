using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.Registry.ProductAreaModuleMapping;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Core.Modules;
using Enterprise.CustomerService.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using WTG.DevTools.Definitions;
using ZClientEDI.Business;
using ZClientEDI.Business.Registry;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI
{
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public sealed partial class EDIDataRegistry : RegistryItemSet
	{
		#region Construction

		public EDIDataRegistry()
		{
		}

		#region Instance

		public static EDIDataRegistry Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new EDIDataRegistry();
				}

				return fInstance;
			}
		}

		[ThreadStatic]
		static EDIDataRegistry fInstance;

		#endregion

		public const string Category = "WiseTech Global Client Extensions";

		#endregion

		#region Categories

		static class Categories
		{
			internal static MultilingualString BorderWise_WorkItemSelectionCriteria => CombineCategories((NoResString)BorderWiseSubCategory, (NoResString)"Auto-Created Work Item Selection Criteria");
		}

		#endregion

		public override bool IsForProductivityWise => true;

		#region Logs Request Maximum Zip Size

		public IntRegistryItem LogsRequestMaximumZipSize
		{
			get
			{
				return GetItem("LogsRequestMaximumZipSize", delegate
				{
					return new IntRegistryItem(
						"LogsRequestMaximumZipSize",
						(NoResString)Category,
						(NoResString)"Service Task Logs Request Maximum Zip Size (MB)",
						(NoResString)@"Service Task Logs Request will not attempt to send a zip file larger in size than this amount of megabytes. (0 to disable.)",
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
						100, 0, int.MaxValue);
				});
			}
		}

		#endregion

		#region Company Export

		public ZString CompanyExportDirectoryPath
		{
			get { return new ZString(CompanyExportDirectoryPathItem.GetValueWithoutFallback(Env.CurrentUser.PK, Guid.Empty, Guid.Empty)); }
			set { CompanyExportDirectoryPathItem.SetValue(Env.CurrentUser.PK, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		IRegistryItem CompanyExportDirectoryPathItem
		{
			get
			{
				return GetItem("CompanyExportDirectoryPathItem", delegate
				{
					IRegistryItem result = new StringRegistryItem("CompanyExportDirectoryPathItem",
						(NoResString)Category,
						(NoResString)"CompanyExportDirectoryPathItem",
						null,
						RegistryStorageFlags.Company,
						RegistryOptions.IsHidden);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion

		#region Licence Key

		public ZString LicenceKeyDirectoryPath
		{
			get { return new ZString(LicenceKeyDirectoryPathItem.GetValueWithoutFallback(Env.CurrentUser.PK, Guid.Empty, Guid.Empty)); }
			set { LicenceKeyDirectoryPathItem.SetValue(Env.CurrentUser.PK, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		IRegistryItem LicenceKeyDirectoryPathItem
		{
			get
			{
				return GetItem("LicenceKeyDirectoryPathItem", delegate
				{
					IRegistryItem result = new StringRegistryItem("LicenceKeyDirectoryPathItem",
						(NoResString)Category,
						(NoResString)"LicenceKeyDirectoryPathItem",
						null,
						RegistryStorageFlags.Company,
						RegistryOptions.IsHidden);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		public CodeDescriptionBoolRegistryItem LicenceEditionActiveList
		{
			get
			{
				return GetItem<CodeDescriptionBoolRegistryItem>("LicenceEditionActiveList", delegate
				{
					var defaultValue = new CodeDescriptionBoolDisallowNewCollection(new LicenceAdvStdOthList());
					foreach (CodeDescriptionBool item in defaultValue)
					{
						if (item.Code != LicenceAdvStdOthList.Codes.ConcurrentCountry &&
							item.Code != LicenceAdvStdOthList.Codes.ConcurrentExpress &&
							item.Code != LicenceAdvStdOthList.Codes.ConcurrentRegional &&
							item.Code != LicenceAdvStdOthList.Codes.ConcurrentUniversal &&
							item.Code != LicenceAdvStdOthList.Codes.ConversionToODPL
							)
						{
							item.Bool = true;
						}
					}

					return new CodeDescriptionBoolDisallowNewRegistryItem(
						"LicenceEditionActiveList",
						(NoResString)Category,
						(NoResString)"Licence Editions",
						(NoResString)"Active licence editions",
						RegistryStorageFlags.System,
						new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Active", true, true),
						defaultValue
					);
				});
			}
		}

		#endregion

		#region Enable Triage Engine Module
		public BooleanRegistryItem EnableTriageEngineModule
		{
			get
			{
				return GetItem("EnableTriageEngineModule", delegate
				{
					return new BooleanRegistryItem(
						"EnableTriageEngineModule",
						(NoResString)Category,
						(NoResString)"Enable Triage Engine Module",
						(NoResString)"Set this registry to 'Yes' to enable the Triage Engine Module and disable legacy menu items functionality.",
						RegistryStorageFlags.System,
						Globals.IsDebugMode);
				});
			}
		}

		public CodeDescriptionPairListRegistryItem TriageAssistFormUserControlLayout
		{
			get
			{
				return GetItem("TriageAssistFormUserControlLayout", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						"TriageAssistFormUserControlLayout",
						(NoResString)Category,
						(NoResString)"TriageAssistFormUserControlLayout",
						(NoResString)"TriageAssistFormUserControlLayout",
						200,
						RegistryStorageFlags.Company,
						RegistryOptions.NotLogged | RegistryOptions.IsHidden,
						new ReadOnlyCodeDescriptionPairList());
				});
			}
		}

		#endregion

		#region Enable Feature Control Module
		public BooleanRegistryItem EnableFeatureControlModule
		{
			get
			{
				return GetItem("EnableFeatureControlModule", delegate
				{
					return new BooleanRegistryItem(
						"EnableFeatureControlModule",
						(NoResString)Category,
						(NoResString)"Enable Feature Control Module",
						(NoResString)"When set to Yes, the Feature Control Module is available to user.",
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public CodeDescriptionPairListRegistryItem FeatureControlCodeList
		{
			get
			{
				return GetItem("FeatureControlCodeList", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						"FeatureControlCodeList",
						(NoResString)Category,
						(NoResString)"Feature Control Code List",
						(NoResString)"The list of Feature Control Codes.",
						9,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						new ReadOnlyCodeDescriptionPairList());
				});
			}
		}

		#endregion

		#region System Expiry

		public const string SystemExpiryMessagesSubCategory = Category + "/System Expiry Messages";

		public CodeDescriptionPairListRegistryItem SystemExpiredMessages
		{
			get
			{
				return GetItem("SystemExpiredMessages",
					delegate
					{
						CodeDescriptionPairList defaultList = new CodeDescriptionPairList();
						defaultList.AddPair("", "<Configure pre-defined messages in " + SystemExpiryMessagesSubCategory + ">");

						var result = new CodeDescriptionPairListRegistryItem("SystemExpiredMessages",
							(NoResString)SystemExpiryMessagesSubCategory,
							(NoResString)"Expired",
							(NoResString)"The list of predefined messages when a system has expired. The macro {ExpiryDate} will be replaced with the actual expiry date and time.",
							3,
							RegistryStorageFlags.System,
							false,
							defaultList);

						result.EditorInfo = new CodeDescriptionPairListEditorInfo(false, true,
							CodeDescriptionPairListEditorInfo.CharacterCasing.Upper, CodeDescriptionPairListEditorInfo.CharacterCasing.Normal);

						return result;
					});
			}
		}

		public CodeDescriptionPairListRegistryItem SystemExpiryWithinWeekMessages
		{
			get
			{
				return GetItem("SystemExpiryWithinWeekMessages",
					delegate
					{
						CodeDescriptionPairList defaultList = new CodeDescriptionPairList();
						defaultList.AddPair("", "<Configure pre-defined messages in " + SystemExpiryMessagesSubCategory + ">");

						var result = new CodeDescriptionPairListRegistryItem("SystemExpiryWithinWeekMessages",
							(NoResString)SystemExpiryMessagesSubCategory,
							(NoResString)"Within One Week",
							(NoResString)"The list of predefined messages when a system is less than one week from expirying. The macro {ExpiryDate} will be replaced with the actual expiry date and time.",
							3,
							RegistryStorageFlags.System,
							false,
							defaultList);

						result.EditorInfo = new CodeDescriptionPairListEditorInfo(false, true,
							CodeDescriptionPairListEditorInfo.CharacterCasing.Upper, CodeDescriptionPairListEditorInfo.CharacterCasing.Normal);

						return result;
					});
			}
		}

		public CodeDescriptionPairListRegistryItem SystemExpiryWithinMonthMessages
		{
			get
			{
				return GetItem("SystemExpiryWithinMonthMessages",
					delegate
					{
						CodeDescriptionPairList defaultList = new CodeDescriptionPairList();
						defaultList.AddPair("", "<Configure pre-defined messages in " + SystemExpiryMessagesSubCategory + ">");

						var result = new CodeDescriptionPairListRegistryItem("SystemExpiryWithinMonthMessages",
							(NoResString)SystemExpiryMessagesSubCategory,
							(NoResString)"Within One Month",
							(NoResString)"The list of predefined messages when a system is more than a week and less than a month from expirying. The macro {ExpiryDate} will be replaced with the actual expiry date and time.",
							3,
							RegistryStorageFlags.System,
							false,
							defaultList);

						result.EditorInfo = new CodeDescriptionPairListEditorInfo(false, true,
							CodeDescriptionPairListEditorInfo.CharacterCasing.Upper, CodeDescriptionPairListEditorInfo.CharacterCasing.Normal);

						return result;
					});
			}
		}

		#endregion

		#region Master Data

		public const string MasterDataSubCategory = Category + "/Master Data";

		public SendOrganizationDataToCertCaptureRegistryItem SendOrganizationDataToCertCapture
		{
			get
			{
				return GetItem("SendOrganizationDataToCertCapture", () =>
				{
					return new SendOrganizationDataToCertCaptureRegistryItem(
						"SendOrganizationDataToCertCapture",
						(NoResString)MasterDataSubCategory,
						(NoResString)"Send Organization Data To Cert Capture",
						(NoResString)"Enables sending of the US receivable organization data to Cert Capture when organization data changes.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue: new SendOrganizationDataToCertCapture());
				});
			}
		}

		#endregion

		#region Issue Manager

		public const string IssueManagerSubCategory = Category + "/Issue Manager";

		#region Ignored Exception Stack Line Regexes

		public ExceptionKeyRegexesRegistryItem IgnoredExceptionStackLineRegexes
		{
			get
			{
				return GetItem("IgnoredExceptionStackLineRegexes", delegate
				{
					return new ExceptionKeyRegexesRegistryItem(
					"IgnoredExceptionStackLineRegexes",
					(NoResString)IssueManagerSubCategory,
					(NoResString)"Ignored Exception Stack Line Regexes",
					(NoResString)"Regular Expression to match stack line in issue exception stack trace. If the stack line match any regex, the weight of this stack line will be set to 0 to avoid participating assign candidates calculation.",
					RegistryStorageFlags.System,
					RegistryOptions.Default);
				});
			}
		}

		#endregion

		#region Fallback Work Item Criteria

		public StringRegistryItem FallbackWorkItemCriteriaRegistryItem
		{
			get
			{
				return GetItem("FallbackWorkItemCriteria", () =>
				{
					var sri = new StringRegistryItem("FallbackWorkItemCriteria",
						(NoResString)IssueManagerSubCategory,
						(NoResString)"Fallback Work Item Criteria",
						(NoResString)"The criteria to use when no other criteria are available for creating a Work Item. Must be in the format 'ABC/DEF/HIJ'.",
						new WorkItemCriteriaRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.TextBox),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						string.Empty);
					return sri;
				});
			}
		}

		#endregion

		#region Earliest Exe Date To Process In IssueManager

		public ZDateTime EarliestExeDateToProcessInIssueManager
		{
			get
			{
				DateTime result = EarliestExeDateToProcessInIssueManagerRegistryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				return (result != DateTime.MinValue) ? new ZDateTime(result) : ZDateTime.Empty;
			}
			set
			{
				if (value.IsValid)
				{
					EarliestExeDateToProcessInIssueManagerRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToDateTime());
				}
				else if (value.IsEmpty)
				{
					EarliestExeDateToProcessInIssueManagerRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.MinValue);
				}
				else
				{
					throw new ArgumentException("Cannot set registry item to an invalid value.");
				}
			}
		}

		DateTimeRegistryItem EarliestExeDateToProcessInIssueManagerRegistryItem
		{
			get
			{
				return GetItem("EarliestExeDateToProcessInIssueManager", delegate
				{
					return new DateTimeRegistryItem(
						"EarliestExeDateToProcessInIssueManager",
						(NoResString)IssueManagerSubCategory,
						(NoResString)"Earliest Exe Date To Process In IssueManager",
						(NoResString)"Issues from a version of Enterprise with an Exe Date before this value will not be processed by the Issue Manager batch processor.",
						RegistryStorageFlags.System,
						RegistryOptions.Default);
				});
			}
		}

		#endregion

		#region Issue Manager Processor Service Task - Exception Key Matching Regexes

		public ExceptionKeyRegexesRegistryItem ExceptionKeyMatchingRegexes
		{
			get
			{
				return GetItem("ExceptionKeyMatchingRegexes", delegate
				{
					return new ExceptionKeyRegexesRegistryItem(
					"ExceptionKeyMatchingRegexes",
					(NoResString)IssueManagerSubCategory,
					(NoResString)"Exception Key Matching Regexes",
					(NoResString)"Settings for the Issue Manager Processor Service Task. Issues will be grouped together where their exception keys match one of the regular expressions entered below. If an exception key matches more than one regular expression, it will be deemed to have matched the  one in the list.",
					RegistryStorageFlags.System,
					RegistryOptions.Default);
				});
			}
		}

		#endregion

		#region Issue Manager Processor Service Task - Exception Key Regexes

		public ExceptionKeyRegexesRegistryItem ExceptionKeyRegexes
		{
			get
			{
				return GetItem("ExceptionKeyRegexes", () =>
				{
					return new ExceptionKeyRegexesRegistryItem(
					"ExceptionKeyRegexes",
					(NoResString)IssueManagerSubCategory,
					(NoResString)"Exception Key Regexes",
					(NoResString)"All exception keys matching any of the following regexes will have the matching component stripped. This can be used to remove non-relevant components in a key, such as query parameter names.",
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					new ExceptionKeyRegexCollection
					{
						new ExceptionKeyRegex() { Regex = @"\s*@p[0-9]+,*", Description = "Removes SQL parameter names" }
					});
				});
			}
		}

		#endregion

		#region Issue Manager exception types stacktrace depth

		public ExceptionKeyStacktraceDepthRegistryItem ExceptionKeyStacktraceDepths
		{
			get
			{
				return GetItem("ExceptionKeyStacktraceDepths", () =>
				{
					return new ExceptionKeyStacktraceDepthRegistryItem(
						"ExceptionKeyStacktraceDepths",
						(NoResString)IssueManagerSubCategory,
						(NoResString)"Exception Key Stacktrace Depths",
						(NoResString)"When issue occurrence keys are generated for the exception types listed below, only the specified number of stack frames from the source will be included.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						new ExceptionKeyStacktraceDepthCollection
						{
							new ExceptionKeyStacktraceDepth { ExceptionType = "System.NullReferenceException", StackDepth = 2 },
							new ExceptionKeyStacktraceDepth { ExceptionType = "System.ArgumentException", StackDepth = 5 },
							new ExceptionKeyStacktraceDepth { ExceptionType = "System.ArgumentNullException", StackDepth = 5 }
						});
				});
			}
		}

		#endregion

		#region Stacktrace Line Extractor Regexes

		public ExceptionKeyRegexesRegistryItem ErrorLogStackLineExtractorRegexes
		{
			get
			{
				return GetItem("ErrorLogStackLineExtractorRegexes", () =>
				{
					return new ExceptionKeyRegexesRegistryItem(
						"ErrorLogStackLineExtractorRegexes",
						(NoResString)IssueManagerSubCategory,
						(NoResString)"Stacktrace Line Extractor Regexes",
						(NoResString)"Regular expressions to parse stacktrace lines for different languages.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						new ExceptionKeyRegexCollection
						{
							new ExceptionKeyRegex() { Regex = @"^(at|위치:)\s*(?<line>.*\(.*\)).*$", Description = "Recognises and removes the beginning of the stack trace line, for example '  at: System.IO.__Error.WinIOError(Int32 errorCode, String maybeFullPath)'" }
						});
				});
			}
		}

		#endregion

		#region Error Reporting Service URIs

		public StringArrayRegistryItem ErrorReportingServiceURIs
		{
			get
			{
				return GetItem("ErrorReportingServiceURIs", () =>
				{
					return new StringArrayRegistryItem(
						"ErrorReportingServiceURIs",
						(NoResString)IssueManagerSubCategory,
						(NoResString)"Error Reporting Service URIs",
						(NoResString)"URIs of any available Error Reporting Services to download error reports from.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						Array.Empty<string>());
				});
			}
		}

		#endregion

		#region Error Reporting Service Max Results

		public IntRegistryItem ErrorReportingServiceMaxResults
		{
			get
			{
				return GetItem("ErrorReportingServiceMaxResults", () =>
					new IntRegistryItem(
						"ErrorReportingServiceMaxResults",
						(NoResString)IssueManagerSubCategory,
						(NoResString)"Error Reporting Service Max Results",
						(NoResString)"This is the maximum number of error reports that will be retrieved at a time.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: 1000)
					);
			}
		}

		#endregion

		#region Error Reporting Service Access Token

		public StringRegistryItem ErrorReportingServiceAccessToken
		{
			get
			{
				return GetItem("ErrorReportingServiceAccessToken", () =>
				{
					return new StringRegistryItem(
						"ErrorReportingServiceAccessToken",
						(NoResString)IssueManagerSubCategory,
						(NoResString)"Error Reporting Service Access Token",
						(NoResString)"Access Token to authenticate with Error Reporting service(s).",
						new StringRegistryDataType(CharacterCase.Normal, minLength: 1, maxLength: 128),
						new TextRegistryEditorInfo(TextEditorType.Password),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: string.Empty);
				});
			}
		}

		#endregion

		#region Error Reporting Testing

		public BooleanRegistryItem ErrorReportingServiceTesting
		{
			get
			{
				return GetItem("ErrorReportingServiceTesting", () =>
				{
					return new BooleanRegistryItem(
						"ErrorReportingServiceTesting",
						(NoResString)IssueManagerSubCategory,
						(NoResString)"Error Reporting Service Testing",
						(NoResString)"Enables use of the Error Reporting service from the Issue Manager Processor service task in test environments. Do NOT enable unless the system is correctly configured.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: false);
				});
			}
		}

		#endregion

		#region Issue Work Item Creation

		public BooleanRegistryItem UseTestRigOriginInIssueWorkItemCreation
		{
			get
			{
				return GetItem("UseTestRigOriginInIssueWorkItemCreation", () =>
				{
					return new BooleanRegistryItem(
						"UseTestRigOriginInIssueWorkItemCreation",
						(NoResString)IssueManagerSubCategory,
						(NoResString)"Test Rig Issues",
						(NoResString)"When enabled, test rig origin will be considered while creating issue work items. If all the occurrences are from a test rig, the issue will be attached to the work item that created the test rig rather than creating a new work item.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: false);
				});
			}
		}

		public IssueWorkItemCreationThresholdRegistryItem IssueWorkItemCreationThresholdNonClientVisible
		{
			get
			{
				return GetItem("IssueWorkItemCreationThresholdNonClientVisible",
					() => new IssueWorkItemCreationThresholdRegistryItem("IssueWorkItemCreationThresholdNonClientVisible",
						(NoResString)IssueManagerSubCategory,
						(NoResString)"Issue Work Item Creation Threshold (Non-Client Visible)",
						(NoResString)"Non-Client Visible Issues with occurrences surpassing the threshold within the timespan will have a Work Item created for them.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						new IssueWorkItemCreationThresholdCollection
						{
							new IssueWorkItemCreationThreshold { IssueOccurrenceThreshold = GetDefaultIssueOccurrenceThreshold(), ThresholdTimespan = 7 },
						}));
			}
		}

		public IssueWorkItemCreationThresholdRegistryItem IssueWorkItemCreationThresholdClientVisible
		{
			get
			{
				return GetItem("IssueWorkItemCreationThresholdClientVisible",
					() => new IssueWorkItemCreationThresholdRegistryItem("IssueWorkItemCreationThresholdClientVisible",
						(NoResString)IssueManagerSubCategory,
						(NoResString)"Issue Work Item Creation Threshold (Client Visible)",
						(NoResString)"Client Visible Issues with occurrences surpassing the threshold within the timespan will have a Work Item created for them.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						new IssueWorkItemCreationThresholdCollection
						{
							new IssueWorkItemCreationThreshold { IssueOccurrenceThreshold = GetDefaultIssueOccurrenceThreshold(), ThresholdTimespan = 14 },
						}));
			}
		}

		static int GetDefaultIssueOccurrenceThreshold()
		{
			var result = 20;
			var daysSinceApril1 = (ZDate.Today - new ZDate(2018, 4, 1)).TotalDays + 1;
			if (daysSinceApril1 > 0)
			{
				result = 23 - (int)(0.5 + Math.Sqrt((2 * daysSinceApril1) + (169 / 4)));
				if (result < 1)
				{
					result = 1;
				}
			}
			return result;
		}

		#endregion

		#region Issue Manager - Machine Learning Team Assignment

		public BooleanRegistryItem EnableMachineLearningTeamAssignment
		{
			get
			{
				return GetItem("EnableMachineLearningTeamAssignment", () => new BooleanRegistryItem("EnableMachineLearningTeamAssignment",
					(NoResString)IssueManagerSubCategory,
					(NoResString)"Enable Machine Learning Team Assignment",
					(NoResString)"By ticking this to enable Machine Learning Team Assignment function.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false));
			}
		}

		public StringRegistryItem MachineLearningTeamAssignmentApiUrl
		{
			get
			{
				return GetItem("MachineLearningTeamAssignmentApiUrl", delegate
				{
					var @default = ""; //set by URL for production later.
#if DEBUG
					@default = ""; //set by URL for test later.
#endif

					return new StringRegistryItem("MachineLearningTeamAssignmentApiUrl",
						(NoResString)IssueManagerSubCategory,
						(NoResString)"Machine Learning Team Assignment API URL",
						(NoResString)"The URL is used to connect to a Machine Learning Team Assignment WebAPI Service.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						@default);
				});
			}
		}

		public IntRegistryItem MachineLearningTeamAssignmentApiRequestTimeout
		{
			get
			{
				return GetItem("MachineLearningTeamAssignmentApiRequestTimeout", delegate
				{
					var @default = 5;
#if DEBUG
					@default = 5;
#endif

					return new IntRegistryItem("MachineLearningTeamAssignmentApiRequestTimeout",
						(NoResString)IssueManagerSubCategory,
						(NoResString)"Machine Learning Team Assignment API Request Timeout",
						(NoResString)"Set the timeout (in seconds) of a Machine Learning Team Assignment WebAPI Service request.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						@default);
				});
			}
		}

		#endregion

		public IntRegistryItem MaxErrorReportSizeInMb
			=> GetItem(
				"MaxErrorReportSizeInMb",
				() => new IntRegistryItem(
					"MaxErrorReportSizeInMb",
					(NoResString)IssueManagerSubCategory,
					(NoResString)"Max Error Report Size",
					(NoResString)("Issue Manager Processor (IMP) service task processes incoming error reports and saves them into db. " +
					"Too large error reports lead to large processing memory allocation potentially leading to a resource contention on the host, as well as excessive space consumption in the database. " +
					$"Systems sending error reports should not generate large or combined reports.{System.Environment.NewLine}" +
					"Reports with sizes greater than this value (MB) are not processed."),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					300,
					100,
					int.MaxValue));

		#endregion

		#region Customer Service Incidents

		public const string CustomerServiceSubCategory = Category + "/Customer Service Incidents";

		public BinaryRegistryItem CWSupportLoginTokenPrivateKey
		{
			get
			{
				return GetItem("CWSupportLoginTokenPrivateKey", delegate
				{
					return new BinaryRegistryItem(
						"CWSupportLoginTokenPrivateKey",
						(NoResString)CustomerServiceSubCategory,
						(NoResString)"CWSupport Account Login Token Private Key",
						(NoResString)"This private key is for signing tokens used for CWSupport account logins into customer systems.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						Array.Empty<byte>())
					{
						EditorInfo = new CWSupportLoginTokenPrivateKeyEditorInfo()
					};
				});
			}
		}

		#region Enable Incident Management Group Module
		public BooleanRegistryItem EnableIncidentManagementGroupModule
		{
			get
			{
				return GetItem("EnableIncidentManagementGroupModule", delegate
				{
					return new BooleanRegistryItem(
						"EnableIncidentManagementGroupModule",
						(NoResString)CustomerServiceSubCategory,
						(NoResString)"Enable Incident Management Group Module",
						(NoResString)"Set this registry to 'Yes' to enable Incident Management Group Module.",
						RegistryStorageFlags.System,
						Globals.IsDebugMode);
				});
			}
		}
		#endregion

		#region Enable Internal Work Item

		public BooleanRegistryItem EnableInternalWorkItem
		{
			get
			{
				return GetItem("EnableInternalWorkItem", delegate
				{
					return new BooleanRegistryItem(
						"EnableInternalWorkItem",
						(NoResString)CustomerServiceSubCategory,
						(NoResString)"Enable Internal Work Item",
						(NoResString)"Set this registry to 'Yes' to enable the Internal Work Item feature and disable legacy menu items functionality.",
						RegistryStorageFlags.System,
						defaultValue: false);
				});
			}
		}

		#endregion

		#region IS Alerts Service Account
		public StringRegistryItem ISAlertsServiceAccount
		{
			get
			{
				return GetItem("ISAlertsServiceAccount", delegate
				{
					return new StringRegistryItem(
						"ISAlertsServiceAccount",
						(NoResString)CustomerServiceSubCategory,
						ResString.GetMultilingualString("73914107-55ec-4082-9ee1-9034c747c0d7", "IS Alerts Service Account"),
						ResString.GetMultilingualString("73914107-55ec-4082-9ee1-9034c747c0d7", "IS Alerts Service Account"),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory,
						"ISS");
				});
			}
		}
		#endregion

		#region Email Notification on Assigned Change

		public BooleanRegistryItem EnableIncidentAssignedStaffChangedEmailNotification
		{
			get
			{
				return GetItem("EnableIncidentAssignedStaffChangedEmailNotification", delegate
				{
					return new BooleanRegistryItem(
						"EnableIncidentAssignedStaffChangedEmailNotification",
						(NoResString)CustomerServiceSubCategory,
						(NoResString)"Email Notification for Assigned Change (Old)",
						(NoResString)"When the Assigned Staff changes for an Incident, the new Assigned Staff will be sent an email notification if this Registry is enabled.",
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		#endregion

		#region Show Incidents Logged after this Date (Web)

		public DateTime ShowIncidentsLoggedAfterThisDate
		{
			get { return ShowIncidentsLoggedAfterThisDateRegistryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { ShowIncidentsLoggedAfterThisDateRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		DateTimeRegistryItem ShowIncidentsLoggedAfterThisDateRegistryItem
		{
			get
			{
				return GetItem("ShowIncidentsLoggedAfterThisDate", delegate
				{
					return new DateTimeRegistryItem(
						"ShowIncidentsLoggedAfterThisDate",
						(NoResString)CustomerServiceSubCategory,
						(NoResString)"Show Incidents Logged after this Date",
						(NoResString)"Incidents logged after this date will be shown on ediWeb. If no date is set, all Incidents will be shown.",
						RegistryStorageFlags.System,
						RegistryOptions.NotCached);
				});
			}
		}

		#endregion

		#region Support Incident Staff Groups

		public const string IncidentGroupsSubCategory = CustomerServiceSubCategory + "/Incident Staff Groups";

		public const string IncidentGroupsSubCategoryENT = IncidentGroupsSubCategory + "/Enterprise";
		public const string IncidentGroupsSubCategoryDLV = IncidentGroupsSubCategory + "/Deliverence";
		public const string IncidentGroupsSubCategoryCAR = IncidentGroupsSubCategory + "/Cargowise";

		#region Enterprise

		public GuidRegistryItem IncidentInstallationsGroupENT
		{
			get
			{
				return GetItem("IncidentInstallationsGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"IncidentInstallationsGroup",
						(NoResString)IncidentGroupsSubCategoryENT,
						(NoResString)"Installations Group",
						(NoResString)"This is the group that looks after installation issues.",
						RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public GuidRegistryItem IncidentFeatureRequestGroupENT
		{
			get
			{
				return GetItem("IncidentFeatureRequestGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"IncidentFeatureRequestGroup",
						(NoResString)IncidentGroupsSubCategoryENT,
						(NoResString)"Feature Request Group",
						(NoResString)"This is the group that looks after feature requests.",
						RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public GuidRegistryItem IncidentDefectManagerGroupENT
		{
			get
			{
				return GetItem("IncidentDefectManagerGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"IncidentDefectManagerGroup",
						(NoResString)IncidentGroupsSubCategoryENT,
						(NoResString)"Defect Manager Group",
						(NoResString)"This is the group that looks after defect management.",
						RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public GuidRegistryItem IncidentCustomisationGroupENT
		{
			get
			{
				return GetItem("IncidentCustomisationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"IncidentCustomisationGroup",
						(NoResString)IncidentGroupsSubCategoryENT,
						(NoResString)"Customisation Manager Group",
						(NoResString)"This is the group that looks after client specific customisations.",
						RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public GuidRegistryItem IncidentGraphicGroupENT
		{
			get
			{
				return GetItem("IncidentGraphicsGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"IncidentGraphicsGroup",
						(NoResString)IncidentGroupsSubCategoryENT,
						(NoResString)"Graphics Customisation Manager Group",
						(NoResString)"This is the group that looks after client graphics customisations.",
						RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		public GuidRegistryItem IncidentSupportGroup
		{
			get
			{
				return GetItem("IncidentSupportGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"IncidentSupportGroup",
						(NoResString)IncidentGroupsSubCategory,
						(NoResString)"Customer Service / Support Group",
						(NoResString)"This is the group that looks after customer service and support.",
						RegistryStorageFlags.System);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#region Customer Service High Criticality Incident Group

		public GuidRegistryItem CustomerServiceHighCriticalityIncidentGroup
		{
			get
			{
				return GetItem("CustomerServiceHighCriticalityIncidentGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"CustomerServiceHighCriticalityIncidentGroup",
						(NoResString)IncidentGroupsSubCategory,
						(NoResString)"eRequest High Criticality Incident Group",
						(NoResString)"When a eRequest is raised by a client and it is logged as a CR1, CR2 or CR3, an email will be sent to this group.",
						RegistryStorageFlags.System);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#endregion

		#region Customer Service Incident Email Templates

		public const string CustomerServiceEmailTemplatesSubCategory = CustomerServiceSubCategory + "/Email Templates";

		#region Customer Service Notification Email Template

		public CodeDescriptionIncidentEmailTemplatePairCollectionRegistryItem CustomerServiceNotificationEmailTemplateRaw
		{
			get
			{
				return GetItem(
					"CustomerServiceNotificationEmailTemplate",
					delegate
					{
						var registryItem = new CodeDescriptionIncidentEmailTemplatePairCollectionRegistryItem(
							"CustomerServiceNotificationEmailTemplate",
							(NoResString)CustomerServiceEmailTemplatesSubCategory,
							(NoResString)"Customer Service Notification Email Template",
							(NoResString)CustomerServiceNotificationEmailRegistryItemHint,
							RegistryStorageFlags.System,
							DefaultCustomerServiceNotificationEmailTemplatePairCollection);
						return registryItem;
					});
			}
		}

		CodeDescriptionIncidentEmailTemplatePairCollection DefaultCustomerServiceNotificationEmailTemplatePairCollection
		{
			get
			{
				var collection = new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident));

				var templatePair = collection.AddNew();
				templatePair.Code = "DEF";
				templatePair.Description = (NoResString)"Default Email Template";
				templatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = DefaultCustomerServiceNotificationEmailSubject;
				templatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = GetDefaultCustomerServiceNotificationEmailBody(false);
				templatePair.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = DefaultCustomerServiceNotificationEmailSubject;
				templatePair.EmailTemplates.ERequestV2EmailTemplate.EmailBody = GetDefaultCustomerServiceNotificationEmailBody(true);
				return collection;
			}
		}

		public const string CustomerServiceNotificationEmailRegistryItemHint = "This is the default template for customer service notification emails.";

		const string DefaultCustomerServiceNotificationEmailSubject = "Notification of Incident: (*IncidentNumber*) - (*Summary*)";

		string GetDefaultCustomerServiceNotificationEmailBody(bool eRequestV2)
		{
			string replyInstructions =
				eRequestV2 ? "sending an eConversation message through the Incident" :
				"directly replying to this email";

			return @"We have received your enquiry regarding the following problem and will contact you soon.

(*DetailedDescription*)

Your Incident Number is: (*IncidentNumber*).

Please quote this Incident Number in any future correspondence.

We are also confirming your contact details are:

	 Name: (*ClientName*)
	 Phone: (*IncidentBranchPhone*)

If your contact details are incorrect, or if you require further clarification regarding this issue please let us know by " + replyInstructions + ".";
		}

		#endregion

		#region Incident Criticality Changed Email Template
		public CodeDescriptionIncidentEmailTemplatePairCollectionRegistryItem IncidentCriticalityChangedRaw
		{
			get
			{
				return GetItem(
					"IncidentCriticalityChanged",
					delegate
					{
						var registryItem = new CodeDescriptionIncidentEmailTemplatePairCollectionRegistryItem(
							"IncidentCriticalityChanged",
							(NoResString)CustomerServiceEmailTemplatesSubCategory,
							(NoResString)"Incident Criticality Changed Email Template",
							(NoResString)"This is the default template for incident criticality changed notification emails.",
							RegistryStorageFlags.System,
							DefaultIncidentCriticalityChangedEmailTemplatePairCollection);
						return registryItem;
					});
			}
		}

		CodeDescriptionIncidentEmailTemplatePairCollection DefaultIncidentCriticalityChangedEmailTemplatePairCollection
		{
			get
			{
				var collection = new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident));
				var templatePair = collection.AddNew();
				templatePair.Code = "DEF";
				templatePair.Description = (NoResString)"Default Email Template";
				templatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = DefaultIncidentCriticalityChangedEmailSubject;
				templatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = GetDefaultIncidentCriticalityChangedEmailBody(false);
				templatePair.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = DefaultIncidentCriticalityChangedEmailSubject;
				templatePair.EmailTemplates.ERequestV2EmailTemplate.EmailBody = GetDefaultIncidentCriticalityChangedEmailBody(true);
				return collection;
			}
		}

		const string DefaultIncidentCriticalityChangedEmailSubject = "Notification of Incident Criticality Changed: (*IncidentNumber*) - (*Summary*)";

		string GetDefaultIncidentCriticalityChangedEmailBody(bool eRequestV2)
		{
			string replyInstructions =
				eRequestV2 ? "sending an eConversation message through the Incident" : "directly replying to this email";

			return @"<div style=""border-style:solid; border-width:1px"">
eRequest Number: (*IncidentNumber*) <br/>
eRequest Title: (*Summary*) <br/>
eRequest Update:<br/>
Criticality changed from (*CriticalityChangedFrom*) to (*Criticality*) <br/>
<br/></div>
If any details are incorrect or there are any concerns please let us know by sending an eConversation message through the the eRequest Management Portal.<br/>";
		}
		#endregion

		#region Self Logged Incident Notification Email Template

		public CodeDescriptionIncidentEmailTemplatePairCollectionRegistryItem SelfLoggedIncidentNotificationEmailTemplateRaw
		{
			get
			{
				return GetItem(
					"SelfLoggedIncidentNotificationEmailTemplate",
					delegate
					{
						var registryItem = new CodeDescriptionIncidentEmailTemplatePairCollectionRegistryItem(
							"SelfLoggedIncidentNotificationEmailTemplate",
							(NoResString)CustomerServiceEmailTemplatesSubCategory,
							(NoResString)"Self Logged Incident Notification Email Template",
							(NoResString)SelfLoggedIncidentNotificationEmailRegistryItemHint,
							RegistryStorageFlags.System,
							DefaultSelfLoggedIncidentNotificationEmailTemplatePairCollection);
						return registryItem;
					});
			}
		}

		CodeDescriptionIncidentEmailTemplatePairCollection DefaultSelfLoggedIncidentNotificationEmailTemplatePairCollection
		{
			get
			{
				var collection = new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident));

				var templatePair = collection.AddNew();
				templatePair.Code = "DEF";
				templatePair.Description = (NoResString)"Default Email Template";
				templatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = DefaultSelfLoggedIncidentNotificationEmailSubject;
				templatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = GetDefaultSelfLoggedIncidentNotificationEmailBody(false);
				templatePair.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = DefaultSelfLoggedIncidentNotificationEmailSubject;
				templatePair.EmailTemplates.ERequestV2EmailTemplate.EmailBody = GetDefaultSelfLoggedIncidentNotificationEmailBody(true);

				return collection;
			}
		}

		public const string SelfLoggedIncidentNotificationEmailRegistryItemHint = "This is the default template for Self Logged Incident notification emails.";

		const string DefaultSelfLoggedIncidentNotificationEmailSubject = "Customer Service Incident Raised - (*IncidentNumber*)";

		string GetDefaultSelfLoggedIncidentNotificationEmailBody(bool eRequestV2)
		{
			string replyInstructions =
				eRequestV2 ? "sending an eConversation message through the Incident" :
				"directly replying to this email";

			return @"We have received your enquiry regarding the following problem and will contact you soon.

(*DetailedDescription*)

Your Incident Number is: (*IncidentNumber*).

Please quote this Incident Number in any future correspondence.

We are also confirming your contact details are:

	 Name: (*ClientName*)
	 Phone: (*IncidentBranchPhone*)

If your contact details are incorrect, or if you require further clarification regarding this issue please let us know by " + replyInstructions + ".";
		}

		#endregion

		#region Customer Service Response Notification Reminder

		public static class CustomerServiceEmailTemplateCodes
		{
			public const string Default = "DFT";
			public const string FollowUpERequest = "NEV";
		}

		public NotificationEmailTemplateRegistryItem CustomerServiceResponseNotificationReminderTemplate
		{
			get
			{
				return GetItem(
					"CustomerServiceResponseNotificationReminderTemplate",
					delegate
					{
						NotificationEmailTemplateRegistryItem registryItem = new NotificationEmailTemplateRegistryItem(
							"CustomerServiceResponseNotificationReminderTemplate",
							(NoResString)CustomerServiceEmailTemplatesSubCategory,
							(NoResString)"Customer Service Response Notification Reminder Template",
							(NoResString)"This is the default customer service incident awaiting response notification message. Use Body only. Subject is NOT in use.",
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							typeof(DocSupportIncident),
							"",
							DefaultCustomerServiceResponseNotificationReminderMessage);
						return registryItem;
					});
			}
		}

		const string DefaultCustomerServiceResponseNotificationReminderMessage = @"Reminder - This eRequest remains as ""awaiting client response"". It will be closed if no response is received in (*TimeLeft*).";

		#endregion

		#region Customer Service Awaiting Response Notification Message Template

		public NotificationEmailTemplateRegistryItem CustomerServiceAwaitingResponseNotificationMessageTemplate
		{
			get
			{
				return GetItem(
					"CustomerServiceAwaitingResponseNotificationMessageTemplate",
					delegate
					{
						NotificationEmailTemplateRegistryItem registryItem = new NotificationEmailTemplateRegistryItem(
							"CustomerServiceAwaitingResponseNotificationMessageTemplate",
							(NoResString)CustomerServiceEmailTemplatesSubCategory,
							(NoResString)"Customer Service Awaiting Response Notification Message Template",
							(NoResString)"This is the default customer service incident awaiting response notification message.",
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							typeof(DocSupportIncident),
							@"Update on Incident: (*IncidentNumber*) - (*Summary*)",
							DefaultCustomerServiceAwaitingResponseNotificationMessage);
						return registryItem;
					});
			}
		}

		const string DefaultCustomerServiceAwaitingResponseNotificationMessage = @"This is an update to Incident number (*IncidentNumber*).
<!--[if gte mso 9]><style>.outlook { font-family: arial, sans-serif; font-size: 12px}</style><![endif]--><table><tr><td>
<table width=""600px"" height=""30px"" align=""left"" border=""1"" bordercolor=""#FDFDFD"" cellpadding=""6"" cellspacing=""0"" bgcolor=""#FFFFFF""><tr><td bordercolor=""#FDFDFD"" style=""width:600px"">
<span class=""outlook"">eRequest Number: <b> (*IncidentNumber*) </b>
eRequest Title: <b>(*Summary*)</b>
eRequest Details:
<i>(*DetailedDescription*)</i>
eRequest Update:
<i>(*eConversationMessages*)</i>
</span></td></tr></table></td></tr><tr><td>
If any details are incorrect or there are any concerns please let us know by sending an eConversation message through the the eRequest Management Portal.</table>
";

		#endregion

		#region Customer Service Final Closure Auto Reply Email Template

		public CodeDescriptionIncidentEmailTemplatePairCollectionRegistryItem CustomerServiceFinalClosureAutoReplyEmailTemplate
		{
			get
			{
				return GetItem(
					"CustomerServiceFinalClosureAutoReplyEmailTemplate",
					delegate
					{
						var registryItem = new CodeDescriptionIncidentEmailTemplatePairCollectionRegistryItem(
							"CustomerServiceFinalClosureAutoReplyEmailTemplate",
							(NoResString)CustomerServiceEmailTemplatesSubCategory,
							(NoResString)"Customer Service Final Closure Auto Reply Email Template",
							(NoResString)"These are default notification templates for customer service final closure auto reply email",
							RegistryStorageFlags.System,
							CustomerServiceFinalClosureAutoReplyEmailTemplateDefault);
						return registryItem;
					});
			}
		}

		CodeDescriptionIncidentEmailTemplatePairCollection CustomerServiceFinalClosureAutoReplyEmailTemplateDefault
		{
			get
			{
				var subject = "eRequest: (*IncidentNumber*) is Closed.";
				var body = $"Incident (*IncidentNumber*) - (*Summary*) - is Closed.{System.Environment.NewLine}Should you need any further help with regards to this incident, please create a follow-up eRequest using the button below.";

				var collection = new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident));

				var defaultTemplatePair = collection.AddNew();
				defaultTemplatePair.Code = CustomerServiceEmailTemplateCodes.Default;
				defaultTemplatePair.Description = (NoResString)"Default Final Closure Auto-Reply";
				defaultTemplatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = subject;
				defaultTemplatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = body;
				defaultTemplatePair.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = subject;
				defaultTemplatePair.EmailTemplates.ERequestV2EmailTemplate.EmailBody = body;

				return collection;
			}
		}

		#endregion

		#region Customer Service Incident Closed Notifiation Email Templates

		public CodeDescriptionIncidentEmailTemplatePairCollectionRegistryItem CustomerServiceIncidentClosedNotificationEmailTemplates
		{
			get
			{
				var defaults = DefaultIncidentClosedEmailTemplatePairs;

				var followUpERequestEmailTemplate = defaults.AddNew();
				followUpERequestEmailTemplate.Code = CustomerServiceEmailTemplateCodes.FollowUpERequest;
				followUpERequestEmailTemplate.Description = (NoResString)"Do Not Reopen Default Close Notification Email Template";
				followUpERequestEmailTemplate.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = GetSupportClosedEmailSubject();
				followUpERequestEmailTemplate.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = GetSupportClosedEmailBody(false);
				followUpERequestEmailTemplate.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = GetSupportClosedEmailSubject();
				followUpERequestEmailTemplate.EmailTemplates.ERequestV2EmailTemplate.EmailBody = GetSupportClosedEmailBody(true);

				return GetItem(
					"CustomerServiceIncidentClosedNotificationEmailTemplates",
					delegate
					{
						var registryItem = new CodeDescriptionIncidentEmailTemplatePairCollectionRegistryItem(
							"CustomerServiceIncidentClosedNotificationEmailTemplates",
							(NoResString)CustomerServiceEmailTemplatesSubCategory,
							(NoResString)"Customer Service Incident Closed Notification Email Templates",
							(NoResString)"These are default notification templates for customer service incident closed emails.",
							RegistryStorageFlags.System,
							defaults);
						return registryItem;
					});
			}
		}

		CodeDescriptionIncidentEmailTemplatePairCollection DefaultIncidentClosedEmailTemplatePairs
		{
			get
			{
				var collection = new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident));

				var defaultTemplatePair = collection.AddNew();
				defaultTemplatePair.Code = CustomerServiceEmailTemplateCodes.Default;
				defaultTemplatePair.Description = (NoResString)"Default Close Notification Email Template";
				defaultTemplatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = GetSupportClosedEmailSubject();
				defaultTemplatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = GetSupportClosedEmailBody(false);
				defaultTemplatePair.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = GetSupportClosedEmailSubject();
				defaultTemplatePair.EmailTemplates.ERequestV2EmailTemplate.EmailBody = GetSupportClosedEmailBody(true);

				return collection;
			}
		}

		string GetSupportClosedEmailSubject()
		{
			return "Incident: (*IncidentNumber*) has been (*SupportIncidentCloseType*).";
		}

		string GetSupportClosedEmailBody(bool eRequestV2)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("Incident (*IncidentNumber*) - (*Summary*) - ");

			string replyInstructions =
				eRequestV2 ? "sending an eConversation message through the Incident" :
				"replying directly to this email";

			builder.Append("has been closed with a disposition of (*DetailedDispositionDescription*).");
			builder.Append("(*ChargeableWorkBillingNotice*)");
			builder.Append("(*ResolutionNoteText*)");
			builder.AppendLine();
			builder.AppendLine();
			builder.Append("Should you need any further help with regards to this incident, please let us know by " + replyInstructions + " and we will re-open this incident and assist you further.");

			return builder.ToString();
		}

		#endregion

		#region Customer Service Resolved Notifiation Template

		public CodeDescriptionIncidentEmailTemplatePairCollectionRegistryItem CustomerServiceResolvedNotificationTemplates
		{
			get
			{
				return GetItem(
					"CustomerServiceResolvedNotificationTemplates",
					delegate
					{
						var registryItem = new CodeDescriptionIncidentEmailTemplatePairCollectionRegistryItem(
							"CustomerServiceResolvedNotificationTemplates",
							(NoResString)CustomerServiceEmailTemplatesSubCategory,
							(NoResString)"Customer Service Resolved Notification Templates",
							(NoResString)"These are default notification templates for customer service incident resolved emails.",
							RegistryStorageFlags.System,
							DefaultIncidentResolvedEmailTemplatePairs);
						return registryItem;
					});
			}
		}

		CodeDescriptionIncidentEmailTemplatePairCollection DefaultIncidentResolvedEmailTemplatePairs
		{
			get
			{
				var collection = new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident));

				var defaultTemplatePair = collection.AddNew();
				defaultTemplatePair.Code = "DRT";
				defaultTemplatePair.Description = (NoResString)"Default Resolve Notification Email Template";
				defaultTemplatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = GetSupportResolvedEmailSubject();
				defaultTemplatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = GetSupportResolvedEmailBody(false);
				defaultTemplatePair.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = GetSupportResolvedEmailSubject();
				defaultTemplatePair.EmailTemplates.ERequestV2EmailTemplate.EmailBody = GetSupportResolvedEmailBody(true);

				return collection;
			}
		}

		string GetSupportResolvedEmailSubject()
		{
			return "Incident: (*IncidentNumber*) has been (*SupportIncidentCloseType*).";
		}

		string GetSupportResolvedEmailBody(bool eRequestV2)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("Incident (*IncidentNumber*) - (*Summary*) - ");

			string replyInstructions =
				eRequestV2 ? "sending an eConversation message through the Incident" :
				"replying directly to this email";

			builder.Append("has been Resolved with a disposition of (*DetailedDispositionDescription*).");
			builder.Append("(*ChargeableWorkBillingNotice*)");
			builder.Append("(*ResolutionNoteText*)");
			builder.AppendLine();
			builder.AppendLine();
			builder.Append("Should you need any further help with regards to this incident, please let us know by " + replyInstructions + " and we will re-open this incident and assist you further.");

			return builder.ToString();
		}

		#endregion

		#region Customer Service Incident Work Item Created Email Templates

		public CodeDescriptionIncidentEmailTemplatePairCollectionRegistryItem CustomerServiceIncidentWorkItemCreatedEmailTemplates
		{
			get
			{
				return GetItem(
					"CustomerServiceIncidentWorkItemCreatedEmailTemplates",
					delegate
					{
						var registryItem = new CodeDescriptionIncidentEmailTemplatePairCollectionRegistryItem(
							"CustomerServiceIncidentWorkItemCreatedEmailTemplates",
							(NoResString)CustomerServiceEmailTemplatesSubCategory,
							(NoResString)"Incident Work Item Created Email Templates",
							(NoResString)"These are default notification templates for customer service incident work item created emails.",
							RegistryStorageFlags.System,
							DefaultIncidentWorkItemCreatedEmailTemplatePairs);
						return registryItem;
					});
			}
		}

		CodeDescriptionIncidentEmailTemplatePairCollection DefaultIncidentWorkItemCreatedEmailTemplatePairs
		{
			get
			{
				var collection = new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident));

				var defectTemplatePair = collection.AddNew();
				defectTemplatePair.Code = SupportIncidentCategoriesList.Codes.Defect;
				defectTemplatePair.Description = (NoResString)"Defect Incident Work Item Created Email Template";
				defectTemplatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = GetWorkItemCreatedSubject();
				defectTemplatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = GetWorkItemCreatedEmailBody(false, SupportIncidentCategoriesList.Codes.Defect);
				defectTemplatePair.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = GetWorkItemCreatedSubject();
				defectTemplatePair.EmailTemplates.ERequestV2EmailTemplate.EmailBody = GetWorkItemCreatedEmailBody(true, SupportIncidentCategoriesList.Codes.Defect);

				var featureTemplatePair = collection.AddNew();
				featureTemplatePair.Code = SupportIncidentCategoriesList.Codes.FeatureRequest;
				featureTemplatePair.Description = (NoResString)"Feature Incident Work Item Created Email Template";
				featureTemplatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = GetWorkItemCreatedSubject();
				featureTemplatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = GetWorkItemCreatedEmailBody(false, SupportIncidentCategoriesList.Codes.FeatureRequest);
				featureTemplatePair.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = GetWorkItemCreatedSubject();
				featureTemplatePair.EmailTemplates.ERequestV2EmailTemplate.EmailBody = GetWorkItemCreatedEmailBody(true, SupportIncidentCategoriesList.Codes.FeatureRequest);

				var complianceRequirementTemplatePair = collection.AddNew();
				complianceRequirementTemplatePair.Code = SupportIncidentCategoriesList.Codes.ComplianceRequirement;
				complianceRequirementTemplatePair.Description = (NoResString)"Compliance Requirement Work Item Created Email Template";
				complianceRequirementTemplatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = GetWorkItemCreatedSubject();
				complianceRequirementTemplatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = GetWorkItemCreatedEmailBody(false, SupportIncidentCategoriesList.Codes.ComplianceRequirement);
				complianceRequirementTemplatePair.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = GetWorkItemCreatedSubject();
				complianceRequirementTemplatePair.EmailTemplates.ERequestV2EmailTemplate.EmailBody = GetWorkItemCreatedEmailBody(true, SupportIncidentCategoriesList.Codes.ComplianceRequirement);

				var customerServiceRequestTemplatePair = collection.AddNew();
				customerServiceRequestTemplatePair.Code = SupportIncidentCategoriesList.Codes.CustomerServiceRequest;
				customerServiceRequestTemplatePair.Description = (NoResString)"Service Request Work Item Created Email Template";
				customerServiceRequestTemplatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = GetWorkItemCreatedSubject();
				customerServiceRequestTemplatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = GetWorkItemCreatedEmailBody(false, SupportIncidentCategoriesList.Codes.CustomerServiceRequest);
				customerServiceRequestTemplatePair.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = GetWorkItemCreatedSubject();
				customerServiceRequestTemplatePair.EmailTemplates.ERequestV2EmailTemplate.EmailBody = GetWorkItemCreatedEmailBody(true, SupportIncidentCategoriesList.Codes.CustomerServiceRequest);

				return collection;
			}
		}

		string GetWorkItemCreatedSubject()
		{
			return "Incident: (*IncidentNumber*) has been scheduled for development work.";
		}

		string GetWorkItemCreatedEmailBody(bool eRequestV2, string stage)
		{
			var advice =
				(stage == SupportIncidentCategoriesList.Codes.Defect) ? "an urgent fix is underway to rectify the problem you were experiencing" :
				(stage == SupportIncidentCategoriesList.Codes.FeatureRequest) ? "a change is underway to complete the new feature that was requested by you" :
				(stage == SupportIncidentCategoriesList.Codes.ComplianceRequirement) ? "work is underway to complete the compliance requirement that was raised by you" :
				(stage == SupportIncidentCategoriesList.Codes.CustomerServiceRequest) ? "work is underway to complete the service request that was raised by you" :
				"work is underway to complete the customer service that was raised by you";

			var replyInstructions =
				eRequestV2 ? "sending an eConversation message through the Incident" :
				"directly replying to this email";

			return string.Format(CultureInfo.CurrentCulture,
@"We wish to advise you that {0}. We will be in further contact when this work has been completed.

If any details are incorrect or there are any concerns please let us know by {1}.

<b>Incident Details (*IncidentNumber*) ((*Summary*))</b>

<i>(*DetailedDescription*)</i>
",
				advice, replyInstructions);
		}

		#endregion

		#region Customer Service Incident Work Item Completed Email Templates

		public CodeDescriptionIncidentEmailTemplatePairCollectionRegistryItem CustomerServiceIncidentWorkItemCompletedEmailTemplates
		{
			get
			{
				return GetItem(
					"CustomerServiceIncidentWorkItemCompletedEmailTemplates",
					delegate
					{
						var registryItem = new CodeDescriptionIncidentEmailTemplatePairCollectionRegistryItem(
							"CustomerServiceIncidentWorkItemCompletedEmailTemplates",
							(NoResString)CustomerServiceEmailTemplatesSubCategory,
							(NoResString)"Incident Work Item Completed Email Templates",
							(NoResString)"These are default notification templates for customer service incident work item completed emails.",
							RegistryStorageFlags.System,
							DefaultIncidentWorkItemCompletedEmailTemplatesValue);
						registryItem.EditorInfo = new CodeDescriptionIncidentEmailTemplatePairCollectionRegistryEditorInfo() { IsNeedUpgradeColumnVisible = true };
						return registryItem;
					});
			}
		}

		CodeDescriptionIncidentEmailTemplatePairCollection DefaultIncidentWorkItemCompletedEmailTemplatesValue
		{
			get
			{
				var collection = new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident));

				var defectTemplatePair = collection.AddNew();
				defectTemplatePair.Code = SupportIncidentCategoriesList.Codes.Defect;
				defectTemplatePair.Description = (NoResString)"Defect Incident Work Item Completed Email Template";
				defectTemplatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = GetWorkItemCompletedSubject();
				defectTemplatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = GetWorkItemCompletedEmailBody(SupportIncidentCategoriesList.Codes.Defect, false);
				defectTemplatePair.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = GetWorkItemCompletedSubject();
				defectTemplatePair.EmailTemplates.ERequestV2EmailTemplate.EmailBody = GetWorkItemCompletedEmailBody(SupportIncidentCategoriesList.Codes.Defect, true);

				var featureTemplatePair = collection.AddNew();
				featureTemplatePair.Code = SupportIncidentCategoriesList.Codes.FeatureRequest;
				featureTemplatePair.Description = (NoResString)"Feature Incident Work Item Completed Email Template";
				featureTemplatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = GetWorkItemCompletedSubject();
				featureTemplatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = GetWorkItemCompletedEmailBody(SupportIncidentCategoriesList.Codes.FeatureRequest, false);
				featureTemplatePair.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = GetWorkItemCompletedSubject();
				featureTemplatePair.EmailTemplates.ERequestV2EmailTemplate.EmailBody = GetWorkItemCompletedEmailBody(SupportIncidentCategoriesList.Codes.FeatureRequest, true);

				var complianceRequirementPair = collection.AddNew();
				complianceRequirementPair.Code = SupportIncidentCategoriesList.Codes.ComplianceRequirement;
				complianceRequirementPair.Description = (NoResString)"Compliance Requirement Work Item Completed Email Template";
				complianceRequirementPair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = GetWorkItemCompletedSubject();
				complianceRequirementPair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = GetWorkItemCompletedEmailBody(SupportIncidentCategoriesList.Codes.ComplianceRequirement, false);
				complianceRequirementPair.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = GetWorkItemCompletedSubject();
				complianceRequirementPair.EmailTemplates.ERequestV2EmailTemplate.EmailBody = GetWorkItemCompletedEmailBody(SupportIncidentCategoriesList.Codes.ComplianceRequirement, true);

				var customerServicesPair = collection.AddNew();
				customerServicesPair.Code = SupportIncidentCategoriesList.Codes.CustomerServiceRequest;
				customerServicesPair.Description = (NoResString)"Service Request Work Item Completed Email Template";
				customerServicesPair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = GetWorkItemCompletedSubject();
				customerServicesPair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = GetWorkItemCompletedEmailBody(SupportIncidentCategoriesList.Codes.CustomerServiceRequest, false);
				customerServicesPair.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = GetWorkItemCompletedSubject();
				customerServicesPair.EmailTemplates.ERequestV2EmailTemplate.EmailBody = GetWorkItemCompletedEmailBody(SupportIncidentCategoriesList.Codes.CustomerServiceRequest, true);

				return collection;
			}
		}

		string GetWorkItemCompletedSubject()
		{
			return "Incident (*IncidentNumber*) resolved: (*Summary*)";
		}

		string GetWorkItemCompletedEmailBody(string stage, bool eRequestV2)
		{
			string advice =
				(stage == SupportIncidentCategoriesList.Codes.Defect) ? "rectify this problem has now been completed. You will receive a further email from our Team when an upgrade / fix has been delivered to your system for you to apply." :
				(stage == SupportIncidentCategoriesList.Codes.FeatureRequest) ? "satisfy your feature request has now been completed. You will receive a further email from our Team when an upgrade has been delivered to your system for you to apply." :
				(stage == SupportIncidentCategoriesList.Codes.ComplianceRequirement) ? "satisfy your compliance requirement has now been completed. You will receive a further email from our Team when an upgrade / fix has been delivered to your system for you to apply." :
				(stage == SupportIncidentCategoriesList.Codes.CustomerServiceRequest) ? "satisfy your service request has now been completed. You will receive a further email from our Team when an upgrade / fix has been delivered to your system for you to apply." :
				"satisfy your customer service has now been completed. You will receive a further email from our Team when an upgrade / fix has been delivered to your system for you to apply.";

			string replyInstructions =
				eRequestV2 ? "sending an eConversation message through the Incident" :
				"directly replying to this email";

			return string.Format(CultureInfo.CurrentCulture,
@"This email is regarding Customer Service Incident (*IncidentNumber*).

We wish to advise you that the work required to " + advice + @"

Incident Details
(*Summary*)
(*DetailedDescription*)

If any details are incorrect or there are any concerns please let us know by " + replyInstructions + ".");
		}

		#endregion

		#region Incident Update Notification Email Template

		public NotificationEmailTemplateRegistryItem IncidentUpdateNotificationEmailTemplate
		{
			get
			{
				return GetItem(
					"IncidentUpdateNotificationEmailTemplate",
					delegate
					{
						NotificationEmailTemplateRegistryItem registryItem = new NotificationEmailTemplateRegistryItem(
							"IncidentUpdateNotificationEmailTemplate",
							(NoResString)CustomerServiceEmailTemplatesSubCategory,
							(NoResString)"Incident Update Notification Email Template",
							(NoResString)"This is the default template for Incident update notification emails to client. Summary of fields updated will append to the end of body set in this template.",
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							typeof(DocSupportIncident),
							DefaultIncidentUpdateNotificationEmailSubject,
							string.Empty);
						return registryItem;
					});
			}
		}

		const string DefaultIncidentUpdateNotificationEmailSubject = "Customer Service Incident (*IncidentNumber*) Has Been Updated";

		#endregion

		#region Customer Support Auto Reply Email Template

		public string CustomerSupportAutoReplyEmailTemplate
		{
			get { return CustomerSupportAutoReplyEmailTemplateRaw.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { CustomerSupportAutoReplyEmailTemplateRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public StringRegistryItem CustomerSupportAutoReplyEmailTemplateRaw
		{
			get
			{
				return GetItem(
					"CustomerSupportAutoReplyEmailTemplate",
					delegate
					{
						StringRegistryItem registryItem = new StringRegistryItem(
							"CustomerSupportAutoReplyEmailTemplate",
							(NoResString)CustomerServiceEmailTemplatesSubCategory,
							(NoResString)"Customer Support Auto Reply Email Template",
							(NoResString)"This is the default template (body) for customer support auto reply emails.",
							RegistryStorageFlags.System,
							string.Empty);
						registryItem.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
						return registryItem;
					});
			}
		}

		#endregion

		#region eRequest Pending Approval Notification Email Template

		public NotificationEmailTemplateRegistryItem ERequestPendingApprovalNotificationMessageTemplate
		{
			get
			{
				return GetItem(
					"ERequestPendingApprovalNotificationMessageTemplate",
					delegate
					{
						var registryItem = new NotificationEmailTemplateRegistryItem(
							"ERequestPendingApprovalNotificationMessageTemplate",
							(NoResString)CustomerServiceEmailTemplatesSubCategory,
							(NoResString)"eRequest Pending Approval Notification Message Template",
							(NoResString)"eRequest Pending Approval Notification Message Template",
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							typeof(DocIncidentRequest),
							@"eRequest Pending Approval - (*IncidentNumber*)",
							@"<br>An internal eRequest review has been created for review and approval (internal only). The eRequest has not been submitted to Wisetech Global.<br><br>



<br>eRequest Number : (*IncidentNumber*)<br>
<br>Criticality             : (*Criticality*)<br>
<br>Client Reference : (*ClientReferenceNumber*)<br>
<br>Summary             : (*Summary*)<br>

<br>The eRequest can be viewed via the following link : <br>

<br>(*IncidentLinkURL*)<br>");
						return registryItem;
					});
			}
		}

		#endregion

		#region Subscriber Update eConversation Email Notification Email Template

		public NotificationEmailTemplateRegistryItem SubscriberUpdateEConversationEmailTemplate
		{
			get
			{
				return GetItem(
					"SubscriberUpdateEConversationEmailTemplate",
					delegate
					{
						NotificationEmailTemplateRegistryItem registryItem = new NotificationEmailTemplateRegistryItem(
							"SubscriberUpdateEConversationEmailTemplate",
							(NoResString)CustomerServiceEmailTemplatesSubCategory,
							(NoResString)"Subscriber Update eConversation Email Notification",
							(NoResString)SubscriberUpdateEConversationHint,
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							typeof(DocSupportIncident),
							DefaultSubscriberUpdateEConversationSubject,
							DefaultSubscriberUpdateEConversationMessage);
						return registryItem;
					});
			}
		}

		public const string DefaultSubscriberUpdateEConversationSubject = @"New Messages in (*ID*)";

		public const string SubscriberUpdateEConversationHint = "Configure the template for subscriber update eConversation message notification emails.";

		public const string DefaultSubscriberUpdateEConversationMessage = @"<html>
<head>
	<style>
		body, td, p, h1, h2, a {
			background-color: #FFFFFF;
			font-family: Arial, sans-serif;
			font-size: 12px;
		}
		body {
			width: 600px;
		}
		h1 {
			font-size: 16px;
		}
		h2 {
			font-size: 14px;
		}
		table {
			border-style: none;
		}
	</style>
</head>
<body><p style=""color:red""><b> Note: </b> Replies to eRequest emails will not be processed unless sent to <u>'support@wisetechglobal.com'. </u>.</p>

	<h1>(*BusinessObjectName*) - New Messages</h1>
	<p>
		New messages have been added to <a href=""(*BusinessObjectHyperlink*)"">(*BusinessObjectName*)</a>
		<br />
		<br />
		You will find the new messages below, with some previous messages to provide additional context.
	</p>
	<strong><a href=""(*BusinessObjectHyperlink*)"">Reply via eConversation</a></strong>
	(*NewMessages*)
	(*PreviousMessages*)
	(*IF(""(*IsInternalRecipient*)""==""Y"", ""<p><em>If you no longer wish to be notified about <a href=""(*BusinessObjectHyperlink*)"">(*BusinessObjectName*)</a>, please unsubscribe yourself through the eConversation tab.</em></p>"", """")*)
	<p><em>All times are displayed in the senders local time of UTC(*UtcOffset*)</em></p>
	(*EmailIdentifier*)
</body>
</html>";

		#endregion

		#region Default Unsubscribe Email Template

		public NotificationEmailTemplateRegistryItem UnsubscriberEmailTemplate
		{
			get
			{
				return GetItem(
					"UnsubscriberEmailTemplate",
					delegate
					{
						NotificationEmailTemplateRegistryItem registryItem = new NotificationEmailTemplateRegistryItem(
							"UnsubscriberEmailTemplate",
							(NoResString)CustomerServiceEmailTemplatesSubCategory,
							(NoResString)"Unsubscribe Email Template",
							(NoResString)UnsubscriberEmailTemplateHint,
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							typeof(DocSupportIncident),
							DefaultUnsubscriberEmailTemplateSubject,
							DefaultUnsubscriberEmailTemplateMessage);
						return registryItem;
					});
			}
		}

		public const string UnsubscriberEmailTemplateHint = @"Configure the template for unsubscriber notification emails.";

		public const string DefaultUnsubscriberEmailTemplateSubject = @"New Messages in (*IncidentDetailedDescription*)";

		public const string DefaultUnsubscriberEmailTemplateMessage = @"You have been unsubscribed from (*IncidentNumber*).";

		#endregion

		#endregion

		#region Internal Incident Licence Settings

		public InternalIncidentLicenceSettingsRegistryItem InternalIncidentLicenceSettings
		{
			get
			{
				return GetItem("InternalIncidentLicenceSettings", delegate
				{
					return new InternalIncidentLicenceSettingsRegistryItem(CustomerServiceSubCategory);
				});
			}
		}

		#region Relationship Manager Company Lookup

		public CodeDescriptionPairListRegistryItem RelationshipManagerCompanyLookup
		{
			get
			{
				CodeDescriptionPairList defaultValue = new CodeDescriptionPairList();
				defaultValue.AddPair("AUS", "EDI");
				defaultValue.AddPair("BRE", "LHR");
				defaultValue.AddPair("DEM", "EDI");
				defaultValue.AddPair("EDI", "EDI");
				defaultValue.AddPair("EDN", "EDI");
				defaultValue.AddPair("EUR", "LHR");
				defaultValue.AddPair("HNK", "EDI");
				defaultValue.AddPair("JNB", "LHR");
				defaultValue.AddPair("LHR", "LHR");
				defaultValue.AddPair("MAY", "EDI");
				defaultValue.AddPair("MIS", "USA");
				defaultValue.AddPair("MUP", "USA");
				defaultValue.AddPair("SIN", "EDI");
				defaultValue.AddPair("USA", "USA");

				return GetItem(
					"RelationshipManagerCompanyLookup", delegate
					{
						return new CodeDescriptionPairListRegistryItem(
							"RelationshipManagerCompanyLookup",
							(NoResString)"Relationship Manager Company Lookup",
							(NoResString)"The lookup table for displaying relationship managers on support incident screen.\r\nThe Code column is the company code of staff home branch, the Description column is which company code should be used to lookup relationship manager assignment.",
							3,
							RegistryStorageFlags.System,
							false,
							defaultValue,
							(NoResString)CustomerServiceSubCategory
						);
					}
				);
			}
		}

		#endregion

		#endregion

		#region Incident Product Areas

		public CodeDescriptionPairListRegistryItem ProductAreas
		{
			get
			{
				return GetItem("ProductAreas",
					delegate
					{
						CodeDescriptionPairList productAreaList = new ProductAreaList();

						var item = new CodeDescriptionPairListRegistryItem("ProductAreas",
							(NoResString)CustomerServiceSubCategory,
							(NoResString)"Incident Product Areas",
							(NoResString)"The list of product areas for Incident.",
							3,
							RegistryStorageFlags.System,
							false,
							productAreaList);
						((CodeDescriptionPairListRegistryDataType)item.DataType).AllowDuplicateCodes = false;
						return item;
					});
			}
		}

		#endregion

		#region Content Finder URL

		public StringRegistryItem ContentFinderUrl
		{
			get
			{
				return GetItem("ContentFinderUrl",
					delegate
					{
						var caption = ResString.GetMultilingualString("0816b2a7-d336-155f-8540-e8247f8c9230", "Content Finder Web App");
						var webUriHint = ResString.GetMultilingualString("97031960-e623-a731-db5f-e43b5788b2b8", @"Specify the root URL for {0}.
Please keep this in sync with your Web Server settings for {0}.", caption);

						var sri = new StringRegistryItem(
						"ContentFinderUrl",
						(NoResString)CustomerServiceSubCategory,
						ResString.GetMultilingualString("f926fa90-e295-6402-5b21-8d62679df8d4", "Content Finder URL"),
						webUriHint,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.IsValueMandatory,
						"https://ist.wtg.zone");
						sri.DataType = new UriRegistryDataType(Uri.UriSchemeHttps) { AllowAutoProtocolPrefixing = false };
						return sri;
					});
			}
		}

		#endregion

		#region Enable Content Finder

		public BooleanRegistryItem EnableContentFinder
		{
			get
			{
				return GetItem("EnableContentFinder", () =>
				{
					return new BooleanRegistryItem(
						"EnableContentFinder",
						(NoResString)CustomerServiceSubCategory,
						ResString.GetMultilingualString("4f8261fd-fd96-8490-d0f7-c2d5b8995dd1", "Enable Content Finder"),
						ResString.GetMultilingualString("502f0b2f-af86-907f-4e6c-e4b570c932cd", "When this registry is set to YES, the 'Launch Content Finder' Action button in the Incident form is enabled. The IST base URL can be set in Content Finder URL registry."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue: false);
				});
			}
		}

		#endregion

		#region Module Mappings

		public const string ModuleMappingsSubCategory = CustomerServiceSubCategory + "/Module Mappings";

		public SystemProductRegistryItem SystemProductMappings
		{
			get
			{
				return GetItem("SystemProductMappings",
					delegate
					{
						return new SystemProductRegistryItem("SystemProductMappings",
							(NoResString)ModuleMappingsSubCategory,
							(NoResString)"Products and Product Area Incident Menu Section Mappings",
							(NoResString)@"New products can be added on this registry and will become available as a selection on the eRequest Management Portal as well as adding a Product on the Organization > Licence > Databases.

Mapping list between product, incident product area and menu sections.
Each menu section mapping can have submappings that allows the product area to be overriden for specific menu items.

New menu sections can also be added with the mappings.",
							new SystemProductRegistryEditorInfo(ModuleListType.MenuSection, true),
							RegistryStorageFlags.System,
							RegistryDefaultsHelper.GetEnterpriseDefaults());
					});
			}
		}

		public SystemProductRegistryItem ProductAreaIncidentCr8Mappings
		{
			get
			{
				return GetItem("SystemProductAreaIncidentCr8Mappings",
					delegate
					{
						return new SystemProductRegistryItem("SystemProductAreaIncidentCr8Mappings",
							(NoResString)ModuleMappingsSubCategory,
							(NoResString)"Product Area Incident CR8 Module Mappings",
							(NoResString)@"Mapping list between product, incident product area and CR8 modules.
New CR8 modules can also be added with the mappings.",
							new SystemProductRegistryEditorInfo(ModuleListType.Cr8, true),
							RegistryStorageFlags.System,
							RegistryDefaultsHelper.GetCr8Defaults());
					});
			}
		}

		public SystemProductRegistryItem ProductAreaIncidentCr9Mappings
		{
			get
			{
				return GetItem("SystemProductAreaIncidentCr9Mappings",
					delegate
					{
						return new SystemProductRegistryItem("SystemProductAreaIncidentCr9Mappings",
							(NoResString)ModuleMappingsSubCategory,
							(NoResString)"Product Area Incident CR9 Module Mappings",
							(NoResString)@"Mapping list between product, incident product area and CR9 modules.
New CR9 modules can also be added with the mappings.",
							new SystemProductRegistryEditorInfo(ModuleListType.Cr9, true),
							RegistryStorageFlags.System,
							RegistryDefaultsHelper.GetCr9Defaults());
					});
			}
		}

		public static SystemProductRegistryItem GetProductAreaModuleMappingsRegistryItem(ModuleListType moduleListType)
		{
			switch (moduleListType)
			{
				case ModuleListType.MenuSection:
					return Instance.SystemProductMappings;

				case ModuleListType.Cr8:
					return Instance.ProductAreaIncidentCr8Mappings;

				case ModuleListType.Cr9:
					return Instance.ProductAreaIncidentCr9Mappings;

				default:
					return null;
			}
		}

		#region Legacy Menu Section Mappings

		public LegacyModuleMappingsRegistryItem LegacyMenuSectionMappings
		{
			get
			{
				return GetItem("LegacyMenuSectionMappings",
					delegate
					{
						return new LegacyModuleMappingsRegistryItem("LegacyMenuSectionMappings",
							(NoResString)ModuleMappingsSubCategory,
							(NoResString)"Legacy Menu Section Mappings",
							(NoResString)@"Mapping list between legacy and current incident menu sections.",
							new LegacyModuleMappingsRegistryEditorInfo("Menu Section Mapping"),
							RegistryStorageFlags.System,
							GetDefaultLegacyMenuSectionMappings());
					});
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		static LegacyModuleMappingCollection GetDefaultLegacyMenuSectionMappings()
		{
			var collection = new LegacyModuleMappingCollection(ModuleListType.MenuSection);
			collection.AddNew("CQW", "ChequeWriter", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Account);
			collection.AddNew("CGB", "Gateway Billing", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Account);
			collection.AddNew("AGC", "GL Consolidations", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Account);
			collection.AddNew("INV", "Invoicing", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Account);
			collection.AddNew("WCF", "ediWebTracker CFS Manager", "", ModuleTreeCustomerServiceMenuSectionList.Codes.CfsCto);
			collection.AddNew("ESB", "eSB - US Customs", "", ModuleTreeCustomerServiceMenuSectionList.Codes.EServices, "US");
			collection.AddNew("EMF", "ExportManifest (CRN)", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs);
			collection.AddNew("IMF", "ImportManifest (ACA F, SCA F)", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs);
			collection.AddNew("ACR", "AirCargoReport", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "AU");
			collection.AddNew("SCR", "SeaCargoReport", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "AU");
			collection.AddNew("IAC", "ImportAirCargo", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "AU");
			collection.AddNew("ISC", "ImportSeaCargo", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "AU");
			collection.AddNew("UKC", "AirCcsukBase", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "GB");
			collection.AddNew("UKA", "AirCcsuk", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "GB");
			collection.AddNew("UKS", "AirCcsukShed", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "GB");
			collection.AddNew("UKD", "AirCcsukDEP", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "GB");
			collection.AddNew("HLC", "HVLVClearance (AU)", "", MandatoryCustomerServiceMenuSectionList.Codes.Etail);
			collection.AddNew("BRK", "Broker", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs);
			collection.AddNew("CCM", "CoreCustomsModule", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs);
			collection.AddNew("XBR", "ExportBroker", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs);
			collection.AddNew("IBR", "ImportBroker", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs);
			collection.AddNew("LDC", "LandedCosting", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs);
			collection.AddNew("DRB", "Drawback", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs);
			collection.AddNew("IQM", "ImportQuarantine (eBACCa)", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "NZ");
			collection.AddNew("RNS", "ReleaseNotificationSystem", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "CA");
			collection.AddNew("ISF", "ImporterSecurityFiling", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "US");
			collection.AddNew("SCD", "SeaCargoCFSCustoms", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "AU");
			collection.AddNew("ACD", "AirCargoCFSCustoms", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "AU");
			collection.AddNew("WBI", "ediWebTracker Import Brokerage", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs);
			collection.AddNew("WBE", "ediWebTracker Export Brokerage", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs);
			collection.AddNew("ACF", "Broker AU - Air Cargo CFS Customs", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "AU");
			collection.AddNew("AIR", "Broker AU - Air Cargo Automation", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "AU");
			collection.AddNew("BAE", "Broker AE", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "AE");
			collection.AddNew("BAU", "Broker AU", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "AU");
			collection.AddNew("BCA", "Broker CA", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "CA");
			collection.AddNew("BDE", "Broker DE", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "DE");
			collection.AddNew("BGB", "Broker GB", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "GB");
			collection.AddNew("BHK", "Broker HK", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "HK");
			collection.AddNew("BIN", "Broker IN", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "IN");
			collection.AddNew("BJP", "Broker JP", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "JP");
			collection.AddNew("BMY", "Broker MY", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "MY");
			collection.AddNew("BNZ", "Broker NZ", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "NZ");
			collection.AddNew("BRB", "Broker - Shared", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs);
			collection.AddNew("BSG", "Broker SG", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "SG");
			collection.AddNew("BUS", "Broker US", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "US");
			collection.AddNew("BZA", "Broker ZA", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "ZA");
			collection.AddNew("SCC", "Broker AU - Sea Cargo CFS Customs", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "AU");
			collection.AddNew("SEA", "Broker AU - Sea Cargo Automation", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "AU");
			collection.AddNew("USI", "US Domestic", "", ModuleTreeCustomerServiceMenuSectionList.Codes.CustomsUs);
			collection.AddNew("TAR", "ediTariff", "", MandatoryCustomerServiceMenuSectionList.Codes.Editariff);
			collection.AddNew("TBU", "ediTariff Bulk Updater", "", ModuleTreeCustomerServiceMenuSectionList.Codes.CustomsFiles);
			collection.AddNew("TAU", "AU ediTariff", "", MandatoryCustomerServiceMenuSectionList.Codes.Editariff, "AU");
			collection.AddNew("TCA", "CA ediTariff", "", MandatoryCustomerServiceMenuSectionList.Codes.Editariff, "CA");
			collection.AddNew("TEU", "EU ediTariff", "", MandatoryCustomerServiceMenuSectionList.Codes.Editariff);
			collection.AddNew("TNZ", "NZ ediTariff", "", MandatoryCustomerServiceMenuSectionList.Codes.Editariff, "NZ");
			collection.AddNew("TUK", "UK ediTariff", "", MandatoryCustomerServiceMenuSectionList.Codes.Editariff, "GB");
			collection.AddNew("TUS", "US ediTariff", "", MandatoryCustomerServiceMenuSectionList.Codes.Editariff, "US");
			collection.AddNew("DSS", "DocManager ScanningStation", "", ModuleTreeCustomerServiceMenuSectionList.Codes.DocManager);
			collection.AddNew("DOC", "DocumentEngine", "", ModuleTreeCustomerServiceMenuSectionList.Codes.DocManager);
			collection.AddNew("EXD", "ExportDocuments", "", ModuleTreeCustomerServiceMenuSectionList.Codes.EServices);
			collection.AddNew("DCS", "eServices - Distance Calculator", "", ModuleTreeCustomerServiceMenuSectionList.Codes.EServices);
			collection.AddNew("DWZ", "DataWizard", "", ModuleTreeCustomerServiceMenuSectionList.Codes.EServices);
			collection.AddNew("EAS", "eAdaptor SDK - OBSOLETE", "", ModuleTreeCustomerServiceMenuSectionList.Codes.EServices);
			collection.AddNew("EAC", "eAdaptor", "", ModuleTreeCustomerServiceMenuSectionList.Codes.EServices);
			collection.AddNew("ESA", "eServices - Airline Messaging", "", ModuleTreeCustomerServiceMenuSectionList.Codes.EServices);
			collection.AddNew("ESC", "eServices - Customs Service Bureau", "", ModuleTreeCustomerServiceMenuSectionList.Codes.EServices);
			collection.AddNew("ESR", "eServices - Rail Container Messaging", "", ModuleTreeCustomerServiceMenuSectionList.Codes.EServices);
			collection.AddNew("ADP", "eAdaptor", "", ModuleTreeCustomerServiceMenuSectionList.Codes.EServices);
			collection.AddNew("AMG", "Air Messaging", "", ModuleTreeCustomerServiceMenuSectionList.Codes.EServices);
			collection.AddNew("BIL", "Billing", "", ModuleTreeCustomerServiceMenuSectionList.Codes.EServices);
			collection.AddNew("PRA", "PRA Messaging", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Forwarding);
			collection.AddNew("PRT", "PRA Messaging (Per Transaction)", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Forwarding);
			collection.AddNew("DPS", "eServices - Denied Party Screening", "", MandatoryCustomerServiceMenuSectionList.Codes.Eservices);
			collection.AddNew("FCP", "Forwarder - Containers Packing", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Forwarding);
			collection.AddNew("FPM", "Forwarder - Port Messaging", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Forwarding);
			collection.AddNew("MFT", "Manifest", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs);
			collection.AddNew("ACI", "ACIReporting - OBSOLETE", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "CA");
			collection.AddNew("ACP", "ACIReporting (Per Transaction)", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "CA");
			collection.AddNew("ACE", "ACIeManifestReporting (Per Transaction)", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "CA");
			collection.AddNew("AMS", "AMSReporting", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "US");
			collection.AddNew("STW", "StowPlanReporting", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "US");
			collection.AddNew("CMD", "CMDReporting", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "SG");
			collection.AddNew("MAN", "Manifest (US e-Manifest)", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "US");
			collection.AddNew("BOO", "Booking / Spot Quotes", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Forwarding);
			collection.AddNew("HVS", "HVSO", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Forwarding);
			collection.AddNew("ACT", "ImportAirCTOReport", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs);
			collection.AddNew("EXT", "ExportAirCTOReport", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs);
			collection.AddNew("WFO", "ediWebTracker Forwarding", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Forwarding);
			collection.AddNew("WBO", "ediWebTracker Booking / Spot Quotes", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Forwarding);
			collection.AddNew("ECI", "Cargo 2000 Phase 1", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Forwarding);
			collection.AddNew("EC2", "Cargo 2000 Phase 2", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Forwarding);
			collection.AddNew("LNG", "Language Packs", "", MandatoryCustomerServiceMenuSectionList.Codes.Language);
			collection.AddNew("GZH", "Simplified Chinese Language Pack", "", MandatoryCustomerServiceMenuSectionList.Codes.Language);
			collection.AddNew("GZT", "Traditional Chinese Language Pack", "", MandatoryCustomerServiceMenuSectionList.Codes.Language);
			collection.AddNew("GFR", "French Language Pack", "", MandatoryCustomerServiceMenuSectionList.Codes.Language);
			collection.AddNew("GDE", "German Language Pack", "", MandatoryCustomerServiceMenuSectionList.Codes.Language);
			collection.AddNew("GEA", "Spanish Latin Language Pack", "", MandatoryCustomerServiceMenuSectionList.Codes.Language);
			collection.AddNew("GES", "Spanish Language Pack", "", MandatoryCustomerServiceMenuSectionList.Codes.Language);
			collection.AddNew("GIT", "Italian Language Pack", "", MandatoryCustomerServiceMenuSectionList.Codes.Language);
			collection.AddNew("GJA", "Japanese Language Pack", "", MandatoryCustomerServiceMenuSectionList.Codes.Language);
			collection.AddNew("GPT", "Portuguese Language Pack", "", MandatoryCustomerServiceMenuSectionList.Codes.Language);
			collection.AddNew("GPB", "Portuguese - Brazil Language Pack", "", MandatoryCustomerServiceMenuSectionList.Codes.Language);
			collection.AddNew("GUK", "Ukrainian Language Pack", "", MandatoryCustomerServiceMenuSectionList.Codes.Language);
			collection.AddNew("GRU", "Russian Language Pack", "", MandatoryCustomerServiceMenuSectionList.Codes.Language);
			collection.AddNew("GAR", "Arabic Language Pack", "", MandatoryCustomerServiceMenuSectionList.Codes.Language);
			collection.AddNew("GBG", "Bulgarian Language Pack", "", MandatoryCustomerServiceMenuSectionList.Codes.Language);
			collection.AddNew("GCS", "Czech Language Pack", "", MandatoryCustomerServiceMenuSectionList.Codes.Language);
			collection.AddNew("GDA", "Danish Language Pack", "", MandatoryCustomerServiceMenuSectionList.Codes.Language);
			collection.AddNew("GNL", "Dutch Language Pack", "", MandatoryCustomerServiceMenuSectionList.Codes.Language);
			collection.AddNew("GFI", "Finnish Language Pack", "", MandatoryCustomerServiceMenuSectionList.Codes.Language);
			collection.AddNew("GEL", "Greek Language Pack", "", MandatoryCustomerServiceMenuSectionList.Codes.Language);
			collection.AddNew("GHU", "Hungarian Language Pack", "", MandatoryCustomerServiceMenuSectionList.Codes.Language);
			collection.AddNew("GKO", "Korean Language Pack", "", MandatoryCustomerServiceMenuSectionList.Codes.Language);
			collection.AddNew("GNB", "Norwegian Language Pack", "", MandatoryCustomerServiceMenuSectionList.Codes.Language);
			collection.AddNew("GPL", "Polish Language Pack", "", MandatoryCustomerServiceMenuSectionList.Codes.Language);
			collection.AddNew("GRO", "Romanian Language Pack", "", MandatoryCustomerServiceMenuSectionList.Codes.Language);
			collection.AddNew("GSV", "Swedish Language Pack", "", MandatoryCustomerServiceMenuSectionList.Codes.Language);
			collection.AddNew("GTR", "Turkish Language Pack", "", MandatoryCustomerServiceMenuSectionList.Codes.Language);
			collection.AddNew("GVI", "Vietnamese Language Pack", "", MandatoryCustomerServiceMenuSectionList.Codes.Language);
			collection.AddNew("TRF", "Translation Feedback", "", MandatoryCustomerServiceMenuSectionList.Codes.Language);
			collection.AddNew("LON", "Vehicle Monitoring and Management", "", ModuleTreeCustomerServiceMenuSectionList.Codes.LocalTransport);
			collection.AddNew("EMT", "MobileTransport", "", ModuleTreeCustomerServiceMenuSectionList.Codes.LocalTransport);
			collection.AddNew("WLO", "ediWebTracker Local Transport", "", ModuleTreeCustomerServiceMenuSectionList.Codes.LocalTransport);
			collection.AddNew("WOR", "ediWebTracker Order Manager", "", ModuleTreeCustomerServiceMenuSectionList.Codes.OrderManager);
			collection.AddNew("CPJ", "Client Projects", "", ModuleTreeCustomerServiceMenuSectionList.Codes.EHubInterfaces);
			collection.AddNew("RPL", "Replenishment", "", MandatoryCustomerServiceMenuSectionList.Codes.Other);
			collection.AddNew("ELA", "eLearning Accreditation", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Recruiter);
			collection.AddNew("ORG", "ReferenceFiles - Organisations", "", ModuleTreeCustomerServiceMenuSectionList.Codes.ReferenceFiles);
			collection.AddNew("AWB", "Air Waybill", "", ModuleTreeCustomerServiceMenuSectionList.Codes.ReferenceFiles);
			collection.AddNew("RAT", "Rating", "", ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing);
			collection.AddNew("QTE", "SalesMarketing Quotations", "", ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing);
			collection.AddNew("CLR", "SalesMarketing Client Rates & Comp Tariffs", "", ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing);
			collection.AddNew("CTF", "SalesMarketing Company Tariffs - OBSOLETE", "", ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing);
			collection.AddNew("COS", "SalesMarketing Costings", "", ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing);
			collection.AddNew("COL", "SalesMarketing Cold Call Register", "", ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing);
			collection.AddNew("INQ", "Inquiry Manager", "", ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing);
			collection.AddNew("CMM", "Communication Manager", "", ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing);
			collection.AddNew("OPP", "SalesMarketing Opportunity Manager", "", ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing);
			collection.AddNew("CAM", "SalesMarketing Campaign Manager", "", ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing);
			collection.AddNew("CCI", "SalesMarketing Client & Competitor Intelligence", "", ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing);
			collection.AddNew("SDB", "Sales Dashboard", "", ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing);
			collection.AddNew("SOP", "SalesMarketing Operations Management", "", ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing);
			collection.AddNew("SOF", "SalesMarketing Operations Management - Forwarder", "", ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing);
			collection.AddNew("SOQ", "SalesMarketing Operations Management - Booking / Spot Quotes", "", ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing);
			collection.AddNew("SOO", "SalesMarketing Operations Management - Order Manager", "", ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing);
			collection.AddNew("SOT", "SalesMarketing Operations Management - Local Transport", "", ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing);
			collection.AddNew("SOB", "SalesMarketing Operations Management - Broker", "", ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing);
			collection.AddNew("SOW", "SalesMarketing Operations - Warehouse", "", ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing);
			collection.AddNew("VIM", "ComTracVesselIntegration", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Schedules);
			collection.AddNew("DIM", "DakosyVesselIntegration", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Schedules);
			collection.AddNew("HIM", "DBHVesselIntegration", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Schedules);
			collection.AddNew("VAU", "ComTracAUContainerFeeds", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Schedules);
			collection.AddNew("VNZ", "ComTracNZContainerFeeds", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Schedules);
			collection.AddNew("RCT", "Rail Container Tracing", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Schedules);
			collection.AddNew("RSH", "eServices - Online Airline Schedules", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Schedules);
			collection.AddNew("CON", "ContainerManager", "", ModuleTreeCustomerServiceMenuSectionList.Codes.LinerAndAgency);
			collection.AddNew("SMD", "ShippingManager Bill of Lading", "", ModuleTreeCustomerServiceMenuSectionList.Codes.LinerAndAgency);
			collection.AddNew("SPA", "ShippingManager Port Authority Messaging", "", ModuleTreeCustomerServiceMenuSectionList.Codes.LinerAndAgency);
			collection.AddNew("SPT", "ShippingManager Port Authority Messaging (Per Transaction)", "", ModuleTreeCustomerServiceMenuSectionList.Codes.LinerAndAgency);
			collection.AddNew("SDO", "ShippingManager E-IDO Messaging", "", ModuleTreeCustomerServiceMenuSectionList.Codes.LinerAndAgency);
			collection.AddNew("SDT", "ShippingManager E-IDO Messaging (Per Transaction)", "", ModuleTreeCustomerServiceMenuSectionList.Codes.LinerAndAgency);
			collection.AddNew("SMC", "ShippingManager ContainerControl", "", ModuleTreeCustomerServiceMenuSectionList.Codes.LinerAndAgency);
			collection.AddNew("SME", "ShippingManager Container Detention", "", ModuleTreeCustomerServiceMenuSectionList.Codes.LinerAndAgency);
			collection.AddNew("SVA", "ShippingManager Voyage Accounting", "", ModuleTreeCustomerServiceMenuSectionList.Codes.LinerAndAgency);
			collection.AddNew("SSC", "ShippingManager Sundry Charges", "", ModuleTreeCustomerServiceMenuSectionList.Codes.LinerAndAgency);
			collection.AddNew("SMB", "ShippingManager Bookings", "", ModuleTreeCustomerServiceMenuSectionList.Codes.LinerAndAgency);
			collection.AddNew("SED", "ShippingManager CustomsExportManifest", "", ModuleTreeCustomerServiceMenuSectionList.Codes.LinerAndAgency);
			collection.AddNew("SID", "ShippingManager CustomsImportManifest", "", ModuleTreeCustomerServiceMenuSectionList.Codes.LinerAndAgency);
			collection.AddNew("WSB", "ediWebTracker Shipping Bookings", "", ModuleTreeCustomerServiceMenuSectionList.Codes.LinerAndAgency);
			collection.AddNew("WSL", "ediWebTracker Shipping Bills of Lading", "", ModuleTreeCustomerServiceMenuSectionList.Codes.LinerAndAgency);
			collection.AddNew("COR", "Core", "", ModuleTreeCustomerServiceMenuSectionList.Codes.System);
			collection.AddNew("REP", "ReportWriter", "", ModuleTreeCustomerServiceMenuSectionList.Codes.System);
			collection.AddNew("BCL", "BarCodeLabels", "", ModuleTreeCustomerServiceMenuSectionList.Codes.System);
			collection.AddNew("FAX", "FaxEngine", "", ModuleTreeCustomerServiceMenuSectionList.Codes.System);
			collection.AddNew("IFC", "InterfaceConnector", "", ModuleTreeCustomerServiceMenuSectionList.Codes.System);
			collection.AddNew("NXC", "NativeXMLConnector", "", MandatoryCustomerServiceMenuSectionList.Codes.Eservices);
			collection.AddNew("RDC", "Remote Desktop Connector", "", ModuleTreeCustomerServiceMenuSectionList.Codes.System);
			collection.AddNew("UCP", "Universal Copy", "", ModuleTreeCustomerServiceMenuSectionList.Codes.System);
			collection.AddNew("ARC", "Architecture", "", ModuleTreeCustomerServiceMenuSectionList.Codes.System);
			collection.AddNew("BAK", "Batch Processor - Backup", "", ModuleTreeCustomerServiceMenuSectionList.Codes.System);
			collection.AddNew("CIM", "Cargo Interchange Messaging", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Forwarding);
			collection.AddNew("EML", "Batch Processor - Email", "", ModuleTreeCustomerServiceMenuSectionList.Codes.System);
			collection.AddNew("FSV", "FaxService", "", ModuleTreeCustomerServiceMenuSectionList.Codes.System);
			collection.AddNew("GEO", "GEO Compliance", "", ModuleTreeCustomerServiceMenuSectionList.Codes.System);
			collection.AddNew("GRA", "Graphics", "", ModuleTreeCustomerServiceMenuSectionList.Codes.System);
			collection.AddNew("HCD", "Client - Desktop", "", MandatoryCustomerServiceMenuSectionList.Codes.Hosting);
			collection.AddNew("HCI", "Client - Internet", "", MandatoryCustomerServiceMenuSectionList.Codes.Hosting);
			collection.AddNew("HCP", "Client - Printing", "", MandatoryCustomerServiceMenuSectionList.Codes.Hosting);
			collection.AddNew("HCW", "Client - Password", "", MandatoryCustomerServiceMenuSectionList.Codes.Hosting);
			collection.AddNew("HWF", "CW - Infrastructure", "", MandatoryCustomerServiceMenuSectionList.Codes.Hosting);
			collection.AddNew("HWI", "CW - Installation", "", MandatoryCustomerServiceMenuSectionList.Codes.Hosting);
			collection.AddNew("HWP", "CW - Process Controller", "", MandatoryCustomerServiceMenuSectionList.Codes.Hosting);
			collection.AddNew("HWT", "CW - Internet", "", MandatoryCustomerServiceMenuSectionList.Codes.Hosting);
			collection.AddNew("HYL", "Hybrid Licensing", "", ModuleTreeCustomerServiceMenuSectionList.Codes.System);
			collection.AddNew("IIT", "Infrastructure", "", MandatoryCustomerServiceMenuSectionList.Codes.Installation);
			collection.AddNew("IMG", "Messaging", "", MandatoryCustomerServiceMenuSectionList.Codes.Installation);
			collection.AddNew("IMI", "Migration", "", MandatoryCustomerServiceMenuSectionList.Codes.Installation);
			collection.AddNew("IPE", "Performance", "", MandatoryCustomerServiceMenuSectionList.Codes.Installation);
			collection.AddNew("IRD", "Client - RDC Installation", "", MandatoryCustomerServiceMenuSectionList.Codes.Installation);
			collection.AddNew("ISP", "Setup", "", MandatoryCustomerServiceMenuSectionList.Codes.Installation);
			collection.AddNew("IUP", "Upgrades", "", MandatoryCustomerServiceMenuSectionList.Codes.Installation);
			collection.AddNew("LIC", "Licensing", "", ModuleTreeCustomerServiceMenuSectionList.Codes.System);
			collection.AddNew("PCT", "Process Controllers", "", ModuleTreeCustomerServiceMenuSectionList.Codes.System);
			collection.AddNew("PRN", "Batch Processor - Print", "", ModuleTreeCustomerServiceMenuSectionList.Codes.System);
			collection.AddNew("UNI", "Universal XML", "", MandatoryCustomerServiceMenuSectionList.Codes.Eservices);
			collection.AddNew("UPG", "Upgrade Assurance", "", MandatoryCustomerServiceMenuSectionList.Codes.EhubInterfaces);
			collection.AddNew("WCU", "Web Site Content Update", "", ModuleTreeCustomerServiceMenuSectionList.Codes.System);
			collection.AddNew("BDS", "BondedWarehouse", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Customs);
			collection.AddNew("WDF", "WarehouseManager Operations & 3PL", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Warehouse);
			collection.AddNew("SPM", "ScanPackManager", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Warehouse);
			collection.AddNew("WWA", "ediWebTracker Warehouse", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Warehouse);
			collection.AddNew("RFM", "RFScannerManager", "", ModuleTreeCustomerServiceMenuSectionList.Codes.Warehouse);
			collection.AddNew("BMS", "BufferManagement", "", ModuleTreeCustomerServiceMenuSectionList.Codes.BufferManagement);

			return collection;
		}

		#endregion

		#region Legacy CR8 Module Mappings

		public LegacyModuleMappingsRegistryItem LegacyCr8ModuleMappings
		{
			get
			{
				return GetItem("LegacyCr8ModuleMappings",
					delegate
					{
						return new LegacyModuleMappingsRegistryItem("LegacyCr8ModuleMappings",
							(NoResString)ModuleMappingsSubCategory,
							(NoResString)"Legacy CR8 Module Mappings",
							(NoResString)@"Mapping list between legacy and current incident CR8 modules.",
							new LegacyModuleMappingsRegistryEditorInfo("Requirement Mapping"),
							RegistryStorageFlags.System,
							GetDefaultLegacyCr8Mappings());
					});
			}
		}

		static LegacyModuleMappingCollection GetDefaultLegacyCr8Mappings()
		{
			var collection = new LegacyModuleMappingCollection(ModuleListType.Cr8);
			collection.AddNew("ALL", "All Items", "", Cr8ModuleList.Codes.OtherComplianceIssue);
			collection.AddNew("UIL", "UI/Web/Docs - Language", "", Cr8ModuleList.Codes.TranslationIssueUiWebDocs);
			return collection;
		}

		#endregion

		#region Legacy CR9 Module Mappings

		public LegacyModuleMappingsRegistryItem LegacyCr9ModuleMappings
		{
			get
			{
				return GetItem("LegacyCr9ModuleMappings",
					delegate
					{
						return new LegacyModuleMappingsRegistryItem("LegacyCr9ModuleMappings",
							(NoResString)ModuleMappingsSubCategory,
							(NoResString)"Legacy CR9 Module Mappings",
							(NoResString)@"Mapping list between legacy and current incident CR9 modules.",
							new LegacyModuleMappingsRegistryEditorInfo("Service Mapping"),
							RegistryStorageFlags.System,
							GetDefaultLegacyCr9Mappings());
					});
			}
		}

		static LegacyModuleMappingCollection GetDefaultLegacyCr9Mappings()
		{
			var collection = new LegacyModuleMappingCollection(ModuleListType.Cr9);
			collection.AddNew("ALL", "All Items", "", Cr9ModuleList.Codes.OtherConsultingPleaseDescribeClearly);
			collection.AddNew("LKY", "License Keys", "", Cr9ModuleList.Codes.LicensingMaintenanceUpdates);
			collection.AddNew("NHU", "New Hosting Users", "", Cr9ModuleList.Codes.WisecloudLoginAndOrPasswordRequests);
			collection.AddNew("RHP", "Reset Hosting Password", "", Cr9ModuleList.Codes.WisecloudLoginAndOrPasswordRequests);
			collection.AddNew("SUD", "System Updates", "", Cr9ModuleList.Codes.MasterDataTakeOn);
			collection.AddNew("TDB", "Development Training on DocBuilder", "", Cr9ModuleList.Codes.ConsultingAssistanceOnDocbuilderCustomization);
			collection.AddNew("TEA", "Development Training on eAdaptor", "", Cr9ModuleList.Codes.ConsultingAssistanceOnEadaptor);
			collection.AddNew("TRW", "Development Training on Report Writer", "", Cr9ModuleList.Codes.ConsultingAssistanceOnReportCustomization);
			return collection;
		}

		#endregion

		#region WebSecurity to ProductModule Mappings

		public WebSecurityMappingRegistryItem WebSecurityProductModuleMappings
		{
			get
			{
				return GetItem("WebSecurityProductModuleMappings", delegate
				{
					var defaultCollection = new WebSecurityMappingCollection();
					var internalModules = SystemProductMappings.Value.GetModuleList(ProductTypes.Codes.WTGInternal);
					if (internalModules.Count > 0)
					{
						const string codeWCE = "WCE";
						const string codeMFA = "MFA";
						if (internalModules.ContainsCode(codeWCE))
						{
							defaultCollection.AddNew(EDIWebSecurityRightsList.WiseBusinessPartner.Code, ProductTypes.Codes.WTGInternal, codeWCE);
						}
						if (internalModules.ContainsCode(codeMFA))
						{
							defaultCollection.AddNew(EDIWebSecurityRightsList.WiseBusinessPartner.Code, ProductTypes.Codes.WTGInternal, codeMFA);
						}
					}

					return new WebSecurityMappingRegistryItem(
					(NoResString)"WebSecurityProductModuleMappings",
					(NoResString)ModuleMappingsSubCategory,
					(NoResString)"Web Securities to Products / Modules Mappings",
					(NoResString)"The list of additional Products/Modules available to the user based on the Web Securities.",
					new WebSecurityMappingRegistryEditorInfo(),
					RegistryStorageFlags.System,
					defaultCollection);
				});
			}
		}

		#endregion

		#endregion

		#region Source Module

		public SourceModulesRegistryItem SourceModules
		{
			get
			{
				return GetItem("SourceModules",
					delegate
					{
						var defaultCollection = new SourceModuleCollection();
						foreach (ModuleCategory category in ModuleTree.Tree.Categories.Values)
						{
							foreach (ModuleSection section in category.Sections.Values)
							{
								foreach (IMainFormModule module in section.Modules.Values)
								{
									defaultCollection.AddNew(module, true, true, ProductTypes.Codes.Enterprise);
								}
							}
						}

						foreach (var licence in new LegacyLicence().GetAllModuleCheckpoints())
						{
							defaultCollection.AddNew(licence.Name, licence.DisplayName, "[Licence]", "", false, true, ProductTypes.Codes.Enterprise);
						}

						defaultCollection.AddNew("ACF", "ediBroker AU - Air Cargo CFS Customs", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("ADP", "eAdaptor", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("AIR", "ediBroker AU - Air Cargo Automation", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("AMG", "Air Messaging", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("ARC", "Architecture", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("AWB", "Air Waybill", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("BAE", "ediBroker AE", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("BAK", "Batch Processor - Backup", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("BAU", "ediBroker AU", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("BCA", "ediBroker CA", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("BDE", "ediBroker DE", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("BGB", "ediBroker GB", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("BHK", "ediBroker HK", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("BIL", "Billing", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("BIN", "ediBroker IN", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("BJP", "ediBroker JP", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("BMY", "ediBroker MY", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("BNZ", "ediBroker NZ", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("BRB", "ediBroker - Shared", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("BSG", "ediBroker SG", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("BUS", "ediBroker US", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("BZA", "ediBroker ZA", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("CIM", "Cargo Interchange Messaging", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("CPJ", "Client Projects", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("DOC", "ediDocumentEngine", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("ELA", "eLearning Accreditation", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("EML", "Batch Processor - Email", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("ESB", "eSB - US Customs", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("FSV", "ediFaxService", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("GEO", "GEO Compliance", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("GRA", "Graphics", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("HCD", "Client - Desktop", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("HCI", "Client - Internet", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("HCP", "Client - Printing", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("HCW", "Client - Password", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("HOS", "Obsolete - Hosting", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("HWF", "CW - Infrastructure", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("HWI", "CW - Installation", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("HWP", "CW - Process Controller", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("HWT", "CW - Internet", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("HYL", "Hybrid Licensing", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("IIT", "Infrastructure", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("IMG", "Messaging", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("IMI", "Migration", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("INC", "Internal - ediIncidentManager", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("INT", "Internal Development", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("INV", "Invoicing", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("IPE", "Performance", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("IRD", "Client - RDC Installation", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("ISP", "Setup", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("IUP", "Upgrades", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("JRB", "Job Related Billing", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("LAN", "Language", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("LIC", "Licensing", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("ORG", "ediReferenceFiles - Organisations", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("OTH", "Other", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("PCT", "Process Controllers", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("PRN", "Batch Processor - Print", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("RAT", "Rating", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("RCT", "Rail Container Tracing", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("REF", "ediReferenceFiles - Other", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("RPL", "Replenishment", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("SCC", "ediBroker AU - Sea Cargo CFS Customs", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("SCH", "ediSchedules", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("SEA", "ediBroker AU - Sea Cargo Automation", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("SOW", "ediSalesMarketing Operations - Warehouse", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("TAU", "AU ediTariff", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("TCA", "CA ediTariff", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("TEU", "EU ediTariff", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("TNZ", "NZ ediTariff", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("TRN", "Internal - ediTrainingManager", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("TUK", "UK ediTariff", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("TUS", "US ediTariff", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("UNI", "Universal XML", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("UPG", "Upgrade Assurance", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("USD", "US Domestic", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("USI", "US Domestic", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("WCU", "Web Site Content Update", "[Legacy Module]", "", false, true, ProductTypes.Codes.Enterprise);

						defaultCollection.AddNew("UIL", "UI/Web/Docs - Language", "[Legacy Requirement]", "", false, false, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("LCD", "Local Country Document", "[Legacy Requirement]", "", false, false, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("VCI", "Compliance Issue", "[Legacy Requirement]", "", false, false, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("WTC", "UI/Withholding Tax Compliance Issue", "[Legacy Requirement]", "", false, false, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("ARR", "General Accounting/Reporting/Regulatory", "[Legacy Requirement]", "", false, false, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("CCP", "Customs Compliance", "[Legacy Requirement]", "", false, false, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("SCS", "Supply Chain Security", "[Legacy Requirement]", "", false, false, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("DGM", "Dangerous Good Management", "[Legacy Requirement]", "", false, false, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("CEC", "Carbon/Environmental Compliance", "[Legacy Requirement]", "", false, false, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("RDB", "Reference Data/Base Data", "[Legacy Requirement]", "", false, false, ProductTypes.Codes.Enterprise);

						defaultCollection.AddNew("NHU", "New Hosting Users", "[Legacy Service]", "", false, false, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("SUD", "System Updates", "[Legacy Service]", "", false, false, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("LKY", "License Keys", "[Legacy Service]", "", false, false, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("RGP", "Reopen GL Period", "[Legacy Service]", "", false, false, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("RHP", "Reset Hosting Password", "[Legacy Service]", "", false, false, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("TEA", "Development Training on eAdaptor", "[Legacy Service]", "", false, false, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("TDB", "Development Training on DocBuilder", "[Legacy Service]", "", false, false, ProductTypes.Codes.Enterprise);
						defaultCollection.AddNew("TRW", "Development Training on Report Writer", "[Legacy Service]", "", false, false, ProductTypes.Codes.Enterprise);

						return new SourceModulesRegistryItem("SourceModules",
							(NoResString)ModuleMappingsSubCategory,
							(NoResString)"Menu Items List",
							(NoResString)@"A list of all menu items.

New menu items can also be added here.",
							RegistryStorageFlags.System,
							defaultCollection);
					});
			}
		}

		#endregion

		#region Product Area Assignments
		public ProductAreaAssignmentsRegistryItem ProductAreaAssignments
		{
			get
			{
				return GetItem("ProductAreaAssignments",
					delegate
					{
						return new ProductAreaAssignmentsRegistryItem("ProductAreaAssignments",
							(NoResString)CustomerServiceSubCategory,
							(NoResString)"Product Area Staff Assignments",
							(NoResString)"Assign staff to be responsible for product area.",
							RegistryStorageFlags.System,
							new ProductAreaAssignmentCollection());
					});
			}
		}

		#endregion

		#region Activity Sub Type Capitalization Assignments

		public ActivitySubtypeAssignmentsRegistryItem ActivitySubtypeAssignments
		{
			get
			{
				return GetItem("ActivitySubtypeAssignments",
					delegate
					{
						return new ActivitySubtypeAssignmentsRegistryItem("ActivitySubtypeAssignments",
							(NoResString)WorkItemSubCategory,
							(NoResString)"Capitalized Development Change Types",
							(NoResString)"Assign Change Types Capitalization",
							RegistryStorageFlags.System,
							new ActivitySubtypeAssignmentCollection());
					});
			}
		}

		#endregion

		#region Self Assign Thank you Messages

		public StringArrayRegistryItem SelfAssignMessages
		{
			get
			{
				return GetItem("SelfAssignMessages", () =>
					new StringArrayRegistryItem("SelfAssignMessages",
						(NoResString)CustomerServiceSubCategory,
						(NoResString)"Self-Assign Messages",
						(NoResString)"One of these messages is randomly picked to display when someone assigns an unassigned incident to themselves during the support stage.",
						RegistryStorageFlags.System,
						DefaultSelfAssignMessages)
				);
			}
		}

		string[] DefaultSelfAssignMessages
		{
			get
			{
				return new[]
				{
					"\"May the force be with you\" - Obi-Wan Kenobi",
					"\"Good luck have fun!\" - Anonymous",
					"\"Nice to meet you, Rose. Run for your life!\" - The Doctor (Doctor Who)",
					"\"Who has a sonic screwdriver? Who looks at a screwdriver and thinks, 'Oh, this could be a bit more sonic!'?\" - Captain Jack Harkness",
					"\"Sticks and stones won't break my bones, so you can imagine how I feel about being called names.\" - The Doctor (Voyager)",
					"\"Fun will now commence.\" - Seven of Nine",
					"\"You know the story. Girl meets boy, girl changes boy's subroutines.\" - Captain Janeway",
					"\"The road to success is always under construction.\" - Lily Tomlin",
					"\"The smallest act of kindness is worth more than the grandest intention.\" - Oscar Wilde",
					"\"I feel a very unusual sensation - if it is not indigestion, I think it must be gratitude.\" - Benjamin Disraeli",
				};
			}
		}

		#endregion

		#region Incident Event Workflow Template Client Org

		public GuidRegistryItem IncidentEventWorkflowTemplateClientOrg
		{
			get
			{
				return GetItem("IncidentEventWorkflowTemplateClientOrg", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"IncidentEventWorkflowTemplateClientOrg",
						(NoResString)CustomerServiceSubCategory,
						(NoResString)"Event Workflow Template Client Org",
						(NoResString)"This organisation is to differentiate normal workflow template and template for special incident events.",
						RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.OrgHeader);
					return result;
				});
			}
		}

		#endregion

		#region Closed Incident Last Assignee Notification Period

		public IntRegistryItem ClosedIncidentLastAssigneeNotificationPeriod
		{
			get
			{
				return GetItem("ClosedIncidentLastAssigneeNotificationPeriod", delegate
				{
					return new IntRegistryItem("ClosedIncidentLastAssigneeNotificationPeriod",
						(NoResString)CustomerServiceSubCategory,
						(NoResString)"Closed Incident Last Assignee Notification Period",
						(NoResString)"Specify number of days that assigned staff of last closed task should receive notification emails after incident is closed.",
						RegistryStorageFlags.System,
						30);
				});
			}
		}

		#endregion

		#region Incident Closure Dispositions

		public IncidentClosureDispositionRegistryItem IncidentClosureDispositions
		{
			get
			{
				return GetItem(
					"IncidentClosureDispositions",
					delegate
					{
						var editorInfo = new IncidentClosureDispositionRegistryEditorInfo(
							new MultilingualString[]
							{
								(NoResString)"Stage",
								(NoResString)"Criticality",
								(NoResString)"Product",
								(NoResString)"Disposition"
							},
							(NoResString)"Enabled",
							null, true, false, new bool[] { false, false, false, true });

						var allDescriptions = new MultilingualString[]
							{
								(NoResString)"All Stages that are not listed",
								(NoResString)"All Criticalities that are not listed",
								(NoResString)"All Products that are not listed"
							};

						var codeLists = new CodeDescriptionPairList[]
							{
								new SupportIncidentCategoriesList(),
								new IncidentApprovalLookups(null).CriticalityList,
								new SupportIncidentLookups(new BusinessObjectFactory()).ProductList,
								null
							};

						var defaultValue = new IncidentClosureDispositionCollection(true, 3, 4, allDescriptions, codeLists);
						var any = CodeDescriptionBoolTreeNode.AllCode;

						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentCategoriesList.Codes.Support, (NoResString)SupportIncidentCategoriesList.Descriptions.Support));
						defaultValue.Find(SupportIncidentCategoriesList.Codes.Support, any).Description = (NoResString)"Support Criticalities that are not listed";
						var supportParent = defaultValue.Find(SupportIncidentCategoriesList.Codes.Support, any, any);
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.TrainingReferredToLearningMaterials, (NoResString)"Training - customer referred to eLearning materials", supportParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.TrainingNoLearningMaterials, (NoResString)"Training - eLearning materials not available", supportParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.SystemHardwareNetwork, (NoResString)"System hardware / network or other problem", supportParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.ThirdPartySystemProblem, (NoResString)"3rd Party System problem", supportParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered, (NoResString)"Upgrade Delivered", supportParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, (NoResString)"Self Resolved", supportParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.UnReproducible, (NoResString)"Un-Reproducible", supportParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.NoSupportContract, (NoResString)"No Current Support Contract", supportParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.DuplicateIncident, (NoResString)"Duplicate Incident", supportParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.CsStageDataFix, (NoResString)"CS Stage Data Fix", supportParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.Other, (NoResString)"Other (see resolution comments)", supportParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.FeatureAccepted, (NoResString)"Feature Request Accepted", false, false, supportParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedByIncidentGroup, (NoResString)"Closed by Incident Group", supportParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.NoResponseFromClient, (NoResString)"No Response from Client", boolValue: false, isSystemDefined: true, supportParent, isResolution: ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedInternal, (NoResString)"Closed Internally", supportParent, ZBool.False));

						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentCategoriesList.Codes.Defect, (NoResString)SupportIncidentCategoriesList.Descriptions.Defect));
						defaultValue.Find(SupportIncidentCategoriesList.Codes.Defect, any).Description = (NoResString)"Defect Criticalities that are not listed";
						var defectParent = defaultValue.Find(SupportIncidentCategoriesList.Codes.Defect, any, any);
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.NoDefectFound, (NoResString)"No Defect Found", defectParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered, (NoResString)"Upgrade Delivered", defectParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedInternal, (NoResString)"Closed Internally", defectParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.DatabaseRequested, (NoResString)"Database Requested", defectParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.UnReproducible, (NoResString)"Un-Reproducible", defectParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.CsStageDataFix, (NoResString)"Data Fix", defectParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.FeatureAccepted, (NoResString)"Feature Request Accepted", false, false, defectParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.Cancelled, (NoResString)"Cancelled", defectParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, (NoResString)"Completed", defectParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedByIncidentGroup, (NoResString)"Closed by Incident Group", defectParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.NoResponseFromClient, (NoResString)"No Response from Client", boolValue: false, isSystemDefined: true, defectParent, isResolution: ZBool.False));

						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentCategoriesList.Codes.FeatureRequest, (NoResString)SupportIncidentCategoriesList.Descriptions.FeatureRequest));
						defaultValue.Find(SupportIncidentCategoriesList.Codes.FeatureRequest, any).Description = (NoResString)"Feature Request Criticalities that are not listed";
						var featureParent = defaultValue.Find(SupportIncidentCategoriesList.Codes.FeatureRequest, any, any);
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.Cancelled, (NoResString)"Cancelled", featureParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.NotFeatureRequest, (NoResString)"Not a Feature Request", featureParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered, (NoResString)"Upgrade Delivered", featureParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, (NoResString)"Completed", featureParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedByIncidentGroup, (NoResString)"Closed by Incident Group", featureParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.NoResponseFromClient, (NoResString)"No Response from Client", boolValue: false, isSystemDefined: true, featureParent, isResolution: ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedInternal, (NoResString)"Closed Internally", featureParent, ZBool.False));

						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentCategoriesList.Codes.ContentDevelopment, (NoResString)SupportIncidentCategoriesList.Descriptions.ContentDevelopment));
						defaultValue.Find(SupportIncidentCategoriesList.Codes.ContentDevelopment, any).Description = (NoResString)"Content Development Criticalities that are not listed";
						var contentDevelopmentParent = defaultValue.Find(SupportIncidentCategoriesList.Codes.ContentDevelopment, any, any);
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.Cancelled, (NoResString)"Cancelled", contentDevelopmentParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, (NoResString)"Completed", contentDevelopmentParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.NotContentDevelopmernt, (NoResString)"Not a Content Development Request", contentDevelopmentParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered, (NoResString)"Upgrade Delivered", contentDevelopmentParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedByIncidentGroup, (NoResString)"Closed by Incident Group", contentDevelopmentParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.NoResponseFromClient, (NoResString)"No Response from Client", boolValue: false, isSystemDefined: true, contentDevelopmentParent, isResolution: ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedInternal, (NoResString)"Closed Internally", contentDevelopmentParent, ZBool.False));

						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentCategoriesList.Codes.ComplianceRequirement, (NoResString)SupportIncidentCategoriesList.Descriptions.ComplianceRequirement));
						defaultValue.Find(SupportIncidentCategoriesList.Codes.ComplianceRequirement, any).Description = (NoResString)"Compliance Requirement Criticalities that are not listed";
						var complianceParent = defaultValue.Find(SupportIncidentCategoriesList.Codes.ComplianceRequirement, any, any);
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.Cancelled, (NoResString)"Cancelled", complianceParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, (NoResString)"Completed", complianceParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.NotComplianceRequirement, (NoResString)"Not a Compliance Requirement", complianceParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered, (NoResString)"Upgrade Delivered", complianceParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedByIncidentGroup, (NoResString)"Closed by Incident Group", complianceParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.NoResponseFromClient, (NoResString)"No Response from Client", boolValue: false, isSystemDefined: true, complianceParent, isResolution: ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedInternal, (NoResString)"Closed Internally", complianceParent, ZBool.False));

						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentCategoriesList.Codes.CustomerServiceRequest, (NoResString)SupportIncidentCategoriesList.Descriptions.CustomerServiceRequest));
						defaultValue.Find(SupportIncidentCategoriesList.Codes.CustomerServiceRequest, any).Description = (NoResString)"Service Request Criticalities that are not listed";
						var serviceParent = defaultValue.Find(SupportIncidentCategoriesList.Codes.CustomerServiceRequest, any, any);
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.Cancelled, (NoResString)"Cancelled", serviceParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, (NoResString)"Completed", serviceParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.NotCustomerServiceRequest, (NoResString)"Not a Service Request", serviceParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered, (NoResString)"Upgrade Delivered", serviceParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedByIncidentGroup, (NoResString)"Closed by Incident Group", serviceParent, ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.NoResponseFromClient, (NoResString)"No Response from Client", boolValue: false, isSystemDefined: true, serviceParent, isResolution: ZBool.False));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedInternal, (NoResString)"Closed Internally", serviceParent, ZBool.False));

						return new IncidentClosureDispositionRegistryItem(
							"IncidentClosureDispositions",
							(NoResString)CustomerServiceSubCategory,
							(NoResString)"Incident Closure Dispositions",
							(NoResString)"Setup incident closure dispositions based on stage, criticality and product. Closure dispositions can be set to resolve an incident or close it immediately.",
							RegistryStorageFlags.System,
							editorInfo,
							defaultValue);
					});
			}
		}

		#endregion

		#region Incident Criticality Stages

		public CriticalityStageMappingRegistryItem IncidentCriticalityStages
		{
			get
			{
				return GetItem(
					"IncidentCriticalityStages",
					delegate
					{
						var editorInfo = new CriticalityStageMappingRegistryEditorInfo(
							new MultilingualString[] { (NoResString)"Criticality", (NoResString)"Stage" },
							(NoResString)"Enabled",
							null,
							new bool[] { false, true },
							new bool[] { false, true },
							false);

						var allDescriptions = new MultilingualString[]
							{
								(NoResString)""
							};

						var codeLists = new CodeDescriptionPairList[]
							{
								new IncidentApprovalLookups(null).CriticalityList,
								new SupportIncidentCategoriesList()
							};

						var defaultValue = new CriticalityStageMappingCollection(true, 3, 2, allDescriptions, codeLists);

						var cr1Code = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
						defaultValue.AddSystemChildren(defaultValue.Add(cr1Code, (NoResString)"Entire system is down � system failure", true, true, null));
						var cr1Parent = defaultValue.Find(cr1Code);
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentCategoriesList.Codes.Support, (NoResString)SupportIncidentCategoriesList.Descriptions.Support, true, true, false, cr1Parent));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentCategoriesList.Codes.Defect, (NoResString)SupportIncidentCategoriesList.Descriptions.Defect, true, false, false, cr1Parent));

						var cr2Code = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR2_ModuleDown;
						defaultValue.AddSystemChildren(defaultValue.Add(cr2Code, (NoResString)"Entire module not working with no manual work around", true, true, null));
						var cr2Parent = defaultValue.Find(cr2Code);
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentCategoriesList.Codes.Support, (NoResString)SupportIncidentCategoriesList.Descriptions.Support, true, true, false, cr2Parent));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentCategoriesList.Codes.Defect, (NoResString)SupportIncidentCategoriesList.Descriptions.Defect, true, false, false, cr2Parent));

						var cr3Code = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
						defaultValue.AddSystemChildren(defaultValue.Add(cr3Code, (NoResString)"Single function not working with no manual work around", true, true, null));
						var cr3Parent = defaultValue.Find(cr3Code);
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentCategoriesList.Codes.Support, (NoResString)SupportIncidentCategoriesList.Descriptions.Support, true, true, false, cr3Parent));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentCategoriesList.Codes.Defect, (NoResString)SupportIncidentCategoriesList.Descriptions.Defect, true, false, false, cr3Parent));

						var cr4Code = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
						defaultValue.AddSystemChildren(defaultValue.Add(cr4Code, (NoResString)"Single function not working with manual work around", true, true, null));
						var cr4Parent = defaultValue.Find(cr4Code);
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentCategoriesList.Codes.Support, (NoResString)SupportIncidentCategoriesList.Descriptions.Support, true, true, false, cr4Parent));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentCategoriesList.Codes.Defect, (NoResString)SupportIncidentCategoriesList.Descriptions.Defect, true, false, false, cr4Parent));

						var cr5Code = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR5_Training;
						defaultValue.AddSystemChildren(defaultValue.Add(cr5Code, (NoResString)"Training Questions", true, true, null));
						var cr5Parent = defaultValue.Find(cr5Code);
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentCategoriesList.Codes.Support, (NoResString)SupportIncidentCategoriesList.Descriptions.Support, true, true, false, cr5Parent));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentCategoriesList.Codes.ContentDevelopment, (NoResString)SupportIncidentCategoriesList.Descriptions.ContentDevelopment, true, false, false, cr5Parent));

						var cr6Code = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
						defaultValue.AddSystemChildren(defaultValue.Add(cr6Code, (NoResString)"Feature Request", true, true, null));
						var cr6Parent = defaultValue.Find(cr6Code);
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentCategoriesList.Codes.Support, (NoResString)SupportIncidentCategoriesList.Descriptions.Support, false, false, false, cr6Parent));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentCategoriesList.Codes.FeatureRequest, (NoResString)SupportIncidentCategoriesList.Descriptions.FeatureRequest, true, true, false, cr6Parent));

						var cr7Code = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest;
						defaultValue.AddSystemChildren(defaultValue.Add(cr7Code, (NoResString)"Estimate / Quote Request", true, true, null));
						var cr7Parent = defaultValue.Find(cr7Code);
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentCategoriesList.Codes.Support, (NoResString)SupportIncidentCategoriesList.Descriptions.Support, false, false, false, cr7Parent));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentCategoriesList.Codes.FeatureRequest, (NoResString)SupportIncidentCategoriesList.Descriptions.FeatureRequest, true, true, false, cr7Parent));

						var cr8Code = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
						defaultValue.AddSystemChildren(defaultValue.Add(cr8Code, (NoResString)"Compliance, Reference and Master Data", true, true, null));
						var cr8Parent = defaultValue.Find(cr8Code);
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentCategoriesList.Codes.Support, (NoResString)SupportIncidentCategoriesList.Descriptions.Support, true, true, false, cr8Parent));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentCategoriesList.Codes.ComplianceRequirement, (NoResString)SupportIncidentCategoriesList.Descriptions.ComplianceRequirement, true, false, false, cr8Parent));

						var cr9Code = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
						defaultValue.AddSystemChildren(defaultValue.Add(cr9Code, (NoResString)"Service Request", true, true, null));
						var cr9Parent = defaultValue.Find(cr9Code);
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentCategoriesList.Codes.Support, (NoResString)SupportIncidentCategoriesList.Descriptions.Support, false, false, false, cr9Parent));
						defaultValue.AddSystemChildren(defaultValue.Add(SupportIncidentCategoriesList.Codes.CustomerServiceRequest, (NoResString)SupportIncidentCategoriesList.Descriptions.CustomerServiceRequest, true, true, false, cr9Parent));

						return new CriticalityStageMappingRegistryItem(
							"IncidentCriticalityStages",
							(NoResString)CustomerServiceSubCategory,
							(NoResString)"Incident Criticality Stages",
							(NoResString)"Setup valid criticality stages by incident criticalities. There should be exactly one default stage for each criticality.",
							RegistryStorageFlags.System,
							editorInfo,
							defaultValue);
					});
			}
		}

		#endregion

		#region Feature Request Quotation Auto Decline Period

		public IntRegistryItem FeatureRequestQuotationAutoExpirePeriod
		{
			get
			{
				// Previously FeatureRequestQuotationAutoDeclinePeriod. Retain key to not lose overridden value.
				return GetItem("FeatureRequestQuotationAutoDeclinePeriod", delegate
				{
					return new IntRegistryItem("FeatureRequestQuotationAutoDeclinePeriod",
						(NoResString)CustomerServiceSubCategory,
						(NoResString)"Feature Request Quotation Auto Expire Period",
						(NoResString)"Specify number of days that feature request quotation is auto expired after quotation is provided.",
						RegistryStorageFlags.System,
						20);
				});
			}
		}

		#endregion

		#region Feature Request Expire Auto Expire Period

		public IntRegistryItem FeatureRequestEstimateAutoExpirePeriod
		{
			get
			{
				return GetItem("FeatureRequestEstimateAutoExpirePeriod", delegate
				{
					return new IntRegistryItem("FeatureRequestEstimateAutoExpirePeriod",
						(NoResString)CustomerServiceSubCategory,
						(NoResString)"Feature Request Estimate Auto Expire Period",
						(NoResString)"Specify number of days that feature request estimate is auto expired after estimate is provided.",
						RegistryStorageFlags.System,
						30);
				});
			}
		}

		#endregion

		#region Feature Request Estimates and Quotes

		public const string FeatureRequestsSubCategory = CustomerServiceSubCategory + "/Feature Requests";
		public const string FeatureRequestsGenericCaptionAttributesSubCategory = FeatureRequestsSubCategory + "/Generic Caption Attributes";

		#region Generic Caption Attributes

		#region Exclusive Estimate Labels

		public const string FeatureRequestsEstimateSubCategory = FeatureRequestsGenericCaptionAttributesSubCategory + "/Estimate";

		public StringRegistryItem EstMinDevHoursLabel
		{
			get
			{
				return GetItem(
					"EstMinDevHoursLabel",
					delegate
					{
						return new StringRegistryItem(
							"EstMinDevHoursLabel",
							(NoResString)FeatureRequestsEstimateSubCategory,
							(NoResString)"Estimate Min. Development Hours Label",
							(NoResString)"This will be displayed in the label on the Feature Request tab page, under the Estimate group.",
							RegistryStorageFlags.System,
							"Min. Development Hours");
					});
			}
		}
		public StringRegistryItem EstMaxDevHoursLabel
		{
			get
			{
				return GetItem(
					"EstMaxDevHoursLabel",
					delegate
					{
						return new StringRegistryItem(
							"EstMaxDevHoursLabel",
							(NoResString)FeatureRequestsEstimateSubCategory,
							(NoResString)"Estimate Max. Development Hours Label",
							(NoResString)"This will be displayed in the label on the Feature Request tab page, under the Estimate group.",
							RegistryStorageFlags.System,
							"Max. Development Hours");
					});
			}
		}

		public StringRegistryItem EstSentDateLabel
		{
			get
			{
				return GetItem(
					"EstSentDateLabel",
					delegate
					{
						return new StringRegistryItem(
							"EstSentDateLabel",
							(NoResString)FeatureRequestsEstimateSubCategory,
							(NoResString)"Estimate Sent Label",
							(NoResString)"This will be displayed in the label on the Feature Request tab page, under the Estimate group.",
							RegistryStorageFlags.System,
							"Estimate Sent");
					});
			}
		}

		public StringRegistryItem EstExpiryDateLabel
		{
			get
			{
				return GetItem(
					"EstExpiryDateLabel",
					delegate
					{
						return new StringRegistryItem(
							"EstExpiryDateLabel",
							(NoResString)FeatureRequestsEstimateSubCategory,
							(NoResString)"Estimate Expiry Label",
							(NoResString)"This will be displayed in the label on the Feature Request tab page, under the Estimate group.",
							RegistryStorageFlags.System,
							"Estimate Expiry");
					});
			}
		}

		public StringRegistryItem EstRequestDateLabel
		{
			get
			{
				return GetItem(
					"EstRequestDateLabel",
					delegate
					{
						return new StringRegistryItem(
							"EstRequestDateLabel",
							(NoResString)FeatureRequestsEstimateSubCategory,
							(NoResString)"Quote Requested Label",
							(NoResString)"This will be displayed in the label on the Feature Request tab page, under the Estimate group.",
							RegistryStorageFlags.System,
							"Quote Requested");
					});
			}
		}

		public StringRegistryItem EstMinMonthlyLabel
		{
			get
			{
				return GetItem(
					"EstMinMonthlyLabel",
					delegate
					{
						return new StringRegistryItem(
							"EstMinMonthlyLabel",
							(NoResString)FeatureRequestsEstimateSubCategory,
							(NoResString)"Min Estimate Monthly Label",
							(NoResString)"This will be displayed in the label on the Feature Request tab page, under the Estimate group.",
							RegistryStorageFlags.System,
							"Min Estimate Monthly");
					});
			}
		}

		public StringRegistryItem EstMaxMonthlyLabel
		{
			get
			{
				return GetItem(
					"EstMaxMonthlyLabel",
					delegate
					{
						return new StringRegistryItem(
							"EstMaxMonthlyLabel",
							(NoResString)FeatureRequestsEstimateSubCategory,
							(NoResString)"Max Estimate Monthly Label",
							(NoResString)"This will be displayed in the label on the Feature Request tab page, under the Estimate group.",
							RegistryStorageFlags.System,
							"Max Estimate Monthly");
					});
			}
		}

		public StringRegistryItem EstMinOneOffLabel
		{
			get
			{
				return GetItem(
					"EstMinOneOffLabel",
					delegate
					{
						return new StringRegistryItem(
							"EstMinOneOffLabel",
							(NoResString)FeatureRequestsEstimateSubCategory,
							(NoResString)"Min Estimate One-Off Label",
							(NoResString)"This will be displayed in the label on the Feature Request tab page, under the Estimate group.",
							RegistryStorageFlags.System,
							"Min Estimate One-Off");
					});
			}
		}

		public StringRegistryItem EstMaxOneOffLabel
		{
			get
			{
				return GetItem(
					"EstMaxOneOffLabel",
					delegate
					{
						return new StringRegistryItem(
							"EstMaxOneOffLabel",
							(NoResString)FeatureRequestsEstimateSubCategory,
							(NoResString)"Max Estimate One-Off Label",
							(NoResString)"This will be displayed in the label on the Feature Request tab page, under the Estimate group.",
							RegistryStorageFlags.System,
							"Max Estimate One-Off");
					});
			}
		}

		public StringRegistryItem EstExpressDelCutOffLabel
		{
			get
			{
				return GetItem(
					"EstExpressDelCutOffLabel",
					delegate
					{
						return new StringRegistryItem(
							"EstExpressDelCutOffLabel",
							(NoResString)FeatureRequestsEstimateSubCategory,
							(NoResString)"Express Del. Cut Off Label",
							(NoResString)"This will be displayed in the label on the Feature Request tab page, under the Estimate group.",
							RegistryStorageFlags.System,
							"Express Del. Cut Off");
					});
			}
		}

		#endregion

		#region Exclusive Quote Labels

		public const string FeatureRequestsQuoteSubCategory = FeatureRequestsGenericCaptionAttributesSubCategory + "/Quote";

		public StringRegistryItem QteMinDevHoursLabel
		{
			get
			{
				return GetItem(
					"QteMinDevHoursLabel",
					delegate
					{
						return new StringRegistryItem(
							"QteMinDevHoursLabel",
							(NoResString)FeatureRequestsQuoteSubCategory,
							(NoResString)"Quote Min. Development Hours Label",
							(NoResString)"This will be displayed in the label on the Feature Request tab page, under the Quote group.",
							RegistryStorageFlags.System,
							"Min. Development Hours");
					});
			}
		}
		public StringRegistryItem QteMaxDevHoursLabel
		{
			get
			{
				return GetItem(
					"QteMaxDevHoursLabel",
					delegate
					{
						return new StringRegistryItem(
							"QteMaxDevHoursLabel",
							(NoResString)FeatureRequestsQuoteSubCategory,
							(NoResString)"Quote Max. Development Hours Label",
							(NoResString)"This will be displayed in the label on the Feature Request tab page, under the Quote group.",
							RegistryStorageFlags.System,
							"Max. Development Hours");
					});
			}
		}

		public StringRegistryItem QteSentDateLabel
		{
			get
			{
				return GetItem(
					"QteSentDateLabel",
					delegate
					{
						return new StringRegistryItem(
							"QteSentDateLabel",
							(NoResString)FeatureRequestsQuoteSubCategory,
							(NoResString)"Quote Sent Label",
							(NoResString)"This will be displayed in the label on the Feature Request tab page, under the Quote group.",
							RegistryStorageFlags.System,
							"Quote Sent");
					});
			}
		}

		public StringRegistryItem QteExpiryDateLabel
		{
			get
			{
				return GetItem(
					"QteExpiryDateLabel",
					delegate
					{
						return new StringRegistryItem(
							"QteExpiryDateLabel",
							(NoResString)FeatureRequestsQuoteSubCategory,
							(NoResString)"Quote Expiry Label",
							(NoResString)"This will be displayed in the label on the Feature Request tab page, under the Quote group.",
							RegistryStorageFlags.System,
							"Quote Expiry");
					});
			}
		}

		public StringRegistryItem QteAcceptedDateLabel
		{
			get
			{
				return GetItem(
					"QteAcceptedDateLabel",
					delegate
					{
						return new StringRegistryItem(
							"QteAcceptedDateLabel",
							(NoResString)FeatureRequestsQuoteSubCategory,
							(NoResString)"Quote Accepted Label",
							(NoResString)"This will be displayed in the label on the Feature Request tab page, under the Quote group.",
							RegistryStorageFlags.System,
							"Quote Accepted");
					});
			}
		}

		public StringRegistryItem QteDateDeliveredLabel
		{
			get
			{
				return GetItem(
					"QteDateDeliveredLabel",
					delegate
					{
						return new StringRegistryItem(
							"QteDateDeliveredLabel",
							(NoResString)FeatureRequestsQuoteSubCategory,
							(NoResString)"Delivered Date Label",
							(NoResString)"This will be displayed in the label on the Feature Request tab page, under the Quote group.",
							RegistryStorageFlags.System,
							"Delivered Date");
					});
			}
		}

		public StringRegistryItem QtePaymentTypeLabel
		{
			get
			{
				return GetItem(
					"QtePaymentTypeLabel",
					delegate
					{
						return new StringRegistryItem(
							"QtePaymentTypeLabel",
							(NoResString)FeatureRequestsQuoteSubCategory,
							(NoResString)"Payment Type Label",
							(NoResString)"This will be displayed in the label on the Feature Request tab page, under the Quote group.",
							RegistryStorageFlags.System,
							"Payment Type");
					});
			}
		}

		public StringRegistryItem QteAmountLabel
		{
			get
			{
				return GetItem(
					"QteAmountLabel",
					delegate
					{
						return new StringRegistryItem(
							"QteAmountLabel",
							(NoResString)FeatureRequestsQuoteSubCategory,
							(NoResString)"Monthly Fee Label",
							(NoResString)"This will be displayed in the label on the Feature Request tab page, under the Quote group.",
							RegistryStorageFlags.System,
							"Monthly Fee");
					});
			}
		}

		public StringRegistryItem QteOneOffUpfrontLabel
		{
			get
			{
				return GetItem(
					"QteOneOffUpfrontLabel",
					delegate
					{
						return new StringRegistryItem(
							"QteOneOffUpfrontLabel",
							(NoResString)FeatureRequestsQuoteSubCategory,
							(NoResString)"One-Off Upfront Label",
							(NoResString)"This will be displayed in the label on the Feature Request tab page, under the Quote group.",
							RegistryStorageFlags.System,
							"One-Off Upfront");
					});
			}
		}

		public StringRegistryItem QteHeadStartIncludedLabel
		{
			get
			{
				return GetItem(
					"QteHeadStartIncludedLabel",
					delegate
					{
						return new StringRegistryItem(
							"QteHeadStartIncludedLabel",
							(NoResString)FeatureRequestsQuoteSubCategory,
							(NoResString)"Head Start Label",
							(NoResString)"This will be displayed in the label on the Feature Request tab page, under the Quote group.",
							RegistryStorageFlags.System,
							"Head Start");
					});
			}
		}

		public StringRegistryItem SurchargeLabel
		{
			get
			{
				return GetItem(
					"SurchargeLabel",
					delegate
					{
						return new StringRegistryItem(
							"SurchargeLabel",
							(NoResString)FeatureRequestsQuoteSubCategory,
							(NoResString)"Surcharge Label",
							(NoResString)"This will be displayed in the labels on the Feature Request tab page, under the Quote group.",
							RegistryStorageFlags.System,
							"Surcharge");
					});
			}
		}

		public StringRegistryItem QteExpressDeliveryIncludedLabel
		{
			get
			{
				return GetItem(
					"QteExpressDeliveryIncludedLabel",
					delegate
					{
						return new StringRegistryItem(
							"QteExpressDeliveryIncludedLabel",
							(NoResString)FeatureRequestsQuoteSubCategory,
							(NoResString)"Express Delivery Label",
							(NoResString)"This will be displayed in the label on the Feature Request tab page, under the Quote group.",
							RegistryStorageFlags.System,
							"Express Delivery");
					});
			}
		}

		#endregion

		public StringRegistryItem PaymentTermsLabel
		{
			get
			{
				return GetItem(
					"PaymentTermsLabel",
					delegate
					{
						return new StringRegistryItem(
							"PaymentTermsLabel",
							(NoResString)FeatureRequestsGenericCaptionAttributesSubCategory,
							(NoResString)"Payment Terms Label",
							(NoResString)"This will be displayed in the labels on the Feature Request tab page, under the Estimate and Quote groups.",
							RegistryStorageFlags.System,
							"Payment Terms");
					});
			}
		}

		public StringRegistryItem CurrencyLabel
		{
			get
			{
				return GetItem(
					"CurrencyLabel",
					delegate
					{
						return new StringRegistryItem(
							"CurrencyLabel",
							(NoResString)FeatureRequestsGenericCaptionAttributesSubCategory,
							(NoResString)"Currency Label",
							(NoResString)"This will be displayed in the labels on the Feature Request tab page, under the Estimate and Quote groups.",
							RegistryStorageFlags.System,
							"Currency");
					});
			}
		}

		public StringRegistryItem CancellationFeeLabel
		{
			get
			{
				return GetItem(
					"CancellationFeeLabel",
					delegate
					{
						return new StringRegistryItem(
							"CancellationFeeLabel",
							(NoResString)FeatureRequestsEstimateSubCategory,
							(NoResString)"Cancellation Fee Label",
							(NoResString)"This will be displayed in the labels on the Feature Request tab page, under the Estimate and Quote groups.",
							RegistryStorageFlags.System,
							"Cancellation Fee");
					});
			}
		}

		#endregion

		public ParentAndChildCodeDescriptionBoolRegistryItem PaymentTypesAndPaymentTerms
		{
			get
			{
				return GetItem("PaymentTypesAndPaymentTerms", delegate
				{
					CodeDescriptionBoolRegistryEditorInfo parentListEditorInfo = new CodeDescriptionBoolRegistryEditorInfo(null, false);
					CodeDescriptionBoolRegistryEditorInfo childListEditorInfo = new CodeDescriptionBoolRegistryEditorInfo(null, false);
					ParentAndChildCodeDescriptionBoolRegistryEditorInfo termsTypesEditorInfo = new ParentAndChildCodeDescriptionBoolRegistryEditorInfo(
						(NoResString)"Payment Terms", (NoResString)"Payment Types",
						parentListEditorInfo, childListEditorInfo, false);

					return new ParentAndChildCodeDescriptionBoolRegistryItem(
					(NoResString)"PaymentTypesAndPaymentTerms",
					(NoResString)FeatureRequestsSubCategory,
					(NoResString)"Payment Types and Payment Terms",
					(NoResString)"The list of Payment Terms and their Types", termsTypesEditorInfo,
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					DefaultPaymentTypesAndPaymentTermsList);
				});
			}
		}

		ParentCodeDescriptionBoolCollection DefaultPaymentTypesAndPaymentTermsList
		{
			get
			{
				var result = new ParentCodeDescriptionBoolCollection();
				var type1 = result.AddNew();
				type1.Code = PaymentTypesAndTermsList.PaymentTypes.Codes.MonthlyType;
				type1.Description = PaymentTypesAndTermsList.PaymentTypes.Descriptions.MonthlyType;
				var term1 = type1.ChildList.AddNew();
				term1.Code = PaymentTypesAndTermsList.PaymentTerms.Codes.MonthlyTerm;
				term1.Description = PaymentTypesAndTermsList.PaymentTerms.Descriptions.MonthlyTerm;

				var type2 = result.AddNew();
				type2.Code = PaymentTypesAndTermsList.PaymentTypes.Codes.OneOffType;
				type2.Description = PaymentTypesAndTermsList.PaymentTypes.Descriptions.OneOffType;
				var term2 = type2.ChildList.AddNew();
				term2.Code = PaymentTypesAndTermsList.PaymentTerms.Codes.OneOffTerm;
				term2.Description = PaymentTypesAndTermsList.PaymentTerms.Descriptions.OneOffTerm;

				return result;
			}
		}

		#endregion

		#region Incident Products Not Send eHub Message Back Even Request is Received Via eHub

		public CodeDescriptionPairListRegistryItem IncidentProductsToIgnoreForceResponseSentViaEHub
		{
			get
			{
				return GetItem("IncidentProductsToIgnoreForceResponseSentViaEHub", delegate
				{
					var defaultList = new CodeDescriptionPairList();
					defaultList.AddPair("SPH", "Sapphire");

					return new CodeDescriptionPairListRegistryItem(
							"IncidentProductsToIgnoreForceResponseSentViaEHub",
							(NoResString)CustomerServiceSubCategory,
							(NoResString)"Incident Products To Ignore Force Response Sent Via EHub",
							(NoResString)"Product codes which do not force sending eHub response message back even request is received from eHub",
							3,
							new CodeDescriptionPairListEditorInfo(true, false),
							RegistryStorageFlags.System,
							false,
							RegistryOptions.Default,
							defaultList,
							false);
				});
			}
		}

		#endregion

		#region Glow Portal

		public const string GlowPortalSubCategory = Category + "/GLOW";

		public StringRegistryItem GlowPortalRootUrl
		{
			get
			{
				return GetItem("EdiGlowPortalRootUrl", delegate
				{
					var item = new StringRegistryItem(
						"EdiGlowPortalRootUrl",
						(NoResString)GlowPortalSubCategory,
						(NoResString)"GLOW Portal Root URL",
						(NoResString)"The base URI for the GLOW portal to ediProd",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						"");
					item.DataType = new UriRegistryDataType(Uri.UriSchemeHttps) { AllowAutoProtocolPrefixing = false };
					return item;
				});
			}
		}

		public StringRegistryItem GlowNewERequestPageUri
		{
			get
			{
				return GetItem("GlowNewERequestPageUri", delegate
				{
					var item = new StringRegistryItem(
						"GlowNewERequestPageUri",
						(NoResString)GlowPortalSubCategory,
						(NoResString)"GLOW New eRequest URL",
						(NoResString)"The GLOW New eRequest landing page URL, relative to the GLOW Portal Root URL",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						"goto/new_erequest");
					return item;
				});
			}
		}

		public StringRegistryItem GlowERequestPortalUri
		{
			get
			{
				return GetItem("GlowERequestPortalUri", delegate
				{
					var item = new StringRegistryItem(
						"GlowERequestPortalUri",
						(NoResString)GlowPortalSubCategory,
						(NoResString)"GLOW eRequest Portal",
						(NoResString)"The GLOW eRequest Portal landing page URL, relative to the GLOW Portal Root URL",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						"INC");
					return item;
				});
			}
		}

		public StringRegistryItem GlowEditERequestPageUri
		{
			get
			{
				return GetItem("EdiGlowERequestPageUri", delegate
				{
					var item = new StringRegistryItem(
						"EdiGlowERequestPageUri",
						(NoResString)GlowPortalSubCategory,
						(NoResString)"GLOW Edit eRequest URL",
						(NoResString)("The GLOW Edit existing eRequest page URL, relative to the GLOW Portal Root URL. Macro " + GlowEditERequestPageUri_PkMacro + " is replaced with the record Primary Key."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						"INC/Desktop#/formFlow/688ed765-2824-48ff-9f48-9aca459979bd/{*PK*}");
					return item;
				});
			}
		}

		public const string GlowEditERequestPageUri_PkMacro = "{*PK*}";

		public BooleanRegistryItem GlowNewInternalIncident
		{
			get
			{
				return GetItem("GlowNewInternalIncident", delegate
				{
					return new BooleanRegistryItem(
						"GlowNewInternalIncident",
						(NoResString)GlowPortalSubCategory,
						(NoResString)"Enable GLOW New Internal Incident",
						(NoResString)"New Internal Incident command brings up the New eRequest page. Requires GLOW eRequests also enabled.",
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		public StringRegistryItem GlowAccreditationPortalUri
		{
			get
			{
				return GetItem("GlowAccreditationPortalUri", delegate
				{
					var item = new StringRegistryItem(
						"GlowAccreditationPortalUri",
						(NoResString)GlowPortalSubCategory,
						(NoResString)"GLOW Accreditation Portal",
						(NoResString)"The GLOW accreditation Portal landing page URL, relative to the GLOW Portal Root URL",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						"AAC");
					return item;
				});
			}
		}

		#endregion

		#region Incident Similarity

		public const string CustomerServiceSubCategoryIncidentSimilarity = CustomerServiceSubCategory + "/Incident Similarity";

		public BooleanRegistryItem EnableIncidentSimilarityFunctionality
		{
			get
			{
				return GetItem("EnableIncidentSimilarityFunctionality", () =>
				{
					return new BooleanRegistryItem(
						"EnableIncidentSimilarityFunctionality",
						(NoResString)CustomerServiceSubCategoryIncidentSimilarity,
						(NoResString)"Incident Similarity Functionality",
						(NoResString)"Enable Incident Similatiry functionality.",
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		public BooleanRegistryItem EnableIncidentSimilarityPreload
		{
			get
			{
				return GetItem("EnableIncidentSimilarityPreload", () =>
				{
					return new BooleanRegistryItem(
						"EnableIncidentSimilarityPreload",
						(NoResString)CustomerServiceSubCategoryIncidentSimilarity,
						(NoResString)"Pre-load incident similarities on control initialisation",
						(NoResString)"Pre-load incident similarities on control initialisation",
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		public IntRegistryItem RelatedIncidentsMonthsToStore
		{
			get
			{
				return GetItem("RelatedIncidentsMonthsToStore", () =>
				{
					return new IntRegistryItem(
						"RelatedIncidentsMonthsToStore",
						(NoResString)CustomerServiceSubCategoryIncidentSimilarity,
						(NoResString)"Related Support Incidents Months to Store",
						(NoResString)"This determines how many months worth of periodically-computed support incident digests and similarity values should be computed and stored in the database, for the purpose of similar incident lookup.",
						RegistryStorageFlags.System,
						24);
				});
			}
		}

		public IntRegistryItem RelatedIncidentsTfIdfBatchSize
		{
			get
			{
				return GetItem("RelatedIncidentsTfIdfBatchSize", () =>
				{
					return new IntRegistryItem(
						"RelatedIncidentsTfIdfBatchSize",
						(NoResString)CustomerServiceSubCategoryIncidentSimilarity,
						(NoResString)"Batch Size for Related Support Incidents Digest Computation",
						(NoResString)"This determines how many support incidents will be processed between successive commits to the database during the document metric processing stage.",
						RegistryStorageFlags.System,
						1000);
				});
			}
		}

		public IntRegistryItem RelatedIncidentsSimilarityMatrixBatchSize
		{
			get
			{
				return GetItem("RelatedIncidentsSimilarityMatrixBatchSize", () =>
				{
					return new IntRegistryItem(
						"RelatedIncidentsSimilarityMatrixBatchSize",
						(NoResString)CustomerServiceSubCategoryIncidentSimilarity,
						(NoResString)"Batch Size for Related Support Incidents Similarity Computation",
						(NoResString)"This determines how many support incidents will be processed between successive commits to the database during the document similarity processing stage.",
						RegistryStorageFlags.System,
						100);
				});
			}
		}

		public DecimalRegistryItem RelatedIncidentsMinimumSimilarity
		{
			get
			{
				return GetItem("RelatedIncidentsMinimumSimilarity", () =>
				{
					return new DecimalRegistryItem(
						"RelatedIncidentsMinimumSimilarity",
						(NoResString)CustomerServiceSubCategoryIncidentSimilarity,
						(NoResString)"Related Support Incidents Minimum Similarity",
						(NoResString)"This determines the minimum cosine similarity between two support incidents to store in the similarity matrix table.",
						new NumericRegistryEditorInfo(decimalPlaces: 3),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						0.25M,
						0.0,
						1.0);
				});
			}
		}

		public IntRegistryItem RelatedIncidentsMaxTopToStore
		{
			get
			{
				return GetItem("RelatedIncidentsMaxTopToStore", () =>
				{
					return new IntRegistryItem(
						"RelatedIncidentsMaxTopToStore",
						(NoResString)CustomerServiceSubCategoryIncidentSimilarity,
						(NoResString)"Related Support Incidents Max To Store",
						(NoResString)"This determines the maximum number of similar incidents to store per incident.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						100);
				});
			}
		}

		public IntRegistryItem RelatedIncidentsDecimalPrecision
		{
			get
			{
				return GetItem("RelatedIncidentsDecimalPrecision", () =>
				{
					return new IntRegistryItem(
						"RelatedIncidentsDecimalPrecision",
						(NoResString)CustomerServiceSubCategoryIncidentSimilarity,
						(NoResString)"Related Support Incidents Decimal Precision",
						(NoResString)"This determines the maximum decimal places to keep when caculating and comparing TFIDF similarity of support incidents.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						3);
				});
			}
		}

		public StringRegistryItem WordContextServiceUrl
		{
			get
			{
				return GetItem("WordContextServiceUrl", delegate
				{
					var item = new StringRegistryItem(
						"WordContextServiceUrl",
						(NoResString)CustomerServiceSubCategoryIncidentSimilarity,
						(NoResString)"Word Context HTTP Service URL",
						(NoResString)"The URL for the Word Context Service, which provides NLP capabilities via HTTP requests",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						"http://wcs.wtg.zone");
					return item;
				});
			}
		}

		public BooleanRegistryItem EnableIncidentSimilarityWebService => GetItem("EnableIncidentSimilarityWebService", () =>
			new BooleanRegistryItem(
				"EnableIncidentSimilarityWebService",
				(NoResString)CustomerServiceSubCategoryIncidentSimilarity,
				(NoResString)"Enable Incident Similarity Web Service",
				(NoResString)"The Incident Similarity functionality will be provided by the web service.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false));

		public StringRegistryItem IncidentSimilarityWebServiceUrl => GetItem("IncidentSimilarityWebServiceUrl", () =>
			new StringRegistryItem(
				"IncidentSimilarityWebServiceUrl",
				(NoResString)CustomerServiceSubCategoryIncidentSimilarity,
				(NoResString)"Incident Similarity Web Service URL",
				(NoResString)"URL for the Incident Similarity Web Service, which will be used instead of the CargoWise service task, if enabled.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				string.Empty));

		public IntRegistryItem IncidentSimilarityWebServiceDataBatchSize => GetItem("IncidentSimilarityWebServiceDataBatchSize", () =>
			new IntRegistryItem(
				"IncidentSimilarityWebServiceDataBatchSize",
				(NoResString)CustomerServiceSubCategoryIncidentSimilarity,
				(NoResString)"Incident Similarity Web Service Data Batch Size",
				(NoResString)"The maximum number of rows that will be sent to the Incident Similarity Web Service at each nudge of relevant data updates.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				100_000));

		#endregion

		#region Incident Autoresponder

		public const string AutoresponderSubCategory = CustomerServiceSubCategory + "/Autoresponder";

		public BooleanRegistryItem EnableAutoresponder
		{
			get
			{
				return GetItem("EnableAutoresponder", () =>
				{
					return new BooleanRegistryItem(
						"EnableAutoresponder",
						(NoResString)AutoresponderSubCategory,
						(NoResString)"Enable Autoresponder Function",
						(NoResString)"This determines if the autoresponder feature is enabled.",
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		public IntRegistryItem NewIncidentsDaysToReply
		{
			get
			{
				return GetItem("NewIncidentsDaysToReply", () =>
				{
					return new IntRegistryItem(
						"NewIncidentsDaysToReply",
						(NoResString)AutoresponderSubCategory,
						(NoResString)"New Incidents Days To Reply",
						(NoResString)"This determines how many days ago an incident created will be replied.",
						RegistryStorageFlags.System,
						14);
				});
			}
		}

		public BooleanRegistryItem IncludeUpdateNoteUrls
		{
			get
			{
				return GetItem("IncludeUpdateNoteUrls", () =>
				{
					return new BooleanRegistryItem(
						"IncludeUpdateNoteUrls",
						(NoResString)AutoresponderSubCategory,
						(NoResString)"Include UpdateNote Urls",
						(NoResString)"Controls if UpdateNotes are included when extracting Urls.",
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		public IntRegistryItem GetSimilarIncidentMaxCount
		{
			get
			{
				return GetItem("GetSimilarIncidentMaxCount", () =>
				{
					return new IntRegistryItem(
						"GetSimilarIncidentMaxCount",
						(NoResString)AutoresponderSubCategory,
						(NoResString)"Max Count of Incidents during Retrieval",
						(NoResString)"Controls the maximum amount of incidents during Retrieval",
						RegistryStorageFlags.System,
						1000
						);
				});
			}
		}

		public IntRegistryItem RespondedUrlsLimit
		{
			get
			{
				return GetItem("RespondedUrlsLimit", () =>
				{
					return new IntRegistryItem(
						"RespondedUrlsLimit",
						(NoResString)AutoresponderSubCategory,
						(NoResString)"Default count of Auto-suggested Urls",
						(NoResString)"Controls the default value for limiting the count of Auto-suggested Urls",
						RegistryStorageFlags.System,
						5
						);
				});
			}
		}

		#endregion

		#region Incident Close Prompt Service Task Last Run Time

		public DateTimeRegistryItem IncidentClosePromptServiceTaskLastRunTimeUtc
		{
			get
			{
				return GetItem("IncidentClosePromptServiceTaskLastRunTimeUtc", delegate
				{
					return new DateTimeRegistryItem(
						"IncidentClosePromptServiceTaskLastRunTimeUtc",
						(NoResString)CustomerServiceSubCategory,
						ResString.GetMultilingualString("ba115434-4aac-40e3-8557-6e216275264f", "Incident Close Prompt Service Task Last Run Time (UTC)."),
						ResString.GetMultilingualString("612c0fd1-7d1a-4ac6-a1b0-277961f06b80", "The Support Incident Close Prompt (SIC) service task will process incidents with their last edit time between this time and the current date."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						new ZDateTime(2021, 09, 13).ToDateTime()); //Default date set to approximate time when issue was identified to catch future incidents
				});
			}
		}

		#endregion

		#region Incident Management Groups
		public const string IncidentManagementGroups = CustomerServiceSubCategory + "/Incident Management Groups";

		public IncidentGroupStatusConfigurationRegistryItem StageAndDispositionsRegistryItem => GetItem("IncidentGroupStatusConfiguration",
					delegate
					{
						var incidentGroupStatusConfigurationDefault = new IncidentGroupTypeCollection();
						incidentGroupStatusConfigurationDefault.AddNew(IncidentGroupStatusConfigurationConstants.MajorIncidentCode, IncidentGroupStatusConfigurationConstants.MajorIncidentDescription, true);

						return new IncidentGroupStatusConfigurationRegistryItem(
							"IncidentGroupStatusConfiguration",
							(NoResString)IncidentManagementGroups,
							(NoResString)"Incident Group Stage Configuration",
							(NoResString)"Setup stage sequence, naming and behaviour for Incident Management Groups.",
							incidentGroupStatusConfigurationDefault);
					});

		public IntRegistryItem LinkedIncidentGridRefreshRate
		{
			get
			{
				return GetItem("LinkedIncidentGridRefreshRate", delegate
				{
					var result = new IntRegistryItem(
						"LinkedIncidentGridRefreshRate",
						(NoResString)IncidentManagementGroups,
						(NoResString)"Linked Incident Grid Refresh Rate",
						(NoResString)@"The number of minutes after which the Linked Incidents grid should automatically refresh. A value of 0 disables automatic refresh of the Linked Incidents grid.",
						RegistryStorageFlags.System,
						defaultValue: 5);
					result.DataType = new IntRegistryDataType(1, 30);
					return result;
				});
			}
		}
		#endregion

		#region Resolution and Closure Behaviour

		public ResolutionAndClosureBehaviourRegistryItem ResolutionAndClosureBehaviour
		{
			get
			{
				return GetItem(
					"ResolutionAndClosureBehaviour",
					delegate
					{
						var editorInfo = new ResolutionAndClosureBehaviourRegistryEditorInfo(
							new MultilingualString[]
							{
								(NoResString)"Criticality",
								(NoResString)"Product"
							},
							(NoResString)"Enabled",
							null, true, false,
							new bool[] { false, false },
							new bool[] { false, true });

						var allDescriptions = new MultilingualString[]
							{
								(NoResString)"All Criticalities that are not listed",
								(NoResString)"All Products that are not listed"
							};

						var codeLists = new CodeDescriptionPairList[]
							{
								new IncidentApprovalLookups(null).CriticalityList,
								new SupportIncidentLookups(new BusinessObjectFactory()).ProductList,

								null
							};

						var defaultValue = new ResolutionAndClosureBehaviourCollection(true, 3, 3, allDescriptions, codeLists);
						defaultValue.AddSystemChildren();

						return new ResolutionAndClosureBehaviourRegistryItem(
							"ResolutionAndClosureBehaviour",
							(NoResString)CustomerServiceSubCategory,
							(NoResString)"Resolution and Closure Behaviour",
							(NoResString)"Configure the behaviour of the CLOSED eRequest status and set number of days to auto-close from RESOLVED and AWAITING CUSTOMER RESPONSE",
							RegistryStorageFlags.System,
							editorInfo,
							defaultValue);
					});
			}
		}

		#endregion

		#region ERICA Azure SDK Configuration

		public const string EricaAzureSubCategory = CustomerServiceSubCategory + "/ERICA Azure Configurations";

		public StringRegistryItem AzureSearchServiceEndpoint
		{
			get
			{
				return GetItem(
					"AzureSearchServiceEndpoint",
					delegate
					{
						var registryItem = new StringRegistryItem(
							"AzureSearchServiceEndpoint",
							(NoResString)EricaAzureSubCategory,
							(NoResString)"Azure Search Service Endpoint",
							(NoResString)"The endpoint for the Azure Search Service",
							RegistryStorageFlags.System,
							string.Empty);
						return registryItem;
					});
			}
		}

		public StringRegistryItem AzureSearchIndex
		{
			get
			{
				return GetItem(
					"AzureSearchIndex",
					delegate
					{
						var registryItem = new StringRegistryItem(
							"AzureSearchIndex",
							(NoResString)EricaAzureSubCategory,
							(NoResString)"Azure Search Index",
							(NoResString)"Azure Search Index",
							RegistryStorageFlags.System,
							string.Empty);
						return registryItem;
					});
			}
		}

		public StringRegistryItem AzureOpenAIEndpoint
		{
			get
			{
				return GetItem(
					"AzureOpenAIEndpoint",
					delegate
					{
						var registryItem = new StringRegistryItem(
							"AzureOpenAIEndpoint",
							(NoResString)EricaAzureSubCategory,
							(NoResString)"Azure OpenAI Endpoint",
							(NoResString)"The endpoint for Azure OpenAI",
							RegistryStorageFlags.System,
							string.Empty);
						return registryItem;
					});
			}
		}

		public StringRegistryItem AzureOpenAIEmbeddingDeployment
		{
			get
			{
				return GetItem(
					"AzureOpenAIEmbeddingDeployment",
					delegate
					{
						var registryItem = new StringRegistryItem(
							"AzureOpenAIEmbeddingDeployment",
							(NoResString)EricaAzureSubCategory,
							(NoResString)"Azure OpenAI Embedding Deployment",
							(NoResString)"The name for the embedding deployment in Azure OpenAI",
							RegistryStorageFlags.System,
							string.Empty);
						return registryItem;
					});
			}
		}

		public StringRegistryItem AzureSearchAdminKey
		{
			get
			{
				return GetItem(
					"AzureSearchAdminKey",
					delegate
					{
						var registryItem = new StringRegistryItem(
							"AzureSearchAdminKey",
							(NoResString)EricaAzureSubCategory,
							(NoResString)"Azure Search Admin Key",
							(NoResString)"The key for the Azure Search Administrator Credentials",
							RegistryStorageFlags.System,
							string.Empty);
						return registryItem;
					});
			}
		}

		public StringRegistryItem AzureOpenAIKey
		{
			get
			{
				return GetItem(
					"AzureOpenAIKey",
					delegate
					{
						var registryItem = new StringRegistryItem(
							"AzureOpenAIKey",
							(NoResString)EricaAzureSubCategory,
							(NoResString)"Azure OpenAI Key",
							(NoResString)"The key for the Azure OpenAI Credentials",
							RegistryStorageFlags.System,
							string.Empty);
						return registryItem;
					});
			}
		}

		public IntRegistryItem AzureOpenAIEmbeddingDimensions
		{
			get
			{
				return GetItem(
					"AzureOpenAIEmbeddingDimensions",
					delegate
					{
						var registryItem = new IntRegistryItem(
							"AzureOpenAIEmbeddingDimensions",
							(NoResString)EricaAzureSubCategory,
							(NoResString)"Azure OpenAI Embedding Dimensions",
							(NoResString)"The Dimensions for the Azure OpenAI Embedding",
							RegistryStorageFlags.System,
							1536);
						return registryItem;
					});
			}
		}

		#endregion

		public const string AIFeaturesSubCategory = CustomerServiceSubCategory + "/AI Features";

		public CodeDescriptionPairListRegistryItem TriagePrediction
		{
			get
			{
				return GetItem(
				"TriagePrediction",
				delegate
				{
					var editorInfo = new CodeDescriptionPairListEditorInfo(showCodeColumn: true, showDescriptionColumn: false);
					return new CodeDescriptionPairListRegistryItem(
						"TriagePrediction",
						(NoResString)AIFeaturesSubCategory,
						(NoResString)"Triage Prediction",
						(NoResString)"Enable Triage Prediction for Products",
						3,
						editorInfo,
						RegistryStorageFlags.System,
						false,
						RegistryOptions.Default,
						new ReadOnlyCodeDescriptionPairList(),
						false
					);
				});
			}
		}
		#endregion

		#region Professional Service Quotes

		public const string PSQuotesSubCategory = Category + "/Professional Service Quotes";

		#region Email Notification on Assigned Change

		public BooleanRegistryItem EnableProfessionalServiceQuoteAssignedStaffChangedEmailNotificationRegistryItem
		{
			get
			{
				return GetItem("EnableProfessionalServiceQuoteAssignedStaffChangedEmailNotification", delegate
				{
					return new BooleanRegistryItem(
						"EnableProfessionalServiceQuoteAssignedStaffChangedEmailNotification",
						(NoResString)PSQuotesSubCategory,
						(NoResString)"Email Notification for Assigned Change",
						(NoResString)"When the Assigned Staff changes for a Professional Service Quote, the new Assigned Staff will be sent an email notification if this Registry is enabled.",
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public StringRegistryItem DefaultOpportunityObjective
		{
			get
			{
				return GetItem(
					"DefaultOpportunityObjective",
					delegate
					{
						StringRegistryItem registryItem = new StringRegistryItem(
							"DefaultOpportunityObjective",
							(NoResString)PSQuotesSubCategory,
							(NoResString)"Default Opportunity Objective ",
							(NoResString)"The default objective when creating a new opportunity from PSQ",
							RegistryStorageFlags.System,
							"SER");
						return registryItem;
					});
			}
		}

		#endregion

		#endregion

		#region Projects

		public const string ProjectsSubCategory = Category + "/Projects";

		#region Project Invoice Email Notification Group

		public GuidRegistryItem ProjectInvoiceEmailNotificationGroup
		{
			get
			{
				return GetItem("ProjectInvoiceEmailNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"ProjectInvoiceEmailNotificationGroup",
						(NoResString)ProjectsSubCategory,
						(NoResString)"Project Invoice Email Notification Group",
						(NoResString)"The staff group that will be notified about project invoicing.",
						RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#endregion

		#region Module to ResourceName

		public const string MasterScheduleSubCategory = Category + "/Master Schedule";

		public CodeDescriptionPairListRegistryItem ModuleToResourceNameList
		{
			get
			{
				return GetItem("ModuleToResourceNameList", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
							"ModuleToResourceNameList",
							(NoResString)MasterScheduleSubCategory,
							(NoResString)"Predefined Module To Resource Names",
							null,
							3,
							RegistryStorageFlags.System,
							false,
							DefaultModuleToResourceNameList);
				});
			}
		}

		CodeDescriptionPairList defaultModuleToResourceNameList;
		CodeDescriptionPairList DefaultModuleToResourceNameList
		{
			get
			{
				if (defaultModuleToResourceNameList == null)
				{
					defaultModuleToResourceNameList = new CodeDescriptionPairList();
					defaultModuleToResourceNameList.AddPair("AWB", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("BCL", "Warehouse Developer[50%]");
					defaultModuleToResourceNameList.AddPair("BAK", "Batch and Interface Developer[50%]");
					defaultModuleToResourceNameList.AddPair("EML", "Batch and Interface Developer[50%]");
					defaultModuleToResourceNameList.AddPair("PRN", "Batch and Interface Developer[50%]");
					defaultModuleToResourceNameList.AddPair("CIM", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("CPJ", "PST Developer[50%]");
					defaultModuleToResourceNameList.AddPair("VIM", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("ACC", "Accounting & Reporting Developer[50%]");
					defaultModuleToResourceNameList.AddPair("ACD", "CargoWise Internal Developer[50%]");
					defaultModuleToResourceNameList.AddPair("ACR", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("ARM", "Core Developer[50%]");
					defaultModuleToResourceNameList.AddPair("BDS", "Customs Developer[50%]");
					defaultModuleToResourceNameList.AddPair("BOO", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("BRK", "Customs Developer[50%]");
					defaultModuleToResourceNameList.AddPair("BRB", "Customs Developer[50%]");
					defaultModuleToResourceNameList.AddPair("BAE", "Customs Developer[50%]");
					defaultModuleToResourceNameList.AddPair("BAU", "Customs Developer[50%]");
					defaultModuleToResourceNameList.AddPair("AIR", "Customs Developer[50%]");
					defaultModuleToResourceNameList.AddPair("ACF", "Customs Developer[50%]");
					defaultModuleToResourceNameList.AddPair("ACT", "Customs Developer[50%]");
					defaultModuleToResourceNameList.AddPair("SEA", "Customs Developer[50%]");
					defaultModuleToResourceNameList.AddPair("SCC", "Customs Developer[50%]");
					defaultModuleToResourceNameList.AddPair("BCA", "Customs Developer[50%]");
					defaultModuleToResourceNameList.AddPair("BGB", "Customs Developer[50%]");
					defaultModuleToResourceNameList.AddPair("BHK", "Customs Developer[50%]");
					defaultModuleToResourceNameList.AddPair("BMY", "Customs Developer[50%]");
					defaultModuleToResourceNameList.AddPair("BNZ", "Customs Developer[50%]");
					defaultModuleToResourceNameList.AddPair("BSG", "Customs Developer[50%]");
					defaultModuleToResourceNameList.AddPair("BUS", "Customs Developer[50%]");
					defaultModuleToResourceNameList.AddPair("BZA", "Customs Developer[50%]");
					defaultModuleToResourceNameList.AddPair("CAM", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("CFS", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("CQW", "Accounting & Reporting Developer[50%]");
					defaultModuleToResourceNameList.AddPair("CON", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("COR", "Core Developer[50%]");
					defaultModuleToResourceNameList.AddPair("DCM", "Core Developer[50%]");
					defaultModuleToResourceNameList.AddPair("DSS", "Core Developer[50%]");
					defaultModuleToResourceNameList.AddPair("DOC", "Document Engine Developer[20%]");
					defaultModuleToResourceNameList.AddPair("DRB", "Accounting & Reporting Developer[50%]");
					defaultModuleToResourceNameList.AddPair("XBR", "Customs Developer[50%]");
					defaultModuleToResourceNameList.AddPair("EXD", "Customs Developer[50%]");
					defaultModuleToResourceNameList.AddPair("FAX", "Core Developer[50%]");
					defaultModuleToResourceNameList.AddPair("FSV", "Core Developer[50%]");
					defaultModuleToResourceNameList.AddPair("FOR", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("HVS", "");
					defaultModuleToResourceNameList.AddPair("IBR", "Customs Developer[50%]");
					defaultModuleToResourceNameList.AddPair("IMF", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("IFC", "Batch and Interface Developer[50%]");
					defaultModuleToResourceNameList.AddPair("LDC", "Accounting & Reporting Developer[50%]");
					defaultModuleToResourceNameList.AddPair("LOC", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("MFT", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("OPP", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("ORD", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("REC", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("ORG", "Core Developer[50%]");
					defaultModuleToResourceNameList.AddPair("REF", "Core Developer[50%]");
					defaultModuleToResourceNameList.AddPair("REP", "Accounting & Reporting Developer[50%]");
					defaultModuleToResourceNameList.AddPair("SAL", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("CLR", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("COL", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("CTF", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("COS", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("QTE", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("SCH", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("SCD", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("SCR", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("SHM", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("SMD", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("SMB", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("SMC", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("SED", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("SID", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("SDO", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("SPA", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("TAR", "Customs Developer[50%]");
					defaultModuleToResourceNameList.AddPair("WAR", "Warehouse Developer[50%]");
					defaultModuleToResourceNameList.AddPair("WDF", "Warehouse Developer[50%]");
					defaultModuleToResourceNameList.AddPair("WEB", "Web Developer[45%]");
					defaultModuleToResourceNameList.AddPair("PRO", "Core Developer[50%]");
					defaultModuleToResourceNameList.AddPair("EMF", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("ECI", "Batch and Interface Developer[50%]");
					defaultModuleToResourceNameList.AddPair("INC", "CargoWise Internal Developer[50%]");
					defaultModuleToResourceNameList.AddPair("TRN", "CargoWise Internal Developer[50%]");
					defaultModuleToResourceNameList.AddPair("PRA", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("TBU", "Batch and Interface Developer[50%]");
					defaultModuleToResourceNameList.AddPair("UPG", "PST Developer[50%]");
					defaultModuleToResourceNameList.AddPair("LON", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("EXF", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("CUS", "Customs Developer[50%]");
					defaultModuleToResourceNameList.AddPair("OTH", "");
					defaultModuleToResourceNameList.AddPair("IFR", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("DMF", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("WMS", "Warehouse Developer[50%]");
					defaultModuleToResourceNameList.AddPair("STX", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("OCN", "Freight/Sales/Marketing Developer[50%]");
					defaultModuleToResourceNameList.AddPair("EDI", "Batch and Interface Developer[50%]");
				}
				return defaultModuleToResourceNameList;
			}
		}

		#endregion

		#region Release Builds and Upgrades

		public const string ReleaseBuildsAndUpgradesSubCategory = Category + "/Release Builds & Upgrades";

		#region NeoUpgradeLicences

		public NeoUpgradeLicencesRegistryItem NeoUpgradeLicences
		{
			get
			{
				return GetItem("NeoUpgradeLicences", delegate
				{
					return new NeoUpgradeLicencesRegistryItem(
						"NeoUpgradeLicences",
						(NoResString)ReleaseBuildsAndUpgradesSubCategory,
						(NoResString)"Neo Enabled Licences",
						(NoResString)"List of licences for which Neo upgrade package should be created.",
						RegistryStorageFlags.System,
						new NeoUpgradeLicenceCollection());
				});
			}
		}

		#endregion

		#region WinzorLicences

		public NeoUpgradeLicencesRegistryItem WinzorLicences
		{
			get
			{
				return GetItem("WinzorLicences", delegate
				{
					return new NeoUpgradeLicencesRegistryItem(
						"WinzorLicences",
						(NoResString)ReleaseBuildsAndUpgradesSubCategory,
						(NoResString)"Winzor Enabled Licences",
						(NoResString)"List of licences for which winzor package should be created.",
						RegistryStorageFlags.System,
						new NeoUpgradeLicenceCollection());
				});
			}
		}

		public NeoUpgradeLicencesRegistryItem NetCoreBinaryLicences
		{
			get
			{
				return GetItem("NetCoreBuildLicences", delegate
				{
					return new NeoUpgradeLicencesRegistryItem(
						"NetCoreBuildLicences",
						(NoResString)ReleaseBuildsAndUpgradesSubCategory,
						(NoResString)"NetCore Build Enabled Licences",
						(NoResString)"List of licences for which NetCore Build Binary package should be created.",
						RegistryStorageFlags.System,
						new NeoUpgradeLicenceCollection());
				});
			}
		}

		#endregion

		#region Auto Deploy Max Duration

		public IntRegistryItem AutoDeployProcessBatchMaxDuration
		{
			get
			{
				return GetItem("AutoDeployProcessBatchMaxDuration", delegate
				{
					return new IntRegistryItem("AutoDeployProcessBatchMaxDuration",
						(NoResString)ReleaseBuildsAndUpgradesSubCategory,
						(NoResString)"Max duration in seconds for a AutoDeploy batchprocess one batch",
						(NoResString)"If duration of the batch is longer that specified interval it will leave the loop of loading new records and finish current batch",
						RegistryStorageFlags.System,
						300);
				});
			}
		}

		#endregion

		#region Auto Deploy Running Interval

		public IntRegistryItem AutoDeployProcessBatchRunningInterval
		{
			get
			{
				return GetItem("AutoDeployProcessBatchRunningInterval", delegate
				{
					return new IntRegistryItem("AutoDeployProcessBatchRunningInterval",
						(NoResString)ReleaseBuildsAndUpgradesSubCategory,
						(NoResString)"Interval in seconds between the AutoDeploy's batches",
						(NoResString)"Interval between finishing the previous batch and starting a new one",
						RegistryStorageFlags.System,
						300);
				});
			}
		}

		#endregion

		#region Remote Access Methods

		public CodeDescriptionPairListRegistryItem RemoteAccessMethodDescriptionList
		{
			get
			{
				return GetItem("EDIRemoteAccessMethodDescriptionList", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						"EDIRemoteAccessMethodDescriptionList",
						(NoResString)ReleaseBuildsAndUpgradesSubCategory,
						(NoResString)"Additional Remote Access Connection Types",
						null,
						3,
						RegistryStorageFlags.System,
						new ReadOnlyCodeDescriptionPairList());
				});
			}
		}

		#endregion

		#region PackageArchive

		public ZString MasterPackageArchivePath
		{
			get { return new ZString(MasterPackageArchivePathItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { MasterPackageArchivePathItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		IRegistryItem MasterPackageArchivePathItem
		{
			get
			{
				return GetItem("MasterPackageArchivePath", delegate
				{
					return new StringRegistryItem("MasterPackageArchivePath",
						(NoResString)ReleaseBuildsAndUpgradesSubCategory,
						(NoResString)"Master Package Archive Path",
						(NoResString)"Path where the Import Builds Service Task will search for the latest packages deployed from DAT",
						RegistryStorageFlags.System,
						@"\\cw1datfiles.wtg.zone\CW1Packages");
				});
			}
		}

		public DateTime LastIBPPackageCheck
		{
			get { return (DateTime)LastIBPPackageCheckItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { LastIBPPackageCheckItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		IRegistryItem LastIBPPackageCheckItem
		{
			get
			{
				return GetItem("LastIBPPackageCheck", delegate
				{
					return new DateTimeRegistryItem("LastIBPPackageCheck",
						(NoResString)ReleaseBuildsAndUpgradesSubCategory,
						(NoResString)"Last Import Build Processor Check",
						(NoResString)"Import Build Processor package file date high water mark (UTC)",
						RegistryStorageFlags.System,
						DateTime.MinValue);
				});
			}
		}

		#endregion

		#region StackLineCountImport
		public const string StackLineCountImportSubCategory = Category + "/Stack Line Count Import";

		public DateTime StackLineCountLastImport
		{
			get { return (DateTime)StackLineCountLastImportItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { StackLineCountLastImportItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		IRegistryItem StackLineCountLastImportItem
		{
			get
			{
				return GetItem(nameof(StackLineCountLastImport), () =>
				{
					return new DateTimeRegistryItem(nameof(StackLineCountLastImport),
						(NoResString)StackLineCountImportSubCategory,
						(NoResString)"Last stack line count update date",
						(NoResString)"Last stack line count update date",
						RegistryStorageFlags.System,
						new ZDateTime(2013, 1, 1).ToDateTime());
				});
			}
		}

		public int StackLineCountNumberOfImportedLogs
		{
			get { return (int)StackLineCountNumberOfImportedLogsImportItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { StackLineCountNumberOfImportedLogsImportItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		IRegistryItem StackLineCountNumberOfImportedLogsImportItem
		{
			get
			{
				return GetItem(nameof(StackLineCountNumberOfImportedLogs), delegate
				{
					return new IntRegistryItem(nameof(StackLineCountNumberOfImportedLogs),
						(NoResString)StackLineCountImportSubCategory,
						(NoResString)"Number of logs imported by the stack line count import task",
						(NoResString)"Number of logs imported by the stack line count import task",
						RegistryStorageFlags.System,
						0);
				});
			}
		}
		#endregion

		#region Send Upgrade Email Notification Automatically

		public BooleanRegistryItem SendUpgradeEmailNotificationAutomatically
		{
			get
			{
				return GetItem("SendUpgradeEmailNotificationAutomatically", delegate
				{
					return new BooleanRegistryItem(
						"SendUpgradeEmailNotificationAutomatically",
						(NoResString)ReleaseBuildsAndUpgradesSubCategory,
						(NoResString)"Send Upgrade Email Notification Automatically",
						(NoResString)"This is the default value for the 'Send Email Notification Automatically' field in the upgrade form that appears when you send upgrades to clients.",
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#region UseAutoDeployBatchProcessorForUpgrades

		public BooleanRegistryItem UseAutoDeployBatchProcessorForUpgrades
		{
			get
			{
				return GetItem("UseAutoDeployBatchProcessorForUpgrades", delegate
				{
					return new BooleanRegistryItem("UseAutoDeployBatchProcessorForUpgrades",
						(NoResString)ReleaseBuildsAndUpgradesSubCategory,
						(NoResString)"Use AutoDeploy BatchProcessor for sending Upgrades",
						(NoResString)"Should place UpgradesToClients records and use AutoDeploy batch processor for sending upgrade packages instead of direct sending upgrades",
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#region Upgrade Package Paths

		public string WebServerClientSpecificPath
		{
			get { return WebServerClientSpecificPathRaw.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { WebServerClientSpecificPathRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string WebServerGenericPath
		{
			get { return WebServerGenericPathRaw.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { WebServerGenericPathRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		public string HttpClientSpecificBaseUrl
		{
			get { return HttpClientSpecificBaseUrlRaw.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { HttpClientSpecificBaseUrlRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		public string HttpGenericBaseUrl
		{
			get { return HttpGenericBaseUrlRaw.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { HttpGenericBaseUrlRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string WebServerUserName
		{
			get { return WebServerUserNameRaw.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { WebServerUserNameRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string WebServerPassword
		{
			get { return WebServerPasswordRaw.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { WebServerPasswordRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string HttpDownloadUserName
		{
			get { return HttpDownloadUserNameRaw.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { HttpDownloadUserNameRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string HttpDownloadPassword
		{
			get { return HttpDownloadPasswordRaw.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { HttpDownloadPasswordRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public StringRegistryItem WebServerClientSpecificPathRaw
		{
			get
			{
				return GetItem("WEB_SERVER_CLIENT_SPECIFIC_PATH", delegate
				{
					return new StringRegistryItem(
						"WEB_SERVER_CLIENT_SPECIFIC_PATH",
						(NoResString)UpgradePackagePathsSubCategory,
						(NoResString)"Web Server Client Specific Path",
						(NoResString)"This is the directory that client specific upgrade packages will be stored in.",
						RegistryStorageFlags.System,
						@"\\web\updates\ediEnterprise\ClientSpecific\");
				});
			}
		}

		public StringRegistryItem WebServerGenericPathRaw
		{
			get
			{
				return GetItem("WEB_SERVER_GENERIC_PATH", delegate
				{
					return new StringRegistryItem(
						"WEB_SERVER_GENERIC_PATH",
						(NoResString)UpgradePackagePathsSubCategory,
						(NoResString)"Web Server Generic Path",
						(NoResString)"This is the directory that generic upgrade packages will be stored in.",
						RegistryStorageFlags.System,
						@"\\web\updates\ediEnterprise\Generic\");
				});
			}
		}

		public StringRegistryItem HttpClientSpecificBaseUrlRaw
		{
			get
			{
				return GetItem("HTTP_CLIENT_SPECIFIC_BASE_URL", delegate
				{
					return new StringRegistryItem(
						"HTTP_CLIENT_SPECIFIC_BASE_URL",
						(NoResString)UpgradePackagePathsSubCategory,
						(NoResString)"HTTP Client Specific Base URL",
						(NoResString)"This is the base HTTP URL for client specific upgrade packages.",
						RegistryStorageFlags.System,
						"http://www.cargowise.com/ftpmirror/ediEnterprise/ClientSpecific/");
				});
			}
		}

		public StringRegistryItem HttpGenericBaseUrlRaw
		{
			get
			{
				return GetItem("HTTP_GENERIC_BASE_URL", delegate
				{
					return new StringRegistryItem(
						"HTTP_GENERIC_BASE_URL",
						(NoResString)UpgradePackagePathsSubCategory,
						(NoResString)"HTTP Generic Base URL",
						(NoResString)"This is the base HTTP URL for generic upgrade packages.",
						RegistryStorageFlags.System,
						"http://www.cargowise.com/ftpmirror/ediEnterprise/Generic/");
				});
			}
		}

		public StringRegistryItem WebServerUserNameRaw
		{
			get
			{
				return GetItem("WEB_SERVER_USER_NAME", delegate
				{
					return new StringRegistryItem(
						"WEB_SERVER_USER_NAME",
						(NoResString)UpgradePackagePathsSubCategory,
						(NoResString)"Web Server User Name",
						(NoResString)"This is the user name to login and copy files onto webserver.",
						RegistryStorageFlags.System,
						"");
				});
			}
		}

		public StringRegistryItem WebServerPasswordRaw
		{
			get
			{
				return GetItem("WEB_SERVER_PASSWORD", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"WEB_SERVER_PASSWORD",
						(NoResString)UpgradePackagePathsSubCategory,
						(NoResString)"Web Server Password",
						(NoResString)"This is the password to login and copy files onto webserver.",
						new StringRegistryDataType(true),
						RegistryStorageFlags.System,
						"");
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password);
					return result;
				});
			}
		}

		public StringRegistryItem HttpDownloadUserNameRaw
		{
			get
			{
				return GetItem("HTTP_DOWNLOAD_USER_NAME", delegate
				{
					return new StringRegistryItem(
						"HTTP_DOWNLOAD_USER_NAME",
						(NoResString)UpgradePackagePathsSubCategory,
						(NoResString)"HTTP Download User Name",
						(NoResString)"This is the user name for secure HTTP downloads. Leave it blank if the URL's are not password protected.",
						RegistryStorageFlags.System,
						"");
				});
			}
		}

		public StringRegistryItem HttpDownloadPasswordRaw
		{
			get
			{
				return GetItem("HTTP_DOWNLOAD_PASSWORD", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"HTTP_DOWNLOAD_PASSWORD",
						(NoResString)UpgradePackagePathsSubCategory,
						(NoResString)"HTTP Download Password",
						(NoResString)"This is the password for secure HTTP downloads.",
						new StringRegistryDataType(true),
						RegistryStorageFlags.System,
						"");
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password);
					return result;
				});
			}
		}

		public CodeDescriptionPairListRegistryItem CorruptedUpgradePackageFilePath
		{
			get
			{
				return GetItem("CorruptedUpgradePackageFilePath", delegate
				{
					var result = new CodeDescriptionPairListRegistryItem(
						"CorruptedUpgradePackageFilePath",
						(NoResString)UpgradePackagePathsSubCategory,
						(NoResString)"Corrupted Upgrade File Path",
						(NoResString)"List of upgrade file path which encounter IO exception during file copying. The file in the list will be deleted in the next package file publish process.",
						20,
						RegistryStorageFlags.System,
						new ReadOnlyCodeDescriptionPairList());
					result.EditorInfo = new CodeDescriptionPairListEditorInfo(true, true,
						CodeDescriptionPairListEditorInfo.CharacterCasing.Normal, CodeDescriptionPairListEditorInfo.CharacterCasing.Normal,
						(NoResString)"Time Stamp",
						(NoResString)"File Path");

					return result;
				});
			}
		}

		public const string UpgradePackagePathsSubCategory = ReleaseBuildsAndUpgradesSubCategory + "/Upgrade Package Paths";

		#endregion

		#region Upgrade Email Default Additional Notification

		public string UpgradeEmailDefaultAdditionalNotification
		{
			get { return UpgradeEmailDefaultAdditionalNotificationRaw.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { UpgradeEmailDefaultAdditionalNotificationRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public StringRegistryItem UpgradeEmailDefaultAdditionalNotificationRaw
		{
			get
			{
				return GetItem("UPGRADE_EMAIL_DEFAULT_ADDITIONAL_NOTIFICATION", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"UPGRADE_EMAIL_DEFAULT_ADDITIONAL_NOTIFICATION",
						(NoResString)ReleaseBuildsAndUpgradesSubCategory,
						(NoResString)"Upgrade Email Default Additional Notification",
						(NoResString)"This is the default additional notification for upgrade emails.",
						RegistryStorageFlags.System);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		#endregion

		#region Incidents Batch Process Notification Group

		public Guid IncidentsBatchProcessNotificationGroup
		{
			get { return IncidentsBatchProcessNotificationGroupItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { IncidentsBatchProcessNotificationGroupItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public GuidRegistryItem IncidentsBatchProcessNotificationGroupItem
		{
			get
			{
				return GetItem("INCIDENTS_BATCH_PROCESS_NOTIFICATION_GROUP", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"INCIDENTS_BATCH_PROCESS_NOTIFICATION_GROUP",
						(NoResString)ReleaseBuildsAndUpgradesSubCategory,
						(NoResString)"Incidents Batch Process Notification Group",
						(NoResString)"This is the group that will be sent the Incidents batch processor reports.",
						RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Upgrade Package Delivery Notification Email Template

		public NotificationEmailTemplateRegistryItem UpgradePackageDeliveryNotificationEmailTemplateRaw
		{
			get
			{
				return GetItem(
					"UpgradePackageDeliveryNotificationEmailTemplate",
					delegate
					{
						NotificationEmailTemplateRegistryItem registryItem = new NotificationEmailTemplateRegistryItem(
							"UpgradePackageDeliveryNotificationEmailTemplate",
							(NoResString)ReleaseBuildsAndUpgradesSubCategory,
							(NoResString)"Upgrade Package Delivery Notification Email Template",
							(NoResString)"This is the default template for Upgrade Package Delivery notification emails.",
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							typeof(DocUpgradeRequestCollectionContainer),
							DefaultUpgradePackageDeliveryNotificationEmailSubject,
							DefaultUpgradePackageDeliveryNotificationEmailBody);
						return registryItem;
					});
			}
		}

		const string DefaultUpgradePackageDeliveryNotificationEmailSubject = "(*NotificationSubjectPrefix*)A System Upgrade Package is Available for (*ClientCode*)";

		const string DefaultUpgradePackageDeliveryNotificationEmailBody =
@"A new upgrade package is available for your system with the following details:

Release: (*ReleaseDescription*)
Exe Date: (*ReleaseExeDate*)
Version Number: (*ReleaseVersionNumber*)

(*AdditionalNotification*)
";

		#endregion

		#region Testing

		public const string ReleaseBuildTesting = ReleaseBuildsAndUpgradesSubCategory + "/Testing";

		public CodeDescriptionPairListRegistryItem DatabasesRequiredReleaseBuildTesting
		{
			get
			{
				return GetItem("DatabasesRequiredReleaseBuildTesting", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						"DatabasesRequiredReleaseBuildTesting",
						(NoResString)ReleaseBuildTesting,
						(NoResString)"Databases Required Release Build Testing",
						(NoResString)"The list of database number of databases which are required to only take release builds with test passed.",
						10,
						RegistryStorageFlags.System,
						new ReadOnlyCodeDescriptionPairList());
				});
			}
		}

		public StringRegistryItem ReleaseBuildTestDbServerName
		{
			get
			{
				return GetItem("ReleaseBuildTestDbServerName", delegate
				{
					return new StringRegistryItem(
						"ReleaseBuildTestDbServerName",
						(NoResString)ReleaseBuildTesting,
						(NoResString)"Release Build Test Db Server Name",
						null,
						RegistryStorageFlags.System,
						string.Empty);
				});
			}
		}

		public StringRegistryItem ReleaseBuildTestDbName
		{
			get
			{
				return GetItem("ReleaseBuildTestDbName", delegate
				{
					return new StringRegistryItem(
						"ReleaseBuildTestDbName",
						(NoResString)ReleaseBuildTesting,
						(NoResString)"Release Build Test Db Name",
						null,
						RegistryStorageFlags.System,
						string.Empty);
				});
			}
		}

		public StringRegistryItem ReleaseBuildTestRemoteCommandExecutablePath
		{
			get
			{
				return GetItem("ReleaseBuildTestRemoteCommandExecutablePath", delegate
				{
					return new StringRegistryItem(
						"ReleaseBuildTestRemoteCommandExecutablePath",
						(NoResString)ReleaseBuildTesting,
						(NoResString)"Release Build Test Remote Command Executable Path",
						null,
						RegistryStorageFlags.System,
						string.Empty);
				});
			}
		}

		public StringRegistryItem ReleaseBuildTestWebAppHost
		{
			get
			{
				return GetItem("ReleaseBuildTestWebAppHost", delegate
				{
					return new StringRegistryItem(
						"ReleaseBuildTestWebAppHost",
						(NoResString)ReleaseBuildTesting,
						(NoResString)"Release Build Test Web App Host",
						null,
						RegistryStorageFlags.System,
						string.Empty);
				});
			}
		}

		public StringRegistryItem ReleaseBuildTestWebAppUrl
		{
			get
			{
				return GetItem("ReleaseBuildTestWebAppUrl", delegate
				{
					return new StringRegistryItem(
						"ReleaseBuildTestWebAppUrl",
						(NoResString)ReleaseBuildTesting,
						(NoResString)"Release Build Test Web App Url",
						null,
						RegistryStorageFlags.System,
						string.Empty);
				});
			}
		}

		public GuidRegistryItem ReleaseBuildTestResultNotificationGroup
		{
			get
			{
				return GetItem("ReleaseBuildTestResultNotificationGroup", delegate
				{
					var result = new GuidRegistryItem(
						"ReleaseBuildTestResultNotificationGroup",
						(NoResString)ReleaseBuildTesting,
						(NoResString)"Release Build Test Result Notification Group",
						null,
						RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Supported Versions

		public const string ReleaseBuildSupportedVersions = ReleaseBuildsAndUpgradesSubCategory + "/Supported Versions";

		#region new eRequest versions

		public StringRegistryItem ERequestsReleaseBuilds
		{
			get
			{
				return GetItem(
					"ERequestsReleaseBuilds",
					delegate
					{
						StringRegistryItem registryItem = new StringRegistryItem(
							"ERequestsReleaseBuilds",
							(NoResString)ReleaseBuildSupportedVersions,
							(NoResString)"eRequest patched versions",
							(NoResString)"Comma separated list of release build versions that were patched with new eRequests (eConversation, etc). Build format should be major.minor.release.patch.",
							RegistryStorageFlags.System,
							"");
						return registryItem;
					});
			}
		}

		public StringRegistryItem ERequestV2ReleaseBuilds
		{
			get
			{
				return GetItem(
					"ERequestV2ReleaseBuilds",
					delegate
					{
						StringRegistryItem registryItem = new StringRegistryItem(
							"ERequestV2ReleaseBuilds",
							(NoResString)ReleaseBuildSupportedVersions,
							(NoResString)"eRequest V2 patched versions",
							(NoResString)"Comma separated list of release build versions that were patched with eRequest Version 2 (sending emails from client system, etc). Build format should be major.minor.release.patch.",
							RegistryStorageFlags.System,
							"1.4.4903.0");
						return registryItem;
					});
			}
		}

		public StringRegistryItem CustomExpiryMessagesReleaseBuilds
		{
			get
			{
				return GetItem(
					"CustomExpiryMessagesReleaseBuilds",
					delegate
					{
						StringRegistryItem registryItem = new StringRegistryItem(
							"CustomExpiryMessagesReleaseBuilds",
							(NoResString)ReleaseBuildSupportedVersions,
							(NoResString)"Custom Expiry Messages patched versions",
							(NoResString)"Comma separated list of release build versions that show custom expiry messages. Build format should be major.minor.release.patch.",
							RegistryStorageFlags.System,
							"2.0.76.0");
						return registryItem;
					});
			}
		}

		public StringRegistryItem AllSystemMessagesViaEhubReleaseBuilds
		{
			get
			{
				return GetItem(
					"AllSystemMessagesViaEhubReleaseBuilds",
					delegate
					{
						StringRegistryItem registryItem = new StringRegistryItem(
							"AllSystemMessagesViaEhubReleaseBuilds",
							(NoResString)ReleaseBuildSupportedVersions,
							(NoResString)"Receive system messages only via eHub versions",
							(NoResString)"Comma separated list of release build versions that receive all system messages (via eHub). Nothing is sent via email. Build format should be major.minor.release.patch.",
							RegistryStorageFlags.System,
							"16.10.20.0,16.10.19.143");
						return registryItem;
					});
			}
		}

		public StringRegistryItem HasGlowERequests
		{
			get
			{
				return GetItem(
					"HasGlowERequests",
					delegate
					{
						StringRegistryItem registryItem = new StringRegistryItem(
							"HasGlowERequests",
							(NoResString)ReleaseBuildSupportedVersions,
							(NoResString)"GLOW eRequest versions",
							(NoResString)"Comma separated list of release build versions that use the GLOW portal for eRequests, and not the CW1 embedded module",
							RegistryStorageFlags.System,
							"18.2.1.0");
						return registryItem;
					});
			}
		}

		#endregion

		#region CargoWise Next Version

		public StringRegistryItem CargoWiseNextMinimumVersion
		{
			get
			{
				return GetItem(
					"CargoWiseNextMinimumVersion",
					delegate
					{
						var registryItem = new StringRegistryItem(
							"CargoWiseNextMinimumVersion",
							(NoResString)ReleaseBuildSupportedVersions,
							(NoResString)"CargoWise Next minimum version",
							(NoResString)"First release build version to be imported as CargoWise Next product. Build version format should be major.minor.release.0.",
							RegistryStorageFlags.System,
							string.Empty);
						return registryItem;
					});
			}
		}

		public BooleanRegistryItem EnableCargoWiseNextTransitionVersionFallbackRule
		{
			get
			{
				return GetItem("EnableCargoWiseNextTransitionVersionFallbackRule", delegate
				{
					return new BooleanRegistryItem(
						"EnableCargoWiseNextTransitionVersionFallbackRule",
						(NoResString)ReleaseBuildSupportedVersions,
						(NoResString)"Enable CargoWise Next Transition Version Fallback Rule",
						(NoResString)"",
						RegistryStorageFlags.System,
						RegistryOptions.NotCached,
						true);
				});
			}
		}

		public StringRegistryItem CargoWiseMinimumVersion
		{
			get
			{
				return GetItem(
					"CargoWiseMinimumVersion",
					delegate
					{
						var registryItem = new StringRegistryItem(
							"CargoWiseMinimumVersion",
							(NoResString)ReleaseBuildSupportedVersions,
							(NoResString)"CargoWise minimum version",
							(NoResString)"First release build version to be imported as unified CargoWise product. Build version format should be major.minor.release.0.",
							RegistryStorageFlags.System,
							string.Empty);
						return registryItem;
					});
			}
		}

		#endregion

		#endregion

		#region CR8/CR9 Release Builds

		public StringRegistryItem Cr8Cr9ReleaseBuilds
		{
			get
			{
				return GetItem(
					"Cr8Cr9ReleaseBuilds",
					delegate
					{
						StringRegistryItem registryItem = new StringRegistryItem(
							"Cr8Cr9ReleaseBuilds",
							(NoResString)ReleaseBuildSupportedVersions,
							(NoResString)"CR8/CR9 patched versions",
							(NoResString)"Comma separated list of release build versions that were patched with CR8/CR9. Build format should be major.minor.release.patch.",
							RegistryStorageFlags.System,
							"");
						return registryItem;
					});
			}
		}

		#endregion

		#region hybrid licencing versions

		public StringRegistryItem HybridLicencingReleaseBuilds
		{
			get
			{
				return GetItem(
					"HybridLicencingReleaseBuilds",
					delegate
					{
						StringRegistryItem registryItem = new StringRegistryItem(
							"HybridLicencingReleaseBuilds",
							(NoResString)ReleaseBuildSupportedVersions,
							(NoResString)"Hybrid Licencing patched versions",
							(NoResString)"Comma separated list of release build versions that were patched with new Hybrid Licencing. Build format should be major.minor.release.patch.",
							RegistryStorageFlags.System,
							"");
						return registryItem;
					});
			}
		}

		#endregion

		#region CW1 user created company version

		public StringRegistryItem UserCreatedCompanyReleaseBuilds
		{
			get
			{
				return GetItem(
					"UserCreatedCompanyReleaseBuilds",
					delegate
					{
						StringRegistryItem registryItem = new StringRegistryItem(
							"UserCreatedCompanyReleaseBuilds",
							(NoResString)ReleaseBuildSupportedVersions,
							(NoResString)"User Create Company version(s)",
							(NoResString)"Comma separated list of release build versions that support user created companies. Build format should be major.minor.release.patch.",
							RegistryStorageFlags.System,
							"");
						return registryItem;
					});
			}
		}

		#endregion

		#endregion

		#region Version Reporting

		public const string VersionReportingSubCategory = Category + "/Version Reporting";

		#endregion

		#region Mail

		public static MultilingualString EmailAddressesCategory { get { return (NoResString)(Category + "/System Email Addresses"); } }

		#region Support

		public StringArrayRegistryItem EmailAddressBlockList
		{
			get
			{
				return GetItem("EmailAddressBlockList", delegate
				{
					var item = new StringArrayRegistryItem(
							"EmailAddressBlockList",
							EmailAddressesCategory,
							(NoResString)"Email Address Block List",
							(NoResString)"CSE ServiceTask will only add emails from this address list to eDoc and will not perform any additional processing.",
							RegistryStorageFlags.System);

					item.DataType.Validating -= EmailAddressBlockList_Validating;
					item.DataType.Validating += EmailAddressBlockList_Validating;

					return item;
				});
			}
		}

		void EmailAddressBlockList_Validating(object sender, RegistryDataTypeValidatingEventArgs<string[]> e)
		{
			var invalidAddress = e.ProposedValue.Where(x => !EmailAddressValidation.IsEmailAddressValid(x));
			if (invalidAddress.Any())
			{
				throw new RegistryValidationException((NoResString)$"{string.Join(", ", invalidAddress)} are not valid email addresses.");
			}
		}

		public StringRegistryItem IncidentFromEmailAddress
		{
			get
			{
				return GetItem("IncidentFromEmailAddress", delegate
				{
					return new StringRegistryItem(
						"IncidentFromEmailAddress",
						EmailAddressesCategory,
						(NoResString)"Customer Service Email Address",
						(NoResString)"This is the 'From' address that will appear on Customer Service emails sent from the Customer Service team.",
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						"support@wisetechglobal.com");
				});
			}
		}

		#endregion

		#region Training Scheduling

		public StringRegistryItem TrainingSchedulingEmailAddress
		{
			get
			{
				return GetItem("TrainingSchedulingEmailAddress", delegate
				{
					return new StringRegistryItem(
						"TrainingSchedulingEmailAddress",
						EmailAddressesCategory,
						(NoResString)"Training Scheduling Email Address",
						(NoResString)"This is the email address of the Training Scheduling team",
						RegistryStorageFlags.Company,
						"trainingcentre@cargowise.com");
				});
			}
		}

		#endregion

		#region Implementation Scheduling

		public StringRegistryItem ImplementationSchedulingEmailAddress
		{
			get
			{
				return GetItem("ImplementationSchedulingEmailAddress", delegate
				{
					return new StringRegistryItem(
						"ImplementationSchedulingEmailAddress",
						EmailAddressesCategory,
						(NoResString)"Implementation Scheduling Email Address",
						(NoResString)"This is the email address of the Implementation Scheduling team",
						RegistryStorageFlags.Company,
						"implementationscheduling@cargowise.com");
				});
			}
		}

		#endregion

		#region Implementation Default Email Address

		public StringRegistryItem ImplementationDefaultFromEmailAddress
		{
			get
			{
				return GetItem("ImplementationDefaultFromEmailAddress", delegate
				{
					return new StringRegistryItem(
						"ImplementationDefaultFromEmailAddress",
						EmailAddressesCategory,
						(NoResString)"Implementation Default From Email Address",
						(NoResString)"This is the 'From' address that will appear on emails sent from the Implementation team.",
						RegistryStorageFlags.Company,
						"implementation@cargowise.com");
				});
			}
		}

		#endregion

		#region Classroom Sessions

		public StringRegistryItem TrainingClassroomSessionsEmailAddress
		{
			get
			{
				return GetItem("EDITrainingDepartmentEmailAddress", delegate
				{
					return new StringRegistryItem(
						"EDITrainingDepartmentEmailAddress",
						EmailAddressesCategory,
						(NoResString)"Training Classroom Sessions Email Address",
						null,
						RegistryStorageFlags.Company,
						"classrooms@cargowise.com");
				});
			}
		}

		#endregion

		#region Accounts

		public StringRegistryItem AccountingDepartmentEmailAddress
		{
			get
			{
				return GetItem("EDIAccountingDepartmentEmailAddress", delegate
				{
					return new StringRegistryItem(
						"EDIAccountingDepartmentEmailAddress",
						EmailAddressesCategory,
						(NoResString)"Accounting Department Email Address ",
						null,
						RegistryStorageFlags.Company,
						"accounts@cargowise.com");
				});
			}
		}

		#endregion

		#region Go Live Updates

		public StringRegistryItem GoLiveUpdatesEmailAddress
		{
			get
			{
				return GetItem("EDIGoLiveUpdatesEmailAddress", delegate
				{
					return new StringRegistryItem(
						"EDIGoLiveUpdatesEmailAddress",
						EmailAddressesCategory,
						(NoResString)"Go Live Updates Email Address ",
						null,
						RegistryStorageFlags.Company,
						"goliveupdates@cargowise.com");
				});
			}
		}

		#endregion

		#region EnableVerboseModeOnEmailProcessors

		public BooleanRegistryItem EnableVerboseModeOnEmailProcessors
		{
			get
			{
				return GetItem("EnableVerboseModeOnEmailProcessors", delegate
				{
					return new BooleanRegistryItem(
						"EnableVerboseModeOnEmailProcessors",
						EmailAddressesCategory,
						(NoResString)"Enable Verbose Mode on Email Processors",
						(NoResString)"This provides additional logging info to help troubleshooting email processors issues.",
						RegistryStorageFlags.System,
						RegistryOptions.NotCached,
						false);
				});
			}
		}

		#endregion

		#region Email Loop Detection

		public static MultilingualString EmailLoopDetectionCategory { get { return (NoResString)(EmailAddressesCategory + "/Email Loop Detection"); } }

		public BooleanRegistryItem EnableEmailLoopDetection
		{
			get
			{
				return GetItem("EnableEmailLoopDetection", delegate
				{
					return new BooleanRegistryItem(
						"EnableEmailLoopDetection",
						EmailLoopDetectionCategory,
						(NoResString)"Enable Email Loop Detection",
						(NoResString)"This will prevent email loops by checking the email headers.",
						RegistryStorageFlags.System,
						RegistryOptions.NotCached,
						true);
				});
			}
		}

		public IntRegistryItem EmailLoopDetectingWindowDuration
		{
			get
			{
				return GetItem("EmailLoopDetectingWindowDuration", delegate
				{
					return new IntRegistryItem(
						"EmailLoopDetectingWindowDuration",
						EmailLoopDetectionCategory,
						(NoResString)"Email Loop Detecting Window Duration",
						(NoResString)"The time window in minutes to check for email loops.",
						RegistryStorageFlags.System,
						RegistryOptions.NotCached,
						defaultValue: 3,
						minValue: 1,
						maxValue: 60);
				});
			}
		}

		public IntRegistryItem EmailLoopMaximumEmailCount
		{
			get
			{
				return GetItem("EmailLoopMaximumEmailCount", delegate
				{
					return new IntRegistryItem(
						"EmailLoopMaximumEmailCount",
						EmailLoopDetectionCategory,
						(NoResString)"Email Loop Maximum Email Count",
						(NoResString)"The maximum number of emails to check for email loops.",
						RegistryStorageFlags.System,
						RegistryOptions.NotCached,
						defaultValue: 15,
						minValue: 2,
						maxValue: 999);
				});
			}
		}

		#endregion

		#endregion

		#region Processed Shelfs

		public const string ProcessedShelfsCategory = Category + "/Processed Shelfs";

		public GuidRegistryItem ProcessedShelfsNotificationGroup
		{
			get
			{
				return GetItem("ProcessedShelfsNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"ProcessedShelfsNotificationGroup",
						(NoResString)ProcessedShelfsCategory,
						(NoResString)"Notification Group",
						(NoResString)"Group which gets notification emails from the Processed Shelfs Batch Processor.",
						RegistryStorageFlags.System,
						RegistryFactory.Instance.GetGroupPK("PMG"));
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public QualityIterationAssignmentRegistryItem QualityIterationAssignments
		{
			get
			{
				return GetItem("QualityIterationAssignments",
					() => new QualityIterationAssignmentRegistryItem("QualityIterationAssignments",
						(NoResString)ProcessedShelfsCategory,
						(NoResString)"Quality Iterations for Failed Shelves",
						(NoResString)"Set which release groups automatically generate quality iterations for failed shelves.",
						RegistryStorageFlags.System,
						new QualityIterationAssignmentHeader()));
			}
		}

		public TestRigRegistryItem TestRigOptions
		{
			get
			{
				return GetItem("TestRigCreationOptions",
					() => new TestRigRegistryItem("TestRigCreationOptions",
						(NoResString)ProcessedShelfsCategory,
						(NoResString)"Test Rig Creation Options",
						(NoResString)@"DAT will create test rigs for successful shelf tests using the configuration specified here. These settings can be overridden per shelf test task by inserting configuration options in the task notes.
In the 'Additional Options' field, separate option sets with semicolons (for example: TestRigIncludeSystemPackage: true; TestRigClientCode: EDI)",
						RegistryStorageFlags.System));
			}
		}

		#endregion

		#region Default FilterStrip Layouts

		public const string DefaultFilterLayoutsSubCategory = Category + "/Default Filter Layouts";

		#region Default FilterStrip Layout For EDIClassrooms Module

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutForWebEDIClassrooms
		{
			get
			{
				return GetItem("DefaultFilterLayoutCargoWiseEDIClassroms", delegate
				{
					FilterLayoutCodePairRegistryItem result = new FilterLayoutCodePairRegistryItem(
							"DefaultFilterLayoutCargoWiseEDIClassroms",
							(NoResString)DefaultFilterLayoutsSubCategory,
							(NoResString)"CargoWise Classroms",
							(NoResString)"This is the default filter layout for the CargoWise Classroms module.",
							true, true, new ComboBoxFilterLayoutRegistryEditorInfo("CargoWiseEDIClassroms"),
							RegistryStorageFlags.Company,
							RegistryOptions.Default,
							string.Empty,
							false);

					return result;
				});
			}
		}

		#endregion

		#region Default FilterStrip Layout For HRJobApplicant Module

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutForWebHRJobApplicant
		{
			get
			{
				return GetItem("DefaultFilterLayoutForWebHRJobApplicant", delegate
				{
					FilterLayoutCodePairRegistryItem result = new FilterLayoutCodePairRegistryItem(
						"DefaultFilterLayoutForWebHRJobApplicant",
						(NoResString)DefaultFilterLayoutsSubCategory,
						(NoResString)"Job Applicant",
						(NoResString)"This is the default filter layout for the Web Job Applicant module.",
						true,
						true,
						new ComboBoxFilterLayoutRegistryEditorInfo("HRJobApplicant"),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						string.Empty,
						false);

					return result;
				});
			}
		}

		#endregion

		#region Default FilterStrip Layout For GlbPerson Module

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutForWebGlbPerson
		{
			get
			{
				return GetItem("DefaultFilterLayoutForWebGlbPerson", delegate
				{
					FilterLayoutCodePairRegistryItem result = new FilterLayoutCodePairRegistryItem(
						"DefaultFilterLayoutForWebGlbPerson",
						(NoResString)DefaultFilterLayoutsSubCategory,
						(NoResString)"Person",
						(NoResString)"This is the default filter layout for the Web Person module.",
						true,
						true,
						new ComboBoxFilterLayoutRegistryEditorInfo("GlbPerson"),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						string.Empty,
						false);

					return result;
				});
			}
		}

		#endregion

		#region Default FilterStrip Layout For WebSecurity Module

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutForWebSecurity
		{
			get
			{
				return GetItem("DefaultFilterLayoutForWebSecurity", delegate
				{
					var result = new FilterLayoutCodePairRegistryItem(
						"DefaultFilterLayoutForWebSecurity",
						(NoResString)DefaultFilterLayoutsSubCategory,
						(NoResString)"Web Security",
						(NoResString)"This is the default filter layout for the Web Security module.",
						true,
						true,
						new ComboBoxFilterLayoutRegistryEditorInfo("CargoWiseEDIWebSecurityContacts"),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						string.Empty,
						false);

					return result;
				});
			}
		}

		#endregion

		#region Default FilterStrip Layout For Notification Roles Module

		public FilterLayoutCodePairRegistryItem DefaultFilterLayoutForNotificationRoles
		{
			get
			{
				return GetItem("DefaultFilterLayoutForNotificationRoles", delegate
				{
					var result = new FilterLayoutCodePairRegistryItem(
						"DefaultFilterLayoutForNotificationRoles",
						(NoResString)DefaultFilterLayoutsSubCategory,
						(NoResString)"Notification Roles",
						(NoResString)"This is the default filter layout for the Notification Roles module.",
						true,
						true,
						new ComboBoxFilterLayoutRegistryEditorInfo("CargoWiseEDINotificationRolesContacts"),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						string.Empty,
						false);

					return result;
				});
			}
		}

		#endregion

		public static CodeDescriptionPairList GetListOfModuleFilterLayouts(string moduleName)
		{
			return ObjectFactory.Get<IFilterModuleLayoutFinder>().GetListOfModuleFilterLayouts(moduleName);
		}

		#endregion

		#region DbConnectionCrikeyServer

		public StringRegistryItem DbConnectionCrikeyServer
		{
			get
			{
				return GetItem("DbConnectionCrikeyServer", delegate
				{
					return new StringRegistryItem("DbConnectionCrikeyServer",
						(NoResString)Category,
						(NoResString)"DB Server for Crikey Connection",
						null,
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory,
						"");
				});
			}
		}

		#endregion

		#region Allow Save Upgrade Package To Disk

		public BooleanRegistryItem AllowSaveUpgradePackageToDisk
		{
			get
			{
				return GetItem("AllowSaveUpgradePackageToDisk", delegate
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
						"AllowSaveUpgradePackageToDisk",
						(NoResString)ReleaseBuildsAndUpgradesSubCategory,
						(NoResString)"Allow Save Upgrade Package To Disk",
						(NoResString)"Saving Upgrade Packages to Disk is completely disabled unless this is enabled. Only Richard White, Henry Ye or specific authorized users can enable it.",
						RegistryStorageFlags.System,
						RegistryOptions.NotCached,
						false);
					return result;
				});
			}
		}

		#endregion

		#region eServices

		public const string EServicesSubCategory = Category + "/eServices";

		public StringRegistryItem DeniedPartyScreeningListsDbServerName
		{
			get
			{
				return GetItem("DeniedPartyScreeningListsDbServerName", delegate
				{
					return new StringRegistryItem(
						"DeniedPartyScreeningListsDbServerName",
						(NoResString)EServicesSubCategory,
						(NoResString)"Denied Party Screening Lists Database Server Name",
						(NoResString)"Enter the name of server which hosts denined party screening lists database.",
						RegistryStorageFlags.System,
						"syddps.db.corporate.cargowise.com");
				});
			}
		}

		public StringRegistryItem DeniedPartyScreeningListsDbName
		{
			get
			{
				return GetItem("DeniedPartyScreeningListsDbName", delegate
				{
					return new StringRegistryItem(
						"DeniedPartyScreeningListsDbName",
						(NoResString)EServicesSubCategory,
						(NoResString)"Denied Party Screening Lists Database Name",
						(NoResString)"Enter the name of database which stores denined party screening lists.",
						RegistryStorageFlags.System,
						"DeniedPartyScreeningProfiles");
				});
			}
		}

		public ServerUsernamePasswordConfigurationRegistryItem DeniedPartyScreeningListsDbLogin
		{
			get
			{
				return GetItem("DeniedPartyScreeningListsDbLogin", delegate
				{
					ServerUsernamePasswordConfiguration defaultValue = new ServerUsernamePasswordConfiguration();
					defaultValue.UserName = "eHubUser";
					defaultValue.Password = "eHubUser";
					defaultValue.ConfirmPassword = defaultValue.Password;

					return new ServerUsernamePasswordConfigurationRegistryItem(
							"DeniedPartyScreeningListsDbLogin",
							(NoResString)EServicesSubCategory,
							(NoResString)"Denied Party Screening Lists Database Login",
							(NoResString)"Enter login information to the database which stores denined party screening lists.",
							RegistryStorageFlags.System,
							defaultValue);
				});
			}
		}

		#endregion

		#region Billing

		public const string LicenceBillingCategory = Category + "/Licence Billing";

		#region Allowed Invoicing Branches

		public BooleanRegistryItem AllowedInvoicingBranches
		{
			get
			{
				return GetItem("AllowedInvoicingBranches", delegate
				{
					return new BooleanRegistryItem(
						"AllowedInvoicingBranches",
						(NoResString)LicenceBillingCategory,
						(NoResString)"Allowed Invoicing Branches",
						(NoResString)"Allow invoicing from this branch/company",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		#endregion

		#region Charge Codes

		public const string OdplBillingCategory = LicenceBillingCategory + "/ODPL";
		public const string MonthlyUsageInvoiceCategory = LicenceBillingCategory + "/Monthly Usage Invoice";

		public StringRegistryItem OdplUsageChargeCode
		{
			get
			{
				return GetItem("OdplUsageChargeCode", delegate
				{
					return new StringRegistryItem(
						"OdplUsageChargeCode",
						(NoResString)OdplBillingCategory,
						(NoResString)"Usage Charge Code",
						null,
						RegistryStorageFlags.System,
						"ODPLMTHUSE");
				});
			}
		}

		public StringRegistryItem OdplDiscountChargeCode
		{
			get
			{
				return GetItem("OdplDiscountChargeCode", delegate
				{
					return new StringRegistryItem(
						"OdplDiscountChargeCode",
						(NoResString)OdplBillingCategory,
						(NoResString)"Discount Charge Code",
						null,
						RegistryStorageFlags.System,
						"DISCODPL");
				});
			}
		}

		public StringRegistryItem OdplSurchargeChargeCode
		{
			get
			{
				return GetItem("OdplSurchargeChargeCode", delegate
				{
					return new StringRegistryItem(
						"OdplSurchargeChargeCode",
						(NoResString)OdplBillingCategory,
						(NoResString)"Surcharge Charge Code",
						null,
						RegistryStorageFlags.System,
						"SURCODPL");
				});
			}
		}

		public StringRegistryItem OdplHybridUsageChargeCode
		{
			get
			{
				return GetItem("OdplHybridUsageChargeCode", delegate
				{
					return new StringRegistryItem(
						"OdplHybridUsageChargeCode",
						(NoResString)OdplBillingCategory,
						(NoResString)"Hybrid Usage Charge Code",
						null,
						RegistryStorageFlags.System,
						"ODPLMUSE-H");
				});
			}
		}

		public StringRegistryItem OdplHybridDiscountChargeCode
		{
			get
			{
				return GetItem("OdplHybridDiscountChargeCode", delegate
				{
					return new StringRegistryItem(
						"OdplHybridDiscountChargeCode",
						(NoResString)OdplBillingCategory,
						(NoResString)"Hybrid Discount Charge Code",
						null,
						RegistryStorageFlags.System,
						"DISCODPL-H");
				});
			}
		}

		public StringRegistryItem OdplDepositChargeCode
		{
			get
			{
				return GetItem("MonthlyUsageDepositChargeCode", delegate
				{
					return new StringRegistryItem(
						"MonthlyUsageDepositChargeCode",
						(NoResString)OdplBillingCategory,
						(NoResString)"Deposit Charge Code",
						null,
						RegistryStorageFlags.System,
						"DEPOSIT");
				});
			}
		}

		public CodeDescriptionPairListRegistryItem ValidStlUsageCodesOnOdplPricelists
		{
			get
			{
				return GetItem("ValidStlUsageCodesOnOdplPricelists", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
					"ValidStlUsageCodesOnOdplPricelists",
					(NoResString)OdplBillingCategory,
					(NoResString)"Valid STL Usage Codes on ODPL Pricelists",
					null,
					ClientChargeableUsageSchema.U1_SubCode.MaxLength,
					RegistryStorageFlags.System);
				});
			}
		}

		#region DepositChargeCodes

		public const string DepositChargeCodesKeyName = "DepositChargeCodes";
		public const string SecurityDepositChargeCode = "ODPLSECDEP";

		public CodeDescriptionPairListRegistryItem DepositChargeCodes
		{
			get
			{
				return GetItem(DepositChargeCodesKeyName, delegate
				{
					var defaultList = new CodeDescriptionPairList();
					defaultList.AddPair("DEPOSIT", "");
					defaultList.AddPair("ODPL", "DEPOSIT");
					defaultList.AddPair(SecurityDepositChargeCode, "");
					defaultList.AddPair("STLDEPOSIT", "");

					var item = new CodeDescriptionPairListRegistryItem(
						DepositChargeCodesKeyName,
						(NoResString)LicenceBillingCategory,
						(NoResString)"Deposit Charge Codes",
						(NoResString)"Charge codes used to hold deposits. For old charge codes that should now be included in another, set the description to the new charge code.",
						AccChargeCodeSchema.AC_Code.MaxLength,
						RegistryStorageFlags.System,
						isLocalizable: false,
						defaultValue: defaultList);

					item.OnBuildLogReference += (args) =>
					{
						Enterprise.Client.EDI.Billing.Business.DepositBalance.UpdateWithRetries(true, 10, 3000);
						return "";
					};

					return item;
				});
			}
		}

		#endregion

		public const string ProcessingFeeCategory = LicenceBillingCategory + "/Processing";

		public StringRegistryItem MonthlyUsageProcessingFeeChargeCode
		{
			get
			{
				return GetItem("MonthlyUsageProcessingFeeChargeCode", delegate
				{
					return new StringRegistryItem(
						"MonthlyUsageProcessingFeeChargeCode",
						(NoResString)ProcessingFeeCategory,
						(NoResString)"Fee Charge Code",
						null,
						RegistryStorageFlags.System,
						"SALESFEE");
				});
			}
		}

		public StringRegistryItem CommentChargeCode
		{
			get
			{
				return GetItem("CommentChargeCode", delegate
				{
					return new StringRegistryItem(
						"CommentChargeCode",
						(NoResString)LicenceBillingCategory,
						(NoResString)"Comment Charge Code",
						null,
						RegistryStorageFlags.System,
						"COMMENT");
				});
			}
		}

		public StringRegistryItem PrepaidBalanceChargeCode
		{
			get
			{
				return GetItem("PrepaidBalanceChargeCode", delegate
				{
					return new StringRegistryItem(
						"PrepaidBalanceChargeCode",
						(NoResString)LicenceBillingCategory,
						(NoResString)"Prepaid Balance Charge Code",
						null,
						RegistryStorageFlags.System,
						"");
				});
			}
		}

		#endregion

		public StringRegistryItem InvoiceAttachmentDocType
		{
			get
			{
				return GetItem("InvoiceAttachmentDocType", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"InvoiceAttachmentDocType",
						(NoResString)LicenceBillingCategory,
						(NoResString)"Invoice Attachments Document Type",
						(NoResString)"Document Type used for general purpose invoice attachments.",
						RegistryStorageFlags.System);
					result.EditorInfo = new CodeFindBoxRegistryEditorInfo(ModuleIDs.RefDocType, GetDocTypeCollection);
					return result;
				});
			}
		}

		public CodeDescriptionBoolRegistryItem InvoicingProcessingFeeLookup
		{
			get
			{
				return GetItem(
					"InvoicingProcessingFeeLookup", delegate
					{
						var defaultValue = new CodeDescriptionBoolCollection(3);
						defaultValue.Add("NON", ResString.GetMultilingualString("EDIDataRegistry|InvoicingProcessingFee|None", "None"), false);
						defaultValue.Add("DDE", ResString.GetMultilingualString("EDIDataRegistry|InvoicingProcessingFee|DirectDebitDiscount", "Direct Debit Discount"), true);
						defaultValue.Add("MPF", ResString.GetMultilingualString("EDIDataRegistry|InvoicingProcessingFee|ManualProcessingFee", "Manual Processing Fee"), false);

						return new CodeDescriptionBoolRegistryItem(
							"InvoicingProcessingFeeLookup",
							(NoResString)ProcessingFeeCategory,
							(NoResString)"Fee Codes",
							(NoResString)"Place a tick in the \"Discount\" column if this is a discount (a negative amount taken off the invoice total), leave it unticked if this is a fee (an additional charge on their invoice)",
							RegistryStorageFlags.System,
							new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("3adcb8f5-3584-4fc1-9024-7719d96c69e3", "Discount")),
							defaultValue
						);
					}
				);
			}
		}

		IBusinessObjectCollection GetDocTypeCollection(BusinessObjectFactory factory)
		{
			return new RefDocTypeCollection(factory);
		}

		public CodePairRegistryItem PriceListDocType
		{
			get
			{
				return GetItem("PriceListDocType", delegate
				{
					var lookUpListProvider = new CodeDescriptionPairListProvider(() => GetOrgDocTypeList());

					CodePairRegistryItem result = new CodePairRegistryItem(
							"PriceListDocType",
							(NoResString)LicenceBillingCategory,
							(NoResString)"Document Type for the Sales Price List",
							(NoResString)"Adding documents of this type to an organization will automatically import the prices for ODPL billing.",
							lookUpListProvider,
							true, true, new ComboBoxRegistryEditorInfo(lookUpListProvider),
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							"PRI",
							false);

					return result;
				});
			}
		}

		CodeDescriptionPairList GetOrgDocTypeList()
		{
			string refType = Enterprise.Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			var factory = new BusinessObjectFactory();
			var query = new DocTypeCategoryQuery(factory, refType);
			query.AddToFilter(RefDocTypeSchema.RT_IsActive, ZBool.True);

			RefDocTypeCollection docTypes = new RefDocTypeCollection(factory, query);
			docTypes.ApplySort(RefDocType.Schema.RT_Desc, System.ComponentModel.ListSortDirection.Ascending);

			CodeDescriptionPairList result = new CodeDescriptionPairList();
			foreach (RefDocType docType in docTypes)
			{
				result.AddPair(docType.RT_DocType, docType.RT_DescMultilingual);
			}

			return result;
		}

		public DecimalRegistryItem MinimumAmountToBill
		{
			get
			{
				return GetItem("MinimumAmountToBill", delegate
				{
					return new DecimalRegistryItem("MinimumAmountToBill",
						(NoResString)MonthlyUsageInvoiceCategory,
						(NoResString)"Minimum amount to invoice",
						(NoResString)"Smaller amounts will be accumulated until the minimum amount is reached",
						RegistryStorageFlags.System,
						20m);
				});
			}
		}

		public CodeDescriptionPairListRegistryItem NonBilledEnterpriseCodes
		{
			get
			{
				return GetItem("NotBilledEnterpriseCodes", delegate
				{
					CodeDescriptionPairList defaultList = new CodeDescriptionPairList();
					defaultList.AddPair("EDI");
					defaultList.AddPair("HYE");
					defaultList.AddPair("EHW");
					defaultList.AddPair("WTL");

					return new CodeDescriptionPairListRegistryItem(
						"NotBilledEnterpriseCodes",
						(NoResString)LicenceBillingCategory,
						(NoResString)"Non-billed Enterprise Codes ",
						(NoResString)"Internal Enterprise Codes that are not to be billed",
						3,
						RegistryStorageFlags.System,
						false,
						defaultList);
				});
			}
		}

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public string[] NonBilledEnterpriseCodesAsStringArray
		{
			get
			{
				return NonBilledEnterpriseCodes.Value.Cast<ICodeDescription>().Select(x => x.Code).ToArray();
			}
			set
			{
				var codes = new CodeDescriptionPairList();
				foreach (var code in value)
				{
					codes.AddPair(code);
				}
				NonBilledEnterpriseCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, codes);
			}
		}

		public MultilingualStringRegistryItem MonthlyUsageInvoiceDescription
		{
			get
			{
				return GetItem("MonthlyUsageInvoiceDescription", delegate
				{
					return new MultilingualStringRegistryItem(
						"MonthlyUsageInvoiceDescription",
						(NoResString)MonthlyUsageInvoiceCategory,
						(NoResString)"Invoice Description",
						null,
						RegistryStorageFlags.System,
						ResString.GetMultilingualString("EDIDataRegistry|MonthlyUsageInvoiceDescription", "Monthly Usage Invoice"));
				});
			}
		}

		public CodeDescriptionPairListRegistryItem SalesTaxRates
		{
			get
			{
				return GetItem("SalesTaxRates", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						"SalesTaxRates",
						(NoResString)LicenceBillingCategory,
						(NoResString)"Sales Tax Rates",
						(NoResString)"The list of sales tax codes and percentage rates.",
						64,
						RegistryStorageFlags.System);
				});
			}
		}

		public const string HostingBillingCategory = LicenceBillingCategory + "/Hosting";

		public StringRegistryItem HostingDataStorageChargeCode
		{
			get
			{
				return GetItem("HostingDataStorageChargeCode", delegate
				{
					return new StringRegistryItem(
						"HostingDataStorageChargeCode",
						(NoResString)HostingBillingCategory,
						(NoResString)"High Speed Data Storage Charge Code",
						null,
						RegistryStorageFlags.System,
						"HOSTPRE1");
				});
			}
		}

		public StringRegistryItem HostingDocsStorageChargeCode
		{
			get
			{
				return GetItem("HostingDocsStorageChargeCode", delegate
				{
					return new StringRegistryItem(
						"HostingDocsStorageChargeCode",
						(NoResString)HostingBillingCategory,
						(NoResString)"Image Storage Charge Code",
						null,
						RegistryStorageFlags.System,
						"HOSTPRE2");
				});
			}
		}

		public StringRegistryItem HostingUltraFastStorageChargeCode
		{
			get
			{
				return GetItem("HostingUltraFastStorageChargeCode", delegate
				{
					return new StringRegistryItem(
						"HostingUltraFastStorageChargeCode",
						(NoResString)HostingBillingCategory,
						(NoResString)"Ultra Fast Storage Charge Code",
						null,
						RegistryStorageFlags.System,
						"HOSTPRE16");
				});
			}
		}

		public StringRegistryItem HostingNonProductionStorageChargeCode
		{
			get
			{
				return GetItem("HostingNonProductionStorageChargeCode", delegate
				{
					return new StringRegistryItem(
						"HostingNonProductionStorageChargeCode",
						(NoResString)HostingBillingCategory,
						(NoResString)"Non-Production Storage Charge Code",
						null,
						RegistryStorageFlags.System,
						"HOSTPRE12");
				});
			}
		}

		public StringRegistryItem HostingRemoteDevicesChargeCode
		{
			get
			{
				return GetItem("HostingRemoteDevicesChargeCode", delegate
				{
					return new StringRegistryItem(
						"HostingRemoteDevicesChargeCode",
						(NoResString)HostingBillingCategory,
						(NoResString)"Remote Devices Charge Code",
						null,
						RegistryStorageFlags.System,
						"HOSTPRE3");
				});
			}
		}

		public StringRegistryItem HostingPrintServersChargeCode
		{
			get
			{
				return GetItem("HostingPrintServersChargeCode", delegate
				{
					return new StringRegistryItem(
						"HostingPrintServersChargeCode",
						(NoResString)HostingBillingCategory,
						(NoResString)"Print Servers Charge Code",
						null,
						RegistryStorageFlags.System,
						"HOSTPRE4");
				});
			}
		}

		public StringRegistryItem HostingDiscountChargeCode
		{
			get
			{
				return GetItem("HostingDiscountChargeCode", delegate
				{
					return new StringRegistryItem(
						"HostingDiscountChargeCode",
						(NoResString)HostingBillingCategory,
						(NoResString)"Discount Charge Code",
						null,
						RegistryStorageFlags.System,
						"DISCHOST");
				});
			}
		}

		public IntRegistryItem HostingStorageBufferPercentage
		{
			get
			{
				return GetItem("HostingStorageBufferPercentage", delegate
				{
					return new IntRegistryItem(
						"HostingStorageBufferPercentage",
						(NoResString)HostingBillingCategory,
						(NoResString)"Storage Buffer Percentage",
						(NoResString)"Percentage of storage size to use as a buffer. Added to the storage size from 2015-5-1 with the switch to billing by used size rather than file size on disk.",
						RegistryStorageFlags.System,
						20);
				});
			}
		}

		public BillingSystemChargeCodeMappingRegistryItem BillingSystemChargeCodeMappings
		{
			get
			{
				return GetItem("BillingSystemChargeCodeMappings", delegate
				{
					return new BillingSystemChargeCodeMappingRegistryItem(
						"BillingSystemChargeCodeMappings",
						(NoResString)LicenceBillingCategory,
						(NoResString)"Billing System Charge Code Mappings",
						(NoResString)"List of charge codes used in billing systems.");
				});
			}
		}

		public CodeDescriptionBoolRegistryItem LicenceFeeTypes
		{
			get
			{
				return GetItem("LicenceFeeTypesEx", delegate
				{
					return new CodeDescriptionBoolRegistryItem(
						"LicenceFeeTypesEx",
						(NoResString)LicenceBillingCategory,
						(NoResString)"Fee Types",
						(NoResString)"The list of fee types available to use in the Fee > Type field and whether they are a 3rd party charge and so exempt from any processing charges or prepayment discount.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						new CodeDescriptionBoolRegistryEditorInfo((NoResString)"3rd Party", true, false),
						new CodeDescriptionBoolCollection {
							{ "ESV", (NoResString)"eServices", false },
							{ "PRE", (NoResString)"Product Enhancement", false },
							{ "UPG", (NoResString)"Upgrade Assurance", false },
							{ "DSO", (NoResString)"Deliverable Services Order", false },
							{ "MAI", (NoResString)"Annual Maintenance Fee", false },
							{ "LTF", (NoResString)"License Transfer Fee", false },
							{ "ESC", (NoResString)"Escrow Annual Fee", false },
							{ "S8", (NoResString)"S8 Cargo Annual Fee", false },
							{ "1ST", (NoResString)"1-Stop ComTrac Annual Renewal Fee", true },
							{ "FFT", (NoResString)"Fixed Fee for Test License", false },
						});
				});
			}
		}

		public CodeDescriptionPairListRegistryItem FeeBillingDiscountChargeCodes
		{
			get
			{
				return GetItem("FeeBillingDiscountChargeCodes", delegate
				{
					var def = new CodeDescriptionPairList();
					def.AddPair("DISCEHUB", "DISCEHUB");
					def.AddPair("DISCSTL", "DISCSTL");
					def.AddPair("DISCWSC", "DISCWSC");
					def.AddPair("EHUB", "DISCEHUB");
					def.AddPair("WISECLOUD", "DISCWSC");

					return new CodeDescriptionPairListRegistryItem(
						"FeeBillingDiscountChargeCodes",
						(NoResString)LicenceBillingCategory,
						(NoResString)"Fee Discount Charge Codes",
						(NoResString)"The list of fee charge codes and the corresponding discount charge code, if a discount is allowed.",
						AutoAccChargeCode.Schema.AC_CodeMaxLength,
						RegistryStorageFlags.System,
						false,
						def);
				});
			}
		}

		public CodeDescriptionPairListRegistryItem ProcessingFeeExemptBillingSystems
		{
			get
			{
				return GetItem("ProcessingFeeExemptBillingSystems",
				delegate
				{
					CodeDescriptionPairList defaultList = new CodeDescriptionPairList();
					defaultList.AddPair("RSH");
					defaultList.AddPair("FAX");

					return new CodeDescriptionPairListRegistryItem(
						"ProcessingFeeExemptBillingSystems",
						(NoResString)ProcessingFeeCategory,
						(NoResString)"Discount Exempt",
						(NoResString)"The billing systems that are exempt from processing discounts and fees.",
						3,
						RegistryStorageFlags.System,
						false,
						defaultList);
				});
			}
		}

		public CodeDescriptionPairListRegistryItem LicenceUsageBilledPerTransaction
		{
			get
			{
				return GetItem("LicenceUsageBilledPerTransaction",
				delegate
				{
					CodeDescriptionPairList defaultList = new CodeDescriptionPairList();
					defaultList.AddPair("ACP", "Native CA ACI (Global - CA)"); // Env.Licence.ACIReportingPerTransaction
					defaultList.AddPair("AMS", "Native US AMS (Global - US)"); // Env.Licence.AMSReporting

					return new CodeDescriptionPairListRegistryItem(
						"LicenceUsageBilledPerTransaction",
						(NoResString)LicenceBillingCategory,
						(NoResString)"Licence Usage Billed Per Transaction",
						(NoResString)"The list of licence modules that are billed from CPT usage in the licence usage report.",
						3,
						RegistryStorageFlags.System,
						false,
						defaultList);
				});
			}
		}

		public CodeDescriptionPairListRegistryItem TransactionChargeCodes
		{
			get
			{
				CodeDescriptionPairList defaultList = new CodeDescriptionPairList();
				defaultList.AddPair("ACP", "ACIREPORT");
				defaultList.AddPair("AMS", "AMSREPORT");
				defaultList.AddPair(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.eBACCA, "EBACCA");
				defaultList.AddPair(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.ImporterSecurityFiling, "IFSTRANS");
				defaultList.AddPair(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.Fax, "PROGFAXES");
				defaultList.AddPair(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.DeniedPartyScreening, "DENPS");
				defaultList.AddPair(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.ExDocs, "EXDOCS");
				defaultList.AddPair(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.DistanceCalculatorGeneric, "DISTANCAL");
				defaultList.AddPair(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.DistanceCalculatorPcMiler, "DISTANCAL");
				defaultList.AddPair(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.S8Cargo, "RESELLS8");
				defaultList.AddPair(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.JapanAFR, "PDSMFJAFR");
				defaultList.AddPair(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.USCustoms, "USCSBUREAU");
				defaultList.AddPair(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.RailincByMessage, "RAILINC");
				defaultList.AddPair(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.OceanTracing, "OCECONTRA");

				return GetItem("TransactionChargeCodes",
					delegate
					{
						return new CodeDescriptionPairListRegistryItem(
							"TransactionChargeCodes",
							(NoResString)OdplBillingCategory,
							(NoResString)"Transaction Amount Charge Codes",
							(NoResString)"The list of billing system codes/price codes and their amount charge codes (in the description column).",
							3,
							RegistryStorageFlags.System,
							false,
							defaultList);
					});
			}
		}

		public CodeDescriptionPairListRegistryItem TransactionDiscountChargeCodes
		{
			get
			{
				CodeDescriptionPairList defaultList = new CodeDescriptionPairList();
				defaultList.AddPair("ACP", "DISCTRANS");
				defaultList.AddPair("AMS", "DISCTRANS");
				defaultList.AddPair(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.eBACCA, "EBACCA");
				defaultList.AddPair(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.ImporterSecurityFiling, "DISCTRANS");
				defaultList.AddPair(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.Fax, "PROGFAXES");
				defaultList.AddPair(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.DeniedPartyScreening, "DENPS");
				defaultList.AddPair(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.ExDocs, "DISCTRANS");
				defaultList.AddPair(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.DistanceCalculatorGeneric, "DISCTRANS");
				defaultList.AddPair(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.DistanceCalculatorPcMiler, "DISCTRANS");
				defaultList.AddPair(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.S8Cargo, "DISCTRANS");
				defaultList.AddPair(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.JapanAFR, "DISCJAFR");
				defaultList.AddPair(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.USCustoms, "DISCUSC");
				defaultList.AddPair(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.OceanTracing, "DISCTRANS");

				return GetItem("TransactionDiscountChargeCodes",
					delegate
					{
						return new CodeDescriptionPairListRegistryItem(
							"TransactionDiscountChargeCodes",
							(NoResString)OdplBillingCategory,
							(NoResString)"Transaction Discount Charge Codes",
							(NoResString)"The list of billing system codes and their discount charge codes (in the description column).",
							3,
							RegistryStorageFlags.System,
							false,
							defaultList);
					});
			}
		}

		public MultilingualStringRegistryItem MonthlyUsageInvoiceComment
		{
			get
			{
				return GetItem(
					"MonthlyUsageInvoiceComment",
					delegate
					{
						var registryItem = new MultilingualStringRegistryItem(
							"MonthlyUsageInvoiceComment",
							(NoResString)MonthlyUsageInvoiceCategory,
							(NoResString)"Comment",
							(NoResString)"Comment to put on the Monthly Usage invoice. If there is an Invoice Comment defined on the organization then that comment will be used instead of this registry.",
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							null);
						return registryItem;
					});
			}
		}

		public MultilingualStringRegistryItem MonthlyUsageReportBreakdownComment
		{
			get
			{
				return GetItem(
					"MonthlyUsageReportBreakdownComment",
					delegate
					{
						var registryItem = new MultilingualStringRegistryItem(
							"MonthlyUsageReportBreakdownComment",
							(NoResString)MonthlyUsageInvoiceCategory,
							(NoResString)"Report Breakdown Comment",
							(NoResString)"The comment added to the end of the monthly usage invoices.",
							RegistryStorageFlags.System,
							ResString.GetMultilingualString("EDIDataRegistry|MonthlyUsageReportBreakdownComment", "For more detailed breakdown of usage, visit myaccount.cargowise.com > Usage Reports."));
						return registryItem;
					});
			}
		}

		public CodeDescriptionPairListRegistryItem Tier4CountryCodes
		{
			get
			{
				return GetItem("Tier4CountryCodes", delegate
				{
					string[] defaultArray = new string[]
					{
"AL", "Albania",
"AS", "American Samoa",
"AD", "Andorra",
"AO", "Angola",
"AI", "Anguilla",
"AQ", "Antarctica",
"AG", "Antigua and Barbuda",
"AM", "Armenia",
"AW", "Aruba",
"AZ", "Azerbaijan",
"BS", "Bahamas",
"BD", "Bangladesh",
"BB", "Barbados",
"BY", "Belarus",
"BZ", "Belize",
"BJ", "Benin",
"BM", "Bermuda",
"BT", "Bhutan",
"BO", "Bolivia",
"BQ", "Bonaire",
"BA", "Bosnia and Herzegovina",
"BW", "Botswana",
"IO", "British Indian Ocean Territory",
"KH", "Cambodia",
"CM", "Cameroon",
"CV", "Cape Verde",
"KY", "Cayman Islands",
"CX", "Christmas Island",
"CC", "Cocos (Keeling) Islands",
"KM", "Comoros",
"CG", "Congo",
"CK", "Cook Islands",
"CR", "Costa Rica",
"HR", "Croatia",
"CU", "Cuba",
"CW", "Curacao",
"DJ", "Djibouti",
"DM", "Dominica",
"DO", "Dominican Republic",
"EC", "Ecuador",
"SV", "El Salvador",
"GQ", "Equatorial Guinea",
"FK", "Falkland Islands (Malvinas)",
"FO", "Faroe Islands",
"FJ", "Fiji",
"PF", "French Polynesia",
"TF", "French Southern Territories (Antarctica)",
"GF", "French Guyana",
"GA", "Gabon",
"GM", "Gambia",
"GE", "Georgia",
"GH", "Ghana",
"GI", "Gibraltar",
"GL", "Greenland",
"GD", "Grenada",
"GP", "Guadeloupe",
"GU", "Guam",
"GG", "Guernsey",
"GW", "Guinea-Bissau",
"GY", "Guyana",
"HM", "Heard Island and McDonald Islands (Antarctica)",
"VA", "Holy See (Vatican City State)",
"IS", "Iceland",
"XZ", "International Waters Installations",
"IM", "Isle of Man",
"IL", "Israel",
"JM", "Jamaica",
"JE", "Jersey",
"JO", "Jordan",
"KZ", "Kazakhstan",
"KE", "Kenya",
"KI", "Kiribati",
"LA", "Lao People's Democratic Republic",
"LS", "Lesotho",
"MK", "Macedonia",
"MW", "Malawi",
"MV", "Maldives",
"ML", "Mali",
"MH", "Marshall Islands",
"MQ", "Martinique",
"MU", "Mauritius",
"YT", "Mayotte",
"FM", "Micronesia, Federated States of",
"MD", "Moldova, Republic of",
"MC", "Monaco",
"MN", "Mongolia",
"ME", "Montenegro",
"MS", "Montserrat",
"MA", "Morocco",
"MZ", "Mozambique",
"MM", "Myanmar",
"NA", "Namibia",
"NR", "Nauru",
"NP", "Nepal",
"NC", "New Caledonia",
"NI", "Nicaragua",
"NU", "Niue",
"NF", "Norfolk Island",
"MP", "Northern Mariana Islands",
"OM", "Oman",
"PW", "Palau",
"PA", "Panama",
"PG", "PapuaNewGuinea",
"PY", "Paraguay",
"PN", "Pitcairn",
"RE", "Reunion",
"SH", "Saint Helena",
"KN", "Saint Kitts and Nevis",
"LC", "Saint Lucia",
"PM", "Saint Pierre and Miquelon",
"VC", "Saint Vincent and the Grenadines",
"WS", "Samoa",
"SM", "San Marino",
"ST", "Sao Tome and Principe",
"SN", "Senegal",
"RS", "Serbia",
"SL", "Sierra Leone",
"SX", "Sint Maarten",
"SB", "Solomon Islands",
"GS", "South Georgia and the South Sandwic",
"SR", "Suriname",
"SJ", "Svalbard and Jan Mayen",
"SZ", "Swaziland",
"TJ", "Tajikistan",
"TZ", "Tanzania (United Republic of)",
"TL", "Timor-Leste",
"TG", "Togo",
"TK", "Tokelau",
"TO", "Tonga",
"TT", "Trinidad and Tobago",
"TN", "Tunisia",
"TM", "Turkmenistan",
"TC", "Turks and Caicos Islands",
"TV", "Tuvalu",
"UG", "Uganda",
"UM", "United States Minor Outlying Island",
"UY", "Uruguay",
"UZ", "Uzbekistan",
"VU", "Vanuatu",
"VG", "Virgin Islands, British",
"VI", "Virgin Islands, U.S.",
"WF", "Wallis and Futuna",
"ZM", "Zambia",
"PR", "Puerto Rico",
"SC", "Seychelles"
					};

					var defaults = new CodeDescriptionPairList();
					for (int i = 0; i < defaultArray.Length; i += 2)
					{
						defaults.AddPair(defaultArray[i], defaultArray[i + 1]);
					}

					return new CodeDescriptionPairListRegistryItem(
							"Tier4CountryCodes",
							(NoResString)LicenceBillingCategory,
							(NoResString)"Tier 4 Country/Region Codes",
							null,
							3,
							RegistryStorageFlags.System,
							false,
							new CodeDescriptionPairList());
				});
			}
		}

		public CodeDescriptionPairListRegistryItem LicenceCountryLanguages
		{
			get
			{
				return GetItem("LicenceCountryLanguages", delegate
				{
					string[] defaultArray = new string[]
					{
"CN", Core.SharedConstants.Languages.ChineseSimplified + "," + Core.SharedConstants.Languages.ChineseTraditional, // Both Chinese
"MO", Core.SharedConstants.Languages.ChineseSimplified + "," + Core.SharedConstants.Languages.ChineseTraditional, // Both Chinese
"HK", Core.SharedConstants.Languages.ChineseSimplified + "," + Core.SharedConstants.Languages.ChineseTraditional, // Both Chinese
"TW", Core.SharedConstants.Languages.ChineseTraditional, // TraditionalChinese
"BJ", Core.SharedConstants.Languages.French, // French
"BF", Core.SharedConstants.Languages.French, // French
"BI", Core.SharedConstants.Languages.French, // French
"CF", Core.SharedConstants.Languages.French, // French
"TD", Core.SharedConstants.Languages.French, // French
"CG", Core.SharedConstants.Languages.French, // French
"CD", Core.SharedConstants.Languages.French, // French
"CI", Core.SharedConstants.Languages.French, // French
"DJ", Core.SharedConstants.Languages.French, // French
"FR", Core.SharedConstants.Languages.French, // French
"PF", Core.SharedConstants.Languages.French, // French
"TF", Core.SharedConstants.Languages.French, // French
"GF", Core.SharedConstants.Languages.French, // French
"GA", Core.SharedConstants.Languages.French, // French
"CA", Core.SharedConstants.Languages.French, // English / French
"GP", Core.SharedConstants.Languages.French, // French
"GN", Core.SharedConstants.Languages.French, // French
"HT", Core.SharedConstants.Languages.French, // French
"MG", Core.SharedConstants.Languages.French, // French
"ML", Core.SharedConstants.Languages.French, // French
"YT", Core.SharedConstants.Languages.French, // French
"MC", Core.SharedConstants.Languages.French, // French
"MQ", Core.SharedConstants.Languages.French, // French
"NC", Core.SharedConstants.Languages.French, // French
"NE", Core.SharedConstants.Languages.French, // French
"RE", Core.SharedConstants.Languages.French, // French
"RW", Core.SharedConstants.Languages.French, // French
"PM", Core.SharedConstants.Languages.French, // French
"SN", Core.SharedConstants.Languages.French, // French
"TG", Core.SharedConstants.Languages.French, // French
"TN", Core.SharedConstants.Languages.French, // French
"WF", Core.SharedConstants.Languages.French, // French
"AT", Core.SharedConstants.Languages.German, // German
"DE", Core.SharedConstants.Languages.German, // German
"LI", Core.SharedConstants.Languages.German, // German
"LU", Core.SharedConstants.Languages.German, // German
"CH", Core.SharedConstants.Languages.German, // German
"AR", Core.SharedConstants.Languages.Spanish, // Spanish
"BZ", Core.SharedConstants.Languages.Spanish, // Spanish
"BO", Core.SharedConstants.Languages.Spanish, // Spanish
"CL", Core.SharedConstants.Languages.Spanish, // Spanish
"CO", Core.SharedConstants.Languages.Spanish, // Spanish
"CR", Core.SharedConstants.Languages.Spanish, // Spanish
"CU", Core.SharedConstants.Languages.Spanish, // Spanish
"DO", Core.SharedConstants.Languages.Spanish, // Spanish
"EC", Core.SharedConstants.Languages.Spanish, // Spanish
"SV", Core.SharedConstants.Languages.Spanish, // Spanish
"GQ", Core.SharedConstants.Languages.Spanish, // Spanish
"GT", Core.SharedConstants.Languages.Spanish, // Spanish
"HN", Core.SharedConstants.Languages.Spanish, // Spanish
"MX", Core.SharedConstants.Languages.Spanish, // Spanish
"NI", Core.SharedConstants.Languages.Spanish, // Spanish
"PA", Core.SharedConstants.Languages.Spanish, // Spanish
"PY", Core.SharedConstants.Languages.Spanish, // Spanish
"PE", Core.SharedConstants.Languages.Spanish, // Spanish
"ES", Core.SharedConstants.Languages.Spanish, // Spanish
"UY", Core.SharedConstants.Languages.Spanish, // Spanish
"VE", Core.SharedConstants.Languages.Spanish, // Spanish
"PR", Core.SharedConstants.Languages.Spanish, // Spanish
"VA", Core.SharedConstants.Languages.Italian, // Italian
"IT", Core.SharedConstants.Languages.Italian, // Italian
"SM", Core.SharedConstants.Languages.Italian, // Italian
"JP", Core.SharedConstants.Languages.Japanese, // Japanese
"AO", Core.SharedConstants.Languages.Portuguese, // Portuguese
"BR", Core.SharedConstants.Languages.Portuguese, // Portuguese
"CV", Core.SharedConstants.Languages.Portuguese, // Portuguese
"GW", Core.SharedConstants.Languages.Portuguese, // Portuguese
"MZ", Core.SharedConstants.Languages.Portuguese, // Portuguese
"PT", Core.SharedConstants.Languages.Portuguese, // Portuguese
"ST", Core.SharedConstants.Languages.Portuguese, // Portuguese
"TL", Core.SharedConstants.Languages.Portuguese, // Portuguese
"UA", Core.SharedConstants.Languages.Ukrainian, // Ukrainian
"BY", Core.SharedConstants.Languages.Russian, // Russian
"RU", Core.SharedConstants.Languages.Russian, // Russian
"AW", Core.SharedConstants.Languages.Dutch, // Dutch
"BE", Core.SharedConstants.Languages.Dutch, // Dutch
"BQ", Core.SharedConstants.Languages.Dutch, // Dutch / English
"CW", Core.SharedConstants.Languages.Dutch, // Dutch / English
"NL", Core.SharedConstants.Languages.Dutch, // Dutch
"SX", Core.SharedConstants.Languages.Dutch, // Dutch / English
"SR", Core.SharedConstants.Languages.Dutch, // Dutch
"BG", Core.SharedConstants.Languages.Bulgarian, // Bulgarian
"KP", Core.SharedConstants.Languages.Korean, // Korean
"KR", Core.SharedConstants.Languages.Korean, // Korean
"PL", Core.SharedConstants.Languages.Polish, // Polish
"SE", Core.SharedConstants.Languages.Swedish, // Swedish
					};

					var defaults = new CodeDescriptionPairList();
					for (int i = 0; i < defaultArray.Length; i += 2)
					{
						defaults.AddPair(defaultArray[i], defaultArray[i + 1]);
					}

					return new CodeDescriptionPairListRegistryItem(
							"LicenceCountryLanguages",
							(NoResString)LicenceBillingCategory,
							(NoResString)"Country/Region Language",
							(NoResString)"Country/Region and a comma separated list of the two letter ISO language codes for local languages. Usage of a local language is free for single country/region entities.",
							3,
							RegistryStorageFlags.System,
							false,
							defaults);
				});
			}
		}

		public CodeDescriptionBoolRegistryItem BillingCountryGroups
		{
			get
			{
				return GetItem("BillingCountryGroup", delegate
				{
					var defaults = new CodeDescriptionBoolCollection();
					defaults.Add("CN", (NoResString)"CN"); // China
					defaults.Add("MO", (NoResString)"CN"); // Macau
					defaults.Add("HK", (NoResString)"CN"); // Hong Kong
														   // The default values of dbo.EdiGetBillingCountryGroups() & Registry.BillingCountryGroups.Default must be synchronized logically.

					var editorInfo = new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Registered User Country/Region Grouping", true, false);

					var hint =
@"Countries/Regions that are grouped for receiving developing country / region discount and for registered user count. Code column is the country/region code and Description column is the country/region code of the main country/region in the group. The Registered User Country/Region Grouping column indicates the groupings are for calculating users count per country/region.

The CHU (Chargeable Usage) Service Task will need to run after updates to this registry.";

					return new CodeDescriptionBoolRegistryItem(
							"BillingCountryGroup",
							(NoResString)LicenceBillingCategory,
							(NoResString)"Country/Region Group",
							(NoResString)hint,
							RegistryStorageFlags.System,
							editorInfo,
							defaults);
				});
			}
		}

		#region SuppressResourceStringsCheckRegion

		public StringRegistryItem StandardPriceCompanyLicenceIdentifier
		{
			get
			{
				return GetItem("StandardPriceCompanyLicenceIdentifier", delegate
				{
					return new StringRegistryItem(
						"StandardPriceCompanyLicenceIdentifier",
						(NoResString)LicenceBillingCategory,
						(NoResString)"Standard Price Company",
						(NoResString)"Enter the licence code of the company containing all the standard pricelists. Do not change this setting without first ensuring all the standard pricelists exist on the new licence since any organization with a missing standard price list cannot be saved.",
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory,
						"EDIEDISYD");
				});
			}
		}

		#endregion

		#region Airline Messaging

		public const string AirlineMessagingCategory = LicenceBillingCategory + "/Airline Messaging";

		public StringRegistryItem AirlineMessagingFWBChargeCode
		{
			get
			{
				return GetItem("AirlineMessagingFWBChargeCode", delegate
				{
					return new StringRegistryItem(
						"AirlineMessagingFWBChargeCode",
						(NoResString)AirlineMessagingCategory,
						(NoResString)"FWB Transaction Charge Code",
						null,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"FWB");
				});
			}
		}

		public StringRegistryItem AirlineMessagingFHLChargeCode
		{
			get
			{
				return GetItem("AirlineMessagingFHLChargeCode", delegate
				{
					return new StringRegistryItem(
						"AirlineMessagingFHLChargeCode",
						(NoResString)AirlineMessagingCategory,
						(NoResString)"FHL Transaction Charge Code",
						null,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"FHL");
				});
			}
		}

		public StringRegistryItem AirlineMessagingFSUChargeCode
		{
			get
			{
				return GetItem("AirlineMessagingFSUChargeCode", delegate
				{
					return new StringRegistryItem(
						"AirlineMessagingFSUChargeCode",
						(NoResString)AirlineMessagingCategory,
						(NoResString)"FSU Transaction Charge Code",
						null,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"FSU");
				});
			}
		}

		public StringRegistryItem AirlineMessagingDiscountAndRemitChargeCode
		{
			get
			{
				return GetItem("AirlineMessagingDiscountAndRemitChargeCode", delegate
				{
					return new StringRegistryItem(
						"AirlineMessagingDiscountAndRemitChargeCode",
						(NoResString)AirlineMessagingCategory,
						(NoResString)"Discount and Remittable Transaction Charge Code",
						null,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"DISCFWBFHL");
				});
			}
		}

		public StringRegistryItem AirlineMessagingTraxonLicenceIdentifier
		{
			get
			{
				return GetItem("AirlineMessagingTraxonLicenceIdentifier", delegate
				{
					return new StringRegistryItem(
						"AirlineMessagingTraxonLicenceIdentifier",
						(NoResString)AirlineMessagingCategory,
						(NoResString)"Traxon Licence Identifier",
						null,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"TXNFFO---");
				});
			}
		}

		#endregion

		#region NZ Customs

		public const string NZCustomsBillingCategory = LicenceBillingCategory + "/NZ Customs";

		public StringRegistryItem NZCustomsJobChargeCode
		{
			get
			{
				return GetItem("NZCustomsJobChargeCode", delegate
				{
					return new StringRegistryItem(
						"NZCustomsJobChargeCode",
						(NoResString)NZCustomsBillingCategory,
						(NoResString)"NZ Customs - Job Charge Code",
						null,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"NZCUSJOB");
				});
			}
		}

		public StringRegistryItem NZCustomsMessageChargeCode
		{
			get
			{
				return GetItem("NZCustomsMessageChargeCode", delegate
				{
					return new StringRegistryItem(
						"NZCustomsMessageChargeCode",
						(NoResString)NZCustomsBillingCategory,
						(NoResString)"NZ Customs - Message Charge Code",
						null,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"NZCUSMSG");
				});
			}
		}

		#endregion

		#region ABM Customs

		public const string ABMCustomsBillingCategory = LicenceBillingCategory + "/ABM Customs";

		public StringRegistryItem ABMCustomsWareMessagingChargeCode
		{
			get
			{
				return GetItem("ABMCustomsWareMessagingChargeCode", delegate
				{
					return new StringRegistryItem(
						"ABMCustomsWareMessagingChargeCode",
						(NoResString)ABMCustomsBillingCategory,
						(NoResString)"ABM CustomsWare Messaging Charge Code",
						null,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"ABMCUSMES");
				});
			}
		}

		public StringRegistryItem ABMMovementMessagingChargeCode
		{
			get
			{
				return GetItem("ABMMovementMessagingChargeCode", delegate
				{
					return new StringRegistryItem(
						"ABMMovementMessagingChargeCode",
						(NoResString)ABMCustomsBillingCategory,
						(NoResString)"ABM Movement Messaging Charge Code",
						null,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"ABMMOVMES");
				});
			}
		}

		public StringRegistryItem ABMFiscalRepInvoiceMessagingChargeCode
		{
			get
			{
				return GetItem("ABMFiscalRepInvoiceMessagingChargeCode", delegate
				{
					return new StringRegistryItem(
						"ABMFiscalRepInvoiceMessagingChargeCode",
						(NoResString)ABMCustomsBillingCategory,
						(NoResString)"ABM Fiscal Rep Invoice Messaging Charge Code",
						null,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"ABMFISREP");
				});
			}
		}

		public StringRegistryItem ABMCustomsWareMessagingDiscountChargeCode
		{
			get
			{
				return GetItem("ABMCustomsWareMessagingDiscountChargeCode", delegate
				{
					return new StringRegistryItem(
						"ABMCustomsWareMessagingDiscountChargeCode",
						(NoResString)ABMCustomsBillingCategory,
						(NoResString)"ABM CustomsWare Messaging Discount Charge Code",
						null,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"DISCABMCUS");
				});
			}
		}

		public StringRegistryItem ABMMovementMessagingDiscountChargeCode
		{
			get
			{
				return GetItem("ABMMovementMessagingDiscountChargeCode", delegate
				{
					return new StringRegistryItem(
						"ABMMovementMessagingDiscountChargeCode",
						(NoResString)ABMCustomsBillingCategory,
						(NoResString)"ABM Movement Messaging Discount Charge Code",
						null,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"DISCABMMOV");
				});
			}
		}

		public StringRegistryItem ABMFiscalRepInvoiceMessagingDiscountChargeCode
		{
			get
			{
				return GetItem("ABMFiscalRepInvoiceMessagingDiscountChargeCode", delegate
				{
					return new StringRegistryItem(
						"ABMFiscalRepInvoiceMessagingDiscountChargeCode",
						(NoResString)ABMCustomsBillingCategory,
						(NoResString)"ABM Fiscal Rep Invoice Messaging Discount Charge Code",
						null,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"");
				});
			}
		}

		#endregion

		#region Client Mapping

		public CodeDescriptionPairListRegistryItem ClientMappingBillingNames
		{
			get
			{
				return GetItem("ClientMappingBillingNames", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
							"ClientMappingBillingNames",
							(NoResString)"eHub Interfaces",
							(NoResString)"Billing names of eHub interfaces (customer specific mappings) with an optional comment. Defines the valid names that can be used as a price list and discount sub code. Names are automatically added if found in the usage records.",
							ClientLicencePriceItemSchema.L7_Ref4.MaxLength,
							new CodeDescriptionPairListEditorInfo(true, true, CodeDescriptionPairListEditorInfo.CharacterCasing.Normal, CodeDescriptionPairListEditorInfo.CharacterCasing.Normal,
								(NoResString)"Name", (NoResString)"Comment"),
							RegistryStorageFlags.System,
							false,
							RegistryOptions.Default,
							new CodeDescriptionPairList(),
							true,
							(NoResString)LicenceBillingCategory);
				});
			}
		}

		#endregion

		#region eAdaptor

		public CodeDescriptionPairListRegistryItem EAdaptorElementNames
		{
			get
			{
				return GetItem("EAdaptorElementNames", delegate
				{
					var defaultList = new CodeDescriptionPairList();
					defaultList.AddPair("1", "Acc Transaction (AR/AP Invoice, Journals)");
					defaultList.AddPair("2", "Outturn Line (Underbond Outturn)");
					defaultList.AddPair("3", "Cargo Report (Air/Sea) - Per House");
					defaultList.AddPair("4", "CTO Export / Import");
					defaultList.AddPair("5", "Formal Customs Declarations (Import)");
					defaultList.AddPair("6", "Formal Customs Declarations (Export)");
					defaultList.AddPair("7", "Commercial Invoice Lines (Import)");
					defaultList.AddPair("8", "Commercial Invoice Lines (Export)");
					defaultList.AddPair("9", "Order (Order Manager)");
					defaultList.AddPair("A", "Order Lines");
					defaultList.AddPair("B", "CFS/CTO Load List / Shipments");
					defaultList.AddPair("C", "Shipping Ocean Bill of Lading");
					defaultList.AddPair("D", "Spot Quote / Quick Bookings / Quoted Bookings");
					defaultList.AddPair("E", "Shipments (Domestic/Import/Export)");
					defaultList.AddPair("F", "Domestic Transport Job / Booking");
					defaultList.AddPair("G", "Events (Universal Events)");
					defaultList.AddPair("H", "Warehouse Inward (Receipts)");
					defaultList.AddPair("I", "Warehouse Outward (Orders)");
					defaultList.AddPair("J", "Warehouse Lines In");
					defaultList.AddPair("K", "Warehouse Lines Out");
					defaultList.AddPair("L", "Master/Reference File Messages");

					return new CodeDescriptionPairListRegistryItem(
							"EAdaptorElementNames",
							(NoResString)"eAdaptor Element Names",
							(NoResString)"Last character of price code and corresponding billing names of eAdaptor elements. Price codes begin with 'IC' for inserts and 'IU' for updates.",
							1,
							new CodeDescriptionPairListEditorInfo(true, true, CodeDescriptionPairListEditorInfo.CharacterCasing.Upper, CodeDescriptionPairListEditorInfo.CharacterCasing.Normal,
								(NoResString)"Insert, Update", (NoResString)"Message Element Name"),
							RegistryStorageFlags.System,
							false,
							RegistryOptions.Default,
							defaultList,
							false,
							(NoResString)LicenceBillingCategory);
				});
			}
		}

		#endregion

		#region Version Surcharge

		public const string BillingVersionSurchargeCategory = LicenceBillingCategory + "/Non-Current Version Surcharge";

		public DecimalRegistryItem VersionSurchargePercent
		{
			get
			{
				return GetItem("VersionSurchargePercent", () =>
				{
					return new DecimalRegistryItem(
						"VersionSurchargePercent",
						(NoResString)BillingVersionSurchargeCategory,
						(NoResString)"Percentage",
						(NoResString)"Default percentage of the Customer’s monthly fees and charges for utilising a Non-Current Version.",
						new NumericRegistryEditorInfo(decimalPlaces: 2),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: 2m,
						lowerBound: 0,
						upperBound: 999);
				});
			}
		}

		public DecimalRegistryItem VersionSurchargeAdditionalPercent
		{
			get
			{
				return GetItem("VersionSurchargeAdditionalPercent", () =>
				{
					return new DecimalRegistryItem(
						"VersionSurchargeAdditionalPercent",
						(NoResString)BillingVersionSurchargeCategory,
						(NoResString)"Percentage Additional per Non-Current Version",
						(NoResString)"Default surcharge percentage of the Customer’s monthly fees and charges for every version released after the Non-Current Version.",
						new NumericRegistryEditorInfo(decimalPlaces: 2),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: 1m,
						lowerBound: 0,
						upperBound: 99);
				});
			}
		}

		public StringRegistryItem VersionSurchargeChargeCode
		{
			get
			{
				return GetItem("VersionSurchargeChargeCode", () =>
				{
					return new StringRegistryItem(
						"VersionSurchargeChargeCode",
						(NoResString)BillingVersionSurchargeCategory,
						(NoResString)"Charge Code",
						(NoResString)"Charge Code to use on the invoice for the Non-Current Version Surcharge line.",
						RegistryStorageFlags.System,
						"GPRSURSTL");
				});
			}
		}

		public MultilingualStringRegistryItem VersionSurchargeDescription
		{
			get
			{
				return GetItem("VersionSurchargeDescription",
					delegate
					{
						var registryItem = new MultilingualStringRegistryItem(
							"VersionSurchargeDescription",
							(NoResString)BillingVersionSurchargeCategory,
							(NoResString)"Invoice Description",
							(NoResString)("Description of the Non-Current Version Surcharge to show on the invoice."),
							RegistryStorageFlags.System,
							ResString.GetMultilingualString("EDIDataRegistry|VersionSurchargeDescription", "Surcharge for Non-Current Version"));
						return registryItem;
					});
			}
		}

		#endregion

		#region STL

		public const string StlBillingCategory = LicenceBillingCategory + "/STL";

		public CodeDescriptionBoolRegistryItem StlDiscountTypes
		{
			get
			{
				return GetItem("StlDiscountTypes", delegate
				{
					var defaultValue = new CodeDescriptionBoolCollection(30); // Billing.Business.EdiPriceHeaderDiscount.Schema.PHD_NameMaxLength
					defaultValue.Add("3RDPARTY", ResString.GetMultilingualString("StlDiscountTypes|3RDPARTY", "3rd Party Transaction Charge"), true);
					defaultValue.Add("CCLPATTAIN", ResString.GetMultilingualString("StlDiscountTypes|CCLPATTAIN", "CCLP Attain Certification Discount"), true);
					defaultValue.Add("CCLPMEET", ResString.GetMultilingualString("StlDiscountTypes|CCLPMEET", "CCLP Meet Requirement Discount"), true);
					defaultValue.Add("CCLPRETAIN", ResString.GetMultilingualString("StlDiscountTypes|CCLPRETAIN", "CCLP Retain Certification Discount"), true);
					defaultValue.Add("COMMIT", ResString.GetMultilingualString("StlDiscountTypes|COMMIT", "Commitment"), true);
					defaultValue.Add("CTR", ResString.GetMultilingualString("StlDiscountTypes|CTR", "Container Tracking"), true);
					defaultValue.Add("DEVCOUNTRY", ResString.GetMultilingualString("StlDiscountTypes|DEVCOUNTRY", "Developing Country/Region"), true);
					defaultValue.Add("DEVPARTNER", ResString.GetMultilingualString("StlDiscountTypes|DEVPARTNER", "Dev Partner"), true);
					defaultValue.Add("DOMESTIC", ResString.GetMultilingualString("StlDiscountTypes|DOMESTIC", "Domestic Entity"), true);
					defaultValue.Add("GROUP", ResString.GetMultilingualString("StlDiscountTypes|GROUP", "Group Buying"), true);
					defaultValue.Add("HTFN", ResString.GetMultilingualString("StlDiscountTypes|HTFN", "HTFN"), true);
					defaultValue.Add("PREPAY", ResString.GetMultilingualString("StlDiscountTypes|PREPAY", "Prepayment"), true);
					defaultValue.Add("SPECIAL", ResString.GetMultilingualString("StlDiscountTypes|SPECIAL", "Special Condition Discount"), true);
					defaultValue.Add("WISESPECIAL", ResString.GetMultilingualString("StlDiscountTypes|WISESPECIAL", "Special Condition WC Discount"), true);
					defaultValue.Add("VOLUME", ResString.GetMultilingualString("StlDiscountTypes|VOLUME", "Standard Volume"), true);
					defaultValue.Add("WIM250", ResString.GetMultilingualString("StlDiscountTypes|WIM250", "WIM250"), true);
					defaultValue.Add("WISECLOUD", ResString.GetMultilingualString("StlDiscountTypes|WISECLOUD", "WiseCloud"), true);
					defaultValue.Add("WISEPARTNER", ResString.GetMultilingualString("StlDiscountTypes|WISEPARTNER", "Wise Industry Partner Discount"), true);
					defaultValue.Add("ABMCUST", ResString.GetMultilingualString("StlDiscountTypes|ABMCUST", "ABM Customs"), true);
					defaultValue.Add("ABMPORTC", ResString.GetMultilingualString("StlDiscountTypes|ABMPORTC", "ABM Port Community"), true);
					defaultValue.Add("ABMFISREP", ResString.GetMultilingualString("StlDiscountTypes|ABMFISREP", "ABM Fiscal Rep Invoice "), true);
					defaultValue.Add("PPFORCE1", ResString.GetMultilingualString("StlDiscountTypes|PPFORCE1", "Prepayment "), true);
					defaultValue.Add("PPFORCE2", ResString.GetMultilingualString("StlDiscountTypes|PPFORCE2", "Prepayment "), true);
					defaultValue.Add("LDAASPREPAY", ResString.GetMultilingualString("StlDiscountTypes|LDAASPREPAY", "LDaaS Prepayment"), true);
					defaultValue.Add("E2EWIP", ResString.GetMultilingualString("StlDiscountTypes|E2EWIP", "E2E WIP Discount"), true);
					defaultValue.Add("E2ENONWIP", ResString.GetMultilingualString("StlDiscountTypes|E2ENONWIP", "E2E Non WIP Discount"), true);
					defaultValue.Add("BWSTUDENT", ResString.GetMultilingualString("StlDiscountTypes|BWSTUDENT", "BorderWise Student Discount"), true);
					defaultValue.Add("BWSPECIAL", ResString.GetMultilingualString("StlDiscountTypes|BWSPECIAL", "BorderWise Special Condition Discount"), true);
					defaultValue.Add("BWLEGACY", ResString.GetMultilingualString("StlDiscountTypes|BWLEGACY", "BorderWise Legacy Discount"), true);

					var editorInfo = new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Active", true, false);

					return new CodeDescriptionBoolRegistryItem(
						"StlDiscountTypes",
						(NoResString)StlBillingCategory,
						(NoResString)"STL Discount Types",
						null,
						RegistryStorageFlags.System,
						editorInfo,
						defaultValue);
				});
			}
		}

		public CodeDescriptionPairListRegistryItem StlFreeTrialDiscounts
		{
			get
			{
				return GetItem("StlFreeTrialDiscounts", delegate
				{
					var result = new CodeDescriptionPairListRegistryItem(
						"StlFreeTrialDiscounts",
						(NoResString)StlBillingCategory,
						(NoResString)"STL Free Trial Discounts",
						(NoResString)"STL Discount Types that automatically trigger a one month free trial, and the usage Category and Price Codes that trigger the trial. Category and Price Code should be comma separated, for example CSU,SNU. If a free trial is triggered by more than one kind of usage, enter a row for each pair of usage codes with the same Discount Type.",
						30, // Billing.Business.EdiPriceHeaderDiscount.Schema.PHD_NameMaxLength;
						RegistryStorageFlags.System,
						new ReadOnlyCodeDescriptionPairList());

					result.EditorInfo = new CodeDescriptionPairListEditorInfo(true, true,
						CodeDescriptionPairListEditorInfo.CharacterCasing.Upper, CodeDescriptionPairListEditorInfo.CharacterCasing.Upper,
						(NoResString)"Discount Type", (NoResString)"Category, Price Code");

					return result;
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1043:WordSpellingRule")]
		public StlRawUsageReportRefCaptionRegistryItem StlRawUsageReportRefCaption
		{
			get
			{
				return GetItem("StlRawUsageReportRefCaption", delegate
				{
					var defaults = new StlRawUsageReportRefCaptionCollection();
					defaults.AddNew("USR", "CoreActiveStaff", "Staff Code", "Staff Name");
					defaults.AddNew("AI3", "CoeFinance3rdPartyInterface", "Cash Batch Ref", "");
					defaults.AddNew("LDG", "CoreFinanceGeneralLedger", "Transaction Num", "Description");
					defaults.AddNew("SLG", "CoreFinanceJoblessSubledger", "Transaction Num", "Description");
					defaults.AddNew("ADP", "ExtensionFinanceAdvPayment", "Transaction Num", "Description");
					defaults.AddNew("COO", "CommunicationExportCertificateOfOrigin", "Declaration Ref", "");
					defaults.AddNew("EFC", "CommunicationExportExportFormalClearance", "Declaration Ref", "");
					defaults.AddNew("RFP", "CommunicationExportRequestForPermit", "Declaration Ref", "");
					defaults.AddNew("IFB", "CommunicationImportClearanceExtBroker", "Declaration Ref", "");
					defaults.AddNew("IFG", "CommunicationImportClearanceGeneral", "Declaration Ref", "");
					defaults.AddNew("FTZ", "CommunicationImportFtzAdmission", "Declaration Ref", "");
					defaults.AddNew("MSC", "CommunicationImportMiscellaneous", "Declaration Ref", "");
					defaults.AddNew("TNP", "CommunicationImportTranshipment", "Declaration Ref", "");
					defaults.AddNew("DRW", "CommunicationSpecialEntryDrawbacks", "Declaration Ref", "");
					defaults.AddNew("INB", "CommunicationSpecialEntryInbond", "Job Ref", "");
					defaults.AddNew("LCH", "CommunicationSpecialEntryLandedCosting", "Landed Cost Type", "Declaration Ref / Order Num");
					defaults.AddNew("UNB", "CommunicationSpecialEntryUnderbond", "Sender Ref", "");
					defaults.AddNew("CB2", "ExtensionsCanadaB2", "Declaration Ref", "");
					defaults.AddNew("LVS", "ExtensionsCanadaLowValueShipment", "Declaration Ref", "");
					defaults.AddNew("ACA", "ExtensionsGreatBritainImportAirAgent", "MAWB", "HAWB");
					defaults.AddNew("ACS", "ExtensionsGreatBritainImportAirShed", "MAWB", "HAWB");
					defaults.AddNew("K45", "ExtensionsMalasiaK4K5Manifest", "Entry Num", "Consign Ref");
					defaults.AddNew("USP", "ExtensionsUsaProtest", "Declaration Ref", "");
					defaults.AddNew("URC", "ExtensionsUsaReconcilliation", "Declaration Ref", "");
					defaults.AddNew("INT", "OceanBookingStatusMessaging", "Consign Ref", "Message Num");
					defaults.AddNew("CTE", "OtherPartiesForwarderAirCargoCtoExport", "BGM Ref", "Manifest Type");
					defaults.AddNew("CTI", "OtherPartiesForwarderAirCargoCtoImport", "MAWB", "HAWB");
					defaults.AddNew("CTT", "OtherPartiesForwarderAirCargoCtoOutturn", "Container Number / Master Bill / House Bill", "Outturn Result Type");
					defaults.AddNew("CTW", "OtherPartiesForwarderAirCargoCtoOutward", "Entry Num", "");
					defaults.AddNew("ECE", "OtherPartiesForwarderAirCargoEciExport", "Declaration Ref", "");
					defaults.AddNew("ECM", "OtherPartiesForwarderAirCargoEciImport", "Declaration Ref", "");
					defaults.AddNew("ANZ", "OtherPartiesForwarderAirCargoNz", "MAWB", "HAWB");
					defaults.AddNew("ACX", "OtherPartiesForwarderAirCargoReportExport", "Entry Num", "Consign Ref");
					defaults.AddNew("ACM", "OtherPartiesForwarderAirCargoReportImport", "MAWB", "HAWB");
					defaults.AddNew("AHK", "OtherPartiesForwarderAirCargoTraxon", "Consign Ref", "Message Num");
					defaults.AddNew("SCE", "OtherPartiesForwarderSeaCargoExport", "Entry Num", "Consign Ref");
					defaults.AddNew("SCI", "OtherPartiesForwarderSeaCargoImport", "Ocean Bill", "House Bill");
					defaults.AddNew("SRE", "OtherPartiesShippingReportExport", "BGM Ref", "Manifest Type");
					defaults.AddNew("SRI", "OtherPartiesShippingReportImport", "Sender Ref", "");
					defaults.AddNew("EAD", "EAdaptorInbound", "Session Guid", "");
					defaults.AddNew("EAO", "EAdaptorOutbound", "Session Guid", "");
					defaults.AddNew("BOL", "AgencyBillOfLading", "Job Num", "Consign Ref");
					defaults.AddNew("BKG", "AgencyBooking", "Job Num", "Consign Ref");
					defaults.AddNew("ACO", "AgencyContainerCount", "Consign Ref", "Container Num");
					defaults.AddNew("CDT", "AgencyContainerDetention", "Job Num", "");
					defaults.AddNew("CDM", "AgencyContainerManager", "Job Num", "Container Num");
					defaults.AddNew("OCK", "AgencyContainerOceanTracking", "Message Number", "Movement Type");
					defaults.AddNew("CAN", "AgencyContainerTeu", "Consign Ref", "Container Num");
					defaults.AddNew("BKA", "BookingConsolidationPrintedAdvice", "Job ID", "");
					defaults.AddNew("BKU", "BookingConsolidationUserCreated", "Job ID", "");
					defaults.AddNew("CFN", "CfsContainerCount", "Consign Ref", "");
					defaults.AddNew("CFC", "CfsContainerPacking", "Consign Ref", "");
					defaults.AddNew("CFT", "CfsContainerTeu", "Consign Ref", "Container Num");
					defaults.AddNew("CFP", "CfsShipmentManagement", "Job Num", "Container Num");
					defaults.AddNew("FGB", "ForwarderGeneralBooking", "Job Num", "");
					defaults.AddNew("GFC", "ForwarderGeneralContainerCount", "Consign Ref", "Container Num");
					defaults.AddNew("GFT", "ForwarderGeneralContainerTeu", "Consign Ref", "Container Num");
					defaults.AddNew("GTW", "ForwarderGeneralGatewayConsolidation", "Job Num", "Consign Ref");
					defaults.AddNew("HKC", "ForwarderGeneralHongKongConsols", "Consign Ref", "Entry Num");
					defaults.AddNew("JPC", "ForwarderGeneralJapaneseConsols", "Consign Ref", "Entry Num");
					defaults.AddNew("ORM", "ForwarderGeneralOrderManager", "Order Num", "");
					defaults.AddNew("SHP", "ForwarderGeneralShipment", "Job Num", "");
					defaults.AddNew("LTL", "LandTransportContainerLegs", "Consignment ID", "");
					defaults.AddNew("LTC", "LandTransportCubicMetresMoved", "Consignment ID", "");
					defaults.AddNew("LTK", "LandTransportKilogramsMoved", "Consignment ID", "");
					defaults.AddNew("LTM", "LandTransportManagement", "Consignment ID", "");
					defaults.AddNew("PTC", "PortTransportContainerCount", "Consignment ID", "Container Num");
					defaults.AddNew("PTT", "PortTransportContainerTeu", "Consignment ID", "Container Num");
					defaults.AddNew("WAD", "Warehouse3plAdjustments", "Warehouse Name", "Docket ID");
					defaults.AddNew("WBM", "Warehouse3plBillOfMaterials", "Warehouse Name", "Docket ID");
					defaults.AddNew("WIN", "Warehouse3plInwards", "Warehouse Name", "Docket ID");
					defaults.AddNew("W3T", "Warehouse3plTransfersAndReplenishments", "Warehouse Name", "Docket ID");
					defaults.AddNew("WVO", "Warehouse3plVasOrder", "Warehouse Name", "Job ID");
					defaults.AddNew("WLO", "Warehouse3plLoads", "Warehouse Name", "Job ID");
					defaults.AddNew("WCL", "Warehouse3plCycleCountLocation", "Warehouse Name", "Job ID");
					defaults.AddNew("W4P", "Warehouse4plLongTermBond", "Warehouse Name", "Docket ID");
					defaults.AddNew("SPK", "WarehouseScanPacking", "Job ID", "");
					defaults.AddNew("WTD", "WarehouseTransitPackagesDispatched", "Package ID", "Consignment ID");
					defaults.AddNew("WTP", "WarehouseTransitPackagesReceived", "Package ID", "Consignment ID");
					defaults.AddNew("WTH", "WarehouseTransitReceiptIntoDepot", "Consignment ID", "Warehouse Code");
					defaults.AddNew("CME", "SalesCampaignEmail", "Campaign Name", "");
					defaults.AddNew("CMR", "SalesCampaignRegistration", "Campaign Name", "");
					defaults.AddNew("OPM", "SalesOpportunityManager", "Opportunity ID", "");
					defaults.AddNew("WKI", "WorkItem", "Work Item Number", "");
					defaults.AddNew("WKP", "Project", "Project Number", "");
					defaults.AddNew("WRS", "RatesService", "Rates Service Usage", "");
					defaults.AddNew("YUU", "YardUnitsUnloaded", "UnitID", "ReceiveTransportationUnit", "CurrentYard");

					return new StlRawUsageReportRefCaptionRegistryItem(
							"StlRawUsageReportRefCaption",
							(NoResString)StlBillingCategory,
							(NoResString)"STL Usage Report Reference Captions",
							(NoResString)"",
							RegistryStorageFlags.System,
							defaults);
				});
			}
		}

		public CodeDescriptionPairListRegistryItem StlPriceListExchangeRateGroups
		{
			get
			{
				return GetItem("StlPriceListExchangeRateGroups", delegate
				{
					var defaults = new CodeDescriptionPairList();
					defaults.AddPair("STL", "STL");
					defaults.AddPair("ESV", "e-Services Transactional");
					defaults.AddPair("LDAAS", "LDAAS");

					return new CodeDescriptionPairListRegistryItem(
							"StlPriceListExchangeRateGroups",
							(NoResString)StlBillingCategory,
							(NoResString)"STL Price List Exchange Rate Groups",
							(NoResString)"STL Price List Exchange Rate Groups",
							5,
							RegistryStorageFlags.System,
							false,
							defaults);
				});
			}
		}

		public StringRegistryItem StlSurchargeChargeCode
		{
			get
			{
				return GetItem("StlSurchargeChargeCode", delegate
				{
					return new StringRegistryItem(
						"StlSurchargeChargeCode",
						(NoResString)StlBillingCategory,
						(NoResString)"STL Surcharge Charge Code",
						null,
						RegistryStorageFlags.System,
						"EXTCRSTL");
				});
			}
		}

		public StringArrayRegistryItem StlInvoiceSupportedLanguages
		{
			get
			{
				return GetItem("StlInvoiceSupportedLanguages", delegate
				{
					return new StringArrayRegistryItem(
					"StlInvoiceSupportedLanguages",
					(NoResString)StlBillingCategory,
					(NoResString)"STL Invoice Supported Languages",
					null,
					RegistryStorageFlags.System, Array.Empty<string>());
				});
			}
		}

		public CodeDescriptionPairListRegistryItem BillingStlGlobalPriceLists
		{
			get
			{
				return GetItem("BillingStlGlobalPriceLists", delegate
				{
					var result = new CodeDescriptionPairListRegistryItem(
						"BillingStlGlobalPriceLists",
						(NoResString)StlBillingCategory,
						(NoResString)"Global STL Price Lists",
						(NoResString)"Code and Description of global pricelists used in STL billing.",
						3,
						RegistryStorageFlags.System);

					return result;
				});
			}
		}

		public BillingDbUsageCodesRegistryItem BillingDbUsageCodesList
		{
			get
			{
				return GetItem("BillingDbUsageCodes", delegate
				{
					var result = new BillingDbUsageCodesRegistryItem(
						"BillingDbUsageCodes",
						(NoResString)StlBillingCategory,
						(NoResString)"Billing Db Usage Codes",
						(NoResString)"Category, Price Code and corresponding Price List System Code for Billing DB usage collected for STL billing that is not under the STL category. Key Ref # is optional and defines the reference field # to further group usage. Use 1 to 5 if there is a key field, or zero if none.");

					return result;
				});
			}
		}

		public StringArrayRegistryItem ProductsRequiringEnterpriseCode
		{
			get
			{
				return GetItem("ProductsRequiringEnterpriseCode", delegate
				{
					return new StringArrayRegistryItem(
					"ProductsRequiringEnterpriseCode",
					(NoResString)LicenceBillingCategory,
					(NoResString)"Products Requiring Enterprise Code",
					null,
					RegistryStorageFlags.System, new[] { "ENT", "CW1", "CWN", "SPH" });
				});
			}
		}

		public CodeDescriptionPairListRegistryItem HandheldDevicePremiumTypes
		{
			get
			{
				return GetItem("HandheldDevicePremiumTypes", delegate
				{
					var defaults = new CodeDescriptionPairList();
					defaults.AddPair("C2S", "");
					defaults.AddPair("C2W", "");
					defaults.AddPair("P3C", "");
					defaults.AddPair("P3S", "");
					defaults.AddPair("PLC", "");
					defaults.AddPair("PLS", "");
					defaults.AddPair("PS1", "");
					defaults.AddPair("PS2", "");
					defaults.AddPair("PSC", "");
					defaults.AddPair("PSS", "");
					defaults.AddPair("S2S", "");
					defaults.AddPair("S2W", "");
					defaults.AddPair("S3S", "");
					defaults.AddPair("S3W", "");
					defaults.AddPair("SCC", "");
					defaults.AddPair("SCS", "");
					defaults.AddPair("TWC", "");
					defaults.AddPair("TWD", "");
					defaults.AddPair("VCS", "");
					defaults.AddPair("VCW", "");
					defaults.AddPair("VPC", "");
					defaults.AddPair("VPS", "");

					var result = new CodeDescriptionPairListRegistryItem(
							"HandheldDevicePremiumTypes",
							(NoResString)StlBillingCategory,
							(NoResString)"Handheld Device Premium Types",
							(NoResString)"Handheld Device Premium Types",
							3,
							RegistryStorageFlags.System,
							false,
							defaults);

					result.EditorInfo = new CodeDescriptionPairListEditorInfo(
						true, false,
						CodeDescriptionPairListEditorInfo.CharacterCasing.Upper,
						CodeDescriptionPairListEditorInfo.CharacterCasing.Normal,
						(NoResString)"Premium Type");

					return result;
				});
			}
		}

		public CodeDescriptionPairListRegistryItem UnregisteredDevicePremiumTypes
		{
			get
			{
				return GetItem("UnregisteredDevicePremiumTypes", delegate
				{
					var defaults = new CodeDescriptionPairList();
					defaults.AddPair("BYR", "Un-registered customer supplied device -  REDUCE COUNT BY");

					var result = new CodeDescriptionPairListRegistryItem(
							"UnregisteredDevicePremiumTypes",
							(NoResString)StlBillingCategory,
							(NoResString)"Un-registered Device Premium Types",
							(NoResString)"Un-registered Device Premium Types",
							3,
							RegistryStorageFlags.System,
							false,
							defaults);

					result.EditorInfo = new CodeDescriptionPairListEditorInfo(
						true, true,
						CodeDescriptionPairListEditorInfo.CharacterCasing.Upper,
						CodeDescriptionPairListEditorInfo.CharacterCasing.Normal,
						(NoResString)"Premium Type", (NoResString)"Description");

					return result;
				});
			}
		}

		public BooleanRegistryItem EnableTaxProcessorForBilling
		{
			get
			{
				return GetItem("EnableTaxProcessorForBilling", () =>
				{
					return new BooleanRegistryItem(
						"EnableTaxProcessorForBilling",
						(NoResString)StlBillingCategory,
						(NoResString)"Enable Tax Processor For Billing",
						(NoResString)"Enable Tax Processor For Billing",
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public DiscountSuspensionPolicyRegistryItem StlDiscountSuspensionPolicyDefault
				=> GetItem("StlDiscountSuspensionPolicyDefault", () =>
				{
					var defaultValue = new DiscountSuspensionPolicyCollection();
					defaultValue.AddNew("CW1", "NVR");
					return new DiscountSuspensionPolicyRegistryItem(
						"StlDiscountSuspensionPolicyDefault",
						(NoResString)StlBillingCategory,
						(NoResString)"Discount Suspension Policy Default",
						(NoResString)"Sets the default discount policy that will apply to all paying organizations when billing for products in STL",
						RegistryStorageFlags.System,
						defaultValue);
				});

		#region Country Tier Code Mappings

		public CountryTierPriceCodeMappingRegistryItem CountryTierPriceCodeMappings
		{
			get
			{
				return GetItem("CountryTierPriceCodeMappings",
					delegate
					{
						return new CountryTierPriceCodeMappingRegistryItem("CountryTierPriceCodeMappings",
							(NoResString)StlBillingCategory,
							(NoResString)"Country Tier Price Code Mapping",
							(NoResString)"This registry allows a user to assign a Country Tier Code against a Price Code for STL usage that requires a Country Tiered Pricing Structure.",
							new CountryTierPriceCodeMappingRegistryEditorInfo(),
							RegistryStorageFlags.System);
					});
			}
		}

		#endregion

		public CodeDescriptionBoolRegistryItem StlMonthlyUsageInvoiceDescription
		{
			get
			{
				return GetItem("StlMonthlyUsageInvoiceDescription", delegate
				{
					var defaultValue = new CodeDescriptionBoolCollection(3);
					defaultValue.Add("SMF", ResString.GetMultilingualString("StlMonthlyUsageInvoiceDescription|SMF", "SmartFreight Monthly Usage Invoice - {Date:MMMM} {Date:yyyy}"), true);
					var editorInfo = new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Active", false, false);

					return new CodeDescriptionBoolRegistryItem(
						"StlMonthlyUsageInvoiceDescription",
						(NoResString)StlBillingCategory,
						(NoResString)"STL Monthly Usage Invoice Description",
						(NoResString)"The Invoice Description of non-CargoWise products (i.e. WTA, SMF, etc) generated from STL Billing can be controlled in this registry.",
						RegistryStorageFlags.System,
						editorInfo,
						defaultValue);
				});
			}
		}

		public BranchCollectionRegistryItem CargoWiseNextBillingARInvoiceOnlyBranches
			=> GetItem("CargoWiseNextBillingARInvoiceOnlyBranches", () =>
					new BranchCollectionRegistryItem
					(
						"CargoWiseNextBillingARInvoiceOnlyBranches",
						(NoResString)StlBillingCategory,
						(NoResString)"CargoWise Next Invoice Split Configuration",
						(NoResString)"Add issuing branch codes in this registry for countries where a Disbursement Rebate invoice is not valid. By adding the issuing branch here, STL Billing will only produce two invoices for Disbursement License Fee Billing. The Disbursement Rebate invoice will be added as a discount line in the STL Monthly Usage invoice.",
						RegistryStorageFlags.System
					));

		#endregion

		#region Prepayment Comment

		public MultilingualStringRegistryItem PrepaymentInvoiceComment
		{
			get
			{
				return GetItem(
					"PrepaymentInvoiceComment",
					delegate
					{
						var registryItem = new MultilingualStringRegistryItem(
							"PrepaymentInvoiceComment",
							(NoResString)LicenceBillingCategory,
							(NoResString)"Prepayment Comment",
							(NoResString)("Prepayment comment to put on the usage invoice."),
							RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							ResString.GetMultilingualString("EDIDataRegistry|PrepaymentInvoiceComment", "If you provide a Simplified Payment Option to ensure you have enough credit funds on your account, this will be processed on the 21st of each month."));
						return registryItem;
					});
			}
		}

		public DecimalRegistryItem PrepaymentMargin
		{
			get
			{
				return GetItem("PrepaymentMargin", delegate
				{
					return new DecimalRegistryItem("PrepaymentMargin",
						(NoResString)LicenceBillingCategory,
						(NoResString)"Prepayment Margin",
						(NoResString)"The prepaid amount must exceed the usage amount after subtracting this margin amount. Allows customers to get the prepayment discount if they are just a few cents short.",
						RegistryStorageFlags.System,
						1m);
				});
			}
		}

		#endregion

		#region Price List Rounding Params

		public CodeDescriptionPairListRegistryItem BillingPriceRoundingParams
		{
			get
			{
				return GetItem("BillingPriceRoundingParams", delegate
				{
					var defaults = new CodeDescriptionPairList();
					defaults.AddPair("V1", "5=0.01,50=0.1,0=5");
					defaults.AddPair("V2", "5=0.01,50=0.1,100=0.5,0=1");

					var result = new CodeDescriptionPairListRegistryItem(
							"BillingPriceRoundingParams",
							(NoResString)LicenceBillingCategory,
							(NoResString)"Price Rounding",
							(NoResString)"Price Rounding Parameters for a particular price list. Code identifies the particular set of parameters. Parameters are a comma separated list of {price break}={round-to-nearest amount} pairs",
							3,
							RegistryStorageFlags.System,
							false,
							defaults)
					{ DataType = new BillingPriceRoundingParamsDataType(3) };

					result.EditorInfo = new CodeDescriptionPairListEditorInfo(
						true, true,
						CodeDescriptionPairListEditorInfo.CharacterCasing.Upper,
						CodeDescriptionPairListEditorInfo.CharacterCasing.Normal,
						ResString.GetMultilingualString("C5A54D54-36FA-4CA8-97A2-8DDB9BD7DD0E", "Code"),
						ResString.GetMultilingualString("EF13D613-5559-4802-A66B-45CFDD7B801A", "Parameters"));

					return result;
				});
			}
		}

		public class BillingPriceRoundingParamsDataType : CodeDescriptionPairListRegistryDataType
		{
			public BillingPriceRoundingParamsDataType(int codeMaxLength)
				: base(codeMaxLength, false)
			{
				AllowDuplicateCodes = false;
				AllowEmptyDescriptions = false;
				AllowEmptyCodes = false;
			}

			protected override void ValidateCore(IRegistryItem registryItem, ReadOnlyCodeDescriptionPairList proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
				for (int i = 0; i < proposedValue.Count; i++)
				{
					try
					{
						Billing.Business.PriceRounding.DecodePriceRoundingParams(proposedValue[i].Description);
					}
					catch (ArgumentException ex)
					{
						throw new RegistryValidationException(ex.Message);
					}
				}
			}
		}

		#endregion

		#region BorderWise module groups

		public CodeDescriptionPairListRegistryItem BorderWisePurchasedGroups
		{
			get
			{
				return GetItem("BorderWisePurchasedGroups", delegate
				{
					var defaults = new CodeDescriptionPairList();
					defaults.AddPair("GALL", "BWS, BWX, USS, USP, USW, USX, AUS, AUP, AUW, AUX, NZP, NZS, NZW, NZX");
					defaults.AddPair("GSGL", "USS, USP, USW, USX, AUS, AUP, AUW, AUX, NZP, NZS, NZW, NZX");
					defaults.AddPair("GPRO", "BW3, US3, AU3, NZ3");
					defaults.AddPair("GSG3", "US3, AU3, NZ3");

					return new CodeDescriptionPairListRegistryItem(
							"BorderWisePurchasedGroups",
							(NoResString)LicenceBillingCategory,
							(NoResString)"BorderWise Purchased Groups",
							(NoResString)"Groups of BorderWise price codes for the Purchased Licences setting, in order of highest priority to lowest for using a licence.",
							4,
							RegistryStorageFlags.System,
							false,
							defaults);
				});
			}
		}

		#endregion BorderWise module groups

		[SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "This is meant to specify an exact product name.")]
		public CodeDescriptionPairListRegistryItem BillingUsageCategoryCodes
		{
			get
			{
				return GetItem("BillingUsageCategoryCodes", delegate
				{
					var defaultValue = new CodeDescriptionPairList();
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.STL, "Seat Transaction Licence CW1");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.ODM, "On Demand Named User CW1");
					defaultValue.AddPair(Licensing.LicenceTypes.Codes.CPT, "Charge Per Transaction - Licence Checkpoint CW1");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.Fax, "FAX");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.eBACCA, "eBACCa (NZ Customs)");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.ExDocs, "ExDocs (AU Quarantine)");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.DeniedPartyScreening, "Denied Party Screening");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.DistanceCalculatorGeneric, "Distance Calculator - Generic");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.DistanceCalculatorPcMiler, "Distance Calculator - PC Miler");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.HostingStorage, "WiseCloud Storage");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.HostingRemoteDevices, "WiseCloud Remote Devices");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.AirlineMessaging, "Airline Messaging");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.NZCustoms, "NZ Customs");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.JapanAFR, "Japan Customs (AFR)");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.ClientMapping, "eHub Interfaces / Client Mappings");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.eAdaptor, "eAdaptor");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.E2E, "E2E Messaging");
					defaultValue.AddPair("AMS", "AMS Automated Manifest System US Customs");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.USCustoms, "US Customs");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.OceanTracing, "Ocean Tracing");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.RailincByMessage, "Railinc");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.GlobalContainerTracking, "Container Automation");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.ForwardAir, "Forward Air");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.ShippingPortMessaging, "Shipping Port Messaging");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.GBCustoms, "GB Customs");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.ImporterSecurityFiling, "Importer Security Filing (US Customs)");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.ASYCUDA, "ASYCUDA - Automated System for Customs Data");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.OceanCarrierMessaging, "Ocean Carrier Messaging");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.Service, "Service (Premium/LDaaS/other)");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.ZACustoms, "ZA Customs");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.BorderWise, "BorderWise");
					defaultValue.AddPair("ACC", "Accounting");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.FlightStats, "Air Waybill Automation");
					defaultValue.AddPair("CSC", "CargoSphere Contracts");
					defaultValue.AddPair("CSU", "CargoSphere Users");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.PortMessaging, "Port Messaging");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.HostingDataAccess, "WiseCloud Read-only Access Excess");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.ABMCustoms, "ABM Customs");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.S8Cargo, "Online Airline Schedules (S8 Cargo)");
					defaultValue.AddPair("3GT", "3Gtms");
					defaultValue.AddPair("SAT", "SaaS Transportation");
					defaultValue.AddPair("CW1", "CargoWise One");
					defaultValue.AddPair(Billing.Business.BillingConstants.BillingSystem.DocumentSigning, "Document Signing");
					defaultValue.AddPair(Billing.Business.BillingConstants.PriceHeaderType.CargoWiseNext, "CargoWise Next");
					defaultValue.Sort();

					return new CodeDescriptionPairListRegistryItem(
							"BillingUsageCategoryCodes",
							(NoResString)"Usage Category Codes",
							(NoResString)"Category codes for billing records. The Category and Price Code uniquely identiy the usage. For example, category 'STL' and price code 'SHP' is an STL shipment.",
							3,
							RegistryStorageFlags.System,
							false,
							defaultValue,
							(NoResString)LicenceBillingCategory);
				});
			}
		}

		public BillingUnitCountAdjustmentRegistryItem BillingUnitCountAdjustments
		{
			get
			{
				return GetItem("BillingUnitCountAdjustments",
					delegate
					{
						var defaultValue = new BillingUnitCountAdjustmentCollection();
						var wtuAdjustment = defaultValue.AddNew();
						wtuAdjustment.PriceCode = "WTU";
						wtuAdjustment.DefaultAdjustedIncrement = 0.245;
						var settings = wtuAdjustment.AdjustmentSettings;
						settings.AddNew(1, 1.00);
						settings.AddNew(2, 1.50);
						settings.AddNew(3, 1.83);
						settings.AddNew(4, 2.08);
						settings.AddNew(5, 2.28);
						settings.AddNew(6, 2.45);
						settings.AddNew(7, 2.62);
						settings.AddNew(8, 2.79);
						settings.AddNew(9, 2.96);
						settings.AddNew(10, 3.13);
						settings.AddNew(11, 3.30);
						settings.AddNew(12, 3.47);
						settings.AddNew(13, 3.64);
						settings.AddNew(14, 3.81);
						settings.AddNew(15, 3.98);
						settings.AddNew(16, 4.15);
						settings.AddNew(17, 4.32);
						settings.AddNew(18, 4.49);
						settings.AddNew(19, 4.66);
						settings.AddNew(20, 4.91);
						settings.AddNew(21, 5.15);
						settings.AddNew(22, 5.40);
						settings.AddNew(23, 5.64);
						settings.AddNew(24, 5.89);
						settings.AddNew(25, 6.13);
						settings.AddNew(26, 6.38);
						settings.AddNew(27, 6.62);
						settings.AddNew(28, 6.87);
						settings.AddNew(29, 7.11);
						settings.AddNew(30, 7.36);
						settings.AddNew(31, 7.60);
						settings.AddNew(32, 7.85);
						settings.AddNew(33, 8.09);
						settings.AddNew(34, 8.34);
						settings.AddNew(35, 8.58);
						settings.AddNew(36, 8.83);
						settings.AddNew(37, 9.07);
						settings.AddNew(38, 9.32);
						settings.AddNew(39, 9.57);
						settings.AddNew(40, 9.81);
						settings.AddNew(41, 10.06);
						settings.AddNew(42, 10.30);
						settings.AddNew(43, 10.55);
						settings.AddNew(44, 10.79);
						settings.AddNew(45, 11.04);
						settings.AddNew(46, 11.28);
						settings.AddNew(47, 11.53);
						settings.AddNew(48, 11.77);
						settings.AddNew(49, 12.02);
						settings.AddNew(50, 12.26);
						return new BillingUnitCountAdjustmentRegistryItem("BillingUnitCountAdjustments",
							(NoResString)LicenceBillingCategory,
							(NoResString)"Billing Adjusted Usage Counts",
							(NoResString)"Setup the adjusted usage count for specific usage codes.\r\n\r\nNote, the Subsequent Adjusted Usage Increment value will be applied when a transactions usage count exceeds the maximum setting in this registry item.\r\n\r\nFor example, the default configuration for the WTU usage code covers usage counts from 1 to 50. When a transaction incurs additional usage from 51 onwards, each subsequent usage increment will be charged an additional 0.245 of adjusted usage.\r\n\r\nAfter a change has been applied to a Usage Code's setup, transactions received prior to the change will need to have their Adjusted Usage Counts re-calculated. Please create a CR9 request, Module = BIL.",
							new BillingUnitCountAdjustmentRegistryEditorInfo(),
							RegistryStorageFlags.System,
							defaultValue);
					});
			}
		}

		[SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "The names are for internal display purposes only.")]
		public CodeDescriptionBoolRegistryItem ProductDisplayCategories
		{
			get
			{
				return GetItem("ProductDisplayCategories", delegate
				{
					var defaultValue = new CodeDescriptionBoolCollection(3);
					defaultValue.Add("CLD", (NoResString)"CargoWise Cloud Pricing", true);
					defaultValue.Add("STL", (NoResString)"CargoWise One STL Full Pricing", true);
					defaultValue.Add("ESV", (NoResString)"eServices Pricing", true);
					defaultValue.Add("BWP", (NoResString)"BorderWise", true);
					defaultValue.Add("ABM", (NoResString)"ABM Customs", true);
					defaultValue.Add("ACC", (NoResString)"Accounting and 3rd Party Services and Transactions (External Services)", true);
					defaultValue.Add("DEV", (NoResString)"Device Pricing", true);

					var editorInfo = new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Active", true, false);

					return new CodeDescriptionBoolRegistryItem(
						"ProductDisplayCategories",
						(NoResString)LicenceBillingCategory,
						(NoResString)"Product Display Categories",
						null,
						RegistryStorageFlags.System,
						editorInfo,
						defaultValue);
				});
			}
		}

		public UsageBillingSettingsRegistryItem UsageBillingSettings
		{
			get
			{
				return GetItem("UsageBillingSettings",
					delegate
					{
						return new UsageBillingSettingsRegistryItem("UsageBillingSettings",
							(NoResString)LicenceBillingCategory,
							(NoResString)"Products (non-CW1) Enabled for Usage Billing",
							(NoResString)"The non CargoWiseOne products enabled for generic usage billing via STL billing engine.\r\nThis enables invoice delivery instructions, independent pricelist and usage calculation in STL billing.",
							RegistryStorageFlags.System);
					});
			}
		}

		public ConsolidatedBillingSettingsRegistryItem ConsolidatedBillingSettings
		{
			get
			{
				return GetItem("ConsolidatedBillingSettings",
					delegate
					{
						return new ConsolidatedBillingSettingsRegistryItem(
						"ConsolidatedBillingSettings",
						(NoResString)LicenceBillingCategory,
						(NoResString)"Products (non-CW1) Enabled for Consolidated Billing",
						(NoResString)$"This will deliver the 'Products (non-CW1) Enabled for Consolidated Billing' registry.\r\nThis will allow multiplie databases to be consolidated into a single billing summary.",
						Integration.RegistryStorageFlags.System);
					});
			}
		}

		public CodeDescriptionPairListRegistryItem ProductsWithThreeDecimalBillingSummary
		{
			get
			{
				return GetItem("ProductsWithThreeDecimalBillingSummary",
					delegate
					{
						return new CodeDescriptionPairListRegistryItem(
								"ProductsWithThreeDecimalBillingSummary",
								(NoResString)"Products (non-CW1) Billing Summary Decimal Point",
								(NoResString)"The non-CargoWiseOne products configured in the registry will produce a billing summary with 3 decimal points. These elements are List price, Total Price, Discounted cost, Extension price with discount, Pricing summary by currency, and Total billable.\r\nThis setting only affects the Billing Summary.",
								3,
								new CodeDescriptionPairListEditorInfo(true, true, CodeDescriptionPairListEditorInfo.CharacterCasing.Upper, CodeDescriptionPairListEditorInfo.CharacterCasing.Normal,
									(NoResString)"Product Code", (NoResString)"Description"),
								RegistryStorageFlags.System,
								false,
								RegistryOptions.Default,
								new CodeDescriptionPairList(),
								true,
								(NoResString)LicenceBillingCategory);
					});
			}
		}

		public CodeDescriptionBoolRegistryItem BillingDisbursementUsageMappings
		{
			get
			{
				return GetItem("BillingDisbursementUsageMappings", delegate
				{
					var editorInfo = new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Active", false, false);
					return new CodeDescriptionBoolRegistryItem(
						"BillingDisbursementUsageMappings",
						(NoResString)StlBillingCategory,
						(NoResString)"STL Billing Disbursement Usage Mappings",
						(NoResString)"Mapping configuration that links Disbursement Codes to Usage Codes (e.g., SHD → SHP, BRD → IF2,IFG).",
						RegistryStorageFlags.System,
						editorInfo,
						new CodeDescriptionBoolCollection(50));
				});
			}
		}

		public UsageMinimumFeeSettingsRegistryItem UsageMinimumFeeSettings
		{
			get
			{
				return GetItem("UsageMinimumFeeSettings",
					delegate
					{
						return new UsageMinimumFeeSettingsRegistryItem("UsageMinimumFeeSettings",
							(NoResString)LicenceBillingCategory,
							(NoResString)"Products (non-CW1) Enabled for Minimum Fee usage",
							(NoResString)"System Pricelist added to this registry will trigger a minimum fee usage to Non-CW1 production license DBs that do not have usage for the billing period. The Usage Code column must match the code of the System Pricelist in WISGLOSYD2.",
							RegistryStorageFlags.System);
					});
			}
		}

		#region External Servers

		public const string ExternalServersCategory = LicenceBillingCategory + "/External Servers";

		public const string ExternalServersEDIHostingStatsUsageDBCategory = ExternalServersCategory + "/Hosting Stats Usage";

		public StringRegistryItem EDIHostingStatsUsageDBServerName
		{
			get
			{
				return GetItem("EDIHostingStatsUsageDBServerName", delegate
				{
					return new StringRegistryItem(
						"EDIHostingStatsUsageDBServerName",
						(NoResString)ExternalServersEDIHostingStatsUsageDBCategory,
						(NoResString)"EDI Hosting Stats Usage Database Server Name",
						(NoResString)"Enter the name of server which hosts Hosting Stats Usage database.",
						RegistryStorageFlags.System,
						"hostingstats.db.wisegrid.net");
				});
			}
		}

		public StringRegistryItem EDIHostingStatsUsageDBName
		{
			get
			{
				return GetItem("EDIHostingStatsUsageDBName", delegate
				{
					return new StringRegistryItem(
						"EDIHostingStatsUsageDBName",
						(NoResString)ExternalServersEDIHostingStatsUsageDBCategory,
						(NoResString)"EDI Hosting Stats Usage Database Name",
						(NoResString)"Enter the name of database which stores Hosting Stats Usage.",
						RegistryStorageFlags.System,
						"HostingStats");
				});
			}
		}

		public ServerUsernamePasswordConfigurationRegistryItem EDIHostingStatsUsageDBLogin
		{
			get
			{
				return GetItem("EDIHostingStatsUsageDBLogin", delegate
				{
					ServerUsernamePasswordConfiguration defaultValue = new ServerUsernamePasswordConfiguration();
					defaultValue.UserName = "edibillingreadonly";
					defaultValue.Password = "password";
					defaultValue.ConfirmPassword = defaultValue.Password;

					return new ServerUsernamePasswordConfigurationRegistryItem(
							"EDIHostingStatsUsageDBLogin",
							(NoResString)ExternalServersEDIHostingStatsUsageDBCategory,
							(NoResString)"EDI Hosting Stats Usage Database Login",
							(NoResString)"Enter login information to the database which stores Hosting Stats Usage.",
							RegistryStorageFlags.System,
							defaultValue);
				});
			}
		}

		public const string ExternalServersEDIFaxUsageDBCategory = ExternalServersCategory + "/Fax Usage";

		public StringRegistryItem EDIFaxUsageDBServerName
		{
			get
			{
				return GetItem("EDIFaxUsageDBServerName", delegate
				{
					return new StringRegistryItem(
						"EDIFaxUsageDBServerName",
						(NoResString)ExternalServersEDIFaxUsageDBCategory,
						(NoResString)"EDI Fax Usage Database Server Name",
						(NoResString)"Enter the name of server which hosts Fax Usage database.",
						RegistryStorageFlags.System,
						"sydedifax.db.wtg.zone");
				});
			}
		}

		public StringRegistryItem EDIFaxUsageDBName
		{
			get
			{
				return GetItem("EDIFaxUsageDBName", delegate
				{
					return new StringRegistryItem(
						"EDIFaxUsageDBName",
						(NoResString)ExternalServersEDIFaxUsageDBCategory,
						(NoResString)"EDI Fax Usage Database Name",
						(NoResString)"Enter the name of database which stores Fax Usage.",
						RegistryStorageFlags.System,
						"EDIFaxDB");
				});
			}
		}

		public ServerUsernamePasswordConfigurationRegistryItem EDIFaxUsageDBLogin
		{
			get
			{
				return GetItem("EDIFaxUsageDBLogin", delegate
				{
					ServerUsernamePasswordConfiguration defaultValue = new ServerUsernamePasswordConfiguration();
					defaultValue.UserName = "edifaxreadonly";
					defaultValue.Password = "password";
					defaultValue.ConfirmPassword = defaultValue.Password;

					return new ServerUsernamePasswordConfigurationRegistryItem(
							"EDIFaxUsageDBLogin",
							(NoResString)ExternalServersEDIFaxUsageDBCategory,
							(NoResString)"EDI Fax Usage Database Login",
							(NoResString)"Enter login information to the database which stores Fax Usage.",
							RegistryStorageFlags.System,
							defaultValue);
				});
			}
		}

		public const string ExternalServersEDIERouterUsageDBCategory = ExternalServersCategory + "/eRouter Usage";

		public StringRegistryItem EDIERouterUsageDBServerName
		{
			get
			{
				return GetItem("EDIERouterUsageDBServerName", delegate
				{
					return new StringRegistryItem(
						"EDIERouterUsageDBServerName",
						(NoResString)ExternalServersEDIERouterUsageDBCategory,
						(NoResString)"EDI eRouter Usage Database Server Name",
						(NoResString)"Enter the name of server which hosts eRouter Usage database.",
						RegistryStorageFlags.System,
						"SYDBILLING.DB.WTG.ZONE");
				});
			}
		}

		public StringRegistryItem EDIERouterUsageDBName
		{
			get
			{
				return GetItem("EDIERouterUsageDBName", delegate
				{
					return new StringRegistryItem(
						"EDIERouterUsageDBName",
						(NoResString)ExternalServersEDIERouterUsageDBCategory,
						(NoResString)"EDI eRouter Usage Database Name",
						(NoResString)"Enter the name of database which stores eRouter Usage.",
						RegistryStorageFlags.System,
						"ediprod_BillingUsageArchive");
				});
			}
		}

		public ServerUsernamePasswordConfigurationRegistryItem EDIERouterUsageDBLogin
		{
			get
			{
				return GetItem("EDIERouterUsageDBLogin", delegate
				{
					ServerUsernamePasswordConfiguration defaultValue = new ServerUsernamePasswordConfiguration();
					defaultValue.UserName = "edibillingreadonly";
					defaultValue.Password = "password";
					defaultValue.ConfirmPassword = defaultValue.Password;

					return new ServerUsernamePasswordConfigurationRegistryItem(
							"EDIERouterUsageDBLogin",
							(NoResString)ExternalServersEDIERouterUsageDBCategory,
							(NoResString)"EDI eRouter Usage Database Login",
							(NoResString)"Enter login information to the database which stores eRouter Usage.",
							RegistryStorageFlags.System,
							defaultValue);
				});
			}
		}

		public const string ExternalServersEDIBillingTestingDatabaseCategory = ExternalServersCategory + "/Billing Testing";

		public StringRegistryItem EDIBillingTestingDatabaseServerName
		{
			get
			{
				return GetItem("EDIBillingTestingDatabaseServerName", delegate
				{
					return new StringRegistryItem(
						"EDIBillingTestingDatabaseServerName",
						(NoResString)ExternalServersEDIBillingTestingDatabaseCategory,
						(NoResString)"EDI Billing Testing Database Server Name",
						(NoResString)"Enter the name of server which hosts Billing Testing database.",
						RegistryStorageFlags.System,
						"");
				});
			}
		}

		public StringRegistryItem EDIBillingTestingDatabaseName
		{
			get
			{
				return GetItem("EDIBillingTestingDatabaseName", delegate
				{
					return new StringRegistryItem(
						"EDIBillingTestingDatabaseName",
						(NoResString)ExternalServersEDIBillingTestingDatabaseCategory,
						(NoResString)"EDI Billing Testing Database Name",
						(NoResString)"Enter the name of database which stores Billing Testing.",
						RegistryStorageFlags.System,
						"");
				});
			}
		}

		public ServerUsernamePasswordConfigurationRegistryItem EDIBillingTestingDatabaseLogin
		{
			get
			{
				return GetItem("EDIBillingTestingDatabaseLogin", delegate
				{
					ServerUsernamePasswordConfiguration defaultValue = new ServerUsernamePasswordConfiguration();
					defaultValue.UserName = "edibillingtesting";
					defaultValue.Password = "";
					defaultValue.ConfirmPassword = defaultValue.Password;

					return new ServerUsernamePasswordConfigurationRegistryItem(
							"EDIBillingTestingDatabaseLogin",
							(NoResString)ExternalServersEDIBillingTestingDatabaseCategory,
							(NoResString)"EDI Billing Testing Database Login",
							(NoResString)"Enter login information to the database which stores Billing Testing.",
							RegistryStorageFlags.System,
							defaultValue);
				});
			}
		}

		#endregion

		#endregion

		#region Archive Manager

		static MultilingualString System_ArchiveManager_PurgeEdiProdSpecificTablesSystem
			=> CombineCategories(SystemDataRegistry.Categories.System_ArchiveManager, (NoResString)"Purge ediProd Specific Tables System");

		public IntRegistryItem PurgeEdiProdSpecificTablesOnOrBeforeMinimum
			=> GetItem("PurgeEdiProdSpecificTablesOnOrBeforeMinimum", () =>
				{
					return new IntRegistryItem(
						name: "PurgeEdiProdSpecificTablesOnOrBeforeMinimum",
						category: System_ArchiveManager_PurgeEdiProdSpecificTablesSystem,
						caption: (NoResString)"On or Before Minimum",
						hint: (NoResString)"This registry allows you to define the minimum number of years that a record must have been in the system before it can be purged using the Purge ediProd Specific Tables Purge System (EST).\n\nWarning: Please make sure to confirm the data retention requirements for this data before purging.",
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.IsOnlyForSupport,
						defaultValue: 7,
						minValue: 0,
						maxValue: 100);
				});

		public IntRegistryItem PurgeEdiProdSpecificTablesBatchSizeControl
			=> GetItem("PurgeEdiProdSpecificTablesBatchSizeControl", () =>
				{
					return new IntRegistryItem(
						name: "PurgeEdiProdSpecificTablesBatchSizeControl",
						category: System_ArchiveManager_PurgeEdiProdSpecificTablesSystem,
						caption: (NoResString)"Set Batch Size",
						hint: (NoResString)"The maximum number of records to be processed in one batch by Archive Manager using the Purge ediProd Specific Tables System (EST).",
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.IsOnlyForSupport,
						defaultValue: 10000,
						minValue: 1,
						maxValue: 10000);
				});

		#endregion

		#region Avalara US Sales Tax Integration

		public const string AvalaraUSSalesTaxCategory = Category + "/Avalara US Sales Tax Integration";

		public CodePairRegistryItem AvalaraIntegrationSettingStatus
			=> GetItem("AvalaraIntegrationSettingStatus", () =>
					new CodePairRegistryItem(
						"AvalaraIntegrationSettingStatus",
						(NoResString)AvalaraUSSalesTaxCategory,
						(NoResString)"Integration Setting Status",
						(NoResString)@"The default value for this registry is 'OFF', which means no messages are exchanged with Avalara at all.
When this registry is configured to 'SND - Sandbox', transactions posted in this login company will be exchanged with the Avalara Sandbox system.
When this registry is configured to 'PRD - Production', transactions posted in this login company will be exchanged with the Avalara Production system.",
						new CodeDescriptionPairListProvider(() => Billing.Business.USSalesTax.AvalaraConstants.IntegrationStatus.List()),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						Billing.Business.USSalesTax.AvalaraConstants.IntegrationStatus.Codes.Off
					)
			);

		public StringRegistryItem AvalaraCompanyCode
			=> GetItem("AvalaraCompanyCode", () =>
					new StringRegistryItem(
						"AvalaraCompanyCode",
						(NoResString)AvalaraUSSalesTaxCategory,
						(NoResString)"Avalara Company Code",
						(NoResString)"Set the exact Avalara Company Code (from the Avalara Website > Settings > Manage Companies > Company Code) for this ediProd login company.",
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						string.Empty
					)
			);

		public GuidRegistryItem AvalaraSalesTaxChargeCode
			=> GetItem("AvalaraSalesTaxChargeCode", () =>
					new GuidRegistryItem(
						"AvalaraSalesTaxChargeCode",
						(NoResString)AvalaraUSSalesTaxCategory,
						(NoResString)"Sales Tax Charge Code",
						(NoResString)"Set the ediProd charge code for this login company to be used for recording and reporting US Sales tax via the Avalara integration. The charge code MUST not be used for ANY OTHER PURPOSE.",
						RegistryStorageFlags.Company
					)
					{
						EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.RevenueChargeCode)
					}
			);

		public StringRegistryItem AvalaraAuthenticationSandboxUserId
			=> GetItem("AvalaraAuthenticationSandboxUserId", () =>
					new StringRegistryItem(
						"AvalaraAuthenticationSandboxUserId",
						(NoResString)AvalaraUSSalesTaxCategory,
						(NoResString)"Sandbox - User ID",
						(NoResString)"Enter the Avalara Sandbox User ID in this registry. This value will be used if the Integration Setting Status registry is set to 'SND - Sandbox'.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						string.Empty
					)
			);

		public StringRegistryItem AvalaraAuthenticationSandboxPassword
			=> GetItem("AvalaraAuthenticationSandboxPassword", () =>
					new StringRegistryItem(
						"AvalaraAuthenticationSandboxPassword",
						(NoResString)AvalaraUSSalesTaxCategory,
						(NoResString)"Sandbox - License Key / Password",
						(NoResString)"Enter the Avalara Sandbox License Key in this registry. This value will be used if the Integration Setting Status registry is set to 'SND - Sandbox'.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						string.Empty
					)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password),
					}
			);

		public StringRegistryItem AvalaraAuthenticationProductionUserId
			=> GetItem("AvalaraAuthenticationProductionUserId", () =>
					new StringRegistryItem(
						"AvalaraAuthenticationProductionUserId",
						(NoResString)AvalaraUSSalesTaxCategory,
						(NoResString)"Production - User ID",
						(NoResString)"Enter the Avalara Production User ID in this registry. This value will be used if the Integration Setting Status registry is set to 'PRD - Production'.",
						RegistryStorageFlags.System,
						string.Empty
					)
			);

		public StringRegistryItem AvalaraAuthenticationProductionPassword
			=> GetItem("AvalaraAuthenticationProductionPassword", () =>
					new StringRegistryItem(
						"AvalaraAuthenticationProductionPassword",
						(NoResString)AvalaraUSSalesTaxCategory,
						(NoResString)"Production - License Key / Password",
						(NoResString)"Enter the Avalara Production License Key in this registry. This value will be used if the Integration Setting Status registry is set to 'PRD - Production'.",
						RegistryStorageFlags.System,
						string.Empty
					)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password),
					}
			);

		public DecimalRegistryItem AvalaraWebTimeoutSeconds
			=> GetItem("AvalaraWebTimeoutSeconds", () =>
					new DecimalRegistryItem(
						"AvalaraWebTimeoutSeconds",
						(NoResString)AvalaraUSSalesTaxCategory,
						(NoResString)"Web Request Timeout",
						(NoResString)@"Enter the timeout in seconds used for all Avalara web requests. Any web request which takes longer than this time will be reported as an error.

The default value for this registry is 60 seconds. Minimum is 0.001 (1 ms). Maximum is 300 (5 minutes). Up to 3 decimal places can be entered.",
						new NumericRegistryEditorInfo(decimalPlaces: 3),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						60m,
						0.001,
						5 * 60.0
					)
			);

		#endregion

		#region Organization Membership Types

		public CodeDescriptionBoolRegistryItem OrgMembershipTypes
		{
			get
			{
				return GetItem("OrgMembershipTypes", delegate
				{
					var defaultValue = new CodeDescriptionBoolCollection(10);
					defaultValue.Add("FTA", ResString.GetMultilingualString("328179a4-9c26-438e-804f-1b0d0f736642", "Freight & Trade Alliance (AU)"), false);
					defaultValue.Add("CBAFF", ResString.GetMultilingualString("a27a172f-1e5b-4f6b-8b0b-aeeab8939120", "Customs Brokers and Freight Forwarders Federation of New Zealand Inc"), false);
					defaultValue.Add("WIPM", ResString.GetMultilingualString("47bddb84-2ca9-44c6-819f-c98617b1f9b8", "WiseIndustry Partner Member"), true);
					defaultValue.Add("WIP", ResString.GetMultilingualString("f04e26a2-1ef0-495f-bbbc-bd2c26e7dafa", "WiseIndustry Partner"), false);
					defaultValue.Add("CSP", ResString.GetMultilingualString("aedbec85-4afa-441a-8fa4-090d87f19f9f", "CargoWise Service Partner"), false);
					defaultValue.Add("CBP", ResString.GetMultilingualString("71d5422f-ac8a-4dff-a1ed-45edbd22fad7", "CargoWise Business Partner"), false);
					defaultValue.Add("CRP", ResString.GetMultilingualString("887c41ed-f942-451f-ae52-553dfe2f5b67", "CargoWise Referral Partner"), false);
					defaultValue.Add("CTP", ResString.GetMultilingualString("4a66c4c8-3f86-4818-b213-ab521eb5bd8a", "CargoWise Technical Partner"), false);
					defaultValue.Add("CEP", ResString.GetMultilingualString("c510328d-8ba3-4378-aa7e-f7a003d69563", "CargoWise Education Partner"), false);
					defaultValue.Add("CIP", ResString.GetMultilingualString("76934c1e-fce3-464c-8d01-561974cde84f", "CargoWise Industry Partner"), false);
					defaultValue.Add("MSP", ResString.GetMultilingualString("ea0e1009-e2e6-45b2-a347-fd32d56e818e", "Microlistics Service Partner"), false);
					defaultValue.Add("MBP", ResString.GetMultilingualString("c89c0b75-0725-454d-b52f-23c912156029", "Microlistics Business Partner"), false);
					defaultValue.Add("MRP", ResString.GetMultilingualString("2d50dd17-8dcb-42a3-b281-f8fcdfced7e3", "Microlistics Referral Partner "), false);
					defaultValue.Add("SSP", ResString.GetMultilingualString("48e8c091-34c1-40bb-b6aa-22fdd6b99c3a", "Smartfreight Service Partner"), false);
					defaultValue.Add("SBP", ResString.GetMultilingualString("1fe57b21-7035-49a7-8707-91906f99aa0f", "Smartfreight Business Partner"), false);
					defaultValue.Add("SRP", ResString.GetMultilingualString("5c0f90e8-707a-4846-a3e3-e97a8e51dab9", "Smartfreight Referral Partner "), false);
					defaultValue.Add("TSSP", ResString.GetMultilingualString("c7e044a3-aa79-4aab-965c-511bcd28d5f2", "Transtream Service Partner"), false);
					defaultValue.Add("TSBP", ResString.GetMultilingualString("1f0af7c3-a75f-4e21-a3fa-41142a943729", "Transtream Business Partner"), false);
					defaultValue.Add("TSRP", ResString.GetMultilingualString("c9a40109-0b0f-4ce5-97bd-5a290f0629a8", "Transtream Referral Partner"), false);
					defaultValue.Add("TSP", ResString.GetMultilingualString("c14a1fd7-0a50-444d-92bd-ae751bf48046", "Trinium Service Partner"), false);
					defaultValue.Add("TBP", ResString.GetMultilingualString("1caf64f1-e1b7-4c4d-95e5-4cc668280d5a", "Trinium Business Partner"), false);
					defaultValue.Add("TRP", ResString.GetMultilingualString("25e65e74-196b-469f-ac7f-7276fb300ab5", "Trinium Referral Partner"), false);

					var codeDescriptionBoolRegistryItem = new CodeDescriptionBoolRegistryItem(
						"OrgMembershipTypes",
						(NoResString)Category,
						(NoResString)"Organization Membership Types",
						(NoResString)"The list of valid types for Organization Membership",
						RegistryStorageFlags.System,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("ca9e285d-8c69-44e4-99c8-8f23926eea46", "Organization is Required")),
						defaultValue
					);
					return codeDescriptionBoolRegistryItem;
				});
			}
		}

		#endregion

		#region Client Database System Info Lookup Values

		public CodeDescriptionPairListRegistryItem OSNames
		{
			get
			{
				return GetItem("OSNames", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						"OSNames",
						(NoResString)VersionReportingSubCategory,
						(NoResString)"OS Names",
						(NoResString)"The list of OS names from clients' systems.",
						64,
						RegistryStorageFlags.System,
						RegistryOptions.NotCached);
				});
			}
		}

		public CodeDescriptionPairListRegistryItem OSVersions
		{
			get
			{
				return GetItem("OSVersions", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						"OSVersions",
						(NoResString)VersionReportingSubCategory,
						(NoResString)"OS Versions",
						(NoResString)"The list of OS versions from clients' systems.",
						64,
						RegistryStorageFlags.System,
						RegistryOptions.NotCached);
				});
			}
		}

		public CodeDescriptionPairListRegistryItem SystemManufacturers
		{
			get
			{
				return GetItem("SystemManufacturers",
					delegate
					{
						return new CodeDescriptionPairListRegistryItem(
							"SystemManufacturers",
							(NoResString)VersionReportingSubCategory,
							(NoResString)"System Manufacturers",
							(NoResString)"The list of system manufacturers from clients' systems.",
							64,
							RegistryStorageFlags.System,
							RegistryOptions.NotCached);
					});
			}
		}

		public CodeDescriptionPairListRegistryItem ProcessorTypes
		{
			get
			{
				return GetItem("ProcessorTypes",
					delegate
					{
						return new CodeDescriptionPairListRegistryItem(
							"ProcessorTypes",
							(NoResString)VersionReportingSubCategory,
							(NoResString)"Processor Types",
							(NoResString)"The list of processor types from clients' systems.",
							256,
							RegistryStorageFlags.System,
							RegistryOptions.NotCached);
					});
			}
		}

		public CodeDescriptionPairListRegistryItem VirtualMachineDetectionKeywords
		{
			get
			{
				CodeDescriptionPairList defaultList = new CodeDescriptionPairList();
				defaultList.AddPair("Virtual Machine", "Virtual Machine");
				defaultList.AddPair("VMware", "VMware");

				return GetItem("VirtualMachineDetectionKeywords",
					delegate
					{
						return new CodeDescriptionPairListRegistryItem(
							"VirtualMachineDetectionKeywords",
							(NoResString)VersionReportingSubCategory,
							(NoResString)"Virtual Machine Detection Keywords",
							(NoResString)"The list of keywords which indicating clients' systems are virtual machine.",
							256,
							RegistryStorageFlags.System,
							false,
							RegistryOptions.NotCached,
							defaultList);
					});
			}
		}

		#endregion

		#region Org Name Change Notification

		public StringArrayRegistryItem OrgNameChangeNotificationAddresses
		{
			get
			{
				return GetItem("OrgNameChangeNotificationAddresses", delegate
				{
					return new StringArrayRegistryItem(
					"OrgNameChangeNotificationAddresses",
					EmailAddressesCategory,
					(NoResString)"Org Name Change Notification Recipients",
					(NoResString)"Email addresses to receive Organization name change notifications",
					RegistryStorageFlags.System);
				});
			}
		}

		#endregion

		#region Translogix Org Name Change Notification

		public StringArrayRegistryItem TranslogixOrgNameChangeNotificationAddresses
		{
			get
			{
				return GetItem("TranslogixOrgNameChangeNotificationAddresses", delegate
				{
					return new StringArrayRegistryItem(
					"TranslogixOrgNameChangeNotificationAddresses",
					EmailAddressesCategory,
					(NoResString)"Translogix Org Name Change Notification Recipients",
					(NoResString)"Email addresses to receive Translogix Organization name change notifications.",
					RegistryStorageFlags.System);
				});
			}
		}

		#endregion

		#region External Monitoring

		public StringRegistryItem ExternalMonitoringWiseGridReportingConnectionString
		{
			get
			{
				return GetItem("ExternalMonitoringWiseGridReportingConnectionString", delegate
				{
					var result = new StringRegistryItem(
						"ExternalMonitoringWiseGridReportingConnectionString",
						System_ExternalMonitoring,
						ResString.GetMultilingualString("e1f4adb4-b94d-4ed6-9a4c-6f53dee3dd27", "CORP Reporting Database Connection String"),
						ResString.GetMultilingualString("072608e5-7b9e-4248-a53c-ca53d72be63f", "Please provide the connection string to CORP reporting database."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						string.Empty)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo)
					};
					return result;
				});
			}
		}

		public StringRegistryItem ExternalMonitoringSqlCpuUsageExecutionPlanUri
		{
			get
			{
				return GetItem(nameof(ExternalMonitoringSqlCpuUsageExecutionPlanUri), delegate
				{
					var result = new StringRegistryItem(
						nameof(ExternalMonitoringSqlCpuUsageExecutionPlanUri),
						System_ExternalMonitoring,
						ResString.GetMultilingualString("52a004be-b9a7-4b8b-b9d3-33d8078e4261", "Kibana CPU Usage Query Hash Discover URL"),
						ResString.GetMultilingualString("6d1dec16-de39-4c86-9d88-d4d74a17cdea", "Please provide the URL to the Kibana Discover URL to show the query hash statistics. It should have {{0}} in it accepting the query hash value"),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						string.Empty)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo)
					};
					return result;
				});
			}
		}

		public IntRegistryItem ExternalMonitoringSqlCpuUsageLowerThreshold
		{
			get
			{
				return GetItem("ExternalMonitoringSqlCpuUsageLowerThreshold", delegate
				{
					return new IntRegistryItem(
						"ExternalMonitoringSqlCpuUsageLowerThreshold",
						System_ExternalMonitoring,
						(NoResString)"CPU Usage Lower Threshold for Raising a WI",
						(NoResString)"This is the minimum value in seconds of the sum of CPU Usage Time for the last 24 hours per query hash to raise a WI when exceeded.",
						RegistryStorageFlags.System,
						10000);
				});
			}
		}

		public ExternalMonitoringSqlExecutionPlanRetrieveTimeoutRegistryItem ExternalMonitoringSqlExecutionPlanRetrieveTimeout
		{
			get
			{
				return GetItem(nameof(ExternalMonitoringSqlExecutionPlanRetrieveTimeout),
					() => new ExternalMonitoringSqlExecutionPlanRetrieveTimeoutRegistryItem(
						nameof(ExternalMonitoringSqlExecutionPlanRetrieveTimeout),
						System_ExternalMonitoring,
						(NoResString)"SQL Execution Plan Retrieve Timeout",
						(NoResString)"The timeout (in milliseconds) of retrieving the SQL execution plan. It should be greater than 0.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						5000));
			}
		}

		public ExternalMonitoringSqlExecutionPlanQueryUriRegistryItem ExternalMonitoringSqlExecutionPlanQueryUri
		{
			get
			{
				return GetItem(nameof(ExternalMonitoringSqlExecutionPlanQueryUri),
					() => new ExternalMonitoringSqlExecutionPlanQueryUriRegistryItem(
						nameof(ExternalMonitoringSqlExecutionPlanQueryUri),
						System_ExternalMonitoring,
						(NoResString)"SQL Execution Plan Query URL",
						(NoResString)@"Please provide the URL for querying SQL execution plan. It should have ""{0}"" accepting the query hash value, and ""{1}"" accepting the database server instance name in the query string.",
						new TextRegistryEditorInfo(TextEditorType.Url),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						@"http://wiselake-systemusage-web.wisecloud.zone/CPUUsageNetCore/api/Search/SearchQueryHash?QueryHash={0}&ServerInstanceName={1}"));
			}
		}

		#region ElasticSearchAdaptorSettings

		public StringRegistryItem ElasticSearchAdaptorRequestUrl
		{
			get
			{
				var defaultValue = "https://r.prod-1.es.wtg.ws";
				return GetItem(nameof(ElasticSearchAdaptorRequestUrl), delegate
				{
					var result = new StringRegistryItem(
						nameof(ElasticSearchAdaptorRequestUrl),
						System_ElasticSearchAdaptorSettings,
						(NoResString)"Elasticsearch Request URL",
						(NoResString)"Please provide the Elasticsearch endpoint URL.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue);
					return result;
				});
			}
		}

		public StringRegistryItem ElasticSearchAdaptorIndex
		{
			get
			{
				var defaultValue = "idx-au2-prod-sqlcpumonitoring-prod";
				return GetItem(nameof(ElasticSearchAdaptorIndex), delegate
				{
					var result = new StringRegistryItem(
						nameof(ElasticSearchAdaptorIndex),
						System_ElasticSearchAdaptorSettings,
						(NoResString)"Elasticsearch Index for SQL CPU monitoring",
						(NoResString)"Please provide the Elasticsearch index for SQL CPU monitoring.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo)
					};
					return result;
				});
			}
		}

		public StringRegistryItem ElasticSearchAdaptorUsername
		{
			get
			{
				var defaultValue = "es_systemusage";
				return GetItem(nameof(ElasticSearchAdaptorUsername), delegate
				{
					var result = new StringRegistryItem(
						nameof(ElasticSearchAdaptorUsername),
						System_ElasticSearchAdaptorSettings,
						(NoResString)"Elasticsearch Username",
						(NoResString)"Please provide the username to access Elasticsearch endpoint.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue);
					return result;
				});
			}
		}

		public StringRegistryItem ElasticSearchAdaptorPassword
		{
			get
			{
				var defaultValue = "pasoidmndhASLKdhjkAsbhdjLAGSDKBN$AdAJsdbajsgdbAGSdiuAtgTPOwe8n231y23n1!$";
				return GetItem(nameof(ElasticSearchAdaptorPassword), delegate
				{
					var result = new StringRegistryItem(
						nameof(ElasticSearchAdaptorPassword),
						System_ElasticSearchAdaptorSettings,
						(NoResString)"Elasticsearch Password",
						(NoResString)"Please provide the password to access Elasticsearch endpoint.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password),
					};
					return result;
				});
			}
		}

		public StringRegistryItem ElasticSearchAdaptorRequestStringWithFilter
		{
			get
			{
				var defaultValue = "{\"index\":[\"wisecloud-syd-systemusage-2*\"],\"ignore_unavailable\":true,\"preference\":1521967565949}\r\n{\"size\":0,\"_source\":{\"excludes\":[]},\"aggs\":{\"topLevelAggregation\":{\"terms\":{\"field\":\"System.keyword\",\"size\":35,\"missing\":\"__missing__\"},\"aggs\":{\"AggCPUTimeMS\":{\"sum\":{\"field\":\"CpuTimeMilliseconds\"}},\"SubAggregation1\":{\"terms\":{\"field\":\"QueryHash.keyword\",\"size\":{MaxAlertPerDayPerSystem},\"order\":{\"AggCPUTimeMS\":\"desc\"}},\"aggs\":{\"AggCPUTimeMS\":{\"sum\":{\"field\":\"CpuTimeMilliseconds\"}},\"SubAggregation2\":{\"terms\":{\"field\":\"Owner.keyword\",\"size\":1,\"order\":{\"AggCPUTimeMS\":\"desc\"},\"missing\":\"__missing__\"},\"aggs\":{\"AggCPUTimeMS\":{\"sum\":{\"field\":\"CpuTimeMilliseconds\"}},\"5\":{\"sum\":{\"field\":\"Qty\"}},\"6\":{\"sum\":{\"field\":\"LogicalReads\"}},\"7\":{\"sum\":{\"field\":\"Writes\"}},\"8\":{\"max\":{\"field\":\"CollectSystemTimeUtc\"}},\"9\":{\"min\":{\"field\":\"ExeDate\"}},\"10\":{\"max\":{\"field\":\"ExeDate\"}},\"11\":{\"max\":{\"script\":{\"lang\":\"painless\",\"source\":\"String versionString=doc['CurrentVersion.keyword'].value;double result=0;int offset=0;int next=0;for(int i=3;i>=0;i--){next=versionString.indexOf('.',offset);if(next==-1)next=versionString.length();result=result+Integer.parseInt(versionString.substring(offset,next))*Math.pow(1000,i);offset=next+1}return result;\"}}}}}}}}}},\"stored_fields\":[\"*\"],\"script_fields\":{},\"docvalue_fields\":[\"CollectSystemTimeUtc\"],\"query\":{\"bool\":{\"must\":[{\"match_all\":{}},{\"range\":{\"ExeDate\":{\"gt\":\"now-3M\"}}},{\"range\":{\"CollectSystemTimeUtc\":{\"gte\":\"{StartDate}\",\"lte\":\"{EndDate}\",\"format\":\"epoch_millis\"}}},{\"terms\":{\"System\":[{CapabilityList}]}}],\"filter\":[],\"should\":[],\"must_not\":[{\"term\":{\"QueryHash\":\"0\"}},{\"term\":{\"Owner\":\"customer\"}}]}}}\r\n";
				return GetItem(nameof(ElasticSearchAdaptorRequestStringWithFilter), delegate
				{
					var result = new StringRegistryItem(
						nameof(ElasticSearchAdaptorRequestStringWithFilter),
						System_ElasticSearchAdaptorSettings,
						(NoResString)"Elasticsearch Request String",
						(NoResString)"Please provide request string to retrieve data for creating SQL performance WI.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo)
					};
					return result;
				});
			}
		}

		public static MultilingualString System_ElasticSearchAdaptorSettings { get { return CombineCategories(System_ExternalMonitoring, ResString.GetMultilingualString("957E911A-D94E-43ED-8D26-EEE4E375A822", "Elasticsearch Adapter Settings")); } }

		#endregion ElasticSearchAdaptorSettings

		static MultilingualString System_ExternalMonitoring { get { return CombineCategories((NoResString)EDIDataRegistry.Category, ResString.GetMultilingualString("9b9cb45f-9734-4388-a42a-00086f7b4892", "External Monitoring")); } }

		#endregion

		public const string PricelistSubCategory = Category + "/Price List";

		#region PricelistCountryRegions

		public CodeDescriptionPairListRegistryItem PriceItemDiscountTypes
		{
			get
			{
				return GetItem("EDIPriceItemDiscountTypes", delegate
				{
					var defaultValue = new CodeDescriptionPairList();
					defaultValue.AddPair((NoResString)"DS1", ResString.GetMultilingualString("BE3D359B-D601-41DD-8CFB-341679FDEB7A", "Single Domestic Trading Entity < 20 Users"));
					defaultValue.AddPair((NoResString)"DS2", ResString.GetMultilingualString("011C4F50-B7A1-45D6-945F-6D1939B459C1", "Single Domestic Trading Entity >= 20 Users"));
					defaultValue.AddPair((NoResString)"DDC", ResString.GetMultilingualString("920A1176-51DC-49DD-A994-215DA393DB77", "China Single Domestic Trading Entity < 20 Users"));
					defaultValue.AddPair((NoResString)"DM1", ResString.GetMultilingualString("C6D02028-ACFF-4B72-8E75-F1A294A3FD81", "Multiple Domestic Trading Entity - Single Country/Region"));
					defaultValue.AddPair((NoResString)"DDI", ResString.GetMultilingualString("022C51E0-331E-49C2-B478-EABFAE3DE642", "India Single Domestic Trading Entity < 20 Users"));
					defaultValue.AddPair((NoResString)"DD4", ResString.GetMultilingualString("3995C63C-D77D-4163-B187-AEA7C9324C09", "Tier 4 Discount - Single Domestic Trading Entity < 20 Users"));

					return new CodeDescriptionPairListRegistryItem(
						"EDIPriceItemDiscountTypes",
						(NoResString)PricelistSubCategory,
						(NoResString)"Price Item Discount Types",
						(NoResString)"List of Discount Codes that should have a negative value in price list",
						3,
						new CodeDescriptionPairListEditorInfo(true, true, CodeDescriptionPairListEditorInfo.CharacterCasing.Upper, CodeDescriptionPairListEditorInfo.CharacterCasing.Normal,
								(NoResString)"Discount Code", (NoResString)"Discount Description"),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue, false);
				});
			}
		}

		public CodeDescriptionPairListRegistryItem PricelistCountryRegions
		{
			get
			{
				return GetItem("EDIPricelistCountryRegions", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						"EDIPricelistCountryRegions",
						(NoResString)PricelistSubCategory,
						(NoResString)"Country/Region Regions",
						(NoResString)"List of Country/Region Code and Region (Description) for calculating Licence Edition",
						3,
						new CodeDescriptionListEditorInfoEx(),
						RegistryStorageFlags.System,
							RegistryOptions.Default,
						new CodeDescriptionPairList(), false);
				});
			}
		}

		#endregion

		#region Notification group for public devlopment

		public GuidRegistryItem InternalNotificationGroup
		{
			get
			{
				return GetItem("InternalNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"InternalNotificationGroup",
						(NoResString)Category,
						(NoResString)"Internal Notification Group",
						(NoResString)"This is the group that receives public development notifications.",
						RegistryStorageFlags.System,
						RegistryFactory.Instance.GetGroupPK("PMG"));
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public GuidRegistryItem NotificationGroupForADETask
		{
			get
			{
				return GetItem("NotificationGroupForADETask", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"NotificationGroupForADETask",
						(NoResString)Category,
						(NoResString)"Notification Group for ADE task",
						(NoResString)"This is the group that receive the notification from ADE task.",
						RegistryStorageFlags.System,
						RegistryFactory.Instance.GetGroupPK("PMG"));
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public GuidRegistryItem OnboardingNotificationGroup
		{
			get
			{
				return GetItem("OnboardingNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"OnboardingNotificationGroup",
						(NoResString)Category,
						(NoResString)"Notification Group When Automation Of Onboarding Status Is ERR",
						(NoResString)"This is the group that receive the notification from service task.",
						RegistryStorageFlags.System,
						RegistryFactory.Instance.GetGroupPK("PMG"));
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public GuidRegistryItem CertProcessingNotificationGroup
		{
			get
			{
				return GetItem("CertProcessingNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"CertProcessingNotificationGroup",
						(NoResString)Category,
						(NoResString)"Notification Group When Automation Of Certificate Processing Failed",
						(NoResString)"This is the group that receive the notification from service task.",
						RegistryStorageFlags.System,
						RegistryFactory.Instance.GetGroupPK("PMG"));
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region GitHub Users Group
		public GuidRegistryItem GitHubUsersGroup
		{
			get
			{
				return GetItem("GitHubUsersGroup", delegate
				{
					var result = new GuidRegistryItem(
						"GitHubUsersGroup",
						(NoResString)Category,
						(NoResString)"GitHub Users Group",
						(NoResString)"This is the group that includes GitHub users.",
						RegistryStorageFlags.System,
						RegistryFactory.Instance.GetGroupPK("GHUSERS"));
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}
		#endregion

		#region My Account Portal

		public const string MyAccountPortalSubCategory = Category + "/My Account";

		#region User Registration

		public GuidRegistryItem UserRegistrationNotificationGroup
		{
			get
			{
				return GetItem("UserRegistrationNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"UserRegistrationNotificationGroup",
						(NoResString)(MyAccountPortalSubCategory + "/User Registration"),
						(NoResString)"My Account New User Registration Notification Group",
						null,
						RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public CodeDescriptionPairListRegistryItem UserRegistrationJobRoleList
		{
			get
			{
				CodeDescriptionPairList defaultList = new CodeDescriptionPairList();
				defaultList.AddPair("Executive / General Management", "Executive / General Management");
				defaultList.AddPair("Operations - Freight", "Operations - Freight");
				defaultList.AddPair("Operations - Warehouse", "Operations - Warehouse");
				defaultList.AddPair("Operations - Customs Brokerage", "Operations - Customs Brokerage");
				defaultList.AddPair("Finance", "Finance");
				defaultList.AddPair("Business Development", "Business Development");
				defaultList.AddPair("Marketing", "Marketing");
				defaultList.AddPair("IT", "IT");
				defaultList.AddPair("Customer Service", "Customer Service");

				return GetItem("UserRegistrationJobRoleList", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						"UserRegistrationJobRoleList",
						(NoResString)(MyAccountPortalSubCategory + "/User Registration"),
						(NoResString)"Predefined Job Roles For My Account User Registration",
						null,
						50,
						RegistryStorageFlags.System,
						false,
						defaultList);
				});
			}
		}

		public CodeDescriptionPairListRegistryItem UserRegistrationTypeOfBusinessList
		{
			get
			{
				CodeDescriptionPairList defaultList = new CodeDescriptionPairList();
				defaultList.AddPair("Freight Forwarding - International Air", "Freight Forwarding - International Air");
				defaultList.AddPair("Freight Forwarding - International Ocean", "Freight Forwarding - International Ocean");
				defaultList.AddPair("Freight Forwarding - Domestic", "Freight Forwarding - Domestic");
				defaultList.AddPair("Customs Brokerage", "Customs Brokerage");
				defaultList.AddPair("Warehouse", "Warehouse");
				defaultList.AddPair("Container Freight Station", "Container Freight Station");
				defaultList.AddPair("NVOCC", "NVOCC");

				return GetItem("UserRegistrationTypeOfBusinessList", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						"UserRegistrationTypeOfBusinessList",
						(NoResString)(MyAccountPortalSubCategory + "/User Registration"),
						(NoResString)"Predefined Business Types For My Account User Registration",
						null,
						50,
						RegistryStorageFlags.System,
						false,
						defaultList);
				});
			}
		}

		public CodeDescriptionBoolRegistryItem UserRegistrationReasonForRequestingAccessList
		{
			get
			{
				var defaultList = new CodeDescriptionBoolCollection(50);
				defaultList.Add("REQUEST PRODUCT INFORMATION", (NoResString)"Request Product Information", false);
				defaultList.Add("SPEAK WITH A CONSULTANT", (NoResString)"Speak with a Consultant", false);
				defaultList.Add("SUBSCRIBE TO THE NEWSLETTER", (NoResString)"Subscribe to the Newsletter", false);
				defaultList.Add("ACCESS LEARNING CENTER FOR PRODUCT REVIEW", (NoResString)"Access the Online Learning Center for Product Review", true);
				defaultList.Add("ACCESS LEARNING CENTER FOR PRODUCT TRAINING", (NoResString)"Access the Online Learning Center for Product Training", true);
				defaultList.Add("ACCESS TECHNICAL GUIDES", (NoResString)"Access Technical Guides", true);

				return GetItem("UserRegistrationReasonForRequestingAccessList", delegate
				{
					return new CodeDescriptionBoolRegistryItem(
						"UserRegistrationReasonForRequestingAccessList",
						(NoResString)(MyAccountPortalSubCategory + "/User Registration"),
						(NoResString)"Predefined Reasons For Requesting Access For My Account User Registration",
						null,
						RegistryStorageFlags.System,
						new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Is My Account Request"),
						defaultList);
				});
			}
		}

		public CodeDescriptionPairListRegistryItem UserRegistrationCompanySizeList
		{
			get
			{
				return GetItem("UserRegistrationCompanySizeList", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						"UserRegistrationCompanySizeList",
						(NoResString)(MyAccountPortalSubCategory + "/User Registration"),
						(NoResString)"Predefined Company Size For My Account User Registration",
						null,
						50,
						RegistryStorageFlags.System);
				});
			}
		}

		#endregion

		#region Terms and Conditions

		public NotificationEmailTemplateRegistryItem MyAccountTermsAndConditionsNotificationEmailTemplate
		{
			get
			{
				return GetItem(
					"MyAccountTermsAndConditionsNotificationEmailTemplate",
					delegate
					{
						NotificationEmailTemplateRegistryItem registryItem = new NotificationEmailTemplateRegistryItem(
							"MyAccountTermsAndConditionsNotificationEmailTemplate",
							(NoResString)(MyAccountPortalSubCategory + "/Terms and Conditions"),
							(NoResString)"My Account Terms And Conditions Notification Email Template",
							(NoResString)"This is the default template for My Account Terms And Conditions notification emails.",
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							typeof(DocMyAccountWebContractEmail),
							DefaultMyAccountTermsAndConditionsNotificationEmailSubject,
							DefaultMyAccountTermsAndConditionsNotificationEmailBody);
						return registryItem;
					});
			}
		}

		const string DefaultMyAccountTermsAndConditionsNotificationEmailSubject = "Web Contract For CargoWise My Account Has Been Signed By (*ContactName*) From (*ClientName*)";

		const string DefaultMyAccountTermsAndConditionsNotificationEmailBody = "(*WebContractContent*)";

		public TermsAndConditionsRegistryItem MyAccountTermsAndConditionsContent
		{
			get
			{
				return GetItem(
					"MyAccountTermsAndConditionsContent",
					delegate
					{
						TermsAndConditionsRegistryItem registryItem = new TermsAndConditionsRegistryItem(
							"MyAccountTermsAndConditionsContent",
							(NoResString)(MyAccountPortalSubCategory + "/Terms and Conditions"),
							(NoResString)"My Account Terms And Conditions Content",
							(NoResString)"This is the content for My Account Terms And Conditions.",
							RegistryStorageFlags.System,
							typeof(DocMyAccountWebContract));
						return registryItem;
					});
			}
		}

		public TermsAndConditionsRegistryItem MyAccountContactTermsAndConditionsContent => GetItem(
					"MyAccountContactTermsAndConditionsContent",
					() => new TermsAndConditionsRegistryItem(
							"MyAccountContactTermsAndConditionsContent",
							(NoResString)(MyAccountPortalSubCategory + "/Terms and Conditions"),
							(NoResString)"My Account Terms And Conditions Content (Contact Level)",
							(NoResString)"This is the content for My Account Terms And Conditions. (Contact Level)",
							RegistryStorageFlags.System,
							typeof(DocMyAccountWebContract)));

		public StringRegistryItem MyAccountTermsAndConditionsNotificationEmailSenderName
		{
			get
			{
				return GetItem("MyAccountTermsAndConditionsNotificationEmailSenderName", delegate
				{
					return new StringRegistryItem(
						"MyAccountTermsAndConditionsNotificationEmailSenderName",
						(NoResString)(MyAccountPortalSubCategory + "/Terms and Conditions"),
						(NoResString)"My Account Terms And Conditions Notification Email Sender Name",
						(NoResString)"This is the default sender's name for My Account Terms And Conditions notification emails.",
						RegistryStorageFlags.System,
						"CargoWise Marketing");
				});
			}
		}

		public StringRegistryItem MyAccountTermsAndConditionsNotificationEmailSenderAddress
		{
			get
			{
				return GetItem("MyAccountTermsAndConditionsNotificationEmailSenderAddress", delegate
				{
					return new StringRegistryItem(
						"MyAccountTermsAndConditionsNotificationEmailSenderAddress",
						(NoResString)(MyAccountPortalSubCategory + "/Terms and Conditions"),
						(NoResString)"My Account Terms And Conditions Notification Email Sender Email Address",
						(NoResString)"This is the default sender's email Address for My Account Terms And Conditions notification emails.",
						RegistryStorageFlags.System,
						"marketing@cargowise.com");
				});
			}
		}

		#endregion

		#region Download

		[SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "This is meant to specify an exact product name.")]
		public StringRegistryItem CW1DvdIsoFileDownloadURL
		{
			get
			{
				return GetItem("CW1DvdIsoFileDownloadURL", delegate
				{
					return new StringRegistryItem(
						"CW1DvdIsoFileDownloadURL",
						(NoResString)(MyAccountPortalSubCategory + "/Download"),
						(NoResString)"Download URL of CargoWise DVD in ISO File Format",
						(NoResString)"This is the download URL of CargoWise DVD in ISO file format.",
						RegistryStorageFlags.System);
				});
			}
		}

		[SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "This is meant to specify an exact product name.")]
		public StringRegistryItem CW1DvdZipFileDownloadURL
		{
			get
			{
				return GetItem("CW1DvdZipFileDownloadURL", delegate
				{
					return new StringRegistryItem(
						"CW1DvdZipFileDownloadURL",
						(NoResString)(MyAccountPortalSubCategory + "/Download"),
						(NoResString)"Download URL of CargoWise DVD in ZIP File Format",
						(NoResString)"This is the download URL of CargoWise DVD in ZIP file format.",
						RegistryStorageFlags.System);
				});
			}
		}

		[SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "This is meant to specify an exact product name.")]
		public StringRegistryItem CW1ExeFileDownloadURL
		{
			get
			{
				return GetItem("CW1ExeFileDownloadURL", delegate
				{
					return new StringRegistryItem(
						"CW1ExeFileDownloadURL",
						(NoResString)(MyAccountPortalSubCategory + "/Download"),
						(NoResString)"Download URL of CargoWise Web Components installer EXE",
						(NoResString)"This is the download URL for the CargoWise Web Components installer EXE.",
						RegistryStorageFlags.System,
						"https://myaccount-portal.cargowise.com/myaccount/downloads/CargoWiseOneWebServerSetup.exe");
				});
			}
		}

		#endregion

		#region Reports

		public StringRegistryItem MyAccountReportsDownloadURL
		{
			get
			{
				return GetItem("MyAccountReportsDownloadURL", delegate
				{
					return new StringRegistryItem(
						"MyAccountReportsDownloadURL",
						(NoResString)(MyAccountPortalSubCategory + "/Report"),
						(NoResString)"Download URL of My Account Reports",
						(NoResString)"This is the download URL of My Account Reports.",
						RegistryStorageFlags.System,
						"https://myaccount-portal.cargowise.com/myaccount/Download.aspx?{0}");
				});
			}
		}

		public StringRegistryItem MyAccountReportsShareFolderPath
		{
			get
			{
				return GetItem("MyAccountReportsShareFolderPath", delegate
				{
					return new StringRegistryItem(
						"MyAccountReportsShareFolderPath",
						(NoResString)(MyAccountPortalSubCategory + "/Report"),
						(NoResString)"My Account Reports Share Folder Path",
						(NoResString)"This is the directory that My Account Reports will be stored in.",
						RegistryStorageFlags.System,
						""); // \\sydco-scfs-1.wtg.zone\WebContent\MyAccountReports\
				});
			}
		}

		public NotificationEmailTemplateRegistryItem MyAccountReportsDownloadNotificationEmailTemplate
		{
			get
			{
				return GetItem(
					"MyAccountReportsDownloadNotificationEmailTemplate",
					delegate
					{
						NotificationEmailTemplateRegistryItem registryItem = new NotificationEmailTemplateRegistryItem(
							"MyAccountReportsDownloadNotificationEmailTemplate",
							(NoResString)(MyAccountPortalSubCategory + "/Report"),
							(NoResString)"My Account Reports Download Notification Email Template",
							(NoResString)"This is the default template for My Account Reports download notification emails to client.",
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							typeof(Billing.Business.DocEdiReportingQueue),
							@"CargoWise My Account - Report Download Notification",
							@"The Report is ready for download.<br/>Download Link:<a href='{DOWNLOAD_LINK}'>{REPORT_NAME}</a>");
						return registryItem;
					});
			}
		}

		#endregion

		public MultilingualStringRegistryItem RegisterPersonalEmailPageFooterText
		{
			get
			{
				return GetItem("RegisterPersonalEmailPageFooterText", () =>
					new MultilingualStringRegistryItem(
							"RegisterPersonalEmailPageFooterText",
							(NoResString)(MyAccountPortalSubCategory + "/Report"),
							ResString.GetMultilingualString("2ef773c4-efdc-4770-b1e3-3cb3fdd40cc3", "Register Personal Email Page Footer Text"),
							ResString.GetMultilingualString("4ccc173b-37d7-44fd-b6f5-8fc859573302", "Specifies the footer text for Register Personal Email Page"),
							RegistryStorageFlags.System)
					{ EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo) });
			}
		}

		public MultilingualStringRegistryItem DistinctEmailRequiredPageFooterText
		{
			get
			{
				return GetItem("DistinctEmailRequiredPageFooterText", () =>
					new MultilingualStringRegistryItem(
							"DistinctEmailRequiredPageFooterText",
							(NoResString)(MyAccountPortalSubCategory + "/Report"),
							ResString.GetMultilingualString("0c473a61-a4d6-4508-a1a5-c918eb4a2758", "Distinct Email Required Page Footer Text"),
							ResString.GetMultilingualString("8a662352-e3b8-4232-8c0c-2f3724ba3d1e", "Specifies the footer text for Distinct Email Required Page"),
							RegistryStorageFlags.System)
					{ EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo) });
			}
		}

		#region URLs and Paths

		const string MyAccountPortalUrlSubCategory = MyAccountPortalSubCategory + "/URLs and Paths";

		public StringRegistryItem MyAccountSiteRootUrl
		{
			get
			{
				return GetItem("MyAccountSiteRootUrl", delegate
				{
					var item = new StringRegistryItem(
						"MyAccountSiteRootUrl",
						(NoResString)MyAccountPortalUrlSubCategory,
						(NoResString)"My Account Portal Root URL",
						(NoResString)"The base URI for the My Account portal to ediProd",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						"https://myaccount-portal.cargowise.com/myaccount");
					item.DataType = new UriRegistryDataType(Uri.UriSchemeHttps) { AllowAutoProtocolPrefixing = false };
					return item;
				});
			}
		}

		public StringRegistryItem MyAccountHostingSiteRootUrl
		{
			get
			{
				return GetItem("MyAccountHostingSiteRootUrl", delegate
				{
					return new StringRegistryItem(
						"MyAccountHostingSiteRootUrl",
						(NoResString)MyAccountPortalUrlSubCategory,
						(NoResString)"My Account Hosting Site URL",
						(NoResString)"This is the Hosting Site URL of My Account.",
						RegistryStorageFlags.System,
						"https://myaccount.cargowise.com/");
				});
			}
		}

		public StringRegistryItem MyAccountIndexPage
		{
			get
			{
				return GetItem("MyAccountIndexPage", delegate
				{
					return new StringRegistryItem(
						"MyAccountIndexPage",
						(NoResString)MyAccountPortalUrlSubCategory,
						(NoResString)"My Account Index Page",
						(NoResString)"This is the Index Page URL of My Account.",
						RegistryStorageFlags.System,
						"https://myaccount.cargowise.com/");
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "We need to do this as Redirect Setting on Server does not allow us to check virtual path and better it has a default value")]
		public StringRegistryItem MyAccountPhysicalServerPath
		{
			get
			{
				return GetItem("MyAccountPhysicalServerPath", delegate
				{
					return new StringRegistryItem(
						"MyAccountPhysicalServerPath",
						(NoResString)MyAccountPortalUrlSubCategory,
						(NoResString)"My Account Physical Server Path",
						(NoResString)"This is the Physical Server Path of My Account.",
						RegistryStorageFlags.System,
						@"C:\inetpub\wwwroot\ediWebProd\My-account\my-account");
				});
			}
		}

		public StringRegistryItem WiseTechAcademyAutoLoginUrl
		{
			get
			{
				return GetItem("WiseTechAcademyAutoLoginUrl", delegate
				{
					return new StringRegistryItem(
						"WiseTechAcademyAutoLoginUrl",
						(NoResString)MyAccountPortalUrlSubCategory,
						(NoResString)"WiseTechAcademy Auto Login URL",
						(NoResString)"This is the Auto Login URL of WiseTechAcademy.",
						RegistryStorageFlags.System,
						"");
				});
			}
		}

		public StringRegistryItem WiseTechAcademyTokenEndpointUrl
		{
			get
			{
				return GetItem("WiseTechAcademyTokenEndpointUrl", delegate
				{
					return new StringRegistryItem(
						"WiseTechAcademyTokenEndpointUrl",
						(NoResString)MyAccountPortalUrlSubCategory,
						(NoResString)"WiseTechAcademy Token URL",
						(NoResString)"This is the URL of the endpoint for getting a token for WiseTechAcademy Services.",
						RegistryStorageFlags.System,
						"");
				});
			}
		}

		public StringRegistryItem WiseTechAcademyDocumentUrlsEndpointUrl
		{
			get
			{
				return GetItem("WiseTechAcademyDocumentUrlsEndpointUrl", delegate
				{
					return new StringRegistryItem(
						"WiseTechAcademyDocumentUrlsEndpointUrl",
						(NoResString)MyAccountPortalUrlSubCategory,
						(NoResString)"WiseTechAcademy Document Urls Endpoint URL",
						(NoResString)"This is the URL of the endpoint for getting document URLs for a set of document IDs via WiseTechAcademy.",
						RegistryStorageFlags.System,
						"");
				});
			}
		}

		public StringRegistryItem MyAccountEndpointBaseUrl
		{
			get
			{
				return GetItem("MyAccountEndpointBaseUrl", delegate
				{
					return new StringRegistryItem(
						"MyAccountEndpointBaseUrl",
						(NoResString)MyAccountPortalUrlSubCategory,
						(NoResString)"MyAccount Endpoint Base Url",
						(NoResString)"MyAccount Endpoint Base Url",
						RegistryStorageFlags.System,
						"https://myaccount-portal.cargowise.com/myaccount/api/");
				});
			}
		}

		public MyAccountHostingSiteLandingPageUrlRegistryItem MyAccountHostingSiteLandingPageUrl
		{
			get
			{
				return GetItem("MyAccountHostingSiteLandingPageUrl", () =>
					new MyAccountHostingSiteLandingPageUrlRegistryItem(
						"MyAccountHostingSiteLandingPageUrl",
						(NoResString)MyAccountPortalUrlSubCategory,
						ResString.GetMultilingualString("51f4c37f-e7e7-4ded-89c2-3655d8cc8fc7", "My Account Hosting Site Landing Page URL"),
						ResString.GetMultilingualString("c24f658d-fe12-42f6-9657-d572eec93a6b", "Specifies a per product landing page for My Account, this page will be called after auto login"),
						RegistryStorageFlags.System,
						new MyAccountHostingSiteLandingPageUrlCollection()));
			}
		}

		#endregion

		#region Logging

		public StringRegistryItem MyAccountLoggerFileTargetPath
		{
			get
			{
				return GetItem("MyAccountLoggerFileTargetPath", () =>
					new StringRegistryItem(
						"MyAccountLoggerFileTargetPath",
						(NoResString)(MyAccountPortalSubCategory + "/Logging"),
						ResString.GetMultilingualString("f1f9fa06-98d8-473a-b034-4bdddd3df8a9", "My Account Logger File Target Path"),
						ResString.GetMultilingualString("0eb0b377-4cc5-473a-b3ba-068900390d24", "Specifies the file path (relative or absolute) as NLog file target config 'path' attribute for My Account."),
						RegistryStorageFlags.System)
				);
			}
		}

		public StringRegistryItem MyAccountLoggerKafkaTargetTopic
		{
			get
			{
				return GetItem("MyAccountLoggerKafkaTargetTopic", () =>
					new StringRegistryItem(
						"MyAccountLoggerKafkaTargetTopic",
						(NoResString)(MyAccountPortalSubCategory + "/Logging"),
						ResString.GetMultilingualString("3335e0f4-9f61-4fb6-b953-caf497688846", "My Account Logger Kafka Target Topic"),
						ResString.GetMultilingualString("1956ac72-9d2a-4e68-aa5b-c9bfc168703b", "Specifies the Kafka topic as NLog Kafka target config 'topic' attribute for My Account."),
						RegistryStorageFlags.System)
				);
			}
		}

		public StringRegistryItem MyAccountLoggerKafkaTargetBrokers
		{
			get
			{
				return GetItem("MyAccountLoggerKafkaTargetBrokers", () =>
					new StringRegistryItem(
						"MyAccountLoggerKafkaTargetBrokers",
						(NoResString)(MyAccountPortalSubCategory + "/Logging"),
						ResString.GetMultilingualString("dc180217-f133-43e0-9d94-32141d11008a", "My Account Logger Kafka Target Brokers"),
						ResString.GetMultilingualString("32a089fc-a732-4d65-84dc-4e79684553be", "Specifies the Kafka brokers as NLog Kafka target config 'brokers' attribute for My Account."),
						RegistryStorageFlags.System)
				);
			}
		}

		#endregion

		#region Trusted Messaging

		public IntRegistryItem MyAccountKeyRotationIntervalMinutes
		{
			get
			{
				return GetItem("MyAccountKeyRotationIntervalMinutes", delegate
				{
					return new IntRegistryItem(
						"MyAccountKeyRotationIntervalMinutes",
						(NoResString)(MyAccountPortalSubCategory + "/Trusted Messaging"),
						(NoResString)"My Account Key Rotation Interval Minutes",
						(NoResString)"This is the MyAccount key rotation interval specified in minutes.",
						RegistryStorageFlags.System,
						15);
				});
			}
		}

		public CodeDescriptionBoolRegistryItem MyAccountTrustedServices
		{
			get
			{
				return GetItem("MyAccountTrustedServices", delegate
				{
					return new CodeDescriptionBoolRegistryItem(
						"MyAccountTrustedServices",
						(NoResString)(MyAccountPortalSubCategory + "/Trusted Messaging"),
						(NoResString)"My Account Trusted Services",
						(NoResString)"Define standalone services for trusting messaging between My Account.",
						RegistryStorageFlags.System,
						(NoResString)"Active");
				});
			}
		}

		public BinaryRegistryItem InternalCW1ActivationCertificate
		{
			get
			{
				return GetItem("InternalCW1ActivationCertificate", delegate
				{
					var result = new BinaryRegistryItem
					(
						"InternalCW1ActivationCertificate",
						(NoResString)(MyAccountPortalSubCategory + "/Trusted Messaging"),
						(NoResString)"Internal CW1 Activation Certificate",
						(NoResString)"This the certificate with private key for internal CW1 trusted system activation.",
						RegistryStorageFlags.System,
						Array.Empty<byte>()
					);
					result.EditorInfo = new FileUpLoaderX509CertificateRegistryEditorInfo(InternalCW1ActivationCertificatePassword);
					return result;
				});
			}
		}

		public StringRegistryItem InternalCW1ActivationCertificatePassword
		{
			get
			{
				return GetItem("InternalCW1ActivationCertificatePassword", delegate
				{
					return new StringRegistryItem(
						"InternalCW1ActivationCertificatePassword",
						(NoResString)(MyAccountPortalSubCategory + "/Trusted Messaging"),
						(NoResString)"Internal CW1 Activation Certificate Password",
						(NoResString)"This the password of certificate with private key for internal CW1 trusted system activation.",
						new StringRegistryDataType(true),
						RegistryStorageFlags.System,
						(NoResString)string.Empty)
					{ EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password) };
				});
			}
		}

		#endregion

		public BooleanRegistryItem EnableMyAccountPersonalPasswordProtection
		{
			get
			{
				return GetItem("EnableMyAccountPersonalPasswordProtection", delegate
				{
					return new BooleanRegistryItem(
						"EnableMyAccountPersonalPasswordProtection",
						(NoResString)MyAccountPortalSubCategory,
						(NoResString)"Personal Password Protection Functionality",
						(NoResString)"Personal Password Protection Functionality",
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#region MyAccount WebAPI

		const string MyAccountProductRegistrationCategory = MyAccountPortalSubCategory + "/Product Registration";

		public GuidRegistryItem ProductRegistrationWebAPINotificationGroup
		{
			get
			{
				return GetItem("ProductRegistrationWebAPINotificationGroup", delegate
				{
					var result = new GuidRegistryItem(
						"ProductRegistrationWebAPINotificationGroup",
						(NoResString)MyAccountProductRegistrationCategory,
						(NoResString)"Product Registration WebAPI Notification Group",
						(NoResString)"Product Registration WebAPI Notification Group",
						RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public StringRegistryItem ProductRegistrationWebAPIDefaultEnterpriseID
		{
			get
			{
				return GetItem("ProductRegistrationWebAPIDefaultEnterpriseID", delegate
					{
						return new StringRegistryItem(
							"ProductRegistrationWebAPIDefaultEnterpriseID",
							(NoResString)MyAccountProductRegistrationCategory,
							(NoResString)"Product Registration WebAPI Default Enterprise ID",
							(NoResString)"Product Registration WebAPI Default Enterprise ID",
							RegistryStorageFlags.System);
					});
			}
		}

		public IntRegistryItem LicenceDatabaseMasterOrgSuggestionBulkUpdateThreshold
		{
			get
			{
				return GetItem("LicenceDatabaseMasterOrgSuggestionBulkUpdateThreshold", delegate
				{
					return new IntRegistryItem(
						"LicenceDatabaseMasterOrgSuggestionBulkUpdateThreshold",
						(NoResString)MyAccountProductRegistrationCategory,
						(NoResString)"Licence Database Master Organisation Suggestion Bulk Update Threshold",
						(NoResString)"This is the minimum value of Master Organisation Suggestion Ranking Score required for automatic matching.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						50, 5, 10000);
				});
			}
		}

		#region MyAccount CA

		public const string MyAccountCertificateAuthorityCategory = MyAccountPortalSubCategory + "/Certificate Authority";

		public StringRegistryItem MyAccountCertificateAuthorityUserAccount
		{
			get
			{
				return GetItem("MyAccountCertificateAuthorityUserAccount", delegate
					{
						return new StringRegistryItem(
							"MyAccountCertificateAuthorityUserAccount",
							(NoResString)MyAccountCertificateAuthorityCategory,
							(NoResString)"MyAccount Certificate Authority User Account",
							(NoResString)"MyAccount Certificate Authority User Account",
							RegistryStorageFlags.System, "PROD\\s_CertRequestor");
					});
			}
		}

		public StringRegistryItem MyAccountCertificateAuthorityUserAccountPassword
		{
			get
			{
				return GetItem("MyAccountCertificateAuthorityUserAccountPassword", delegate
				{
					return new StringRegistryItem(
						"MyAccountCertificateAuthorityUserAccountPassword",
						(NoResString)MyAccountCertificateAuthorityCategory,
						(NoResString)"MyAccount Certificate Authority User Account Password",
						(NoResString)"MyAccount Certificate Authority User Account Password",
						new StringRegistryDataType(true),
						RegistryStorageFlags.System, "")
					{ EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password) };
				});
			}
		}

		public StringRegistryItem MyAccountCertificateAuthoritySoapRequestTemplate
		{
			get
			{
				return GetItem("MyAccountCertificateAuthoritySoapRequestTemplate", delegate
					{
						return new StringRegistryItem(
							"MyAccountCertificateAuthoritySoapRequestTemplate",
							(NoResString)MyAccountCertificateAuthorityCategory,
							(NoResString)"MyAccount Certificate Authority Soap Request Template",
							(NoResString)"MyAccount Certificate Authority Soap Request Template",
							RegistryStorageFlags.System, "<s:Envelope xmlns:a=\"http://www.w3.org/2005/08/addressing\" xmlns:s=\"http://www.w3.org/2003/05/soap-envelope\"><s:Header><a:Action s:mustUnderstand=\"1\">http://schemas.microsoft.com/windows/pki/2009/01/enrollment/RST/wstep</a:Action><a:MessageID>urn:uuid:(*NEWID*)</a:MessageID><a:To s:mustUnderstand=\"1\">https://ca.wisecloud.zone/WiseCloudZone-Root-CA-1_CES_Kerberos/service.svc/ces</a:To></s:Header><s:Body><RequestSecurityToken PreferredLanguage=\"en-AU\" xmlns=\"http://docs.oasis-open.org/ws-sx/ws-trust/200512\"><TokenType>http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3</TokenType><RequestType>http://docs.oasis-open.org/ws-sx/ws-trust/200512/Issue</RequestType><BinarySecurityToken ValueType=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd#PKCS7\" EncodingType=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd#base64binary\" a:Id=\"\" xmlns:a=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" xmlns=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\">(*REQUEST*)</BinarySecurityToken><AdditionalContext xmlns=\"http://schemas.xmlsoap.org/ws/2006/12/authorization\"><ContextItem Name=\"CertificateTemplate\"><Value>WTGApps</Value></ContextItem><ContextItem Name=\"ccm\"><Value>ca.wisecloud.zone</Value></ContextItem></AdditionalContext></RequestSecurityToken></s:Body></s:Envelope>")
						{ EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo) };
					});
			}
		}

		#endregion

		#region MyAccount Gateway

		const string MyAccountGLOWGateway = MyAccountPortalSubCategory + "/GLOW My Account Gateway";

		public StringRegistryItem MyAccountGatewayRootUriKey
		{
			get
			{
				return GetItem(CargoWise.Definitions.MyAccountRegistry.MyAccountGatewayRootUriKey, delegate
				{
					var sri = new StringRegistryItem(
						CargoWise.Definitions.MyAccountRegistry.MyAccountGatewayRootUriKey,
						(NoResString)MyAccountGLOWGateway,
						(NoResString)"My Account Gateway Root URL",
						(NoResString)"The base URI for My Account Gateway Login",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.IsValueMandatory,
						"https://myaccount-portal.cargowise.com/myaccount/gateway");
					sri.DataType = new UriRegistryDataType(Uri.UriSchemeHttps) { AllowAutoProtocolPrefixing = false };
					return sri;
				});
			}
		}

		public StringRegistryItem MyAccountGatewaySecret
		{
			get
			{
				return GetItem(CargoWise.Definitions.MyAccountRegistry.MyAccountGatewaySecretKey, delegate
				{
					var result = new StringRegistryItem(
						CargoWise.Definitions.MyAccountRegistry.MyAccountGatewaySecretKey,
						(NoResString)MyAccountGLOWGateway,
						(NoResString)"My Account Gateway Logon Client Secret",
						(NoResString)"Key used for encryption of My Account Gateway Requests. The key should be 64 bytes long.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport
					);
					result.DataType = new BinaryKeyRegistryDataType(keySize: 64);
					return result;
				});
			}
		}

		#endregion

		#region MyAccount S2ST

		public CodeDescriptionPairListRegistryItem AzpsApprovedForMyAccount
		{
			get
			{
				return GetItem("AzpsApprovedForMyAccount", delegate
				{
					var result = new CodeDescriptionPairListRegistryItem(
						"AzpsApprovedForMyAccount",
						(NoResString)(MyAccountPortalSubCategory),
						(NoResString)"Authorized Parties Approved For Trusted Messaging using Azure B2C",
						(NoResString)"List of AZPs (Authorized Parties) which are approved for Trusted Messaging APIs using Azure B2C. Enter the azp (in GUID format xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx) and the product they are authorized for.",
						maxCodeLength: 36,
						storage: RegistryStorageFlags.System,
						isLocalizable: false,
						defaultValue: new CodeDescriptionPairList());

					var editorInfo = new CodeDescriptionPairListEditorInfo(showCodeColumn: true, showDescriptionColumn: true, codeFieldCasing: CodeDescriptionPairListEditorInfo.CharacterCasing.Lower, descriptionFieldCasing: CodeDescriptionPairListEditorInfo.CharacterCasing.Normal);
					editorInfo.SetCodeColumnCaption((NoResString)"AZP");
					editorInfo.SetDescriptionColumnCaption((NoResString)"Product");
					result.EditorInfo = editorInfo;
					return result;
				});
			}
		}

		public CodeSelectionCollectionRegistryItem TrustedMessagingUserAgreementCheckBypassProducts
		{
			get
			{
				return GetItem("TrustedMessagingUserAgreementCheckBypassProducts", delegate
				{
					return new CodeSelectionCollectionRegistryItem(
						"TrustedMessagingUserAgreementCheckBypassProducts",
						(NoResString)(MyAccountPortalSubCategory),
						(NoResString)"Trusted Messaging User Agreement Check Bypass",
						(NoResString)"List of products that will bypass user agreement in the Auto-Login API. WiseTech Global legal representatives are to confirm any products added to this list.",
						storage: RegistryStorageFlags.System,
						TrustedMessagingBypassProductListProvider);
				});
			}
		}

		CodeDescriptionPairListProvider TrustedMessagingBypassProductListProvider
		{
			get
			{
				return new CodeDescriptionPairListProvider(() => IncidentDetailsLookupsHelper.ProductList);
			}
		}

		#endregion

		#endregion

		#region Web Config

		public BooleanRegistryItem EnableMyAccountWebConfigOveridden
		{
			get
			{
				return GetItem("EnableMyAccountWebConfigOveridden", delegate
				{
					return new BooleanRegistryItem(
						"EnableMyAccountWebConfigOveridden",
						(NoResString)(MyAccountPortalSubCategory + "/Web Config"),
						(NoResString)"Enable My Account Web Config Overidden",
						(NoResString)"Allow My Account overwrites default web.config with registry item values.",
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		public StringRegistryItem MyAccountFormAuthenticationCookieDomain
		{
			get
			{
				return GetItem("MyAccountFormAuthenticationCookieDomain", delegate
				{
					return new StringRegistryItem(
						"MyAccountFormAuthenticationCookieDomain",
						(NoResString)(MyAccountPortalSubCategory + "/Web Config"),
						(NoResString)"My Account Auth Cookie Domain",
						(NoResString)"This overwrites the 'domain' attribute value on system.web/authentication/form of My Account web.config.",
						RegistryStorageFlags.System,
						".cargowise.com");
				});
			}
		}

		public StringRegistryItem MyAccountFormAuthenticationCookieSameSite
		{
			get
			{
				return GetItem("MyAccountFormAuthenticationCookieSameSite", delegate
				{
					return new StringRegistryItem(
						"MyAccountFormAuthenticationCookieSameSite",
						(NoResString)(MyAccountPortalSubCategory + "/Web Config"),
						(NoResString)"My Account Auth Cookie Same Site",
						(NoResString)"This overwrites the 'cookieSameSite' attribute value on system.web/authentication/form of My Account web.config.",
						RegistryStorageFlags.System,
						"None");
				});
			}
		}

		public StringRegistryItem MyAccountFormAuthenticationCookieSSL
		{
			get
			{
				return GetItem("MyAccountFormAuthenticationCookieSSL", delegate
				{
					return new StringRegistryItem(
						"MyAccountFormAuthenticationCookieSSL",
						(NoResString)(MyAccountPortalSubCategory + "/Web Config"),
						(NoResString)"My Account Auth Cookie SSL",
						(NoResString)"This overwrites the 'requireSSL' attribute value on system.web/authentication/form of My Account web.config.",
						RegistryStorageFlags.System,
						"True");
				});
			}
		}

		#endregion

		#region MyAccount IP Address Validation

		public const string MyAccountIPSubCategory = MyAccountPortalSubCategory + "/MyAccount IP Address Validation";

		public BooleanRegistryItem DisableMyAccountWhitelisting
		{
			get
			{
				return GetItem("DisableMyAccountWhitelisting", () =>
				{
					return new BooleanRegistryItem(
						"DisableMyAccountWhitelisting",
						(NoResString)MyAccountIPSubCategory,
						(NoResString)"Disable MyAccount IP address whitelisting",
						(NoResString)"Change to ‘Yes’ to allow all IP addresses to access the MyAccount API",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false
					);
				});
			}
		}

		#endregion

		#region MyAccount_Sigle Sign-On

		const string MyAccount_SignleSignOn = MyAccountPortalSubCategory + "/Single Sign-On";

		public BooleanRegistryItem EnableAcademyJWTAuthentication
		{
			get
			{
				return GetItem("EnableAcademyJWTAuthentication", delegate
				{
					return new BooleanRegistryItem(
						"EnableAcademyJWTAuthentication",
						(NoResString)MyAccount_SignleSignOn,
						(NoResString)"Enable Academy JWT Authentication",
						(NoResString)"The MyAccount will only post a JWT to academy if enable this registry.",
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		public BinaryRegistryItem MyAccountSSOJWTTokenExchangePrivateKey
		{
			get
			{
				return GetItem("MyAccountSSOJWTTokenExchangePrivateKey", delegate
				{
					var registry = new BinaryRegistryItem(
						"MyAccountSSOJWTTokenExchangePrivateKey",
						(NoResString)MyAccount_SignleSignOn,
						(NoResString)"JWT Token Exchange Private Key",
						(NoResString)"MyAccount will generate signature for JWT by this key. The key should be PKCS #1",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						Array.Empty<byte>())
					{
						EditorInfo = new CWSupportLoginTokenPrivateKeyEditorInfo()
					};

					return registry;
				});
			}
		}

		#endregion

		#region MyAccount_OpenID Connect

		const string MyAccount_OpenIDConnect = MyAccountPortalSubCategory + "/OpenID Connect";

		public StringArrayRegistryItem RedirectedEmailDomains
		{
			get
			{
				return GetItem("RedirectedEmailDomains", delegate
				{
					var registry = new StringArrayRegistryItem(
						"RedirectedEmailDomains",
						(NoResString)MyAccount_OpenIDConnect,
						(NoResString)"Redirected Email Domains",
						(NoResString)"When logging in, a user will be redirected to OIDC authentication if an Organisation Code has not been provided and the domain of their email address is in this list.",
						RegistryStorageFlags.System);
					registry.DataType.Validating -= ValidateRedirectedEmailDomains;
					registry.DataType.Validating += ValidateRedirectedEmailDomains;

					return registry;
				});
			}
		}

		void ValidateRedirectedEmailDomains(object sender, RegistryDataTypeValidatingEventArgs<string[]> e)
		{
			if (e.ProposedValue.IsNullOrEmpty())
			{
				return;
			}

			var expression = @"^@(((?!-)[a-zA-Z\d\-]+(?<!-)\.)+[a-zA-Z]{2,}" +
					@"|\[" +
					@"(((?(?<!\[)\.)(25[0-5]|2[0-4]\d|[01]?\d?\d)){4}" +
					@"|[a-zA-Z\d\-]*[a-zA-Z\d]:" +
					@"((?=[\x01-\x7f])[^\\\[\]]|\\[\x01-\x7f])+)" +
					@"\])" +
					@"(?(angle)>)$"; //Refer from EmailAddressValidation.IsEmailAddressValid
			var re = new Regex(expression);
			foreach (var emailAddress in e.ProposedValue)
			{
				if (!re.IsMatch(emailAddress))
				{
					throw new RegistryValidationException((NoResString)"Please enter valid email domain. E.g. @domain_name.com");
				}
			}
		}

		public GuidArrayRegistryItem RedirectedOrganisations
		{
			get
			{
				return GetItem("RedirectedOrganisations", delegate
				{
					var registry = new GuidArrayRegistryItem(
						"RedirectedOrganisations",
						(NoResString)MyAccount_OpenIDConnect,
						(NoResString)"Redirected Organisations",
						(NoResString)"When logging in, a user will be redirected to OIDC authentication if they provide an Organisation Code from the list below, even if their email domain does not match any Redirected Email Domains.",
						RegistryStorageFlags.System);
					registry.EditorInfo = new TextRegistryEditorInfo(TextEditorType.OrgHeaderCodeListEdit);
					(registry.DataType as GuidArrayRegistryDataType).Validating -= ValidateRedirectedOrganisations;
					(registry.DataType as GuidArrayRegistryDataType).Validating += ValidateRedirectedOrganisations;

					return registry;
				});
			}
		}

		void ValidateRedirectedOrganisations(object sender, RegistryDataTypeValidatingEventArgs<Guid[]> e)
		{
			if (e.ProposedValue.Distinct().Count() != e.ProposedValue.Length)
			{
				throw new RegistryValidationException((NoResString)"Please select unique organisations.");
			}
		}

		public StringRegistryItem MyAccountQueryOIDCClientID
		{
			get
			{
				return GetItem("MyAccountQueryOIDCClientID", delegate
				{
					var registry = new StringRegistryItem(
						"MyAccountQueryOIDCClientID",
						(NoResString)MyAccount_OpenIDConnect,
						(NoResString)"Client ID",
						(NoResString)string.Empty,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport);

					return registry;
				});
			}
		}

		public StringRegistryItem MyAccountOIDCUserDataHashIV
		{
			get
			{
				return GetItem("MyAccountOIDCUserDataHashIV", delegate
				{
					var registry = new StringRegistryItem(
						"MyAccountOIDCUserDataHashIV",
						(NoResString)MyAccount_OpenIDConnect,
						(NoResString)"User Data Hash Initialization Vector",
						(NoResString)"The value should be a valid GUID string.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: "d0d4936d-c4a5-4ecb-b79a-358a60a35955")
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password),
					};

					(registry.DataType as StringRegistryDataType).Validating -= ValidateUserDataHashIV;
					(registry.DataType as StringRegistryDataType).Validating += ValidateUserDataHashIV;

					return registry;
				});
			}
		}

		public Guid MyAccountOIDCUserDataHashIVGuid
		{
			get
			{
				if(Guid.TryParse(MyAccountOIDCUserDataHashIV.Value, out var result))
				{
					return result;
				}
				else
				{
					return Guid.Empty;
				}
			}
		}

		void ValidateUserDataHashIV(object sender, RegistryDataTypeValidatingEventArgs<string> e)
		{
			if(string.IsNullOrEmpty(e.ProposedValue) || !Guid.TryParse(e.ProposedValue, out _))
			{
				throw new RegistryValidationException((NoResString)"Please enter a valid GUID string");
			}
		}

		#endregion

		#endregion My Account Portal

		#region Pricelists

		public FaxPriceCollection FaxPrices
		{
			get { return FaxPriceRegistryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public FaxPriceRegistryItem FaxPriceRegistryItem
		{
			get
			{
				return GetItem("FaxPrice",
						delegate
						{ return new FaxPriceRegistryItem(PricelistSubCategory); });
			}
		}

		public int LicenceModuleViewMode
		{
			get { return LicenceModuleViewModeItem.GetValueWithoutFallback(Env.CurrentUser.PK, Guid.Empty, Guid.Empty); }
			set { LicenceModuleViewModeItem.SetValue(Env.CurrentUser.PK, Guid.Empty, Guid.Empty, value); }
		}

		public IntRegistryItem LicenceModuleViewModeItem
		{
			get
			{
				return GetItem("LicenceModuleViewMode", delegate
				{
					return new IntRegistryItem(
						"LicenceModuleViewMode",
						null,
						null,
						null,
						RegistryStorageFlags.Company,
							RegistryOptions.NotLogged | RegistryOptions.IsHidden,
						0);
				});
			}
		}

		#endregion

		public IntRegistryItem SplitterDistance
		{
			get
			{
				return GetItem("SplitterDistance", delegate
				{
					return new IntRegistryItem(
						"SplitterDistance",
						(NoResString)"Splitters",
						null,
						null,
						RegistryStorageFlags.Company,
							RegistryOptions.NotLogged | RegistryOptions.IsHidden,
						-1);
				});
			}
		}

		#region OpenID Connect

		public const string OpenIDConnectSubCategory = Category + "/OpenID Connect";

		public StringArrayRegistryItem WTGWiseCloudAccessSecurityGroup
		{
			get
			{
				return GetItem("WTGWiseCloudAccessSecurityGroup", delegate
				{
					var result = new StringArrayRegistryItem(
						"WTGWiseCloudAccessSecurityGroup",
						(NoResString)OpenIDConnectSubCategory,
						(NoResString)"WiseCloud Access Security Group for WTG",
						(NoResString)"Newly created Active Directory users starting with 'WTG.' will be placed into the following Active Directory groups. Existing users will not be affected.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport);
					return result;
				});
			}
		}

		public WTGActiveDirectoryCredentialsRegistryItem WTGActiveDirectoryCredentials
		{
			get
			{
				return GetItem("WTGActiveDirectoryCredentials", delegate
				{
					return new WTGActiveDirectoryCredentialsRegistryItem(
						"WTGActiveDirectoryCredentials",
						(NoResString)OpenIDConnectSubCategory,
						(NoResString)"WTG Active Directory Credentials",
						(NoResString)"The Active Directory credentials needed for managing the WTG. Active Directory accounts used for token-based authentication.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						ZClientEDI.Business.WTGActiveDirectoryCredentials.DefaultValue);
				});
			}
		}

		public GuidRegistryItem PasswordAndSignatureAlwaysVisibleToGroup
		{
			get
			{
				return GetItem("PasswordAndSignatureAlwaysVisibleToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"PasswordAndSignatureAlwaysVisibleToGroup",
						(NoResString)OpenIDConnectSubCategory,
						(NoResString)"Password & Signature Always Visible To Group",
						(NoResString)"This registry setting is used to specify the Active Directory group name in ediProd which will bypass the logic that hides the Password and Signature tab on the Edit Staff form when OIDC is enabled.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						RegistryFactory.Instance.GetGroupPK("HRGROUPUSER"));
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Last Successful Sync For EDI UTC

		public DateTimeRegistryItem LastSuccessfulSyncForEDIUTC
		{
			get
			{
				return GetItem("LastSuccessfulSyncForEDIUTC", () =>
				{
					return new DateTimeRegistryItem(
						name: "LastSuccessfulSyncForEDIUTC",
						category: null,
						caption: null,
						hint: null,
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue,
						defaultValue: SqlDateTime.MinValue.Value
					);
				});
			}
		}

		#endregion

		#region Hosting

		public CodeDescriptionBoolRegistryItem DatabaseHostedLocations
		{
			get
			{
				return GetItem("DatabaseBillingHostedLocations", delegate
				{
					return new CodeDescriptionBoolRegistryItem(
						"DatabaseBillingHostedLocations",
						(NoResString)Category,
						(NoResString)"Database Hosted Locations",
						(NoResString)"Defines database hosting locations and if they are owned by CargoWise",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						new CodeDescriptionBoolRegistryEditorInfo((NoResString)"CargoWise", true, false),
						new CodeDescriptionBoolCollection {
							{ Enterprise.Core.Constants.LicenceConstants.NotHostedWithCargoWise, (NoResString)"Not Hosted With CargoWise", false },
							{ "SYD", (NoResString)"Sydney", true },
							{ "CHI", (NoResString)"Chicago", true },
							{ "LON", (NoResString)"London", true },
							{ "TRA", (NoResString)"Trans-Soft", false },
						});
				});
			}
		}

		#endregion

		#region Work Item

		public const string WorkItemSubCategory = Category + "/Work Item";

		public StringRegistryItem WorkItemDefaultFixPriority
		{
			get
			{
				return GetItem(
					"WorkItemDefaultFixPriority",
					delegate
					{
						StringRegistryItem registryItem = new StringRegistryItem(
							"WorkItemDefaultFixPriority",
							(NoResString)WorkItemSubCategory,
							(NoResString)"Default Defect Priority",
							(NoResString)"Default Priority for Work Items with a Change Type of the configured defect types",
							RegistryStorageFlags.System,
							ReleaseRings.Codes.GPR);
						return registryItem;
					});
			}
		}

		public CodeDescriptionPairListRegistryItem CodingTasks
		{
			get
			{
				var defaultList = new CodeDescriptionPairList();
				defaultList.AddPair("CDU");
				defaultList.AddPair("CDF");
				defaultList.AddPair("COD");
				defaultList.AddPair("RCD");

				return GetItem("CodingTasks", delegate
				{
					var result = new CodeDescriptionPairListRegistryItem(
						"CodingTasks",
						(NoResString)WorkItemSubCategory,
						(NoResString)"Coding Tasks",
						(NoResString)"Task types appearing in this list are considered coding task types.",
						maxCodeLength: 3,
						storage: RegistryStorageFlags.System,
						isLocalizable: false,
						defaultValue: defaultList);

					result.EditorInfo = new CodeDescriptionPairListEditorInfo(showCodeColumn: true, showDescriptionColumn: false, codeFieldCasing: CodeDescriptionPairListEditorInfo.CharacterCasing.Upper, descriptionFieldCasing: CodeDescriptionPairListEditorInfo.CharacterCasing.Normal);

					return result;
				});
			}
		}

		public CodeDescriptionPairListRegistryItem TasksMandatoryInShelfCreation
		{
			get
			{
				return GetItem("TasksMandatoryInShelfCreation", delegate
				{
					CodeDescriptionPairList defaultList = new CodeDescriptionPairList();

					CodeDescriptionPairListRegistryItem result = new CodeDescriptionPairListRegistryItem(
						"TasksMandatoryInShelfCreation",
						(NoResString)WorkItemSubCategory,
						(NoResString)"Tasks For Shelf Creation",
						(NoResString)"Note, these tasks must be closed/cancelled to be able to queue a shelf for check-in.",
							3,
							RegistryStorageFlags.System,
							false,
							defaultList);

					result.EditorInfo = new CodeDescriptionPairListEditorInfo(true, false,
						CodeDescriptionPairListEditorInfo.CharacterCasing.Upper, CodeDescriptionPairListEditorInfo.CharacterCasing.Normal);

					return result;
				});
			}
		}

		public CodeDescriptionBoolRegistryItem ReviewTasks
		{
			get
			{
				return GetItem("ReviewTasks", delegate
				{
					var defaultList = new CodeDescriptionBoolCollection(3);

					var editorInfo = new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Active", true, false);

					var result = new CodeDescriptionBoolRegistryItem(
						"ReviewTasks",
						(NoResString)WorkItemSubCategory,
						(NoResString)"Review Tasks",
						(NoResString)"Process tasks considered as review.",
							RegistryStorageFlags.System,
							editorInfo,
							defaultList);

					return result;
				});
			}
		}

		public CodePairRegistryItem CompetencyLearningTask
		{
			get
			{
				return GetItem("CompetencyLearningTask", delegate
				{
					var lookupListProvider = new CodeDescriptionPairListProvider(() => WorkflowDataRegistryHelper.GetTaskTypeList(WorkflowDescriptors.WorkItemWorkflowDescriptorCode));

					var result = new CodePairRegistryItem(
						"CompetencyLearningTask",
						(NoResString)WorkItemSubCategory,
						(NoResString)"ASSESS Learning Task",
						(NoResString)"Choose a Work item task type that should be set when creating an ASSESS task for code authors.",
						lookupListProvider,
						RegistryStorageFlags.System,
						"UDF");

					return result;
				});
			}
		}

		public BooleanRegistryItem RequireCompletedCodeReviewForCheckin
		{
			get
			{
				return GetItem(
					"RequireCompletedCodeReviewForCheckin",
					() => new BooleanRegistryItem(
						name: "RequireCompletedCodeReviewForCheckin",
						category: (NoResString)WorkItemSubCategory,
						caption: (NoResString)"Require completed code review for checkin",
						hint: (NoResString)"When enabled, there must be at least one closed code review task (type CBC or CBA) to be able to submit a checkin to DAT with a CH0 task. The review task(s) must be lower in sequence in the same workflow as the CH0 task, or be in a prerequisite workflow within the same work item. If there are multiple such review tasks, all must be closed or cancelled.",
						storage: RegistryStorageFlags.System,
						defaultValue: true));
			}
		}

		#endregion

		#region Monitoring Windows Event Logs

		public const string MonitoringWindowsEventLogsSubCategory = Category + "/Monitoring Windows Event Logs";

		public HostNameStringListRegistryItem MachineHostNameList
		{
			get
			{
				return GetItem("MachineHostNameList", delegate
				{
					return new HostNameStringListRegistryItem("MachineHostNameList",
						(NoResString)MonitoringWindowsEventLogsSubCategory,
						ResString.GetMultilingualString("83b2a45c-70ab-4c0f-9a65-40117acb6dc1", "List of Monitoring Machines' Host name"),
						ResString.GetMultilingualString("3ef3869d-722c-4b91-85c8-c0f67ff44ea0", "Please separate each host name by ','."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						"");
				});
			}
		}

		public StringRegistryItem EventLogHighWaterMarkList
		{
			get
			{
				return GetItem("EventLogHighWaterMarkList", delegate
				{
					return new StringRegistryItem("EventLogHighWaterMarkList",
						(NoResString)MonitoringWindowsEventLogsSubCategory,
						(NoResString)"Last Log High-Water Mark Of Machine HostName List",
						(NoResString)"Store the last event log of each hostname in hostname list",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.NotCached,
						"")
					{
						IsExcludedFromCwOnlyNonCachedTest = true
					};
				});
			}
		}

		public EventLogsListProvidersAndTheirCapacityRegistryItem ListProvidersAndTheirCapacity
		{
			get
			{
				return GetItem("EventLogsListProvidersAndTheirCapacity", delegate
				{
					return new EventLogsListProvidersAndTheirCapacityRegistryItem("EventLogsListProvidersAndTheirCapacity",
						(NoResString)MonitoringWindowsEventLogsSubCategory,
						ResString.GetMultilingualString("4431d2c9-a232-4ef1-a348-5c9b43d233fc", "Listing providers and their associated capability"),
						ResString.GetMultilingualString("265343c0-9593-4326-a188-52651b07f0c5", "Listing providers and their associated capability. This will allow information services to take the majority(uncategorized windows event log should go to a blank item in the registry)."));
				});
			}
		}

		public CodePairRegistryItem EvenLogTraceLevel
		{
			get
			{
				return GetItem("EvenLogTraceLevel", delegate
				{
					var lookUpListProvider = new CodeDescriptionPairListProvider(() =>
					{
						var lookUpList = new CodeDescriptionPairList();
						lookUpList.AddPair(EvenLogTraceLevelOptions.All, ResString.GetMultilingualString("5223e43d-448d-4ef2-ad5e-553279e1ec6c", "Trace all event logs."));
						lookUpList.AddPair(EvenLogTraceLevelOptions.Medium, ResString.GetMultilingualString("ebf30883-6b8f-4581-b19e-c8d1bdca9365", "Trace English meaningful event logs and having call-stack event logs"));
						lookUpList.AddPair(EvenLogTraceLevelOptions.CallStackOnly, ResString.GetMultilingualString("95a37dc8-1f22-4902-b95f-81d094251c26", "Trace only event logs having call-stack."));
						return lookUpList;
					});

					return new CodePairRegistryItem("EvenLogTraceLevel",
						(NoResString)MonitoringWindowsEventLogsSubCategory,
						ResString.GetMultilingualString("0042ad6f-5033-4907-a2e5-4c421a255b6f", "Trace Level for ELS Service."),
						ResString.GetMultilingualString("ba1ffb1c-846c-4cab-a558-9a017f923c18", "Trace Level for retrieving event logs. There are three level: All, Medium, Call-stack only."),
						lookUpListProvider,
						RegistryStorageFlags.System,
						EvenLogTraceLevelOptions.Medium);
				});
			}
		}

		public static class EvenLogTraceLevelOptions
		{
			public const string All = "All";
			public const string Medium = "Medium";
			public const string CallStackOnly = "Call Stack only";
		}

		#endregion

		#region Scavenging Purge

		public ScavengingPurgeSettingsRegistryItem ScavengingPurgeSettings
		{
			get { return GetItem("ScavengingPurgeSettings", () => new ScavengingPurgeSettingsRegistryItem(Category)); }
		}

		#endregion

		#region Telematics Devices

		public const string TelematicsCategory = Category + "/Telematics";

		public const string TelematicsRimCategory = TelematicsCategory + "/Rim";

		public CodeDescriptionPairListRegistryItem TelematicsDeviceComponents
		{
			get
			{
				return GetItem("TelematicsDeviceComponents", () =>
				{
					var defaultList = new CodeDescriptionPairList();
					defaultList.AddPair("DVC", "3rd Party Device (eg iPad, Motorola Device, Android, Tablet)");
					defaultList.AddPair("BAT", "Battery");
					defaultList.AddPair("CAS", "Case");
					defaultList.AddPair("ANT", "Antenna");
					defaultList.AddPair("STP", "Strap");
					defaultList.AddPair("MNT", "Mount");
					defaultList.AddPair("MDM", "Modem");
					defaultList.AddPair("GPS", "GPS");
					defaultList.AddPair("SIM", "SIM Card");
					defaultList.AddPair("TPM", "TPM Sensor");
					defaultList.AddPair("TES", "Temperature Sensor");
					defaultList.Sort();
					defaultList.AddPair("OTH", "Other - You can add more types in the registery under <" + Category + " -> Telematics Device Component Types>");

					var result = new CodeDescriptionPairListRegistryItem("TelematicsDeviceComponents",
						(NoResString)Category,
						(NoResString)"Telematics Device Component Types",
						(NoResString)"The list of components that can be specified when creating device models and devices.",
						3,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						false,
						defaultList);

					return result;
				});
			}
		}

		public static class ClientDeviceComponentIdentificationTypes
		{
			public static CodeDescriptionPair IPAddress { get; } = new CodeDescriptionPair("IPA", "IP Address");
			public static CodeDescriptionPair InstallDate { get; } = new CodeDescriptionPair("IND", "Install Date");
			public static CodeDescriptionPair SerialNumber { get; } = new CodeDescriptionPair("SN", "Serial Number");
			public static CodeDescriptionPair OtherIdentifier { get; } = new CodeDescriptionPair("OTH", "Other Identifier");
			public static CodeDescriptionPair ManufactureDate { get; } = new CodeDescriptionPair("MFD", "Manufacture Date");
			public static CodeDescriptionPair InstalledLocation { get; } = new CodeDescriptionPair("INL", "Installed Location");
			public static CodeDescriptionPair SIMUniqueSerialNumber { get; } = new CodeDescriptionPair("ICC", "SIM Unique Serial Number (ICCID)");
			public static CodeDescriptionPair InternationalMobileStationEquipmentIdentity { get; } = new CodeDescriptionPair("IME", "International Mobile Station Equipment Identity (IMEI)");
		}

		public CodeDescriptionPairListRegistryItem TelematicsDevicesComponentIdentifications
		{
			get
			{
				return GetItem("TelematicsDevicesComponentIdentifications", () =>
				{
					var defaultList = new CodeDescriptionPairList
					{
						ClientDeviceComponentIdentificationTypes.IPAddress,
						ClientDeviceComponentIdentificationTypes.InstallDate,
						ClientDeviceComponentIdentificationTypes.SerialNumber,
						ClientDeviceComponentIdentificationTypes.OtherIdentifier,
						ClientDeviceComponentIdentificationTypes.ManufactureDate,
						ClientDeviceComponentIdentificationTypes.InstalledLocation,
						ClientDeviceComponentIdentificationTypes.SIMUniqueSerialNumber,
						ClientDeviceComponentIdentificationTypes.InternationalMobileStationEquipmentIdentity
					};

					defaultList.Sort();

					var result = new CodeDescriptionPairListRegistryItem("TelematicsDevicesComponentIdentifications",
						(NoResString)Category,
						(NoResString)"Telematics Device Component Identification Types",
						(NoResString)"The list of component identifications that can be specified when creating device models and devices.",
						3,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						false,
						defaultList);

					return result;
				});
			}
		}

		public StringRegistryItem MobileServicesEHubClientID
		{
			get
			{
				return GetItem("MobileServicesEHubClientID", () =>
				{
					var result = new StringRegistryItem("MobileServicesEHubClientID",
						(NoResString)Category,
						(NoResString)"MobileServices Client ID",
						(NoResString)"The eHub Client ID of the MobileServices system. This can be changed if there is a need to connect to another MobileServices system such as a UAT system.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						"GLOWMOB");
					return result;
				});
			}
		}

		public CodeDescriptionBoolRegistryItem ActiveMiddlewareService
		{
			get
			{
				return GetItem("ActiveMiddlewareService", () => new CodeDescriptionBoolRegistryItem(
					nameof(ActiveMiddlewareService),
					(NoResString)TelematicsCategory,
					(NoResString)"Active Middle-ware Services",
					(NoResString)"Active and inactive Middle-ware services. Any services that are marked as inactive will not receive device registrations",
					RegistryStorageFlags.System,
					RegistryOptions.MustOverrideDefaultValue,
					new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Enabled"),
					new CodeDescriptionBoolCollection()));
			}
		}

		public TcaRimEnrollmentSchemeRegistryItem TcaRimEnrollmentSchemeDescriptions
		{
			get
			{
				var defaultList = new TcaRimEnrollmentSchemeCollection();
				defaultList.Add("OSOM", (NoResString)"Oversize Overmass Vehicle Movement Scheme", true);
				defaultList.Add("SPECTS", (NoResString)"Safety Productivity Construction And Environment Transport Scheme", true);
				defaultList.Add("RIMSTFGA", (NoResString)"Semi-Trailer Farm Gate Access Schema", true);

				return GetItem(nameof(TcaRimEnrollmentSchemeDescriptions), () => new TcaRimEnrollmentSchemeRegistryItem(
					nameof(TcaRimEnrollmentSchemeDescriptions),
					(NoResString)TelematicsCategory,
					(NoResString)"Rim Enrollment Scheme Descriptions",
					(NoResString)"Description for the scheme's that a vehicle can be enrolled in for the TCA RIM program. See https://tca.gov.au/national-telematics-framework/schemes/road-infrastructure-management-schemes/ for information on these Schemes",
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					32,
					new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Active"),
					defaultList));
			}
		}

		#endregion

		#region Client Intelligence Customizable Labels

		static MultilingualString ClientIntelligence_CustomizableLabels { get { return CombineCategories(OrganisationsDataRegistry.Categories.SalesMarketing_ClientIntelligence, ResString.GetMultilingualString("028B8AFE-1091-470B-8F0B-EF5169DE70F9", "Customizable Labels")); } }

		public MultilingualStringRegistryItem AchievableBusinessLabel
		{
			get
			{
				return GetItem("AchievableBusinessLabel", delegate
				{
					return new MultilingualStringRegistryItem(
						"AchievableBusinessLabel",
						ClientIntelligence_CustomizableLabels,
						ResString.GetMultilingualString("2B9AE5B5-A48A-463D-A7B7-1CBBC18F6DE5", "Achievable Business Label"),
						ResString.GetMultilingualString("31E9E8EF-251D-46FE-90F9-06570F18072D", "Allows you to further categorize your organizations, by a Achievable Business identifier with customizable label."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("E47FC52B-0089-469D-826F-0B67796EF435", "Achievable Business"));
				});
			}
		}

		public MultilingualStringRegistryItem ConsultingRevenueLabel
		{
			get
			{
				return GetItem("ConsultingRevenueLabel", delegate
				{
					return new MultilingualStringRegistryItem(
						"ConsultingRevenueLabel",
						ClientIntelligence_CustomizableLabels,
						ResString.GetMultilingualString("DA139805-8D32-4881-9AF9-2CA827016D60", "Consulting Revenue Label"),
						ResString.GetMultilingualString("173F89F3-DD7F-4B0E-8409-BB7902D718AE", "Allows you to further categorize your organizations, by a Consulting Revenue identifier with customizable label."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("61D2AEAE-C5D5-44A1-A523-317AEEFC0FBD", "Consulting Revenue"));
				});
			}
		}

		public MultilingualStringRegistryItem NumberOfEmployeesLabel
		{
			get
			{
				return GetItem("NumberOfEmployeesLabel", delegate
				{
					return new MultilingualStringRegistryItem(
						"NumberOfEmployeesLabel",
						ClientIntelligence_CustomizableLabels,
						ResString.GetMultilingualString("0A5A5E94-5FB2-492E-8755-4208AC1F3C0E", "Number of Employees Label"),
						ResString.GetMultilingualString("633C5134-F153-45D9-8FF9-1441E79A0CEE", "Allows you to further categorize your organizations, by a Number Of Employees identifier with customizable label."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("ADB18AB5-B702-4608-BC28-819BB033A643", "No. of Employees"));
				});
			}
		}

		public MultilingualStringRegistryItem PaidUpCapitalLabel
		{
			get
			{
				return GetItem("PaidUpCapitalLabel", delegate
				{
					return new MultilingualStringRegistryItem(
						"PaidUpCapitalLabel",
						ClientIntelligence_CustomizableLabels,
						ResString.GetMultilingualString("59BE9737-40E0-4A4D-91B0-D7D3AF7E85BE", "Paid In Capital Label"),
						ResString.GetMultilingualString("E64A1BD1-EED7-4644-A8C3-580D722CFC15", "Allows you to further categorize your organizations, by a Paid In Capital identifier with customizable label."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("ACF8B336-0B02-48AD-AEB5-F9735F00B972", "Paid In Capital"));
				});
			}
		}

		public MultilingualStringRegistryItem AmountOfBusinessWonLabel
		{
			get
			{
				return GetItem("AmountOfBusinessWonLabel", delegate
				{
					return new MultilingualStringRegistryItem(
						"AmountOfBusinessWonLabel",
						ClientIntelligence_CustomizableLabels,
						ResString.GetMultilingualString("A6849356-DFD7-4289-B75F-71F1FF3B57BF", "Percentage Won Label"),
						ResString.GetMultilingualString("0DBE7F1E-52E4-4327-A211-EA2EE3402382", "Allows you to further categorize your organizations, by a Amount Of Business Won identifier with customizable label."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("956A265B-D153-4A68-BF20-454167A60B3C", "Percentage Won"));
				});
			}
		}

		public MultilingualStringRegistryItem TotalClientRevenueLabel
		{
			get
			{
				return GetItem("TotalClientRevenueLabel", delegate
				{
					return new MultilingualStringRegistryItem(
						"TotalClientRevenueLabel",
						ClientIntelligence_CustomizableLabels,
						ResString.GetMultilingualString("3630A9C2-5B2A-4CE3-822B-4E5BB9F9135A", "Total Client Revenue Label"),
						ResString.GetMultilingualString("DA9232B4-EEBD-4B7F-9BEF-4C708ADB1F5E", "Allows you to further categorize your organizations, by a Total Client Revenue identifier with customizable label."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("8D900048-D006-4268-BBB2-BA0A1D1CE45A", "Total Client Revenue"));
				});
			}
		}

		public MultilingualStringRegistryItem WarehouseRevenueLabel
		{
			get
			{
				return GetItem("WarehouseRevenueLabel", delegate
				{
					return new MultilingualStringRegistryItem(
						"WarehouseRevenueLabel",
						ClientIntelligence_CustomizableLabels,
						ResString.GetMultilingualString("9165B29D-F4C2-4A79-AE31-E4672503509C", "Warehouse Revenue Label"),
						ResString.GetMultilingualString("2F250366-289E-40A0-8842-0CCC6F40B010", "Allows you to further categorize your organizations, by a Warehouse Revenue identifier with customizable label."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("B4284CB5-C4A6-4FB6-9C56-E7F4AD0F74CA", "Warehouse Revenue"));
				});
			}
		}

		#endregion

		#region Address Validation

		internal StringArrayRegistryItem AvsMonitoringEndpointsItem
		{
			get
			{
				return GetItem("AvsMonitoringEndpoints", () =>
				{
					return new StringArrayRegistryItem(
						"AvsMonitoringEndpoints",
						OrganisationsDataRegistry.Categories.Organizations_AddressValidationService,
						(NoResString)"Monitoring Endpoints",
						(NoResString)"Define the endpoints for AVS monitoring service task (AXM).",
						RegistryStorageFlags.System);
				});
			}
		}

		[SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Requirement of current underlying Registry Infrastructure.")]
		public string[] AvsMonitoringEndpoints
		{
			get => AvsMonitoringEndpointsItem.Value;
			set => AvsMonitoringEndpointsItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		internal GuidRegistryItem AvsMonitoringNotificationGroupItem
		{
			get
			{
				return GetItem("AvsMonitoringNotificationGroup", () =>
				{
					var item = new GuidRegistryItem(
						"AvsMonitoringNotificationGroup",
						OrganisationsDataRegistry.Categories.Organizations_AddressValidationService,
						(NoResString)"Monitoring Notification Group",
						(NoResString)"Define the notification group for AVS monitoring service task (AXM).",
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						Guid.Empty);

					return item;
				});
			}
		}

		public Guid AvsMonitoringNotificationGroup
		{
			get => AvsMonitoringNotificationGroupItem.Value;
			set => AvsMonitoringNotificationGroupItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		#endregion

		#region Clients Upgrage Stategy

		public const string ClientsUprgadeStrategyCategory = ReleaseBuildsAndUpgradesSubCategory + "/Clients Upgrade Strategy";

		public BooleanRegistryItem EnableWeeklyBuildCutOff
		{
			get
			{
				var hint =
					"If enabled all WTG-hosted clients get the same version during a regular weekly upgrade. " +
					"This version is the first one generated after a specified day and time (see the other two options in the category).\n\n" +
					"If disabled the clients get the latest version.\n\n" +
					"The parameter does not affect self-hosted clients (they always get the latest version).";

				return GetItem("EnableWeeklyBuildCutOff",
					() =>
					{
						return new BooleanRegistryItem(
							"EnableWeeklyBuildCutOff",
							(NoResString)ClientsUprgadeStrategyCategory,
							(NoResString)"Enable weekly build cut-off",
							(NoResString)hint,
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							false
							);
					});
			}
		}

		public CodePairRegistryItem WeeklyBuildCutOffDay
		{
			get
			{
				var daysOfWeekListProvider = new CodeDescriptionPairListProvider(() =>
				{
					var daysOfWeek = new CodeDescriptionPairList();

					foreach (var d in Enum.GetValues(typeof(DayOfWeek)))
					{
						daysOfWeek.AddPair(d.ToString());
					}
					return daysOfWeek;
				});

				return GetItem("WeeklyBuildCutOffDay", () =>
				{
					return new CodePairRegistryItem(
						"WeeklyBuildCutOffDay",
						(NoResString)ClientsUprgadeStrategyCategory,
						(NoResString)"Weekly build cut-off day",
						(NoResString)"Choose a day of weekly build cut-off",
						daysOfWeekListProvider,
						false,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						"Friday"
						);
				});
			}
		}

		public StringRegistryItem WeeklyBuildCutOffTime
		{
			get
			{
				return GetItem(
					"WeeklyBuildCutOffTime",
					() =>
					{
						var registryItem = new StringRegistryItem(
							"WeeklyBuildCutOffTime",
							(NoResString)ClientsUprgadeStrategyCategory,
							(NoResString)"Weekly build cut-off time",
							(NoResString)"Enter a time (Sydney) of weekly build cut-off (e.g. 23:45:00 or 9:08:57 AM)",
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							"11:59:59 PM");

						registryItem.DataType = new TimeRegistryDataType();

						return registryItem;
					});
			}
		}

		#endregion

		#region BorderWise

		#region User Management Portal

		public MultilingualString BorderWiseUmpCategory => MultilingualString.Join("/", (NoResString)BorderWiseSubCategory, (NoResString)"User Management Portal");

		public BooleanRegistryItem BorderWiseUmpSyncEnabled =>
			GetItem("BorderWise_SyncEnabled",
				() => new BooleanRegistryItem(
					"BorderWise_SyncEnabled",
					BorderWiseUmpCategory,
					(NoResString)"Enable Sync with BorderWise UMP",
					(NoResString)"Enable synchronization of organization and contact details with BorderWise User Management Portal.",
					RegistryStorageFlags.System,
					false));

		public StringRegistryItem BorderWiseUmpSyncServers =>
			GetItem("BorderWise_SyncBootstrapServers",
				() => new StringRegistryItem(
					"BorderWise_SyncBootstrapServers",
					BorderWiseUmpCategory,
					(NoResString)"Sync Server",
					(NoResString)"Synchronization server(s) address.",
					RegistryStorageFlags.System,
					""));

		public StringRegistryItem BorderWiseUmpSyncOutboundTopic =>
			GetItem("BorderWise_SyncTopic",
				() => new StringRegistryItem(
					"BorderWise_SyncTopic",
					BorderWiseUmpCategory,
					(NoResString)"Outbound Sync Topic",
					(NoResString)"Topic for outbound synchronization messages.",
					RegistryStorageFlags.System,
					""));

		public StringRegistryItem BorderWiseUmpSyncInboundTopic =>
			GetItem("BorderWise_SyncTopic_Inbound",
				() => new StringRegistryItem(
					"BorderWise_SyncTopic_Inbound",
					BorderWiseUmpCategory,
					(NoResString)"Inbound Sync Topic",
					(NoResString)"Topic for inbound synchronization messages.",
					RegistryStorageFlags.System,
					""));

		public StringRegistryItem BorderWiseUmpSyncUserName =>
			GetItem("BorderWise_SyncUserName",
				() => new StringRegistryItem(
					"BorderWise_SyncUserName",
					BorderWiseUmpCategory,
					(NoResString)"Sync User Name",
					(NoResString)"Sync user name",
					RegistryStorageFlags.System,
					""));

		public StringRegistryItem BorderWiseUmpSyncPassword =>
			GetItem("BorderWise_SyncPassword",
				() => new StringRegistryItem(
					"BorderWise_SyncPassword",
					BorderWiseUmpCategory,
					(NoResString)"Sync Password",
					(NoResString)"Sync password",
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					defaultValue: string.Empty)
				{
					EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password)
				});

		public IntRegistryItem BorderWiseUmpSyncSessionTimeout =>
			GetItem("BorderWise_SyncSessionTimeout",
				() => new IntRegistryItem(
					"BorderWise_SyncSessionTimeout",
					BorderWiseUmpCategory,
					(NoResString)"Sync Session Timeout",
					(NoResString)"Timeout in milliseconds when connecting to synchronization server and waiting for inbound messages.",
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					10_000, 1_000, 120_000));

		public BooleanRegistryItem BorderWiseUmpApiEnabled =>
			GetItem("BorderWise_UmpApiEnabled",
				() => new BooleanRegistryItem(
					"BorderWise_UmpApiEnabled",
					BorderWiseUmpCategory,
					(NoResString)"Enable BorderWise UMP API",
					(NoResString)"Enable BorderWise User Management Portal API and use it for authentication and licensing from BorderWise applications.",
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					true));

		public StringRegistryItem BorderWiseUmpApiAddress =>
			GetItem("BorderWise_UmpApiAddress",
				() => new StringRegistryItem(
					"BorderWise_UmpApiAddress",
					BorderWiseUmpCategory,
					(NoResString)"UMP API Address",
					(NoResString)"UMP API base url",
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					"https://ump.borderwise.com/api"));

		public StringRegistryItem BorderWiseUmpApiUpdatePasswordApiKey =>
			GetItem("BorderWise_UmpApiUpdatePasswordApiKey",
				() => new StringRegistryItem(
					"BorderWise_UmpApiUpdatePasswordApiKey",
					BorderWiseUmpCategory,
					(NoResString)"UMP API Update Password Api Key",
					(NoResString)"The api key is required for calling api to update password.",
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					defaultValue: string.Empty)
				{
					EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password)
				});

		public StringRegistryItem BorderWiseUmpDatabaseConnectionString =>
			GetItem("BorderWiseUmpDatabaseConnectionString", () =>
			{
				return new StringRegistryItem(
					  "BorderWiseUmpDatabaseConnectionString",
					  BorderWiseUmpCategory,
					  (NoResString)"UMP Database Connection String",
					  (NoResString)"The connection string used for running queries directly on the UMP database from ediProd.",
					  RegistryStorageFlags.System,
					  RegistryOptions.Default,
					  defaultValue: string.Empty)
				{
					EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password)
				};
			});

		#endregion

		#region Book Review

		public StringRegistryItem SelectionCriteriaForUpdatedBookToManuallyProcess
		{
			get
			{
				return GetItem("SelectionCriteriaForUpdatedBookToManuallyProcess", () =>
				{
					return new StringRegistryItem("SelectionCriteriaForUpdatedBookToManuallyProcess",
						Categories.BorderWise_WorkItemSelectionCriteria,
						(NoResString)"Updated Book to Manually Process",
						(NoResString)"The value to enter in the 5th selection criterion field for Work Items created for a book update that is to be processed by the Manual Conversion team.",
						new StringRegistryDataType(CharacterCase.Upper, 0, 3),
						new TextRegistryEditorInfo(),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						"UPM");
				});
			}
		}

		public StringRegistryItem SelectionCriteriaForUpdatedBookToAutomaticallyProcess
		{
			get
			{
				return GetItem("SelectionCriteriaForUpdatedBookToAutomaticallyProcess", () =>
				{
					return new StringRegistryItem("SelectionCriteriaForUpdatedBookToAutomaticallyProcess",
						Categories.BorderWise_WorkItemSelectionCriteria,
						(NoResString)"Updated Book to Auto-Process",
						(NoResString)"The value to enter in the 5th selection criterion field for Work Items created for a book update that is to be processed through the Content Service.",
						new StringRegistryDataType(CharacterCase.Upper, 0, 3),
						new TextRegistryEditorInfo(),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						"UPC");
				});
			}
		}

		#endregion

		public const string BorderWiseSubCategory = Category + "/BorderWise";

		public IntRegistryItem BorderWiseMaximumDeviceCount
		{
			get
			{
				return GetItem("BorderWiseMaximumDeviceCount", delegate
				{
					return new IntRegistryItem(
						"BorderWiseMaximumDeviceCount",
						(NoResString)BorderWiseSubCategory,
						(NoResString)"Maximum Devices per User",
						(NoResString)"The maximum number of device licences an individual user can have, before WiseTech need to manually intervene.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						1);
				});
			}
		}

		public IntRegistryItem BorderWiseMaximumLicenceRelocationsPerMonth
		{
			get
			{
				return GetItem("BorderWiseMaximumLicenceRelocationsPerMonth", delegate
				{
					return new IntRegistryItem(
						"BorderWiseMaximumLicenceRelocationsPerMonth",
						(NoResString)BorderWiseSubCategory,
						(NoResString)"Maximum Licence Changes per Month",
						(NoResString)"The maximum number of licence moves between devices in a calendar month before WiseTech need to manually intervene.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						3);
				});
			}
		}

		public StringRegistryItem BorderWiseRegistrationUrl
		{
			get
			{
				return GetItem("BorderWiseRegistrationUrl", delegate
				{
					var item = new StringRegistryItem(
						"BorderWiseRegistrationUrl",
						(NoResString)BorderWiseSubCategory,
						(NoResString)"Registration URL",
						(NoResString)"The base URL for BorderWise registration",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						"https://app.borderwise.com/account/activate?token=");

					item.DataType = new UriRegistryDataType(Uri.UriSchemeHttps) { AllowAutoProtocolPrefixing = false };
					return item;
				});
			}
		}

		public GuidRegistryItem BorderWiseNewOrganisationEmailNotificationGroup
		{
			get
			{
				return GetItem("BorderWiseNewOrganisationEmailNotificationGroup", () =>
				{
					return new GuidRegistryItem(
						"BorderWiseNewOrganisationEmailNotificationGroup",
						(NoResString)BorderWiseSubCategory,
						(NoResString)"New Organisation Notification Group",
						(NoResString)"Define the notification group for newly created organisations through the registration process.",
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						Guid.Empty);
				});
			}
		}

		public StringRegistryItem BorderWiseConfirmWebAccessUrl
		{
			get
			{
				return GetItem("BorderWiseConfirmWebAccessUrl", delegate
				{
					var item = new StringRegistryItem(
						"BorderWiseConfirmWebAccessUrl",
						(NoResString)BorderWiseSubCategory,
						(NoResString)"Confirm WebAccess URL",
						(NoResString)"The URL for admin to allow accessing to BorderWise and remindering password",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						"https://app.borderwise.com/account/forgot-password?token=");

					item.DataType = new UriRegistryDataType(Uri.UriSchemeHttps) { AllowAutoProtocolPrefixing = false };
					return item;
				});
			}
		}

		#endregion

		#region XTCredentialManagement

		public const string XTCredentialManagementSubCategory = Category + "/XT Credential Management";

		public DateTimeRegistryItem TXIOutageStartTime
		{
			get
			{
				return GetItem("TXIOutageStartTime", delegate
				{
					return new DateTimeRegistryItem(
						"TXIOutageStartTime",
						(NoResString)XTCredentialManagementSubCategory,
						(NoResString)"TXI Service Task Outage Start Time",
						(NoResString)"The time the current outage for the TXI service task started",
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						DateTime.MinValue,
						false);
				});
			}
		}

		public DateTimeRegistryItem TXOOutageStartTime
		{
			get
			{
				return GetItem("TXOOutageStartTime", delegate
				{
					return new DateTimeRegistryItem(
						"TXOOutageStartTime",
						(NoResString)XTCredentialManagementSubCategory,
						(NoResString)"TXO Service Task Outage Start Time",
						(NoResString)"The time the current outage for the TXO service task started",
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						DateTime.MinValue,
						false);
				});
			}
		}

		#endregion

		#region BranchForEDIServiceTasks

		public GuidRegistryItem BranchForEDIServiceTasks
		{
			get
			{
				return GetItem("BranchForEDIServiceTasks", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"BranchForEDIServiceTasks",
						(NoResString)Category,
						ResString.GetMultilingualString("94d471f9-48eb-41e8-91dd-df6a6a84392a", "Branch for EDI Service Tasks"),
						ResString.GetMultilingualString("4f5098fe-dd3d-4573-81bd-4f05fd73a44a", "Branch will be used to run EDI Service Tasks."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						Guid.Empty);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbBranch);
					return result;
				});
			}
		}

		#endregion

		#region TestHelpers
#if DEBUG
		public static void CreateProductsAndModulesForTest()
		{
			var collection = new SystemProductCollection();
			var parent1 = collection.AddNew();
			parent1.Code = "AAA";
			parent1.Description = (NoResString)"AAA Product";
			parent1.Enabled = true;
			var child1a = parent1.ModuleMappings.AddNew();
			child1a.ModuleCode = "AA1";
			child1a.ModuleDescription = (NoResString)"AA1 Module";
			var child1b = parent1.ModuleMappings.AddNew();
			child1b.ModuleCode = "AA2";
			child1b.ModuleDescription = (NoResString)"AA2 Module";
			var parent2 = collection.AddNew();
			parent2.Code = "BBB";
			parent2.Description = (NoResString)"BBB Product";
			parent2.Enabled = true;
			var child2a = parent2.ModuleMappings.AddNew();
			child2a.ModuleCode = "BB1";
			child2a.ModuleDescription = (NoResString)"BB1 Module";

			Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}
#endif
		#endregion

		protected override IEnumerable<IRegistryItem> GetItemsNotAccessedUsingProperties()
		{
			List<IRegistryItem> result = new List<IRegistryItem>();

			foreach (var propertyInfo in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				if (typeof(MailboxSettings).IsAssignableFrom(propertyInfo.PropertyType))
				{
					var mailboxSettings = (MailboxSettings)(propertyInfo.GetValue(this, null));
					result.AddRange(mailboxSettings.GetAllItems());
				}
			}

			return result;
		}

		IRegistryItem GetMailBoxItem(string key, MailboxSettings.CreateMailBoxItemDelegate createMailBoxItemDelegate)
		{
			return GetItem(key, () => createMailBoxItemDelegate());
		}

		public MailboxSettings CustomerServiceMailBox { get { return GetMailBox("CustomerService", "Customer Service Mailbox"); } }
		public MailboxSettings CurrentVersionReportMailBox { get { return GetMailBox("CurrentVersionReport", "Current Version Report Mailbox"); } }
		public MailboxSettings ImplementationMailBox { get { return GetMailBox("Implementation", "Implementation Mailbox"); } }
		public MailboxSettings IssueReportMailBox { get { return GetMailBox("IssueReport", "Issue Report Mailbox"); } }
		public MailboxSettings TrainingScheduleMailBox { get { return GetMailBox("TrainingSchedule", "Training Schedule Mailbox"); } }
		public MailboxSettings SupportRequestMailBox { get { return GetMailBox("SupportRequest", "Support Request Mailbox"); } }
		public MailboxSettings DeliveredVersionReportMailBox { get { return GetMailBox("DeliveredVersionReport", "Delivered Version Report Mailbox"); } }

		MailboxSettings GetMailBox(string namePrefix, string caption)
		{
			if (mailboxMap == null)
			{
				mailboxMap = new Dictionary<string, MailboxSettings>();
			}

			MailboxSettings mailboxSettings;
			if (!mailboxMap.TryGetValue(namePrefix, out mailboxSettings))
			{
				var category = RegistryItemSet.CombineCategories(EDIDataRegistry.EmailAddressesCategory, (NoResString)caption);
				mailboxSettings = new MailboxSettings(namePrefix, category, GetMailBoxItem);
				mailboxMap.Add(namePrefix, mailboxSettings);
			}

			return mailboxSettings;
		}
		Dictionary<string, MailboxSettings> mailboxMap;

		#region Linked Server

		public const string LinkedServerSubCategory = Category + "/Linked Server";

		public StringRegistryItem eRouterLinkedServer
		{
			get
			{
				return GetItem(
					"eRouterLinkedServer",
					delegate
					{
						StringRegistryItem registryItem = new StringRegistryItem(
							"eRouterLinkedServer",
							(NoResString)LinkedServerSubCategory,
							(NoResString)"eRouter",
							(NoResString)"Linked server for eRouter, e.g., syderouter.db.wtg.zone",
							RegistryStorageFlags.System);
						return registryItem;
					});
			}
		}

		#endregion

		#region OpportunityValueAnalysisDefault

		public const string SalesAndMarketingSubCategory = Category + "/Sales & Marketing";

		public GuidRegistryItem CommissionGeneratorNotificationGroup
		{
			get
			{
				return GetItem("CommissionGeneratorNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"CommissionGeneratorNotificationGroup",
						(NoResString)SalesAndMarketingSubCategory,
						(NoResString)"Commission Generator Notification Group",
						(NoResString)"This is the group which gets notification emails from the Commission Generator service task.",
						RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public OpportunityValueAnalysisDefaultCollection OpportunityValueAnalysisDefaults
		{
			get { return OpportunityValueAnalysisDefaultRegistryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public OpportunityValueAnalysisDefaultRegistryItem OpportunityValueAnalysisDefaultRegistryItem
		{
			get
			{
				return GetItem("OpportunityValueAnalysisDefault",
						delegate
						{ return new OpportunityValueAnalysisDefaultRegistryItem(SalesAndMarketingSubCategory); });
			}
		}

		public GuidRegistryItem OpportunityExchangeRateCompany
		{
			get
			{
				return GetItem("OpportunityExchangeRateCompany", delegate
				{
					var exchangeRateCompany = RegistryFactory.Instance.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "AUS");

					var result = new GuidRegistryItem(
						"OpportunityExchangeRateCompany",
						(NoResString)SalesAndMarketingSubCategory,
						(NoResString)"Opportunity Exchange Rate Company",
						null,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						(exchangeRateCompany != null ? exchangeRateCompany.PK.ToGuid() : Guid.Empty));

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbCompany);
					return result;
				});
			}
		}

		#endregion

		#region Sales Conductor

		public const string SalesConductorSubCategory = SalesAndMarketingSubCategory + "/Sales Conductor";

		public StringRegistryItem SalesConductorDomainUrl
		{
			get
			{
				return GetItem("SalesConductorDomainUrl", delegate
				{
					return new StringRegistryItem(
						"SalesConductorDomainUrl",
						(NoResString)SalesConductorSubCategory,
						ResString.GetMultilingualString("88c84af4-2818-4475-b59f-aceee2d86f38", "Sales Conductor Domain URL"),
						ResString.GetMultilingualString("6e26c3c8-09b2-4055-bd03-1fb102fc7b7d", "Specify the base URL to the Sales Conductor Discovery Site."),
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"https://myaccount.cargowise.com/en-us/Home/SalesConductor/Discovery.aspx");
				});
			}
		}

		public StringRegistryItem SalesConductorContentPlaylistBuilderUrl
		{
			get
			{
				return GetItem("SalesConductorContentPlaylistBuilderUrl", delegate
				{
					return new StringRegistryItem(
						"SalesConductorContentPlaylistBuilderUrl",
						(NoResString)SalesConductorSubCategory,
						ResString.GetMultilingualString("48966744-a7b6-402d-b1fb-2afac81fce29", "Sales Conductor Content Play List Builder URL"),
						ResString.GetMultilingualString("6ee17113-a00a-476e-8aa8-965c23a8c9d3", "Specify the base URL to the Sales Conductor Content Play List Builder Site."),
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"https://myaccount.cargowise.com/Home/SalesConductor/SalesContentPlaylistBuilder.aspx");
				});
			}
		}

		#endregion

		#region AYC Trigger Type

		public AYCTriggerTypeRegistryItem AYCTriggerTypeSettings
		{
			get
			{
				return GetItem("AYCTriggerTypeSettings", delegate
				{
					var result = new AYCTriggerTypeRegistryItem(
						"AYCTriggerTypeSettings",
						(NoResString)SalesAndMarketingSubCategory,
						(NoResString)"AYC Trigger Type Settings",
						(NoResString)"Primary and Secondary Charge Codes");

					return result;
				});
			}
		}

		#endregion

		#region Payroll Metrics

		public const string HRSubCategory = Category + "/HR";
		public const string PayrollMetricsSubCategory = HRSubCategory + "/Payroll Metrics";

		public CodeDescriptionPairListRegistryItem PayrollMetricsLeaveTypes
		{
			get
			{
				return GetItem("PayrollMetricsLeaveTypes", delegate
				{
					var defaultValue = new CodeDescriptionPairList();
					defaultValue.AddPair("Annual Leave", "ANN");
					defaultValue.AddPair("Annual Leave - Cash Out", "ANN");
					defaultValue.AddPair("Bereavement Leave", "COM");
					defaultValue.AddPair("Carers Leave", "FAM");
					defaultValue.AddPair("Carers Leave No MC", "FAM");
					defaultValue.AddPair("Jury Duty", "JUR");
					defaultValue.AddPair("Leave Without Pay", "LWP");
					defaultValue.AddPair("Long Service Leave", "LSL");
					defaultValue.AddPair("Parental/Maternal Leave Unpaid", "MAT");
					defaultValue.AddPair("Personal Leave", "SIC");
					defaultValue.AddPair("Personal Leave No MC", "SIC");
					defaultValue.AddPair("Study Leave", "STU");
					defaultValue.AddPair("Unpaid Personal Leave", "SIU");

					return new CodeDescriptionPairListRegistryItem(
						"PayrollMetricsLeaveTypes",
						(NoResString)PayrollMetricsSubCategory,
						(NoResString)"Leave Types",
						(NoResString)"List of Payroll Metrics Leave Types and the matching public staff leave type",
						50,
						new CodeDescriptionPairListEditorInfo(true, true, CodeDescriptionPairListEditorInfo.CharacterCasing.Normal, CodeDescriptionPairListEditorInfo.CharacterCasing.Upper,
								(NoResString)"Payroll Metrics", (NoResString)"Internal"),
						RegistryStorageFlags.System,
						false,
						RegistryOptions.Default,
						defaultValue, false);
				});
			}
		}

		public StringArrayRegistryItem PayrollMetricsPayrollNames
		{
			get
			{
				return GetItem("PayrollMetricsPayrollNames", delegate
				{
					return new StringArrayRegistryItem(
						"PayrollMetricsPayrollNames",
						(NoResString)PayrollMetricsSubCategory,
						(NoResString)"Payroll Names",
						(NoResString)"Our payroll names.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						Array.Empty<string>());
				});
			}
		}

		public DateTimeRegistryItem PayrollMetricsLastLeaveSyncUtc
		{
			get
			{
				return GetItem("PayrollMetricsLastLeaveSync", delegate
				{
					return new DateTimeRegistryItem(
						"PayrollMetricsLastLeaveSync",
						(NoResString)PayrollMetricsSubCategory,
						(NoResString)"Last Leave Sync UTC",
						(NoResString)"DateTime of last leave synchronization in UTC. Automatically managed by service task.",
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.LongIncludingSeconds),
						RegistryStorageFlags.System,
						RegistryOptions.NotCached,
						DateTime.MinValue,
						false);
				});
			}
		}

		public StringRegistryItem PayrollMetricsServiceUri
		{
			get
			{
				return GetItem(
					"PayrollMetricsServiceUri",
					delegate
					{
						StringRegistryItem registryItem = new StringRegistryItem(
							"PayrollMetricsServiceUri",
							(NoResString)PayrollMetricsSubCategory,
							(NoResString)"Service URI",
							(NoResString)"Specify the URL for the Payroll Metrics Service",
							RegistryStorageFlags.System);

						return registryItem;
					});
			}
		}

		public StringRegistryItem PayrollMetricsCustomerCode
		{
			get
			{
				return GetItem(
					"PayrollMetricsCustomerCode",
					delegate
					{
						StringRegistryItem registryItem = new StringRegistryItem(
							"PayrollMetricsCustomerCode",
							(NoResString)PayrollMetricsSubCategory,
							(NoResString)"Customer Code",
							(NoResString)"Our Customer Code in Payroll Metrics",
							RegistryStorageFlags.System);

						return registryItem;
					});
			}
		}

		public StringRegistryItem PayrollMetricsServiceUser
		{
			get
			{
				return GetItem(
					"PayrollMetricsServiceUser",
					delegate
					{
						StringRegistryItem registryItem = new StringRegistryItem(
							"PayrollMetricsServiceUser",
							(NoResString)PayrollMetricsSubCategory,
							(NoResString)"Service User",
							(NoResString)"Specify the user name for login to the Payroll Metrics Service",
							RegistryStorageFlags.System);

						return registryItem;
					});
			}
		}

		public StringRegistryItem PayrollMetricsServicePassword
		{
			get
			{
				return GetItem(
					"PayrollMetricsServicePassword",
					delegate
					{
						StringRegistryItem registryItem = new StringRegistryItem(
							"PayrollMetricsServicePassword",
							(NoResString)PayrollMetricsSubCategory,
							(NoResString)"Service Password",
							(NoResString)"Specify the password for login to the Payroll Metrics Service",
							new StringRegistryDataType(true),
							RegistryStorageFlags.System);

						registryItem.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password);
						return registryItem;
					});
			}
		}

		public BooleanRegistryItem AllowPayrollMetricsServiceTask
		{
			get
			{
				return GetItem("AllowPayrollMetricsServiceTask", delegate
				{
					var result = new BooleanRegistryItem(
						"AllowPayrollMetricsServiceTask",
						(NoResString)PayrollMetricsSubCategory,
						(NoResString)"Allow Payroll Metrics Service Task",
						(NoResString)"Set this to \"yes\" to allow Payroll Metrics Service Task",
						RegistryStorageFlags.System,
						true);
					return result;
				});
			}
		}

		#endregion

		#region BambooHR
		public GuidRegistryItem HRNotificationGroup
		{
			get
			{
				return GetItem("HRNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"HRNotificationGroup",
						(NoResString)HRSubCategory,
						(NoResString)"HR Notification Group",
						(NoResString)"This is the group that receives notifications about problems with BambooHR and PayrollMetrics sync.",
						RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region MonthlyUsageInvoiceCategory

		//In order to keep analysis baseline valid, we have to add new registry item here. The baseline is determined by line numbers.
		public MultilingualStringRegistryItem RevisedPrepaymentBalanceDescription
		{
			get
			{
				return GetItem(
					"RevisedPrepaymentBalanceDescription",
					delegate
					{
						var registryItem = new MultilingualStringRegistryItem(
							"RevisedPrepaymentBalanceDescription",
							(NoResString)MonthlyUsageInvoiceCategory,
							(NoResString)"Revised Prepayment Balance Description",
							(NoResString)"Description to put on the Billing Summary.",
							RegistryStorageFlags.System,
							ResString.GetMultilingualString("EDIDataRegistry|RevisedPrepaymentBalanceDescription", "Revised Prepayment Balance - Effective 1st June 2018"));
						return registryItem;
					});
			}
		}

		#endregion

		#region LicenceBillingCategory

		public StringArrayRegistryItem BillingTranslationExportSupportedLanguages
		{
			get
			{
				return GetItem("BillingTranslationExportSupportedLanguages", delegate
				{
					return new StringArrayRegistryItem(
					"BillingTranslationExportSupportedLanguages",
					(NoResString)LicenceBillingCategory,
					(NoResString)"Billing Translation Export Supported Languages",
					null,
					RegistryStorageFlags.System, new string[] { Core.SharedConstants.Languages.ChineseSimplified });
				});
			}
		}

		#endregion

		#region Incident Unavailable Staff

		public BooleanRegistryItem IncidentStaffUnavailableSupportNotificationEnable
		{
			get
			{
				return GetItem("IncidentStaffUnavailableSupportNotificationEnable", () =>
				{
					return new BooleanRegistryItem(
						"IncidentStaffUnavailableSupportNotificationEnable",
						(NoResString)CustomerServiceSubCategory,
						(NoResString)"Incident Staff Unavailable Support Notification Enable",
						(NoResString)"Enable or disable all support notifications related to the unavailable assigned staff.",
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public GuidRegistryItem IncidentStaffUnavailableSupportNotificationGroup
		{
			get
			{
				return GetItem("IncidentStaffUnavailableSupportNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"IncidentStaffUnavailableSupportNotificationGroup",
						(NoResString)CustomerServiceSubCategory,
						(NoResString)"Incident Staff Unavailable Support Notification Group",
						(NoResString)"The staff group that will be notified about unavailable assigned staff to the Incident.",
						RegistryStorageFlags.System,
						IncidentStaffUnavailableSupportNotificationEnable.Value ? RegistryOptions.Default : RegistryOptions.IsHidden);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Email Verification

		public const string EmailVerificationSubCategory = MyAccountPortalSubCategory + "/Email Verification";

		public NotificationEmailTemplateRegistryItem EmailVerificationNotificationMessageTemplate
		{
			get
			{
				return GetItem(
					"EmailVerificationNotificationMessageTemplate",
					delegate
					{
						var registryItem = new NotificationEmailTemplateRegistryItem(
							"EmailVerificationNotificationMessageTemplate",
							(NoResString)EmailVerificationSubCategory,
							(NoResString)"Email Verification Notification Message Template",
							(NoResString)"Email Verification Notification Message Template",
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							typeof(DocEdiCustomerUserAccount),
							@"Website Email Verification",
							@"<b>Website Email Verication Request</b><br><br>

Dear (*PersonName*), <br><br>
A request has been made for the email verification.<br><br>
User details:<br>
Person Name		: (*PersonName*)<br>
Person Email		: (*PersonEmail*)<br>
User Account Email	: (*UserAccountEmail*)<br>

<br>Click the 'Verify Email' button to reauntheticate and enable Auto Login for your account.<br><br>
(*VerifyButton*)<br><br>
<br>If you have issues clicking the link, copy and paste the following line into your browser: <br>
(*VerifyLinkAsText*)<br><br>
<b>Note: This email verification  link is only valid for the next 24 hours.</b><br><br>
If you did not request a password set, please contact us immediately and do not click on this link.<br><br>"
);
						return registryItem;
					});
			}
		}

		#endregion

		#region Ambiguous Contact Login Notification

		public const string AmbiguousContactLoginNotificationSubCategory = MyAccountPortalSubCategory + "/Ambiguous Contact Login Notification";

		public NotificationEmailTemplateRegistryItem AmbiguousContactLoginNotificationMessageTemplate
		{
			get
			{
				return GetItem(
					"AmbiguousContactLoginNotificationMessageTemplate",
					delegate
					{
						var registryItem = new NotificationEmailTemplateRegistryItem(
							"AmbiguousContactLoginNotificationMessageTemplate",
							(NoResString)AmbiguousContactLoginNotificationSubCategory,
							(NoResString)"Ambiguous Contact Login Notification Message Template",
							(NoResString)"Ambiguous Contact Login Notification Message Template",
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							typeof(DocMyAccountAmbiguousLogin),
							"Complete your My Account setup",
							@"<b>Complete your My Account setup</b><br><br>

A recent log in attempt was made to  My Account using this email without a company code specified.<br><br>

To log in with an email and password, you'll need to complete your account setup by registering a Personal Email address with each company account you are registered with.<br><br>

You can complete this setup by logging into My Account with a specific Company Code and using the Change Personal Email feature. To access this feature, navigate to Account Information -> Contact Information -> Change personal email.<br><br>

The Company Codes linked to this email address are:<br><br>
(*CompanyList*)<br><br>

Alternatively, you can specify a Company Code when you login to My Account."
);
						return registryItem;
					});
			}
		}

		#endregion

		#region Licence Database

		public CodeDescriptionPairListRegistryItem InternalEnterpriseMasterOrgs
		{
			get
			{
				return GetItem("InternalEnterpriseMasterOrgs", delegate
				{
					var defaultList = new CodeDescriptionPairList();
					defaultList.AddPair("HYE", (NoResString)string.Empty);
					defaultList.AddPair("WTL", (NoResString)string.Empty);
					defaultList.AddPair("WUT", (NoResString)string.Empty);

					return new CodeDescriptionPairListRegistryItem(
						"InternalEnterpriseMasterOrgs",
						(NoResString)MyAccountPortalSubCategory,
						(NoResString)"Internal Enterprise Master Orgs",
						(NoResString)"Specify default master org for licence databases under internal enterprise.",
						3,
						RegistryStorageFlags.System,
						false,
						defaultList);
				});
			}
		}

		#endregion

		#region EdiUserAgreement

		const string UserAgreementSubCategory = Category + "/User Agreement";

		public StringRegistryItem MyAccountUserVerifyAgreementEmailTemplate
		{
			get
			{
				return GetItem(
					"MyAccountUserVerifyAgreementEmailTemplate",
					delegate
					{
						StringRegistryItem registryItem = new StringRegistryItem(
							"MyAccountUserVerifyAgreementEmailTemplate",
							(NoResString)UserAgreementSubCategory,
							(NoResString)"MyAccount User Verify Agreement Email Template",
							(NoResString)"This is the default template (body) for user verify agreement emails.",
							RegistryStorageFlags.System,
							MyAccountUserVerifyAgreementUrlMacro);
						registryItem.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
						return registryItem;
					});
			}
		}

		public const string MyAccountUserVerifyAgreementUrlMacro = "(*UserVerifyAgreementUrl*)";

		public NotificationEmailTemplateRegistryItem UserAgreementAcknowledgementNotificationMessageTemplate
		{
			get
			{
				return GetItem(
					"UserAgreementAcknowledgementNotificationMessageTemplate",
					delegate
					{
						NotificationEmailTemplateRegistryItem registryItem = new NotificationEmailTemplateRegistryItem(
							"UserAgreementAcknowledgementNotificationMessageTemplate",
							(NoResString)UserAgreementSubCategory,
							(NoResString)"User Agreement Acknowledgement Notification Message Template",
							(NoResString)"This is the default user agreement acknowledgement notification message.",
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							typeof(DocEdiUserAgreement),
							"(*AgreementTitle*)",
							"(*AgreementContent*)");
						return registryItem;
					});
			}
		}

		public NotificationEmailTemplateRegistryItem CorporateAgreementAcknowledgementNotificationMessageTemplate
		{
			get
			{
				return GetItem(
					"CorporateAgreementAcknowledgementNotificationMessageTemplate",
					delegate
					{
						NotificationEmailTemplateRegistryItem registryItem = new NotificationEmailTemplateRegistryItem(
							"CorporateAgreementAcknowledgementNotificationMessageTemplate",
							(NoResString)UserAgreementSubCategory,
							(NoResString)"Corporate Agreement Acknowledgement Notification Message Template",
							(NoResString)"This is the default corporate agreement acknowledgement notification message.",
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							typeof(DocEdiUserAgreement),
							"(*AgreementTitle*)",
							"(*AgreementContent*)");
						return registryItem;
					});
			}
		}

		public StringRegistryItem UserAgreementAcknowledgementNotificationEmailSenderName
		{
			get
			{
				return GetItem("UserAgreementAcknowledgementNotificationEmailSenderName", delegate
				{
					return new StringRegistryItem(
						"UserAgreementAcknowledgementNotificationEmailSenderName",
						(NoResString)UserAgreementSubCategory,
						(NoResString)"User Agreement Acknowledgement Notification Email Sender Name",
						(NoResString)"This is the default sender's name for user agreement acknowledgement notification emails.",
						RegistryStorageFlags.System,
						"WiseTech Global");
				});
			}
		}

		public StringRegistryItem UserAgreementAcknowledgementNotificationEmailSenderAddress
		{
			get
			{
				return GetItem("UserAgreementAcknowledgementNotificationEmailSenderAddress", delegate
				{
					return new StringRegistryItem(
						"UserAgreementAcknowledgementNotificationEmailSenderAddress",
						(NoResString)UserAgreementSubCategory,
						(NoResString)"User Agreement Acknowledgement Notification Email Sender Email Address",
						(NoResString)"This is the default sender's email Address for user agreement acknowledgement notification emails.",
						RegistryStorageFlags.System,
						"legal@wisetechglobal.com");
				});
			}
		}

		#endregion

		#region ELearningDocument

		const string ELearningDocumentSubCategory = CustomerServiceSubCategory + "/ELearningDocument";
		public StringRegistryItem ELearningDocumentRecentPDFUpdatesApiClientUrl
		{
			get
			{
				return GetItem(
					"ELearningDocumentRecentPDFUpdatesApiClientUrl",
					delegate
					{
						return new StringRegistryItem(
							"ELearningDocumentRecentPDFUpdatesApiClientUrl",
							(NoResString)ELearningDocumentSubCategory,
							(NoResString)"ELearning document recent pdf updates api url",
							(NoResString)"This will be utilized by RecentPDFUpdatesApiClient module to download descriptions of pdf files.",
							RegistryStorageFlags.System,
							"https://myaccount.cargowise.com/DesktopModules/WiseWeb.Framework.WebServices/api/eLearningMedia/RecentPDFUpdates?ancestorPK=6873D1C4-6E67-4AA9-AE7F-D7467C417CB1&cultureCode=en-US&fromDate=");
					});
			}
		}

		public StringRegistryItem ELearningDocumentRecentPDFUpdatesForUpdateNotesApiClientUrl
		{
			get
			{
				return GetItem(
					"ELearningDocumentRecentPDFUpdatesForUpdateNotesApiClientUrl",
					delegate
					{
						return new StringRegistryItem(
							"ELearningDocumentRecentPDFUpdatesForUpdateNotesApiClientUrl",
							(NoResString)ELearningDocumentSubCategory,
							(NoResString)"ELearning Update notes pdf updates api url",
							(NoResString)"This will be utilized by RecentPDFUpdatesApiClient module to download descriptions of Update notes pdf files.",
							RegistryStorageFlags.System,
							"https://myaccount.cargowise.com/DesktopModules/WiseWeb.Framework.WebServices/api/eLearningMedia/RecentPDFUpdates?ancestorPK=CCDE0073-744A-430F-8435-E80A042C154E&cultureCode=en-US&fromDate=");
					});
			}
		}

		public StringRegistryItem ELearningDocumentTechnicalAdvisoryDocumentsApiClientUrl
		{
			get
			{
				return GetItem(
					"ELearningDocumentTechnicalAdvisoryDocumentsApiClientUrl",
					delegate
					{
						return new StringRegistryItem(
							"ELearningDocumentTechnicalAdvisoryDocumentsApiClientUrl",
							(NoResString)ELearningDocumentSubCategory,
							(NoResString)"ELearning CW Technical Advisory updates api url",
							(NoResString)"This will be utilized by RecentPDFUpdatesApiClient module to download descriptions of CW Technical Advisory files.",
							RegistryStorageFlags.System,
							"https://myaccount.cargowise.com/DesktopModules/WiseWeb.Framework.WebServices/api/eLearningMedia/RecentPDFUpdates?ancestorPK=66D678ED-44C4-4E16-A0F4-D77255A82BB0&cultureCode=en-US&fromDate=");
					});
			}
		}

		public StringRegistryItem ELearningDocumentBorderWiseDocumentsApiClientUrl
		{
			get
			{
				return GetItem(
					"ELearningDocumentBorderWiseDocumentsApiClientUrl",
					delegate
					{
						return new StringRegistryItem(
							"ELearningDocumentBorderWiseDocumentsApiClientUrl",
							(NoResString)ELearningDocumentSubCategory,
							(NoResString)"ELearning BorderWise Documents api url",
							(NoResString)"This will be utilized by RecentPDFUpdatesApiClient module to download descriptions of BorderWise Documents files.",
							RegistryStorageFlags.System,
							"https://myaccount.cargowise.com/DesktopModules/WiseWeb.Framework.WebServices/api/eLearningMedia/RecentPDFUpdates?ancestorPK=3DB1F679-5B0E-4DC7-90C9-9634CE5C05DF&cultureCode=en-US&fromDate=");
					});
			}
		}

		public StringRegistryItem ELearningDocumentNetworkSharePdfPathLocationPrefix
		{
			get
			{
				return GetItem(
					"ELearningDocumentNetworkSharePdfPathLocationPrefix",
					delegate
					{
						return new StringRegistryItem(
							"ELearningDocumentNetworkSharePdfPathLocationPrefix",
							(NoResString)ELearningDocumentSubCategory,
							(NoResString)"ELearning document shared folder prefix path location",
							(NoResString)"Network shared folder path to the ELearning documents. This path will be utilized by the Service task to download pdf files from the specified location for the further processing.",
							RegistryStorageFlags.System,
							@"\\sydco-sweb-3\ediWebProd\My-account\");
					});
			}
		}

		public StringRegistryItem ELearningDocumentNetworkShareUser
		{
			get
			{
				return GetItem(
					"ELearningDocumentNetworkShareUser",
					delegate
					{
						return new StringRegistryItem(
							"ELearningDocumentNetworkShareUser",
							(NoResString)ELearningDocumentSubCategory,
							(NoResString)"ELearning document shared folder user",
							(NoResString)"This item should be populated (after system has been deployed) with correct domain\\username for getting access to the ELearningDocumentNetworkSharePdfPathLocationPrefix shared folder that contains pdf ELearning files",
							RegistryStorageFlags.System,
							string.Empty);
					});
			}
		}

		public StringRegistryItem ELearningDocumentNetworkSharePassword
		{
			get
			{
				return GetItem(
					"ELearningDocumentNetworkSharePassword",
					delegate
					{
						return new StringRegistryItem(
							"ELearningDocumentNetworkSharePassword",
							(NoResString)ELearningDocumentSubCategory,
							(NoResString)"ELearning document shared folder password",
							(NoResString)"This will be utilized by the network module to download pdf files.",
							RegistryStorageFlags.System,
							string.Empty);
					});
			}
		}

		public StringRegistryItem ELearningDocumentMyAccountUrl
		{
			get
			{
				return GetItem(
					"ELearningDocumentMyAccountUrl",
					delegate
					{
						return new StringRegistryItem(
							"ELearningDocumentMyAccountUrl",
							(NoResString)ELearningDocumentSubCategory,
							(NoResString)"Domain url of ELearning document location",
							(NoResString)"This will be utilized by the network module to download pdf files.",
							RegistryStorageFlags.System,
							"https://myaccount-portal.cargowise.com/");
					});
			}
		}

		public IntRegistryItem ELearningDocumentMyAccountCallTimeout
		{
			get
			{
				return GetItem("ELearningDocumentMyAccountCallTimeout", delegate
				{
					return new IntRegistryItem(
						"ELearningDocumentMyAccountCallTimeout",
						(NoResString)Category,
						(NoResString)"Timeout for http call to myaccount url",
						(NoResString)@"This value will be utilized by ELearningDocument service task to call myaccount http endpoint",
						RegistryStorageFlags.System,
						defaultValue: 1000 * 60 * 5);
				});
			}
		}

		#endregion

		#region
		public IntRegistryItem TraverseMyAccountIncidentsDaysBack
		{
			get
			{
				return GetItem("TraverseMyAccountIncidentsDaysBack", delegate
				{
					return new IntRegistryItem(
						"TraverseMyAccountIncidentsDaysBack",
						(NoResString)Category,
						(NoResString)"How many days back should lookup process take into consideration",
						(NoResString)@"This value will be utilized by IncidentLinkHealthCheckServiceTask service task to call to fetch incidents for processing and myaccount links validation",
						RegistryStorageFlags.System,
						defaultValue: 14);
				});
			}
		}
		#endregion

		#region EditDocumentOrganizationRegistrationMappingModule

		public BooleanRegistryItem EditDocumentOrganizationRegistrationMappingModule
		{
			get
			{
				return GetItem("EditDocumentOrganizationRegistrationMappingModule", delegate
				{
					return new BooleanRegistryItem(
						"EditDocumentOrganizationRegistrationMappingModule",
						(NoResString)Category,
						ResString.GetMultilingualString("ea0b657c-f17f-88bd-4d89-90a1f14cef4a", "Edit Document Organization Registration Mapping module"),
						ResString.GetMultilingualString("91a9b4de-25b9-abbf-4bf1-bd0d6cb5595a", "When set to Yes, the Document Organization Registration Mapping module may be edited."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region Service Type

		public CodeDescriptionPairListRegistryItem ServiceTypes
		{
			get
			{
				return GetItem("ServiceTypes", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
									"ServiceTypes",
									(NoResString)ModuleMappingsSubCategory,
									(NoResString)"Service Types",
									(NoResString)"The list of valid types for eRequest Service Type",
									3,
									RegistryStorageFlags.System,
									ServiceTypesDefaultList());
				});
			}
		}

		CodeDescriptionPairList ServiceTypesDefaultList()
		{
			var defaultList = new CodeDescriptionPairList();
			defaultList.AddPair("SIM", ResString.GetMultilingualString("c533989d-8bf3-4083-9406-ec733fdaa7a3", "Service Improvement"));
			defaultList.AddPair("TEA", ResString.GetMultilingualString("88c40558-87d8-41a8-a053-288badc667d4", "Tear down"));
			defaultList.AddPair("CAC", ResString.GetMultilingualString("95d6f399-0052-41e8-9d40-a2637aba05cd", "Connectivity/Access"));
			defaultList.AddPair("PER", ResString.GetMultilingualString("edbc61cd-5c1d-451a-8e31-f31cd3e2cb1d", "Performance"));
			defaultList.AddPair("CON", ResString.GetMultilingualString("4a3d9bf5-2a75-4047-8010-6ae7f5712ab6", "Configuration"));
			defaultList.AddPair("CRE", ResString.GetMultilingualString("40790163-7cac-4b14-a4fd-2dba12ed3228", "Creation"));
			defaultList.AddPair("DEP", ResString.GetMultilingualString("7d778cd3-93f2-465c-9e6a-d381e4056314", "Deployment"));
			defaultList.AddPair("FAI", ResString.GetMultilingualString("7226ba3a-fd9c-4c32-a992-bd50488ecf1a", "Failure"));
			return defaultList;
		}

		public ServiceTypeRegistryItem ServiceTypeMappings
		{
			get
			{
				return GetItem("ServiceTypeMappings",
					delegate
					{
						return new ServiceTypeRegistryItem("ServiceTypeMappings",
							(NoResString)ModuleMappingsSubCategory,
							(NoResString)"Service Type Menu Section Mappings",
							(NoResString)@"Mapping list between product, module, criticality and service type.",
							new ServiceTypeRegistryEditorInfo(ModuleListType.MenuSection, true),
							RegistryStorageFlags.System);
					});
			}
		}

		public ServiceTypeRegistryItem ServiceTypeCr8Mappings
		{
			get
			{
				return GetItem("ServiceTypeCr8Mappings",
					delegate
					{
						return new ServiceTypeRegistryItem("ServiceTypeCr8Mappings",
							(NoResString)ModuleMappingsSubCategory,
							(NoResString)"Service Type CR8 Module Mappings",
							(NoResString)@"Mapping list between product, incident product area and CR8 modules.
New CR8 modules can also be added with the mappings.",
							new ServiceTypeRegistryEditorInfo(ModuleListType.Cr8, true),
							RegistryStorageFlags.System,
							GetDefaultServiceTypeCr8Mappings());
					});
			}
		}

		SystemProductCollection GetDefaultServiceTypeCr8Mappings()
		{
			return new SystemProductCollection(ModuleListType.Cr8);
		}

		public ServiceTypeRegistryItem ServiceTypeCr9Mappings
		{
			get
			{
				return GetItem("ServiceTypeCr9Mappings",
					delegate
					{
						return new ServiceTypeRegistryItem("ServiceTypeCr9Mappings",
							(NoResString)ModuleMappingsSubCategory,
							(NoResString)"Service Type CR9 Module Mappings",
							(NoResString)@"Mapping list between product, incident product area and CR9 modules.
New CR9 modules can also be added with the mappings.",
							new ServiceTypeRegistryEditorInfo(ModuleListType.Cr9, true),
							RegistryStorageFlags.System,
							GetDefaultServiceTypeCr9Mappings());
					});
			}
		}

		SystemProductCollection GetDefaultServiceTypeCr9Mappings()
		{
			return new SystemProductCollection(ModuleListType.Cr9);
		}

		#endregion

		#region Data Science

		public const string DataScienceSubCategory = Category + "/Data Science";
		public const string DataScienceSubCategoryAudit = DataScienceSubCategory + "/Audit";
		public const string DataScienceSubCategoryAuditCore = DataScienceSubCategoryAudit + "/Core";
		public const string DataScienceSubCategoryAuditIncidentRelated = DataScienceSubCategoryAudit + "/Incident-related";
		public const string DataScienceSubCategoryAuditBilling = DataScienceSubCategoryAudit + "/Billing";
		public const string DataScienceSubCategoryAuditProductivity = DataScienceSubCategoryAudit + "/Productivity";
		public const string DataScienceSubCategoryAuditIssues = DataScienceSubCategoryAudit + "/Issues";

		public BooleanRegistryItem EnableDataScienceCoreSubscribers => GetItem("EnableDataScienceCoreSubscribers", () =>
			new BooleanRegistryItem(
				"EnableDataScienceCoreSubscribers",
				(NoResString)DataScienceSubCategoryAuditCore,
				(NoResString)"Enable",
				(NoResString)"Send Core table changes to Kafka.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false));

		public StringRegistryItem DataScienceCoreKafkaTopic => GetItem("DataScienceCoreKafkaTopic", () =>
			new StringRegistryItem(
				"DataScienceCoreKafkaTopic",
				(NoResString)DataScienceSubCategoryAuditCore,
				(NoResString)"Kafka topic",
				(NoResString)"Send Core table changes to this Kafka topic.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				string.Empty));

		public StringRegistryItem DataScienceCoreKafkaSaslUsername => GetItem("DataScienceCoreKafkaSaslUsername", () =>
			new StringRegistryItem(
				"DataScienceCoreKafkaSaslUsername",
				(NoResString)DataScienceSubCategoryAuditCore,
				(NoResString)"Kafka SASL username",
				(NoResString)"Kafka SASL username for the Core topic. Defaults to Kafka topic.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				DataScienceIncidentRelatedKafkaTopic.Value));

		public StringRegistryItem DataScienceCoreKafkaSaslPassword
		{
			get
			{
				return GetItem("DataScienceCoreKafkaSaslPassword", delegate
				{
					var result = new StringRegistryItem(
						"DataScienceCoreKafkaSaslPassword",
						(NoResString)DataScienceSubCategoryAuditCore,
						(NoResString)"Kafka SASL password",
						(NoResString)"Kafka SASL password for the Core topic, encoded with TwoWayEncoder.",
						RegistryStorageFlags.System,
						RegistryOptions.Default
					);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password);
					return result;
				});
			}
		}

		public BooleanRegistryItem DataScienceCoreKafkaEnableTransactions => GetItem("DataScienceCoreKafkaEnableTransactions", () =>
			new BooleanRegistryItem(
				"DataScienceCoreKafkaEnableTransactions",
				(NoResString)DataScienceSubCategoryAuditCore,
				(NoResString)"Enable Kafka producer transactions",
				(NoResString)"Send each set of Core table changes within a transaction. There are caveats; see docs in source code for details.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false));

		public BooleanRegistryItem EnableDataScienceIncidentRelatedSubscribers => GetItem("EnableDataScienceIncidentRelatedSubscribers", () =>
			new BooleanRegistryItem(
				"EnableDataScienceIncidentRelatedSubscribers",
				(NoResString)DataScienceSubCategoryAuditIncidentRelated,
				(NoResString)"Enable",
				(NoResString)"Send Incident-related table changes to Kafka.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false));

		public StringRegistryItem DataScienceIncidentRelatedKafkaTopic => GetItem("DataScienceIncidentRelatedKafkaTopic", () =>
			new StringRegistryItem(
				"DataScienceIncidentRelatedKafkaTopic",
				(NoResString)DataScienceSubCategoryAuditIncidentRelated,
				(NoResString)"Kafka topic",
				(NoResString)"Send Incident-related table changes to this Kafka topic.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				string.Empty));

		public StringRegistryItem DataScienceIncidentRelatedKafkaSaslUsername => GetItem("DataScienceIncidentRelatedKafkaSaslUsername", () =>
			new StringRegistryItem(
				"DataScienceIncidentRelatedKafkaSaslUsername",
				(NoResString)DataScienceSubCategoryAuditIncidentRelated,
				(NoResString)"Kafka SASL username",
				(NoResString)"Kafka SASL username for the Incident-related topic. Defaults to Kafka topic.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				DataScienceIncidentRelatedKafkaTopic.Value));

		public StringRegistryItem DataScienceIncidentRelatedKafkaSaslPassword
		{
			get
			{
				return GetItem("DataScienceIncidentRelatedKafkaSaslPassword", delegate
				{
					var result = new StringRegistryItem(
						"DataScienceIncidentRelatedKafkaSaslPassword",
						(NoResString)DataScienceSubCategoryAuditIncidentRelated,
						(NoResString)"Kafka SASL password",
						(NoResString)"Kafka SASL password for the Incident-related topic, encoded with TwoWayEncoder.",
						RegistryStorageFlags.System,
						RegistryOptions.Default
					);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password);
					return result;
				});
			}
		}

		public BooleanRegistryItem DataScienceIncidentRelatedKafkaEnableTransactions => GetItem("DataScienceIncidentRelatedKafkaEnableTransactions", () =>
			new BooleanRegistryItem(
				"DataScienceIncidentRelatedKafkaEnableTransactions",
				(NoResString)DataScienceSubCategoryAuditIncidentRelated,
				(NoResString)"Enable Kafka producer transactions",
				(NoResString)"Send each set of Incident-related table changes within a transaction. There are caveats; see docs in source code for details.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false));

		public BooleanRegistryItem EnableDataScienceBillingSubscribers => GetItem("EnableDataScienceBillingSubscribers", () =>
			new BooleanRegistryItem(
				"EnableDataScienceBillingSubscribers",
				(NoResString)DataScienceSubCategoryAuditBilling,
				(NoResString)"Enable",
				(NoResString)"Send Billing table changes to Kafka.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false));

		public StringRegistryItem DataScienceBillingKafkaTopic => GetItem("DataScienceBillingKafkaTopic", () =>
			new StringRegistryItem(
				"DataScienceBillingKafkaTopic",
				(NoResString)DataScienceSubCategoryAuditBilling,
				(NoResString)"Kafka topic",
				(NoResString)"Send Billing table changes to this Kafka topic.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				string.Empty));

		public StringRegistryItem DataScienceBillingKafkaSaslUsername => GetItem("DataScienceBillingKafkaSaslUsername", () =>
			new StringRegistryItem(
				"DataScienceBillingKafkaSaslUsername",
				(NoResString)DataScienceSubCategoryAuditBilling,
				(NoResString)"Kafka SASL username",
				(NoResString)"Kafka SASL username for the Billing topic. Defaults to Kafka topic.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				DataScienceIncidentRelatedKafkaTopic.Value));

		public StringRegistryItem DataScienceBillingKafkaSaslPassword
		{
			get
			{
				return GetItem("DataScienceBillingKafkaSaslPassword", delegate
				{
					var result = new StringRegistryItem(
						"DataScienceBillingKafkaSaslPassword",
						(NoResString)DataScienceSubCategoryAuditBilling,
						(NoResString)"Kafka SASL password",
						(NoResString)"Kafka SASL password for the Billing topic, encoded with TwoWayEncoder.",
						RegistryStorageFlags.System,
						RegistryOptions.Default
					);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password);
					return result;
				});
			}
		}

		public BooleanRegistryItem DataScienceBillingKafkaEnableTransactions => GetItem("DataScienceBillingKafkaEnableTransactions", () =>
			new BooleanRegistryItem(
				"DataScienceBillingKafkaEnableTransactions",
				(NoResString)DataScienceSubCategoryAuditBilling,
				(NoResString)"Enable Kafka producer transactions",
				(NoResString)"Send each set of Billing table changes within a transaction. There are caveats; see docs in source code for details.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false));

		public BooleanRegistryItem EnableDataScienceProductivitySubscribers => GetItem("EnableDataScienceProductivitySubscribers", () =>
			new BooleanRegistryItem(
				"EnableDataScienceProductivitySubscribers",
				(NoResString)DataScienceSubCategoryAuditProductivity,
				(NoResString)"Enable",
				(NoResString)"Send Productivity table changes to Kafka.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false));

		public StringRegistryItem DataScienceProductivityKafkaTopic => GetItem("DataScienceProductivityKafkaTopic", () =>
			new StringRegistryItem(
				"DataScienceProductivityKafkaTopic",
				(NoResString)DataScienceSubCategoryAuditProductivity,
				(NoResString)"Kafka topic",
				(NoResString)"Send Productivity table changes to this Kafka topic.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				string.Empty));

		public StringRegistryItem DataScienceProductivityKafkaSaslUsername => GetItem("DataScienceProductivityKafkaSaslUsername", () =>
			new StringRegistryItem(
				"DataScienceProductivityKafkaSaslUsername",
				(NoResString)DataScienceSubCategoryAuditProductivity,
				(NoResString)"Kafka SASL username",
				(NoResString)"Kafka SASL username for the Productivity topic. Defaults to Kafka topic.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				DataScienceIncidentRelatedKafkaTopic.Value));

		public StringRegistryItem DataScienceProductivityKafkaSaslPassword
		{
			get
			{
				return GetItem("DataScienceProductivityKafkaSaslPassword", delegate
				{
					var result = new StringRegistryItem(
						"DataScienceProductivityKafkaSaslPassword",
						(NoResString)DataScienceSubCategoryAuditProductivity,
						(NoResString)"Kafka SASL password",
						(NoResString)"Kafka SASL password for the Productivity topic, encoded with TwoWayEncoder.",
						RegistryStorageFlags.System,
						RegistryOptions.Default
					);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password);
					return result;
				});
			}
		}

		public BooleanRegistryItem DataScienceProductivityKafkaEnableTransactions => GetItem("DataScienceProductivityKafkaEnableTransactions", () =>
			new BooleanRegistryItem(
				"DataScienceProductivityKafkaEnableTransactions",
				(NoResString)DataScienceSubCategoryAuditProductivity,
				(NoResString)"Enable Kafka producer transactions",
				(NoResString)"Send each set of Productivity table changes within a transaction. There are caveats; see docs in source code for details.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false));

		public BooleanRegistryItem EnableDataScienceIssuesSubscribers => GetItem("EnableDataScienceIssuesSubscribers", () =>
			new BooleanRegistryItem(
				"EnableDataScienceIssuesSubscribers",
				(NoResString)DataScienceSubCategoryAuditIssues,
				(NoResString)"Enable",
				(NoResString)"Send Issues table changes to Kafka.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
			false));

		public StringRegistryItem DataScienceIssuesKafkaTopic => GetItem("DataScienceIssuesKafkaTopic", () =>
			new StringRegistryItem(
				"DataScienceIssuesKafkaTopic",
				(NoResString)DataScienceSubCategoryAuditIssues,
				(NoResString)"Kafka topic",
				(NoResString)"Send Issues table changes to this Kafka topic.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				string.Empty));

		public StringRegistryItem DataScienceIssuesKafkaSaslUsername => GetItem("DataScienceIssuesKafkaSaslUsername", () =>
			new StringRegistryItem(
				"DataScienceIssuesKafkaSaslUsername",
				(NoResString)DataScienceSubCategoryAuditIssues,
				(NoResString)"Kafka SASL username",
				(NoResString)"Kafka SASL username for the Issues topic. Defaults to Kafka topic.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				DataScienceIssuesKafkaTopic.Value));

		public StringRegistryItem DataScienceIssuesKafkaSaslPassword
		{
			get
			{
				return GetItem("DataScienceIssuesKafkaSaslPassword", delegate
				{
					var result = new StringRegistryItem(
						"DataScienceIssuesKafkaSaslPassword",
						(NoResString)DataScienceSubCategoryAuditIssues,
						(NoResString)"Kafka SASL password",
						(NoResString)"Kafka SASL password for the Issues topic, encoded with TwoWayEncoder.",
						RegistryStorageFlags.System,
						RegistryOptions.Default
					);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password);
					return result;
				});
			}
		}

		public BooleanRegistryItem DataScienceIssuesKafkaEnableTransactions => GetItem("DataScienceIssuesKafkaEnableTransactions", () =>
			new BooleanRegistryItem(
				"DataScienceIssuesKafkaEnableTransactions",
				(NoResString)DataScienceSubCategoryAuditIssues,
				(NoResString)"Enable Kafka producer transactions",
				(NoResString)"Send each set of Issues table changes within a transaction. There are caveats; see docs in source code for details.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false));

		#endregion

		#region Kafka

		public const string KafkaSubCategory = Category + "/Kafka";

		public StringRegistryItem EdiKafkaBootstrapServers => GetItem("EdiKafkaBootstrapServers", () =>
			new StringRegistryItem(
				"EdiKafkaBootstrapServers",
				(NoResString)KafkaSubCategory,
				(NoResString)"Bootstrap servers",
				(NoResString)"Comma-separated list of bootstrap servers of the Kafka cluster to connect to.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				string.Empty));

		#endregion

		#region AWS PCA Management

		public const string AWSManagementSubCategory = Category + "/AWS Private CA";

		public AWSPrivateCARegistryItem AWSPrivateCAListManager
		{
			get
			{
				return GetItem("AWSPrivateCAListManager", delegate
				{
					return new AWSPrivateCARegistryItem(
						"AWSPrivateCAListManager",
						(NoResString)AWSManagementSubCategory,
						(NoResString)"AWS Private CA List Manager",
						(NoResString)"AWS Private CA List Manager",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport
					);
				});
			}
		}

		#endregion

		#region Azure OpenID Connect Configuration

		public const string AzureOpenIDConnectConfigurationListSubCategory = Category + "/Azure OpenID Connect Configuration";

		public AzureOpenIDConnectConfigurationRegistryItem AzureOpenIDConnectConfiguration
		{
			get
			{
				return GetItem("AzureOpenIDConnectConfiguration", delegate
				{
					return new AzureOpenIDConnectConfigurationRegistryItem(
						"AzureOpenIDConnectConfiguration",
						(NoResString)AzureOpenIDConnectConfigurationListSubCategory,
						(NoResString)"Azure OpenID Connect Configuration",
						(NoResString)"This will be utilized by Token Authentication Onboarding Data Form to verify the IDP federations in different environments (Prod and Staging).",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport
						);
				});
			}
		}

		#endregion

		#region Azure Application Management

		public const string AzureApplicationManagementSubCategory = Category + "/Azure Application Management";

		public StringRegistryItem TokenValidationServiceDiscoveryEndpoint
		{
			get
			{
				return GetItem("TokenValidationServiceDiscoveryEndpoint", delegate
				{
					return new StringRegistryItem(
						"TokenValidationServiceDiscoveryEndpoint",
						(NoResString)AzureApplicationManagementSubCategory,
						(NoResString)"Token Validation Service Discovery Endpoint URL",
						(NoResString)"Specify the URL of Token Validation Service discovery endpoint, the URL ends with '/.well-known/openid-configuration'",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"https://identity-temp.wisetechglobal.com/9a6ebfef-9638-4fdd-95bb-5394926c0422/.well-known/openid-configuration");
				});
			}
		}

		public StringRegistryItem AzureApplicationManagementTenantID
		{
			get
			{
				return GetItem("AzureApplicationManagementTenantID", delegate
				{
					return new StringRegistryItem(
						"AzureApplicationManagementTenantID",
						(NoResString)AzureApplicationManagementSubCategory,
						(NoResString)"Azure Application Management Tenant ID",
						(NoResString)"The Tenant ID of Azure Application Management.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport);
				});
			}
		}

		public StringRegistryItem AzureApplicationManagementClientID
		{
			get
			{
				return GetItem("AzureApplicationManagementClientID", delegate
				{
					return new StringRegistryItem(
						"AzureApplicationManagementClientID",
						(NoResString)AzureApplicationManagementSubCategory,
						(NoResString)"Azure Application Management Client ID",
						(NoResString)"The Client ID of Azure Application Management.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport);
				});
			}
		}

		public IntRegistryItem AzureApplicationRedirectUrlSyncInterval
		{
			get
			{
				return GetItem("AzureApplicationRedirectUrlSyncInterval", delegate
				{
					return new IntRegistryItem(
						"AzureApplicationRedirectUrlSyncInterval",
						(NoResString)AzureApplicationManagementSubCategory,
						(NoResString)"The interval days for redirect urls sync",
						(NoResString)"The interval days for redirect urls sync",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						1);
				});
			}
		}

		public DateTimeRegistryItem GithubActionSecretNextCheckDate
		{
			get
			{
				return GetItem("GithubActionSecretNextCheckDate", delegate
				{
					return new DateTimeRegistryItem(
						"GithubActionSecretNextCheckDate",
						(NoResString)AzureApplicationManagementSubCategory,
						(NoResString)"Github Action Secret Next Check Date",
						(NoResString)"This registry item is used to monitor the GitHub actions secret expiry date and send notifications to a predefined notifications group. When the CPS service task runs, it first checks this date to see if it needs to check the secret expira date. After running successfully, it will update the next run date to 7 days later from current date.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport);
				});
			}
		}
		#endregion

		#region B2C Secret Check Management

		public const string B2CSecretCheckManagementSubCategory = Category + "/B2C Secret Check Management";

		public StringRegistryItem B2CSecretCheckManagementTenantID
		{
			get
			{
				return GetItem("B2CSecretCheckManagementTenantID", delegate
				{
					return new StringRegistryItem(
						"B2CSecretCheckManagementTenantID",
						(NoResString)B2CSecretCheckManagementSubCategory,
						(NoResString)"B2C Secret Check Management Tenant ID",
						(NoResString)"The Tenant ID of B2C Secret Check Management.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"4dbcef22-d396-4ee1-b2e3-fa62fa6191df");
				});
			}
		}

		public StringRegistryItem B2CSecretCheckManagementClientID
		{
			get
			{
				return GetItem("B2CSecretCheckManagementClientID", delegate
				{
					return new StringRegistryItem(
						"B2CSecretCheckManagementClientID",
						(NoResString)B2CSecretCheckManagementSubCategory,
						(NoResString)"B2C Secret Check Management Client ID",
						(NoResString)"The Client ID of B2C Secret Check Management.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"642e0e48-0670-45ff-814c-03a86135096e");
				});
			}
		}

		public StringRegistryItem B2CSecretCheckManagementSecretClientID
		{
			get
			{
				return GetItem("B2CSecretCheckManagementSecretClientID", delegate
				{
					return new StringRegistryItem(
						"B2CSecretCheckManagementSecretClientID",
						(NoResString)B2CSecretCheckManagementSubCategory,
						(NoResString)"B2C Secret Check Management Secret Client ID",
						(NoResString)"The client id of azure application that stores the shared secret",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"7a0715ec-77a5-49ce-8121-469540e8de88");
				});
			}
		}

		#endregion

		#region Distributed Data

		public const string DistributedDataSubCategory = Category + "/Distributed Data";

		public IntRegistryItem MaxRowsForDistributedDataSyncRequest
		{
			get
			{
				return GetItem("MaxRowsForDistributedDataSyncRequest", delegate
				{
					return new IntRegistryItem(
						"MaxRowsForDistributedDataSyncRequest",
						(NoResString)DistributedDataSubCategory,
						(NoResString)"Max rows for the distributed data sync request.",
						(NoResString)@"This setting specifies the maximum number of rows that can be processed in a single call during the distributed data synchronization request.",
						RegistryStorageFlags.System,
						1000);
				});
			}
		}

		#endregion

		#region TokenAuthOnboardingSubCategory

		public const string TokenAuthOnboardingSubCategory = Category + "/Token Auth Onboarding";

		public BinaryRegistryItem B2CConfigurationRepositoryGitHubAppPrivateKey
		{
			get
			{
				return GetItem("B2CCustomPolicyGitHubAppPrivateKey", delegate
				{
					return new BinaryRegistryItem(
						"B2CCustomPolicyGitHubAppPrivateKey",
						(NoResString)TokenAuthOnboardingSubCategory,
						(NoResString)"Github Application Private Key",
						(NoResString)"The private key of the Github application that creates pull requests for Azure B2C Configuration repository.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						Array.Empty<byte>())
					{
						EditorInfo = new CWSupportLoginTokenPrivateKeyEditorInfo()
					};
				});
			}
		}

		public StringRegistryItem B2CConfigurationRepositoryGitHubAppID
		{
			get
			{
				return GetItem("B2CCustomPolicyGitHubAppID", delegate
				{
					return new StringRegistryItem(
						"B2CCustomPolicyGitHubAppID",
						(NoResString)TokenAuthOnboardingSubCategory,
						(NoResString)"Github Application ID",
						(NoResString)"The ID of the Github application that creates pull requests for Azure B2C Configuration repository.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport);
				});
			}
		}

		public StringRegistryItem B2CConfigurationRepositoryGitHubAppName
		{
			get
			{
				return GetItem("B2CConfigurationRepositoryGitHubAppName", delegate
				{
					return new StringRegistryItem(
						"B2CConfigurationRepositoryGitHubAppName",
						(NoResString)TokenAuthOnboardingSubCategory,
						(NoResString)"Github Application Name",
						(NoResString)"The name of the Github application that creates pull requests for Azure B2C Configuration repository.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"wtg-b2b-config-bot");
				});
			}
		}

		public StringRegistryItem B2CConfigurationRepositoryOwnerName
		{
			get
			{
				return GetItem("B2CConfigurationRepositoryOwnerName", delegate
				{
					return new StringRegistryItem(
						"B2CConfigurationRepositoryOwnerName",
						(NoResString)TokenAuthOnboardingSubCategory,
						(NoResString)"B2C Configuration Repository Owner Name",
						(NoResString)"Azure B2C Configuration repository owner name.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"WiseTechGlobal");
				});
			}
		}

		#endregion

		#region LicenceKeyBuilder

		public const string LicenceKeyBuilderSubCategory = Category + "/Licence Key Builder";

		public IntRegistryItem ValidDaysOfGenerateLoginTokenWithOldEntCodeButton
		{
			get
			{
				return GetItem("ValidDaysOfGenerateLoginTokenWithOldEntCodeButton", delegate
				{
					return new IntRegistryItem(
						"ValidDaysOfGenerateLoginTokenWithOldEntCodeButton",
						(NoResString)LicenceKeyBuilderSubCategory,
						(NoResString)"Valid Days Of Generate Login Token With Old Entprise Code Button",
						(NoResString)"Valid Days Of Generate Login Token With Old Entprise Code Button",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						7);
				});
			}
		}

		#endregion

		#region DevTools Kafka

		public const string DevToolsSubCategory = Category + "/DevTools";

		public StringRegistryItem DevToolsIntegrationKafkaBootstrapServers => GetItem("EDIDevToolsIntegrationKafkaBootstrapServers", () =>
			new StringRegistryItem(
				"EDIDevToolsIntegrationKafkaBootstrapServers",
				(NoResString)DevToolsSubCategory,
				(NoResString)"Kafka Bootstrap Servers",
				(NoResString)"Comma-separated list of bootstrap servers of the Kafka cluster to connect to.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				string.Empty));

		public StringRegistryItem DevToolsIntegrationKafkaTopic => GetItem("EDIDevToolsIntegrationKafkaTopic", () =>
			new StringRegistryItem(
				"EDIDevToolsIntegrationKafkaTopic",
				(NoResString)DevToolsSubCategory,
				(NoResString)"Kafka Topic",
				(NoResString)"Topic to post Kafka messages to.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				string.Empty));

		public StringRegistryItem DevToolsIntegrationKafkaSaslUserName => GetItem("EDIDevToolsIntegrationKafkaSaslUserName", () =>
			new StringRegistryItem(
				"EDIDevToolsIntegrationKafkaSaslUserName",
				(NoResString)DevToolsSubCategory,
				(NoResString)"Kafka SASL Username",
				(NoResString)"User name to use for SASL authentication to Kafka.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				string.Empty));

		public StringRegistryItem DevToolsIntegrationKafkaSaslPassword => GetItem("EDIDevToolsIntegrationKafkaSaslPassword", () =>
			new StringRegistryItem(
				"EDIDevToolsIntegrationKafkaSaslPassword",
				(NoResString)DevToolsSubCategory,
				(NoResString)"Kafka SASL Password",
				(NoResString)"Password to use for SASL authentication to Kafka.",
				new StringRegistryDataType(isEncrypted: true),
				new TextRegistryEditorInfo(TextEditorType.Password),
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				string.Empty));

		#endregion
	}
}
