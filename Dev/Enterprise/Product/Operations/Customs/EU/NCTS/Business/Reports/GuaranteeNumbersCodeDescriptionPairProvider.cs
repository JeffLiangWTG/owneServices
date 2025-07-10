using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class GuaranteeNumbersCodeDescriptionPairProvider : Integration.Customs.EU.NCTS.IGuaranteeNumbersCodeDescriptionPairProvider, DocumentEngine.RuntimeOptions.IDependenceCodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList() => GetGuaranteeNumbers(ZGuid.Empty);

		public ReadOnlyCodeDescriptionPairList GetDependenceCodeDescriptionPairList(string value)
		{
			ZGuid.TryParse(value, out var holderPk);
			return GetGuaranteeNumbers(holderPk);
		}

		#region Implementation

		ReadOnlyCodeDescriptionPairList GetGuaranteeNumbers(ZGuid holderPk)
		{
			var guaranteeCollection = LoadGuaranteeCollection(holderPk);
			return MapToCodeDescriptionPairList(guaranteeCollection);
		}

		CusGuaranteeHeaderCollection LoadGuaranteeCollection(ZGuid holderPk)
		{
			var factory = new BusinessObjectFactory();
			var guaranteeCollection = new CusGuaranteeHeaderCollection(factory, GetLoadGuaranteeCollectionQuery(factory, holderPk));
			guaranteeCollection.ApplySort(AutoCusPermitHeader.Schema.CPH_Number, System.ComponentModel.ListSortDirection.Ascending);
			return guaranteeCollection;
		}

		ZQuery GetLoadGuaranteeCollectionQuery(BusinessObjectFactory factory, ZGuid holderPk)
		{
			var nctsCountries = ZZRefCusCodeListCombined.GetUniqueCodes(factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009, ZDateTime.Today, includeParentDataGroupings: true);
			if (nctsCountries.Length == 0)
			{
				return ZQuery.NoResultQuery;
			}

			var query = new ZQuery(CusPermitHeaderSchema.CPH_Type, EUGuaranteeTypeList.Codes.TRA);
			query.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, nctsCountries);

			var frAdditionalFilter = new ZQuery(CusPermitHeaderSchema.CPH_Type, EUGuaranteeTypeList.Codes.COD);
			frAdditionalFilter.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, Core.Constants.CountryCodes.France);

			query.AddToFilter(frAdditionalFilter, JoinCondition.Or);

			if (holderPk.IsValid)
			{
				query.AddToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, holderPk);
			}
			return query;
		}

		ReadOnlyCodeDescriptionPairList MapToCodeDescriptionPairList(CusGuaranteeHeaderCollection guaranteeCollection)
		{
			var result = new CodeDescriptionPairList();
			foreach (var guarantee in guaranteeCollection)
			{
				result.AddPairIfNotExist(guarantee.CPH_Number, guarantee.PermitHolder.OH_Code);
			}
			return result;
		}

		#endregion
	}
}
