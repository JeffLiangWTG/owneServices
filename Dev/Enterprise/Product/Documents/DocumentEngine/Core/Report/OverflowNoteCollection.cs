using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine
{
	public class OverflowNoteCollection : DocumentWrapperCollection<OverflowNote>
	{
		public OverflowNoteCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
