
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocBillOfLadingBodySectionCollection : DocumentWrapperCollection
	{
		public DocBillOfLadingBodySectionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocBillOfLadingBodySection this[int index]
		{
			get { return (DocBillOfLadingBodySection)base[index]; }
		}
	}
}
