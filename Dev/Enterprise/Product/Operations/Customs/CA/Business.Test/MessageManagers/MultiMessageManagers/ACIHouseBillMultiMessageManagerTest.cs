using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	sealed class ACIHouseBillMultiMessageManagerTest : TestCaseWithFactory
	{
		public void TestAllMessageManagers()
		{
			SingleMessageManager[] allMessageManagers = Manager.GetAllMessageManagers_Exposed();
			AssertEquals("AllMessageManagers.Length", 2, allMessageManagers.Length);
			AssertEquals("AllMessageManagers[0]", typeof(ACIHouseBillMessageManager), allMessageManagers[0].GetType());
			AssertEquals("AllMessageManagers[1]", typeof(ACIHouseBillMessageManager), allMessageManagers[1].GetType());
		}

		public void TestAllMessageManagersWithNull()
		{
			SingleMessageManager[] allMessageManagers = ManagerWithNull.GetAllMessageManagers_Exposed();
			AssertEquals("AllMessageManagers.Length", 0, allMessageManagers.Length);
		}

		public void TestRefreshAll()
		{
			var consol = Factory.New<ForwardingConsol>();

			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "HB1";

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "HB2";

			master.BP_ParentID = consol.PK;

			var house1 = master.HouseBills[0];
			house1.BW_ParentID = shipment.PK;
			house1.BW_HouseBill = "NO1";

			var house2 = master.HouseBills[1];
			house2.BW_ParentID = shipment2.PK;

			var sender = new SendsMessagesToCustomsShutterUppererForTest();
			Manager.RefreshAll(sender);
			AssertEquals("details should be refreshed from shipment", "HB1", master.HouseBills[0].BW_HouseBill);

			master.HouseBills.DeleteAll();
			Manager.RefreshAll(sender);
			AssertEquals("nothing for refresh", 0, master.HouseBills.Count);
		}

		public void TestSendOriginalMessages()
		{
			var manager = Manager;
			var sender = new SendsMessagesToCustomsShutterUppererForTest();
			sender.ThrowExceptionOnInvalidOperation = false;
			Env.Security.ConsolCAeManifestSendWithMessageErrors.IsAllowed = false;
			manager.SendOriginalMessages(sender, manager.GetAllMessageManagers_Exposed().Cast<ACIHouseBillMessageManager>().ToArray());
			AssertEquals("House1 Messages Count", 0, house1.Messages.Count);
			AssertEquals("House2 Messages Count", 0, house2.Messages.Count);
			Env.Security.ConsolCAeManifestSendWithMessageErrors.IsAllowed = true;
			manager.SendOriginalMessages(sender, manager.GetAllMessageManagers_Exposed().Cast<ACIHouseBillMessageManager>().ToArray());
			AssertEquals("House1 Messages Count", 1, house1.Messages.Count);
			AssertContains("MessageText", "CCN1", house1.Messages[0].EM_MessageText);
			AssertEquals("House2 Messages Count", 1, house2.Messages.Count);
			AssertContains("MessageText", "CCN2", house2.Messages[0].EM_MessageText);
		}

		public void TestSendCloseMessages()
		{
			var manager = Manager;
			var sender = new SendsMessagesToCustomsShutterUppererForTest();
			sender.ThrowExceptionOnInvalidOperation = false;
			Env.Security.ConsolCAeManifestSendWithMessageErrors.IsAllowed = false;
			manager.SendCloseMessages(sender);
			AssertEquals("Count", 0, master.Messages.Count);
			Env.Security.ConsolCAeManifestSendWithMessageErrors.IsAllowed = true;
			manager.SendCloseMessages(sender);
			AssertEquals("Count", 1, master.Messages.Count);
			var message = master.Messages[0];
			AssertContains("MessageText", "CLS-CAM1", message.EM_MessageText);
			AssertContains("MessageText", "CCN1", message.EM_MessageText);
			AssertNotContains("MessageText", "CCN2", message.EM_MessageText);
			manager.SendCloseMessages(sender, true, true);
			AssertEquals("Count", 1, master.Messages.Count);
			message = master.Messages[0];
			AssertContains("MessageText", "CLS-CAM1", message.EM_MessageText);
			AssertContains("MessageText", "CCN1", message.EM_MessageText);
			AssertNotContains("MessageText", "CCN2", message.EM_MessageText);
		}

		public void TestGetCloseWrapper()
		{
			var manager = Manager;
			var closeWrapper = manager.GetCloseWrapper();
			AssertEquals("Count", 1, closeWrapper.RelatedCCNs.Count());
			AssertEquals("CCN", "CCN1", closeWrapper.RelatedCCNs.First());
		}

		public void TestGetAcceptedCCNsFromCloseWrapper()
		{
			var manager = Manager;
			var closeWrapper = manager.GetCloseWrapper();
			AssertEquals("CCN", "CCN1", manager.GetAcceptedCCNsFromCloseWrapper(closeWrapper));
			house2.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
			manager = Manager;
			closeWrapper = manager.GetCloseWrapper();
			AssertEquals("CCN", "CCN1, CCN2", manager.GetAcceptedCCNsFromCloseWrapper(closeWrapper));
			house1.BW_CustomsStatus = ZString.Empty;
			house2.BW_CustomsStatus = ZString.Empty;
			manager = Manager;
			closeWrapper = manager.GetCloseWrapper();
			AssertEquals("CCN", ZString.Empty, manager.GetAcceptedCCNsFromCloseWrapper(closeWrapper));
			house1.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
		}

		public void TestWithdrawCloseMessages()
		{
			var manager = Manager;
			var sender = new SendsMessagesToCustomsShutterUppererForTest();
			manager.WithdrawCloseMessages(sender);
			AssertEquals("Count", 1, master.Messages.Count);
			var message = master.Messages[0];
			AssertContains("MessageText", "CLS-CAM1", message.EM_MessageText);
			AssertContains("MessageText", "BGM+87+8036X666+1'", message.EM_MessageText);
		}

		public void TestResetToOriginal()
		{
			var manager = Manager;
			var sender = new SendsMessagesToCustomsShutterUppererForTest();
			manager.useBase = false;
			manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManagerWithChanges };
			Env.Security.ConsolCAeManifestResetToOriginal.IsAllowed = false;
			sender.ReturnAllForWhichMessagesShouldWeReset = true;
			manager.ResetToOriginal(sender);
			AssertEquals("ResetToOriginalCalled", false, manager.allMessageManagers[0].ResetToOriginalCalled);
			Env.Security.ConsolCAeManifestResetToOriginal.IsAllowed = true;
			sender.ReturnAllForWhichMessagesShouldWeReset = true;
			manager.ResetToOriginal(sender);
			AssertEquals("ResetToOriginalCalled", true, manager.allMessageManagers[0].ResetToOriginalCalled);
		}

		public void TestCheckBW_HouseBillValidation_UsesDictionaryWhenValidationIsFromMaster()
		{
			house1.BW_HouseBill = "1234";
			house2.BW_HouseBill = "5678";

			var manager = Manager;
			var sender = new SendsMessagesToCustomsShutterUppererForTest();
			sender.ThrowExceptionOnInvalidOperation = false;

			Env.Security.ConsolCAeManifestSendWithMessageErrors.IsAllowed = false;
			manager.SendOriginalMessages(sender, manager.GetAllMessageManagers_Exposed().Cast<ACIHouseBillMessageManager>().ToArray());

			AssertEquals("There should only be 2 hits since we are using a dictionary when validating from master.", 2, master.ValidationCallsOriginatingFromMaster);

			house2.BW_HouseBill = "1234";
			house2.Validation.ValidateBW_HouseBill();
			AssertEquals("It should not have used the dictionary for validation.", 2, master.ValidationCallsOriginatingFromMaster);
			Assert("CheckBW_HouseBill uses default validation when changing manually, so there is a duplicate error.", house2.HasMessageErrors);

			var duplicateMessageErrorExists = house2.GetMessageErrors().Any(errors => errors.Message == "Message Error - BW_HouseBill: Another House Bill has same bill number.");
			Assert(duplicateMessageErrorExists);
		}

		#region Implemenetation

		ACIHouseBillMultiMessageManagerForTesting Manager
		{
			get { return new ACIHouseBillMultiMessageManagerForTesting(() => master); }
		}

		ACIHouseBillMultiMessageManagerForTesting ManagerWithNull
		{
			get { return new ACIHouseBillMultiMessageManagerForTesting(null); }
		}

		CusCAeMHMaster master;
		CusCAeMHHouse house1;
		CusCAeMHHouse house2;
		protected override void SetUp()
		{
			master = Factory.New<CusCAeMHMaster>();
			master.BP_MessageReference = "CAM1";
			master.BP_PrimaryCCN = "8036X666";
			house1 = master.HouseBills.AddNew();
			house1.BW_HouseCCN = "CCN1";
			house1.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
			house1.BW_Weight = 1;
			house2 = master.HouseBills.AddNew();
			house2.BW_HouseCCN = "CCN2";
			house2.BW_Weight = 1;
		}

		#region ACIHouseBillMultiMessageManagerForTesting

		class ACIHouseBillMultiMessageManagerForTesting : ACIHouseBillMultiMessageManager
		{
			public ACIHouseBillMultiMessageManagerForTesting(GetMaserDelegate getMasterDelegate)
				: base(getMasterDelegate)
			{
				useBase = true;
			}

			protected override SingleMessageManager[] GetAllMessageManagers()
			{
				if (!useBase)
				{
					return allMessageManagers;
				}
				else
				{
					return base.GetAllMessageManagers();
				}
			}

			public bool useBase;
			public TestHelperSingleMessageManager[] allMessageManagers;

			public SingleMessageManager[] GetAllMessageManagers_Exposed()
			{
				return GetAllMessageManagers();
			}
		}

		#endregion

		class SendsMessagesToCustomsShutterUppererForTest : SendsMessagesToCustomsShutterUpperer, IConsolSendsMessagesToCustoms
		{
			SingleMessageManager[] IConsolSendsMessagesToCustoms.WhichMessagesShouldWeRefresh(SingleMessageManager[] allManagers)
			{
				return allManagers;
			}
		}

		TestHelperSingleMessageManager singleManagerWithChanges;
		TestHelperSingleMessageManager SingleManagerWithChanges
		{
			get
			{
				if (singleManagerWithChanges == null)
				{
					singleManagerWithChanges = new TestHelperSingleMessageManager();
					singleManagerWithChanges.BusinessObject.Factory.Save();
					singleManagerWithChanges.canSendWithdrawal = true;
					singleManagerWithChanges.ReturnDifferentOriginalMessages = true;
				}
				return singleManagerWithChanges;
			}
		}

		#endregion
	}
}
