using CargoWise.Types;
using Enterprise.DataConverters.Accounting.Cyber2;
using Enterprise.DataConverters.Testing.DataImporters;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DataConverters.Testing.Accounting.Cyber2
{
	internal abstract class LedgerDataImporterBase : InterbaseImporterTestBase
	{
		public void TestGetEnterpriseLedgerType()
		{
			AssertEquals(LedgerTypes.AccountsPayable, Importer.GetEnterpriseLedgerType("C"));
			AssertEquals(LedgerTypes.AccountsReceivable, Importer.GetEnterpriseLedgerType("D"));
		}

		public void TestGetLocalCurrencyCodeIfCurrencyIsEmpty()
		{
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, Importer.GetLocalCurrencyCodeIfCurrencyIsEmpty(ZString.Empty));
			AssertEquals("AUD", Importer.GetLocalCurrencyCodeIfCurrencyIsEmpty("AUD"));
		}

		protected new LedgerDataImporter Importer
		{
			get { return (LedgerDataImporter)base.Importer; }
		}
	}
}
