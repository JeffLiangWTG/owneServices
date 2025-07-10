using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class ABLEntryNumCollection : DependentBusinessObjectCollection<ABLEntryNum, AsycudaBill>
	{
		public ABLEntryNumCollection(AsycudaBill master)
			: base(master)
		{
		}

		protected override void SetCollectionRelationships(BusinessObject dependent1)
		{
			base.SetCollectionRelationships(dependent1);
			var master = Master;
			var entryNum = (ABLEntryNum)dependent1;
			entryNum.Parent = master;
			entryNum.CE_RN_NKCountryCode = master.CountryCode;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var entryNum = (ABLEntryNum)child;
			var customsEntryNumberTypes = entryNum.Lookups.CustomsEntryNumberTypes.GetAllCodes();
			if (customsEntryNumberTypes != null && customsEntryNumberTypes.Length == 1)
			{
				entryNum.CE_EntryType = customsEntryNumberTypes[0];
			}
			else if (Master?.Header?.SupportMultipleCustomsNumbers ?? false)
			{
				entryNum.CE_EntryType = Master.FilterForSingleEntryType;
			}
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => CusEntryNumSchema.CE_ParentID;
	}
}
