using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public partial class JobDeclarationCollection : EU.Business.Declaration.JobDeclarationCollection
	{
		public JobDeclarationCollection(BusinessObjectFactory factory, ZGuid companyPkToFilterOn)
			: base(factory, companyPkToFilterOn)
		{
		}

		protected override bool AllowNewCore => false;
	}
}
