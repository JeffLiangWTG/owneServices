using System.Linq;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

class ConsignmentItemRefinementDataProviderTest : BasePassarDataProviderTest<ConsignmentItemRefinementDataProvider>
{
	public void TestConstructorNullArgument() => CombineAssertions(() =>
	{
		AssertNull(ConsignmentItemRefinementDataProvider.New(null));
		AssertNull(ConsignmentItemRefinementDataProvider.New(EntryLine));
	});

	public void TestProvider() => CombineAssertions(() =>
	{
		var invoiceLine = (JobComInvoiceLine)EntryLine.InvoiceLines.First();
		invoiceLine.InAndOutwardProcessingRepairReason = "Repair Reason";
		var notifyCustomsOffices1 = invoiceLine.NotifyCustomsOffices.AddNew();
		notifyCustomsOffices1.CY_Data = "CH001001";
		var notifyCustomsOffices2 = invoiceLine.NotifyCustomsOffices.AddNew();
		notifyCustomsOffices2.CY_Data = "CH001004";

		AssertEquals("Reason", "Repair Reason", DataProvider.Reason);
		AssertEquals("SupervisionOfficeReferenceNumber", notifyCustomsOffices1.CY_Data, DataProvider.SupervisionOfficeReferenceNumber);
	});

	public void TestAccountingType() => CombineAssertions(() =>
	{
		var invoiceLine = (JobComInvoiceLine)EntryLine.InvoiceLines.First();

		invoiceLine.InAndOutwardProcessingBillingType = InAndOutwardProcessingBillingTypesEdec.SuspensiveProcedure;
		AssertEquals("edec: 1 / Passar: non-collection", InAndOutwardRefinementAccountingTypesPassar.NonCollection, DataProvider.AccountingType);

		ResetDataProvider();

		invoiceLine.InAndOutwardProcessingBillingType = InAndOutwardProcessingBillingTypesEdec.RefundProcedure;
		AssertEquals("edec: 2 / Passar: refund", InAndOutwardRefinementAccountingTypesPassar.Refund, DataProvider.AccountingType);

		ResetDataProvider();

		invoiceLine.InAndOutwardProcessingBillingType = "3";
		AssertEquals("edec: 3 invalid / Passar: empty", string.Empty, DataProvider.AccountingType);
	});

	public void TestProcessType() => CombineAssertions(() =>
	{
		var invoiceLine = (JobComInvoiceLine)EntryLine.InvoiceLines.First();

		invoiceLine.InAndOutwardProcessingProcessType = InAndOutwardProcessingProcessTypesEdec.DueProcedure;
		AssertEquals("edec: 1 / Passar: ordinary", InAndOutwardRefinementProcessTypesPassar.Ordinary, DataProvider.ProcessType);

		ResetDataProvider();

		invoiceLine.InAndOutwardProcessingProcessType = InAndOutwardProcessingProcessTypesEdec.SimplifiedProcedure;
		AssertEquals("edec: 2 / Passar: simplified", InAndOutwardRefinementProcessTypesPassar.Simplified, DataProvider.ProcessType);

		ResetDataProvider();

		invoiceLine.InAndOutwardProcessingProcessType = InAndOutwardProcessingProcessTypesEdec.SpecialProcedure;
		AssertEquals("edec: 3 / Passar: special", InAndOutwardRefinementProcessTypesPassar.Special, DataProvider.ProcessType);

		ResetDataProvider();

		invoiceLine.InAndOutwardProcessingProcessType = "4";
		AssertEquals("edec: 4  invalid / Passar: empty", string.Empty, DataProvider.ProcessType);
	});

	public void TestRefinementType() => CombineAssertions(() =>
	{
		var invoiceLine = (JobComInvoiceLine)EntryLine.InvoiceLines.First();

		invoiceLine.InAndOutwardProcessingRefinementType = InAndOutwardProcessingRefinementTypesEdec.CommercialProcessing;
		AssertEquals("edec: 1 / Passar: own", InAndOutwardRefinementTypesPassar.Own, DataProvider.Type);

		ResetDataProvider();

		invoiceLine.InAndOutwardProcessingRefinementType = InAndOutwardProcessingRefinementTypesEdec.ContractProcessing;
		AssertEquals("edec: 2 / Passar: pay", InAndOutwardRefinementTypesPassar.Pay, DataProvider.Type);

		ResetDataProvider();

		invoiceLine.InAndOutwardProcessingRefinementType = "3";
		AssertEquals("edec: 3 invalid / Passar: empty", string.Empty, DataProvider.Type);
	});

	protected override ConsignmentItemRefinementDataProvider CreateDataProvider() => ConsignmentItemRefinementDataProvider.New(EntryLine);
}
