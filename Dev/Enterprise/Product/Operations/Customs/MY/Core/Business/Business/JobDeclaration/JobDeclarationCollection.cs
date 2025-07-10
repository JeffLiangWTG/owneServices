using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.MY.Business
{
	public class JobDeclarationCollection : TypeSafeJobDeclarationCollection
	{
		public JobDeclarationCollection(BusinessObjectFactory factory, ZGuid companyPkToFilterOn)
			: base(factory, companyPkToFilterOn)
		{
		}

		protected override bool AllowNewCore => false;
	}
}
