using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.DataTransfer.EInvoicing.KoreaSouth
{
	public class KoreaDocTaxInvoiceFinder
	{
		public KoreaDocTaxInvoiceFinder(AccEInvoicingBatch batch)
		{
			Batch = Argument.NotNull(batch, nameof(AccEInvoicingBatch));
			UniversalEvent = GetUniversalEvent();
		}

		public AccEInvoicingBatch Batch { get; private set; }
		public UniversalEvent UniversalEvent { get; private set; }
		public XmlDocument XMLDocument { get; private set; }
		public ZString XMLStr { get; private set; }

		public void GetXml(string issueID)
		{
			XMLStr = null;
			XMLDocument = null;

			var contexts = UniversalEvent?.ContextCollection.Where
				(x => x.Type.Type.HasValue
					&& x.Type.Type.Equals(EInvoicingKoreaSouthConstants.DataContext.KoreaDocTaxInvoice)
					&& x.Value.HasValue
					&& !x.Value.Value.IsEmpty
				);
 
			if (!issueID.IsNullOrEmpty() && contexts != null)
			{
				foreach (var context in contexts)
				{
					var xmlBytes = Convert.FromBase64String(context.Value.Value);
					var xmlStr = MessageEncoding.UTF8WithoutBOM.GetString(xmlBytes);
					var xmlDocument = new XmlDocument();
					xmlDocument.LoadXml(xmlStr);

					var issueIDFormXML = XmlToAdditionalInfoConverter.GetIssueIDFromXML(xmlDocument);
					if (!issueIDFormXML.IsEmpty && issueIDFormXML.EqualsIgnoringCase(issueID))
					{
						XMLStr = xmlStr;
						XMLDocument = xmlDocument;
						return;
					}
				}
			}
		}

		UniversalEvent GetUniversalEvent()
		{
			var validEventTypes = new List<string> { Events.InterchangeAcknowledgedCode, Events.InterchangeRejectedCode };
			var interchangeAcknowledgedLogs = Batch.Logs.Find(l => validEventTypes.Contains(l.SL_SE_NKEvent));

			var log = interchangeAcknowledgedLogs.OrderBy(l => l.SL_EventTime)
				.LastOrDefault(l => l.SourceInfoItems.Any(x => x.Key.Replace(StmALog.SpaceDelimiter.ToString(), "") == EInvoicingKoreaSouthConstants.DataContext.KoreaDocTaxInvoiceCount));

			return log?.RelatedEDIMessage?.Message?.GetEM_MessageTextReader()?.Parse<UniversalEvent>();
		}
	}
}
