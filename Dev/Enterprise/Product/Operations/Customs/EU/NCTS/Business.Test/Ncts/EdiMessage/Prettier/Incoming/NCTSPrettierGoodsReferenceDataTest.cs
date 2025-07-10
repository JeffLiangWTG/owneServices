using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NCTSPrettierGoodsReferenceDataTest : TestCase
	{
		public void TestGoodsReferenceDataSequenceNumber() => AssertEquals(1, prettierGoodsReferenceData.SequenceNumber);
		public void TestGoodsReferenceDataDeclarationGoodsItemNumber() => AssertEquals(2, prettierGoodsReferenceData.DeclarationGoodsItemNumber);
		protected override void SetUp()
		{
			base.SetUp();

			prettierGoodsReferenceData = new NCTSPrettierGoodsReferenceData(
				sequenceNumber: "1",
				declarationGoodsItemNumber: "2");
		}
		NCTSPrettierGoodsReferenceData prettierGoodsReferenceData;
	}
}
