using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSCAR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CTOMessageBuilder : CMRCUSCARMessageBuilder
	{
		public CTOMessageBuilder(ICTOMessageLine line)
		{
			this.line = line;
			Messages = line.Messages;
		}

		#region Implementation

		protected internal override void GenerateMessageText()
		{
			if (CUSCAR == null)
			{
				CUSCAR = new CUSCARMessage();
				PopulateUNH();
				PopulateBGM();
				PopulateCTOEstablishmentLOC();
				PopulateTDT();
				PopulateSegmentGroup7s();
				PopulateUNT();
			}
		}

		protected void PopulateCTOEstablishmentLOC()
		{
			if (!line.CTOEstablishmentID.IsEmpty)
			{
				MessageUtilities.PopulateLOC(CUSCAR.LOC[0], LocationFunctionCodeQualifierList.Terminal, line.CTOEstablishmentID, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
		}

		protected void PopulateTDT()
		{
			TransportMeansDescriptionCodeList meansCode = line.IsAir ? TransportMeansDescriptionCodeList.Aircraft : TransportMeansDescriptionCodeList.Ship;
			MessageUtilities.PopulateTDT(CUSCAR.Group4[0].TDT[0], TransportStageCodeQualifierList.MainCarriageTransport, null, meansCode, null, null, null);
		}

		protected void PopulateSegmentGroup7s()
		{
			int lineNumber = 1; // We're dealing with 1 line messages only
			SegmentGroup7 group7 = CUSCAR.Group7.InstantiateAChildAndAddItToChildrenCollection();
			MessageUtilities.PopulateCNI(group7.CNI[0], lineNumber.ToString(), "I");
			SegmentGroup8 group8 = PopulateRFFCargoIdentifiers(line, group7);
			PopulateSegmentGroup8(group8, line, lineNumber);
			MessageUtilities.PopulateGID(group8.Group14[0].GID[0], "1");
		}

		protected abstract void PopulateSegmentGroup8(SegmentGroup8 group8, ICTOMessageLine cTOItem, int lineNumber);

		SegmentGroup8 PopulateRFFCargoIdentifiers(ICTOMessageLine line, SegmentGroup7 group7)
		{
			int group8Number = 0;

			if (line.IsAir && !line.AirWaybill.IsEmpty)
			{
				MessageUtilities.PopulateRFF(group7.Group8[group8Number++].RFF[0], ReferenceFunctionCodeQualifierList.AirWaybillNumber, line.AirWaybill, null);
			}
			else if (line.IsSea)
			{
				if (!line.ContainerNumber.IsEmpty)
				{
					MessageUtilities.PopulateRFF(group7.Group8[group8Number++].RFF[0], ReferenceFunctionCodeQualifierList.UnitLoadDeviceEGContainerIdentificationNumber, line.ContainerNumber, null);
				}
				else if (!line.NonContainerisedIdentifier.IsEmpty)
				{
					MessageUtilities.PopulateRFF(group7.Group8[group8Number++].RFF[0], ReferenceFunctionCodeQualifierList.GeneralCargoConsignmentReferenceNumber, line.NonContainerisedIdentifier, null);
				}
			}

			ZString cAN = line.CustomsAuthorityNumber;
			ZString exemptionCode = line.ExportDeclarationExemptionCode;
			ZString cCAN = line.CustomsContingencyAuthorityNumber;
			if (!cAN.IsEmpty)
			{
				MessageUtilities.PopulateRFF(group7.Group8[group8Number++].RFF[0], ReferenceFunctionCodeQualifierList.TransactionReferenceNumber, cAN, null);
			}
			else if (!exemptionCode.IsEmpty)
			{
				MessageUtilities.PopulateRFF(group7.Group8[group8Number++].RFF[0], ReferenceFunctionCodeQualifierList.TaxExemptionLicenceNumber, exemptionCode, null);
			}
			else if (!cCAN.IsEmpty)
			{
				MessageUtilities.PopulateRFF(group7.Group8[group8Number++].RFF[0], ReferenceFunctionCodeQualifierList.ManualProcessingAuthorityNumber, cCAN, null);
			}

			for (int i = 0; i < group8Number - 1; i++)
			{
				MessageUtilities.PopulateGID(group7.Group8[i].Group14[0].GID[0], "1");
			}

			return group7.Group8[group8Number];
		}

		protected ICTOMessageLine line;

		#endregion
	}
}
