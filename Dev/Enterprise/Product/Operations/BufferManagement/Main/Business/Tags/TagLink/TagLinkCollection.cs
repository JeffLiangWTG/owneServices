using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class TagLinkCollection : ActiveBusinessObjectCollection<TagLink>, ITagLinkCollection
	{
		public TagLinkCollection(ITagable parent)
			: base(parent.Factory, (BusinessObject)parent, new ZQuery(), TagLinkSchema.TGL_ParentId)
		{
		}

		public TagLinkCollection(TagMagnitude parent)
			: base(parent.Factory, parent, new ZQuery(), TagLinkSchema.TGL_TGM_Magnitude)
		{
		}

		ITagLink ITagLinkCollection.this[int index]
		{
			get { return base[index]; }
		}
	}
}
