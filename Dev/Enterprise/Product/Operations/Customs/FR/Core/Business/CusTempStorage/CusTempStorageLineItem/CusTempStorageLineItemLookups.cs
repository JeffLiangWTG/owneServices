using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class CusTempStorageLineItemLookups : EU.Business.CusTempStorage.CusTempStorageLineItemLookups
	{
		public CusTempStorageLineItemLookups(AutoCusTempStorageLineItem parent) : base(parent)
		{
		}

		public CodeDescriptionPairList WeightUQList => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);

		public RefCountryCollection CountryOfOrigins => new RefCountryCollection(Factory);
	}
}
