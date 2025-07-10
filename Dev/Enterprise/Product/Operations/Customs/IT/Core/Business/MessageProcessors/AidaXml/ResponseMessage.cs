using System;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageDefinitions;
using CargoWise.Types;
using ResponseStatusCodes = Enterprise.Customs.IT.Business.Ucc6AcknowledgementStatusList.Codes;

namespace Enterprise.Customs.IT.Business;

public abstract class ResponseMessage<TResponseBody> : IResponseMessage<TResponseBody>
	where TResponseBody : class
{
	protected ResponseMessage(ZString message)
	{
		Message = Argument.NotNullOrEmpty(message, nameof(message));
		responseBodyLazy = new Lazy<TResponseBody>(GetResponseBody);
		responseStatusLazy = new Lazy<string>(GetMessageStatus);
	}

	public TResponseBody ResponseBody => responseBodyLazy.Value;

	public string ResponseStatus => responseStatusLazy.Value;

	internal bool IsElaborationKO
		=> ResponseStatus is ResponseStatusCodes.ElaborationKoWithoutResult or ResponseStatusCodes.ElaborationKoWithResult;

	internal bool IsServiceNotAvailable
		=> ResponseStatus is ResponseStatusCodes.ServiceNotAvailable;

	TResponseBody GetResponseBody()
	{
		ISoapMessageBodyDeserializer<TResponseBody> bodyDeserializer = new SoapMessageBodyDeserializer<TResponseBody>(ResponseBodyTypeNamespace);
		return bodyDeserializer.GetDeserializedBody(Message, GetXmlOverridesCreationFactory());
	}

	protected string Message { get; }

	protected abstract IXmlOverridesCreationFactory GetXmlOverridesCreationFactory();

	protected virtual string ResponseBodyTypeNamespace { get; }

	protected abstract string GetMessageStatus();

	readonly Lazy<TResponseBody> responseBodyLazy;

	readonly Lazy<string> responseStatusLazy;
}

public abstract class ResponseMessage<TResponseBody, TData> : ResponseMessage<TResponseBody>
	where TResponseBody : class, IResponseDataProvider
	where TData : class
{
	protected ResponseMessage(ZString message) : base(message)
	{
		dataLazy = new Lazy<TData>(GetData);
	}

	public TData Data => dataLazy.Value;

	TData GetData()
	{
		var dataString = ResponseBody.DataString;
		if (!string.IsNullOrWhiteSpace(dataString))
		{
			var processedDataString = ProcessDataStringBeforeSerialization(dataString);
			return new ZString(processedDataString).DeserializeToObject<TData>();
		}

		return null;
	}

	protected virtual string ProcessDataStringBeforeSerialization(string dataString) => dataString;

	readonly Lazy<TData> dataLazy;
}
