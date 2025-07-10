using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	public abstract class KoreaSouthElectronicMessagingProcessingServiceTask : GlobalElectronicMessagingProcessingServiceTask
	{
		public override string CountryCode => CountryCodes.KoreaSouth;

		protected override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeCreator(GlbCompany company)
		{
			if (AccountingConfigurationRegistry.Instance.EnableKoreaSouthEDIInterchangeCreator.Value)
			{
				return new KoreaSouthEDIInterchangeCreator(company, CountryFactory);
			}
			else
			{
				return null;
			}
		}
	}
}
