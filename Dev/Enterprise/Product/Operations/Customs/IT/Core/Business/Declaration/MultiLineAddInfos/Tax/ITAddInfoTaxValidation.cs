using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.IT.Business.Declaration;

public class ITAddInfoTaxValidation : EUAddInfoTaxValidation
{
	public ITAddInfoTaxValidation(Tax_OnlyForPivot parent) : base(parent)
	{
	}

	protected new Tax_OnlyForPivot Parent => (Tax_OnlyForPivot)base.Parent;

	protected override void CheckG4_Type()
	{
		base.CheckG4_Type();

		var pivot = Parent.Pivot;
		if (pivot != null && Parent.IsPortTax)
		{
			CheckPortTaxTypeHasBeenEnteredOnceIfNeeded(pivot.Taxes, Parent.G4_TypeInfo);
		}
	}

	void CheckPortTaxTypeHasBeenEnteredOnceIfNeeded(TaxForPivotCollection taxPivotCollection, ZPropertyInfo targetPropertyInfo)
	{
		var taxesData = taxPivotCollection.Cast<ITTaxOnlyForPivot>().Select(x => x.Data);
		if (taxesData.Any(x => x.IsPortTax && x.PK != Parent.PK))
		{
			targetPropertyInfo.AddError(ValidationCaptions.CusEntryLineFee.PortTaxCanOnlyBeEnteredOnce);
		}
	}
}
