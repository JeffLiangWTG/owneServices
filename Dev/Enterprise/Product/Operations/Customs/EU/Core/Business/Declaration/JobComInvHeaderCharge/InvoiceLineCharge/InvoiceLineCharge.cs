using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class InvoiceLineCharge : TypeSafeInvoiceLineCharge, Integration.Customs.EU.IInvoiceLineCharge
	{
		public InvoiceLineCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void DefaultIsIncludedInInvoice(IncoTermAndCustomsChargeFactory incoTermAndChargeFactory, ICustomsChargeCode charge)
		{
			if (charge != null && charge.Code == ChargeTypeList.Codes.StatisticalValue)
			{
				J7_Calc_IsIncludedInInvoiceAmount = false;
			}
			else
			{
				base.DefaultIsIncludedInInvoice(incoTermAndChargeFactory, charge);
			}
		}
	}
}
