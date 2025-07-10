using System;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSeaManArrivalPortCARLSTAmendmentGeneratorTest : CMRAmendmentGeneratorAbstractTest
	{
		public void TestUniqueIdentifierInfos()
		{
			var arrival = Bizo as CusSeaManArrivalPort;
			var result = ((CusSeaManArrivalPortCARLSTAmendmentGeneratorForTest)Generator).UniqueIdentifierInfos;
			AssertEquals("Length", 3, result.Length);
			AssertEquals("Result[0]", arrival.Header.BT_VesselNameInfo, result[0]);
			AssertEquals("Result[1]", arrival.Header.BT_VoyageNumInfo, result[1]);
			AssertEquals("Result[2]", arrival.BA_RL_NKArrivalPortInfo, result[2]);
		}

		protected override ICMRAmendmentGeneratorForTest GetGenerator(BusinessObject bizo) => new CusSeaManArrivalPortCARLSTAmendmentGeneratorForTest(bizo as CusSeaManArrivalPort);

		protected override Type ExpectedMessageType => typeof(CMRCARLSTMessage);

		protected override BusinessObject GetSavedBizo()
		{
			var header = Factory.New<CusSeaManTranHead>();
			var result = header.Arrivals.AddNew();
			Factory.Save();
			return result;
		}

		sealed class CusSeaManArrivalPortCARLSTAmendmentGeneratorForTest : CusSeaManArrivalPortCARLSTAmendmentGenerator, ICMRAmendmentGeneratorForTest
		{
			public CusSeaManArrivalPortCARLSTAmendmentGeneratorForTest(CusSeaManArrivalPort arrival) : base(arrival)
			{
			}

			public new ZPropertyInfo[] UniqueIdentifierInfos => base.UniqueIdentifierInfos;
			public new EDIMessage GenerateOriginalMessageCore() => base.GenerateOriginalMessageCore();
			public new EDIMessage GenerateAmendmentMessageCore() => base.GenerateAmendmentMessageCore();
			public new EDIMessage GenerateWithdrawalMessageCore(BusinessObject bizo, BusinessObject bizoInNewFactory) => base.GenerateWithdrawalMessageCore(bizo, bizoInNewFactory);
		}
	}
}
