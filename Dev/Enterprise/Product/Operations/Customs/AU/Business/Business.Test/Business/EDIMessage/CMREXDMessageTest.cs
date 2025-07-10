using CargoWise.EntityFramework;
using Enterprise.Customs.Common.AU;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMREXDMessage))]
	public class CMREXDMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestJE_EntryStatusUpdatedOnSaved()
		{
			TestJE_EntryStatusUpdatedOnSaved(CMRMessage.MessageSubTypes.Original, CustomsEntryStatus.AwaitingOriginal.Code);
			TestJE_EntryStatusUpdatedOnSaved(CMRMessage.MessageSubTypes.Amendment, CustomsEntryStatus.AwaitingReplacement.Code);
			TestJE_EntryStatusUpdatedOnSaved(CMRMessage.MessageSubTypes.Withdraw, CustomsEntryStatus.AwaitingWithdrawal.Code);
		}

		public void TestJE_EntryStatusNotUpdatedIfMessageAlreadyExisted()
		{
			JobDeclaration dec = Factory.New<JobDeclaration>();
			CMREXDMessage message = (CMREXDMessage)dec.Messages.AddNew(typeof(CMREXDMessage));
			message.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			Factory.Save();
			AssertEquals("JE_EntryStatus", CustomsEntryStatus.AwaitingOriginal.Code, dec.JE_EntryStatus);
			message.EM_Status = "XXX";
			dec.JE_EntryStatus = "YYY";
			Factory.Save();
			AssertEquals("JE_EntryStatus", "YYY", dec.JE_EntryStatus);
		}

		void TestJE_EntryStatusUpdatedOnSaved(string messageSubType, string expectedStatus)
		{
			JobDeclaration dec = Factory.New<JobDeclaration>();
			CMREXDMessage message = (CMREXDMessage)dec.Messages.AddNew(typeof(CMREXDMessage));
			message.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			message.EM_MessageSubType = messageSubType;
			Factory.Save();
			AssertEquals("JE_EntryStatus", expectedStatus, dec.JE_EntryStatus);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			EDIMessage result = (EDIMessage)GetNewBusinessObject();
			result.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			return result;
		}

		protected override bool CanPersistedObjectBeDeleted => false;
	}
}
