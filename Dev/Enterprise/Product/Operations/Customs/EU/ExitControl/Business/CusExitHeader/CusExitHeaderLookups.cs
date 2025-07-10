using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitHeaderLookups : ExitControlBase.Business.CusExitHeaderLookups
	{
		public CusExitHeaderLookups(AutoCusExitHeader parent)
			: base(parent)
		{
		}

		public new CusExitHeader Parent => (CusExitHeader)base.Parent;

		public OrganisationsFindBoxCollection OrganizationsFindBoxList => new OrganisationsFindBoxCollection(Factory);
	}
}
