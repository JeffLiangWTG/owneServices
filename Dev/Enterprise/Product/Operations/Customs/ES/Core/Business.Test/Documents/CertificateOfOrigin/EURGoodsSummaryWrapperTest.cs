using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Documents.DocDataObjects;

namespace Enterprise.Customs.ES.Business.Documents.CertificateOfOrigin.Testing
{
	class EURGoodsSummaryWrapperTest : TestCaseWithFactory
	{
		public void TestGetNewBuilder()
		{
			var wrapperForTest = new EURGoodsSummaryWrapperForTest(Array.Empty<JobComInvoiceLine>());
			AssertType<EUR1BoxItemsBuilder>("ItemsBuilder Type", wrapperForTest.GetNewBuilderExposed());
		}

		#region ItemsDetailDataProviderForTest

		class EURGoodsSummaryWrapperForTest : EURGoodsSummaryWrapper
		{
			public EURGoodsSummaryWrapperForTest(IEnumerable<JobComInvoiceLine> invoiceLines) : base(invoiceLines)
			{
			}

			public EU.Business.Documents.DocDataObjects.EUR1BoxItemsBuilder GetNewBuilderExposed() => GetNewBuilder();
		}

		#endregion
	}
}
