using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business;

public class SupplementaryCodePropertyChangedNotifier : BaseSupplementaryCodePropertyChangedNotifier
{
	public SupplementaryCodePropertyChangedNotifier(BaseSupplementaryCode parentSupplementaryCode) : base(parentSupplementaryCode)
	{
	}

	protected override void NotifyPropertyCY_CodeChanged(ZString oldValue, ZString newValue)
	{
		base.NotifyPropertyCY_CodeChanged(oldValue, newValue);

		if (Parent.SupplementaryCodeSupporter is JobComInvoiceLine invoiceLine && !invoiceLine.IsTaxTypeDefaultingSuspended && !invoiceLine.HasMoreThanOneVatQVatAdditionalCode())
		{
			var supplementaryCodes = invoiceLine.SupplementaryCodes;
			if (SupplementaryCodeHelper.IsQVatSupplementaryCode(newValue))
			{
				var vatApplicability = invoiceLine.GetEffectiveVATApplicabilities().FirstOrDefault(x => x.ZX5_AdditionalCode == newValue);
				invoiceLine.JI_ZZF_NKTaxType = vatApplicability?.ZX5_ZZF_NKTaxOrFeeCode ?? ZString.Empty;
			}
			else if (newValue.IsEmpty && SupplementaryCodeHelper.IsQVatSupplementaryCode(oldValue))
			{
				invoiceLine.JI_ZZF_NKTaxType = ZString.Empty;
			}
		}
	}
}
