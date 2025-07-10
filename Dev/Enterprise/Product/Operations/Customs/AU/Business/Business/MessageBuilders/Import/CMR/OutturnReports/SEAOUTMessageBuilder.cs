using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SEAOUTMessageBuilder : CMRCUSCARMessageBuilder
	{
		public SEAOUTMessageBuilder(ISeaOutturnReportHeaderInformation reportHeader, IEnumerable<ISeaOutturnReportLineInformation> lines, bool splitMessage, bool subsequentSplitMessages)
		{
			this.ReportHeader = reportHeader;
			this.lines = lines;
			this.isSplitMessage = splitMessage;
			this.isSubsequentSplitMessage = subsequentSplitMessages;
		}

		readonly IEnumerable<ISeaOutturnReportLineInformation> lines;
		readonly bool isSplitMessage;
		readonly bool isSubsequentSplitMessage;
		internal readonly ISeaOutturnReportHeaderInformation ReportHeader;

		protected internal override ZString EM_MessageType => CMRMessage.CMRMessageTypes.SEAOUT;

		protected internal override Type TypeOfMessage => typeof(CMRSEAOUTMessage);

		protected internal override ZString DocumentName => "SEAOUT";

		protected internal override DocumentNameCodeList DocumentNameCode => DocumentNameCodeList.InventoryAdjustmentStatusReport;

		protected override void SetAdditionalEDIMessageDetails(EDIMessage message)
		{
			base.SetAdditionalEDIMessageDetails(message);
			if (SetStatusToPending)
			{
				message.EM_Status = EDIMessage.Status.Pending;
			}

			if (SetSplitMessageIdentifier)
			{
				message.EM_ApplicationReference = CMRCUSRESMessage.splitMessageIdentifier + CurrentVersion.ToString(CultureInfo.InvariantCulture).PadLeft(7, '0');
				message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMilliseconds(CurrentVersion);  // Cusres processing cannot find 'Last' outgoing message when split messages are all created with the same time.
				if (isSubsequentSplitMessage)
				{
					message.EM_MessageSubType = CMRMessage.MessageSubTypes.Amendment;
				}
			}
		}

		protected internal override void GenerateMessageText()
		{
			if (CUSCAR == null)
			{
				CUSCAR = new Edifact.D99B.Messages.CUSCAR.CUSCARMessage();
				PopulateUNH();
				if (isSubsequentSplitMessage)
				{
					PopulateSplitMessageBGM();
				}
				else
				{
					PopulateBGM();
				}

				if (!IsWithdrawal)
				{
					PopulateNAD();
				}

				PopulateTDT();
				if (CUSCAR.Group4.Count > 0)
				{
					PopulateLOC();
				}

				PopulateGroup7(lines);
				PopulateUNT();
			}
		}

		protected void PopulateSplitMessageBGM()
		{
			MessageUtilities.PopulateBGM(BGM, DocumentNameCode, DocumentName, BGMReference, Version.ToString(CultureInfo.InvariantCulture), MessageFunctionCodeList.Change);
		}

		void PopulateNAD()
		{
			ZString responsiblePartyID = ZString.Empty;

			if (!ReportHeader.ResponsiblePartyID.IsEmpty)
			{
				responsiblePartyID = ReportHeader.ResponsiblePartyID;
			}
			else
			{
				ABNCACSplitter splitter = new ABNCACSplitter(GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number);
				responsiblePartyID = splitter.ABN;
			}

			if (!responsiblePartyID.IsEmpty)
			{
				MessageUtilities.PopulateNAD(CUSCAR.Group2[0].NAD[0], PartyFunctionCodeQualifierList.ResponsibleParty, responsiblePartyID, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, null, null);
			}
		}

		void PopulateTDT()
		{
			if (!ReportHeader.VoyageNumber.IsEmpty && !ReportHeader.VesselID.IsEmpty)
			{
				MessageUtilities.PopulateTDT(CUSCAR.Group4[0].TDT[0], TransportStageCodeQualifierList.MainCarriageTransport, ReportHeader.VoyageNumber, TransportMeansDescriptionCodeList.Ship, null, null, ReportHeader.VesselID);
			}
		}

		void PopulateLOC()
		{
			if (!ReportHeader.EstablishmentID.IsEmpty)
			{
				MessageUtilities.PopulateLOC(CUSCAR.Group4[0].LOC[0], LocationFunctionCodeQualifierList.GoodsReceiptPlace, ReportHeader.EstablishmentID, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
		}

		void PopulateGroup7(IEnumerable<ISeaOutturnReportLineInformation> group7lines)
		{
			if (MessageSubType == Common.MessageBuilders.MessageSubTypes.Change)
			{
				if (isSplitMessage)
				{
					new UniqueIdentifierMessageLinePopulator().PopulateSplitMsgLines(CUSCAR, GetLineBuilders(group7lines), GetLineBuilders(ReportHeader.Lines), CustomsLinesBuilder.LineCollection);
				}
				else
				{
					new UniqueIdentifierMessageLinePopulator().Populate(CUSCAR, GetLineBuilders(ReportHeader.Lines), CustomsLinesBuilder.LineCollection, false);
				}
			}
			else if (MessageSubType == Common.MessageBuilders.MessageSubTypes.Create || MessageSubType == Common.MessageBuilders.MessageSubTypes.Replace)
			{
				new UniqueIdentifierMessageLinePopulator().Populate(CUSCAR, GetLineBuilders(group7lines));
			}
		}

		SEAOUTMessageLine[] GetLineBuilders(IEnumerable<ISeaOutturnReportLineInformation> reportLines)
		{
			ArrayList result = new ArrayList();

			foreach (ISeaOutturnReportLineInformation reportLine in reportLines)
			{
				result.Add(new SEAOUTMessageLine(reportLine));
			}
			return (SEAOUTMessageLine[])result.ToArray(typeof(SEAOUTMessageLine));
		}

		SEAOUTMessageLineSentToCustomsBuilder CustomsLinesBuilder
		{
			get { return fCustomsLinesBuilder ?? (fCustomsLinesBuilder = new SEAOUTMessageLineSentToCustomsBuilder(ReportHeader.MessagesProvider)); }
		}
		SEAOUTMessageLineSentToCustomsBuilder fCustomsLinesBuilder;
	}
}
