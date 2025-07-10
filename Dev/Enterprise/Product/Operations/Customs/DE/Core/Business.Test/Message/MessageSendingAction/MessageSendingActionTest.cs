using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.CusTempStorage;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(MessageSendingAction))]
	class MessageSendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDetails()
		{
			var bo = (MessageSendingAction)GetNewBusinessObject();
			AssertEquals("REF", bo.Details);
		}
		protected override BusinessObject GetNewBusinessObject()
		{
			return new MessageSendingAction(JobHeader.CUSPCSCusTempStorageDecs.AddNew(), x => "REF");
		}

		CusTempStorageJobHeader JobHeader
		{
			get { return fJobHeader ?? (fJobHeader = Factory.New<CusTempStorageJobHeader>()); }
		}
		CusTempStorageJobHeader fJobHeader;
	}
}
