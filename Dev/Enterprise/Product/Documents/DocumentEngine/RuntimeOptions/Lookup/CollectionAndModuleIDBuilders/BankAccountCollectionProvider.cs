using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class BankAccountCollectionProvider : CollectionProvider
	{
		public BankAccountCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new AccBankAccountCollection(BusinessObjectFactory, GlbCompany.CurrentCompany, Filter);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.AccBankAccount;
	}
}
