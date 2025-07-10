using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.MasterFiles
{
	public class CusClassPartPivotLookups : Customs.Business.CusClassPartPivotLookups
	{
		public CusClassPartPivotLookups(CusClassPartPivot parent)
			: base(parent)
		{
		}

		public new CusClassPartPivot Parent
		{
			get { return (CusClassPartPivot)base.Parent; }
		}

		public override CodeDescriptionPairList ClassificationTypes => Factory.GetCachedValue<ClassificationTypeList>();

		public virtual RefCusProcedureCollection RefCusProcedureCollection => new RefCusProcedureCollection(Factory, Parent.CI_RN_NKCountry, ZDateTime.Today, "", Parent.CI_ChildType);

		public CodeDescriptionPairList AdditionalCPCs
		{
			get
			{
				var procedureCodes = Parent.CI_CPC.Left(4);
				if (procedureCodes.Length == 4)
				{
					return Factory.GetCachedValue(
						"Enterprise.Customs.EU.AdditionalCPCs_" + Parent.CI_ChildType + "_" + procedureCodes + "_" + Parent.DataGroupingCodeForAdditionalProcedures,
						() =>
						{
							var result = new CodeDescriptionPairList();
							result.AddRange(new RefCusProcedure.Loader(Factory).LoadForProcedureCodesAndGrouping(Parent.CI_ChildType, procedureCodes.Left(2), procedureCodes.Right(2), Parent.DataGroupingCodeForAdditionalProcedures));
							result.Sort();
							return result;
						});
				}
				else
				{
					return new CodeDescriptionPairList();
				}
			}
		}

		public ICodeDescriptionPairList TaxTypeList => RefCusTaxOrFee.Loader.GetList(Factory, Env.CurrentCompany.Country.Code, ZDateTime.Today);

		public CodeDescriptionPairList GoodsCategoryList => GoodsCategoryListCore();

		protected virtual CodeDescriptionPairList GoodsCategoryListCore() => new CodeDescriptionPairList();
	}
}
