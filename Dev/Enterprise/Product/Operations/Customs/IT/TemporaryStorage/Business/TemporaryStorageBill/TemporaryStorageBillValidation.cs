using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public class TemporaryStorageBillValidation : EU.Business.CusTempStorage.TemporaryStorageBillValidation
{
	public TemporaryStorageBillValidation(TemporaryStorageBill parent) : base(parent)
	{
	}

	protected override void CheckABL_GoodsDescription()
	{
		base.CheckABL_GoodsDescription();
		var parent = Parent;
		var hasFormattedTariff = parent.PackedItems.Any(x => !x.API_FormattedTariff.IsEmpty);

		if (parent.ABL_GoodsDescription.IsEmpty && !hasFormattedTariff)
		{
			parent.ABL_GoodsDescriptionInfo.AddMessageError(ValidationCaptions.TemporaryStorageBill.GoodsDescriptionOrTariffMustBeThere);
		}
	}

	protected override void CheckABL_BillNumber()
	{
		base.CheckABL_BillNumber();
		if (Parent.Packs.Count == 0)
		{
			Parent.ABL_BillNumberInfo.AddMessageError(ValidationCaptions.TemporaryStorageBill.PacksLineMustBeThere);
		}
	}

	protected override void CheckDuplicateTypeAndNumber(ZPropertyInfo propertyInfo)
	{
		if (Parent.Header is TemporaryStorageHeader header
			&&  header.DuplicatedBillNumbers.Contains(Parent.ABL_BillNumber))
		{
			propertyInfo.AddWarning(ValidationCaptions.TemporaryStorageBill.DuplicatedBills);
		}
	}
}
