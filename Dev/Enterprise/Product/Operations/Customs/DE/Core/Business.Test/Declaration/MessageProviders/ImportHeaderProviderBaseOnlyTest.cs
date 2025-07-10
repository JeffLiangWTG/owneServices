using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.DE.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(ImportHeaderProvider))]
	sealed class ImportHeaderProviderBaseOnlyTest : ImportHeaderProviderAbstractTest<ImportHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ImportHeaderProviderForTest(null));
		}

		public void TestLocalReferenceNumber()
		{
			declaration.JE_OwnerRef = "LocalReferenceNumber";
			entryHeader.LocalReferenceNumber = "YYDS";
			AssertEquals("Has LRNCusEntryNumber", "YYDS", Provider.LocalReferenceNumber);
		}

		protected override ImportHeaderProvider GetProvider() => new ImportHeaderProviderForTest(entryHeader);

		new IImportHeader Provider => base.Provider;
	}

	sealed class ImportHeaderProviderForTest : ImportHeaderProvider
	{
		public ImportHeaderProviderForTest(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}
	}
}
