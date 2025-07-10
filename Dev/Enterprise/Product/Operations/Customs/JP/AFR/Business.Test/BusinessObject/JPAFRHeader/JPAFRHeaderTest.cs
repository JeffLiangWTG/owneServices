using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(JPAFRHeader))]
	class JPAFRHeaderTest : EnterpriseBusinessObjectTestCase
	{
		[TestDate(2017, 5, 3)]
		public void TestCheckJPH_VesselDetailsChanged()
		{
			using (JPAFRRegistry.Instance.AFR2017EffectiveLiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2017, 10, 06)))
			{
				var header = Factory.New<JPAFRHeader>();
				AssertEquals(false, header.JPH_IsShippingLineEntry);
				AssertEquals(false, header.JPH_VesselDetailsChanged);
			}

			using (JPAFRRegistry.Instance.AFR2017EffectiveLiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2017, 1, 1)))
			{
				var header = Factory.New<JPAFRHeader>();
				AssertEquals(false, header.JPH_IsShippingLineEntry);
				AssertEquals(true, header.JPH_VesselDetailsChanged);
			}
		}

		public void TestNewVesselVoyage()
		{
			var header = Factory.New<JPAFRHeader>();
			var blanketVesselChange = new BlanketVesselChange(header);
			blanketVesselChange.Factory.Save();
			AssertNotNull(header.NewVesselVoyage);
		}

		public void TestUniversalDataContext()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_JobReference = "AFR0015654";
			IDataContextManager manager = null;
			AssertNoExceptionThrown(() => { manager = header.GetUniversalDataContextManager(); });
			AssertNotNull("JPAFRHeader should have [UniversalDataContext(DataContextType.AFRHeader)] attribute", manager);
			AssertEquals(DataContextType.AFRHeader, manager.DataContextType);
			AssertEquals("AFR0015654", manager.DataContextKey);
		}

		public void TestGetMatchingContainers()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var container1 = bill1.Containers.AddNew();
			container1.JPC_ContainerNum = "CONT1";
			var container2 = bill1.Containers.AddNew();
			container2.JPC_ContainerNum = "CONT1";
			var container3 = bill2.Containers.AddNew();
			container3.JPC_ContainerNum = "CONT1";
			var container4 = bill2.Containers.AddNew();
			container4.JPC_ContainerNum = "CONT4";
			AssertEquals(0, header.GetMatchingContainers(ZGuid.Invalid, "CONT5").Count());
			var list = header.GetMatchingContainers(ZGuid.Invalid, "CONT4").ToArray();
			AssertEquals(1, list.Length);
			AssertCollectionContains(container4, list);
			list = header.GetMatchingContainers(container2.PK, "CONT1").ToArray();
			AssertEquals(2, list.Length);
			AssertCollectionContains(container1, list);
			AssertCollectionContains(container3, list);
		}

		public void TestMessagesAndAFRMessages()
		{
			var header = Factory.New<JPAFRHeader>();
			var ediMessage1 = Factory.New<JPAFRMessage>();
			ediMessage1.EM_MessageText = "Dummy Text";
			ediMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			ediMessage1.EM_LinkedObject = header;
			ediMessage1.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			var ediMessage2 = Factory.New<JPAFRMessage>();
			ediMessage2.EM_MessageText = "Dummy Text";
			ediMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			ediMessage2.EM_LinkedObject = ediMessage2;
			ediMessage2.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			var ediMessage3 = Factory.New<JPAFRMessage>();
			ediMessage3.EM_MessageText = "Dummy Text";
			ediMessage3.EM_ApplicationCode = EDIMessage.ApplicationCodes.Unknown;
			ediMessage3.EM_LinkedObject = header;
			var log = header.Logs.AddNew(Events.MessageStatusChange);
			var pivot = Factory.New<GenPivot>();
			pivot.XX_Relation1ID = log.PK;
			var ediMessage4 = Factory.New<JPAFRMessage>();
			ediMessage4.EM_MessageText = "Dummy Text";
			ediMessage4.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			ediMessage4.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage4.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			pivot.XX_Relation2ID = ediMessage4.PK;
			pivot = Factory.New<GenPivot>();
			pivot.XX_Relation1ID = log.PK;
			var ediMessage5 = Factory.New<JPAFRMessage>();
			ediMessage5.EM_MessageText = "Dummy Text";
			ediMessage5.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			ediMessage5.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage5.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			pivot.XX_Relation2ID = ediMessage5.PK;
			pivot = Factory.New<GenPivot>();
			pivot.XX_Relation1ID = log.PK;
			var ediMessage6 = Factory.New<JPAFRMessage>();
			ediMessage6.EM_MessageText = "Dummy Text";
			ediMessage6.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			ediMessage6.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			ediMessage6.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			pivot.XX_Relation2ID = ediMessage6.PK;
			Factory.Save();

			var messages = header.Messages;
			AssertEquals(1, messages.Count);
			AssertEquals(ediMessage1.PK, messages[0].PK);

			var afrMessages = header.AFRMessages;
			AssertEquals(typeof(EDIMessageForDisplayCollection<JPAFRMessage>), afrMessages.GetType());
			AssertEquals(2, afrMessages.Count);
			AssertCollectionContains(ediMessage1, afrMessages);
			AssertCollectionContains(ediMessage4, afrMessages);
		}

		public void TestIsBillRegistrationCompletedAndLogBillRegistrationCompletion()
		{
			var header = Factory.New<JPAFRHeader>();
			AssertEquals(false, header.IsBillRegistrationCompleted);
			AssertEquals("", header.JPH_BillRegistrationStatus);
			AssertNull(header.Logs.MostRecentLogByEventTime(Events.TaskCompleted));
			header.LogBillRegistrationCompletion();
			AssertEquals(true, header.IsBillRegistrationCompleted);
			AssertEquals("Completed", header.JPH_BillRegistrationStatus);
			var log = header.Logs.MostRecentLogByEventTime(Events.TaskCompleted);
			AssertEquals(User.ServiceUserCode, log.SL_GS_NKUser);
			AssertEquals("Bill Registration", log.SL_Reference);
			log.SL_GS_NKUser = "ZZ";
			AssertEquals(false, header.IsBillRegistrationCompleted);
			AssertEquals("", header.JPH_BillRegistrationStatus);
			log.SL_GS_NKUser = User.ServiceUserCode;
			AssertEquals(true, header.IsBillRegistrationCompleted);
			AssertEquals("Completed", header.JPH_BillRegistrationStatus);
			log.Cancel();
			AssertEquals(false, header.IsBillRegistrationCompleted);
			AssertEquals("", header.JPH_BillRegistrationStatus);
			header.JPH_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillRegistrationCompletion;
			AssertEquals("Pending", header.JPH_BillRegistrationStatus);
		}

		public void TestIsDepartureTimeRegistrationCompletedAndLogDepartureTimeRegistrationCompletion()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_IsShippingLineEntry = true;
			AssertEquals(false, header.IsDepartureTimeRegistered);
			AssertEquals("", header.JPH_BillRegistrationStatus);
			AssertNull(header.Logs.MostRecentLogByEventTime(Events.TaskCompleted));
			header.LogDepartureTimeRegistration();
			AssertEquals(true, header.IsDepartureTimeRegistered);
			AssertEquals("Completed", header.JPH_BillRegistrationStatus);
			var log = header.Logs.MostRecentLogByEventTime(Events.TaskCompleted);
			AssertEquals(User.ServiceUserCode, log.SL_GS_NKUser);
			AssertEquals("Departure Time Registration", log.SL_Reference);
			log.SL_GS_NKUser = "ZZ";
			AssertEquals(false, header.IsDepartureTimeRegistered);
			AssertEquals("", header.JPH_BillRegistrationStatus);
			log.SL_GS_NKUser = User.ServiceUserCode;
			AssertEquals(true, header.IsDepartureTimeRegistered);
			AssertEquals("Completed", header.JPH_BillRegistrationStatus);
			log.Cancel();
			AssertEquals(false, header.IsDepartureTimeRegistered);
			AssertEquals("", header.JPH_BillRegistrationStatus);
			header.JPH_MessageStatus = MessageStatusList.Codes.AwaitingDepartureTimeRegistration;
			AssertEquals("Pending", header.JPH_BillRegistrationStatus);
		}

		public void TestCancelBillRegistrationCompletionLog()
		{
			var header = Factory.New<JPAFRHeader>();
			header.LogBillRegistrationCompletion();
			header.LogBillRegistrationCompletion();
			var logs = header.Logs.Find(JPAFRHeader.BillRegistrationCompletedQuery);
			AssertEquals(2, logs.Length);
			var log1 = logs[0];
			var log2 = logs[1];
			header.CancelBillRegistrationCompletionLog();
			AssertEquals("log1.IsCancelled", true, log1.IsCancelled);
			AssertEquals("log2.IsCancelled", true, log2.IsCancelled);
			logs = header.Logs.Find(JPAFRHeader.BillRegistrationCompletedQuery);
			AssertEquals(0, logs.Length);
		}

		public void TestCancelDepartureTimeRegistrationCompletionLog()
		{
			var header = Factory.New<JPAFRHeader>();
			header.LogDepartureTimeRegistration();
			header.LogDepartureTimeRegistration();
			var logs = header.Logs.Find(JPAFRHeader.DepartureTimeRegisteredQuery);
			AssertEquals(2, logs.Length);
			var log1 = logs[0];
			var log2 = logs[1];
			header.CancelDepartureTimeRegistrationLog();
			AssertEquals("log1.IsCancelled", true, log1.IsCancelled);
			AssertEquals("log2.IsCancelled", true, log2.IsCancelled);
			logs = header.Logs.Find(JPAFRHeader.DepartureTimeRegisteredQuery);
			AssertEquals(0, logs.Length);
		}

		public void TestMessageStatusDesc()
		{
			var header = Factory.New<JPAFRHeader>();
			AssertEquals(ZString.Empty, header.JPH_MessageStatusDescription);

			header.JPH_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillRegistrationCompletion;
			AssertEquals(MessageStatusList.Descriptions.AwaitingHouseBillRegistrationCompletion, header.JPH_MessageStatusDescription);
		}

		public void TestAreAllBillsRegistered()
		{
			var header = Factory.New<JPAFRHeader>();
			AssertEquals(false, header.AreAllBillsRegistered);
			var bill1 = header.Bills.AddNew();
			AssertEquals(false, header.AreAllBillsRegistered);
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.DoNotLoad;
			AssertEquals(true, header.AreAllBillsRegistered);
			var bill2 = header.Bills.AddNew();
			AssertEquals(false, header.AreAllBillsRegistered);
			bill2.JPB_ReleaseStatus = ZString.Empty;
			AssertEquals(false, header.AreAllBillsRegistered);
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.NotRegistered;
			AssertEquals(false, header.AreAllBillsRegistered);
			bill2.JPB_ReleaseStatus = "@!";
			AssertEquals(false, header.AreAllBillsRegistered);
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			AssertEquals(true, header.AreAllBillsRegistered);
			bill2.Delete();
			AssertEquals(true, header.AreAllBillsRegistered);
		}

		public void TestRegistryHelper()
		{
			var company2 = Factory.New<GlbCompany>();
			var branch2 = company2.Branches.AddNew();

			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			AssertEquals(GlbCompany.CurrentCompany.PK, header.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK, header.RegistryBranchPK);

			header.JPH_GB_Branch = branch2.PK;
			AssertEquals(company2.PK, header.RegistryCompanyPK);
			AssertEquals(branch2.PK, header.RegistryBranchPK);
		}

		public void TestHumanReadableName()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C0324232";
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			AssertEquals("Advance Filing Rules", header.HumanReadableName);
			header.JPH_MasterBillNumber = "BOL2131";
			AssertEquals("Advance Filing Rules (MBOL: BOL2131)", header.HumanReadableName);
			Factory.Save();
			AssertEquals("Advance Filing Rules (JOB: C0324232 MBOL: BOL2131)", header.HumanReadableName);
		}

		public void TestJobReferenceIsAssignedFromConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			Factory.Save();
			AssertEquals(consol.JK_UniqueConsignRef, header.JPH_JobReference);
		}

		public void TestGenerateJobReference()
		{
			Env.NumberFountains.JPAFRJobReference.SetNext(Factory, 999);
			var header = Factory.New<JPAFRHeader>();
			Factory.Save();
			AssertEquals("Job reference", "AFR00000999", header.JPH_JobReference);
		}

		public void TestSetDefaults()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			AssertEquals(GlbBranch.CurrentBranch.PK, header.JPH_GB_Branch);
			AssertEquals(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK), header.Company);
		}

		public void TestDocAddresses()
		{
			var header = Factory.New<JPAFRHeader>();
			var carrier = header.Carrier;
			AssertEquals(DocAddressType.Carrier, carrier.DocAddressType);
			carrier.E2_AddressOverride = ZBool.True;
			AssertEquals(1, header.DocAddresses.Count);
			AssertCollectionContains(carrier, header.DocAddresses);
		}

		public void TestJPH_BillMessageStatus()
		{
			var header = Factory.New<JPAFRHeader>();
			AssertEquals("", header.JPH_BillMessageStatus);
			AssertEquals("", header.JPH_BillMessageStatusDescription);
			var bill1 = header.Bills.AddNew();
			bill1.JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillRegistration;
			AssertEquals(MessageStatusList.Codes.AwaitingHouseBillRegistration, header.JPH_BillMessageStatus);
			AssertEquals(MessageStatusList.Descriptions.AwaitingHouseBillRegistration, header.JPH_BillMessageStatusDescription);
			var bill2 = header.Bills.AddNew();
			AssertEquals(MessageStatusList.Codes.AwaitingHouseBillRegistration + ",No Status", header.JPH_BillMessageStatus);
			AssertEquals(MessageStatusList.Descriptions.AwaitingHouseBillRegistration + "; also there is a bill without any status.", header.JPH_BillMessageStatusDescription);
			bill2.JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillRegistration;
			AssertEquals(MessageStatusList.Codes.AwaitingHouseBillRegistration, header.JPH_BillMessageStatus);
			AssertEquals(MessageStatusList.Descriptions.AwaitingHouseBillRegistration, header.JPH_BillMessageStatusDescription);
			var bill3 = header.Bills.AddNew();
			AssertEquals(MessageStatusList.Codes.AwaitingHouseBillRegistration + ",No Status", header.JPH_BillMessageStatus);
			AssertEquals(MessageStatusList.Descriptions.AwaitingHouseBillRegistration + "; also there is a bill without any status.", header.JPH_BillMessageStatusDescription);
			bill3.JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillRegistration;
			AssertEquals(MessageStatusList.Codes.AwaitingHouseBillRegistration, header.JPH_BillMessageStatus);
			AssertEquals(MessageStatusList.Descriptions.AwaitingHouseBillRegistration, header.JPH_BillMessageStatusDescription);
			bill3.JPB_MessageStatus = MessageStatusList.Codes.ClearHouseBillRegistration;
			AssertEquals(AFRStatusHelper.MultipleStatusCode, header.JPH_BillMessageStatus);
			AssertEquals(JPAFRHeader.MultipleBillsWithDifferentStatuses, header.JPH_BillMessageStatusDescription);
			bill2.JPB_MessageStatus = ZString.Empty;
			AssertEquals(AFRStatusHelper.MultipleStatusCode + ",No Status", header.JPH_BillMessageStatus);
			AssertEquals(JPAFRHeader.MultipleBillsWithDifferentStatuses + "; also there is a bill without any status.", header.JPH_BillMessageStatusDescription);
		}

		public void TestJPH_BillReleaseStatus()
		{
			var header = Factory.New<JPAFRHeader>();
			AssertEquals("", header.JPH_BillReleaseStatus);
			AssertEquals("", header.JPH_BillReleaseStatusDescription);
			var bill1 = header.Bills.AddNew();
			AssertEquals(AFRBillCustomsStatusList.Codes.NotRegistered, header.JPH_BillReleaseStatus);
			AssertEquals(AFRBillCustomsStatusList.Descriptions.NotRegistered, header.JPH_BillReleaseStatusDescription);
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.NotRegistered;
			AssertEquals(AFRBillCustomsStatusList.Codes.NotRegistered, header.JPH_BillReleaseStatus);
			AssertEquals(AFRBillCustomsStatusList.Descriptions.NotRegistered, header.JPH_BillReleaseStatusDescription);

			var bill2 = header.Bills.AddNew();
			AssertEquals(AFRBillCustomsStatusList.Codes.NotRegistered, header.JPH_BillReleaseStatus);
			AssertEquals(AFRBillCustomsStatusList.Descriptions.NotRegistered, header.JPH_BillReleaseStatusDescription);

			bill1.JPB_ReleaseStatus = ZString.Empty;
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.NotRegistered;
			AssertEquals(AFRBillCustomsStatusList.Codes.NotRegistered, header.JPH_BillReleaseStatus);
			AssertEquals(AFRBillCustomsStatusList.Descriptions.NotRegistered, header.JPH_BillReleaseStatusDescription);

			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.NotRegistered;
			bill2.JPB_ReleaseStatus = ZString.Empty;
			AssertEquals(AFRBillCustomsStatusList.Codes.NotRegistered, header.JPH_BillReleaseStatus);
			AssertEquals(AFRBillCustomsStatusList.Descriptions.NotRegistered, header.JPH_BillReleaseStatusDescription);

			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.HLD;
			AssertEquals(AFRStatusHelper.MultipleStatusCode, header.JPH_BillReleaseStatus);
			AssertEquals(JPAFRHeader.MultipleBillsWithDifferentStatuses, header.JPH_BillReleaseStatusDescription);

			var bill3 = header.Bills.AddNew();
			AssertEquals(AFRStatusHelper.MultipleStatusCode, header.JPH_BillReleaseStatus);
			AssertEquals(JPAFRHeader.MultipleBillsWithDifferentStatuses, header.JPH_BillReleaseStatusDescription);
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.NotRegistered;
			AssertEquals(AFRBillCustomsStatusList.Codes.NotRegistered, header.JPH_BillReleaseStatus);
			AssertEquals(AFRBillCustomsStatusList.Descriptions.NotRegistered, header.JPH_BillReleaseStatusDescription);
		}

		public void TestSynchronisation_NVOCC()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "JPTKY";
			var header = Factory.New<JPAFRHeader>();
			AssertEquals(false, header.ShouldSynchroniseWithConsol);
			AssertExceptionThrown(typeof(NotSupportedException), "You can't synchronise when you don't have a consol for this NVOCC job.", delegate
			{ var a = header.Synchroniser; });

			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			AssertEquals(true, header.ShouldSynchroniseWithConsol);
			var synchroniser = header.Synchroniser;
			AssertNotNull(synchroniser);
			Assert(synchroniser is JPAFRHeaderConsolSynchroniser);
			synchroniser.Synchronise(true);
			AssertEquals("JPTKY", header.JPH_RL_NKDischarge);

			header.JPH_OverrideFreightDefaults = true;
			AssertEquals(false, header.ShouldSynchroniseWithConsol);
			AssertEquals(false, synchroniser.IsEnabled);
			AssertEquals("JPTKY", header.JPH_RL_NKDischarge);

			header.JPH_RL_NKDischarge = "JPHAO";
			header.JPH_OverrideFreightDefaults = false;
			AssertEquals(true, header.ShouldSynchroniseWithConsol);
			AssertEquals(true, header.Synchroniser.IsEnabled);
			AssertEquals("JPTKY", header.JPH_RL_NKDischarge);

			var consolSynchonisation = (Integration.Customs.JP.AFR.IJPAFRHeaderWithConsolSynchonisation)header;
			header.JPH_RL_NKDischarge = "JPHAO";
			consolSynchonisation.SynchroniseWithConsolIfNeeded();
			AssertEquals(true, header.ShouldSynchroniseWithConsol);
			AssertEquals(true, header.Synchroniser.IsEnabled);
			AssertEquals("JPTKY", header.JPH_RL_NKDischarge);

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.XDC;
			interchange.EI_From = "ENT";
			interchange.EI_To = Enterprise.Customs.JP.AFR.Business.Constants.JapanCustomsReceipientID;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.eHubQueued;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			interchange.EI_GB = header.JPH_GB_Branch;
			var message = interchange.ContainedMessages.AddNew(ObjectFactory.GetType<IXmlEDIMessage>());
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			message.EM_Status = EDIMessageStatusList.Codes.Sent;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message.EM_MessageText = interchange.EI_BodyText;
			message.EM_MessageOwner = MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse;
			message.EM_GB = header.JPH_GB_Branch;
			header.Messages.Add(message);
			AssertEquals(false, header.ShouldSynchroniseWithConsol);
			synchroniser = header.Synchroniser;
			AssertEquals(false, synchroniser.IsEnabled);

			consol.JK_RL_NKDischargePort = "JPHAO";
			AssertEquals("JPTKY", header.JPH_RL_NKDischarge);

			header.JPH_OverrideFreightDefaults = true;
			AssertEquals(false, header.ShouldSynchroniseWithConsol);
			AssertEquals(false, synchroniser.IsEnabled);
			AssertEquals("JPTKY", header.JPH_RL_NKDischarge);

			cancelOverrideFreightDefaultsForTesting = true;
			header.OnOverrideFreightDefaultsChanging += new CancelEventHandler(header_OnOverrideFreightDefaultsChanging);
			header.JPH_OverrideFreightDefaults = false;
			AssertEquals(true, header.JPH_OverrideFreightDefaults);
			AssertEquals(false, header.ShouldSynchroniseWithConsol);
			AssertEquals("JPTKY", header.JPH_RL_NKDischarge);

			cancelOverrideFreightDefaultsForTesting = false;
			header.JPH_OverrideFreightDefaults = false;
			AssertEquals(false, header.JPH_OverrideFreightDefaults);
			AssertEquals(false, header.ShouldSynchroniseWithConsol);
			AssertEquals("JPHAO", header.JPH_RL_NKDischarge);
		}

		public void TestSynchronisation_VOCC()
		{
			var testJobSailing = Factory.New<JobSailing>();
			var testJobVoyage = Factory.New<JobVoyage>();
			testJobVoyage.JV_VoyageFlight = "009N";
			var testJobVoyOrigin = Factory.New<VoyageOrigin>();
			testJobVoyOrigin.JA_JV = testJobVoyage.PK;
			testJobSailing.JX_JA = testJobVoyOrigin.PK;

			var header = Factory.New<JPAFRHeader>();
			header.JPH_IsShippingLineEntry = true;
			AssertEquals(false, header.ShouldSynchroniseWithConsol);
			AssertEquals(false, header.ShouldSynchroniseWithSailing);
			AssertExceptionThrown(typeof(NotSupportedException), "You can't synchronise when you don't have a sailing for this VOCC job.", delegate
			{ var a = header.Synchroniser; });

			header.JPH_ParentId = testJobSailing.PK;
			header.JPH_ParentTableCode = testJobSailing.TablePrefix;
			AssertEquals(false, header.ShouldSynchroniseWithConsol);
			AssertEquals(true, header.ShouldSynchroniseWithSailing);
			var synchroniser = header.Synchroniser;
			AssertNotNull(synchroniser);
			Assert(synchroniser is JPAFRHeaderSailingSynchroniser);
			synchroniser.Synchronise(true);
			AssertEquals("009N", header.JPH_Voyage);

			header.JPH_OverrideFreightDefaults = true;
			AssertEquals(false, header.ShouldSynchroniseWithConsol);
			AssertEquals(false, header.ShouldSynchroniseWithSailing);
			AssertEquals(false, synchroniser.IsEnabled);
			AssertEquals("009N", header.JPH_Voyage);

			header.JPH_RL_NKDischarge = "009T";
			header.JPH_OverrideFreightDefaults = false;
			AssertEquals(false, header.ShouldSynchroniseWithConsol);
			AssertEquals(true, header.ShouldSynchroniseWithSailing);
			AssertEquals(true, header.Synchroniser.IsEnabled);
			AssertEquals("009N", header.JPH_Voyage);

			header.SynchroniseIfNeeded();
			AssertEquals(false, header.ShouldSynchroniseWithConsol);
			AssertEquals(true, header.ShouldSynchroniseWithSailing);
			AssertEquals(true, header.Synchroniser.IsEnabled);
			AssertEquals("009N", header.JPH_Voyage);

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.XDC;
			interchange.EI_From = "ENT";
			interchange.EI_To = Enterprise.Customs.JP.AFR.Business.Constants.JapanCustomsReceipientID;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.eHubQueued;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			interchange.EI_GB = header.JPH_GB_Branch;
			var message = interchange.ContainedMessages.AddNew(ObjectFactory.GetType<IXmlEDIMessage>());
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			message.EM_Status = EDIMessageStatusList.Codes.Sent;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message.EM_MessageText = interchange.EI_BodyText;
			message.EM_MessageOwner = MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse;
			message.EM_GB = header.JPH_GB_Branch;
			header.Messages.Add(message);
			AssertEquals(false, header.ShouldSynchroniseWithConsol);
			AssertEquals(false, header.ShouldSynchroniseWithSailing);
			synchroniser = header.Synchroniser;
			AssertEquals(false, synchroniser.IsEnabled);

			testJobVoyage.JV_VoyageFlight = "009T";
			AssertEquals("009T", testJobSailing.JX_JV_VoyageFlight);
			AssertEquals("009N", header.JPH_Voyage);

			header.JPH_OverrideFreightDefaults = true;
			AssertEquals(false, header.ShouldSynchroniseWithConsol);
			AssertEquals(false, header.ShouldSynchroniseWithSailing);
			AssertEquals(false, synchroniser.IsEnabled);
			AssertEquals("009N", header.JPH_Voyage);

			cancelOverrideFreightDefaultsForTesting = true;
			header.OnOverrideFreightDefaultsChanging += new CancelEventHandler(header_OnOverrideFreightDefaultsChanging);
			header.JPH_OverrideFreightDefaults = false;
			AssertEquals(true, header.JPH_OverrideFreightDefaults);
			AssertEquals(false, header.ShouldSynchroniseWithConsol);
			AssertEquals(false, header.ShouldSynchroniseWithSailing);
			AssertEquals("009N", header.JPH_Voyage);

			cancelOverrideFreightDefaultsForTesting = false;
			header.JPH_OverrideFreightDefaults = false;
			AssertEquals(false, header.JPH_OverrideFreightDefaults);
			AssertEquals(false, header.ShouldSynchroniseWithConsol);
			AssertEquals(false, header.ShouldSynchroniseWithSailing);
			AssertEquals("009T", header.JPH_Voyage);

			header.ChangeSailing(ZGuid.Empty);
			AssertEquals(false, header.JPH_OverrideFreightDefaults);
			AssertEquals(false, header.ShouldSynchroniseWithConsol);
			AssertEquals(false, header.ShouldSynchroniseWithSailing);
			AssertEquals(0, header.Sailings.Count);
			AssertEquals("009T", header.JPH_Voyage);
			AssertEquals(ZGuid.Empty, header.JPH_ParentId);
			AssertEquals(string.Empty, header.JPH_ParentTableCode);
			AssertExceptionThrown(typeof(NotSupportedException), "You can't synchronise when you don't have a sailing for this VOCC job.", delegate
			{ var a = header.Synchroniser; });
		}

		public void TestSaveCarrierAddressWithChangeSailing_VOCC()
		{
			var testCarrier = Factory.NewWithValidTestData<OrgHeader>();
			var testJobSailing = Factory.NewWithValidTestData<JobSailing>();
			var testJobVoyage = Factory.NewWithValidTestData<JobVoyage>();
			testJobVoyage.JV_VoyageFlight = "009N";
			var testJobVoyOrigin = Factory.NewWithValidTestData<VoyageOrigin>();
			testJobVoyOrigin.JA_JV = testJobVoyage.PK;
			testJobSailing.JX_JA = testJobVoyOrigin.PK;
			testJobVoyage.JV_OH_Line = testCarrier.PK;

			var testCarrier2 = Factory.NewWithValidTestData<OrgHeader>();
			var testJobSailing2 = Factory.NewWithValidTestData<JobSailing>();
			var testJobVoyage2 = Factory.NewWithValidTestData<JobVoyage>();
			testJobVoyage2.JV_VoyageFlight = "009M";
			var testJobVoyOrigin2 = Factory.NewWithValidTestData<VoyageOrigin>();
			testJobVoyOrigin2.JA_JV = testJobVoyage2.PK;
			testJobSailing2.JX_JA = testJobVoyOrigin2.PK;
			testJobVoyage2.JV_OH_Line = testCarrier2.PK;
			Factory.Save();

			AssertNotEquals(testCarrier.MainAddress.PK, testCarrier2.MainAddress.PK);

			var header = Factory.New<JPAFRHeader>();
			header.JPH_IsShippingLineEntry = true;

			header.JPH_ParentId = testJobSailing.PK;
			header.JPH_ParentTableCode = testJobSailing.TablePrefix;
			AssertEquals(false, header.ShouldSynchroniseWithConsol);
			AssertEquals(true, header.ShouldSynchroniseWithSailing);
			var synchroniser = header.Synchroniser;
			AssertNotNull(synchroniser);
			Assert(synchroniser is JPAFRHeaderSailingSynchroniser);

			header.ChangeSailing(testJobSailing.PK);
			AssertEquals("TestSync Voyage", "009N", header.JPH_Voyage);
			AssertEquals("TestSync Carrier Address", testCarrier.MainAddress.PK, header.Carrier.E2_OA_Address);
			Assert("Header should be unsaved", !header.IsInDatabase);
			Assert("Header Carrier Address should be unsaved", !header.Carrier.IsInDatabase);
			var reloadedCarrierAddress = (new BusinessObjectFactory()).Load<JobDocAddress>(header.Carrier.PK);
			AssertNull(reloadedCarrierAddress);
			Factory.Save();
			Assert("Header should be saved", header.IsInDatabase);
			Assert("Header Carrier Address should be saved", header.Carrier.IsInDatabase);
			reloadedCarrierAddress = (new BusinessObjectFactory()).Load<JobDocAddress>(header.Carrier.PK);
			AssertNotNull(reloadedCarrierAddress);
			AssertEquals(testCarrier.MainAddress.PK, reloadedCarrierAddress.E2_OA_Address);

			header.ChangeSailing(testJobSailing2.PK);
			AssertEquals("TestSync Voyage", "009M", header.JPH_Voyage);
			AssertEquals("TestSync Carrier Address", testCarrier2.MainAddress.PK, header.Carrier.E2_OA_Address);
			reloadedCarrierAddress = (new BusinessObjectFactory()).Load<JobDocAddress>(header.Carrier.PK);
			AssertNotNull(reloadedCarrierAddress);
			AssertEquals(testCarrier.MainAddress.PK, reloadedCarrierAddress.E2_OA_Address);
			Factory.Save();
			Assert("Header should be saved", header.IsInDatabase);
			Assert("Header Carrier Address should be saved", header.Carrier.IsInDatabase);
			reloadedCarrierAddress = (new BusinessObjectFactory()).Load<JobDocAddress>(header.Carrier.PK);
			AssertNotNull(reloadedCarrierAddress);
			AssertEquals(testCarrier2.MainAddress.PK, reloadedCarrierAddress.E2_OA_Address);

			header.ChangeSailing(ZGuid.Empty);
			AssertEquals("TestSync Voyage", "009M", header.JPH_Voyage);
			AssertEquals("TestSync Carrier Address", testCarrier2.MainAddress.PK, header.Carrier.E2_OA_Address);
			reloadedCarrierAddress = (new BusinessObjectFactory()).Load<JobDocAddress>(header.Carrier.PK);
			AssertNotNull(reloadedCarrierAddress);
			AssertEquals(testCarrier2.MainAddress.PK, reloadedCarrierAddress.E2_OA_Address);
			Factory.Save();
			Assert("Header should be saved", header.IsInDatabase);
			Assert("Header Carrier Address should be saved", header.Carrier.IsInDatabase);
			reloadedCarrierAddress = (new BusinessObjectFactory()).Load<JobDocAddress>(header.Carrier.PK);
			AssertNotNull(reloadedCarrierAddress);
			AssertEquals(testCarrier2.MainAddress.PK, reloadedCarrierAddress.E2_OA_Address);
		}

		public void TestChangeSailing_VOCC()
		{
			var testCarrier = Factory.NewWithValidTestData<OrgHeader>();
			var testJobSailing = Factory.NewWithValidTestData<JobSailing>();
			var testJobVoyage = Factory.NewWithValidTestData<JobVoyage>();
			testJobVoyage.JV_VoyageFlight = "009N";
			var testJobVoyOrigin = Factory.NewWithValidTestData<VoyageOrigin>();
			testJobVoyOrigin.JA_JV = testJobVoyage.PK;
			testJobSailing.JX_JA = testJobVoyOrigin.PK;
			testJobVoyage.JV_OH_Line = testCarrier.PK;

			var testCarrier2 = Factory.NewWithValidTestData<OrgHeader>();
			var testJobSailing2 = Factory.NewWithValidTestData<JobSailing>();
			var testJobVoyage2 = Factory.NewWithValidTestData<JobVoyage>();
			testJobVoyage2.JV_VoyageFlight = "009M";
			var testJobVoyOrigin2 = Factory.NewWithValidTestData<VoyageOrigin>();
			testJobVoyOrigin2.JA_JV = testJobVoyage2.PK;
			testJobSailing2.JX_JA = testJobVoyOrigin2.PK;
			testJobVoyage2.JV_OH_Line = testCarrier2.PK;
			Factory.Save();

			AssertNotEquals(testCarrier.MainAddress.PK, testCarrier2.MainAddress.PK);

			var header = Factory.New<JPAFRHeader>();
			header.JPH_IsShippingLineEntry = true;
			Factory.Save();
			AssertEquals(string.Empty, header.JPH_Voyage);

			header.ChangeSailing(testJobSailing.PK);
			AssertEquals("1st Sync Voyage", "009N", header.JPH_Voyage);
			Factory.Save();
			var reloadedHeader = (new BusinessObjectFactory()).Load<JPAFRHeader>(header.PK);
			AssertEquals("1st Result should be saved", "009N", reloadedHeader.JPH_Voyage);
			AssertEquals("1st Result should be saved", testJobSailing.PK, reloadedHeader.JPH_ParentId);
			AssertEquals("1st Result should be saved", testJobSailing.TablePrefix, reloadedHeader.JPH_ParentTableCode);

			header.ChangeSailing(testJobSailing2.PK);
			AssertEquals("2nd Sync Voyage", "009M", header.JPH_Voyage);
			Factory.Save();
			reloadedHeader = (new BusinessObjectFactory()).Load<JPAFRHeader>(header.PK);
			AssertEquals("2nd Result should be saved", "009M", reloadedHeader.JPH_Voyage);
			AssertEquals("2nd Result should be saved", testJobSailing2.PK, reloadedHeader.JPH_ParentId);
			AssertEquals("2nd Result should be saved", testJobSailing2.TablePrefix, reloadedHeader.JPH_ParentTableCode);

			header.ChangeSailing(ZGuid.Empty);
			AssertEquals("2nd Sync Voyage", "009M", header.JPH_Voyage);
			Factory.Save();
			reloadedHeader = (new BusinessObjectFactory()).Load<JPAFRHeader>(header.PK);
			AssertEquals("2nd Result should be saved", "009M", reloadedHeader.JPH_Voyage);
			AssertEquals("2nd Result should be saved", ZGuid.Empty, reloadedHeader.JPH_ParentId);
			AssertEquals("2nd Result should be saved", string.Empty, reloadedHeader.JPH_ParentTableCode);
		}

		public void TestSynchronisationViaDataRefreshBus_NVOCC()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT1";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "ABCDHB1";
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, container);
			packLine.JL_HarmonisedCode = "1010.10.10";
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var synchroniser = header.Synchroniser;
			AssertNotNull(synchroniser);
			synchroniser.Synchronise(true);
			AssertEquals(1, header.Bills.Count);
			var bill = header.Bills[0];
			AssertEquals("ABCDHB1", bill.JPB_BillNumber);
			AssertEquals(1, bill.Containers.Count);
			var billContainer = bill.Containers[0];
			AssertEquals("CONT1", billContainer.JPC_ContainerNum);
			AssertEquals("101010", bill.JPB_Tariff);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var containerInOtherFactory = newFactory.Load<ForwardingContainer>(container.PK);
			AssertEquals("CONT1", containerInOtherFactory.JC_ContainerNum);
			containerInOtherFactory.JC_ContainerNum = "CONT2";
			var shipmentInOtherFactory = newFactory.Load<ForwardingShipment>(shipment.PK);
			AssertEquals("ABCDHB1", shipmentInOtherFactory.JS_HouseBill);
			shipmentInOtherFactory.JS_HouseBill = "EFGHHB2";
			var packLineInOtherFactory = newFactory.Load<ForwardingPackLine>(packLine.PK);
			AssertEquals("1010.10.10", packLineInOtherFactory.JL_HarmonisedCode);
			packLineInOtherFactory.JL_HarmonisedCode = "2020.20.10";
			newFactory.Save();
			AssertEquals("EFGHHB2", shipment.JS_HouseBill);
			AssertEquals(1, header.Bills.Count);
			AssertEquals(false, bill.IsDeleted);
			AssertEquals(bill, header.Bills["EFGHHB2"]);

			AssertEquals("CONT2", container.JC_ContainerNum);
			AssertEquals(1, bill.Containers.Count);
			AssertEquals(false, billContainer.IsDeleted);
			AssertEquals(billContainer, bill.Containers["CONT2"]);

			AssertEquals("2020.20.10", packLine.JL_HarmonisedCode);
			AssertEquals("202020", bill.JPB_Tariff);

			header.JPH_OverrideFreightDefaults = true;
			Factory.Save();

			containerInOtherFactory.JC_ContainerNum = "CONT3";
			shipmentInOtherFactory.JS_HouseBill = "IJKLHB3";
			packLineInOtherFactory.JL_HarmonisedCode = "3020.10.10";
			newFactory.Save();

			AssertEquals("IJKLHB3", shipment.JS_HouseBill);
			AssertEquals(1, header.Bills.Count);
			AssertEquals(false, bill.IsDeleted);
			AssertEquals(bill, header.Bills["EFGHHB2"]);

			AssertEquals("CONT3", container.JC_ContainerNum);
			AssertEquals(1, bill.Containers.Count);
			AssertEquals(false, billContainer.IsDeleted);
			AssertEquals(billContainer, bill.Containers["CONT2"]);

			AssertEquals("3020.10.10", packLine.JL_HarmonisedCode);
			AssertEquals("202020", bill.JPB_Tariff);

			header.JPH_OverrideFreightDefaults = false;
			AssertEquals("IJKLHB3", shipment.JS_HouseBill);
			AssertEquals(1, header.Bills.Count);
			AssertEquals(true, bill.IsDeleted);
			bill = header.Bills["IJKLHB3"];
			AssertNotNull(bill);

			AssertEquals("CONT3", container.JC_ContainerNum);
			AssertEquals(1, bill.Containers.Count);
			AssertEquals(true, billContainer.IsDeleted);
			billContainer = bill.Containers["CONT3"];
			AssertNotNull(billContainer);

			AssertEquals("3020.10.10", packLine.JL_HarmonisedCode);
			AssertEquals("302010", bill.JPB_Tariff);
		}

		public void TestParentWorkflowProviders()
		{
			var header = Factory.New<JPAFRHeader>();
			var prov = header.ParentWorkflowProviders;
			AssertNotNull(prov);
			AssertEquals(0, prov.Count);

			var consol = Factory.New<ForwardingConsol>();

			header.JPH_ParentTableCode = "JK";
			header.JPH_ParentId = consol.PK;
			prov = header.ParentWorkflowProviders;
			AssertNotNull(prov);
			AssertEquals(1, prov.Count);
			AssertEquals(consol, prov[0]);
		}

		public void TestHasVesselInformationChanged()
		{
			var testHeader = Factory.NewWithValidTestData<JPAFRHeader>();
			CombineAssertions(() =>
			{
				AssertEquals(false, testHeader.HasVesselInformationChanged);
				testHeader.JPH_Voyage = "VT1";
				AssertEquals("unsaved - JPH_Voyage", false, testHeader.HasVesselInformationChanged);
				testHeader.JPH_CarrierCode = "CT1";
				AssertEquals("unsaved - JPH_CarrierCode", false, testHeader.HasVesselInformationChanged);
				testHeader.JPH_VesselName = "VC1";
				AssertEquals("unsaved - JPH_VesselName", false, testHeader.HasVesselInformationChanged);
				testHeader.JPH_RL_NKLoading = "LP1";
				AssertEquals("unsaved - JPH_RL_NKLoading", false, testHeader.HasVesselInformationChanged);
				testHeader.JPH_LoadingPortSuffix = "1";
				AssertEquals("unsaved - JPH_LoadingPortSuffix", false, testHeader.HasVesselInformationChanged);
			});

			Factory.Save();
			CombineAssertions(() =>
			{
				testHeader.JPH_Voyage = "VT1";
				AssertEquals("Saved - JPH_Voyage", false, testHeader.HasVesselInformationChanged);
				testHeader.JPH_Voyage = "VT2";
				AssertEquals("Saved - JPH_Voyage", true, testHeader.HasVesselInformationChanged);
				testHeader.JPH_Voyage = "VT1";
				AssertEquals("Saved - JPH_Voyage", false, testHeader.HasVesselInformationChanged);

				testHeader.JPH_CarrierCode = "CT1";
				AssertEquals("Saved - JPH_CarrierCode", false, testHeader.HasVesselInformationChanged);
				testHeader.JPH_CarrierCode = "CT2";
				AssertEquals("Saved - JPH_CarrierCode", true, testHeader.HasVesselInformationChanged);
				testHeader.JPH_CarrierCode = "CT1";
				AssertEquals("Saved - JPH_CarrierCode", false, testHeader.HasVesselInformationChanged);

				testHeader.JPH_VesselName = "VC1";
				AssertEquals("Saved - JPH_VesselName", false, testHeader.HasVesselInformationChanged);
				testHeader.JPH_VesselName = "VC2";
				AssertEquals("Saved - JPH_VesselName", true, testHeader.HasVesselInformationChanged);
				testHeader.JPH_VesselName = "VC1";
				AssertEquals("Saved - JPH_VesselName", false, testHeader.HasVesselInformationChanged);

				testHeader.JPH_RL_NKLoading = "LP1";
				AssertEquals("Saved - JPH_RL_NKLoading", false, testHeader.HasVesselInformationChanged);
				testHeader.JPH_RL_NKLoading = "LP2";
				AssertEquals("Saved - JPH_RL_NKLoading", true, testHeader.HasVesselInformationChanged);
				testHeader.JPH_RL_NKLoading = "LP1";
				AssertEquals("Saved - JPH_RL_NKLoading", false, testHeader.HasVesselInformationChanged);

				testHeader.JPH_LoadingPortSuffix = "1";
				AssertEquals("Saved - JPH_LoadingPortSuffix", false, testHeader.HasVesselInformationChanged);
				testHeader.JPH_LoadingPortSuffix = "2";
				AssertEquals("Saved - JPH_LoadingPortSuffix", true, testHeader.HasVesselInformationChanged);
				testHeader.JPH_LoadingPortSuffix = "1";
				AssertEquals("Saved - JPH_LoadingPortSuffix", false, testHeader.HasVesselInformationChanged);
			});
		}

		public void TestHasATDInformationChanged()
		{
			var testHeader = Factory.NewWithValidTestData<JPAFRHeader>();
			CombineAssertions(() =>
			{
				testHeader.JPH_RelaxedAppId = true;
				AssertEquals("unsaved - JPH_RelaxedAppId", false, testHeader.HasATDInformationChanged);
				testHeader.JPH_RL_NKDischarge = "1";
				AssertEquals("unsaved - JPH_RL_NKDischarge", false, testHeader.HasATDInformationChanged);
				testHeader.JPH_ETD = new ZDateTime("2013-01-01");
				AssertEquals("unsaved - JPH_ETD", false, testHeader.HasATDInformationChanged);
				testHeader.JPH_ETA = new ZDateTime("2013-01-01");
				AssertEquals("unsaved - JPH_ETA", false, testHeader.HasATDInformationChanged);
			});

			Factory.Save();
			CombineAssertions(() =>
			{
				testHeader.JPH_RelaxedAppId = true;
				AssertEquals("Saved - JPH_RelaxedAppId", false, testHeader.HasATDInformationChanged);
				testHeader.JPH_RelaxedAppId = false;
				AssertEquals("Saved - JPH_RelaxedAppId", true, testHeader.HasATDInformationChanged);
				testHeader.JPH_RelaxedAppId = true;
				AssertEquals("Saved - JPH_RelaxedAppId", false, testHeader.HasATDInformationChanged);

				testHeader.JPH_RL_NKDischarge = "1";
				AssertEquals("Saved - JPH_RL_NKDischarge", false, testHeader.HasATDInformationChanged);
				testHeader.JPH_RL_NKDischarge = "2";
				AssertEquals("Saved - JPH_RL_NKDischarge", false, testHeader.HasATDInformationChanged);
				testHeader.JPH_RL_NKDischarge = "1";
				AssertEquals("Saved - JPH_RL_NKDischarge", false, testHeader.HasATDInformationChanged);

				testHeader.JPH_ETD = new ZDateTime("2013-01-01");
				AssertEquals("Saved - JPH_ETD", false, testHeader.HasATDInformationChanged);
				testHeader.JPH_ETD = new ZDateTime("2014-01-01");
				AssertEquals("Saved - JPH_ETD", true, testHeader.HasATDInformationChanged);
				testHeader.JPH_ETD = new ZDateTime("2013-01-01");
				AssertEquals("Saved - JPH_ETD", false, testHeader.HasATDInformationChanged);

				testHeader.JPH_ETA = new ZDateTime("2013-01-01");
				AssertEquals("Saved - JPH_ETA", false, testHeader.HasATDInformationChanged);
				testHeader.JPH_ETA = new ZDateTime("2014-01-01");
				AssertEquals("Saved - JPH_ETA", false, testHeader.HasATDInformationChanged);
				testHeader.JPH_ETA = new ZDateTime("2013-01-01");
				AssertEquals("Saved - JPH_ETA", false, testHeader.HasATDInformationChanged);
			});
		}

		public void TestHasMasterInformationChanged()
		{
			var testHeader = Factory.NewWithValidTestData<JPAFRHeader>();
			CombineAssertions(() =>
			{
				testHeader.JPH_RelaxedAppId = true;
				AssertEquals("unsaved - JPH_RelaxedAppId", false, testHeader.HasMasterInformationChanged);
				testHeader.JPH_RL_NKDischarge = "1";
				AssertEquals("unsaved - JPH_RL_NKDischarge", false, testHeader.HasMasterInformationChanged);
				testHeader.JPH_ETD = new ZDateTime("2013-01-01");
				AssertEquals("unsaved - JPH_ETD", false, testHeader.HasMasterInformationChanged);
				testHeader.JPH_ETA = new ZDateTime("2013-01-01");
				AssertEquals("unsaved - JPH_ETA", false, testHeader.HasMasterInformationChanged);
			});

			Factory.Save();
			CombineAssertions(() =>
			{
				testHeader.JPH_RelaxedAppId = true;
				AssertEquals("Saved - JPH_RelaxedAppId", false, testHeader.HasMasterInformationChanged);
				testHeader.JPH_RelaxedAppId = false;
				AssertEquals("Saved - JPH_RelaxedAppId", true, testHeader.HasMasterInformationChanged);
				testHeader.JPH_RelaxedAppId = true;
				AssertEquals("Saved - JPH_RelaxedAppId", false, testHeader.HasMasterInformationChanged);

				testHeader.JPH_RL_NKDischarge = "1";
				AssertEquals("Saved - JPH_RL_NKDischarge", false, testHeader.HasMasterInformationChanged);
				testHeader.JPH_RL_NKDischarge = "2";
				AssertEquals("Saved - JPH_RL_NKDischarge", true, testHeader.HasMasterInformationChanged);
				testHeader.JPH_RL_NKDischarge = "1";
				AssertEquals("Saved - JPH_RL_NKDischarge", false, testHeader.HasMasterInformationChanged);

				testHeader.JPH_ETD = new ZDateTime("2013-01-01");
				AssertEquals("Saved - JPH_ETD", false, testHeader.HasMasterInformationChanged);
				testHeader.JPH_ETD = new ZDateTime("2014-01-01");
				AssertEquals("Saved - JPH_ETD", true, testHeader.HasMasterInformationChanged);
				testHeader.JPH_ETD = new ZDateTime("2013-01-01");
				AssertEquals("Saved - JPH_ETD", false, testHeader.HasMasterInformationChanged);

				testHeader.JPH_ETA = new ZDateTime("2013-01-01");
				AssertEquals("Saved - JPH_ETA", false, testHeader.HasMasterInformationChanged);
				testHeader.JPH_ETA = new ZDateTime("2014-01-01");
				AssertEquals("Saved - JPH_ETA", true, testHeader.HasMasterInformationChanged);
				testHeader.JPH_ETA = new ZDateTime("2013-01-01");
				AssertEquals("Saved - JPH_ETA", false, testHeader.HasMasterInformationChanged);
			});
		}

		public void TestDocumentSupporter()
		{
			var header = Factory.NewWithValidTestData<JPAFRHeader>();
			AssertEquals("Document support is JP type", typeof(JPAFRHeaderDocumentSupporter), header.DocumentSupporter.GetType());
		}

		public void TestOnFactorySavingBeforeTransactionCore()
		{
			var header = Factory.New<JPAFRHeader>();
			Factory.Save();
			AssertEquals("Log Count", header.Logs.Find(o => o.SL_SE_NKEvent == Events.MessageStatusChange.Code).Count(), 0);
			header.JPH_MessageStatus = "AHR";
			Factory.Save();
			var log = header.Logs.Find(o => o.SL_SE_NKEvent == Events.MessageStatusChange.Code).FirstOrDefault();
			AssertNotNull(log);
			AssertEquals("Reference", "AHR - Awaiting House Bill Registration", log.SL_Reference);
		}

		public void TestBusinessObjectsWithRelatedEvents()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			AssertCollectionContains("BusinessObjectsWithRelatedEvents should contain Bills.", bill1, header.BusinessObjectsWithRelatedEvents);
			AssertCollectionContains("BusinessObjectsWithRelatedEvents should contain Bills.", bill2, header.BusinessObjectsWithRelatedEvents);
		}

		public void TestGetNewJPAFRHeaderProcessTaskCollection()
		{
			var header = Factory.New<JPAFRHeader>();
			AssertType("WorkflowItems's type should be ProcessTaskCollection<JPAFRHeaderProcessTask, JPAFRHeader>", typeof(ProcessTaskCollection<JPAFRHeaderProcessTask, JPAFRHeader>), ((IWorkflowProvider)header).WorkflowItems);
		}

		#region helper

		void header_OnOverrideFreightDefaultsChanging(object sender, CancelEventArgs e)
		{
			e.Cancel = cancelOverrideFreightDefaultsForTesting;
		}
		bool cancelOverrideFreightDefaultsForTesting;

		#endregion
	}

	class JPAFRHeaderImportBillsFromSailingTest : JPAFRImportFromSailingTestBase
	{
		public void TestResynchroniseVesselDetails()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "VESSEL";
			vessel.RV_RadioCallSign = "12345";
			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Code = "VESSEL2";
			vessel2.RV_RadioCallSign = "54321";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "009N";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			var origin = voyage.Origins.AddNew();
			var destination = voyage.Destinations.AddNew();
			var sailing = Factory.New<JobSailing>();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			var header = Factory.New<JPAFRHeader>();
			header.JPH_IsShippingLineEntry = true;
			header.ChangeSailing(sailing.PK);
			AssertEquals("VESSEL", header.Vessel.RV_Code);
			AssertEquals("12345", header.Vessel.RV_RadioCallSign);
			AssertEquals("009N", header.JPH_Voyage);
			header.JPH_OverrideFreightDefaults = true;
			header.JPH_VesselName = "VESSEL2";
			header.JPH_Voyage = "009M";
			AssertEquals("VESSEL2", header.Vessel.RV_Code);
			AssertEquals("54321", header.Vessel.RV_RadioCallSign);
			AssertEquals("009M", header.JPH_Voyage);
		}

		public void TestNoOfBillsAndContainers()
		{
			var voyage = Factory.New<JobVoyage>();
			var origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "AUSYD";
			origin1.JA_E_DEP = ZDateTime.Today.AddDays(10);
			var destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "USLAX";
			destination1.JB_E_ARV = ZDateTime.Today.AddDays(20);
			var origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "CAAAD";
			origin2.JA_E_DEP = ZDateTime.Today.AddDays(30);
			var destination2 = voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "USCHI";
			destination2.JB_E_ARV = ZDateTime.Today.AddDays(40);

			voyage.GenerateSailings();

			var sailing = voyage.Sailings.Cast<JobSailing>().FirstOrDefault(x => x.JX_JA_RL_NKPortOfLoading == "AUSYD" && x.JX_JB_RL_NKPortOfDischarge == "USLAX");
			var sBill1 = Factory.New<BillOfLading>();
			sBill1.JS_JX = sailing.PK;
			sBill1.JS_HouseBill = "SCACAAA";
			var sBill2 = Factory.New<BillOfLading>();
			sBill2.JS_JX = sailing.PK;
			sBill2.JS_HouseBill = "SCACBBB";

			var sailing2 = voyage.Sailings.Cast<JobSailing>().FirstOrDefault(x => x.JX_JA_RL_NKPortOfLoading == "CAAAD" && x.JX_JB_RL_NKPortOfDischarge == "USCHI");
			var sBill3 = Factory.New<BillOfLading>();
			sBill3.JS_JX = sailing2.PK;
			sBill3.JS_HouseBill = "SCAZZZ";

			var header = Factory.New<JPAFRHeader>();
			header.JPH_IsShippingLineEntry = true;
			header.ChangeSailing(sailing.PK);
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "AAA";
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "BBB";
			var bill3 = header.Bills.AddNew();
			bill3.JPB_BillNumber = "CCC";

			AssertEquals(3, header.JPH_NoOfAFRBills);
			AssertEquals(2, header.JPH_NoOfSailingBills);

			header.ChangeSailing(sailing2.PK);
			AssertEquals(1, header.JPH_NoOfSailingBills);
		}

		public void TestAddAndRemoveSailing()
		{
			var sailing1 = Factory.NewWithValidTestData<JobSailing>();
			var sailing2 = Factory.NewWithValidTestData<JobSailing>();
			Factory.Save();

			var header = Factory.New<JPAFRHeader>();
			header.JPH_IsShippingLineEntry = true;
			header.ChangeSailing(sailing1.PK);
			AssertEquals(sailing1.PK, header.Sailing.PK);
			AssertEquals(1, header.Sailings.Count);

			header.ChangeSailing(sailing2.PK);
			AssertEquals(sailing2.PK, header.Sailing.PK);
			AssertEquals(1, header.Sailings.Count);

			header.ChangeSailing(ZGuid.Empty);
			AssertNull(header.Sailing);
			AssertEquals(0, header.Sailings.Count);
		}

		public void TestISailingParentFindBoxMembers()
		{
			var sailing = Factory.NewWithValidTestData<JobSailing>();
			Factory.Save();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_RL_NKDischarge = "USCHI";
			header.JPH_OverrideFreightDefaults = true;
			header.ChangeSailing(sailing.PK);

			ISailingParentFindBox sailingParent = header;
			AssertEquals("USCHI", sailingParent.DischargePort);
			AssertEquals(ZString.Empty, sailingParent.LoadPort);
			AssertEquals(ZString.Empty, sailingParent.Origin);
			AssertEquals(sailing.PK, sailingParent.SailingPK);
			AssertEquals(Core.Constants.TransportModes.Sea, sailingParent.TransportMode);
		}

		public void TestImportBillsOfLadingLinkedToTheSailling()
		{
			SetUpSailingEnvironmentForImporting();
			SetUpHeaderEnvironmentForImporting();
			header.ImportBillsOfLadingLinkedToTheSameSailing(new BillImportActionCollection(header.Bills));
			var bill = header.Bills.FirstOrDefault(x => x.JPB_BillNumber == "SCACAAA");
			AssertEquals(1, header.Bills.Count);
			AssertNotNull(bill);
			AssertEquals("AUSYD", bill.JPB_RL_NKOrigin);
			AssertEquals(1000m, bill.JPB_GrossWeight);
			AssertEquals("KG", bill.JPB_GrossWeightUQ);
			AssertEquals(11, bill.JPB_ManifestQty);
			AssertEquals("PLT", bill.JPB_ManifestUQ);
			AssertEquals(consignor.PK, bill.Consignor.OrganisationPK);
			AssertEquals(consignee.PK, bill.Consignee.OrganisationPK);
			AssertEquals(consignee.PK, bill.NotifyParty1.OrganisationPK);
			AssertEquals(notifyParty.PK, bill.NotifyParty2.OrganisationPK);

			AssertNull(header.Bills.FirstOrDefault(x => x.JPB_BillNumber == "SCACBBB"));
			AssertNull(header.Bills.FirstOrDefault(x => x.JPB_BillNumber == "SCACCCC"));
		}
	}

	[TestedType(typeof(JPAFRHeader))]
	class JPAFRHeaderWorkflowProviderTestCase : WorkflowProviderTest<JPAFRHeader, ProcessTaskCollection<JPAFRHeaderProcessTask, JPAFRHeader>>
	{
		JPAFRHeader Header
		{
			get { return BusinessObject; }
		}

		public void TestGetTemplateFilterCriteria_ForLoadPort()
		{
			Header.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(Header.JPH_RL_NKLoadingInfo, ProcessTaskTemplate.P0_LoadPortCountryInfo, "AUSYD", "MYPKG", ZString.Empty);
		}

		public void TestGetTemplateFilterCriteria_ForDischargePort()
		{
			Header.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(Header.JPH_RL_NKDischargeInfo, ProcessTaskTemplate.P0_DischargePortCountryInfo, "AUSYD", "MYPKG", ZString.Empty);
		}

		public void TestGetTemplateFilterCriteria_ForBranch()
		{
			Header.Factory.Save();
			AssertGetTemplateFilterCriteria(Header.JPH_GB_BranchInfo, ProcessTaskTemplate.P0_GBInfo, Branch.PK, Branch2.PK, ZGuid.Empty);
		}

		public void TestGetTemplateFilterCriteria_ForClient()
		{
			Header.Factory.Save();
			AssertGetTemplateFilterCriteria(Header.Carrier.OrganisationPKInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);
		}

		protected override ZString ExpectedWorkflowType
		{
			get { return JPAFRWorkflowDescriptor.Constants.Code; }
		}
	}

	class JPAFRHeaderJobReferenceNoDuplicationTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestJPAFRHeaderJobReferenceNoDuplication()
		{
			var factory1 = new BusinessObjectFactory();
			var job1 = factory1.New<JPAFRHeader>();
			var messageReference1 = ZString.Empty;

			var connection1 = ((CargoWise.Data.IDbConnected)factory1).Connection;
			try
			{
				connection1.BeginTransaction();
				job1.OnSaving();
				messageReference1 = job1.JPH_JobReference;
			}
			finally
			{
				connection1.RollbackTransaction();
			}

			var factory2 = new BusinessObjectFactory();
			var job2 = factory2.New<JPAFRHeader>();
			factory2.Save();
			AssertEquals("Should return the former reference number for connection1 rolled back.", messageReference1, job2.JPH_JobReference);
			AssertEquals("JPH_JobReference should remain for factory not saved.", messageReference1, job1.JPH_JobReference);

			factory1.Save();
			AssertNotEquals("Should get next reference number from number fountain.", messageReference1, job1.JPH_JobReference);
		}
	}

	internal class JPAFRImportFromSailingTestBase : TestCaseWithFactory
	{
		protected JPAFRHeader header;
		JobVoyage voyage;
		JobSailing sailing;
		BillOfLading fclSailingBill;
		BillOfLading roroSailingBill;
		BillOfLading bulkSailingBill;
		protected OrgHeader consignor;
		protected OrgHeader consignee;
		protected OrgHeader notifyParty;
		protected RefContainer refContainer;
		BillOfLadingContainer sailingContainer;
		AgencyShipmentContainer vehicle;
		AgencyShipmentContainer topPack;
		OrgContact contact;
		BillOfLadingPackLine sailingPackLine;

		protected void SetUpSailingEnvironmentForImporting()
		{
			var exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
			exchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-10);
			exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(10);
			exchangeRate.RE_ExRateType = Enterprise.Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			exchangeRate.RE_SellRate = 0.5m;

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "AALSMEERGRACHT";

			voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "123SD";
			voyage.JV_RV_NKVessel = vessel.RV_FK;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = new ZDateTime(2006, 4, 10);
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			destination.JB_E_ARV = new ZDateTime(2006, 4, 20);

			voyage.GenerateSailings();
			sailing = voyage.Sailings[0];

			consignor = Factory.New<OrgHeader>();
			consignee = Factory.New<OrgHeader>();
			notifyParty = Factory.New<OrgHeader>();

			fclSailingBill = Factory.New<BillOfLading>();
			fclSailingBill.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			fclSailingBill.JS_NKLoadPort = "AUSYD";
			fclSailingBill.JS_JX = sailing.PK;
			fclSailingBill.JS_HouseBill = "SCACAAA";
			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "SCAC", Core.Constants.CountryCodes.UnitedStates);
			fclSailingBill.JS_OA_BookedShippingLineAddress = shippingLine.MainAddress.PK;
			fclSailingBill.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			fclSailingBill.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			fclSailingBill.NotifyParty2DocumentaryAddress.OrganisationPK = notifyParty.PK;
			fclSailingBill.JS_GoodsDescription = "VICGOODSDESCRIPTION";
			fclSailingBill.JS_MarksAndNumbers = @"TESTMARK01
