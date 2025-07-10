using System;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSeaManArrivalPortSEAAARAmendmentGeneratorTest : CMRAmendmentGeneratorAbstractTest
	{
		public void TestUniqueIdentifierInfos()
		{
			var arrival = Bizo as CusSeaManArrivalPort;
			var result = ((CusSeaManArrivalPortSEAAARAmendmentGeneratorForTest)Generator).UniqueIdentifierInfos;
			AssertEquals("Length", 4, result.Length);
			AssertEquals("Result[0]", arrival.Header.BT_VesselNameInfo, result[0]);
			AssertEquals("Result[1]", arrival.Header.BT_VoyageNumInfo, result[1]);
			AssertEquals("Result[2]", arrival.BA_RL_NKArrivalPortInfo, result[2]);
			AssertEquals("Result[3]", arrival.BA_OA_CTOAddressInfo, result[3]);
		}

		protected override ICMRAmendmentGeneratorForTest GetGenerator(BusinessObject bizo) => new CusSeaManArrivalPortSEAAARAmendmentGeneratorForTest(bizo as CusSeaManArrivalPort);

		protected override Type ExpectedMessageType => typeof(CMRSEAAARMessage);

		protected override BusinessObject GetSavedBizo()
		{
			var header = Factory.New<CusSeaManTranHead>();
			var result = header.Arrivals.AddNew();
			Factory.Save();
			return result;
		}

		sealed class CusSeaManArrivalPortSEAAARAmendmentGeneratorForTest : CusSeaManArrivalPortSEAAARAmendmentGenerator, ICMRAmendmentGeneratorForTest
		{
			public CusSeaManArrivalPortSEAAARAmendmentGeneratorForTest(CusSeaManArrivalPort arrival) : base(arrival)
			{
			}

			public new ZPropertyInfo[] UniqueIdentifierInfos => base.UniqueIdentifierInfos;
			public new EDIMessage GenerateOriginalMessageCore() => base.GenerateOriginalMessageCore();
			public new EDIMessage GenerateAmendmentMessageCore() => base.GenerateAmendmentMessageCore();
			public new EDIMessage GenerateWithdrawalMessageCore(BusinessObject bizo, BusinessObject bizoInNewFactory) => base.GenerateWithdrawalMessageCore(bizo, bizoInNewFactory);
		}
	}
}
