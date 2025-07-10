using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using ClassificationTypeList = Enterprise.Customs.CA.Business.ClassificationTypeList;

namespace Enterprise.Customs.CA.Module
{
	public class OrgSupplierPartFilterStripBusinessObject : Customs.Module.OrgSupplierPartFilterStripBusinessObject
	{
		public override IBaseClassificationCollection<BaseCusClassification> Classifications => new BaseClassificationCollection<CusClassification>(Factory, Core.Constants.CountryCodes.Canada);

		protected override ZArchitecture.Core.CodeDescriptionPairList GetClassificationTypeList() => Factory.GetCachedValue<ClassificationTypeList>();
	}
}
