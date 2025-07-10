using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRSEIMessage : CMRCUSRESMessage, ICMRDepotMessage
	{
		public CMRSEIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.SEI;
		}

		CusOutturnHeader header
		{
			get { return _header ?? (_header = Factory.LoadTop1<CusOutturnHeader>(new ZQuery(CusOutturnHeaderSchema.C6_SendersMessageReference, GetReferenceFromSendersReference()))); }
		}
		CusOutturnHeader _header;

		#region overrides

		protected override internal BusinessObject GetWrappedObject()
		{
			CMRSeaDepotMessageProcessor depotMessageProcessor = new CMRSeaDepotMessageProcessor(this);
			depotMessageProcessor.Process();
			return header;
		}

		protected override ZString GetStatusCore()
		{
			return CMRMessageStatusDescription.ACCEPTED;
		}

		public override ZString GetReport()
		{
			ZStringBuilder builder = new ZStringBuilder();
			if (CUSRES != null)
			{
				builder.Append("Sea Cargo Establishment Information - (SEI)");
				builder.Append("Processing Date: " + ProcessingDate.ToString("dd/MM/yyyy HH:mm:ss"));
				RefVessel vessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, LloydsNumber));
				ZString vesselName = vessel == null ? ZString.Empty : vessel.RV_Code;
				builder.Append("Vessel: " + vesselName + " (" + LloydsNumber + ")");
				builder.Append("Voyage: " + VoyageNumber);
				builder.Append("Inland Movement Mode: " + InlandMovementMode);
				builder.Append("Recipient Site: " + RecipientSiteID);
				builder.Append("UBM Responsible Party: " + UBMResponsiblePartyID);
				foreach (SegmentGroup6 group6 in CUSRES.Group6)
				{
					builder.Append("");
					CMRSEIMessageLine line = new CMRSEIMessageLine(this, group6);
					builder.Append(line.ToString());
				}
				builder.Append("");
			}
			return builder.ToStringWithNewLineBetweenAppends();
		}

		protected override CMRCUSRESMessage LinkOrCloneMessageCore(EDIMessageCollection messages)
		{
			var result = (CMRSEIMessage)base.LinkOrCloneMessageCore(messages);// this sets EM_LinkedObject
			var depotCusOutturn = result.EM_LinkedObject as DepotCusOutturn;
			if (depotCusOutturn != null)
			{
				var line = MatchingLine(depotCusOutturn.C5_ContainerNumber, depotCusOutturn.C5_MasterBill, depotCusOutturn.C5_HouseBill);
				if (line != null)
				{
					result.EM_MessageText = ShortenedMessageTemplate.Replace(Group6PlaceHolder, line.LineGroup6.ToString(new UNOCCMRCharacterSet()));
				}
				else
				{
					result.EM_MessageText = EM_MessageText;
				}
			}
			return result;
		}

		string ShortenedMessageTemplate
		{
			get
			{
				if (shortenedMessageTemplate == null)
				{
					var charSet = new UNOCCMRCharacterSet();
					var result = new ZStringBuilder();
					result.Append(CUSRES.UNH[0].ToString(charSet));
					result.Append(CUSRES.BGM[0].ToString(charSet));
					foreach (DTMSegment dtm in CUSRES.DTM)
					{
						result.Append(dtm.ToString(charSet));
					}
					foreach (TDTSegment tdt in CUSRES.TDT)
					{
						result.Append(tdt.ToString(charSet));
					}
					foreach (SegmentGroup1 group1 in CUSRES.Group1)
					{
						result.Append(group1.ToString(charSet));
					}
					foreach (SegmentGroup3 group3 in CUSRES.Group3)
					{
						result.Append(group3.ToString(charSet));
					}
					result.Append(Group6PlaceHolder);
					result.Append(CUSRES.UNT[0].ToString(charSet));

					shortenedMessageTemplate = result.ToString();
				}
				return shortenedMessageTemplate;
			}
		}
		string shortenedMessageTemplate;
		const string Group6PlaceHolder = "<<-GROUP6PlaceHolder->>";

		#endregion

		public CMRSEIMessageLine MatchingLine(ZString containerNumber, ZString oceanBill, ZString houseBill)
		{
			foreach (SegmentGroup6 group6 in CUSRES.Group6)
			{
				CMRSEIMessageLine result = new CMRSEIMessageLine(this, group6);
				if (result.ContainerNumber == containerNumber && result.HouseBillNumber == houseBill && result.OceanBillNumber == oceanBill)
				{
					return result;
				}
			}
			return null;
		}

		public ZString InlandMovementMode
		{
			get
			{
				TDTSegment tDT = GetTDTSegment(CUSRES.TDT, TransportStageCodeQualifierList.InlandTransport);
				if (tDT != null)
				{
					return tDT.ModeOfTransport.TransportModeNameCode;
				}
				return ZString.Empty;
			}
		}

		public ZString RecipientSiteID
		{
			get { return GetNADCode(PartyFunctionCodeQualifierList.MessageRecipient); }
		}

		public ZString UBMResponsiblePartyID
		{
			get { return GetNADCode(PartyFunctionCodeQualifierList.ResponsibleParty); }
		}

		ZString GetNADCode(PartyFunctionCodeQualifierList functionCode)
		{
			foreach (SegmentGroup1 group1 in CUSRES.Group1)
			{
				foreach (NADSegment nAD in group1.NAD)
				{
					if (nAD.PartyFunctionCodeQualifier == functionCode)
					{
						return nAD.PartyIdentificationDetails.PartyIdentifier;
					}
				}
			}
			return ZString.Empty;
		}

		#region ICMRDepotMessage Members

		public ZString LloydsNumber
		{
			get
			{
				TDTSegment tDT = GetTDTSegment(CUSRES.TDT, TransportStageCodeQualifierList.MainCarriageTransport);
				if (tDT != null && tDT.TransportMeans.TransportMeansDescriptionCode == TransportMeansDescriptionCodeList.Ship)
				{
					return tDT.TransportIdentification.TransportMeansIdentificationNameIdentifier;
				}
				return ZString.Empty;
			}
		}

		public ZString VoyageNumber
		{
			get
			{
				TDTSegment tDT = GetTDTSegment(CUSRES.TDT, TransportStageCodeQualifierList.MainCarriageTransport);
				if (tDT != null && tDT.TransportMeans.TransportMeansDescriptionCode == TransportMeansDescriptionCodeList.Ship)
				{
					return tDT.ConveyanceReferenceNumber;
				}
				return ZString.Empty;
			}
		}

		ZString ICMRDepotMessage.DestinationPremiseID
		{
			get { return ZString.Empty; }
		}

		ZString ICMRDepotMessage.OurPremiseID
		{
			get { return header == null ? ZString.Empty : header.C6_OutturningPremiseID; }
		}

		ZString ICMRDepotMessage.OriginPremiseID
		{
			get { return ZString.Empty; }
		}

		CMRDepotMessageType ICMRDepotMessage.MessageType
		{
			get { return CMRDepotMessageType.Status; }
		}

		ICMRDepotMessageLine[] ICMRDepotMessage.Lines
		{
			get
			{
				List<ICMRDepotMessageLine> result = new List<ICMRDepotMessageLine>();
				if (CUSRES != null)
				{
					foreach (SegmentGroup6 group6 in CUSRES.Group6)
					{
						result.Add(new CMRSEIMessageLine(this, group6));
					}
				}
				return result.ToArray();
			}
		}

		void ICMRDepotMessage.AddUnmatchedContainer(CARSTRecord carstRecord)
		{
			this.AddUnmatchedContainer(carstRecord, UnmatchedContainers);
		}

		public Dictionary<string, List<CARSTRecord>> UnmatchedContainers
		{
			get { return unmatchedContainers ?? (unmatchedContainers = new Dictionary<string, List<CARSTRecord>>()); }
		}
		Dictionary<string, List<CARSTRecord>> unmatchedContainers;

		#endregion
	}
}
