using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using OrgSupplierPartCollection = Enterprise.Customs.Business.OrgSupplierPartCollection;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsDepartureCargoDescPhase5Lookups : NctsCommonCargoDescLookups, IDepartureCargoDescLookups
	{
		public NctsDepartureCargoDescPhase5Lookups(NctsDepartureCargoDesc parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList CountryOfDispatchList => Factory.GetCachedCountryNC008List(Parent.DataGroupingCode);

		public CodeDescriptionPairList CountryOfDestinationList => Factory.GetCachedCountryNC008List(Parent.DataGroupingCode);

		public CodeDescriptionPairList DeclarationTypeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var parent = Parent;
				var moveHeader = parent.Header?.MovementHeader;
				if (moveHeader != null)
				{
					result = Factory.GetCachedValue("NctsDepartureCargoDescPhase5Lookups.DeclarationTypeList." + parent.DataGroupingCode, delegate
					{
						var declarationTypeList = new CodeDescriptionPairList();
						declarationTypeList.AddRange(moveHeader.Lookups.DeclarationTypeList);
						declarationTypeList.RemoveCode(NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5);
						declarationTypeList.RemoveCode(NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration);
						return declarationTypeList;
					});
				}
				return result;
			}
		}

		public CodeDescriptionPairList TaxOrFeeCodeList => RefCusTaxOrFee.Loader.GetList(Factory, Parent.DataGroupingCode, Parent.ValuationDate, Constants.Customs.CusEntryFeeTypes.VAT); //Should we use the vatApplicabilities used in EU.Business.Declaration.JobComInvoiceLineLookups?

		public ConsigneeCollection ConsigneeList => new ConsigneeCollection(Factory);

		protected new NctsDepartureCargoDesc Parent => (NctsDepartureCargoDesc)base.Parent;

		public override OrgSupplierPartCollection Parts
		{
			get
			{
				if (Parent.Bill is NctsBill bill)
				{
					var consignor = bill.Consignor?.Organisation ?? bill.Header?.Consignor?.Organisation;
					return new OrgSupplierPartCollection(Factory, consignor, consignor, true);
				}

				return base.Parts;
			}
		}

		public virtual CodeDescriptionPairList BondedWhsUnitQtyList => RefCusCodeListTypes.GetCachedList(Factory, Parent.DataGroupingCode, Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, ZDateTime.Today);

		public override RefCurrencyCollection LinePriceCurrencies
		{
			get
			{
				var filter = new ZQuery(RefCurrencySchema.RX_IsActive, true);
				filter.AddToFilter(new ZQuery(RefCurrencySchema.RX_Desc, SQLComparisonOperator.NotEqual, ZString.Empty));
				return new(Factory, filter) { AdditionalFilter = { OrderBy = RefCurrencySchema.RX_Desc.Name } };
			}
		}
	}
}
