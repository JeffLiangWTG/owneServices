using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public abstract class CMRIncomingMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			EDIMessage message = (EDIMessage)Factory.New(ExpectedBusinessObjectType);
			AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
		}

		#region Implementation

		protected void TestLoad(Type loadingType, Type expectedType, ZGuid pK)
		{
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			AssertEquals("Loaded Type", ExpectedBusinessObjectType, secondFactory.Load(loadingType, pK).GetType());
		}

		#endregion
	}
}
