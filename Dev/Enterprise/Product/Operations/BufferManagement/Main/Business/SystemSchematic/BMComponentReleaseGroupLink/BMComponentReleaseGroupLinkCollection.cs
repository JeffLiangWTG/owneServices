using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class BMComponentReleaseGroupLinkCollection : ActiveBusinessObjectCollection<BMComponentReleaseGroupLink>, IBMComponentReleaseGroupLinkCollection
	{
		public BMComponentReleaseGroupLinkCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}

		public BMComponentReleaseGroupLinkCollection(BMComponent master)
			: base(master.Factory, master, new ZQuery(), BMComponentReleaseGroupLinkSchema.FO_FC_Component)
		{
		}

		public BMComponentReleaseGroupLinkCollection(GlbGroup master)
			: base(master.Factory, master, new ZQuery(), BMComponentReleaseGroupLinkSchema.FO_GG_ReleaseGroup)
		{
		}

		#region IBMComponentReleaseGroupLinkCollection

		IBMComponentReleaseGroupLink IBMComponentReleaseGroupLinkCollection.this[int index]
		{
			get { return this[index]; }
		}

		#endregion
	}
}
