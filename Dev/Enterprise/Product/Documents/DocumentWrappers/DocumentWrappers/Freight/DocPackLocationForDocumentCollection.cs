using System.Collections;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocPackLocationForDocumentCollection : DocumentWrapperCollection
	{
		public DocPackLocationForDocumentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocPackLocationForDocument this[int index]
		{
			get { return (DocPackLocationForDocument)Elements[index]; }
		}

		public void SortOnWarehouseLocation()
		{
			this.Sort(new WarehouseLocationComparer());
		}
	}

	public class WarehouseLocationComparer : IComparer
	{
		public int Compare(object x, object y)
		{
			DocPackLocationForDocument locationA = (DocPackLocationForDocument)x;
			DocPackLocationForDocument locationB = (DocPackLocationForDocument)y;
			return locationA.WhsLocation.CompareTo(locationB.WhsLocation);
		}
	}
}