TESTMARK02
TESTMARK03
TESTMARK04
TESTMARK05
TESTMARK06
TESTMARK07
TESTMARK08
TESTMARK09
TESTMARK10
TESTMARK11
TESTMARK12
TESTMARK13
TESTMARK14
TESTMARK15";

			refContainer = Factory.New<RefContainer>();

			sailingContainer = fclSailingBill.RealContainers.AddNew();
			sailingContainer.JC_ContainerNum = "ABCD1111";
			sailingContainer.JC_RC = refContainer.PK;
			sailingContainer.JC_SealNum = "123";
			sailingContainer.JC_AdditionalSealNum = "32112321233213432145";

			contact = Factory.New<OrgContact>();

			sailingPackLine = fclSailingBill.OuterPackLines.AddNew();
			sailingContainer.PackLines.Add(sailingPackLine);
			sailingPackLine.JL_HarmonisedCode = "01011000";
			sailingPackLine.JL_ActualWeight = 1000m;
			sailingPackLine.JL_ActualWeightUQ = "KG";
			sailingPackLine.JL_F3_NKPackType = "PLT";
			sailingPackLine.JL_ActualVolume = 120000;
			sailingPackLine.JL_ActualVolumeUQ = "CI";
			sailingPackLine.JL_PackageCount = 11;
			sailingPackLine.JL_DetailedDescription = "DESCRIPTION";
			sailingPackLine.JL_MarksAndNumbers = "MARKS AND NUMBERS";
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "!!5%";
			subs.DG_Variant = "";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var subs2 = Factory.New<UNDGSubstance>();
			subs2.DG_UNNO = "!!6%";
			subs2.DG_Variant = "";
			subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undg = sailingPackLine.UNDGs.AddNew();
			undg.DI_DG = subs.PK;
			undg.LinkDefault(subs);
			undg.DI_DGFlashPoint = 1.2m;
			undg.DI_OC_DGContact = contact.PK;
			undg = sailingPackLine.UNDGs.AddNew();
			undg.DI_DG = subs2.PK;
			undg.LinkDefault(subs2);
			undg.DI_DGFlashPoint = 1.2m;
			undg.DI_OC_DGContact = contact.PK;

			roroSailingBill = Factory.New<BillOfLading>();
			roroSailingBill.JS_PackingMode = Core.Constants.ContainerModes.RollOnRollOff;
			vehicle = roroSailingBill.Vehicles.AddNew();
			roroSailingBill.JS_NKLoadPort = "AUSYD";
			roroSailingBill.JS_JX = sailing.PK;
			roroSailingBill.JS_HouseBill = "BBB";
			roroSailingBill.JS_OA_BookedShippingLineAddress = shippingLine.MainAddress.PK;
			roroSailingBill.JS_ActualWeight = 1m;
			roroSailingBill.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			roroSailingBill.JS_ActualVolume = 2m;
			roroSailingBill.JS_UnitOfVolume = Core.Constants.Volume.CubicFeet;
			roroSailingBill.JS_OuterPacks = 3;
			roroSailingBill.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			roroSailingBill.JS_MarksAndNumbers = @"MARKTEST01
