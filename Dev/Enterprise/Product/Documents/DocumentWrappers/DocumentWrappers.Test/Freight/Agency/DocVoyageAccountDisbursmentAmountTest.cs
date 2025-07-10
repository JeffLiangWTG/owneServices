using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocVoyageAccountDisbursementAmount))]
	sealed class DocVoyageAccountDisbursmentAmountTest : DocumentWrapperTestCase
	{
		public void TestTitle()
		{
			AssertEquals("title", DocVoyageAccountDisbursementAmount.New("title", Money.Empty, Factory).Title);
		}

		public void TestLocalAmount()
		{
			RefCurrency currency = GlbCompany.CurrentCompany.LocalCurrency;

			AssertEquals("amount", 5323.34m, DocVoyageAccountDisbursementAmount.New("title", new Money(5323.34, currency), Factory).LocalAmount.Amount);
			AssertEquals("currency", currency.RX_Code, DocVoyageAccountDisbursementAmount.New("title", new Money(2134, currency), Factory).LocalAmount.Currency.Code);
		}

		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocVoyageAccountDisbursementAmount.New("Title", new Money(213, GlbCompany.CurrentCompany.LocalCurrency), Factory),
			};
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return DocVoyageAccountDisbursementAmount.New("Title", Money.Empty, Factory);
		}

		#endregion
	}
}
