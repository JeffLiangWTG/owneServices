using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public sealed class InvoiceApportionCharge : EU.Business.Declaration.InvoiceApportionCharge, Integration.Customs.DE.IInvoiceApportionCharge
	{
		public InvoiceApportionCharge(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : JobComInvCharge.Schema
		{
			public const string IsJ7_ExchangeRateIATA = nameof(InvoiceApportionCharge.IsJ7_ExchangeRateIATA);
		}

		[ResourceStringData("9BEA9822-42E4-459D-917B-1A1EE910B9FF", Caption = "IATA")]
		public ZBool IsJ7_ExchangeRateIATA => J7_ExchangeRateType == ChargeExchangeRateTypeList.Codes.IATARate;

		public ZPropertyInfo IsJ7_ExchangeRateIATAInfo => GetZPropertyInfo(Schema.IsJ7_ExchangeRateIATA);

		protected override void ResetExchangeRateData()
		{
			var isImport = Invoice?.IsImport ?? ZBool.False;
			if (!(isImport && J7_ChargeType.SupportsIATA() && IsJ7_ExchangeRateIATA))
			{
				base.ResetExchangeRateData();
			}
		}

		public new JobComInvoiceHeader Invoice => (JobComInvoiceHeader)base.Invoice;

		public new InvoiceApportionChargeLookups Lookups => (InvoiceApportionChargeLookups)base.Lookups;

		protected override JobComInvHeaderChargeLookups GetNewLookups() => new InvoiceApportionChargeLookups(this);

		public new InvoiceApportionChargeValidation Validation => (InvoiceApportionChargeValidation)base.Validation;

		protected override JobComInvHeaderChargeValidation GetNewValidation() => new InvoiceApportionChargeValidation(this);
	}
}
