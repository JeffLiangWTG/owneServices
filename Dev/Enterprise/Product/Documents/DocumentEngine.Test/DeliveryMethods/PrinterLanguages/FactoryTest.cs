using Enterprise.DocumentEngine.Scheduler.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DeliveryMethods.PrinterLanguages.Testing
{
	sealed class FactoryTest : TestCase
	{
		public void TestEpsonHandled()
		{
			Base actual = Base.Create(StmPrintQueue.PrintLanguageTypes.Epson);
			Assert("Expected EpsonEscP or subclass, was " + actual.GetType(), typeof(EpsonEscP).IsAssignableFrom(actual.GetType()));
		}

		public void TestOkiHandled()
		{
			Base actual = Base.Create(StmPrintQueue.PrintLanguageTypes.OKI);
			Assert("Expected OkiMicroline or subclass, was " + actual.GetType(), typeof(OkiMicroline).IsAssignableFrom(actual.GetType()));
		}

		public void TestNoneHandled()
		{
			Base actual = Base.Create(StmPrintQueue.PrintLanguageTypes.None);
			Assert("[None] Expected None or subclass, was " + actual.GetType(), typeof(None).IsAssignableFrom(actual.GetType()));

			actual = Base.Create("");
			Assert("[empty string] Expected None or subclass, was " + actual.GetType(), typeof(None).IsAssignableFrom(actual.GetType()));

			actual = Base.Create(null);
			Assert("[null] Expected None or subclass, was " + actual.GetType(), typeof(None).IsAssignableFrom(actual.GetType()));
		}

		public void TestIbmHandled()
		{
			Base actual = Base.Create(StmPrintQueue.PrintLanguageTypes.IBM);
			Assert("Expected IbmProPrinter or subclass, was " + actual.GetType(), typeof(IbmProPrinter).IsAssignableFrom(actual.GetType()));
		}
	}
}
