using CargoWise.Types;
using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;
using Enterprise.Edifact.D97BAU.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EXDOCMesageDecoderPermit
	{
		public EXDOCMesageDecoderPermit(SegmentGroup1 group1)
		{
			this.group1 = group1;
		}

		public void Process()
		{
			ProcessDOC();
			ProcessDTM();
		}

		void ProcessDOC()
		{
			foreach (DOCSegment dOC in group1.DOC)
			{
				if (dOC.DocumentMessageName.DocumentMessageNameCoded == DocumentMessageNameCodedList.ImportLicence)
				{
					importLicenseNumber = dOC.DocumentMessageDetails.DocumentMessageNumber;
				}
			}
		}

		void ProcessDTM()
		{
			foreach (DTMSegment dTM in group1.DTM)
			{
				if (dTM.DateTimePeriod.DateTimePeriodQualifier == DateTimePeriodQualifierList.DocumentMessageDateTime)
				{
					ZDateTime.TryParseExact(dTM.DateTimePeriod.DateTimePeriod, out importLicenseDate, "yyyyMMdd");
				}
			}
		}

		public ZDateTime importLicenseDate;
		public ZString importLicenseNumber;
		readonly SegmentGroup1 group1;
	}
}