MARKTEST02
MARKTEST03
MARKTEST04
MARKTEST05
MARKTEST06
MARKTEST07
MARKTEST08
MARKTEST09
MARKTEST10
MARKTEST11
MARKTEST12
MARKTEST13
MARKTEST14
MARKTEST15";
			vehicle.JC_HarmonisedCode = "01011001";
			vehicle.JC_GrossWeight = 1m;
			vehicle.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			vehicle.JC_ContainerCount = 2;
			vehicle.JC_Description = "VEHICLE";
			vehicle.JC_MarksAndNumbers = "VEHICLE MARKS & NUMBERS";
			vehicle.JC_ContainerNum = "VIN";
			vehicle.JC_F3_NKPackType = Core.Constants.PkgUnit.Unit;
			vehicle.JC_GoodsValue = 100m;
			vehicle.JC_RX_NKGoodsCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			undg = vehicle.UNDGs.AddNew();
			undg.LinkDefault(subs);
			undg.DI_DGFlashPoint = 1.2m;
			undg.DI_OC_DGContact = contact.PK;
			undg = vehicle.UNDGs.AddNew();
			undg.LinkDefault(subs2);
			undg.DI_DGFlashPoint = 1.2m;
			undg.DI_OC_DGContact = contact.PK;

			bulkSailingBill = Factory.New<BillOfLading>();
			bulkSailingBill.JS_PackingMode = Core.Constants.ContainerModes.Bulk;
			bulkSailingBill.JS_NKLoadPort = "AUSYD";
			bulkSailingBill.JS_JX = sailing.PK;
			bulkSailingBill.JS_HouseBill = "CCC";
			bulkSailingBill.JS_OA_BookedShippingLineAddress = shippingLine.MainAddress.PK;
			bulkSailingBill.JS_ActualWeight = 1m;
			bulkSailingBill.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			bulkSailingBill.JS_ActualVolume = 2m;
			bulkSailingBill.JS_UnitOfVolume = Core.Constants.Volume.CubicFeet;
			bulkSailingBill.JS_OuterPacks = 3;
			bulkSailingBill.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			topPack = bulkSailingBill.TopLevelPacks.AddNew();
			topPack.JC_HarmonisedCode = "01011002";
			topPack.JC_GrossWeight = 1m;
			topPack.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			topPack.JC_ContainerCount = 2;
			topPack.JC_Description = "TOP PACK";
			topPack.JC_MarksAndNumbers = "TOP PACK MAKRS & NUMBERS";
			topPack.JC_F3_NKPackType = Core.Constants.PkgUnit.Box;
			topPack.JC_GoodsValue = 100m;
			topPack.JC_RX_NKGoodsCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
		}

		protected void SetUpHeaderEnvironmentForImporting()
		{
			JPAFRRegistry.Instance.AFRAddSCACtoBillsDuringSync.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			header = Factory.New<JPAFRHeader>();
			header.JPH_GB_Branch = GlbBranch.CurrentBranch.PK;
			header.JPH_IsShippingLineEntry = true;
			header.ChangeSailing(sailing.PK);
			header.JPH_OverrideFreightDefaults = true;
			header.JPH_CarrierCode = "SCAC";
		}
	}
}
