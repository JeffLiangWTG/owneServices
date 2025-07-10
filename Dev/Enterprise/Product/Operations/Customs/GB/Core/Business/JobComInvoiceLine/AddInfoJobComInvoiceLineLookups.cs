using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class AddInfoJobComInvoiceLineLookups : EU.Business.Declaration.AddInfoJobComInvoiceLineLookups
	{
		public AddInfoJobComInvoiceLineLookups(AddInfoJobComInvoiceLine parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList MethodOfPaymentList
		{
			get
			{
				var invLine = Parent.Parent;
				var declaration = invLine.Declaration;
				var countryCode = declaration?.GetDefaultDataGroupingCode() ?? Core.Constants.CountryCodes.UnitedKingdom;
				var dateOfValuation = declaration?.DateOfValuation ?? ZDateTime.Today;

				return TaxLookupsCommon.GetMOPList(Factory, countryCode, invLine.Declaration, dateOfValuation);
			}
		}

		protected override ICollection GetCountriesOfDestinationCore()
		{
			var invLine = Parent?.Parent;
			var declaration = invLine?.Declaration;
			var dataGroupingCode = declaration?.GetDefaultDataGroupingCode() ?? Core.Constants.CountryCodes.UnitedKingdom;
			var dateOfValuation = declaration?.DateOfValuation ?? ZDateTime.Today;
			return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, dataGroupingCode, UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, dateOfValuation);
		}

		protected override CodeDescriptionPairList GoodsCategoryListCore() => new GoodsCategoryList();
	}
}
