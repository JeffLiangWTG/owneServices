using CargoWise.EntityFramework;
namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusHAWBAIRCRAmendmentGenerator : CusHAWBBaseAIRCRAmendmentGenerator
	{
		public CusHAWBAIRCRAmendmentGenerator(CusHAWB hAWB)
			: base(hAWB)
		{
		}

		protected internal override CMRMessageBuilder GetBuilder(BusinessObject bizo) => new AIRCRMessageBuilder(bizo as CusHAWB);

		protected override ZPropertyInfo[] UniqueIdentifierInfos => new ZPropertyInfo[] { HAWB.MAWB.CM_FlightNoInfo, HAWB.MAWB.CM_ArrivalDateInfo, HAWB.MAWB.CM_MAWBInfo, HAWB.CS_HAWBInfo };
	}
}
