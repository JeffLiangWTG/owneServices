using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DK.Business.Declaration.Testing
{
	[TestedType(typeof(ExportJobComInvoiceHeaderLookups))]
	class ExportJobComInvoiceHeaderLookupsTest : JobComInvoiceHeaderLookupsAbstractTest<ExportJobComInvoiceHeaderLookups>
	{
		protected override string MessageType => MessageTypeList.Codes.Export;

		protected override ExportJobComInvoiceHeaderLookups GetLookups() => new ExportJobComInvoiceHeaderLookups(invoice);
	}
}
