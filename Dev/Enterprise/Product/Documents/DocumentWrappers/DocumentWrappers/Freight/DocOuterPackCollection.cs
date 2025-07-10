using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocOuterPackCollection : DocumentWrapperCollection
	{
		public DocOuterPackCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocOuterPack this[int index]
		{
			get { return (DocOuterPack)base[index]; }
		}

		// Published - used in AWB labels
		public ZString PieceDesc
		{
			get { return Res.GetString("cb06695f-ddad-45b0-8b83-e0191d3e4fa8", "of {0}", Count.ToString()); }
		}

		public new ZInt Count
		{
			get { return base.Count; }
		}
	}
}
