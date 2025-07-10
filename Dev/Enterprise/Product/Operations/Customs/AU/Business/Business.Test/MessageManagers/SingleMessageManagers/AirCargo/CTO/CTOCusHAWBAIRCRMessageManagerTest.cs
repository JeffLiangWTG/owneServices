namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CTOCusHAWBAIRCRMessageManagerTest : CusHAWBBaseAIRCRManagerAbstractTest
	{
		public override void TestMessageFriendlyName()
		{
			HAWB.CS_HAWB = "08133333333";
			AssertEquals("MessageFriendlyName", "Air Cargo Report for MAWB: 08133333333", Manager.MessageFriendlyName);
		}

		protected override CMRMessageManager GetManager() => GetManager(HAWB);

		protected override CMRMessageManager GetManager(CusHAWBBase houseBill) => new CTOCusHAWBAIRCRMessageManager(houseBill as CTOCusHAWB);

		CTOCusHAWB hAWB;
		protected override CusHAWBBase HAWB
		{
			get
			{
				if (hAWB == null)
				{
					var mawb = Factory.New<CTOCusMAWB>();
					hAWB = mawb.ChildBills.AddNew();
				}
				return hAWB;
			}
		}
	}
}
