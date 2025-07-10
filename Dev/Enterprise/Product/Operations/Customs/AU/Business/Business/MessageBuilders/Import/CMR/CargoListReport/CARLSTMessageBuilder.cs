using System;
using System.Collections;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSCAR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CARLSTMessageBuilder : CMRCUSCARMessageBuilder
	{
		public CARLSTMessageBuilder(CusSeaManArrivalPort arrivalPort)
			: this(new CusSeaManArrivalPortCargoListReportHeader(arrivalPort))
		{
		}

		internal CARLSTMessageBuilder(ICargoListReportHeader reportHeader)
			: base(ZString.Empty)
		{
			this.reportHeader = reportHeader;
		}

		protected internal override ZString DocumentName => "CARLST";

		protected internal override DocumentNameCodeList DocumentNameCode => DocumentNameCodeList.CargoMovementEventLog;

		protected internal override ZString EM_MessageType => CMRMessage.CMRMessageTypes.CARLST;

		protected internal override Type TypeOfMessage => typeof(CMRCARLSTMessage);

		protected internal override void GenerateMessageText()
		{
			if (CUSCAR == null)
			{
				CUSCAR = new CUSCARMessage();
				PopulateUNH();
				PopulateBGM();
				PopulateNAD();
				PopulateTDT();
				PopulateLOC();
				PopulateLineDetails();
				PopulateUNT();
			}
		}

		void PopulateNAD()
		{
			if (!reportHeader.CargoResponsiblePartyID.IsEmpty)
			{
				MessageUtilities.PopulateNAD(CUSCAR.Group2[0].NAD[0], PartyFunctionCodeQualifierList.GoodsCustodian, reportHeader.CargoResponsiblePartyID, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, null, null);
			}
		}

		void PopulateTDT()
		{
			if (!reportHeader.LloydsNumber.IsEmpty && !reportHeader.VoyageNumber.IsEmpty)
			{
				MessageUtilities.PopulateTDT(CUSCAR.Group4[0].TDT[0], TransportStageCodeQualifierList.MainCarriageTransport, reportHeader.VoyageNumber, TransportMeansDescriptionCodeList.Ship, null, null, reportHeader.LloydsNumber);
			}
		}

		void PopulateLOC()
		{
			if (CUSCAR.Group4.Count > 0)
			{
				if (!reportHeader.DischargePort.IsEmpty)
				{
					MessageUtilities.PopulateLOC(CUSCAR.Group4[0].LOC[0], LocationFunctionCodeQualifierList.PortOfDischarge, reportHeader.DischargePort, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
				}
			}
		}

		void PopulateLineDetails()
		{
			if (MessageSubType == Common.MessageBuilders.MessageSubTypes.Change)
			{
				new UniqueIdentifierMessageLinePopulator().Populate(CUSCAR, GetLineBuilders(reportHeader.Lines), GetLineBuilders(reportHeader.DatabaseLines));
			}
			else if (MessageSubType == Common.MessageBuilders.MessageSubTypes.Create)
			{
				new UniqueIdentifierMessageLinePopulator().Populate(CUSCAR, GetLineBuilders(reportHeader.Lines));
			}
		}

		CARLSTLineBuilder[] GetLineBuilders(ICargoListReportLine[] reportLines)
		{
			ArrayList result = new ArrayList();

			foreach (ICargoListReportLine reportLine in reportLines)
			{
				result.Add(new CARLSTLineBuilder(reportLine));
			}
			return (CARLSTLineBuilder[])result.ToArray(typeof(CARLSTLineBuilder));
		}

		readonly ICargoListReportHeader reportHeader;
	}
}
