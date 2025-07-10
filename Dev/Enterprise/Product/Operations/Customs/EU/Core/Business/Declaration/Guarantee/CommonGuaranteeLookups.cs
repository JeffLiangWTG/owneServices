using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CommonGuaranteeLookups : CusBondDetailLookups
	{
		public CommonGuaranteeLookups(CommonGuarantee bondData)
			: base(bondData)
		{
		}

		protected new CommonGuarantee Parent => (CommonGuarantee)base.Parent;

		public CodeDescriptionPairList BondTypeList => BondTypeListCore;

		protected virtual CodeDescriptionPairList BondTypeListCore => Factory.GetCachedValue<CodeDescriptionPairLists.EUNctsGuaranteeTypeList>();

		public CustomsOfficeCodeCollection OfficeCodeList => OfficeCodeListCore;

		protected virtual CustomsOfficeCodeCollection OfficeCodeListCore => EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRoles(Factory, "CGU", "GUA");

		public CodeDescriptionPairList List71NonEcContractingCountriesList
		{
			get
			{
				return Factory.GetCachedValue("List71NonEcContractingCountriesList", delegate
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(Core.Constants.CountryCodes.Andorra, Res.GetString("22e05f2b-3fb9-4aae-9059-296b351ad882", "Andorra"));
					list.AddPair(Core.Constants.CountryCodes.Switzerland, Res.GetString("767bca19-7755-418c-84b1-991fd2359061", "Switzerland"));
					list.AddPair(Core.Constants.CountryCodes.Iceland, Res.GetString("31f735fb-cac1-4e45-88a1-9c9e5a1587f5", "Iceland"));
					list.AddPair(Core.Constants.CountryCodes.Macedonia, Res.GetString("1a5f6367-97bc-4d0a-8f8b-b3cf6de3c5b9", "Macedonia"));
					list.AddPair(Core.Constants.CountryCodes.Norway, Res.GetString("0f5b39f1-71d2-4306-9a0e-323068b095f7", "Norway"));
					list.AddPair(Core.Constants.CountryCodes.SanMarino, Res.GetString("0c3afa08-36ba-49fd-803a-a6ddfd46eb0f", "San Marino"));
					list.AddPair(Core.Constants.CountryCodes.Serbia, Res.GetString("55f6167a-6c5a-4962-906b-f928c28a3083", "Serbia"));
					list.AddPair(Core.Constants.CountryCodes.Turkey, Res.GetString("9cd5cc65-931c-41ea-8dc0-bf8e49956618", "Turkey"));
					return list;
				});
			}
		}

		public virtual CusGuaranteeHeaderCollection ReferenceNumbers
		{
			get
			{
				var guarantees = GetGuaranteeHeaderCollection();
				guarantees.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusGuaranteeHeaderCollectionFiltered.FilterConstants.GuaranteeHolders, "Property1", PrimaryGuaranteeHolderAddress));
				guarantees.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusGuaranteeHeaderCollectionFiltered.FilterConstants.GuaranteeHolders, "Property2", SecondaryGuaranteeHolderAddress));
				guarantees.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusGuaranteeHeaderCollectionFiltered.FilterConstants.GuaranteeNumber, "Property", Parent.PW_BondNumber));
				guarantees.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusGuaranteeHeaderCollectionFiltered.FilterConstants.GuaranteeType, "Property", Parent.PW_BondType));
				guarantees.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusGuaranteeHeaderCollectionFiltered.FilterConstants.EndDate, "PropertySearch", new ZString(ResString.GetMultilingualString("Filter|DateRangeSearchList|InTheFuture", "In the Future"))));
				return guarantees;
			}
		}

		public CodeDescriptionPairList AccessCodeList => AccessCodeListCore;

		protected virtual CodeDescriptionPairList AccessCodeListCore => Factory.GetCachedValue<CodeDescriptionPairList>();

		protected virtual CusGuaranteeHeaderCollection GetGuaranteeHeaderCollection()
		{
			CusGuaranteeHeaderCollection result = null;
			if (GuaranteeReferencesTypeFilter.Count > 0)
			{
				result = new CusGuaranteeHeaderCollectionFiltered(Factory, System.Array.Empty<ZString>(), GuaranteeTypeFilter, GuaranteeReferencesTypeFilter);
			}
			else
			{
				result = new CusGuaranteeHeaderCollectionFiltered(Factory, System.Array.Empty<ZString>(), GuaranteeTypeFilter);
			}
			return result;
		}

		protected virtual IReadOnlyList<ZString> GuaranteeReferencesTypeFilter => System.Array.Empty<ZString>();

		protected virtual IReadOnlyList<ZString> GuaranteeTypeFilter => System.Array.Empty<ZString>();

		protected virtual ZGuid? PrimaryGuaranteeHolderAddress => null;

		protected virtual ZGuid? SecondaryGuaranteeHolderAddress => null;

		public virtual CodeDescriptionPairList HolderIdentificationList => new CodeDescriptionPairList();
	}
}
