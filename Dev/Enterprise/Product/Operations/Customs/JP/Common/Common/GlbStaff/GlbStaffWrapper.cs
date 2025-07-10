using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.Common
{
	public class GlbStaffWrapper : MasterFiles.Business.GlbStaffWrapper
	{
		protected GlbStaffWrapper(GlbStaff staff) : base(staff)
		{
		}

		public static GlbStaffWrapper Get(GlbStaff staff) => staff?.Factory.GetCachedValue($"JP.GlbStaffWrapper|{staff.PK}", () => new GlbStaffWrapper(staff));

		[ChildEditable]
		public GlbExternalPasswordCUSCollection PasswordCollection
		{
			get
			{
				if (passwordCollection == null)
				{
					passwordCollection = new GlbExternalPasswordCUSCollection(Staff);
					passwordCollection.Load();
					RegisterEditableChildObject(passwordCollection);
				}

				return passwordCollection;
			}
		}
		GlbExternalPasswordCUSCollection passwordCollection;
	}
}
