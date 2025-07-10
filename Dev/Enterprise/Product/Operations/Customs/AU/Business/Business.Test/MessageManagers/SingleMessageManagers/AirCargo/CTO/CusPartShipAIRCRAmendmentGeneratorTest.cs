using System;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusPartShipAIRCRAmendmentGeneratorTest : CMRAmendmentGeneratorAbstractTest
	{
		public void TestGetBuilder()
		{
			AssertEquals("BuilderType", typeof(AIRCRMessageBuilder), Generator.GetBuilder(PartShip).GetType());
		}

		public void TestGetMessageCollection()
		{
			AssertEquals("Messages", PartShip.Messages, Generator.GetMesssageCollection(PartShip));
		}

		public void TestUniqueIdentifierInfos()
		{
			var result = Generator.UniqueIdentifierInfos;
			AssertEquals("Length", 3, result.Length);
			AssertEquals("Result[0]", PartShip.HouseBill.MAWB.CM_FlightNoInfo, result[0]);
			AssertEquals("Result[1]", PartShip.CG_ArrivalDateInfo, result[1]);
			AssertEquals("Result[2]", PartShip.HouseBill.CS_HAWBInfo, result[2]);
		}

		protected override Type ExpectedMessageType => typeof(CMRAIRCRMessage);

		protected override ICMRAmendmentGeneratorForTest GetGenerator(BusinessObject bizo) => new CusPartShipAIRCRAmendmentGeneratorForTest(bizo as CusPartShip);

		protected override BusinessObject GetSavedBizo()
		{
			var mawb = Factory.New<CTOCusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var partShip = hawb.PartShips.AddNew();
			Factory.Save();
			return partShip;
		}

		CusPartShipAIRCRAmendmentGeneratorForTest generator;
		new CusPartShipAIRCRAmendmentGeneratorForTest Generator => generator ?? (generator = new CusPartShipAIRCRAmendmentGeneratorForTest(PartShip));

		CusPartShip partShip;
		CusPartShip PartShip => partShip ?? (partShip = (CusPartShip)GetSavedBizo());

		sealed class CusPartShipAIRCRAmendmentGeneratorForTest : CusPartShipAIRCRAmendmentGenerator, ICMRAmendmentGeneratorForTest
		{
			public CusPartShipAIRCRAmendmentGeneratorForTest(CusPartShip partShip) : base(partShip)
			{
			}

			public new ZPropertyInfo[] UniqueIdentifierInfos => base.UniqueIdentifierInfos;
			public new EDIMessage GenerateOriginalMessageCore() => base.GenerateOriginalMessageCore();
			public new EDIMessage GenerateAmendmentMessageCore() => base.GenerateAmendmentMessageCore();
			public new EDIMessage GenerateWithdrawalMessageCore(BusinessObject bizo, BusinessObject bizoInNewFactory) => base.GenerateWithdrawalMessageCore(bizo, bizoInNewFactory);
		}
	}
}
