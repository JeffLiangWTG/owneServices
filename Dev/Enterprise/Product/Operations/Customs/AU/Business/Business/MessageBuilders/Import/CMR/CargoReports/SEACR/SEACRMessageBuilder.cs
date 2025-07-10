using System;
using System.Collections;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SEACRMessageBuilder : CargoReportMessageBuilder
	{
		public SEACRMessageBuilder(CusSeaManOBLHeader header)
			: this(new CusSeaManOBLHeaderSeaCargoReportHeader(header), ZString.Empty)
		{
		}

		public SEACRMessageBuilder(CusSCAHouse scaHouse)
			: this(scaHouse, ZString.Empty) { }

		public SEACRMessageBuilder(CusSCAHouse scaHouse, ZString messageOwnerSiteID, bool shouldDelaySending = false)
			: this(new CusSCAHouseSeaCargoReportHeader(scaHouse), messageOwnerSiteID, shouldDelaySending)
		{
		}

		public SEACRMessageBuilder(ISeaCargoReportHeader reportHeader, ZString messageOwnerSiteID, bool shouldDelaySending = false)
			: base(reportHeader, messageOwnerSiteID)
		{
			this.reportHeader = reportHeader;
			this.shouldDelaySending = shouldDelaySending;
		}

		#region Implementation

		protected internal override ZString DocumentName => "SEACR";

		protected internal override ZString EM_MessageType => CMRMessage.CMRMessageTypes.SEACR;

		protected internal override Type TypeOfMessage => typeof(CMRSEACRMessage);

		protected override bool IsBureau => reportHeader.IsBureau;

		protected override void SetAdditionalEDIMessageDetails(EDIMessage message)
		{
			base.SetAdditionalEDIMessageDetails(message);
			if (shouldDelaySending && reportHeader.CanDelaySending)
			{
				message.EM_HeldUntilDate = ZDateTime.UtcNow;
			}
		}

		#region Group1

		protected override void PopulateGroup1()
		{
			base.PopulateGroup1();

			if (!reportHeader.HouseBill.IsEmpty)
			{
				MessageUtilities.PopulateRFF(CUSCAR.Group1[0].RFF.InstantiateAChildAndAddItToChildrenCollection(), ReferenceFunctionCodeQualifierList.HouseBillOfLadingNumber, reportHeader.HouseBill, null);
			}
			if (!reportHeader.ParentBill.IsEmpty && (MessageSubType != Enterprise.Customs.Common.MessageBuilders.MessageSubTypes.Withdraw))
			{
				MessageUtilities.PopulateRFF(CUSCAR.Group1[0].RFF.InstantiateAChildAndAddItToChildrenCollection(), ReferenceFunctionCodeQualifierList.BillOfLadingNumber, reportHeader.ParentBill, null);
			}
			if (!reportHeader.OceanBill.IsEmpty)
			{
				MessageUtilities.PopulateRFF(CUSCAR.Group1[0].RFF.InstantiateAChildAndAddItToChildrenCollection(), ReferenceFunctionCodeQualifierList.MasterBillOfLadingNumber, reportHeader.OceanBill, null);
			}
		}

		#endregion

		#region Group2

		protected internal override void PopulateGroup2()
		{
			base.PopulateGroup2();

			if (!reportHeader.NotifyPartyName.IsEmpty)
			{
				var notifyPartyGroup = CUSCAR.Group2.InstantiateAChildAndAddItToChildrenCollection();
				PopulateNADWithEnteredDetails(notifyPartyGroup, PartyFunctionCodeQualifierList.NotifyParty, header.NotifyPartyName, header.NotifyPartyStreet, header.NotifyPartyStreet2, header.NotifyPartyCity, header.NotifyPartyPostCode, header.NotifyPartyCountry);
			}

			var principalID = reportHeader.PrincipalID;
			if (!principalID.IsEmpty)
			{
				MessageUtilities.PopulateNAD(CUSCAR.Group2.InstantiateAChildAndAddItToChildrenCollection().NAD[0], PartyFunctionCodeQualifierList.TransitPrincipalsAgentRepresentative, principalID, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, null, null);
			}
		}

		#endregion

		#region Group4

		protected override void PopulateGroup4()
		{
			if (!reportHeader.Voyage.IsEmpty && !reportHeader.LloydsNumber.IsEmpty)
			{
				var group4 = CUSCAR.Group4.InstantiateAChildAndAddItToChildrenCollection();
				MessageUtilities.PopulateTDT(group4.TDT[0], TransportStageCodeQualifierList.MainCarriageTransport, reportHeader.Voyage, TransportMeansDescriptionCodeList.Ship, null, null, reportHeader.LloydsNumber);
				base.PopulateGroup4();
			}
		}

		protected internal override void PopulateLocations()
		{
			base.PopulateLocations();

			if (!header.Discharge.IsEmpty)
			{
				MessageUtilities.PopulateLOC(CUSCAR.Group4[0].LOC.InstantiateAChildAndAddItToChildrenCollection(), LocationFunctionCodeQualifierList.PortOfDischarge, header.Discharge, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
			}
			if (MessageSubType != Enterprise.Customs.Common.MessageBuilders.MessageSubTypes.Withdraw)
			{
				if (!header.Origin.IsEmpty)
				{
					MessageUtilities.PopulateLOC(CUSCAR.Group4[0].LOC.InstantiateAChildAndAddItToChildrenCollection(), LocationFunctionCodeQualifierList.BillOfLadingReleaseOffice, header.Origin, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
				}

				if (!reportHeader.OriginCountry.IsEmpty)
				{
					MessageUtilities.PopulateLOC(CUSCAR.Group4[0].LOC.InstantiateAChildAndAddItToChildrenCollection(), LocationFunctionCodeQualifierList.CountryOfOrigin, reportHeader.OriginCountry, CodeListResponsibleAgencyCodeList.IsoInternationalOrganizationForStandardization);
				}
			}
		}

		#endregion

		#region GIS

		protected override void PopulateGISSegments()
		{
			base.PopulateGISSegments();
			if (reportHeader.IsConsolidation)
			{
				MessageUtilities.PopulateGIS(CUSCAR.GIS.InstantiateAChildAndAddItToChildrenCollection(), ProcessingIndicatorDescriptionCodeList.GetFromString("FFO"), CodeListIdentificationCodeList.CustomsIndicator, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
		}

		#endregion

		#region Group7

		protected override void PopulateGroup7()
		{
			if (MessageSubType == Common.MessageBuilders.MessageSubTypes.Create || MessageSubType == Common.MessageBuilders.MessageSubTypes.Replace)
			{
				new UniqueIdentifierMessageLinePopulator().Populate(CUSCAR, GetMessagesLines(reportHeader.Lines));
			}
			else if (MessageSubType == Common.MessageBuilders.MessageSubTypes.Change)
			{
				new UniqueIdentifierMessageLinePopulator().Populate(CUSCAR, GetMessagesLines(reportHeader.Lines), GetMessagesLines(reportHeader.DatabaseLines), true);
			}
		}

		protected UniqueIdentifierMessageLine[] GetMessagesLines(ISeaCargoReportLine[] reportLines)
		{
			var result = new ArrayList();

			foreach (var reportLine in reportLines)
			{
				result.Add(new SEACRMessageLine(reportLine));
			}

			return (UniqueIdentifierMessageLine[])result.ToArray(typeof(UniqueIdentifierMessageLine));
		}

		#endregion

		protected ISeaCargoReportHeader reportHeader;
		protected readonly bool shouldDelaySending;

		#endregion
	}
}
