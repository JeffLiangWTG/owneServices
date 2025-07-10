using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsDepartureCargoDescPhase4Lookups : NctsCommonCargoDescLookups, IDepartureCargoDescLookups
	{
		public NctsDepartureCargoDescPhase4Lookups(NctsDepartureCargoDesc parent)
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
				var moveHeader = parent.MoveHeader;
				if (moveHeader != null)
				{
					var date = ZDateTime.Today;
					result = Factory.GetCachedValue("NctsDepartureCargoDescPhase4Lookups.DeclarationTypeList." + parent.DataGroupingCode + date.ToShortDateString(), delegate
					{
						var declarationTypeList = new CodeDescriptionPairList();
						declarationTypeList.AddRange(moveHeader.Lookups.DeclarationTypeList);
						declarationTypeList.RemoveCode(NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase4);
						declarationTypeList.RemoveCode(NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration);
						declarationTypeList.RemoveCode(NctsConstants.NctsTypeOfDeclaration.Codes.T2PlusSanMarino);
						return declarationTypeList;
					});
				}
				return result;
			}
		}

		public CodeDescriptionPairList TaxOrFeeCodeList => RefCusTaxOrFee.Loader.GetList(Factory, Parent.DataGroupingCode, Parent.ValuationDate, Constants.Customs.CusEntryFeeTypes.VAT); //Should we use the vatApplicabilities used in EU.Business.Declaration.JobComInvoiceLineLookups?

		public ConsigneeCollection ConsigneeList => new ConsigneeCollection(Factory);

		protected new NctsDepartureCargoDesc Parent => (NctsDepartureCargoDesc)base.Parent;

		public CodeDescriptionPairList BondedWhsUnitQtyList => new CodeDescriptionPairList();
	}
}
