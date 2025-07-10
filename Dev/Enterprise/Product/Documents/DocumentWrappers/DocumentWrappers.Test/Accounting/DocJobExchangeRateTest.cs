using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocJobExchangeRate))]
	sealed class DocJobExchangeRateTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocJobExchangeRate.New(Rate, Factory)
			};
		}

		ExchangeRate Rate;
		protected override void SetUp()
		{
			Rate = Factory.New<ExchangeRate>();
			base.SetUp();
		}
	}
}
