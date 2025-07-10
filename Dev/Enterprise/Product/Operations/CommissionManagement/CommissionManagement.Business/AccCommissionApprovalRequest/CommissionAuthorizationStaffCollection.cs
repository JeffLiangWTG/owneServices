using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionAuthorizationStaffCollection : GlbStaffCollection, IFilterModuleExtraNotificationProvider
	{
		public CommissionAuthorizationStaffCollection(BusinessObjectFactory factory, Func<SecurityCore, SecurityCheckpoint> securityCheckpointGetter)
			: base(factory)
		{
			this.securityCheckpointGetter = securityCheckpointGetter;
		}

		readonly Func<SecurityCore, SecurityCheckpoint> securityCheckpointGetter;

		#region IFilterModuleExtraNotificationProvider Members

		public INotification GetExtraNotification(BusinessObject businessObject)
		{
			var staff = (GlbStaff)businessObject;
			var security = new SecurityCore(null, staff, Guid.Empty, Guid.Empty, Guid.Empty);
			var securityCheckpoint = securityCheckpointGetter(security);

			if (!securityCheckpoint.IsAllowed)
			{
				return new Notification(CargoWise.EntityFramework.NotificationType.Error, Res.GetString("570e2f88-247e-45e2-bc40-f5e745a7b36f", "Staff does not have the appropriate security rights to approve this."));
			}

			return null;
		}

		#endregion
	}
}
