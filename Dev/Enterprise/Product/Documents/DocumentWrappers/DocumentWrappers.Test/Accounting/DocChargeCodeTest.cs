using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocChargeCode))]
	sealed class DocChargeCodeTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocChargeCode.New(ChargeCode, Factory)
			};
		}

		AccChargeCode ChargeCode;
		protected override void SetUp()
		{
			ChargeCode = Factory.New<AccChargeCode>();
			base.SetUp();
		}
	}
}
