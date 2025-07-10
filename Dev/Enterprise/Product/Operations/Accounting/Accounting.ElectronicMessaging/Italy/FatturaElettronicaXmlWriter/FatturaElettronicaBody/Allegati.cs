using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Italy
{
	public class Allegati
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "it is the xml node name")]
		public XStreamingElement[] BuildXML(TransactionInfo transaction)
		{
			var result = new List<XStreamingElement>();
			if (transaction.AttachedDocumentCollection != null)
			{
				foreach (var attachedDocument in transaction.AttachedDocumentCollection)
				{
					var fileExtension = Path.GetExtension(attachedDocument.FileName);
					if (string.IsNullOrEmpty(fileExtension))
					{
						fileExtension = attachedDocument.Type?.Code ?? ZString.Empty;
					}
					else if (fileExtension.StartsWith(".", StringComparison.Ordinal))
					{
						fileExtension = fileExtension.Remove(0, 1);
					}
					result.Add(new XStreamingElement("Allegati",
						new XElement("NomeAttachment", attachedDocument.FileName?.Left(60).EnsureComplianceWithBasicLatinAndLatin1Supplement()),
						new XElement("FormatoAttachment", fileExtension.EnsureComplianceWithBasicLatin()),
						new XElement("DescrizioneAttachment", attachedDocument.Type?.Description?.Left(100).EnsureComplianceWithBasicLatinAndLatin1Supplement() ?? ZString.Empty),
						new XElement("Attachment", attachedDocument.ImageData?.ConvertToBase64StringAndLeaveStreamOpen())));
				}
			}
			return result.ToArray();
		}
	}
}
