using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RequestForPermitFishLineMessageBuilder : RequestForPermitLineMessageBuilder
	{
		public RequestForPermitFishLineMessageBuilder(SANCRTMessage sANCRT, string messageTypeToSend)
			: base(sANCRT, messageTypeToSend)
		{
		}

		protected override void GenerateCatchDates(SegmentGroup11 group11)
		{
			var startDate = invoiceLine.QuarantineExDocLine.QL_CatchStartDate;
			var endDate = invoiceLine.QuarantineExDocLine.QL_CatchEndDate;
			if (!startDate.IsEmpty || !endDate.IsEmpty)
			{
				if (!startDate.IsEmpty)
				{
					EXDOCMessageUtilities.PopulateDTM(group11.DTM.InstantiateAChildAndAddItToChildrenCollection(),
								DateTimePeriodQualifierList.ProcessingStartDateTime,
								startDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
								DateTimePeriodFormatQualifierList.Ccyymmdd);   //CCYYMMDD = 102
				}
				if (!endDate.IsEmpty)
				{
					EXDOCMessageUtilities.PopulateDTM(group11.DTM.InstantiateAChildAndAddItToChildrenCollection(),
								DateTimePeriodQualifierList.ProcessingEndDateTime,
								endDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
								DateTimePeriodFormatQualifierList.Ccyymmdd);   //CCYYMMDD = 102
				}
			}
		}

		protected override void GenerateLineImperialNetWeight(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_ImperialNetWeight.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateMEA(group11.MEA.InstantiateAChildAndAddItToChildrenCollection(),
						MeasurementPurposeQualifierList.ItemWeight,
						PropertyMeasuredCodedList.NetWeight,
						invoiceLine.QuarantineExDocLine.QL_ImperialNetWeightUnit,
						invoiceLine.QuarantineExDocLine.QL_ImperialNetWeight.ToStringTrimZeros());
			}
		}

		protected override void GenerateDrainedWeight(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_DrainedWeight.IsEmpty && !invoiceLine.QuarantineExDocLine.QL_DrainedWeightUnit.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateMEA(group11.MEA.InstantiateAChildAndAddItToChildrenCollection(),
					MeasurementPurposeQualifierList.ItemWeight,
					PropertyMeasuredCodedList.NetNetWeight,
					invoiceLine.QuarantineExDocLine.QL_DrainedWeightUnit,
					invoiceLine.QuarantineExDocLine.QL_DrainedWeight.ToStringTrimZeros());
			}
		}

		bool Errata48Enabled()
		{
			return ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.EXDOCS_Errata48, Core.Constants.CountryCodes.Australia, ZDateTime.Today);
		}

		protected override void GenerateFishWaterIndicator(SegmentGroup11 group11)
		{
			if (Errata48Enabled())
			{
				var exdocLine = invoiceLine.QuarantineExDocLine;
				if (!exdocLine.QL_FishWaterIndicator.IsEmpty)
				{
					var att = group11.ATT.InstantiateAChildAndAddItToChildrenCollection();
					EXDOCMessageUtilities.PopulateATT(att, EXDOCMessageUtilities.FishWaterIndicatorQualifier, exdocLine.QL_FishWaterIndicator);
				}
			}
		}

		protected override void GenerateProdCountryOfOrigin(SegmentGroup11 group11)
		{
			if (Errata48Enabled())
			{
				var countryOfOrigin = invoiceLine.JI_CountryOfOrigin;
				if (!countryOfOrigin.IsEmpty)
				{
					var loc = group11.LOC.InstantiateAChildAndAddItToChildrenCollection();
					EXDOCMessageUtilities.PopulateLOC(loc, PlaceLocationQualifierList.CountryOfOrigin, countryOfOrigin);
				}
			}
		}

		protected override void GenerateHarvestProcess(SegmentGroup18 group18, QuarantineExDocEstablishmentAndTime process)
		{
			EXDOCMessageUtilities.PopulatePRC(group18.PRC.InstantiateAChildAndAddItToChildrenCollection(),
								ProcessTypeIdentificationList.GetFromString(EXDOCProcessTypeCodes.Codes.Harvest));
		}

		protected override void GenerateHarvestStartDate(SegmentGroup18 group18, QuarantineExDocEstablishmentAndTime process)
		{
			if (!process.EE_StartDate.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateDTM(group18.DTM.InstantiateAChildAndAddItToChildrenCollection(),
					DateTimePeriodQualifierList.StartDateTime,
					process.EE_StartDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
					DateTimePeriodFormatQualifierList.Ccyymmdd);
			}
		}

		protected override void GenerateHarvestEndDate(SegmentGroup18 group18, QuarantineExDocEstablishmentAndTime process)
		{
			if (!process.EE_EndDate.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateDTM(group18.DTM.InstantiateAChildAndAddItToChildrenCollection(),
					DateTimePeriodQualifierList.EndDateTime,
					process.EE_EndDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
					DateTimePeriodFormatQualifierList.Ccyymmdd);
			}
		}

		protected override void GenerateDepurationDate(SegmentGroup18 group18, QuarantineExDocEstablishmentAndTime process)
		{
			if (!process.EE_Depuration.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateDTM(group18.DTM.InstantiateAChildAndAddItToChildrenCollection(),
					DateTimePeriodQualifierList.ProcessingDateTime,
					process.EE_Depuration.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
					DateTimePeriodFormatQualifierList.Ccyymmdd);
			}
		}

		protected override void GenerateHarvestAreaLeaseNumber(SegmentGroup18 group18, QuarantineExDocEstablishmentAndTime process)
		{
			if (!process.EE_HarvestArea.IsEmpty || !process.EE_LeaseNumber.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateLOC(group18.LOC.InstantiateAChildAndAddItToChildrenCollection(),
						PlaceLocationQualifierList.RegionOfProduction,
						process.EE_HarvestArea,
						process.EE_LeaseNumber);
			}
		}

		protected override void GenerateDepurationPlantNumber(SegmentGroup18 group18, QuarantineExDocEstablishmentAndTime process)
		{
			if (!process.EE_AuthorisationEstablishmentID.IsEmpty)
			{
				SegmentGroup19 group19 = group18.Group19.InstantiateAChildAndAddItToChildrenCollection();
				EXDOCMessageUtilities.PopulatePNA(group19.PNA.InstantiateAChildAndAddItToChildrenCollection(),
					PartyQualifierList.Plant,
					process.EE_AuthorisationEstablishmentID);
			}
		}

		protected override void GenerateProductSourceState(SegmentGroup11 group11)
		{
			if (!invoiceLine.JI_AUState.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateLOC(group11.LOC.InstantiateAChildAndAddItToChildrenCollection(),
					PlaceLocationQualifierList.MutuallyDefined,
					invoiceLine.AddInfo.ZA_AUState_Hidden);
			}
		}
	}
}
