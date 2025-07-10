using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaPackFetchStrategy : ManifestBase.AsycudaPackFetchStrategy
	{
		public AsycudaPackFetchStrategy(AsycudaPack pack)
			: base(pack)
		{
		}

		protected new AsycudaPack BusinessObject => (AsycudaPack)base.BusinessObject;

		protected override void AddFetchHintsForDeleteCore()
		{
			base.AddFetchHintsForDeleteCore();
			var businessObjectPK = BusinessObject.PK;
			Factory.AddFetchHint(GenAddOnColumnSchema.XA_ParentID, businessObjectPK);
			Factory.AddFetchHint(UNDGDataItemSchema.DI_ParentID, businessObjectPK);
		}

		protected override IEnumerable<BusinessObject> LoadChildrenForDelete()
		{
			var genAddOnColumns = Factory.Load<GenAddOnColumn>(QueryHelper.QueryWithFetchOnlyFromLocalCache(GenAddOnColumnSchema.XA_ParentID, BusinessObject.PK, BusinessObject.IsInDatabase));
			return base.LoadChildrenForDelete().Union(genAddOnColumns);
		}

		protected override void AddFetchHintsForLoadChildEditableObjectsCore()
		{
			base.AddFetchHintsForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(UNDGDataItemSchema.DI_ParentID, BusinessObject.PK);
		}

		protected override void AddFetchHintsForValidateCore()
		{
			base.AddFetchHintsForValidateCore();
			Factory.AddFetchHint(GenAddOnColumnSchema.XA_ParentID, BusinessObject.PK);
		}
	}
}
