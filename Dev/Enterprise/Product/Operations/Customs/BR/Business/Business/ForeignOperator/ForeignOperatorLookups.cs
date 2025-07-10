using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public class ForeignOperatorLookups : Customs.Business.CusGoodsCatalogProductionInfoLookups
	{
		public ForeignOperatorLookups(ForeignOperator parent) : base(parent)
		{
		}

		new ForeignOperator Parent => base.Parent as ForeignOperator;

		public RefCountryCollection Countries => new (Factory);

		public CodeDescriptionPairList CustomsStatusList => Factory.GetCachedValue<CustomsPostedStatusList>();

		public CusBRForeignOperatorCollection Manufacturers
		{
			get
			{
				var ownerPK = Parent.GoodsCatalog?.CGC_OH_Owner ?? ZGuid.Empty;
				var collection = new CusBRForeignOperatorCollection(Factory, ownerPK);
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.FilterConstants.ForeignOperator.Owner, "Property", ownerPK, false));
				return collection;
			}
		}
	}
}
