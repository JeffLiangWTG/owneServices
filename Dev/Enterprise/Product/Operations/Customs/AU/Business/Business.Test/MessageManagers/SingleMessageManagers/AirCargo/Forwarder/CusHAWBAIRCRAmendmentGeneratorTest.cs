using System;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusHAWBAIRCRAmendmentGeneratorTest : CMRAmendmentGeneratorAbstractTest
	{
		public void TestUniqueIdentifierInfos()
		{
			var hawb = Bizo as CusHAWBBase;
			var result = ((CusHAWBAIRCRAmendmentGeneratorForTest)Generator).UniqueIdentifierInfos;
			AssertEquals("Length", 4, result.Length);
			AssertEquals("Result[0]", hawb.MAWB.CM_FlightNoInfo, result[0]);
			AssertEquals("Result[1]", hawb.MAWB.CM_ArrivalDateInfo, result[1]);
			AssertEquals("Result[2]", hawb.MAWB.CM_MAWBInfo, result[2]);
			AssertEquals("Result[3]", hawb.CS_HAWBInfo, result[3]);
		}

		protected override Type ExpectedMessageType => typeof(CMRAIRCRMessage);

		protected override ICMRAmendmentGeneratorForTest GetGenerator(BusinessObject bizo) => new CusHAWBAIRCRAmendmentGeneratorForTest(bizo as CusHAWB);

		protected override BusinessObject GetSavedBizo()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			Factory.Save();
			return hawb;
		}

		sealed class CusHAWBAIRCRAmendmentGeneratorForTest : CusHAWBAIRCRAmendmentGenerator, ICMRAmendmentGeneratorForTest
		{
			public CusHAWBAIRCRAmendmentGeneratorForTest(CusHAWB hAWB) : base(hAWB)
			{
			}

			public new ZPropertyInfo[] UniqueIdentifierInfos => base.UniqueIdentifierInfos;
			public new EDIMessage GenerateOriginalMessageCore() => base.GenerateOriginalMessageCore();
			public new EDIMessage GenerateAmendmentMessageCore() => base.GenerateAmendmentMessageCore();
			public new EDIMessage GenerateWithdrawalMessageCore(BusinessObject bizo, BusinessObject bizoInNewFactory) => base.GenerateWithdrawalMessageCore(bizo, bizoInNewFactory);
		}
	}
}
