using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class CusContainer : BaseCusContainer
	{
		public CusContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		public new CusContainer Clone() => (CusContainer)base.Clone();

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public new CusContainerLookups Lookups => base.Lookups;

		public new CusContainerValidation Validation => (CusContainerValidation)base.Validation;

		protected override System.Type GetJobDeclarationType() => typeof(JobDeclaration);

		protected override Customs.Business.CusContainerValidation GetNewValidation()
		{
			return new CusContainerValidation(this);
		}
	}
}
