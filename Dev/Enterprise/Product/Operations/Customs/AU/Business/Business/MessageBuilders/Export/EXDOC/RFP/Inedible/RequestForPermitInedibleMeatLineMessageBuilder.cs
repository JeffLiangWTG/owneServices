using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RequestForPermitInedibleMeatLineMessageBuilder : RequestForPermitLineMessageBuilder
	{
		public RequestForPermitInedibleMeatLineMessageBuilder(SANCRTMessage sANCRT, string messageTypeToSend)
			: base(sANCRT, messageTypeToSend)
		{
		}

		protected override void GenerateBatchCode(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_BatchCode.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateGIN(group11.GIN.InstantiateAChildAndAddItToChildrenCollection(),
					IdentityNumberQualifierList.BatchNumber,
					invoiceLine.QuarantineExDocLine.QL_BatchCode);
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
