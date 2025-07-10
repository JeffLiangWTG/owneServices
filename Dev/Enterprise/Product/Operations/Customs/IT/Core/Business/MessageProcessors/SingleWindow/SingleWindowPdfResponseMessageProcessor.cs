using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.Extensions;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.IT.Business.MessageProcessorConstants.InterchangeTypes;

namespace Enterprise.Customs.IT.Business;

public class SingleWindowPdfResponseMessageProcessor : SingleWindowIncomingMessageProcessor<ISingleWindowPdfCustomsResponse>
{
	public SingleWindowPdfResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"Single Window PDF Response Message";

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { SingleWindowPdfResponseMessageType };

	protected override void ProcessSingleWindowMessage(ISingleWindowCustomsLinkedObjectAdapter entryAdapter, ISingleWindowPdfCustomsResponse customsResponse)
	{
		UploadPdfToEdocs(entryAdapter, customsResponse);
		AddStatusUpdatedLog(entryAdapter, StatusUpdatedLogTypePdf);
	}

	protected override ISingleWindowPdfCustomsResponse LoadCustomsResponseCore(ZString messageText) => SingleWindowPdfCustomsResponse.Load(messageText);

	void UploadPdfToEdocs(ISingleWindowCustomsLinkedObjectAdapter entryAdapter, ISingleWindowPdfCustomsResponse customsResponse)
	{
		var docManagerInfo = entryAdapter.DocManagerInfo;
		docManagerInfo.ForceToUseAnotherFactory(entryAdapter.Factory);
		docManagerInfo.AddFileOrDocument(customsResponse.ImageData.ToArray(), customsResponse.Filename, customsResponse.DocumentType);
	}

	const string StatusUpdatedLogTypePdf = "PDF";
}

public class SingleWindowPdfCustomsResponse : ISingleWindowPdfCustomsResponse
{
	SingleWindowPdfCustomsResponse(ZString documentType, ZString filename, byte[] imageData)
	{
		this.documentType = Argument.NotNullOrEmpty(documentType, nameof(documentType));
		this.filename = Argument.NotNullOrEmpty(filename, nameof(filename));
		this.imageData = CheckImageData(imageData);
	}

	readonly ZString documentType;
	readonly ZString filename;
	readonly byte[] imageData;

	public static ISingleWindowPdfCustomsResponse Load(ZString messageText)
	{
		return new SingleWindowPdfCustomsResponse(GetValue((NoResString)"Code"), GetValue((NoResString)"Filename"), Convert.FromBase64String(GetValue("ImageData")));

		ZString GetValue(ZString tag) => MessageProcessorHelper.RetrieveValueOfXmlNode(messageText, tag, true);
	}

	ZString ISingleWindowPdfCustomsResponse.DocumentType => documentType;

	ZString ISingleWindowPdfCustomsResponse.Filename => filename;

	ReadOnlyMemory<byte> ISingleWindowPdfCustomsResponse.ImageData => imageData;

	byte[] CheckImageData(byte[] imageData)
	{
		var result = Argument.NotNull(imageData, nameof(imageData));
		if (result.Length == 0)
		{
			throw new ArgumentException($"{nameof(imageData)} cannot be empty");
		}
		return result;
	}
}
