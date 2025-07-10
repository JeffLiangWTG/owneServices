using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class AddInfoJobComInvoiceLineLookups(AddInfoJobComInvoiceLine parent) : EUAddInfoLookups(parent)
	{
		public new AddInfoJobComInvoiceLine Parent => (AddInfoJobComInvoiceLine)base.Parent;

		JobComInvoiceLine InvoiceLine => Parent.Parent;

		public virtual CodeDescriptionPairList MethodOfPaymentList => new CodeDescriptionPairList();

		public CodeDescriptionPairList SecondQuotaList => UniversalReferenceDataHelper.GetDynamicOrderNumberList(InvoiceLine.UniversalTariff, InvoiceLine.AllApplicableRatesSelectionCriteria);

		public ICollection CountriesOfDestination => GetCountriesOfDestinationCore();

		protected virtual ICollection GetCountriesOfDestinationCore()
		{
			if (InvoiceLine.IsImport)
			{
				return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, ZDateTime.Today);
			}
			else
			{
				return new RefCountryCollection(Factory);
			}
		}

		public ICollection CountriesOfDispatch => GetCountriesOfDispatchCore();

		protected virtual ICollection GetCountriesOfDispatchCore() => new RefCountryCollection(Factory);

		public ValuationAdjustmentCodeList ValuationAdjustmentCodeList => Factory.GetCachedValue<ValuationAdjustmentCodeList>();

		public ZZRefCusCodeListCombinedCollection CusNumberList
		{
			get
			{
				var invoiceLine = Parent.Parent;
				if (invoiceLine.JI_Tariff.IsEmpty)
				{
					return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, invoiceLine.GetDefaultDataGroupingCode(), UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, ZDateTime.Today);
				}
				else
				{
					return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, invoiceLine.GetDefaultDataGroupingCode(), new ZString[] { UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS }, ZDateTime.Today, new List<RefCusCodeListAttributeFilter>
					{
						new (Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CombinedNomenclatureCode, SQLComparisonOperator.StartsWith, invoiceLine.JI_Tariff.SubstringSafe(0, 6))
					});
				}
			}
		}

		public ICollection CountryOfSupplyList => CountryOfSupplyListCore();

		protected virtual ICollection CountryOfSupplyListCore() => new RefCountryCollection(Factory);

		public ICodeDescriptionPairList TransactionNatureList => Factory.GetTranNatureList(InvoiceLine.GetDefaultDataGroupingCode());

		public ICollection RegionOfDestinationList => RegionOfDestinationListCore;

		protected virtual ICollection RegionOfDestinationListCore => new CodeDescriptionPairList();

		public CodeDescriptionPairList RelatedIndicatorList => Factory.GetCachedValue<ValuationIndicatorCodeList>();

		public CodeDescriptionPairList GoodsCategoryList => GoodsCategoryListCore();

		protected virtual CodeDescriptionPairList GoodsCategoryListCore() => new ();
	}
}
