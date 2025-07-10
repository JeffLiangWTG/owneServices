using System;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSeaManTranHeadSEAIARAmendmentGeneratorTest : CMRAmendmentGeneratorAbstractTest
	{
		public void TestUniqueIdentifierInfos()
		{
			var transportHeader = Bizo as CusSeaManTranHead;
			var result = ((CusSeaManTranHeadSEAIARAmendmentGeneratorForTest)Generator).UniqueIdentifierInfos;
			AssertEquals("Length", 2, result.Length);
			AssertEquals("Result[0]", transportHeader.BT_VesselNameInfo, result[0]);
			AssertEquals("Result[1]", transportHeader.BT_VoyageNumInfo, result[1]);
		}

		protected override ICMRAmendmentGeneratorForTest GetGenerator(BusinessObject bizo) => new CusSeaManTranHeadSEAIARAmendmentGeneratorForTest(bizo as CusSeaManTranHead);

		protected override Type ExpectedMessageType => typeof(CMRSEAIARMessage);

		protected override BusinessObject GetSavedBizo()
		{
			var result = Factory.New<CusSeaManTranHead>();
			Factory.Save();
			return result;
		}

		sealed class CusSeaManTranHeadSEAIARAmendmentGeneratorForTest : CusSeaManTranHeadSEAIARAmendmentGenerator, ICMRAmendmentGeneratorForTest
		{
			public CusSeaManTranHeadSEAIARAmendmentGeneratorForTest(CusSeaManTranHead transportHeader) : base(transportHeader)
			{
			}

			public new ZPropertyInfo[] UniqueIdentifierInfos => base.UniqueIdentifierInfos;
			public new EDIMessage GenerateOriginalMessageCore() => base.GenerateOriginalMessageCore();
			public new EDIMessage GenerateAmendmentMessageCore() => base.GenerateAmendmentMessageCore();
			public new EDIMessage GenerateWithdrawalMessageCore(BusinessObject bizo, BusinessObject bizoInNewFactory) => base.GenerateWithdrawalMessageCore(bizo, bizoInNewFactory);
		}
	}
}
