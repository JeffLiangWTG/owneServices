using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Customs.CH.MessageDefinitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CH.Business;

public class DocumentMessageProcessor : BaseXmlInboundMessageProcessor<AttachedDocument>
{
	public DocumentMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	const string DocSource = "CUS";

	protected override string MessageFriendlyNameCore => Res.GetString("FF2F0CC9-0A70-4C40-A8A7-61085A6688C3", "Document Message Processor");

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeCodeList.Codes.Import, MessageTypeCodeList.Codes.Export };

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.Document };

	protected override BusinessObject FindLinkedObject(EDIMessage message, AttachedDocument xmlObject) => FindLinkedObjectByReference(message, xmlObject);

	protected override ZString GetEntryHeaderReference(AttachedDocument attachedDocument)
	{
		return Path.GetFileNameWithoutExtension(attachedDocument?.FileName).Split('_').ElementAtOrDefault(3) ?? string.Empty;
	}

	protected override void ProcessResponseMessage(CHEDIMessage message, AttachedDocument attachedDocument)
	{
		if (attachedDocument != null && message.EM_LinkedObject is CusEntryHeader entryHeader)
		{
			var docManagerInfo = (entryHeader as IDocManagerSupport).DocManagerInfo;
			if (!docManagerInfo.AllEDocs.Cast<IeDoc>().Any(d => !d.IsDeleted && (d.FileName == attachedDocument.FileName || GetFileNameWithoutSuffix(d.FileName) == attachedDocument.FileName)))
			{
				docManagerInfo.ForceToUseAnotherFactory(entryHeader.Factory);
				var doc = docManagerInfo.AddFileOrDocument(attachedDocument.ImageData, attachedDocument.FileName, attachedDocument.Type?.Code);
				doc.DocSource = DocSource;
			}
		}

		string GetFileNameWithoutSuffix(string fileName)
		{
			var fileNameOnly = Path.GetFileNameWithoutExtension(fileName);
			if (fileNameOnly.EndsWith("]") && fileNameOnly.Contains("["))
			{
				fileNameOnly = fileNameOnly.Substring(0, fileNameOnly.LastIndexOf("["));
			}
			return fileNameOnly + Path.GetExtension(fileName);
		}
	}
}
