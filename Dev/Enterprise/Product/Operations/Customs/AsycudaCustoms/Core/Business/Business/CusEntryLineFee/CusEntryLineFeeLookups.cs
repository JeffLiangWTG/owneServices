using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static System.FormattableString;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusEntryLineFeeLookups : Customs.Business.CusEntryLineFeeLookups
	{
		public CusEntryLineFeeLookups(CusEntryLineFee parent)
			: base(parent)
		{
		}

		public CusEntryLineFee EntryLineFee
		{
			get { return Parent; }
		}

		protected new CusEntryLineFee Parent
		{
			get { return (CusEntryLineFee)base.Parent; }
		}

		public CodeDescriptionPairList ChargeTypeList
		{
			get
			{
				CodeDescriptionPairList result = null;

				if (Parent.EntryLine?.RandomLine is JobComInvoiceLine invoiceLine)
				{
					var effectiveDate = invoiceLine.EffectiveAssessmentDate;
					var dataSource = invoiceLine.ApplicationBusinessProvider?.UniversalRefDataSource ?? ZString.Empty;
					var defaultDataGrouping = Parent.EntryLine.Declaration?.GetDefaultDataGroupingCode() ?? null;
					var type = invoiceLine.Lookups.TaxOrFeeType;

					if (!defaultDataGrouping.IsEmpty && !dataSource.IsEmpty)
					{
						result = Factory.GetCachedValue(Invariant($"AsycudaCustoms.ChargeTypeList_{defaultDataGrouping}_{effectiveDate.ToShortDateString()}_{type}"), () => // Key used in factory cache
						{
							var chargeTypeList = new CodeDescriptionPairList();
							chargeTypeList.AddPairIfNotExist(type, ZArchitecture.Environment.Country.GetConsumptionTaxDescription(invoiceLine.Declaration?.CountryCode ?? GlbCompany.CurrentCompany.Country.Code));
							chargeTypeList.AddRange(CusRefRateCodeView.Loader.LoadByDataSet(Factory, defaultDataGrouping, dataSource, false));

							return chargeTypeList;
						});
					}
				}

				return result ?? new CodeDescriptionPairList();
			}
		}
	}
}
