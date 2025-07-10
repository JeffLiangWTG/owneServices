using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class CusEntryLine : EU.Business.Declaration.CusEntryLine
	{
		public CusEntryLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override System.Type GetCusEntryHeaderType() => typeof(CusEntryHeader);

		protected override void DoMergeInvoiceLine(BaseJobComInvoiceLine baseInvoiceLine)
		{
			base.DoMergeInvoiceLine(baseInvoiceLine);
			CL_StatisticalValue = ZArchitecture.Core.Utilities.Round(CL_StatisticalValue < 1 ? 1 : CL_StatisticalValue, 0);
		}

		protected override IEnumerable<ZString> GuaranteeDeferredMethodsOfPayment => Factory.GetCachedValue("DE.CusEntryLine.GuaranteeDeferredMethodsOfPayment", () => new ZString[] {
			UniversalReferenceConstants.MethodOfPaymentTypes.E, UniversalReferenceConstants.MethodOfPaymentTypes.F, UniversalReferenceConstants.MethodOfPaymentTypes.G });

		protected override IEnumerable<ZString> DeferredMethodsOfPayment => Factory.GetCachedValue("DE.CusEntryLine.DeferredMethodsOfPayment", () => MethodOfPaymentHelper.DeferredMethodsOfPayment);

		protected override MoPLevel MoPDetailsLevel => MoPLevel.Declaration;

		public new CusEntryHeader Header => (CusEntryHeader)base.Header;

		public new JobComInvoiceLine RandomLine => base.RandomLine as JobComInvoiceLine;

		protected override ICusEntryLineFeeCollection<Customs.Business.CusEntryLineFee, Customs.Business.CusEntryLine> GetCusEntryLineFeeCollection() => new EU.Business.Declaration.CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>(this, Factory);

		protected override bool EffectiveGrossWeightIsApplicableCore => true;

		public ZString ZG_CustomsStatus
		{
			get => AddInfo.ZG_CustomsStatus;
			set => AddInfo.ZG_CustomsStatus = value;
		}

		public ZString CustomsStatusDescription
		{
			get
			{
				var result = ZString.Empty;
				var declaration = Header.Declaration;
				if (declaration != null)
				{
					result = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, ZG_CustomsStatus, declaration.GetDefaultDataGroupingCode(),
							Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus,
							ZDateTime.Now)?.ZZD_Description ?? ZString.Empty;
				}
				return result;
			}
		}

		protected override EU.Business.EntryLineVatCalculator GetEntryLineVatCalculatorCore() => new EntryLineVatCalculator(this);

		public ZPropertyInfo ZG_CustomsStatusInfo => GetWrappedZPropertyInfo(nameof(ZG_CustomsStatus), x => AddInfo.ZG_CustomsStatusInfo);

		protected override EU.Business.Declaration.CusEntryLineConfirmedFeeWrapperCollection GetConfirmedFeesReadOnlyCore() => new CusEntryLineConfirmedFeeWrapperCollection(this);

		internal bool AtLeastOneInvoiceLineHasPackingDetails => Factory.GetValue(ref atLeastOneInvoiceLineHasPackingDetails, () => InvoiceLines.Cast<JobComInvoiceLine>()
			.Any(x => x.PackagesForInvoiceLinesForBindingOnly.Cast<BaseCusLinkPackage>().Any(y => y.IsLinked)));
		CachedProperty<bool> atLeastOneInvoiceLineHasPackingDetails;
	}
}
