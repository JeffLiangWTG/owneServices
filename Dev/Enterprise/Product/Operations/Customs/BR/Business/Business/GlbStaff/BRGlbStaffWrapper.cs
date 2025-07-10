using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.BR;

namespace Enterprise.Customs.BR.Business
{
	public class BRGlbStaffWrapper : GlbStaffWrapper, IBRGlbStaffWrapper
	{
		public BRGlbStaffWrapper(GlbStaff staff)
			: base(staff)
		{
		}

		public static BRGlbStaffWrapper Get(GlbStaff staff)
		{
			return staff?.Factory.GetCachedValue(staff.PK.ToString(), () => new BRGlbStaffWrapper(staff));
		}

		public GlbExternalPassword_CCT CCTPassword
		{
			get
			{
				if (glbExternalPassword == null)
				{
					glbExternalPassword = GetGlbExternalPasswordOrCreateNew<GlbExternalPassword_CCT>(PasswordTypesList.Codes.CCT, GlbCompany.CurrentCompany.PK);
					RegisterEditableChildObject(glbExternalPassword);
				}

				return glbExternalPassword;
			}
		}

		GlbExternalPassword_CCT glbExternalPassword;

		IGlbExternalPassword IBRGlbStaffWrapper.CCTPassword => GetCCTPassword();

		public GlbExternalPassword_CCT GetCCTPassword()
		{
			return GetGlbExternalPassword<GlbExternalPassword_CCT>(PasswordTypesList.Codes.CCT, GlbCompany.CurrentCompany.PK);
		}

		#region BRSPasswordCollection

		[ChildEditable]
		public EventSubscriptionCollection EventSubscriptions
		{
			get
			{
				if (eventSubscriptionCollection == null)
				{
					eventSubscriptionCollection = new EventSubscriptionCollection(Staff);
					eventSubscriptionCollection.Load();
					RegisterEditableChildObject(eventSubscriptionCollection);
				}

				return eventSubscriptionCollection;
			}
		}

		EventSubscriptionCollection eventSubscriptionCollection;

		#endregion
	}
}
