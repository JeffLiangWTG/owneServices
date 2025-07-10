using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class ExitControlMessageSendingNotificationCollector : CustomsNotificationCollector
	{
		public ExitControlMessageSendingNotificationCollector(CusExitHeader cusExitHeader, IEnumerable<CusExitReport> selectedReports) : base(cusExitHeader, true, false, PropertyDescriptionType.HumanReadableName)
		{
			this.selectedReports = selectedReports;
		}

		readonly IEnumerable<CusExitReport> selectedReports;

		protected override bool ShouldIncludeNotificationsFromObject(BusinessObject businessObject)
		{
			var result = base.ShouldIncludeNotificationsFromObject(businessObject);

			if (result)
			{
				switch (businessObject)
				{
					case CusExitReport _:
						result = selectedReports.Any(x => x.PK == businessObject.PK);
						break;
					case CusExitConsignment _:
						result = selectedReports.Any(x => x.CER_CXC_Consignment == businessObject.PK);
						break;
					case CusExitHeader _:
						result = selectedReports.Any(x => x.CER_CXH_Header == businessObject.PK);
						break;
					case CusExitContainer _:
						result = selectedReports.Any(x => x.Consignment.CusExitConsignmentItems.Any(item => item.CusExitConsignmentContainerPivots.Any(p => p.CNP_CXN_Container == businessObject.PK)));
						break;
					case CusExitConsignmentPackage _:
						result = selectedReports.Any(x => x.Consignment.CusExitConsignmentItems.Any(item => item.CusExitConsignmentPackagePivots.Any(p => p.CNP_CXP_Package == businessObject.PK)));
						break;
				}
			}

			return result;
		}
	}
}
