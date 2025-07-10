using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsLocationCollection : DocumentWrapperCollection
	{
		#region Constructors

		public DocWhsLocationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocWhsLocationCollection(List<WhsLocation> locations, BusinessObjectFactory factoryToWrap)
			: base(locations, factoryToWrap)
		{
		}

		public new DocWhsLocation this[int index]
		{
			get { return (DocWhsLocation)Elements[index]; }
		}

		#endregion
	}
}
