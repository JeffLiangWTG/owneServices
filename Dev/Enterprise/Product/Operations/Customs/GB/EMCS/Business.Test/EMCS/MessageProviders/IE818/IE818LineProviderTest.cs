using System.Linq;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(IE818LineProvider))]
	sealed class IE818LineProviderTest : LineProviderProviderAbstractTest<IE818LineProvider>
	{
		public void TestObservedShortageOrExcess()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Error for empty", decimal.Zero, LineProvider.ObservedShortageOrExcess);
				emcsInvoiceLine.JI_CustomsQuantity = 5.11;
				emcsInvoiceLine.ZG_DeclaredValue = 5.11;
				AssertEquals("No difference to the Declared", decimal.Zero, LineProvider.ObservedShortageOrExcess);
				emcsInvoiceLine.JI_CustomsQuantity = 10.12;
				emcsInvoiceLine.ZG_DeclaredValue = 5.11;
				AssertEquals("Customs Qty greater than the Declared Value", 5.01m, LineProvider.ObservedShortageOrExcess);
				emcsInvoiceLine.JI_CustomsQuantity = 1.34;
				AssertEquals("Customs Qty less than the Declared Value", -3.77m, LineProvider.ObservedShortageOrExcess);
				emcsInvoiceLine.JI_CustomsQuantity = 6.11;
				AssertEquals("Value is integer", "1", LineProvider.ObservedShortageOrExcess.ToString());
				emcsInvoiceLine.JI_CustomsQuantity = 5.61;
				AssertEquals("Value is normalized", "0.5", LineProvider.ObservedShortageOrExcess.ToString());
			});
		}

		public void TestRefusedQuantity()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Error for empty", decimal.Zero, LineProvider.RefusedQuantity);
				emcsInvoiceLine.Outturn.C5_RejectedQuantity = 5.15;
				AssertEquals(5.15m, LineProvider.RefusedQuantity);
				emcsInvoiceLine.Outturn.C5_RejectedQuantity = 2.00;
				AssertEquals("Value is integer", "2", LineProvider.RefusedQuantity.ToString());
				emcsInvoiceLine.Outturn.C5_RejectedQuantity = 3.50;
				AssertEquals("Value is normalized", "3.5", LineProvider.RefusedQuantity.ToString());
			});
		}

		public void TestUnsatisfactoryReasons_Null()
		{
			AssertEquals("IENumerables Not Null", false, LineProvider.UnsatisfactoryReasons.Any());
		}

		public void TestUnsatisfactoryReasons()
		{
			for (var i = 1; i < 6; i++)
			{
				emcsInvoiceLine.Outturn.ReportOfReceiptReasons.AddNew();
			}
			AssertEquals("Correct number of reasons", 5, LineProvider.UnsatisfactoryReasons.Count);
		}

		protected override IE818LineProvider GetLineProvider() => new IE818LineProvider(emcsInvoiceLine);
	}
}
