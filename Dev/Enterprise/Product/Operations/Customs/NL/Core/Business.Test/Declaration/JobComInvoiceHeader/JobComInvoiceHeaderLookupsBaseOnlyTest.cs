using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(JobComInvoiceHeaderLookups))]
class JobComInvoiceHeaderLookupsBaseOnlyTest : JobComInvoiceHeaderLookupsAbstractTest<JobComInvoiceHeaderLookups>
{
	protected override string MessageType => MessageTypeList.Codes.MiscellaneousCustoms;

	protected override JobComInvoiceHeaderLookups GetLookups() => new JobComInvoiceHeaderLookups(invoice);
}
