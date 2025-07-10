using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.IL;

namespace Enterprise.Customs.IL.Business
{
	public class GlbStaffWrapper : MasterFiles.Business.GlbStaffWrapper, IILGlbStaffWrapper
	{
		public GlbStaffWrapper(GlbStaff staff) : base(staff)
		{
		}

		public static GlbStaffWrapper Get(GlbStaff staff)
		{
			return staff?.Factory.GetCachedValue(staff.PK.ToString(), () => new GlbStaffWrapper(staff));
		}

		#region PasswordCollection

		public GlbILStaffExternalPasswordCollection PasswordCollection
		{
			get
			{
				if (passwordCollection == null)
				{
					passwordCollection = new GlbILStaffExternalPasswordCollection(Staff);
					passwordCollection.Load();
					RegisterEditableChildObject(passwordCollection);
				}

				return passwordCollection;
			}
		}

		GlbILStaffExternalPasswordCollection passwordCollection;

		#endregion

	}
}
