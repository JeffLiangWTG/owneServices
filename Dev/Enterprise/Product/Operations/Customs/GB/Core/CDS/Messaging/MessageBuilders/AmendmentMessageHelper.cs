using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Customs.GB.MessageDefinitions.CDS;
using CargoWise.Customs.Shared.WorldCustomsOrganisation;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageSending;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.GB.CDS.Constants;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public class AmendmentMessageHelper : EU.Business.WorldCustomsOrganisation.MessageBuilders.AmendmentMessageHelper
	{
		public AmendmentMessageHelper(PointerParser pointerParser) : base(pointerParser)
		{
		}

		public static AmendmentMessageHelper Instance => amendmentMessageHelper.Value;
		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<AmendmentMessageHelper> amendmentMessageHelper = new Lazy<AmendmentMessageHelper>(() => new AmendmentMessageHelper(new PointerParser()));

		protected override List<XNamespace> NamespacesToKeep => new List<XNamespace>
																{ "urn:wco:datamodel:WCO:Declaration_DS:DMS:2" };

		protected override EDIMessage CreateNewMessageForComparison(WCOJobDeclarationMessageSendingObject objectToSend)
		{
			EDIMessage result = null;
			var entry = ((JobDeclarationMessageSendingObject)objectToSend).Header;

			if (!entry.IsNull)
			{
				var newMessageXML = new MessageBuilderManager().NewMessageBuilder((JobDeclarationMessageSendingObject)objectToSend, new EU.Business.ErrorCollector(), new Customs.Business.CusdecMessageFunction.New()).Build();
				newMessageXML = newMessageXML.Replace(CusEntryHeader.BGMReferencePlaceHolderXmlFriendly, entry.CH_BGMReference);
				newMessageXML = newMessageXML.Replace(CusEntryHeader.UCRReferencePlaceHolderXmlFriendly, entry.DeclarationUCR);
				newMessageXML = newMessageXML.Replace(CusEntryHeader.LRNReferencePlaceHolderXmlFriendly, entry.LRN);

				var newMessage = entry.Messages.AddNew(typeof(CDSAmendmentComparisonEDIMessage));
				newMessage.EM_ApplicationCode = entry.GetApplicationCodeForMessage();
				newMessage.EM_MessageText = newMessageXML;
				newMessage.EM_ApplicationReference = EDIMessageApplicationReferencesForAmendment.Current;
				result = newMessage;
			}

			return result;
		}

		internal AmendmentObjectWrapper GetFakeNilAmendment(JobDeclarationMessageSendingObject objectToSend)
		{
			var differences = new List<NilAmendmentChange>();
			var mucr = objectToSend?.Header?.CH_MasterUCR ?? "";

			if (!mucr.IsEmpty)
			{
				var nodes = new List<Node>();
				nodes.Add(new Node() { NodeName = "MetaData" });
				nodes.Add(new Node() { NodeName = "Declaration" });
				nodes.Add(new Node() { NodeName = "GoodsShipment" });
				nodes.Add(new Node() { NodeName = "PreviousDocument" });
				nodes.Add(new Node() { NodeName = "ID" });

				var entry = objectToSend?.Header;
				var lastGoodMessage = ZString.Empty;

				if (entry != null)
				{
					lastGoodMessage = entry.GetLatestMessageForComparison()?.EM_MessageText ?? ZString.Empty;
				}

				var difference = new NilAmendmentChange(Parser, lastGoodMessage, "", nodes, mucr);
				differences.Add(difference);

				return new AmendmentObjectWrapper
				{
					Differences = differences,
					Xml = null
				};
			}
			else
			{
				return null;
			}
		}

		internal AmendmentObjectWrapper GetFakeFECAmendment(JobDeclarationMessageSendingObject objectToSend)
		{
			var differences = new List<FecAmendmentChange>();
			var grossMass = objectToSend?.Header?.InvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault()?.JI_Weight;

			var nodes = new List<Node>();
			nodes.Add(new Node() { NodeName = "MetaData" });
			nodes.Add(new Node() { NodeName = "Declaration" });
			nodes.Add(new Node() { NodeName = "GoodsShipment" });
			nodes.Add(new Node() { NodeName = "GovernmentAgencyGoodsItem" });
			nodes.Add(new Node() { NodeName = "Commodity" });
			nodes.Add(new Node() { NodeName = "GoodsMesaure" });
			nodes.Add(new Node() { NodeName = "GrossMassMeasure" });

			var difference = new FecAmendmentChange(Parser, "", "", nodes, grossMass.ToString());
			differences.Add(difference);

			return new AmendmentObjectWrapper
			{
				Differences = differences,
				Xml = null
			};
		}
	}
}
