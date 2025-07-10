using System;
using CargoWise.Application;
using CargoWise.Billing.Collectors;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Business;
using Enterprise.Customs.Universal;
using Enterprise.Integration.Billing;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Billing.StlCollector.Retriever
{
	public class TransactionFactoryProvider
	{
		readonly UsageTransactionFactory usageTransactionFactory = new ();
		readonly BillingTransactionFactory billingTransactionFactory = new ();
		readonly Lazy<ZBool> EnableSendingNonBilledItemsAsUsage = new (() => ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalSystem() || (bool)new RefSysConfig.Loader(new BusinessObjectFactory()).GetBoolValue(RefStlScriptKeys.CollectUsageTransactionKey));

		public IStlTransactionFactory GetTransactionFactory(IRefStlScript refStlScript)
		{
			if (ObjectFactory.Get<IProductRegistration>().Key.DatabaseType != DatabaseTypes.Codes.Production && !ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalSystem())
			{
				return usageTransactionFactory;
			}
			if (string.Equals(refStlScript.DataGranularity, RefStlItemGrain.Snapshot, StringComparison.InvariantCultureIgnoreCase) ||
				(!refStlScript.UsedInBilling && EnableSendingNonBilledItemsAsUsage.Value))
			{
				return usageTransactionFactory;
			}

			return billingTransactionFactory;
		}
	}
}
