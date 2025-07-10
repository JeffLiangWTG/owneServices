using System;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSCAR;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AIRCRMessageBuilder : CargoReportMessageBuilder
	{
		public AIRCRMessageBuilder(CusPartShip partShip, CTOCusHAWB hAWB)
			: this(new CTOCusPartShipCargoReportHeader(partShip, hAWB), ZString.Empty)
		{
		}

		public AIRCRMessageBuilder(CTOCusHAWB hAWB)
			: this(new CTOCusHAWBCargoReportHeader(hAWB), ZString.Empty)
		{
		}

		public AIRCRMessageBuilder(CusHAWB hAWB, bool shouldDelaySending = false)
			: this(hAWB, ZString.Empty, shouldDelaySending)
		{
		}

		public AIRCRMessageBuilder(CusHAWB hAWB, ZString ownerComanyABN, bool shouldDelaySending = false)
			: this(new CusHAWBAirCargoReportHeader(hAWB), ownerComanyABN, shouldDelaySending)
		{
		}

		public AIRCRMessageBuilder(CusPartShip partShip, ZString ownerComanyABN)
			: this(new CusPartShipAirCargoReportHeader(partShip), ownerComanyABN)
		{
		}

		public AIRCRMessageBuilder(IAirCargoReportHeader reportHeader, ZString ownerComanyABN, bool shouldDelaySending = false)
			: base(reportHeader, ownerComanyABN)
		{
			this.reportHeader = reportHeader;
			this.ShouldDelaySending = shouldDelaySending;
		}

		#region Implementation

		protected internal override ZString DocumentName => "AIRCR";

		protected internal override ZString EM_MessageType => CMRMessage.CMRMessageTypes.AIRCR;

		protected internal override Type TypeOfMessage => typeof(CMRAIRCRMessage);

		protected override bool IsBureau => reportHeader.IsBureau;

		protected override void SetAdditionalEDIMessageDetails(EDIMessage message)
		{
			base.SetAdditionalEDIMessageDetails(message);
			if (ShouldDelaySending && reportHeader.CanDelaySending)
			{
				message.EM_HeldUntilDate = ZDateTime.UtcNow;
			}
		}

		#region Group1

		protected override void PopulateGroup1()
		{
			base.PopulateGroup1();
			if (!reportHeader.MasterHouseBill.IsEmpty && MessageSubType != Enterprise.Customs.Common.MessageBuilders.MessageSubTypes.Withdraw)
			{
				MessageUtilities.PopulateRFF(CUSCAR.Group1[0].RFF.InstantiateAChildAndAddItToChildrenCollection(), ReferenceFunctionCodeQualifierList.AirWaybillNumber, reportHeader.MasterHouseBill, null);
			}
			if (!reportHeader.HAWBNum.IsEmpty)
			{
				MessageUtilities.PopulateRFF(CUSCAR.Group1[0].RFF.InstantiateAChildAndAddItToChildrenCollection(), ReferenceFunctionCodeQualifierList.HouseWaybillNumber, reportHeader.HAWBNum, null);
			}
			if (!reportHeader.MAWB.IsEmpty)
			{
				MessageUtilities.PopulateRFF(CUSCAR.Group1[0].RFF.InstantiateAChildAndAddItToChildrenCollection(), ReferenceFunctionCodeQualifierList.MasterAirWaybillNumber, reportHeader.MAWB, null);
			}
			if (!reportHeader.MatchConsignmentReference.IsEmpty)
			{
				MessageUtilities.PopulateRFF(CUSCAR.Group1[0].RFF.InstantiateAChildAndAddItToChildrenCollection(), ReferenceFunctionCodeQualifierList.GetFromString("CNR"), reportHeader.MatchConsignmentReference, null);
			}
		}

		#endregion

		#region Group2

		protected internal override void PopulateGroup2()
		{
			base.PopulateGroup2();

			CargoReportHelper.DoICSRelease(() =>
			{
				var vendor = header.ConsignorVendor;
				if (!vendor.IsEmpty)
				{
					MessageUtilities.PopulateNAD(CUSCAR.Group2.InstantiateAChildAndAddItToChildrenCollection().NAD[0], PartyFunctionCodeQualifierList.Vendor, vendor, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, null, null);
				}
			});

			if (reportHeader.IsHVLVSpecialReporter && Env.Registry.AUCustoms.HVLVSpecialReporterNumber.Length > 0)
			{
				MessageUtilities.PopulateNAD(CUSCAR.Group2.InstantiateAChildAndAddItToChildrenCollection().NAD[0], PartyFunctionCodeQualifierList.ApprovedConsignor, Env.Registry.AUCustoms.HVLVSpecialReporterNumber, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, null, null);
			}
			if (reportHeader.IsRemailSpecialReporter && Env.Registry.AUCustoms.RemailSpecialReporterNumber.Length > 0)
			{
				MessageUtilities.PopulateNAD(CUSCAR.Group2.InstantiateAChildAndAddItToChildrenCollection().NAD[0], PartyFunctionCodeQualifierList.ApprovedConsignor, Env.Registry.AUCustoms.RemailSpecialReporterNumber, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, null, null);
			}
		}

		protected override void PopulateGroup2ResponsiblePartyID()
		{
			if (!reportHeader.ResponsiblePartyID.IsEmpty)
			{
				MessageUtilities.PopulateNAD(CUSCAR.Group2.InstantiateAChildAndAddItToChildrenCollection().NAD[0], PartyFunctionCodeQualifierList.ResponsibleParty, reportHeader.ResponsiblePartyID, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, null, null);
			}
			else
			{
				base.PopulateGroup2ResponsiblePartyID();
			}
		}

		#endregion

		#region Group4

		protected override void PopulateGroup4()
		{
			if (!reportHeader.FlightNo.IsEmpty)
			{
				SegmentGroup4 group4 = CUSCAR.Group4.InstantiateAChildAndAddItToChildrenCollection();
				MessageUtilities.PopulateTDT(group4.TDT[0], TransportStageCodeQualifierList.MainCarriageTransport, reportHeader.FlightNo.SubstringSafe(2), TransportMeansDescriptionCodeList.Aircraft, reportHeader.FlightNo.Left(2), CodeListResponsibleAgencyCodeList.IataInternationalAirTransportAssociation, null);

				if (!reportHeader.ArivalDate.IsEmpty)
				{
					MessageUtilities.PopulateDTM(group4.DTM.InstantiateAChildAndAddItToChildrenCollection(), DateTimePeriodFunctionCodeQualifierList.ArrivalDateTimeActual, reportHeader.ArivalDate.ToString("yyyyMMdd"), DateTimePeriodFormatCodeList.Ccyymmdd);
				}

				base.PopulateGroup4();
			}
		}

		protected internal override void PopulateLocations()
		{
			base.PopulateLocations();

			if (MessageSubType != Enterprise.Customs.Common.MessageBuilders.MessageSubTypes.Withdraw)
			{
				if (!header.Discharge.IsEmpty)
				{
					MessageUtilities.PopulateLOC(CUSCAR.Group4[0].LOC.InstantiateAChildAndAddItToChildrenCollection(), LocationFunctionCodeQualifierList.PortOfDischarge, header.Discharge, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
				}
				if (!header.Origin.IsEmpty)
				{
					MessageUtilities.PopulateLOC(CUSCAR.Group4[0].LOC.InstantiateAChildAndAddItToChildrenCollection(), LocationFunctionCodeQualifierList.PlaceOfDocumentIssue, header.Origin, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
				}
			}
		}

		#endregion

		#region GIS

		protected override void PopulateGISSegments()
		{
			base.PopulateGISSegments();
			if (reportHeader.IsMasterHouse)
			{
				MessageUtilities.PopulateGIS(CUSCAR.GIS.InstantiateAChildAndAddItToChildrenCollection(), ProcessingIndicatorDescriptionCodeList.GetFromString("FFO"), CodeListIdentificationCodeList.CustomsIndicator, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
			if (reportHeader.IsDocuments)
			{
				MessageUtilities.PopulateGIS(CUSCAR.GIS.InstantiateAChildAndAddItToChildrenCollection(), ProcessingIndicatorDescriptionCodeList.GetFromString("DOC"), CodeListIdentificationCodeList.CustomsIndicator, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
		}

		#endregion

		#region Group7

		protected override void PopulateGroup7()
		{
			base.PopulateGroup7();
			CUSCAR.Group7[0].CNI[0].ConsolidationItemNumber = "1";
			MessageUtilities.PopulateRFF(CUSCAR.Group7[0].Group8[0].RFF[0], ReferenceFunctionCodeQualifierList.UniqueConsignmentReferenceNumber, EDIMessage.SendersReferencePlaceHolder, null);
			PopulateMOA();
			PopulateGroup8GIS();
			PopulatePAC();
			PopulateFTX();
			PopulateMEA();
		}

		protected void PopulateMOA()
		{
			if (reportHeader.GoodsValue.IsEmpty)
			{
				MessageUtilities.PopulateMOA(CUSCAR.Group7[0].Group8[0].MOA[0], MonetaryAmountTypeCodeQualifierList.NoDeclaredValueForCustoms, "NDV", null);
			}
			else
			{
				if (!reportHeader.GoodsValueCurrency.IsEmpty)
				{
					MessageUtilities.PopulateMOA(CUSCAR.Group7[0].Group8[0].MOA[0], MonetaryAmountTypeCodeQualifierList.DeclaredValueForCarriage, reportHeader.GoodsValue.ToString(2), reportHeader.GoodsValueCurrency);
				}
			}
		}

		protected void PopulateGroup8GIS()
		{
			if (reportHeader.IsPersonalEffects)
			{
				MessageUtilities.PopulateGIS(CUSCAR.Group7[0].Group8[0].GIS.InstantiateAChildAndAddItToChildrenCollection(), ProcessingIndicatorDescriptionCodeList.GetFromString("PER"), CodeListIdentificationCodeList.CustomsIndicator, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
			if (reportHeader.IsSelfAssessedClearance)
			{
				MessageUtilities.PopulateGIS(CUSCAR.Group7[0].Group8[0].GIS.InstantiateAChildAndAddItToChildrenCollection(), ProcessingIndicatorDescriptionCodeList.GetFromString("SAC"), CodeListIdentificationCodeList.CustomsIndicator, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
		}

		protected void PopulatePAC()
		{
			MessageUtilities.PopulateGID(CUSCAR.Group7[0].Group8[0].Group14[0].GID[0], "1");
			MessageUtilities.PopulatePAC(CUSCAR.Group7[0].Group8[0].Group14[0].PAC[0], reportHeader.PackageCount);
		}

		protected void PopulateFTX()
		{
			if (!reportHeader.GoodsDescription.IsEmpty)
			{
				MessageUtilities.PopulateFTX(CUSCAR.Group7[0].Group8[0].Group14[0].FTX[0], TextSubjectCodeQualifierList.GoodsDescription, reportHeader.GoodsDescription);
			}
		}

		protected void PopulateMEA()
		{
			if (!reportHeader.WeightUQ.IsEmpty && !reportHeader.Weight.IsEmpty)
			{
				var roundedDecimal = Enterprise.ZArchitecture.Core.Utilities.Round(reportHeader.Weight, 2);
				var weightUQ = reportHeader.WeightUQ;
				if (weightUQ == Core.Constants.Weight.Kilograms && roundedDecimal != reportHeader.Weight && reportHeader.Weight < 1m)
				{
					roundedDecimal = Enterprise.ZArchitecture.Core.Utilities.Round(reportHeader.Weight * 1000m, 2);
					weightUQ = Core.Constants.Weight.Grams;
				}
				MessageUtilities.PopulateMEA(CUSCAR.Group7[0].Group8[0].Group14[0].MEA[0], MeasurementAttributeCodeList.Measurement, MeasuredAttributeCodeList.GrossWeight, weightUQ, new ZDecimal(roundedDecimal).ToString(2));
			}
		}

		#endregion

		protected IAirCargoReportHeader reportHeader;
		protected readonly bool ShouldDelaySending;

		#endregion
	}
}
