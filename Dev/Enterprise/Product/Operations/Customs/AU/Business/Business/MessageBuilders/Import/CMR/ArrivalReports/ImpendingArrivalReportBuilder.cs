using System.Collections;

using Enterprise.Edifact.D99B.Elements;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// Summary description for ImpendingArrivalReportBuilder.
	/// </summary>
	public abstract class ImpendingArrivalReportBuilder : ArrivalReportBuilder
	{
		public ImpendingArrivalReportBuilder(IImpendingArrivalReportInformation reportInfo)
			: base(reportInfo)
		{
			reportInformation = reportInfo;
		}

		protected override void PopulateGroup2LOCs()
		{
			if (ShouldPopulatePortOfDeparture && !reportInformation.LastOverseasPortOfDeparture.IsEmpty)
			{
				MessageUtilities.PopulateLOC(cUSREP.Group2.InstantiateAChildAndAddItToChildrenCollection().LOC[0], LocationFunctionCodeQualifierList.LastPlacePortOfCallOfConveyance, reportInformation.LastOverseasPortOfDeparture, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
			}
		}

		protected override void PopulateGroup2DTMSegments()
		{
			int group2Count = cUSREP.Group2.Count;
			if (group2Count > 0)
			{
				if (ShouldPopulateDepartureDate && !reportInformation.DateTimeOfDepartureUTC.IsEmpty)
				{
					MessageUtilities.PopulateDTM(cUSREP.Group2[group2Count - 1].DTM[0], DepartureDateTimeCode, reportInformation.DateTimeOfDepartureUTC.ToString("yyyyMMdd"), DateTimePeriodFormatCodeList.Ccyymmdd);
					if (MessageSubType != Enterprise.Customs.Common.MessageBuilders.MessageSubTypes.Withdraw)
					{
						MessageUtilities.PopulateDTM(cUSREP.Group2[group2Count - 1].DTM[1], DepartureDateTimeCode, reportInformation.DateTimeOfDepartureUTC.ToString("HHmm"), DateTimePeriodFormatCodeList.Hhmm);
					}
				}
			}
		}

		protected abstract bool ShouldPopulatePortOfDeparture
		{
			get;
		}

		protected abstract bool ShouldPopulateDepartureDate
		{
			get;
		}

		protected abstract bool ShouldPopulatePortOfArrival
		{
			get;
		}

		protected override void PopulateSegmentGroup9()
		{
			base.PopulateSegmentGroup9();

			if (MessageSubType == Enterprise.Customs.Common.MessageBuilders.MessageSubTypes.Create || MessageSubType == Enterprise.Customs.Common.MessageBuilders.MessageSubTypes.Replace)
			{
				new UniqueIdentifierMessageLinePopulator().Populate(cUSREP, GetMessageLines(reportInformation.Lines));
			}
			else if (MessageSubType == Enterprise.Customs.Common.MessageBuilders.MessageSubTypes.Change)
			{
				new UniqueIdentifierMessageLinePopulator().Populate(cUSREP, GetMessageLines(reportInformation.Lines), GetMessageLines(reportInformation.DatabaseLines));
			}
		}

		UniqueIdentifierMessageLine[] GetMessageLines(IImpendingArrivalReportLineInformation[] infoLines)
		{
			ArrayList result = new ArrayList();
			foreach (IImpendingArrivalReportLineInformation infoLine in infoLines)
			{
				result.Add(new ImpendingArrivalReportMessageLine(infoLine));
			}
			return (UniqueIdentifierMessageLine[])result.ToArray(typeof(UniqueIdentifierMessageLine));
		}

		protected abstract DateTimePeriodFunctionCodeQualifierList DepartureDateTimeCode
		{
			get;
		}

		protected IImpendingArrivalReportInformation reportInformation;
	}
}
