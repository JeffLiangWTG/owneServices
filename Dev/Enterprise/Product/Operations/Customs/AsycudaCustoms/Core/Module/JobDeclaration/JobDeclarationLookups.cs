using System;
using CargoWise.Types;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AsycudaCustoms.Module
{
	public class JobDeclarationFilterLookups : Customs.Module.JobDeclarationFilterLookups
	{
		public JobDeclarationFilterLookups(JobDeclarationFilterBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}
		protected new JobDeclarationFilterBusinessObject FilterBizObj => (JobDeclarationFilterBusinessObject)base.FilterBizObj;

		public CodeDescriptionPairList GuaranteeStatusList => new GuaranteeStatusList();

		public CodeDescriptionPairList GuaranteeActivityList => new GuaranteeActivityCodeList();

		public CusGuaranteeHeaderCollection GuaranteeHeaders => new CusGuaranteeHeaderCollection(Factory, new ZString[] { GlbCompany.CurrentCompany.GC_RN_NKCountryCode }, Array.Empty<ZString>());
	}
}
