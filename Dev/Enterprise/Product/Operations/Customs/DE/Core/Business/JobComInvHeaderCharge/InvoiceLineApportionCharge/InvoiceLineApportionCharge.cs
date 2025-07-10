using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public sealed class InvoiceLineApportionCharge : EU.Business.Declaration.InvoiceLineApportionCharge, Integration.Customs.DE.IInvoiceLineApportionCharge
	{
		public InvoiceLineApportionCharge(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("36E05302-98F1-4308-8CFB-B8D373529CD5", Caption = "IATA")]
		public ZBool IsJ7_ExchangeRateIATA
		{
			get { return J7_ExchangeRateType == ChargeExchangeRateTypeList.Codes.IATARate; }
			set
			{
				if (IsJ7_ExchangeRateIATA != value)
				{
					if (value)
					{
						J7_ExchangeRateType = ChargeExchangeRateTypeList.Codes.IATARate;
					}
					else if (IsJ7_ExchangeRateIATA)
					{
						J7_ExchangeRateType = ZString.Empty;
					}
					IsJ7_ExchangeRateIATAInfo.RefreshBinding();

					if (IsZeroPercentageAndNotCopying)
					{
						MarkApportionmentDirty();
					}
				}
			}
		}

		public ZPropertyInfo IsJ7_ExchangeRateIATAInfo => GetZPropertyInfo(nameof(IsJ7_ExchangeRateIATA));

		protected override void ResetExchangeRateData()
		{
			var isImport = InvoiceLine?.IsImport ?? ZBool.False;
			if (!(isImport && J7_ChargeType.SupportsIATA() && IsJ7_ExchangeRateIATA))
			{
				base.ResetExchangeRateData();
			}
		}

		public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		public new InvoiceLineApportionChargeLookups Lookups => (InvoiceLineApportionChargeLookups)base.Lookups;

		protected override JobComInvHeaderChargeLookups GetNewLookups() => new InvoiceLineApportionChargeLookups(this);

		public new InvoiceLineApportionChargeValidation Validation => (InvoiceLineApportionChargeValidation)base.Validation;

		protected override JobComInvHeaderChargeValidation GetNewValidation() => new InvoiceLineApportionChargeValidation(this);
	}
}
