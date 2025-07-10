using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsStocktakeLineCollection : DocumentWrapperCollection
	{
		public DocWhsStocktakeLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocWhsStocktakeLineCollection(WhsStocktakeLineCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocWhsStocktakeLine this[int index]
		{
			get { return (DocWhsStocktakeLine)base[index]; }
		}
	}
}
