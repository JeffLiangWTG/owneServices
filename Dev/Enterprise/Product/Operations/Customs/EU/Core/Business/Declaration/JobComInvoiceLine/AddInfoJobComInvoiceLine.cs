using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class AddInfoJobComInvoiceLine : AddInfo, ISupportMultipleResourceStringData
	{
		public AddInfoJobComInvoiceLine(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.MethodOfPaymentList))]
		public override ZString ZG_MethodOfPayment
		{
			get => base.ZG_MethodOfPayment;
			set => base.ZG_MethodOfPayment = value;
		}

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.SecondQuotaList))]
		public override ZString ZG_SecondQuota
		{
			get => base.ZG_SecondQuota;
			set => base.ZG_SecondQuota = value;
		}

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.PrincipalsRepresentativeCities))]
		public override ZString ZG_RL_NKPrincipalsRepresentativeCity
		{
			get => base.ZG_RL_NKPrincipalsRepresentativeCity;
			set => base.ZG_RL_NKPrincipalsRepresentativeCity = value;
		}

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.CountriesOfDestination))]
		public override ZString ZG_CountryOfDestination
		{
			get => base.ZG_CountryOfDestination;
			set => base.ZG_CountryOfDestination = value;
		}

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.CountriesOfDispatch))]
		public override ZString ZG_CountryOfDispatch
		{
			get => base.ZG_CountryOfDispatch;
			set => base.ZG_CountryOfDispatch = value;
		}

		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.ValuationAdjustmentCodeList))]
		public override ZString ZG_ValueAdjustmentCode
		{
			get => base.ZG_ValueAdjustmentCode;
			set => base.ZG_ValueAdjustmentCode = value;
		}

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.CusNumberList))]
		public override ZString ZG_CusNumber
		{
			get { return base.ZG_CusNumber; }
			set { base.ZG_CusNumber = value; }
		}

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.CountryOfSupplyList))]
		public override ZString ZG_CountryOfSupply
		{
			get => base.ZG_CountryOfSupply;
			set => base.ZG_CountryOfSupply = value;
		}

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.TransactionNatureList))]
		public override ZString ZG_TransNature
		{
			get => base.ZG_TransNature;
			set
			{
				var oldValue = ZG_TransNature;
				base.ZG_TransNature = value;
				if (!IsCopying && oldValue != ZG_TransNature)
				{
					Parent.InvoiceHeader?.MarkAsNeedingValidation();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.RegionOfDestinationList))]
		public override ZString ZG_RegionOfDestination
		{
			get => base.ZG_RegionOfDestination;
			set => base.ZG_RegionOfDestination = value;
		}

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.RelatedIndicatorList))]
		public override ZString ZG_RelatedIndicator2
		{
			get => base.ZG_RelatedIndicator2;
			set => base.ZG_RelatedIndicator2 = value;
		}

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.RelatedIndicatorList))]
		public override ZString ZG_RelatedIndicator3
		{
			get => base.ZG_RelatedIndicator3;
			set => base.ZG_RelatedIndicator3 = value;
		}

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.RelatedIndicatorList))]
		public override ZString ZG_RelatedIndicator4
		{
			get => base.ZG_RelatedIndicator4;
			set => base.ZG_RelatedIndicator4 = value;
		}

		[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.GoodsCategoryList))]
		public override ZString ZG_GoodsCategory
		{
			get => base.ZG_GoodsCategory;
			set => base.ZG_GoodsCategory = value;
		}

		public new AddInfoJobComInvoiceLineLookups Lookups => (AddInfoJobComInvoiceLineLookups)base.Lookups;

		public new AddInfoJobComInvoiceLineValidation Validation => (AddInfoJobComInvoiceLineValidation)base.Validation;

		public IReadOnlyList<string> MultipleKeysToUse => Parent.MultipleKeysToUse;

		protected override EUAddInfoLookups GetNewLookups() => Parent.GetAddInfoJobComInvoiceLineLookups(this);

		protected override EUAddInfoValidation GetNewValidation()
		{
			var declaration = Parent.Declaration;
			if (declaration != null)
			{
				return declaration.GetAddInfoJobComInvoiceLineValidation(this);
			}
			else
			{
				return new AddInfoJobComInvoiceLineValidation(this);
			}
		}

		protected override IValueSetStrategy GetValueSetStrategy()
		{
			if (iValueSetStrategy == null)
			{
				var declaration = Parent.Declaration;
				if (declaration != null)
				{
					iValueSetStrategy = declaration.GetAddInfoJobComInvoiceLineValueSetStrategy(this);
				}
			}
			return iValueSetStrategy;
		}
		IValueSetStrategy iValueSetStrategy;
	}
}
