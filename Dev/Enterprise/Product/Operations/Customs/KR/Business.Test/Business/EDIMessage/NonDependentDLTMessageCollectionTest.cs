using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(NonDependentDLTMessageCollection))]
	public class NonDependentDLTMessageCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestFilteredCollection()
		{
			var newBranch = Factory.New<GlbBranch>();
			var newCompany = Factory.New<GlbCompany>();
			newBranch.GB_GC = newCompany.PK;
			newCompany.GC_Name = "Test Company Name";

			var msg1 = Factory.New<EDIMessage>();
			msg1.EM_GB = newBranch.PK;
			msg1.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			msg1.EM_MessageType = KR.Messaging.Constants.EDIInterchangeType.DLT;
			msg1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var msg2 = Factory.New<EDIMessage>();
			msg2.EM_GB = GlbBranch.CurrentBranch.PK;
			msg2.EM_ApplicationCode = EDIMessage.ApplicationCodes.BRCustoms;
			msg2.EM_MessageType = KR.Messaging.Constants.EDIInterchangeType.DLT;
			msg2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var msg3 = Factory.New<EDIMessage>();
			msg3.EM_GB = GlbBranch.CurrentBranch.PK;
			msg3.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			msg3.EM_MessageType = KR.Messaging.Constants.EDIInterchangeType.RSP;
			msg3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var msg4 = Factory.New<EDIMessage>();
			msg4.EM_GB = GlbBranch.CurrentBranch.PK;
			msg4.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			msg4.EM_MessageType = KR.Messaging.Constants.EDIInterchangeType.DLT;
			msg4.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var msg5 = Factory.New<EDIMessage>();
			msg5.EM_GB = GlbBranch.CurrentBranch.PK;
			msg5.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			msg5.EM_MessageType = KR.Messaging.Constants.EDIInterchangeType.DLT;
			msg5.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var msg6 = Factory.New<EDIMessage>();
			msg6.EM_GB = GlbBranch.CurrentBranch.PK;
			msg6.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			msg6.EM_MessageType = KR.Messaging.Constants.EDIInterchangeType.DLT;
			msg6.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var collection = new NonDependentDLTMessageCollection(Factory, GlbCompany.CurrentCompany);
			collection.Load();

			AssertCollectionNotContains(msg1, collection);
			AssertCollectionNotContains(msg2, collection);
			AssertCollectionNotContains(msg3, collection);
			AssertCollectionContains(msg4, collection);
			AssertCollectionNotContains(msg5, collection);
			AssertCollectionContains(msg6, collection);
		}
		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<EDIMessage>();
		protected override BusinessObjectCollection GetCollectionToTest() => new NonDependentDLTMessageCollection(Factory, GlbCompany.CurrentCompany);
	}
}
