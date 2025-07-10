using CargoWise.EntityFramework;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaPackedItemEntryNumValidation : CusEntryNumValidation
	{
		public AsycudaPackedItemEntryNumValidation(AsycudaPackedItemEntryNum entryNumber)
			: base(entryNumber)
		{
		}

		public new AsycudaPackedItemEntryNum Parent => (AsycudaPackedItemEntryNum)base.Parent;

		protected override void CheckCE_EntryType()
		{
			base.CheckCE_EntryType();
			if (!Parent.CE_EntryNum.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CE_EntryTypeInfo);
			}
			var list = Parent.Lookups.CustomsEntryNumberTypes;
			if (list.Count > 0)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CE_EntryTypeInfo, list);
			}
		}
	}
}
