using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentManagementGroupMessageCollection : BusinessObjectCollection<IncidentManagementGroupMessage>
	{
		readonly ZGuid groupPk;

		public IncidentManagementGroupMessageCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public IncidentManagementGroupMessageCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, additionalFilter)
		{
		}

		public IncidentManagementGroupMessageCollection(BusinessObjectFactory factory, ZGuid groupPk)
			: base(factory, new ZQuery(IncidentManagementGroupMessageSchema.IGM_ING_Group, groupPk))
		{
			this.groupPk = groupPk;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((IncidentManagementGroupMessage)child).IGM_ING_Group = groupPk;
		}
	}
}
