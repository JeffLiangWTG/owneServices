using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.ES;

namespace Enterprise.Customs.ES.Business
{
	public class GlbStaffWrapper : Enterprise.MasterFiles.Business.GlbStaffWrapper, IGlbStaffWrapper
	{
		protected GlbStaffWrapper(GlbStaff staff)
			: base(staff)
		{
		}
		public static GlbStaffWrapper Get(GlbStaff staff)
			=> staff?.Factory.GetCachedValue(staff.PK.ToString(), () => new GlbStaffWrapper(staff));

		#region ESBPasswordCollection
		[ChildEditable]
		public GlbExternalPasswordCollection ESBPasswordCollection
		{
			get
			{
				if (esGlbExternalPasswordCollection == null)
				{
					esGlbExternalPasswordCollection = new GlbExternalPasswordCollection(Staff);
					esGlbExternalPasswordCollection.Load();
					RegisterEditableChildObject(esGlbExternalPasswordCollection);
				}

				return esGlbExternalPasswordCollection;
			}
		}

		GlbExternalPasswordCollection esGlbExternalPasswordCollection;

		#endregion

		IGlbExternalPasswordCollection IGlbStaffWrapper.ESBPasswordCollection => ESBPasswordCollection;
	}
}
