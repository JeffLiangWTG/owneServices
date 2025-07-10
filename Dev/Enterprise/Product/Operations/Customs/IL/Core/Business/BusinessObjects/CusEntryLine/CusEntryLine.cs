using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Business
{
	public partial class CusEntryLine : Customs.Business.CusEntryLine
	{
		public CusEntryLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : Customs.Business.CusEntryLine.Schema
		{
			public const int CL_InvoiceAmount_DecimalPlaces = 2;
		}

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public EntryLineVatCalculator GetEntryLineVatCalculator() => new EntryLineVatCalculator(this);

		public ZDecimal DutyDetailsForVAT => Factory.GetValue(ref dutyDetailsForVATCached, () => GetDutyTotalAmount(Fees.AllLineFees.Where(f => f.IncludeForVatCalculation).ToList().AsReadOnly()));
		CachedProperty<ZDecimal> dutyDetailsForVATCached;

		public CusEntryLineConfirmedFeeWrapperCollection ConfirmedFeesReadOnly => Factory.GetValue(ref confirmedFeesReadOnly, () => GetConfirmedFeesReadOnlyCore());
		CachedProperty<CusEntryLineConfirmedFeeWrapperCollection> confirmedFeesReadOnly;

		[DecimalPlaces(Schema.CL_InvoiceAmount_DecimalPlaces)]
		public override ZDecimal CL_InvoiceAmount { get => base.CL_InvoiceAmount; set => base.CL_InvoiceAmount = value; }

		protected override ZDecimal GetDutyAmountCore() => GetDutyTotalAmount(Fees.AllLineFees.ToList().AsReadOnly());

		protected ZDecimal GetDutyTotalAmount(IReadOnlyCollection<CusEntryLineFee> effectiveFees)
		{
			var result = ZDecimal.Zero;
			if (ShouldCalculateDutyAmount)
			{
				var dataGrouping = Declaration?.GetDefaultDataGroupingCode(DefaultDataGroupingType.DutyRateCodes);

				if (dataGrouping.HasValue)
				{
					var dutyRateCodes = GetAllDutyTypeRateCodes(dataGrouping.Value);

					foreach (var rateCode in dutyRateCodes)
					{
						result += effectiveFees.CalculateTotalFeeAmount(rateCode, includeLandedCostOnly: false);
					}
				}
			}

			return result;
		}

		protected bool ShouldCalculateDutyAmount => RandomLine.CusProcedure?.ZZ6_CalculateDuty ?? true;

		protected override ZDecimal GetGSTVATAmountCore()
		{
			var result = ZDecimal.Zero;
			foreach (var vatFee in Fees.OfType<CusEntryLineFee>().Where(f => f.CF_ChargeType == Constants.EntryLineFee.VATFeeTypeCode && !f.CF_IsLandedCostOnly))
			{
				result += vatFee.CF_ChargeAmount;
			}
			return result;
		}

		protected CusEntryLineConfirmedFeeWrapperCollection GetConfirmedFeesReadOnlyCore() => new CusEntryLineConfirmedFeeWrapperCollection(this);

		IEnumerable<string> GetAllDutyTypeRateCodes(ZString dataGrouping)
		{
			var refCusRates = Factory.GetCachedRatesByType(dataGrouping);

			var rateCodes = new HashSet<string>();

			foreach (var refCusRate in refCusRates)
			{
				rateCodes.Add(refCusRate.ZY1_RateCode);
			}

			return rateCodes;
		}

		protected override ZString GetInvoicedDocumentaryAmountCurrencyCore() => RandomLine.InvoiceHeader.JZ_RX_NKInvoice_Currency;

		protected override ZDecimal GetInvoicedDocumentaryAmountCore() => InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.JI_LinePrice);
	}
}
