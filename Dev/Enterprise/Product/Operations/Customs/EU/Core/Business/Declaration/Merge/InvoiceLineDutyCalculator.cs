using System;
using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business
{
	public class InvoiceLineDutyCalculator : UniversalDutyCalculator<BaseJobComInvoiceLine, InvoiceLineDutyCalculator.EUUniversalRateCalcDataForInvoiceLine>
	{
		public InvoiceLineDutyCalculator(BaseJobComInvoiceLine entity, RateCalculationVisitorMode rateCalculationVisitorMode)
			: base(entity, rateCalculationVisitorMode, CreateEUUniversalRateCalcData)
		{
		}

		static IUniversalRateCalcData CreateEUUniversalRateCalcData(BaseJobComInvoiceLine invoiceLine, RateView rateView) =>
			new EUUniversalRateCalcDataForInvoiceLine(invoiceLine, rateView);

		#region EUUniversalRateCalcDataForInvoiceLine

		public class EUUniversalRateCalcDataForInvoiceLine : InvoiceLineUniversalRateCalcData
		{
			public EUUniversalRateCalcDataForInvoiceLine(BaseJobComInvoiceLine invoiceLine, RateView rateView) : base(invoiceLine, rateView)
			{
			}

			protected override IList<Tuple<string, string>> GetInitialAdditionalInformationList()
			{
				var result = new List<Tuple<string, string>>();

				foreach (var supportingDocument in InvoiceLine.SupportingDocuments.Where(x => x.CSI_Type == CusSupportingInfoTypeList.Codes.SupportingDocument))
				{
					result.Add(UniversalReferenceConstants.SupportingDocumentTypes.Certificate, supportingDocument.CSI_Code);
				}

				return result;
			}

			protected override IDictionary<string, string> GetMeursingExpressionList()
				=> new RateCalcMeursingExpressionReplacerForInvoiceLine(InvoiceLine).ReplaceApplicableMeursingExpressions(RateView);

			protected override IDictionary<string, decimal> GetCountrySpecificValueDictionary()
				=> new RateCalcSpecificValueAggregator().GetSpecificValueDictionary(InvoiceLine);

			new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;
		}
		#endregion

		#region RateCalcMeursingExpressionReplacerForInvoiceLine

		public sealed class RateCalcMeursingExpressionReplacerForInvoiceLine : RateCalcMeursingExpressionReplacer<BaseJobComInvoiceLine>
		{
			public RateCalcMeursingExpressionReplacerForInvoiceLine(BaseJobComInvoiceLine entity)
				: base(entity, i => i)
			{
			}

			protected override TariffView GetMeursingTariff()
				=> RateCalcMeursingExpressionReplacer.GetMeursingTariff(Declaration, (JobComInvoiceLine)Entity);
		}

		#endregion
	}
}
