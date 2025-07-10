using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocWithholdingTaxRate))]
	sealed class DocWithholdingTaxRateTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocWithholdingTaxRate.New(Withholding, Factory)
			};
		}

		AccWithholding Withholding;
		protected override void SetUp()
		{
			Withholding = Factory.New<AccWithholding>();
			base.SetUp();
		}
	}
}
