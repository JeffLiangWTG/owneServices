using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RequestForPermitWoolLineMessageBuilder : RequestForPermitLineMessageBuilder
	{
		public RequestForPermitWoolLineMessageBuilder(SANCRTMessage sANCRT, string messageTypeToSend) : base(sANCRT, messageTypeToSend)
		{
		}

		protected override void GenerateExtraCertificate(SegmentGroup12 group12)
		{
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
