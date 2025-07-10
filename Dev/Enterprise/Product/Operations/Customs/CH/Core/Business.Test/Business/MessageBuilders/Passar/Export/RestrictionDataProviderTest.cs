using System.Linq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(RestrictionDataProvider))]
sealed class RestrictionDataProviderTest : BasePassarDataProviderTest<RestrictionDataProvider>
{
	public void TestNewCollection() => CombineAssertions(() =>
	{
		var invoiceLine = (JobComInvoiceLine)EntryLine.InvoiceLines.FirstOrDefault();

		AssertEquals("null argument", Enumerable.Empty<RestrictionDataProvider>(), RestrictionDataProvider.NewCollection(null));
		invoiceLine.Restrictions.AddNew();
		invoiceLine.Restrictions.AddNew();
		AssertContainsExactElementsInExactOrder("NewCollection", new[] { 1, 2 }, RestrictionDataProvider.NewCollection(invoiceLine.Restrictions).Select(x => x.SequenceNumber));
	});

	public void TestProperties() => CombineAssertions(() =>
	{
		const int code = 12;
		const string permitExceptionReason = "A Reason";
		const string permitNumber = "123";

		var invoiceLine = (JobComInvoiceLine)EntryLine.InvoiceLines.FirstOrDefault();
		var restriction = invoiceLine.Restrictions.AddNew();
		invoiceLine.Restrictions.AddNew();

		AssertNull("Initial PermitExceptionReason", DataProvider.PermitExceptionReason);
		AssertNull("Initial PermitNumber", DataProvider.PermitNumber);

		restriction.CSI_Description = permitExceptionReason;
		restriction.CSI_ReferenceNumber = permitNumber;
		restriction.CSI_Code = code.ToString();

		AssertEquals("SequenceNumber", 1, DataProvider.SequenceNumber);
		AssertEquals("PermitExceptionReason", permitExceptionReason, DataProvider.PermitExceptionReason);
		AssertEquals("PermitNumber", permitNumber, DataProvider.PermitNumber);
		AssertEquals("Code", code, DataProvider.Code);
	});

	public void TestPermitOwner()
	{
		var invoiceLine = (JobComInvoiceLine)EntryLine.InvoiceLines.FirstOrDefault();
		var restriction = invoiceLine.Restrictions.AddNew();
		AssertNull(DataProvider.PermitOwner);

		restriction.CSI_ReferenceNumber = "123";
		AssertType<PermitOwnerDataProvider>(DataProvider.PermitOwner);
		AssertSame("cached", DataProvider.PermitOwner, DataProvider.PermitOwner);
	}

	public void TestAdditionalInformation()
	{
		var invoiceLine = (JobComInvoiceLine)EntryLine.InvoiceLines.FirstOrDefault();
		invoiceLine.Restrictions.AddNew();

		AssertType<RestrictionAdditionalInformationDataProvider[]>(DataProvider.AdditionalInformations);
		AssertSame("cached", DataProvider.AdditionalInformations, DataProvider.AdditionalInformations);
	}

	protected override RestrictionDataProvider CreateDataProvider() => RestrictionDataProvider.NewCollection((EntryLine.InvoiceLines.FirstOrDefault() as JobComInvoiceLine)?.Restrictions).First();
}
