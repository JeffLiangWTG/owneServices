using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Integration.Customs.EUExitControl;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class JobDeclarationCollection : TypeSafeJobDeclarationCollection
	{
		public JobDeclarationCollection(BusinessObjectFactory factory, ZGuid companyPkToFilterOn)
			: base(factory, companyPkToFilterOn)
		{
		}

		public ICusExitHeaderLoader ExitHeaderLoader => exitHeaderLoader ?? (exitHeaderLoader = ObjectFactory.Get<ICusExitHeaderLoader>("EUExitControl.ICusExitHeaderLoader", Factory));
		ICusExitHeaderLoader exitHeaderLoader;

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy() => new JobDeclarationCollectionFetchStrategy(this);
	}
}
