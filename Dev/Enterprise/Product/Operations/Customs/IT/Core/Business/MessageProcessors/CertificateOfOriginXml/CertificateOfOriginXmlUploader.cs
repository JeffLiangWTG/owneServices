using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business;

public class CertificateOfOriginXmlUploader
{
	public CertificateOfOriginXmlUploader(CusEntryHeader entryHeader)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
	}

	readonly CusEntryHeader entryHeader;

	public void Upload(StreamReader streamReader)
	{
		Argument.NotNull(streamReader, nameof(streamReader));

		var fileContent = streamReader.ReadToEnd();
		var matchedType = ValidateFileAgainstRootTag(fileContent);
		var message = entryHeader.GetCertificateOfOriginMessage() ?? AddNewPrePopulatedMessage();
		message.EM_MessageSubType = xmlTagAndSubTypeCorrelations[matchedType];
		message.EM_MessageText = fileContent;
	}

	#region Implementation

	ITEDIMessage AddNewPrePopulatedMessage()
	{
		var newMessage = entryHeader.Messages.AddNew();
		newMessage.EM_MessageType = EDIMessageTypeList.Codes.CertificateOfOrigin;
		newMessage.IsTransmitMessage = false;
		newMessage.EM_Status = EDIMessageStatusList.Codes.Manual;
		return newMessage;
	}

	string ValidateFileAgainstRootTag(ZString content)
	{
		var regexMatch = Regex.Match(content, $"({string.Join("|", xmlTagAndSubTypeCorrelations.Keys)})");
		return regexMatch.Success ? regexMatch.Value : throw new NotSupportedException(Res.GetString("2B6A3388-88F6-4681-A076-EFA0F55F786A", "The uploaded file is not supported."));
	}

	readonly ImmutableDictionary<string, string> xmlTagAndSubTypeCorrelations = new Dictionary<string, string>()
	{
		{ "<CERTIFICATO_EUR1>", CertificateOfOriginMessageSubTypeList.Codes.Eur1Certificate },
		{ "<CERTIFICATO_ATR>", CertificateOfOriginMessageSubTypeList.Codes.AtrCertificate },
		{ "<CERTIFICATO_EURMED>", CertificateOfOriginMessageSubTypeList.Codes.EurMedCertificate }
	}.ToImmutableDictionary();

	#endregion
}
