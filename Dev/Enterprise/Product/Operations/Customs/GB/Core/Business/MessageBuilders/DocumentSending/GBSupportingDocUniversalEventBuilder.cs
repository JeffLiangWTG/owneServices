using System.Collections.Generic;
using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.GB.Business.MessageBuilders.DocumentSending
{
	public class GBSupportingDocUniversalEventBuilder : SupportingDocUniversalEventBuilder
	{
		public GBSupportingDocUniversalEventBuilder(ISupportingDocumentMessageDataProvider dataWrapper) : base(dataWrapper)
		{
		}

		public override IEnumerable<UniversalEvent> BuildUniversalEvent(ISupportingDocumentMessageDataProvider[] sendingObjects)
		{
			return sendingObjects.Select(sendingObject => CreateUniversalEvent(DataWrapper, sendingObject));
		}

		UniversalEvent CreateUniversalEvent(ISupportingDocumentMessageDataProvider dataWrapper, ISupportingDocumentMessageDataProvider sendingObject)
		{
			var universalEvent = new UniversalEvent
			{
				EventTime = ZDateTimeOffset.Now,
				EventType = Events.DocumentSentCode,
				EventReference = CreateEventReference(),
				DataContext = CreateDataContext(dataWrapper),
				ContextCollection = new List<Context>
				{
					new Context
					{
						Type = Customs.Common.CusEntryNumberTypes.Standard.MovementReferenceNumber,
						Value = dataWrapper.LocalReferenceNumber
					},
					new Context
					{
						Type = Key,
						Value = GetCredentialKey(dataWrapper)
					}
				},
				AttachedDocumentCollection = new List<AttachedDocument>
				{
					CreateAttachedDocument(sendingObject)
				}
			};

			return universalEvent;
		}

		protected virtual ZString GetCredentialKey(ISupportingDocumentMessageDataProvider dataWrapper)
		{
			var declaration = dataWrapper.BusinessObject as JobDeclaration;
			return declaration?.GetCredentialsKey() ?? ZString.Empty;
		}

		static ZString CreateEventReference()
		{
			var parameters = new Dictionary<string, string>
			{
				{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, DocUpload },
				{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Service, GBCustomsCDS }
			};
			return StmALog.GenerateEventReference(ZString.Empty, parameters);
		}

		static IDataContextDataObject CreateDataContext(ISupportingDocumentMessageDataProvider dataWrapper)
		{
			var dataContext = DataContextFactory.New(UniversalXmlInfo.Namespace_2011_11);
			dataContext.AddDataSource(dataWrapper.ContextType, dataWrapper.ContextReference);
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			return dataContext;
		}

		static AttachedDocument CreateAttachedDocument(ISupportingDocumentMessageDataProvider sendingObject)
		{
			var document = sendingObject.Document;
			var fileName = document.FileName;

			var attachedDocument = new AttachedDocument
			{
				ImageData = document.GetImageDataReader().CopyToSubStreamableStreamAndCloseStream(),
				FileName = fileName,
				ContextCollection = new List<Context>
				{
					new Context
					{
						Type = DocType,
						Value = sendingObject.DocumentType
					},
					new Context
					{
						Type = Mime,
						Value = GetMimeType(fileName)
					}
				}
			};
			return attachedDocument;
		}

		static string GetMimeType(string fileName)
		{
#if NET48
			return System.Web.MimeMapping.GetMimeMapping(fileName);
#else
			return MimeKit.MimeTypes.GetMimeType(fileName);
#endif
		}

		const string Mime = "MIME";
		const string DocType = "DOCTYPE";
		const string DocUpload = "DOCUPLOAD";
		const string GBCustomsCDS = "GBCustomsCDS";
		const string Key = "Key";
	}
}
