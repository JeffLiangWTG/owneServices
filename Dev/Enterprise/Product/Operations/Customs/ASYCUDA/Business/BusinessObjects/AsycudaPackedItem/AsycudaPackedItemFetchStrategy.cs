using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaPackedItemFetchStrategy : AsycudaFetchStrategy
	{
		public AsycudaPackedItemFetchStrategy(AsycudaPackedItem packedItem)
			: base(packedItem)
		{
		}

		new protected AsycudaPackedItem BusinessObject => (AsycudaPackedItem)base.BusinessObject;

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case AsycudaPackedItem.Schema.RegistrationNumber:
						Factory.AddFetchHint(typeof(AsycudaPackedItemEntryNum), CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
						break;
				}
			}
		}

		protected override void AddFetchHintsForValidateCore()
		{
			base.AddFetchHintsForValidateCore();
			Factory.AddFetchHint(GenAddOnColumnSchema.XA_ParentID, BusinessObject.PK);
		}

		protected override void AddFetchHintsForDeleteCore()
		{
			base.AddFetchHintsForDeleteCore();
			Factory.AddFetchHint(typeof(AsycudaPackedItemEntryNum), CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(UNDGDataItemSchema.DI_ParentID, BusinessObject.PK);
		}

		protected override IEnumerable<BusinessObject> LoadChildrenForDelete() => base.LoadChildrenForDelete()
			.Union(BusinessObject.CustomsEntryNumbers);

		protected override void AddFetchHintsForLoadChildEditableObjectsCore()
		{
			base.AddFetchHintsForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(typeof(AsycudaPackedItemEntryNum), CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(UNDGDataItemSchema.DI_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(typeof(AsycudaPackPackedItemPivot), AsycudaPackPackedItemPivotSchema.APP_API_Item, BusinessObject.PK);
			Factory.AddFetchHint(typeof(AsycudaTax), AsycudaTaxSchema.AET_API_AsycudaPackedItem, BusinessObject.PK);
		}
	}
}
