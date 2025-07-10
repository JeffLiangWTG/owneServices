using System.Globalization;
using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;
using Enterprise.Edifact.D97BAU.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RequestForPermitSkinsAndHidesLineMessageBuilder : RequestForPermitLineMessageBuilder
	{
		public RequestForPermitSkinsAndHidesLineMessageBuilder(SANCRTMessage sANCRT, string messageTypeToSend)
			: base(sANCRT, messageTypeToSend)
		{
		}

		protected override void GenerateSaltingDate(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_SaltingDate.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateDTM(group11.DTM.InstantiateAChildAndAddItToChildrenCollection(),
					DateTimePeriodQualifierList.ProcessingDateTime,
					invoiceLine.QuarantineExDocLine.QL_SaltingDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
					DateTimePeriodFormatQualifierList.Ccyymmdd);
			}
		}

		protected override void GenerateProductSourceState(SegmentGroup11 group11)
		{
			if (!invoiceLine.JI_AUState.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateLOC(group11.LOC.InstantiateAChildAndAddItToChildrenCollection(),
					PlaceLocationQualifierList.MutuallyDefined,
					invoiceLine.JI_AUState);
			}
		}

		protected override void GenerateCustomsWeights(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_AqisCustomsWeightUQ.IsEmpty && invoiceLine.QuarantineExDocLine.QL_AqisCustomsWeight > 0)
			{
				MEASegment mea = group11.MEA.InstantiateAChildAndAddItToChildrenCollection();
				mea.MeasurementPurposeQualifier = MeasurementPurposeQualifierList.CustomsLineItemMeasurement;  //  MEA 6311, AAF
				mea.MeasurementDetails.PropertyMeasuredCoded = PropertyMeasuredCodedList.ShippedQuantity;  //  MEA C502/6313, SQ
				mea.ValueRange.MeasureUnitQualifier = invoiceLine.QuarantineExDocLine.QL_AqisCustomsWeightUQ; // MEA C174/6411
				mea.ValueRange.MeasurementValue = invoiceLine.QuarantineExDocLine.QL_AqisCustomsWeight.ToString(); // MEA C174/6314
			}
		}
	}
}
