using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.EServices.Schemas.EsCustoms.DocumentRequest;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.Business
{
	public abstract class CommonDocumentRequest<BO> where BO : BusinessObject
	{
		public CommonDocumentRequest(BO cusBusinessObject, ZString certName)
		{
			businessObject = Argument.NotNull(cusBusinessObject, nameof(cusBusinessObject));
			certificateName = Argument.NotNullOrEmpty(certName, nameof(certName));

			csvClearance = GetCSVReference(businessObject);

			var docManagerInfo = ((IDocManagerSupport)businessObject).DocManagerInfo;
			eDocs = docManagerInfo.GetRelatedEDocs();

			boFactory = businessObject.Factory;
		}

		protected readonly BO businessObject;
		protected readonly BusinessObjectFactory boFactory;
		readonly ZString certificateName;

		protected readonly ZString csvClearance;
		protected ZString mrnCode;

		readonly IEnumerable<IeDoc> eDocs;

		const string PFDFileNameEnding = ".pdf";

		public ZInt RequestMissingDocuments() => RequestMissingDocumentsCore();

		protected abstract ZInt RequestMissingDocumentsCore();

		protected ZBool IsDocumentMissing(ZString documentName)
		{
			var docMissing = !eDocs.Any(x => x.FileName.EqualsIgnoringCase(documentName + PFDFileNameEnding));

			return docMissing && NoRequestPending(documentName);
		}

		protected ZBool NoRequestPending(ZString documentName)
		{
			var sentMessagesForDocName = GetMessages(businessObject).GetMatchingMessages("ESC", new ZString[] { "DOC" }, "TRX").Where(x => x.EM_MessageText.Contains(documentName));

			return sentMessagesForDocName.IsNullOrEmpty() || sentMessagesForDocName.All(x => x.EM_Status == EDIMessage.Status.Sent || x.EM_Status == EDIMessage.Status.Failed);
		}

		protected void CreateRequest(BusinessObjectFactory factory, ZString documentName, ZString urlParamenter)
		{
			var messageToSend = CreateEDIMessageForDocumentRequest(factory, documentName + PFDFileNameEnding, urlParamenter);
			messageToSend.EM_LinkedObject = businessObject;
		}

		ESEDIMessage CreateEDIMessageForDocumentRequest(BusinessObjectFactory factory, ZString documentName, ZString urlParameter)
		{
			var messageToSend = factory.New<ESEDIMessage>();
			messageToSend.EM_MessageType = DeclarationMessageTypeList.Codes.EsDocumentRequest;
			messageToSend.EM_MessageSubType = DeclarationMessageSubTypeList.Codes.EsDocumentRequest;
			messageToSend.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			messageToSend.EM_MessageText = GetDocumentRequestBodyText(documentName, urlParameter);
			messageToSend.EM_Status = EDIMessage.Status.Queued;
			messageToSend.EM_IsTestMessage = GetIsTrainingDeclaration();
			messageToSend.EM_ApplicationReference = certificateName;
			messageToSend.BusinessObjectReference = GetBGMReference(businessObject);
			messageToSend.EM_GP = GetCertificatePK();
			
			return messageToSend;
		}

		ZString GetDocumentRequestBodyText(ZString documentName, ZString urlParameter)
		{
			var documentBody = new EsCustoms()
			{
				File = new EsCustomsFile()
				{
					Name = documentName,
					UrlParameter = urlParameter
				}
			};
			return CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer.SerializeDefaultSettingsWithoutNamespaces(documentBody);
		}

		ZGuid GetCertificatePK()
		{
			var certificate = CertificateHelper.GetCertificate(GetBroker(), certificateName);
			return certificate?.PK ?? ZGuid.Empty;
		}

		protected abstract ZString GetBGMReference(BO businessObject);

		protected abstract ZString GetCSVReference(BO businessObject);

		protected abstract EDIMessageCollection GetMessages(BO businessObject);

		protected abstract ZBool GetIsTrainingDeclaration();

		protected abstract GlbStaff GetBroker();
	}
}
