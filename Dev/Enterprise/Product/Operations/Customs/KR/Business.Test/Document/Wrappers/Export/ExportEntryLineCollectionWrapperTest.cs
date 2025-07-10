using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ExportEntryLineCollectionWrapper))]
	sealed class ExportEntryLineCollectionWrapperTest : NonPersistentBusinessObjectCollectionTestCase<ExportEntryLineCollectionWrapper>
	{
		protected override ExportEntryLineCollectionWrapper GetCollectionToTest() => new ExportEntryLineCollectionWrapper(Enumerable.Empty<IExportEntryLine>(), ZDecimal.Zero, Factory);
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var entryLine = new ExportEntryLine();
			var invoiceLines = new List<ExportInvoiceLine>();
			entryLine.InvoiceLines = invoiceLines.ToArray();
			return new ExportEntryLineWrapper(entryLine, ZDecimal.Zero, ZBool.False, Factory);
		}
	}
}
