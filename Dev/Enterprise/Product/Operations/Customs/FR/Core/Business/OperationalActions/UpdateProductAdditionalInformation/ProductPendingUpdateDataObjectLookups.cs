using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.OperationalActions
{
	public class ProductPendingUpdateDataObjectLookups : ZLookups
	{
		public ProductPendingUpdateDataObjectLookups(ProductPendingUpdateDataObject parent) : base(parent)
		{
		}

		#region Products

		public OrgSupplierPartCollection Products => new OrgSupplierPartCollection(Factory);

		#endregion

		#region CustomsTypes

		public CodeDescriptionPairList CustomsTypes => Factory.GetCachedValue<ProductFilesImpExpList>();

		#endregion

		#region RelatedOrgs

		public OrgHeaderCollection RelatedOrgs
		{
			get
			{
				var consigneeOrConsignorCollection = new OrgHeaderCollection(Factory);
				consigneeOrConsignorCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property2", ZBool.True));
				consigneeOrConsignorCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property3", ZBool.True));
				consigneeOrConsignorCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "OrJoinCondition", ZBool.True));
				consigneeOrConsignorCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "AndJoinCondition", ZBool.False));
				return consigneeOrConsignorCollection;
			}
		}

		#endregion
	}
}
