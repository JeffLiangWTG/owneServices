using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaPackedItemEntryNumCollection : DependentBusinessObjectCollection<AsycudaPackedItemEntryNum, AsycudaPackedItem>
	{
		public AsycudaPackedItemEntryNumCollection(AsycudaPackedItem master)
			: base(master)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => CusEntryNumSchema.CE_ParentID;

		protected override void SetCollectionRelationships(BusinessObject dependent1)
		{
			base.SetCollectionRelationships(dependent1);
			var master = Master;
			var entryNum = (AsycudaPackedItemEntryNum)dependent1;
			entryNum.Parent = master;
			entryNum.CE_RN_NKCountryCode = master.CountryCode;
		}
	}
}
