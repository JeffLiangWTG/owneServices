using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class BMComponentResourceLinkCollection : ActiveBusinessObjectCollection<BMComponentResourceLink>, IBMComponentResourceLinkCollection
	{
		public BMComponentResourceLinkCollection(BMComponent parent)
			: base(parent.Factory, parent, new ZQuery(), BMComponentResourceLinkSchema.FD_FC_Component)
		{
		}

		public BMComponentResourceLinkCollection(GlbStaff parent)
			: base(parent.Factory, parent, new ZQuery(), BMComponentResourceLinkSchema.FD_GS_NKResource)
		{
		}

		public BMComponentResourceLinkCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		IBMComponentResourceLink IBMComponentResourceLinkCollection.this[int index]
		{
			get { return base[index]; }
		}
	}
}
