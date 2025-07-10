using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentManagementLinkCollection : BusinessObjectCollection<IncidentManagementLink>
	{
		public IncidentManagementLinkCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public IncidentManagementLinkCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, additionalFilter)
		{
		}

		public void FireListReset()
		{
			base.FireListResetEvent();
		}

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new IncidentManagementLinkCollectionFetchStrategy(this);
		}

		public override void Add(BusinessObject businessObject)
		{
			base.Add(businessObject);
			var link = (IncidentManagementLink)businessObject;
			if (string.IsNullOrEmpty(link.INL_GS_NKResponder))
			{
				var incidentManagementGroup = Factory.Load<IncidentManagementGroup>(link.INL_ING_Group);
				if (incidentManagementGroup != null)
				{
					incidentManagementGroup.SetWorkItemsCascadeToIncident(link);
					if (!string.IsNullOrEmpty(incidentManagementGroup.ING_GS_NKGroupOwner))
					{
						link.INL_GS_NKResponder = incidentManagementGroup?.ING_GS_NKGroupOwner ?? ZString.Empty;
					}
				}
			}
		}

		protected override bool AllowNewCore => false;
	}
}
