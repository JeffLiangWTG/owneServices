
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusHAWBStatusCalculator : CMRStatusCalculator<CusHAWBBase>
	{
		public CusHAWBStatusCalculator(CusHAWBBase hAWB) : base(hAWB)
		{
		}

		protected internal override ZPropertyInfo StatusInfo => Parent.CS_CustomsStatusInfo;

		protected internal override ZString[] InterestedMessageTypes => new ZString[] { CMRMessage.CMRMessageTypes.CARST };

		protected internal override ZString StatusChangedEventLogPrefix => "CusHAWB Customs Status - ";

		protected override ZString GetDefaultStatus()
		{
			var query = new ZQuery(CusOutturnSchema.C5_ParentID, Parent.PK);
			query.AddToFilter(CusOutturnSchema.C5_OutturnResultType, CMROutturnResultType.Codes.SurplusConsignment);
			var isSurplusConsignment = Parent.CS_CustomsStatus == CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl && Factory.LoadTop1<CusOutturn>(query) != null;
			return isSurplusConsignment ? Parent.CS_CustomsStatus : base.GetDefaultStatus();
		}
	}
}
