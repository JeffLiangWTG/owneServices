using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders.MessageManagers;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders.Query;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class CDSQuerySendingManagerTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestNoSender()
		{
			new CDSQuerySendingManager(null, ZGuid.Empty);
		}

		[TestDate(2022, 1, 1)]
		public void TestCannotSend()
		{
			var mrnSO = new CDSQueryMRNSendingObject(declaration, QueryNotificationType.Status);

			var sendManager = new CDSQuerySendingManager(mrnSO, ZGuid.Empty);

			sendManager.Send();

			var msg = ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage;
			AssertEquals("Check reason", "This entry does not have a value for its MRN", msg.Text);
			AssertEquals("Should be an Error", true, msg.WasError);
		}

		[TestDate(2022, 1, 1)]
		public void TestDeliveryFailure()
		{
			using (Factory.AddDisposableService())
			{
				var ducrSO = new CDSQueryDUCRSendingObject(declaration);
				var sendManager = new CDSQuerySendingManagerToTestDeliveryFailure(ducrSO, ZGuid.Empty);

				sendManager.Send();

				var msg = ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage;
				AssertEquals("Check msg", "An error occurred while trying to queue the query", msg.Text);
				AssertEquals("Should be Error", true, msg.WasError);
			}
		}

		[TestDate(2022, 1, 1)]
		public void TestDeliverySuccess()
		{
			using (Factory.AddDisposableService())
			{
				var ducrSO = new CDSQueryDUCRSendingObject(declaration);
				var sendManager = new CDSQuerySendingManager(ducrSO, ZGuid.Empty);

				sendManager.Send();

				var msg = ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage;
				AssertEquals("Check msg", "CDS Query has been queued successfully", msg.Text);
				AssertEquals("Should be Info", true, msg.WasInformation);

				var ediMsg = Factory.LoadTop1<Enterprise.Messaging.Business.EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkTable, "CusEntryHeader").AddToFilter(EDIMessageSchema.EM_LinkUniqueID, cusEntryHeader1.PK));
				AssertNotNull(ediMsg);
				AssertEquals("UDM", ediMsg.EM_ApplicationCode);
				AssertEquals("XUE", ediMsg.EM_MessageType);
				AssertEquals("TRX", ediMsg.EM_ReceiveTransmit);
				AssertEquals("SNT", ediMsg.EM_Status);
			}
		}

		[TestDate(2022, 1, 1)]
		public void TestGetUniversalEventText()
		{
			var msg = Factory.New<CDSDISQueryMessage>();
			msg.EM_ApplicationReference = "BOBCAT.GB132435465768.FAN";

			var listSO = new CDSQueryListSendingObject(msg);
			var sendManager = new CDSQuerySendingManager(listSO);

			var content = sendManager.GetUniversalEventContent();
			AssertEquals("SO property should be a Universal event", true, listSO.IsUniversalEvent);
			Assert("Should be a Universal event", content.StartsWith("<UniversalEvent"));
		}

		[TestDate(2022, 1, 1)]
		public void TestCDSIsUniversalEventProperty()
		{
			declaration.JE_MessageType = "IMP";
			var inventorySO = new CDSQueryInventorySendingObject(declaration);
			AssertEquals("SO property should be a Universal event", true, inventorySO.IsUniversalEvent);
			declaration.JE_MessageType = "EXP";
			AssertEquals("SO property should be a CDS Inventory Linking Query Message - not Universal event", false, inventorySO.IsUniversalEvent);

			var msg = Factory.New<CDSDISQueryMessage>();
			msg.EM_ApplicationReference = "BOBCAT.GB132435465768.FAN";

			var listSO = new CDSQueryListSendingObject(msg);
			var sendManager = new CDSQuerySendingManager(listSO);

			var content = sendManager.GetUniversalEventContent();
			AssertEquals("SO property should be a Universal event", true, listSO.IsUniversalEvent);
		}

		[TestDate(2022, 1, 1)]
		public void TestCDSMUCRQueryInventoryMessageSuccess()
		{
			declaration.JE_MessageType = "EXP";
			cusEntryHeader1.CH_MasterUCR = "MASTER123";
			var mucrInventorySO = new CDSQueryInventorySendingObject(declaration);
			var sendManager = new CDSQuerySendingManager(mucrInventorySO, ZGuid.Empty);
			sendManager.Send();

			var msg = ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage;
			AssertEquals("Check msg", "CDS Query has been queued successfully", msg.Text);
			AssertEquals("Should be Info", true, msg.WasInformation);

			var ediMsg = Factory.LoadTop1<Enterprise.Messaging.Business.EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkTable, "CusEntryHeader").AddToFilter(EDIMessageSchema.EM_LinkUniqueID, declaration.ActiveEntryHeaders[0].PK));
			AssertNotNull(ediMsg);
			AssertEquals("CDS", ediMsg.EM_ApplicationCode);
			AssertEquals("LQQ", ediMsg.EM_MessageType);
			AssertEquals("TRX", ediMsg.EM_ReceiveTransmit);
			AssertEquals("QUE", ediMsg.EM_Status);
			Assert("Message should be an InventoryLinkingQueryRequest", ediMsg.EM_MessageText.StartsWith("<inventoryLinkingQueryRequest"));
			AssertContains("Message should contain MUCR", "<ucr>MASTER123</ucr>", ediMsg.EM_MessageText);
			AssertContains("Message should contain UCRType=M", "<ucrType>M</ucrType>", ediMsg.EM_MessageText);
			AssertContains("Message should contain HTML interpretation", "<H3>Inventory Linking Query Request to CDS:</H3><p><strong>UCR: </strong>MASTER123<br><strong>UCR Type: </strong>M</p>", ediMsg.EM_MessageInterpretation);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_CustomsProfile = "ABC";

			var password = Factory.New<GlbExternalPassword_GB>();
			password.GP_GC = declaration.CompanyPK;
			password.Badge = declaration.JE_CustomsProfile;
			password.EORI = declaration.DeclarantTraderId;
			password.StatusMessage = "Status Message";
			password.Status = PasswordStatusList.Codes.Valid;
			password.GP_ExpiryDate = new ZDate(2022, 12, 1);
			password.GP_IssueDate = new ZDate(2021, 3, 22);
			password.IsTokenForCDS = true;

			var cei1 = declaration.CustomsEntryInstructions.AddNew();
			cei1.CEI_Style = "H1";

			var inv1 = declaration.Invoices.AddNew();
			var line1 = inv1.InvoiceLines.AddNew();
			line1.JI_CEI = cei1.PK;

			var doc = declaration.PreviousDocuments.AddNew();
			doc.CSI_Code = "DCR";
			doc.CSI_ReferenceNumber = "UNITTEST/00001";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();

			cusEntryHeader1 = declaration.ActiveEntryHeaders[0] as CusEntryHeader;
		}

		JobDeclaration declaration;
		CusEntryHeader cusEntryHeader1;
	}

	class CDSQuerySendingManagerToTestDeliveryFailure : CDSQuerySendingManager
	{
		public CDSQuerySendingManagerToTestDeliveryFailure(CDSQuerySendingObject sender, ZGuid cusEntryHeaderPK) : base(sender, cusEntryHeaderPK)
		{
		}

		protected override string DeliveryDestination => string.Empty;
	}
}
