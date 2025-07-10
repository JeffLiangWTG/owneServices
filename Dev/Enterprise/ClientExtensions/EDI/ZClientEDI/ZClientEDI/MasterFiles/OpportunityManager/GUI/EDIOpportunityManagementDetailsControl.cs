using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public class EDIOpportunityManagementDetailsControl : OpportunityManagementDetailsControl
	{
		protected override ControllerID GetPluginControllerID()
		{
			return ClientControllerRegistration.EDIOpportunityValueAnalysis;
		}

		protected override ZUserControl CreateValueEstimationControl()
		{
			return new OpportunityValueEstimationUserControl();
		}

		public new EDIOrgOpportunity CurrentDataItem
		{
			get { return (EDIOrgOpportunity)base.CurrentDataItem; }
		}

		protected override IEnumerable<ZPropertyInfo> CurrencyPropertyInfos
		{
			get
			{
				return base.CurrencyPropertyInfos.Union(new[] { CurrentDataItem?.OrgOpportunityEx?.EOM_RX_NKLifetimeValueCurrencyInfo }).Where(x => x != null);
			}
		}

		protected override SalesRelationControl CreateSalesRelationControl()
		{
			return new EDIOrgOpportunitySalesRelationControl();
		}
	}
}
