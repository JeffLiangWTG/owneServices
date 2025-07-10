using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentScanning.DataTransfer.Universal
{
	public class UniversalDocumentRequestHandler : UniversalXmlRequestBaseHandler<DocumentRequest>
	{
		public UniversalDocumentRequestHandler(IXmlSessionTracker xmlSessionTracker) : this(xmlSessionTracker, new DefaultProcessingConfig())
		{
		}

		public UniversalDocumentRequestHandler(IXmlSessionTracker xmlSessionTracker, IHttpXmlProcessingConfig processingConfig) : base(xmlSessionTracker, processingConfig)
		{
		}

		protected override string RequestMessageSubType => EDIMessageSubTypeList.Codes.XmlUniversalDocumentRequest;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "part of error log")]
		protected override (IDataObject, bool) GetRequestedDataObject(DocumentRequest documentRequest, BusinessObject dataProvider, IXmlSessionTracker xmlSessionTracker)
		{
			ITopLevelDataObject result = null;
			var recipientRoles = new List<RecipientRoleType>();
			if (documentRequest.DataContext.RecipientRoleCollection != null)
			{
				recipientRoles.AddRange(documentRequest.DataContext.RecipientRoleCollection.Where(x => x.Code.HasValue).Select(x => x.Code.GetValueOrDefault()));
			}
			var eventDataContext = dataProvider.GetUniversalDataContextManager() as IEventDataContextManager ?? new DocManagerDataContextManager();
			using (eventDataContext is IDataContextManagerForManyTypes manager ? dataProvider.SetUniversalDataContextManagerForManyTypes(manager) : null)
			{
				var actionInfo = new ActionInfo(recipientRoles.ToRecipientRoleDetails(), dataProvider);
				var writingManager = new DataWritingManager(actionInfo);

				var source = documentRequest.DataContext.DataSourceCollection?.FirstOrDefault();
				var sourceDescription = source != null && source.Type.HasValue && source.Key.HasValue ? source.Type + " " + source.Key : "Data source";

				var docManagerSupport = dataProvider as IDocManagerSupport;
				if (docManagerSupport?.DocManagerInfo == null)
				{
					xmlSessionTracker.Log(LogType.Error, sourceDescription + " was found, but it does not support eDocs.");
					return (null, false);
				}

				var logBizo = dataProvider.GetLogs()?.AddNew(Events.DataExport);
				result = eventDataContext.GetEventDataObjectWriter(writingManager).GetDataObject(logBizo);
				var eventBizo = result as Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
				if (eventBizo != null)
				{
					var docDataWriter = new AttachedDocumentDataObjectWriter();
					eventBizo.AttachedDocumentCollection = new List<AttachedDocument>();
					try
					{
						if (documentRequest.FilterCollection != null)
						{
							foreach (var filter in documentRequest.FilterCollection?.ToList())
							{
								if (filter.Type.Value == DocumentFilterType.SaveDateUTCFrom || filter.Type.Value == DocumentFilterType.SaveDateUTCTo)
								{
									if (ZDateTimeOffset.TryParse(filter.Value.Value, out var dto) && (filter.Value.Value.Contains("+" + ZDateTimeOffset.Now.Offset.ToString("hh':'mm")) || dto.Offset != ZDateTimeOffset.Now.Offset) && filter.Value.Value[filter.Value.Value.Length - 1] != 'Z')
									{
										xmlSessionTracker.Log(LogType.Warning, filter.Type.Value + ":Time Zone Offset Detected and Ignored.");
									}
								}
							}
						}
						var filteredEDocs = FilterDocuments(documentRequest, docManagerSupport.DocManagerInfo).ToArray();
						if (filteredEDocs.Length == 0)
						{
							xmlSessionTracker.Log(LogType.Warning, sourceDescription + " was found, but no eDocs were found matching the filters specified.");
							return (null, true);
						}
						eventBizo.AttachedDocumentCollection.AddRange(docDataWriter.GenerateAttachedDocuments(documentRequest.ReturnDocumentDescriptionsOnly.GetValueOrDefault(), filteredEDocs));
					}
					catch (DataObjectReadFailureException readFailureException)
					{
						xmlSessionTracker.Log(LogType.Error, readFailureException.Message);
						return (null, false);
					}
					catch (ExternalStorageException ex)
					{
						xmlSessionTracker.Log(LogType.Error, ex.UnableToAccessStorageFriendlyMessage);
						return (null, false);
					}
				}

				new DataContextDataObjectWriter().PopulateDataObject(actionInfo, dataProvider, result.DataContext, includeWorkflowInfo: false);
			}
			return (result, result != null);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Logging error message should not be translated.")]
		protected override string MultipleResultsErrorMessage => "The Universal Document Query interface can only return one UniversalDocument for each query. Multiple results were found using the references provided.";

		IEnumerable<IeDoc> FilterDocuments(DocumentRequest documentRequest, DocManagerInfo docMangerInfo)
		{
			IEnumerable<IeDoc> FindMatchedDocuments(IEnumerable<IeDoc> documents, List<DocumentFilterBase> filters)
			{
				return documents.Where(eDoc => !eDoc.IsDeleted && filters.All(f => f.IsMatch(eDoc)));
			}

			var allDocumentFilters = DocumentFilterFactory.GetDocumentFilters(documentRequest.FilterCollection)?.ToList() ?? new List<DocumentFilterBase>();
			var nonRelatedEDocFilters = allDocumentFilters.Where(x => !(x is DocumentFilterRelatedEDoc)).ToList();
			var result = FindMatchedDocuments(docMangerInfo.AllEDocs.Cast<IeDoc>(), nonRelatedEDocFilters).ToList();
			if (allDocumentFilters.Count != nonRelatedEDocFilters.Count)
			{
				result.AddRange(FindMatchedDocuments(docMangerInfo.GetRelatedEDocs().Except(result), allDocumentFilters));
			}

			return result;
		}
	}
}
