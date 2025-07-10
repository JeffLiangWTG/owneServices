using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	public class EventSubscriptionCollection : DependentBusinessObjectCollection<GlbExternalPassword_BRS, GlbStaff>
	{
		public EventSubscriptionCollection(GlbStaff staff)
			: base(staff, new ZQuery(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.BRS))
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((GlbExternalPassword_BRS)child).GP_PasswordType = PasswordTypesList.Codes.BRS;
		}
	}
}
