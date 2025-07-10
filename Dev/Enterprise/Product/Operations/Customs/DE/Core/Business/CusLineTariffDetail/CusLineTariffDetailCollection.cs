using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public class CusLineTariffDetailCollection : Customs.Business.CusLineTariffDetailCollection<CusLineTariffDetail>
	{
		public CusLineTariffDetailCollection(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var cusLineTariffDetail = (Customs.Business.CusLineTariffDetail)child;

			cusLineTariffDetail.BZ_Type = Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Excise;
		}
	}
}
