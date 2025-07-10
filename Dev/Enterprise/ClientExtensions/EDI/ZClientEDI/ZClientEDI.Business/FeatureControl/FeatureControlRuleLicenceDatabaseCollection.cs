using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.ComponentModel;

namespace Enterprise.Client.EDI.FeatureControl.Business
{
	[ModuleID("LicenceDatabase")]
	public class FeatureControlRuleLicenceDatabaseCollection : ActiveBusinessObjectCollection<LicenceDatabase>, IFilterModuleExtraNotificationProvider
	{
		public FeatureControlRuleLicenceDatabaseCollection(FeatureControlRule parent, ZQuery query)
			: base(parent.Factory, query)
		{
			Parent = parent;
		}

		protected override void SetRelationshipDefaultsForElementCore(LicenceDatabase newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, false);
		}

		readonly FeatureControlRule Parent;

		INotification IFilterModuleExtraNotificationProvider.GetExtraNotification(BusinessObject businessObject)
		{
			INotification notification = null;

			if (businessObject is LicenceDatabase licenceDatabase && !Parent.CanAttachLicenceDatabase(licenceDatabase, out var errorMessage))
			{
				notification = new Notification(CargoWise.ComponentModel.NotificationType.Error, errorMessage);
			}

			return notification;
		}
	}
}
