using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CusHAWBBaseAIRCRAmendmentGenerator : CMRAmendmentGenerator
	{
		public CusHAWBBaseAIRCRAmendmentGenerator(CusHAWBBase hAWB)
			: base(hAWB)
		{
			HAWB = hAWB;
		}

		protected readonly CusHAWBBase HAWB;

		protected internal override EDIMessageCollection GetMesssageCollection(BusinessObject bizo) => (bizo as CusHAWBBase).Messages;

		protected override ZPropertyInfo[] UniqueIdentifierInfos => new ZPropertyInfo[] { HAWB.MAWB.CM_FlightNoInfo, HAWB.MAWB.CM_ArrivalDateInfo, HAWB.CS_HAWBInfo };
	}
}
