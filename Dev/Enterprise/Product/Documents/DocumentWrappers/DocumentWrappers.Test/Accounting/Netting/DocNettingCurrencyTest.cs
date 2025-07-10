using Enterprise.Accounting.Netting;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocNettingCurrency))]
	sealed class DocNettingCurrencyTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocNettingCurrency.New(NettingCurrency, Factory)
			};
		}

		NettingCurrency NettingCurrency;
		protected override void SetUp()
		{
			NettingCurrency = new NettingCurrency(Factory);
			base.SetUp();
		}
	}
}
