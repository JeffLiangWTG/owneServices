using System.Xml;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageDefinitions.SingleWindow;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

public class ElectronicFolderUpdatesRequester
{
	public ElectronicFolderUpdatesRequester(ISingleWindowRequestDataProvider dataProvider, BusinessObjectFactory factory)
	{
		this.dataProvider = Argument.NotNull(dataProvider, nameof(ISingleWindowRequestDataProvider), "data Provider cannot be null");
		this.factory = Argument.NotNull(factory, nameof(factory), "Factory cannot be null");
	}

	public ITEDIMessage RequestUpdates()
	{
		var requestMessage = factory.New<ITEDIMessage>();
		var updatesRequest = GetRequestObject();

		requestMessage.EM_ApplicationCode = Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.ITCustoms;
		requestMessage.EM_MessageType = MessageProcessorConstants.InterchangeTypes.SingleWindowRequest;
		requestMessage.EM_MessageSubType = MessageProcessorConstants.InterchangeTypes.SingleWindowRequest;
		requestMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		requestMessage.EM_ApplicationReference = dataProvider.ApplicationReference;
		requestMessage.EM_Status = EDIMessage.Status.Queued;
		requestMessage.MessageNumberStrategy = new FixedMessageNumberStrategy(ZString.Empty);
		requestMessage.EM_LinkTable = dataProvider.TableName;
		requestMessage.EM_LinkUniqueID = dataProvider.PK;

		var xmlWriterSettings = new XmlWriterSettings()
		{
			OmitXmlDeclaration = true,
			Encoding = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
		};
		requestMessage.EM_MessageText = updatesRequest.SerializeToXml(xmlWriterSettings);

		return requestMessage;
	}

	richiesta_esito GetRequestObject()
	{
		richiesta_esito updatesRequest = new richiesta_esito();
		updatesRequest.dichiarazione = new dichiarazione();

		var issueDate = dataProvider.IssueDate;
		updatesRequest.dichiarazione.anno_reg = !issueDate.IsEmpty ? issueDate.ToCustomsDateString().ToString() : string.Empty;
		updatesRequest.dichiarazione.cod_reg = dataProvider.RegisterIncludingSeries;
		updatesRequest.dichiarazione.cod_uff_dog = dataProvider.CustomsOffice.Right(6);
		updatesRequest.dichiarazione.num_reg = dataProvider.RegistrationNumberWithoutCin;
		return updatesRequest;
	}

	readonly ISingleWindowRequestDataProvider dataProvider;
	readonly BusinessObjectFactory factory;
}
