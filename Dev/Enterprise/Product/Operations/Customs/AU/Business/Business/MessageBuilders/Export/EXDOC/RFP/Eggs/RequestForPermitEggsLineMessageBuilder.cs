using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RequestForPermitEggsLineMessageBuilder : RequestForPermitLineMessageBuilder
	{
		public RequestForPermitEggsLineMessageBuilder(SANCRTMessage sANCRT, string messageTypeToSend)
			: base(sANCRT, messageTypeToSend)
		{
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

		protected override void GenerateProductSourceState(SegmentGroup11 group11)
		{
			if (!invoiceLine.JI_AUState.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateLOC(group11.LOC.InstantiateAChildAndAddItToChildrenCollection(),
					PlaceLocationQualifierList.MutuallyDefined,
					invoiceLine.JI_AUState);
			}
		}
	}
}
