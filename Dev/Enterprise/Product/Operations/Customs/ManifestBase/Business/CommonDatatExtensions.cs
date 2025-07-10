using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ManifestBase.Extensions
{
	public static class CommonDatatExtensions
	{
		public static bool IsRowDetached(this BusinessObject bizObj)
		{
			var row = (bizObj as INeedRow)?.Row;
			return row != null && row.RowState == System.Data.DataRowState.Detached;
		}

		public static CodeDescriptionPairList GetPackageTypeList(this BusinessObjectFactory factory)
		{
			return RefPackTypeCollection.GetAsCodeDescriptionPairWithStandardUnits(factory);
		}

		public static CodeDescriptionPairList GetWeightUQList(this BusinessObjectFactory factory)
		{
			return factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);
		}

		public static CodeDescriptionPairList GetVolumeUQList(this BusinessObjectFactory factory)
		{
			return factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume);
		}
	}
}
