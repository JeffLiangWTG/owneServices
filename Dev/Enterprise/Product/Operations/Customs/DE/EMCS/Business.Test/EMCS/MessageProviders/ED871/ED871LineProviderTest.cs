using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(ED871LineProvider))]
	class ED871LineProviderTest : LineProviderAbstractTest<ED871LineProvider>
	{
		public void TestActualQuantity()
		{
			CombineAssertions(() =>
			{
				emcsInvoiceLine.JI_CustomsQuantity = 10.12;
				AssertEquals("Actual Quantity", 10.12m, LineProvider.ActualQuantity);

				emcsInvoiceLine.JI_CustomsQuantity = 100.200m;
				AssertEquals("Normalized Actual Quantity", "100.2", LineProvider.ActualQuantity.ToString());

				emcsInvoiceLine.JI_CustomsQuantity = 100.000m;
				AssertEquals("Normalized Actual Quantity", "100", LineProvider.ActualQuantity.ToString());
			});
		}

		public void TestActualQuantity_Rejected()
		{
			emcsInvoiceLine.JI_CustomsQuantity = 5.11;
			emcsInvoiceLine.Outturn.C5_RejectedQuantity = 1.34;
			AssertEquals("Actual Quantity", 3.77m, LineProvider.ActualQuantity);
		}

		public void TestExplanation()
		{
			AssertEquals("EXPLANATION ON REASON FOR SHORTAGE", LineProvider.Explanation.Text);
		}

		protected override ED871LineProvider GetLineProvider()
		{
			emcsInvoiceLine.Outturn.C5_OutturnResultReason = "EXPLANATION ON REASON FOR SHORTAGE";
			return new ED871LineProvider(emcsInvoiceLine);
		}

		protected override IEnumerable<Expression<Func<ED871LineProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.Explanation;
		}
	}
}
