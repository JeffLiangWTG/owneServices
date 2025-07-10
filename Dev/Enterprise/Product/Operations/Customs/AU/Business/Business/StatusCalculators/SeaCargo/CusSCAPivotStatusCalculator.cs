
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class CusSCAPivotStatusCalculator : CMRStatusCalculator<CusSCAPivot>
	{
		public CusSCAPivotStatusCalculator(CusSCAPivot pivot)
			: base(pivot)
		{ }

		protected internal override ZPropertyInfo StatusInfo => Parent.CV_CargoStatusInfo;

		protected internal override ZString[] InterestedMessageTypes => new ZString[] { CMRMessage.CMRMessageTypes.CARST };

		protected override ZString GetDefaultStatus()
		{
			if (Parent.CV_CargoStatus == CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl)
			{
				var houseBill = Parent.HouseBill;
				if (houseBill != null)
				{
					var houseBillNumber = houseBill.CA_HouseBill;
					var associatedContainer = Parent.CV_AssociatedContainer;
					foreach (CusUnderbond underbond in Parent.AllUnderbonds)
					{
						if (underbond.OutturnHeader != null)
						{
							var outturnLine = underbond.OutturnHeader.Outturns.Cast<CusOutturn>().FirstOrDefault(x => x.C5_HouseBill == houseBillNumber && x.C5_ContainerNumber == associatedContainer);
							if (outturnLine != null && outturnLine.C5_OutturnResultType == CMROutturnResultType.Codes.SurplusConsignment)
							{
								return Parent.CV_CargoStatus;
							}
						}
					}
				}
			}
			return base.GetDefaultStatus();
		}
	}
}
