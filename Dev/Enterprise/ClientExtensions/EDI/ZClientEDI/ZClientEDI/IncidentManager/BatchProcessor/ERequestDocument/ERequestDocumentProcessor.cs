using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.CustomerService.Business;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.BatchProcessor
{
	internal class ERequestDocumentProcessor
	{
		readonly ILogger ServiceLogger;

		public ERequestDocumentProcessor(ILogger serviceLogger)
		{
			ServiceLogger = serviceLogger;
		}

		public void Process(ERequestDocument requestDoc)
		{
			if (ZGuid.TryParse(requestDoc.ReferenceId, out var referenceIdAsZGuid))
			{
				ServiceLogger?.Information(FormattableString.Invariant($"Processing ERequestDocument({requestDoc.ReferenceId})..."));

				var query = new ZQuery(IncidentRequestSchema.INC_ReferenceID, referenceIdAsZGuid);
				query.OrderBy = IncidentRequestSchema.Constants.INC_SystemCreateTimeUtc;

				var mainFactory = new BusinessObjectFactory();
				var incidentRequest = mainFactory.LoadTop1<IncidentRequest>(query);

				if (AddDocuments(mainFactory, referenceIdAsZGuid, incidentRequest, requestDoc))
				{
					try
					{
						if (incidentRequest != null)
						{
							BusinessObjectFactory.SaveTogether(mainFactory, incidentRequest.DocManagerInfo.MasterFactory);
						}
						else
						{
							mainFactory.Save();
						}
					}
					catch (ZSaveException ex) when (ex is ZSaveConcurrencyException
						|| (ex.InnerException?.InnerException is SqlException sqlException && new DbErrorMatch(sqlException).ExceptionType == DbErrorType.CannotInsertDuplicateUniqueIndexKey))
					{
						var failsafeFactory = new BusinessObjectFactory();
						AddDocuments(failsafeFactory, referenceIdAsZGuid, null, requestDoc);
						failsafeFactory.Save();
					}
				}

				ServiceLogger?.Information(FormattableString.Invariant($"ERequestDocument({requestDoc.ReferenceId}) processed successfully."));
			}
		}

		static bool AddDocuments(BusinessObjectFactory factory, ZGuid referenceId, IncidentRequest incidentRequest, ERequestDocument requestDoc)
		{
			var shouldSave = false;

			foreach (ERequestDocumentAttachment attachment in requestDoc.Attachments)
			{
				shouldSave |= AddDocument(factory, referenceId, incidentRequest, attachment.FileName, attachment.Data, attachment.IsPublished);
			}

			return shouldSave;
		}

		static bool AddDocument(BusinessObjectFactory factory, ZGuid referenceId, IncidentRequest incidentRequest, string fileName, byte[] contents, bool isPublished = false)
		{
			var shouldSave = false;

			if (contents != null && contents.Any())
			{
				if (incidentRequest != null)
				{
					var attachedFile = incidentRequest.DocManagerInfo.AddFileOrDocument(contents, fileName, "COR");
					attachedFile.IsPublished = isPublished;
					shouldSave = true;
				}
				else
				{
					var docQueue = factory.New<EdiERequestDocumentQueue>();
					docQueue.EDQ_INC_ReferenceID = referenceId;
					docQueue.EDQ_FileName = fileName;
					docQueue.EDQ_Data = contents;
					docQueue.EDQ_IsPublished = isPublished;
					shouldSave = true;
				}
			}

			return shouldSave;
		}
	}
}
