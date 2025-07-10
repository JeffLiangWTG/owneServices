using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(StatusRequestDocManagerInfo))]
	public class StatusRequestDocManagerInfoTest : DocManagerInfoTestCase
	{
		public void TestRelatedObjects()
		{
			var responseMessage = SetupValidResponseMessage();
			var info = GetDocManagerInfo(true);
			AssertEquals("Have 1 response message.", 1, info.RelatedObjects.Length);
			AssertEquals("Linked response message", responseMessage.PK, info.RelatedObjects[0].PK);
		}

		public void TestBusinessEntity()
		{
			var info = GetDocManagerInfo(true);
			AssertType<StatusRequest>(info.BusinessEntity);
		}

		public override BusinessObject GetEmptyParentBusinessObject() => Factory.New<StatusRequest>();

		public override BusinessObject GetPopulatedParentBusinessObject() => statusRequest;

		protected override void SetUp()
		{
			base.SetUp();
			statusRequest = Factory.New<StatusRequest>();
		}
		StatusRequest statusRequest;

		EDIMessage SetupValidResponseMessage()
		{
			var responseMessage = Factory.NewWithValidTestData<EDIMessage>();
			responseMessage.EM_ApplicationCode = statusRequest.EM_ApplicationCode;
			responseMessage.EM_MessageType = statusRequest.EM_MessageType;
			responseMessage.EM_MessageSubType = statusRequest.EM_MessageSubType;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_LinkTable = statusRequest.TableName;
			responseMessage.EM_LinkUniqueID = statusRequest.PK;
			return responseMessage;
		}
	}
}
