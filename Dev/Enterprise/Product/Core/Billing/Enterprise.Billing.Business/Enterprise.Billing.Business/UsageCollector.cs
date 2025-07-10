using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.MasterFiles.Business;
using Newtonsoft.Json.Linq;
using WTG.StaticAnalysis.Annotation;

#region SuppressResourceStringsCheckRegion

namespace Enterprise.Billing.Business
{
	public static class UsageCollector
	{
#if DEBUG
		[ThreadSafe]
		internal static IEnumerable<UsageFeatureProperties> FeaturePropertiesForTest = null;
#endif

		[ThreadSafe]
		internal static ImmutableDictionary<string, IUsageFeature> Features
		{
			get
			{
				lock (featuresLockObj)
				{
					if (features != null && ZDateTime.UtcNow > featuresRefreshTime)
					{
						features = null;
					}
					if (features == null)
					{
						var featureFactory = new UsageFeatureFactory();
						features = GetFeatureProperties().Select(fp => featureFactory.Create(fp)).ToImmutableDictionary(fp => fp.FeatureProperties.Code);
						featuresRefreshTime = ZDateTime.UtcNow.AddMinutes(10);
					}
					return features;
				}
			}
		}

		[ThreadSafe]
		internal static ImmutableDictionary<string, IUsageFeature> features;

		[ThreadSafe]
		static ZDateTime featuresRefreshTime;

		internal static readonly object featuresLockObj = new object();

		static IEnumerable<UsageFeatureProperties> GetFeatureProperties()
		{
#if DEBUG
			if (FeaturePropertiesForTest != null)
			{
				foreach (var feature in FeaturePropertiesForTest)
				{
					yield return feature;
				}

				yield break;
			}
#endif

			// Rating
			yield return new UsageFeatureProperties(UsageFeatures.Codes.Autorate, UsageFeatures.Modules.Rating, "AutoRate");
			yield return new UsageFeatureProperties(UsageFeatures.Codes.RateSelector, UsageFeatures.Modules.Rating, "Rate Selector");
			yield return new UsageFeatureProperties(UsageFeatures.Codes.RateSelectorSearch, UsageFeatures.Modules.Rating, "Rate Selector Search");
			yield return new UsageFeatureProperties(UsageFeatures.Codes.ChargeAutorated, UsageFeatures.Modules.Rating, "Charge AutoRated");
			yield return new UsageFeatureProperties(UsageFeatures.Codes.RatesAPI, UsageFeatures.Modules.Rating, "Rates API");
			yield return new UsageFeatureProperties(UsageFeatures.Codes.RatesLoaded, UsageFeatures.Modules.Rating, "Rates Loaded");

			//Document Delivery
			yield return new UsageFeatureProperties(UsageFeatures.Codes.DocumentGenerated, UsageFeatures.Modules.Rating, "Document Generated", produceDailySummaries: true);

			//PAVE
			var paveUsageFeatures = ObjectFactory.Get<IPAVEUsageCollectorFeatureProvider>();

			foreach (var feature in paveUsageFeatures.GetFeatures())
			{
				yield return new UsageFeatureProperties(feature.Code, feature.Module, feature.Description);
			}

			// Accounting
			yield return new UsageFeatureProperties(UsageFeatures.Codes.UploadGLJournalCount, UsageFeatures.Modules.Accounting, "Upload GL Journal Count");
			yield return new UsageFeatureProperties(UsageFeatures.Codes.UploadGLJournalDetails, UsageFeatures.Modules.Accounting, "Upload GL Journal Details");
			yield return new UsageFeatureProperties(UsageFeatures.Codes.UploadGLJournalViaADAWCount, UsageFeatures.Modules.Accounting, "Upload GL Journal via ADAW Count");
			yield return new UsageFeatureProperties(UsageFeatures.Codes.UploadGLJournalViaADAWDetails, UsageFeatures.Modules.Accounting, "Upload GL Journal via ADAW Details");
			yield return new UsageFeatureProperties(UsageFeatures.Codes.AccComplianceReport, UsageFeatures.Modules.Accounting, "Accounting Compliance Report");
			yield return new UsageFeatureProperties(UsageFeatures.Codes.AccGeneralLedgerData, UsageFeatures.Modules.Accounting, "Generate and Store Journal Entries for Posted Accounting Transactions");
			yield return new UsageFeatureProperties(UsageFeatures.Codes.AccGeneralLedgerProcess, UsageFeatures.Modules.Accounting, "Completed Journal Entries Backlog Processing");
			yield return new UsageFeatureProperties(UsageFeatures.Codes.AccConsolidationGroup, UsageFeatures.Modules.Accounting, "Accounting General Ledger Consolidation Group");
			yield return new UsageFeatureProperties(UsageFeatures.Codes.AccConsolidationGroupGenerateExportFile, UsageFeatures.Modules.Accounting, "Accounting General Ledger Consolidation Group Generate Export File");
			yield return new UsageFeatureProperties(UsageFeatures.Codes.AccConsolidationGroupCreateEliminationJournals, UsageFeatures.Modules.Accounting, "Accounting General Ledger Consolidation Group Create Elimination Journals");

			//Document Signing
			yield return new UsageFeatureProperties(UsageFeatures.Codes.DocumentSigning, UsageFeatures.Modules.DocumentSigning, "Document Signing");

			//Archive Manager
			yield return new UsageFeatureProperties(UsageFeatures.Codes.ArchiveManager, UsageFeatures.Modules.ArchiveManager, "Archive Manager");
			yield return new UsageFeatureProperties(UsageFeatures.Codes.ArchiveManagerDeadlock, UsageFeatures.Modules.ArchiveManager, "Archive Manager Deadlocks");

			//Document S3 Transfer
			yield return new UsageFeatureProperties(UsageFeatures.Codes.DocumentS3Transfer, UsageFeatures.Modules.DocumentS3Transfer, "Document S3 Transfer");

			// Business Intelligence
			yield return new UsageFeatureProperties(UsageFeatures.Codes.BiReplicationAPI, UsageFeatures.Modules.BusinessIntelligence, "Replication API");

			//Search Performed
			yield return new UsageFeatureProperties(UsageFeatures.Codes.SearchPerformed, UsageFeatures.Modules.SearchPerformed, "Search Performed");
		}

