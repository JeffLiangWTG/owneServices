namespace Enterprise.Accounting.Business.EInvoicing
{
	public interface IElectronicInvoicingAccountingObjectFactory
	{
		IElectronicInvoicingUpdateActionPermissions GetElectronicInvoicingUpdateActionPermissions();

		IEInvoicingConfigurationChecks GetEInvoicingConfigurationChecks();

		IElectronicInvoicingRequeueStrategy GetElectronicInvoicingRequeueStrategy();
	}

	class ElectronicInvoicingObjectFactory : IElectronicInvoicingAccountingObjectFactory
	{
		IElectronicInvoicingUpdateActionPermissions IElectronicInvoicingAccountingObjectFactory.GetElectronicInvoicingUpdateActionPermissions() => new ElectronicInvoicingUpdateActionPermissions();

		IEInvoicingConfigurationChecks IElectronicInvoicingAccountingObjectFactory.GetEInvoicingConfigurationChecks() => new EInvoicingConfigurationChecks();

		IElectronicInvoicingRequeueStrategy IElectronicInvoicingAccountingObjectFactory.GetElectronicInvoicingRequeueStrategy() => new ElectronicInvoicingRequeueStrategy();
	}
}
