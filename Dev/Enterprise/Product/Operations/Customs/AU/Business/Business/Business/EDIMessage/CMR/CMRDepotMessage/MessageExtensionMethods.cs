using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class MessageExtensionMethods
	{
		public static EmailDef ConstructUnmatchedContainersEmail(this ICMRDepotMessage message, Dictionary<string, List<CARSTRecord>> carstRecords, BusinessObjectFactory factory, string messageFrendlyName)
		{
			string subject = "One, or more, Inconsistent Shipment/Consol/Containers, so cannot set container on shipment pack line.";
			EmailDefBuilder emailBuilder = new EmailDefBuilder(subject, EmailDefBuilder.HtmlTemplates.FreeFormResponse);
			var vessel = RefVessel.LookupVesselByLloyds(message.LloydsNumber, factory);
			string report = string.Format(@"A status message has been received from Customs with the details shown below.
One, or more, shipment have been found that match the vessel/voyage and a line house bill, however the
Container mentioned on the inbound status message line is inconsistent with the Consol of the shipment,
i.e. the Container has been registered on another Consol.

Inbound status message header details:
Vessel: {0} ({1})
Voyage: {2}
Origin Premise: {3}
Destination Premis: {4}

", vessel == null ? ZString.Empty : vessel.RV_Code, message.LloydsNumber, message.VoyageNumber, message.OriginPremiseID, message.DestinationPremiseID);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, report.Replace("\r\n", "<br>"));

			var creator = new HtmlTableCreator(new[] { "Master", "House", "Shipment", "Related Consol", "Container", "Container Consol" });
			foreach (var pair in carstRecords)
			{
				foreach (var carstRecord in pair.Value)
				{
					creator.WriteRow(carstRecord.Line != null ? carstRecord.Line.OceanBillNumber : ZString.Empty,
						carstRecord.Line != null ? carstRecord.Line.HouseBillNumber : ZString.Empty,
						carstRecord.Shipment != null ? carstRecord.Shipment.JS_UniqueConsignRef : ZString.Empty,
						carstRecord.Consol != null ? carstRecord.Consol.JK_UniqueConsignRef : ZString.Empty,
						carstRecord.Line != null ? carstRecord.Line.ContainerNumber : ZString.Empty,
						carstRecord.Container != null && carstRecord.Container.Consol != null ? carstRecord.Container.Consol.JK_UniqueConsignRef : ZString.Empty);
				}
			}
			emailBuilder.AddArgReplacement(messageFrendlyName);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml2, creator.ToHtml());
			return emailBuilder.ToEmail();
		}

		public static void AddUnmatchedContainer(this ICMRDepotMessage message, CARSTRecord carstRecord, Dictionary<string, List<CARSTRecord>> carstRecords)
		{
			List<CARSTRecord> recordsForContainer;

			var key = carstRecord.Container.PK.ToStringKey();
			if (!carstRecords.TryGetValue(key, out recordsForContainer))
			{
				recordsForContainer = new List<CARSTRecord>();
				carstRecords.Add(key, recordsForContainer);
			}

			if (recordsForContainer.Find(x => x.Shipment == carstRecord.Shipment) == null)
			{
				recordsForContainer.Add(carstRecord);
			}
		}

		public static EmailDef ConstructUnmatchedHouseEmail(this CMRCARSTMessage message, string messageFrendlyName)
		{
			string subject = "Unidentified consignment status message received.";
			var emailBuilder = new EmailDefBuilder(subject, EmailDefBuilder.HtmlTemplates.FreeFormResponse);
			string report = string.Format(@"A status message has been received from Customs but a matching house bill could not be found.
An Air Cargo job has been identified for MAWB '{0}', but no house bill with number '{1}' could be found.
The message has been attached to the master.", message.MAWB, message.HAWB);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, report.Replace("\r\n", "<br>"));
			emailBuilder.AddArgReplacement(messageFrendlyName);
			return emailBuilder.ToEmail();
		}
	}
}
