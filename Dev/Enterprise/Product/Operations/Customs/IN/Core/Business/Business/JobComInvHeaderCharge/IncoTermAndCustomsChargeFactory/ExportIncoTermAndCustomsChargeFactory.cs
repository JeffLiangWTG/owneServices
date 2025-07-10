using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.IN.Business;

public class ExportIncoTermAndCustomsChargeFactory : CommonIncoTermAndCustomsChargeFactory
{
	protected override ICustomsChargeCode[] GetCharges()
	{
		var chargeCodes = base.GetCharges().Cast<CustomsChargeCode>().ToArray();

		// TODO: Remove this line when ParentTypes are set on CustomsChargeCode
		chargeCodes.ForEach(x => x.ParentTypes = ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice);

		return chargeCodes;
	}
}
