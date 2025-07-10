using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CH;

namespace Enterprise.Customs.CH.Business;

public class TobaccoCollection : CusSupportingInfoCollection<Tobacco>
{
	public TobaccoCollection(BusinessObject parent)
		: base(parent, CusSupportingInfoTypeList.Codes.Tobacco)
	{
		RefreshMaxCount();
	}

	public void RefreshMaxCount()
	{
		var maxCount = InvoiceLine.Declaration?.IsExport ?? false ? 1 : -1;
		this.EnableMaxCountValidation(maxCount, warnAtHalfway: false, notificationType: NotificationType.MessageError, PassarValidationMessages.MessageNS30104_Tobacco);
	}

	JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)Master;
}
