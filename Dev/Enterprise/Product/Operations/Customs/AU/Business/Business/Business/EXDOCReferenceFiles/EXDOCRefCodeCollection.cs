using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[ModuleID(ModuleId.ZZRefCusCodeList)]
	public abstract class EXDOCRefCodeCollection : BusinessObjectCollection<ZZRefCusCodeListCombined>
	{
		//Collections on the QuarantineExdocLine require the header for filtering, if the Header is null we need an empty collection
		//Hence the null check and NoResult in call to base() additional filter.
		protected EXDOCRefCodeCollection(IEXDOCRefCodeTypeProvider typeProvider, ZString baseCodeType)
			: base(typeProvider?.Factory ?? new BusinessObjectFactory(), typeProvider == null ? ZQuery.NoResultQuery : null)
		{
			this.typeProvider = typeProvider;
			this.baseCodeType = baseCodeType;

			InitialiseFilterDefaults();
		}

		protected EXDOCRefCodeCollection(IEXDOCRefCodeTypeProvider typeProvider, ZString baseCodeType, ZDateTime effectiveDate)
			: this(typeProvider, baseCodeType)
		{
			this.effectiveDate = effectiveDate;
		}

		readonly protected IEXDOCRefCodeTypeProvider typeProvider;
		readonly protected ZString baseCodeType;
		readonly protected ZDateTime effectiveDate;

		protected void InitialiseFilterDefaults()
		{
			FilterBusinessObjectDefaults.RemoveAll();
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.ListType, "Property", GetCodeType));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.Description, "Property", ZString.Empty));
		}

		IZType GetCodeType() => GetCodeTypeCore();

		protected virtual ZString GetCodeTypeCore() => baseCodeType;

		protected override ZQuery CreateRelationshipFilter()
		{
			var tmpCodeType = GetCodeTypeCore();
			if (relationshipFilter == null || tmpCodeType != codeType)
			{
				codeType = tmpCodeType;
				relationshipFilter = base.CreateRelationshipFilter();

				if (!codeType.IsEmpty)
				{
					relationshipFilter.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CountryOrGrouping, Core.Constants.CountryCodes.Australia);
					relationshipFilter.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CodeType, codeType);
					if (effectiveDate != ZDateTime.Empty)
					{
						relationshipFilter.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_StartDate, SQLComparisonOperator.LessThanOrEqualTo, effectiveDate);
						relationshipFilter.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, effectiveDate);
					}
					relationshipFilter.OrderBy = ZZRefCusCodeListCombinedSchema.Constants.ZZD_Description;
				}
				else
				{
					relationshipFilter.IsNoResultQuery = true;
				}
			}

			return relationshipFilter;
		}
		ZString codeType;
		ZQuery relationshipFilter;

		public ZZRefCusCodeListCombined FindByCode(ZString code)
		{
			if (code.IsEmpty)
			{
				return null;
			}
			Load();
			return this.Cast<ZZRefCusCodeListCombined>().FirstOrDefault(rc => rc.ZZD_Code == code);
		}
	}
}
