
using Enterprise.Edifact.D99B.Elements;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// Summary description for ImpendingArrivalReportBuilder.
	/// </summary>
	public abstract class ActualArrivalReportBuilder : ArrivalReportBuilder
	{
		public ActualArrivalReportBuilder(IActualArrivalReportInformation reportInfo)
			: base(reportInfo)
		{
			reportInformation = reportInfo;
		}

		protected override void PopulateGroup2LOCs()
		{
			if (!reportInformation.PortOfArrival.IsEmpty && ShouldPopulatePortOfArrival)
			{
				MessageUtilities.PopulateLOC(cUSREP.Group2.InstantiateAChildAndAddItToChildrenCollection().LOC[0], LocationFunctionCodeQualifierList.PlaceOfArrival, reportInformation.PortOfArrival, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
			}
		}

		protected virtual bool ShouldPopulatePortOfArrival
		{
			get
			{
				return true;
			}
		}

		protected override void PopulateGroup2DTMSegments()
		{
			int group2Count = cUSREP.Group2.Count;
			if (group2Count > 0)
			{
				if (!reportInformation.ActualArrivalDateTimeUTC.IsEmpty)
				{
					if (MessageSubType != Enterprise.Customs.Common.MessageBuilders.MessageSubTypes.Withdraw)
					{
						MessageUtilities.PopulateDTM(cUSREP.Group2[group2Count - 1].DTM[0], DateTimePeriodFunctionCodeQualifierList.ArrivalDateTimeActual, reportInformation.ActualArrivalDateTimeUTC.ToString("yyyyMMdd"), DateTimePeriodFormatCodeList.Ccyymmdd);
						MessageUtilities.PopulateDTM(cUSREP.Group2[group2Count - 1].DTM[1], DateTimePeriodFunctionCodeQualifierList.ArrivalDateTimeActual, reportInformation.ActualArrivalDateTimeUTC.ToString("HHmm"), DateTimePeriodFormatCodeList.Hhmm);
					}
				}
			}
		}

		readonly IActualArrivalReportInformation reportInformation;
	}
}
