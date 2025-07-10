using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class TaiwanAccountingCountryFactory : IAccountingCountryFactory, IInstanceProvider<IComplianceDocumentVoidingProvider>, IInstanceProvider<IComplianceDocumentNumberProvider>
	{
		IComplianceDocumentVoidingProvider IInstanceProvider<IComplianceDocumentVoidingProvider>.Get() => new TaiwanComplianceDocumentVoidingProvider();

		IComplianceDocumentNumberProvider IInstanceProvider<IComplianceDocumentNumberProvider>.Get() => new TaiwanComplianceDocumentNumberProvider();
	}
}
