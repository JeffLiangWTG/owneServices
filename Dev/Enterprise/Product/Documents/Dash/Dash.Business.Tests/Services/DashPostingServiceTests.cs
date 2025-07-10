using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Dash.Business.Services;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Dash.Business.Tests.Services
{
	public class DashPostingServiceTests : TestCaseWithDocumentFactory
	{
		public void TestPostUxml_Created_EDIMessage()
		{
			ExecutePostUxmlTest();
			ExecutePostUxmlTest(true);
		}

		void ExecutePostUxmlTest(bool clearUserContext = false)
		{
			// arrange
			var dashPostingService = new DashPostingService();
			var uxml = "<UniversalMessage/>";
			var dashDocumentId = Guid.NewGuid();
			var branchId = GlbBranch.CurrentBranch.PK;
			var departmentId = GlbDepartment.CurrentDepartment.PK;
			var ctx = Env.CurrentUserContext;

			try
			{
				// act
				if (clearUserContext)
				{
					Env.ClearUserContext();
				}
				var guid = dashPostingService.PostUxml(branchId, departmentId, dashDocumentId, uxml, Factory);
				Factory.Save();

				// assert
				AssertNotNull(guid);
				var ediMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.PK, guid));
				AssertNotNull(ediMessage);
				AssertEquals(ApplicationCodeList.Codes.UniversalDataMessaging, ediMessage.EM_ApplicationCode);
				AssertEquals(EDIMessage.Direction.Receive, ediMessage.EM_ReceiveTransmit);
				AssertEquals(EDIMessageTypeList.Codes.XDC, ediMessage.EM_MessageType);
				AssertEquals(EDIMessageSubTypeList.Codes.XmlUniversalShipment, ediMessage.EM_MessageSubType);
				AssertEquals(branchId, ediMessage.EM_GB);
				AssertEquals(departmentId, ediMessage.EM_GE);
				AssertEquals(EDIMessage.Status.Queued, ediMessage.EM_Status);
				AssertEquals(DashDocument.Schema.TableName, ediMessage.EM_LinkTable);
				AssertEquals(dashDocumentId, ediMessage.EM_LinkUniqueID);
				AssertEquals(uxml, ediMessage.EM_MessageText);
				AssertZDatesWithin5Minutes("Create time must be recent", ZDateTime.UtcNow, ediMessage.EM_SystemCreateTimeUtc);
				AssertEquals(DocManagerRegistry.Instance.ShipamaxServiceCode.Value, ediMessage.EM_SystemCreateUser);
			}
			finally
			{
				Env.SetUserContext(ctx);
			}
		}
	}
}
