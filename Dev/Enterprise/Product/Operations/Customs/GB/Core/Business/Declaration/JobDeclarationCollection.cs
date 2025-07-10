using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class JobDeclarationCollection : EU.Business.Declaration.JobDeclarationCollection
	{
		public JobDeclarationCollection(BusinessObjectFactory factory, ZGuid companyPkToFilterOn)
			: base(factory, companyPkToFilterOn)
		{
		}

		public new JobDeclaration this[int index] => (JobDeclaration)Elements[index];

		public new JobDeclaration AddNew() => (JobDeclaration)base.AddNew();
	}
}