		[ThreadSafe]
		internal static readonly Lazy<ImmutableDictionary<string, object>> GenericProperties = new Lazy<ImmutableDictionary<string, object>>(() =>
		{
			var builder = ImmutableDictionary.CreateBuilder<string, object>();
			builder.Add(UsageProperties.FeatureCode.ToLower(), null);
			builder.Add(UsageProperties.Module.ToLower(), null);
			builder.Add(UsageProperties.FeatureDescription.ToLower(), null);
			builder.Add(UsageProperties.OrganisationName.ToLower(), null);

			var propertyObject = typeof(BillingTransaction);
			var properties = propertyObject.GetProperties();
			foreach (var property in properties)
			{
				builder.Add(property.Name.ToLower(), null);
			}

			propertyObject = typeof(UsageTransaction);
			properties = propertyObject.GetProperties();
			foreach (var property in properties)
			{
				builder.Add(property.Name.ToLower(), null);
			}
			return builder.ToImmutable();
		});

		[ThreadSafe]
		static readonly AsyncLocal<ImmutableStack<(string name, object value)[]>> currentScope = new AsyncLocal<ImmutableStack<(string name, object value)[]>>();

		static ImmutableStack<(string name, object value)[]> CurrentScope
		{
			get
			{
				if (currentScope.Value == null)
				{
					currentScope.Value = ImmutableStack<(string name, object value)[]>.Empty;
				}
				return currentScope.Value;
			}
			set
			{
				currentScope.Value = value;
			}
		}

		public static void Report(string featureCode, params (string name, object value)[] properties)
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			Report(factory, featureCode, properties);
			factory.Save();
		}

		public static void Report(BusinessObjectFactory factory, string featureCode, params (string name, object value)[] properties)
		{
			Report(factory, featureCode, null, properties);
		}

		public static void Report(BusinessObjectFactory factory, string featureCode, GlbBranch currentBranch, params (string name, object value)[] properties)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			if (!Features.TryGetValue(featureCode, out var feature))
			{
				throw new ArgumentException("The specified featureCode does not exist. Please add new features to UsageCollector.Features when needed.");
			}
			CheckForClashesWithGenericProperties(properties);

