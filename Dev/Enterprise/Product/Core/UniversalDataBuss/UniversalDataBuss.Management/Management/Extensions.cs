using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XmlReading;
using Enterprise.ZArchitecture.Business;
using AttachedDocument = Enterprise.UniversalDataBuss.DataObjects.Universal.AttachedDocument;
using DummyLogger = Enterprise.UniversalDataBuss.Integration.DummyLogger;
using eAdaptorRegistry = Enterprise.Registry.Business.eServices.eAdaptorRegistry;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.UniversalDataBuss.Management
{
	public static class Extensions
	{
		public static T Parse<T>(this TextReader eventXmlReader, IXmlImportLogger logger = null, ICodeMappingManager codeMapper = null, bool throwOnParsingError = false) where T : TopLevelDataObject, new()
		{
			using (var eventXmlStream = new CargoWise.IO.Shim.SubStreamableStream())
			{
				if (eventXmlReader != null)
				{
					using (eventXmlReader)
					{
						StreamWriter writer = new StreamWriter(eventXmlStream);
						var buffer = new char[32768];
						while (true)
						{
							int readCount = eventXmlReader.Read(buffer, 0, buffer.Length);
							if (readCount == 0)
							{
								break;
							}

							writer.Write(buffer, 0, readCount);
						}

						writer.Flush();
					}
				}

				return eventXmlStream.Parse<T>(logger, codeMapper, throwOnParsingError);
			}
		}

		public static T Parse<T>(this SubStreamableStream eventXmlStream, IXmlImportLogger logger = null, ICodeMappingManager codeMapper = null, bool throwOnParsingError = false) where T : TopLevelDataObject, new()
		{
			return eventXmlStream.ParseAndReturnMoreData<T>(logger, codeMapper, throwOnParsingError).DataObject;
		}

		public static ParseData<T> ParseAndReturnMoreData<T>(this SubStreamableStream eventXmlStream, IXmlImportLogger logger = null, ICodeMappingManager codeMapper = null, bool throwOnParsingError = false) where T : TopLevelDataObject, new()
		{
			string namespaceUsed = null;
			var dataObjectFactory = new SetWriterStrategyDataObjectFactory(DefaultDataObjectWriterStrategy.Instance);
			var dataObject = dataObjectFactory.Create<T>();

			if (eventXmlStream != null)
			{
				eventXmlStream.Position = 0;
				IXmlReader xmlReader = new XmlReader(codeMapper, throwOnParsingError, dataObjectFactory);
				xmlReader.ReadXML(dataObject, eventXmlStream, logger ?? new DummyLogger());
				namespaceUsed = xmlReader.Namespace;
			}

			return new ParseData<T>(dataObject, namespaceUsed);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static void AddIfNotEmpty(this List<KeyValuePair<TypeWithDescription, IZType>> list, UniversalEvent.ContextTypes key, IZType value)
		{
			AddCore(list, key.ToString(), value, addIfNotEmpty: true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static void AddIfNotEmpty(this List<KeyValuePair<TypeWithDescription, IZType>> list, AttachedDocument.ContextTypes key, IZType value)
		{
			AddCore(list, key.ToString(), value, addIfNotEmpty: true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static void Add(this List<KeyValuePair<TypeWithDescription, IZType>> list, UniversalEvent.ContextTypes key, IZType value)
		{
			AddCore(list, key.ToString(), value, addIfNotEmpty: false);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static void Add(this List<KeyValuePair<TypeWithDescription, IZType>> list, AttachedDocument.ContextTypes key, IZType value)
		{
			AddCore(list, key.ToString(), value, addIfNotEmpty: false);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		static void AddCore(this List<KeyValuePair<TypeWithDescription, IZType>> list, ZString key, IZType value, bool addIfNotEmpty)
		{
			if (list != null && (!addIfNotEmpty || !value.IsEmpty))
			{
				list.Add(new KeyValuePair<TypeWithDescription, IZType>(new TypeWithDescription(key), value));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static void AddOrUpdateIfNotEmpty(this List<KeyValuePair<TypeWithDescription, IZType>> list, UniversalEvent.ContextTypes key, IZType value)
		{
			if (list != null && !value.IsEmpty)
			{
				var typeWithDescription = new TypeWithDescription(key.ToString());
				if (!list.Any(x => x.Key.Type == typeWithDescription.Type))
				{
					list.Add(new KeyValuePair<TypeWithDescription, IZType>(typeWithDescription, value));
				}
			}
		}

		public static ZString GetIATACode(this IRefUNLOCO unloco)
		{
			return unloco != null ? unloco.RL_IATA : ZString.Empty;
		}

		public static ZString GetUNLOCO(this IRefUNLOCO unloco)
		{
			return unloco != null ? unloco.RL_Code : ZString.Empty;
		}

		public static ZString FormatAirMAWB(this ZString masterBillNum)
		{
			return masterBillNum.Length > 3 ? masterBillNum.InsertSafe(3, "-") : masterBillNum;
		}

		public static ZString EscapeTildas(this ZString value)
		{
			return value.Replace("!", "!!").Replace("~", "!~");
		}

		public static ZString UnEscapeTildas(this ZString value)
		{
			return value.Replace("!~", "~").Replace("!!", "!");
		}

		public static UniversalEvent ToUniversalEvent(this IImportResult importResult, ITopLevelDataObject exportedData, IGlbCompany currentCompany)
		{
			UniversalEvent result;

			if (importResult.LinkedJobs != null && importResult.LinkedJobs.Any())
			{
				result = GetLinkedJobsEvent(exportedData, importResult, currentCompany);
			}
			else if (importResult.WasSuccessful)
			{
				result = GetSuccessfulImportEvent(exportedData, importResult, currentCompany);
			}
			else
			{
				result = GetFailedImportEvent(exportedData, importResult, currentCompany);
			}

			return result;
		}

		static UniversalEvent GetLinkedJobsEvent(ITopLevelDataObject exportedData, IImportResult importResult, IGlbCompany currentCompany)
		{
			return GetUniversalEvent(Events.JobsLinked, exportedData, currentCompany, importResult.LinkedJobs, importResult.Logs, nameof(UniversalEvent.ContextTypes.ProcessingLog));
		}

		static UniversalEvent GetSuccessfulImportEvent(ITopLevelDataObject exportedData, IImportResult importResult, IGlbCompany currentCompany)
		{
			return GetUniversalEvent(Events.DataImport, exportedData, currentCompany, new IEntityID[1] { importResult }, importResult.Logs, nameof(UniversalEvent.ContextTypes.ProcessingLog));
		}

		static UniversalEvent GetFailedImportEvent(ITopLevelDataObject exportedData, IImportResult importResult, IGlbCompany currentCompany)
		{
			return GetUniversalEvent(Events.DataImportFailure, exportedData, currentCompany, new IEntityID[1] { importResult }, importResult.Logs, nameof(UniversalEvent.ContextTypes.FailureReason));
		}

		static UniversalEvent GetUniversalEvent(ZArchitecture.Business.Event eventType, ITopLevelDataObject exportedData, IGlbCompany currentCompany, IEnumerable<IEntityID> entityIDs, IEnumerable<ISimpleLog> logs, string contextType)
		{
			var universalEvent = new UniversalEvent();
			universalEvent.EventType = eventType.Code;
			universalEvent.DataContext = DataContextFactory.New();
			universalEvent.DataContext.SetCompanyAndDataProviderDetails(currentCompany);
			universalEvent.AddDataTargetToUniversalEvent(exportedData);

			foreach (var entityID in entityIDs.Where(id => !(id is IEntityIDWithNullableContext nullable) || nullable.NullableDataContextType != null))
			{
				universalEvent.DataContext.AddDataSource(entityID.DataContextType, entityID.DataContextKey);
			}

			universalEvent.ContextCollection = new List<Context>();
			foreach (var context in (exportedData as UniversalEvent)?.ContextCollection ?? new List<Context>())
			{
				universalEvent.ContextCollection.Add(context);
			}

			universalEvent.ContextCollection.Add(Context.GetContextFromLogs(contextType, logs));

			return universalEvent;
		}

		public static void AddDataTargetToUniversalEvent(this UniversalEvent deliveryEvent, ITopLevelDataObject exportedData)
		{
			if (exportedData?.DataContext?.DataSourceCollection == null)
			{
				return;
			}

			foreach (var dataSource in exportedData.DataContext.DataSourceCollection)
			{
				if (Enum.TryParse(dataSource?.Type, out DataContextType dataContextType))
				{
					var context = deliveryEvent.DataContext ?? (deliveryEvent.DataContext = DataContextFactory.New());
					context.AddDataTarget(dataContextType, dataSource.Key);
				}
			}
		}

		public static void SetCodesMappedToTarget<T>(this T dataObject)
			where T : ITopLevelDataObject
		{
			var dataContext = dataObject.DataContext;
			if (dataContext != null)
			{
				var enterpriseServerAndCompanyID = dataContext.GetEnterpriseServerAndCompanyIDs();
				var companyCode = enterpriseServerAndCompanyID.CompanyCode;
				if (!companyCode.IsEmpty)
				{
					var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
					if (enterpriseServerAndCompanyID.EnterpriseID == registrationKey.EnterpriseCode
						&& enterpriseServerAndCompanyID.ServerID == registrationKey.ServerCode)
					{
						dataContext.CodesMappedToTarget = true;
					}
				}
			}
		}

		public static BusinessObject GetLoadedJobFromDataContextType(this IDataSourceDataObject dataSource, ITopLevelDataObject topLevelDataObject, BusinessObjectFactory factory)
		{
			if (dataSource != null)
			{
				DataContextType dataContextType;
				if (Enum.TryParse(dataSource.Type, true, out dataContextType))
				{
					var dataContextManager = dataContextType.GetUniversalDataContextManager();
					if (dataContextManager != null)
					{
						var dataTarget = DataContextFactory.NewDataTarget();
						dataTarget.Type = dataContextType.ToString();
						dataTarget.Key = dataSource.Key;
						return dataContextManager.LoadBusinessObjectFromDataTarget(topLevelDataObject, dataTarget, factory, new DummyLogger());
					}
				}
			}

			return null;
		}

		public static bool HasMatchingDataTarget(this ITopLevelDataObject topLevelDataObject, DataContextType dataContextType)
		{
			return topLevelDataObject.GetMatchingDataTarget(dataContextType) != null;
		}

		public static IDataTargetDataObject GetMatchingDataTarget(this ITopLevelDataObject topLevelDataObject, DataContextType dataContextType)
		{
			return topLevelDataObject != null ? topLevelDataObject.DataContext.GetMatchingDataTarget(dataContextType) : null;
		}

		public static IDataTargetDataObject GetMatchingDataTarget(this IDataContextDataObject dataContext, DataContextType dataContextType)
		{
			return dataContext.GetMatchingDataTargets(dataContextType).FirstOrDefault();
		}

		static IEnumerable<IDataTargetDataObject> GetMatchingDataTargets(this IDataContextDataObject dataContext, DataContextType dataContextType)
		{
			IEnumerable<IDataTargetDataObject> result = null;

			if (dataContext != null)
			{
				var dataTargets = dataContext.DataTargetCollection;
				if (dataTargets != null)
				{
					result = dataTargets.Where(dataTarget => (dataTarget != null && dataTarget.Type.GetValueOrDefault().EqualsIgnoringCase(dataContextType.ToString())));
				}
			}

			return result ?? Array.Empty<IDataTargetDataObject>();
		}

		public static IDataSourceDataObject GetMatchingDataSource(this ITopLevelDataObject topLevelDataObject, DataContextType dataContextType)
		{
			return topLevelDataObject.GetMatchingDataSources(dataContextType).FirstOrDefault();
		}

		public static IDataSourceDataObject GetMatchingDataSource(this IDataContextDataObject dataContext, DataContextType dataContextType)
		{
			return dataContext.GetMatchingDataSources(dataContextType).FirstOrDefault();
		}

		public static IEnumerable<IDataSourceDataObject> GetMatchingDataSources(this ITopLevelDataObject topLevelDataObject, DataContextType dataContextType)
		{
			return topLevelDataObject != null ? topLevelDataObject.DataContext.GetMatchingDataSources(dataContextType) : Array.Empty<IDataSourceDataObject>();
		}

		public static ImportAction? GetImportAction(this ITopLevelDataObject topLevelDataObject) => topLevelDataObject?.DataContext?.Action;

		public static IEnumerable<IDataSourceDataObject> GetMatchingDataSources(this IDataContextDataObject dataContext, DataContextType dataContextType)
		{
			IEnumerable<IDataSourceDataObject> result = null;

			if (dataContext != null)
			{
				var dataSources = dataContext.DataSourceCollection;
				if (dataSources != null)
				{
					result = dataSources.Where(dataSource => (dataSource != null && dataSource.Type.GetValueOrDefault().EqualsIgnoringCase(dataContextType.ToString())));
				}
			}

			return result ?? Array.Empty<IDataSourceDataObject>();
		}

		public static bool IsFromSameSystem(this IDataContextDataObject dataContext)
		{
			if (dataContext != null)
			{
				var sender = dataContext.GetEnterpriseServerAndCompanyIDs();
				if (!sender.CompanyCode.IsEmpty)
				{
					var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
					var enterpriseCode = registrationKey.EnterpriseCode;
					var serverCode = registrationKey.ServerCode;

					return serverCode == sender.ServerID && enterpriseCode == sender.EnterpriseID;
				}
			}
			return false;
		}

		public static bool IsTransitDataSource(this ITopLevelDataObject topLevelDataObject)
		{
			if (GetMatchingDataSource(topLevelDataObject, DataContextType.TransitReceive) != null)
			{
				return true;
			}
			if (GetMatchingDataSource(topLevelDataObject, DataContextType.TransitDispatch) != null)
			{
				return true;
			}
			return false;
		}

		public static bool IsInternalImport(this IXmlImportLogger logger)
		{
			return (logger as IXmlSessionTracker).IsInternalImport();
		}

		public static bool IsInternalImport(this IXmlSessionTracker sessionTracker)
		{
			return sessionTracker != null && sessionTracker.OutboundSessionTracker != null;
		}

		public static void LogLinkCreated(this IXmlImportLogger logger, UniversalObjectFactory factory, IEntityID parentEntityID, IEntityID childEntityID)
		{
			var sessionTracker = logger as IXmlSessionTracker;
			if (sessionTracker != null)
			{
				// entities might not have their Job Numbers until after save.
				factory.AddPostSaveAction(() => sessionTracker.LogLinkCreated(parentEntityID, childEntityID));
			}
		}

		internal static IEnumerable<T> ConcatIgnoreNull<T>(this IEnumerable<T> seq, IEnumerable<T> oth)
		{
			return (oth != null ? seq.Concat(oth) : seq) ?? Enumerable.Empty<T>();
		}

		internal static IEnumerable<string> SelectKeysSafe<T>(this IEnumerable<T> seq, Func<T, IEnumerable<string>> getKeys)
		{
			return seq != null ? seq.WhereNotNull().SelectMany(getKeys) : Enumerable.Empty<string>();
		}

		internal static IEnumerable<string> SelectKeysSafe<T>(this T obj, Func<T, IEnumerable<string>> getKeys)
		{
			return obj != null ? getKeys(obj) : Enumerable.Empty<string>();
		}

		internal static IEnumerable<string> AsStringsIgnoringNull(params ZString?[] zStrings)
		{
			return zStrings.Where(s => s.HasValue).Select(s => (string)s.Value);
		}

		internal static string JoinIgnoringNull(params ZString?[] zStrings)
		{
			return string.Join("", AsStringsIgnoringNull(zStrings));
		}

		internal static IEnumerable<(string Value, string FieldName)> SelectKeysSafe<T>(
	this IEnumerable<T> seq, Func<T, IEnumerable<(string Value, string FieldName)>> getKeys)
		{
			return seq != null ? seq.WhereNotNull().SelectMany(getKeys) : Enumerable.Empty<(string, string)>();
		}

		internal static IEnumerable<(string Value, string FieldName)> SelectKeysSafe<T>(
			this T obj, Func<T, IEnumerable<(string Value, string FieldName)>> getKeys)
		{
			return obj != null ? getKeys(obj) : Enumerable.Empty<(string, string)>();
		}

		internal static IEnumerable<(string Value, string FieldName)> AsStringsIgnoringNull(
			params (ZString? Value, string FieldName)[] zStrings)
		{
			return zStrings
				.Where(s => s.Value.HasValue)
				.Select(s => ((string)s.Value.Value, s.FieldName));
		}

		internal static IEnumerable<(string Value, string FieldName)> SelectNonNull<T>(
	this IEnumerable<T> items, Func<T, (ZString? Value, string FieldName)> selector)
		{
			foreach (var item in items)
			{
				if (item != null)
				{
					var (property, fieldName) = selector(item);
					if (property.HasValue)
					{
						yield return ((string)property.Value, fieldName);
					}
				}
			}
		}

		internal static IEnumerable<string> SelectNonNull<T>(this IEnumerable<T> items, Func<T, ZString?> selector)
		{
			foreach (var item in items)
			{
				if (item != null)
				{
					var property = selector(item);
					if (property.HasValue)
					{
						yield return property.Value;
					}
				}
			}
		}

		internal static ZString? CleanMasterBill(this ZString? masterBill)
		{
			return masterBill.HasValue ? masterBill.Value.Replace("-", "") : masterBill;
		}

		public static void ReportActiveState(this (bool active, IGlbDepartment department) departmentResult, IXmlImportLogger logger, string message, out bool failProcess)
		{
			if (!departmentResult.active)
			{
				var logType = eAdaptorRegistry.Instance.UniversalXMLInactiveDepartmentFailsMessage.Value ? LogType.Error : LogType.Warning;
				logger.LogBoth(logType, message);
				failProcess = logType == LogType.Error;
			}
			else
			{
				failProcess = false;
			}
		}

		public class ParseData<T>
			where T : TopLevelDataObject, new()
		{
			public ParseData()
			{
				DataObject = new T();
				NamespaceUsed = null;
			}

			public ParseData(T dataObject, string namespaceUsed)
			{
				DataObject = dataObject;
				NamespaceUsed = namespaceUsed;
			}

			public T DataObject { get; private set; }
			public string NamespaceUsed { get; private set; }
		}
	}
}
