using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CMRTypeDeciderTest : TestCaseWithFactory
	{
		public void TestTypeDecider()
		{
			CheckMessage(typeof(CMRContrlMessage));
			CheckMessage(typeof(CMRCTORECMessage));
			CheckMessage(typeof(CMRCTOREMMessage));
			CheckMessage(typeof(CMRDEPARTMessage));
			CheckMessage(typeof(CMRDEPRECMessage));
			CheckMessage(typeof(CMRDEPRELMessage));
			CheckMessage(typeof(CMREMMMessage));
			CheckMessage(typeof(CMRESMMessage));
			CheckMessage(typeof(CMREXDMessage));
			CheckMessage(typeof(CMRSTREQMessage));
			CheckMessage(typeof(CMRWARRELMessage));
			CheckMessage(typeof(CMRWARRETMessage));
			CheckMessage(typeof(CMRAIRCRMessage));
			CheckMessage(typeof(CMRSEACRMessage));
			CheckMessage(typeof(CMRCARLSTMessage));
			CheckMessage(typeof(CMRIMDMessage));
			CheckMessage(typeof(CMRPAYSTDMessage));
			CheckMessage(typeof(CMRSACMessage));
			CheckMessage(typeof(CMRREFACCMessage));
			CheckMessage(typeof(CMRDRWBCKMessage));
			CheckMessage(typeof(CMRSEQMessage));
			CheckMessage(typeof(CMRCLREGMessage));
		}

		#region Implementation

		protected void CheckMessage(Type typeToCheck)
		{
			CheckMessage(typeToCheck, typeToCheck);
		}

		protected void CheckMessage(Type typeToCheck, Type typeExpected)
		{
			BusinessObject message = Factory.New(typeToCheck);
			AssertEquals(typeExpected, decider.GetTypeForLoad(((INeedRow)message).Row, Factory));
		}

		protected override void SetUp()
		{
			base.SetUp();
			decider = new CMRTypeDecider();
		}

		CMRTypeDecider decider;
		#endregion

	}
}
