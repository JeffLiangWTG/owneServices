using System.Collections;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public class CIQProductQualificationLookups : CusSupportingInfoLookups
	{
		public CIQProductQualificationLookups(AutoCusSupportingInfo parent) : base(parent)
		{
		}

		public new CIQProductQualification Parent => (CIQProductQualification)base.Parent;

		JobDeclaration Declaration => Parent?.Parent?.Declaration;

		public override ICollection CodeList =>
			Declaration == null
				? new CodeDescriptionPairList()
				: ProductQualificationCodeList.GetCachedProductQualificationCodeList(Factory, Declaration.IsImport);

		public CodeDescriptionPairList UnitOfMeasurementList => Factory.GetCachedValue<RefCusPackListProvider>().GetCIPCustomsPackList(Factory, Core.Constants.CountryCodes.China);
	}
}
