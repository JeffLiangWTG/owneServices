using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.JP.Business
{
	public partial class JobComInvoiceGroupHeader : AutoJobComInvoiceGroupHeader
	{
		public JobComInvoiceGroupHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override string GetStandaloneIncoTermAndChargeFactoryCountryContext() => this.GetCountryContext();

		public override ZString JZ_RX_NKInvoice_Currency
		{
			get => base.JZ_RX_NKInvoice_Currency;
			set
			{
				base.JZ_RX_NKInvoice_Currency = value;
				Charges.MarkAsNeedingValidation();
			}
		}
	}
}
