using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers;
using Enterprise.Customs.GB.Registry;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class CIMFMA : CargoImpBase, ICimParser
	{
		public CIMFMA(List<string> list, BusinessObjectFactory factory, CargoFactMessage cargoFact)
			: base(new ErrorCollector())
		{
			linesOfCargoImpWithoutId = new List<string>();
			for (int i = 1; i < list.Count; i++)
			{
				linesOfCargoImpWithoutId.Add(list[i]);
			}
			this.cargoFact = cargoFact;
			this.factory = factory;
		}

		public override ZString CargoImpCode
		{
			get { return Code; }
		}

		public const string Code = "FMA";

		protected override string[] CargoImpLinesWithoutType
		{
			get { return linesOfCargoImpWithoutId.ToArray(); }
		}

		public override ZString MessageInterpretation
		{
			get
			{
				return string.Format(
				  @" {0}
<p><b>FMA Acknowledgement</b></p>
<p><i>{1}</i>  </p>
<p>Original message type: {2} </p> 
",
					MessagePrettierCss.CSS,
					acknowledgementText,
					originalMessageType);
			}
		}

		public bool DoAllProcessingBeforePrinting()
		{
			try
			{
				/*
					FMA
					ACK/C.0001
					/MESSAGE STORED FOR LATER TRANSMISSION.
					FRD
					LHRCWE
				*/
				var sb = new ZStringBuilder();
				int i;
				for (i = 0; i < linesOfCargoImpWithoutId.Count; i++)
				{
					if (i == 0 || linesOfCargoImpWithoutId[i].StartsWith("/"))
					{
						sb.Append(linesOfCargoImpWithoutId[i]);
					}
					else
					{ break; }
				}
				acknowledgementText = sb.ToStringWithDelimiterBetweenAppends(" ");
				acknowledgementText = acknowledgementText.Replace("ACK/", "");
				acknowledgementText = acknowledgementText.Replace(" /", " ");
				originalMessageType = linesOfCargoImpWithoutId[i];
				var outboundMessage = TryLoadOutboundMessageFromCommonAccessReference(cargoFact.UNH[0].CommonAccessReference, factory);
				var couldNotFindJobFromCARMessage = "";
				if (outboundMessage != null)
				{
					Awb = (ICcsukCusAwb)outboundMessage.EM_LinkedObject;
					outboundMessage.EM_Status = OutboundMessageNewStatus;
				}
				else
				{
					couldNotFindJobFromCARMessage = "<p>It was not possible to find an outgoing message (and therefore job) because the response did not contain a valid common access reference. The original message text follows to help you identify the job.</p><p>"
							+ String.Join("<br> \r\n", linesOfCargoImpWithoutId.ToArray()) + "</p>";
				}

				if (Awb == null)
				{
					string subject = CargoImpCode + " message received";
					new CcsukEmailSender(factory, CcsukEmailSender.ToWhom.StaffAndOrCustomsGroupBasedOnRegistry, (BusinessObject)Awb).SendEmail(subject, MessageInterpretation + couldNotFindJobFromCARMessage,
						GBCustomsDataRegistry.Instance.NotificationCcsukCargoFact, "CIM" + CargoImpCode, Guid.Empty, Guid.Empty, Guid.Empty);
				}
				else
				{
					string subject = CargoImpCode + " message received for CCSUK job " + Awb.ReferenceNumber;
					new CcsukEmailSender(factory, CcsukEmailSender.ToWhom.StaffAndOrCustomsGroupBasedOnRegistry, (BusinessObject)Awb).SendEmail(subject, MessageInterpretation + couldNotFindJobFromCARMessage,
						GBCustomsDataRegistry.Instance.NotificationCcsukCargoFact, "CIM" + CargoImpCode, Awb.Branch.Company.PK.ToGuid(), Awb.Branch.PK.ToGuid(), Guid.Empty);
				}
				return true;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				throw new FormatException("Could not process inbound " + this.CargoImpCode + ", format was unexpected.", ex);
			}
		}

		protected virtual ZString OutboundMessageNewStatus
		{
			get { return EDIMessage.Status.Acknowledged; }
		}
		public void DoPrinting()
		{ }

		protected string acknowledgementText;
		protected string originalMessageType;
		public ICcsukCusAwb Awb { get; private set; }
		readonly List<string> linesOfCargoImpWithoutId;
		readonly BusinessObjectFactory factory;
		readonly CargoFactMessage cargoFact;
	}
}
