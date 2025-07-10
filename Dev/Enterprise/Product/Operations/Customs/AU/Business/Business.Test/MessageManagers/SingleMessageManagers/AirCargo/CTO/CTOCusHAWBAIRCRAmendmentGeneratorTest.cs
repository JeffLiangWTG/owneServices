using System;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CTOCusHAWBAIRCRAmendmentGeneratorTest : CMRAmendmentGeneratorAbstractTest
	{
		public void TestUniqueIdentifierInfos()
		{
			var hawb = Bizo as CusHAWBBase;
			var result = Generator.UniqueIdentifierInfos;
			AssertEquals("Length", 3, result.Length);
			AssertEquals("Result[0]", hawb.MAWB.CM_FlightNoInfo, result[0]);
			AssertEquals("Result[1]", hawb.MAWB.CM_ArrivalDateInfo, result[1]);
			AssertEquals("Result[2]", hawb.CS_HAWBInfo, result[2]);
		}

		protected override Type ExpectedMessageType => typeof(CMRAIRCRMessage);

		protected override ICMRAmendmentGeneratorForTest GetGenerator(BusinessObject bizo) => new CTOCusHAWBAIRCRAmendmentGeneratorForTest(bizo as CTOCusHAWB);

		protected override BusinessObject GetSavedBizo()
		{
			var mawb = Factory.New<CTOCusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			Factory.Save();
			return hawb;
		}

		sealed class CTOCusHAWBAIRCRAmendmentGeneratorForTest : CTOCusHAWBAIRCRAmendmentGenerator, ICMRAmendmentGeneratorForTest
		{
			public CTOCusHAWBAIRCRAmendmentGeneratorForTest(CTOCusHAWB hAWB) : base(hAWB)
			{
			}

			public new ZPropertyInfo[] UniqueIdentifierInfos => base.UniqueIdentifierInfos;
			public new EDIMessage GenerateOriginalMessageCore() => base.GenerateOriginalMessageCore();
			public new EDIMessage GenerateAmendmentMessageCore() => base.GenerateAmendmentMessageCore();
			public new EDIMessage GenerateWithdrawalMessageCore(BusinessObject bizo, BusinessObject bizoInNewFactory) => base.GenerateWithdrawalMessageCore(bizo, bizoInNewFactory);
		}
	}
}
