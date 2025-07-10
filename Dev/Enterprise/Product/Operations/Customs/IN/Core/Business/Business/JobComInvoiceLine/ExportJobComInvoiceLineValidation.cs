using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Business;

public class ExportJobComInvoiceLineValidation : JobComInvoiceLineValidation
{
	public ExportJobComInvoiceLineValidation(JobComInvoiceLine parent) : base(parent)
	{
	}

	protected override void CheckJI_EndUse()
	{
		base.CheckJI_EndUse();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_EndUseInfo);
	}

	protected override void CheckJI_Description()
	{
		var exceededFromRequiredLimit = Res.GetString("5CA5F565-9E37-4B6C-87B2-B79DEDD06ACC", "Goods Description is more than 120 Char. Only first 120 Characters will be used for SB filling.");
		base.CheckJI_Description();
		if (Parent.JI_Description.Length > 120)
		{
			Parent.JI_DescriptionInfo.AddWarning(exceededFromRequiredLimit);
		}
	}
	protected override void CheckJI_PMV()
	{
		base.CheckJI_PMV();
		var parent = Parent;
		if (parent.JI_ValuationMarkup.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.JI_PMVInfo);
		}
	}

	protected override void CheckJI_UnitPrice()
	{
		base.CheckJI_UnitPrice();
		var parent = Parent;
		var info = parent.JI_UnitPriceInfo;
		MandatoryValidation.MessageErrorIfNotEntered(info);
	}

	protected override void CheckJI_UnitQuantity()
	{
		base.CheckJI_UnitQuantity();
		var parent = Parent;
		var info = parent.JI_UnitQuantityInfo;
		MandatoryValidation.MessageErrorIfNotEntered(info);
	}

	protected override void CheckJI_UnitUQ()
	{
		base.CheckJI_UnitUQ();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_UnitUQInfo);
	}

	protected override void CheckJI_AccessoryStatus()
	{
		base.CheckJI_AccessoryStatus();
		ListValidation.MessageErrorIfInvalidCode(Parent.JI_AccessoryStatusInfo);
	}

	protected override void CheckAccessoryDescription()
	{
		var parent = Parent;
		if (!parent.AccessoryDescription_ReadOnly)
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.AccessoryDescriptionInfo);
		}
	}

	protected override void CheckJI_StateOrRegionOfOrigin()
	{
		base.CheckJI_StateOrRegionOfOrigin();
		ListValidation.MessageErrorIfInvalidCode(Parent.JI_StateOrRegionOfOriginInfo);
	}

	protected override void CheckJI_RewardItem()
	{
		base.CheckJI_RewardItem();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_RewardItemInfo);
	}

	protected override void CheckJI_RN_NKCountryOfTransit()
	{
		base.CheckJI_RN_NKCountryOfTransit();
		ListValidation.MessageErrorIfInvalidCode(Parent.JI_RN_NKCountryOfTransitInfo);
	}

	protected override void CheckJI_InvoiceQuantity()
	{
		base.CheckJI_InvoiceQuantity();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_InvoiceQuantityInfo);
	}

	protected override void CheckJI_InvoiceUQ()
	{
		base.CheckJI_InvoiceUQ();
		var parent = Parent;
		var info = parent.JI_InvoiceUQInfo;
		var invoiceUQ = parent.JI_InvoiceUQ;
		MandatoryValidation.MessageErrorIfNotEntered(info, Res.GetString("5FB6A3F0-4A1E-4C06-8220-6CB81A2D0AD0", "UOM"));
		if (!invoiceUQ.IsEmpty && !parent.Lookups.InvoiceUQList.ContainsCode(invoiceUQ) && RefCusPackListProvider.GetRefCusPackList(parent.Factory, invoiceUQ).Count == 0)
		{
			info.AddMessageError(Res.GetString("1201886F-E977-421E-86DB-A9C1BB78B007", "Package type {0} does not map to a Customs Package Type for country IN. Please add the mapping via Maintain > Customs > Customs Files > Packs Conversion.", invoiceUQ));
		}
	}
}
