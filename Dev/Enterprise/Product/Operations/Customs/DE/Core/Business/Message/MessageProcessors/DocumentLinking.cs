using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DE.Business
{
	[SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
	public class DocumentLinking : HashSet<IDocManagerSupportBase>
	{
		readonly IXmlImportLogger logger;

		public DocumentLinking(LoggingInformation logger)
		{
			this.logger = new DEXmlImportLogger(logger);
		}

		public void Subscribe(BusinessObject businessObject)
		{
			if (businessObject is IDocManagerSupportBase docManagerSupport)
			{
				Add(docManagerSupport);
			}
		}

		public void DoLink(List<AttachedDocument> attachments)
		{
			if (attachments?.Any() ?? false)
			{
				foreach (var docManagerSupport in this)
				{
					var eDocsReader = ObjectFactory.New<IAttachedDocumentDataObjectReader>();
					foreach (var attachedDocument in attachments)
					{
						eDocsReader.TryAddAttachedDocument(attachedDocument, logger, docManagerSupport, out IeDoc _);
					}
				}
			}
		}
	}

	class DEXmlImportLogger : IXmlImportLogger
	{
		readonly LoggingInformation logger;

		public DEXmlImportLogger(LoggingInformation logger)
		{
			this.logger = logger;
		}

		bool IXmlImportLogger.IsUpdatingConsol { get; set; }

		bool IXmlImportLogger.HasIgnoredModule { get; set; }

		bool IXmlImportLogger.OrgMatchingDisabled => false;

		IEnumerable<IValidationRule> IXmlImportLogger.ValidationRuleCollection { get; set; }

		ITopLevelDataObject IXmlImportLogger.TopLevelDataObject => null;

		IDataContextDataObject IXmlImportLogger.TopLevelDataContext => null;

		IEnumerable<ISimpleLog> ISimpleLogResult.Logs => logger?.Logs ?? Enumerable.Empty<ISimpleLog>();

		void IXmlImportLogger.FireDataImportedToBusinessObject(BusinessObject targetBO)
		{
		}

		void ISimpleLogger.Log(LogType type, string message)
		{
			logger?.Log(type, message);
		}

		void IXmlImportLogger.LogBoth(LogType type, string message)
		{
		}

		void IXmlImportLogger.LogErrorToServiceTaskOnly(string message)
		{
		}

		void IXmlImportLogger.LogTopLevelDataContextKey(GetDataContextKey getDataContextKey)
		{
		}
	}
}
