using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public static class LookupsHelper
	{
		#region GetPortCollectionCore

		public static RefUNLOCOCollection GetPortCollectionCore(BusinessObjectFactory factory, ZPropertyInfo codeInfo, bool isAir)
		{
			var collection = new RefUNLOCOCollection(factory);
			if (isAir)
			{
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("IATA Code", "Property", codeInfo.Value, true));
			}

			return collection;
		}

		#endregion
	}
}