			currentBranch = currentBranch ?? GlbBranch.CurrentBranch;
			var currentCompany = currentBranch.Company ?? GlbCompany.CurrentCompany;
			var organisation = (currentCompany != null && currentCompany.GC_OH_OrgProxy.IsValid) ? factory.Load<OrgHeader>(currentCompany.GC_OH_OrgProxy) : null;
			var allProperties = new JObject();

			AddProperty(allProperties, UsageProperties.FeatureCode, feature.FeatureProperties.Code);
			AddProperty(allProperties, UsageProperties.Module, feature.FeatureProperties.Module);
			AddProperty(allProperties, UsageProperties.FeatureDescription, feature.FeatureProperties.Description);
			AddProperty(allProperties, UsageProperties.OrganisationName, organisation?.OH_FullName.ToString());
			properties.ForEach(property => AddProperty(allProperties, property.name, property.value));

			foreach (var scopeProperties in CurrentScope)
			{
				foreach (var property in scopeProperties)
				{
					if (!allProperties.Properties().Any(p => p.Name.Equals(property.name, StringComparison.InvariantCultureIgnoreCase)))
					{
						AddProperty(allProperties, property.name, property.value);
					}
				}
			}

			feature.Report(allProperties, currentBranch, factory);
		}

		static JToken ObjectToJSON(object value)
		{
			if (value == null)
			{
				return null;
			}

			if (value is string stringValue)
			{
				return string.IsNullOrEmpty(stringValue) ? null : JToken.FromObject(value);
			}

			var valueType = value.GetType();
			if (valueType.IsValueType)
			{
				return Equals(value, Activator.CreateInstance(valueType)) ? null : JToken.FromObject(value);
			}

			if (value is IEnumerable<KeyValuePair<string, object>> innerPropertyPairs)
			{
				return PairsToJSON(innerPropertyPairs);
			}

			if (value is IEnumerable<(string, object)> propertyTuples)
			{
				return PairsToJSON(propertyTuples.Select(p => new KeyValuePair<string, object>(p.Item1, p.Item2)));
			}

			if (value is Array array)
			{
				var jArray = new JArray();
				foreach (var item in array)
				{
					var jToken = ObjectToJSON(item);
					if (jToken != null)
					{
						jArray.Add(jToken);
					}
				}

				return jArray.Count > 0 ? jArray : null;
			}

			return PairsToJSON(valueType.GetProperties().Select(p => new KeyValuePair<string, object>(p.Name, p.GetValue(value))));
		}

		static JObject PairsToJSON(IEnumerable<KeyValuePair<string, object>> keyValuePairs)
		{
			var innerPropertiesAsJSON = new JObject();
			keyValuePairs.ForEach(p => AddProperty(innerPropertiesAsJSON, p.Key, p.Value));
			return innerPropertiesAsJSON.Properties().Any() ? innerPropertiesAsJSON : null;
		}

		static void CheckForClashesWithGenericProperties(params (string name, object value)[] properties)
		{
			foreach (var property in properties)
			{
				if (GenericProperties.Value.ContainsKey(property.name.ToLower()))
				{
					throw new ArgumentOutOfRangeException("name", $"Property {property.name} clashes with a generic property with the same name.");
				}
			}
		}

		static void AddProperty(JObject properties, string name, object value)
		{
			var valueAsJSON = ObjectToJSON(value);
			if (valueAsJSON != null)
			{
				properties.Add(name, valueAsJSON);
			}
		}

		public static IDisposable Scope(params (string name, object value)[] properties)
		{
			CheckForClashesWithGenericProperties(properties);

			CurrentScope = CurrentScope.Push(properties);
			return new DisposableAction(() => CurrentScope = CurrentScope.Pop());
		}
	}

	public static class UsageFeatures
	{
		public static class Codes
		{
			// Rating
			public const string Autorate = "ATR";
			public const string RateSelector = "RSL";
			public const string RateSelectorSearch = "RSS";
			public const string ChargeAutorated = "CHG";
			public const string RatesAPI = "RAP";
			public const string RatesLoaded = "RLD";

			// Accounting
			public const string UploadGLJournalCount = "UGC";
			public const string UploadGLJournalDetails = "UGD";
			public const string UploadGLJournalViaADAWCount = "U1C";
			public const string UploadGLJournalViaADAWDetails = "U1D";
			public const string AccComplianceReport = "ACR";
			public const string AccGeneralLedgerData = "GLD";
			public const string AccGeneralLedgerProcess = "GLP";
			public const string AccConsolidationGroup = "ACG";
			public const string AccConsolidationGroupGenerateExportFile = "CGG";
			public const string AccConsolidationGroupCreateEliminationJournals = "CGC";

			//Document Delivery
			public const string DocumentGenerated = "DGN";

			//Document Signing
			public const string DocumentSigning = "DOS";

			//Archive Manager
			public const string ArchiveManager = "AMA";
			public const string ArchiveManagerDeadlock = "AMD";

			//Document S3 Transfer
			public const string DocumentS3Transfer = "DST";

			// Business Intelligence
			public const string BiReplicationAPI = "BRP";

			//Search Performed
			public const string SearchPerformed = "SPF";
		}

		public static class Modules
		{
			public const string Rating = "Rating";
			public const string DocumentSigning = "DocumentSigning";
			public const string Accounting = "Accounting";
			public const string ArchiveManager = "ArchiveManager";
			public const string DocumentS3Transfer = "DocumentS3Transfer";
			public const string BusinessIntelligence = "BusinessIntelligence";
			public const string SearchPerformed = "SearchPerformed";
		}
	}

	/// <summary>
	/// 	Properties and attributes to be reported to kibana alongside with usage events. To avoid any problems and conflicts,
	/// 	we need to follow some conventions:
	/// 	1. Keep properties list as short as possible. Too many properties in elasticsearch index may cause performance issues.
	/// 	2. Properties with the same name must have values of the same type. So, if one team defines some property Container="20GP",
	/// 		and another team defines Container=Object - this will cause indexing problems on elasticsearch.
	///
	/// 	To achieve above, we need to keep properties centrally and make them reusable. Each team is free to define properties
	/// 	specific for them.
	/// </summary>
	public static class UsageProperties
	{
		#region Common

		/// <summary>
		/// 	The three letter unique feature identifier for the usage.  All feature codes should be declared in UsageFeatures.Codes above.
		/// </summary>
		public const string FeatureCode = "FeatureCode";

		/// <summary>
		/// 	The functional area that the usage was from.  All modules should be declared in UsageFeatures.Modules above.
		/// </summary>
		public const string Module = "Module";

		/// <summary>
		/// 	The detailed description of the feature.
		/// </summary>
		public const string FeatureDescription = "FeatureDescription";

		/// <summary>
		/// 	The name of the organisation reporting the usage.
		/// </summary>
		public const string OrganisationName = "OrganisationName";

		/// <summary>
		/// 	Type of the job associated with the usage. ForwardingShipment/ForwardingConsol, etc.
		/// </summary>
		public const string JobType = "JobType";

		/// <summary>
		/// 	ID of the job associated with the usage. I.e. shipment number, consol number, etc.
		/// </summary>
		public const string JobID = "JobID";

		/// <summary>
		/// 	Any mode associated with the event. Every team may have their own modes.
		/// </summary>
		public const string Mode = "Mode";

		/// <summary>
		/// 	Any action associated with the event. Every team may have their own actions.
		/// </summary>
		public const string Action = "Action";

		/// <summary>
		/// 	The place where an event was initiated. For example, Menu, Workflow.
		/// </summary>
		public const string TriggerSource = "TriggerSource";

		/// <summary>
		///		Container packing mode. FCL, LCL, etc.
		/// </summary>
		public const string ContainerMode = "ContainerMode";

		/// <summary>
		/// 	Transport mode, AIR, SEA, etc.
		/// </summary>
		public const string TransportMode = "TransportMode";

		/// <summary>
		///		An integer value representing a count of events. Depending on the context (i.e. feature code), it might be jobs created, charges created, rates found, whatever.
		///		You are free to create custom properties if it is not clear from the context, i.e. JobsCreatedCount, ChargesCreatedCount, but better to use the generic one
		///		whenever possible so that the total amount of unique properties being reported to elasticsearch is as small as possible.
		/// </summary>
		public const string Count = "Count";

		/// <summary>
		///		The name of the database from which the report originated.
		/// </summary>
		public const string DatabaseName = "DatabaseName";

		/// <summary>
		///		The URL endpoint the usage was from.
		/// </summary>
		public const string RequestUrl = "RequestUrl";

		/// <summary>
		///		The returned status code for the request the usage was from.
		/// </summary>
		public const string StatusCode = "StatusCode";

		/// <summary>
		///		The timestamp at which the usage occurred.
		///	</summary>
		public const string Timestamp = "Timestamp";

		/// <summary>
		///		A list of the parameters in the URL the usage was from.
		/// </summary>
		public const string Parameters = "Parameters";

		/// <summary>
		/// 	The login name of the user who initiated the usage.
		/// </summary>
		public const string UserLoginName = "UserLoginName";

		/// <summary>
		///		The elapsed time of the feature if the feature is some kind of action. The value must be in milliseconds.
		/// </summary>
		public const string ElapsedTime = "ElapsedTime";

		#endregion

		#region Rating

		public const string IsRateSelector = "IsRateSelector";
		public const string SelectedProvider = "SelectedProvider";
		public const string RatesSearchResult = "RatesSearchResult";
		public const string RateType = "RateType";
		public const string RateMode = "RateMode";
		public const string RateCategory = "RateCategory";
		public const string ChargeCalculator = "ChargeCalculator";
		public const string RateEntriesCount = "RateEntriesCount";
		public const string RateLinesCount = "RateLinesCount";
		public const string RateSelectorType = "RateSelectorType";
		public const string SessionTime = "SessionTime";
		public const string DBStats = "DBStats";
		public const string RatesLoadedStats = "RatesLoadedStats";

		#endregion

		#region Document Signing

		public const string CountryDocumentCode = "CountryDocumentCode";
		public const string DocumentName = "DocumentName";
		public const string ParentGuid = "ParentGuid";
		public const string ParentTableName = "ParentTableName";
		public const string ProcessorName = "ProcessorName";
		public const string RunDateTime = "RunDateTime";
		public const string SystemCreateUser = "SystemCreateUser";
		public const string SignStatus = "SignStatus";
		public const string TransactionId = "TransactionId";

		#endregion

		#region Accounting

		public const string JournalIdentifier = "JournalIdentifier";
		public const string JournalType = "JournalType";
		public const string JournalCompanyCode = "JournalCompanyCode";
		public const string CountOfJournalTypes = "CountOfJournalTypes";
		public const string CountOfCompanies = "CountOfCompanies";
		public const string ConsolidationGroupClientID = "ConsolidationGroupClientID";
		public const string ConsolidationGroupCreatedTime = "ConsolidationGroupCreatedTime";
		public const string ConsolidationGroupLastEditTime = "ConsolidationGroupLastEditTime";

		#endregion

		#region Archive Manager

		public const string ArchiveScheduleStartTime = "ArchiveScheduleStartTime";
		public const string ArchiveScheduleTotalRuntime = "ArchiveScheduleTotalRuntime";
		public const string ArchiveSystemName = "ArchiveSystemName";
		public const string ArchiveStageName = "ArchiveStageName";
		public const string ArchiveSystemDateParameters = "ArchiveSystemDateParameters";
		public const string IncludeCustomsJobsInArchiving = "IncludeCustomsJobsInArchiving";
		public const string ArchivingBatchSize = "ArchivingBatchSize";
		public const string TotalRecordsDeletedFromAllTablesDuringArchiving = "TotalRecordsDeletedFromAllTablesDuringArchiving";
		public const string TotalJobHeadersProcessedDuringArchiving = "TotalJobHeadersProcessedDuringArchiving";
		public const string TotalMainRecordLoaded = "TotalMainRecordLoaded";
		public const string TotalMissingDocumentsGeneratedDuringArchiving = "TotalMissingDocumentsGeneratedDuringArchiving";
		public const string TotalDocumentsDeletedDuringArchiving = "TotalDocumentsDeletedDuringArchiving";
		public const string ArchiveActionName = "ArchiveActionName";
		public const string ArchiveItemNK = "ArchiveItemNK";

		#endregion

		#region Document S3 Transfer

		public const string DocumentS3TimeStamp = "DocumentS3TimeStamp";
		public const string DocumentS3BucketName = "DocumentS3BucketName";
		public const string DocumentS3PrimaryKey = "DocumentS3PrimaryKey";
		public const string DocumentS3Size = "DocumentS3Size";
		public const string DocumentS3ActionType = "DocumentS3ActionType";

		#endregion

		#region Search Performed

		public const string SearchType = "SearchType";
		public const string ModuleID = "ModuleID";
		public const string Filters = "Filters";

		#endregion
	}
}

#endregion
