using System;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(LineProvider))]
	sealed class LineProviderBaseOnlyTest : LineProviderProviderAbstractTest<LineProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new LineProvider(null));
		}

		public void TestLineNumber()
		{
			var invoice = emcsInvoiceLine.Declaration.InvoiceHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Pre-condition Line 1", emcsInvoiceLine.JI_LineNo, new ZShort(1));
				AssertEquals("Line 1", 1, LineProvider.LineNumber);

				var invLine2 = invoice.InvoiceLines.AddNew();
				var lineProvider2 = new LineProvider(invLine2) as IEMCSLine;
				AssertEquals("Pre-condition Line 2", invLine2.JI_LineNo, new ZShort(2));
				AssertEquals("Line 2", 2, lineProvider2.LineNumber);
			});
		}

		public void TestExciseProductCode()
		{
			emcsInvoiceLine.ZG_ExciseProductCode = "E300";
			AssertEquals("E300", LineProvider.ExciseProductCode);
		}

		protected override LineProvider GetLineProvider() => new LineProvider(emcsInvoiceLine);
	}
}
