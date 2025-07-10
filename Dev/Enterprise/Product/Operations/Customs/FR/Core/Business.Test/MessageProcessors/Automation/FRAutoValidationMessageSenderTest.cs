using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Logging;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Registry;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	abstract class FRAutoValidationMessageSenderTest : TestCaseWithFactory
	{
		public void TestAutomationWorkingCorrectly()
		{
			var branch = CreateBranch();
			var declaration = CreateDeclaration(branch);
			var container = CreateContainer(declaration, Core.Constants.ContainerModes.FCL, "CONTAINER1");
			var entryHeader = CreateEntryHeader(declaration, CandidateEntriesRequiredStatus, ZString.Empty, TriggerPointsCodeList.Codes.PAB);
			entryHeader.MergedLines.AddNew();
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_From = FrenchPortSystemCodeList.Codes.MGI;
			var message = SetEDIMessageToIncludeContainerNumber("CONTAINER1");
			message.EM_EI = interchange.PK;
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			var log = CreateEventForTransport(shipment, message, Events.ArrivalCode);
			var configuration = CreateConfiguration();

			var registryCodes = CreateRegistryCodes(FrenchPortSystemCodeList.Codes.MGI);
			using (PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCodes))
			using (FRCustomsDataRegistry.Instance.TriggerPointsConfiguration.SetTemporaryValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, configuration))
			{
				Assert("Prerequisite.", Sender.ReadyToSendForTest(entryHeader));

				var logger = new TestServiceLogger();
				var processor = new FRAutoValidationMessageSenderForTest(new LoggerWrapper(logger), MessageType, CandidateEntriesRequiredStatus, IsUCC6);
				processor.Process(GlbCompany.CurrentCompany);

				AssertEquals($"The entry meets all requirement to have a Delta {MessageType} message automatically sent.", 1, entryHeader.Messages.Count);
				AssertContains(@$"Information|Start to run Automated Delta {MessageType} messages in Company EDI Branch FR1.", logger.ToString());
				AssertContains(@$"Information|Automated Delta {MessageType} messages have been sent for Customs Entries 9999999999", logger.ToString());
				AssertContains(@$"Information|Finished running Automated Delta {MessageType} messages in Company EDI Branch FR1.", logger.ToString());
			}

			using (PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCodes))
			using (FRCustomsDataRegistry.Instance.TriggerPointsConfiguration.SetTemporaryValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, configuration))
			{
				Assert("Prerequisite.", Sender.ReadyToSendForTest(entryHeader));

				var logger = new TestServiceLogger();
				var processor = new FRAutoValidationMessageSender(new LoggerWrapper(logger));
				processor.Process(GlbCompany.CurrentCompany);

				AssertEquals("No new message should have been sent as all entries have been previously processed.", 1, entryHeader.Messages.Count);
				AssertContains(@"Information|No candidate entries", logger.ToString());
			}
		}

		public void TestIsAutomationTurnedOn()
		{
			var configuration1 = new TriggerPointsConfiguration();
			configuration1.EnableAutomatedValidation = true;
			var configuration2 = new TriggerPointsConfiguration();
			configuration2.EnableAutomatedValidation = false;
			using (FRCustomsDataRegistry.Instance.TriggerPointsConfiguration.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, configuration1))
			{
				Assert("Sender.IsAutomationTurnedOn should be true when EnableAutomatedValidation is true", Sender.IsAutomationTurnedOnForTest());
			}
			using (FRCustomsDataRegistry.Instance.TriggerPointsConfiguration.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, configuration2))
			{
				Assert("Sender.IsAutomationTurnedOn should be false when EnableAutomatedValidation is false", !Sender.IsAutomationTurnedOnForTest());
			}
		}

		public void TestGetCandidateCusEntryHeaderPKsPerBranch()
		{
			var branch = GlbBranch.CurrentBranch;
			var declaration = CreateDeclaration(branch);
			var entryHeader1 = CreateEntryHeader(declaration, CandidateEntriesRequiredStatus, ZString.Empty, TriggerPointsCodeList.Codes.PAB);
			var entryHeader2 = CreateEntryHeader(declaration, UnsuitableEntryStatus, ZString.Empty, TriggerPointsCodeList.Codes.PAB);
			var entryHeader3 = CreateEntryHeader(declaration, CandidateEntriesRequiredStatus, MessageStatusCodeList.Codes.AWR, TriggerPointsCodeList.Codes.PAB);
			var entryHeader4 = CreateEntryHeader(declaration, CandidateEntriesRequiredStatus, ZString.Empty, TriggerPointsCodeList.Codes.NUL);
			var entryHeader5 = CreateEntryHeader(declaration, CandidateEntriesRequiredStatus, ZString.Empty, ZString.Empty);
			var entryHeader6 = CreateEntryHeader(declaration, CandidateEntriesRequiredStatus, MessageStatusCodeList.Codes.Error, TriggerPointsCodeList.Codes.PAB);
			Factory.Save();

			var candidateCusEntryHeaderPKs = Sender.GetCandidateCusEntryHeaderPKsPerBranchForTest(Factory);
			AssertContainsExactElementsInAnyOrder("Entries from declaration without shipment aren't candidate.", candidateCusEntryHeaderPKs, Array.Empty<ZGuid>());

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			Factory.Save();
			candidateCusEntryHeaderPKs = Sender.GetCandidateCusEntryHeaderPKsPerBranchForTest(Factory);
			AssertContainsExactElementsInAnyOrder("Entries from declaration which JE_ContainerMode is not FCL aren't candidate.", candidateCusEntryHeaderPKs, Array.Empty<ZGuid>());

			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			Factory.Save();
			candidateCusEntryHeaderPKs = Sender.GetCandidateCusEntryHeaderPKsPerBranchForTest(Factory);
			AssertContainsExactElementsInAnyOrder("Only first entry meets all requirements: declaration has shipment, declaration JE_ContainerMode is correct, entry is not awaiting response and has 050 status.", [entryHeader1.PK], candidateCusEntryHeaderPKs);
		}

		public void TestReadyToSendWhenDeclarationNotLinkedToAnyShipment()
		{
			var branch = CreateBranch();
			var declaration = CreateDeclaration(branch);
			var container = CreateContainer(declaration, Core.Constants.ContainerModes.FCL, "CONTAINER1");
			var entryHeader = CreateEntryHeader(declaration, EntryStatusDescriptionCodeList.Codes.ES050, ZString.Empty, TriggerPointsCodeList.Codes.PAB);
			var registryCodes = CreateRegistryCodes(FrenchPortSystemCodeList.Codes.MGI);
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_From = FrenchPortSystemCodeList.Codes.MGI;
			var message = SetEDIMessageToIncludeContainerNumber("CONTAINER1");
			message.EM_EI = interchange.PK;
			var shipment = Factory.New<ForwardingShipment>();
			var log = CreateEventForTransport(shipment, message, Events.ArrivalCode);
			var configuration = CreateConfiguration();

			using (PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCodes))
			using (FRCustomsDataRegistry.Instance.TriggerPointsConfiguration.SetTemporaryValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, configuration))
			{
				Assert("Declaration is not linked to shipment, should not send message.", !Sender.ReadyToSendForTest(entryHeader));

				declaration.JE_JS = shipment.PK;
				Assert("Declaration is linked to a shipment, should send message.", Sender.ReadyToSendForTest(entryHeader));
			}
		}

		public void TestReadyToSendWhenNoContainer()
		{
			var branch = CreateBranch();
			var declaration = CreateDeclaration(branch);
			var entryHeader = CreateEntryHeader(declaration, EntryStatusDescriptionCodeList.Codes.ES050, ZString.Empty, TriggerPointsCodeList.Codes.PAB);
			var registryCodes = CreateRegistryCodes(FrenchPortSystemCodeList.Codes.MGI);
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_From = FrenchPortSystemCodeList.Codes.MGI;
			var message = SetEDIMessageToIncludeContainerNumber("CONTAINER1");
			message.EM_EI = interchange.PK;
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			var log = CreateEventForTransport(shipment, message, Events.ArrivalCode);
			var configuration = CreateConfiguration();

			using (PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCodes))
			using (FRCustomsDataRegistry.Instance.TriggerPointsConfiguration.SetTemporaryValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, configuration))
			{
				Assert("Shipment has no container, should not send message.", !Sender.ReadyToSendForTest(entryHeader));

				var container = CreateContainer(declaration, Core.Constants.ContainerModes.FCL, "CONTAINER1");
				Assert("Shipment has trackable container, should send message.", Sender.ReadyToSendForTest(entryHeader));
			}
		}

		public void TestReadyToSendWhenNoTrackableContainer()
		{
			var branch = CreateBranch();
			var declaration = CreateDeclaration(branch);
			var container = CreateContainer(declaration, Core.Constants.ContainerModes.BreakBulk, "CONTAINER1");
			var entryHeader = CreateEntryHeader(declaration, EntryStatusDescriptionCodeList.Codes.ES050, ZString.Empty, TriggerPointsCodeList.Codes.PAB);
			var registryCodes = CreateRegistryCodes(FrenchPortSystemCodeList.Codes.MGI);
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_From = FrenchPortSystemCodeList.Codes.MGI;
			var message = SetEDIMessageToIncludeContainerNumber("CONTAINER1");
			message.EM_EI = interchange.PK;
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			var log = CreateEventForTransport(shipment, message, Events.ArrivalCode);
			var configuration = CreateConfiguration();

			using (PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCodes))
			using (FRCustomsDataRegistry.Instance.TriggerPointsConfiguration.SetTemporaryValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, configuration))
			{
				Assert("Shipment has no trackable container, should not send message.", !Sender.ReadyToSendForTest(entryHeader));

				container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
				Assert("Shipment has trackable container, should send message.", Sender.ReadyToSendForTest(entryHeader));
			}
		}

		public void TestReadyToSendWhenNoTriggeringPoint()
		{
			var branch = CreateBranch();
			var declaration = CreateDeclaration(branch);
			var container = CreateContainer(declaration, Core.Constants.ContainerModes.FCL, "CONTAINER1");
			var entryHeader = CreateEntryHeader(declaration, EntryStatusDescriptionCodeList.Codes.ES050, ZString.Empty, TriggerPointsCodeList.Codes.NUL);
			var registryCodes = CreateRegistryCodes(FrenchPortSystemCodeList.Codes.MGI);
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_From = FrenchPortSystemCodeList.Codes.MGI;
			var message = SetEDIMessageToIncludeContainerNumber("CONTAINER1");
			message.EM_EI = interchange.PK;
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			var log = CreateEventForTransport(shipment, message, Events.ArrivalCode);
			var configuration = CreateConfiguration();

			using (PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCodes))
			using (FRCustomsDataRegistry.Instance.TriggerPointsConfiguration.SetTemporaryValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, configuration))
			{
				Assert("Entry has no triggering point, should not send message.", !Sender.ReadyToSendForTest(entryHeader));

				entryHeader.CH_TriggeringPointForValidation = TriggerPointsCodeList.Codes.PAB;
				Assert("Entry triggering point matches event, should send message.", Sender.ReadyToSendForTest(entryHeader));
			}
		}

		public void TestReadyToSendWhenEventFromUnexpectedPCS()
		{
			var branch = CreateBranch();
			var declaration = CreateDeclaration(branch);
			var container = CreateContainer(declaration, Core.Constants.ContainerModes.FCL, "CONTAINER1");
			var entryHeader = CreateEntryHeader(declaration, EntryStatusDescriptionCodeList.Codes.ES050, ZString.Empty, TriggerPointsCodeList.Codes.PAB);
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_From = "XXX";
			var message = SetEDIMessageToIncludeContainerNumber("CONTAINER1");
			message.EM_EI = interchange.PK;
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			var log = CreateEventForTransport(shipment, message, Events.ArrivalCode);
			var configuration = CreateConfiguration();

			var registryCodes = CreateRegistryCodes(FrenchPortSystemCodeList.Codes.MGI);
			using (PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCodes))
			using (FRCustomsDataRegistry.Instance.TriggerPointsConfiguration.SetTemporaryValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, configuration))
			{
				Assert("Event issuer is unknown, should not send message.", !Sender.ReadyToSendForTest(entryHeader));

				interchange.EI_From = FrenchPortSystemCodeList.Codes.MGI;
				Assert("Event issuer matches registry settings, should send message.", Sender.ReadyToSendForTest(entryHeader));
			}
		}

		public void TestReadyToSendWhenEventDoesNotMatch()
		{
			var branch = CreateBranch();
			var declaration = CreateDeclaration(branch);
			var container = CreateContainer(declaration, Core.Constants.ContainerModes.FCL, "CONTAINER1");
			var entryHeader = CreateEntryHeader(declaration, EntryStatusDescriptionCodeList.Codes.ES050, ZString.Empty, TriggerPointsCodeList.Codes.PAB);
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_From = FrenchPortSystemCodeList.Codes.MGI;
			var message = SetEDIMessageToIncludeContainerNumber("CONTAINER1");
			message.EM_EI = interchange.PK;
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			var log = CreateEventForTransport(shipment, message, Events.GateInCode);
			var configuration = CreateConfiguration();

			var registryCodes = CreateRegistryCodes(FrenchPortSystemCodeList.Codes.MGI);
			using (PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCodes))
			using (FRCustomsDataRegistry.Instance.TriggerPointsConfiguration.SetTemporaryValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, configuration))
			{
				Assert("Event does not match trigger point, should not send message.", !Sender.ReadyToSendForTest(entryHeader));

				using (log.LockForUpdatingKeyFieldsForTesting())
				{
					log.SL_SE_NKEvent = Events.ArrivalCode;
				}
				Assert("Event matches trigger point, should send message.", Sender.ReadyToSendForTest(entryHeader));
			}
		}

		public void TestReadyToSendWhenEventCanceled()
		{
			var branch = CreateBranch();
			var declaration = CreateDeclaration(branch);
			var container = CreateContainer(declaration, Core.Constants.ContainerModes.FCL, "CONTAINER1");
			var entryHeader = CreateEntryHeader(declaration, EntryStatusDescriptionCodeList.Codes.ES050, ZString.Empty, TriggerPointsCodeList.Codes.PAB);
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_From = FrenchPortSystemCodeList.Codes.MGI;
			var message = SetEDIMessageToIncludeContainerNumber("CONTAINER1");
			message.EM_EI = interchange.PK;
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			var log = CreateEventForTransport(shipment, message, Events.ArrivalCode);
			var configuration = CreateConfiguration();

			var registryCodes = CreateRegistryCodes(FrenchPortSystemCodeList.Codes.MGI);
			using (PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCodes))
			using (FRCustomsDataRegistry.Instance.TriggerPointsConfiguration.SetTemporaryValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, configuration))
			{
				Assert("Event is valid, should send message.", Sender.ReadyToSendForTest(entryHeader));

				log.Cancel();
				Factory.Save();
				Assert("Event was canceled, should send message.", Sender.ReadyToSendForTest(entryHeader));
			}
		}

		public void TestReadyToSendWhenEventIsEstimate()
		{
			var branch = CreateBranch();
			var declaration = CreateDeclaration(branch);
			var container = CreateContainer(declaration, Core.Constants.ContainerModes.FCL, "CONTAINER1");
			var entryHeader = CreateEntryHeader(declaration, CandidateEntriesRequiredStatus, ZString.Empty, TriggerPointsCodeList.Codes.PAB);
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_From = FrenchPortSystemCodeList.Codes.MGI;
			var message = SetEDIMessageToIncludeContainerNumber("CONTAINER1");
			message.EM_EI = interchange.PK;
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			var log = CreateEventForTransport(shipment, message, Events.ArrivalCode);
			var configuration = CreateConfiguration();

			var registryCodes = CreateRegistryCodes(FrenchPortSystemCodeList.Codes.MGI);
			using (PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCodes))
			using (FRCustomsDataRegistry.Instance.TriggerPointsConfiguration.SetTemporaryValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, configuration))
			{
				Assert("Event does not match trigger point and is not estimate, should send message.", Sender.ReadyToSendForTest(entryHeader));

				using (log.LockForUpdatingKeyFieldsForTesting())
				{
					log.SL_IsEstimate = true;
				}
				Assert("Event matches trigger point but is estimate, should not send message.", !Sender.ReadyToSendForTest(entryHeader));
			}
		}

		public void TestProcessorWhenEntryHasNoEntryLine()
		{
			var branch = CreateBranch();
			var declaration = CreateDeclaration(branch);
			var container = CreateContainer(declaration, Core.Constants.ContainerModes.FCL, "CONTAINER1");
			var entryHeader = CreateEntryHeader(declaration, CandidateEntriesRequiredStatus, ZString.Empty, TriggerPointsCodeList.Codes.PAB);
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_From = FrenchPortSystemCodeList.Codes.MGI;
			var message = SetEDIMessageToIncludeContainerNumber("CONTAINER1");
			message.EM_EI = interchange.PK;
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			var log = CreateEventForTransport(shipment, message, Events.ArrivalCode);
			var configuration = CreateConfiguration();

			var registryCodes = CreateRegistryCodes(FrenchPortSystemCodeList.Codes.MGI);
			using (PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCodes))
			using (FRCustomsDataRegistry.Instance.TriggerPointsConfiguration.SetTemporaryValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, configuration))
			{
				Assert("Prerequisite.", Sender.ReadyToSendForTest(entryHeader));

				var logger = new TestServiceLogger();
				var processor = new FRAutoValidationMessageSenderForTest(new LoggerWrapper(logger), MessageType, CandidateEntriesRequiredStatus, IsUCC6);
				processor.Process(GlbCompany.CurrentCompany);

				AssertEquals($"The entry has no entry line. This prevents a Delta {MessageType} message to be automatically sent.", 0, entryHeader.Messages.Count);
				AssertContains(@$"Error|Failed to send message", logger.ToString());
				AssertContains(@$"CustEntryHeader merged lines should exist for Customs Entry 9999999999", logger.ToString());
				ErrorReporter.Clear();
			}
		}

		public void TestProcessorWhenTriggerPointsConfigurationEnableAutomatedValidationIsFalse()
		{
			var branch = CreateBranch();
			var declaration = CreateDeclaration(branch);
			var container = CreateContainer(declaration, Core.Constants.ContainerModes.FCL, "CONTAINER1");
			var entryHeader = CreateEntryHeader(declaration, CandidateEntriesRequiredStatus, ZString.Empty, TriggerPointsCodeList.Codes.PAB);
			entryHeader.MergedLines.AddNew();
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_From = FrenchPortSystemCodeList.Codes.MGI;
			var message = SetEDIMessageToIncludeContainerNumber("CONTAINER1");
			message.EM_EI = interchange.PK;
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			var log = CreateEventForTransport(shipment, message, Events.ArrivalCode);
			var registryCodes = CreateRegistryCodes(FrenchPortSystemCodeList.Codes.MGI);

			var configuration = CreateConfiguration();
			configuration.EnableAutomatedValidation = false;

			using (PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCodes))
			using (FRCustomsDataRegistry.Instance.TriggerPointsConfiguration.SetTemporaryValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, configuration))
			{
				Assert("Prerequisite.", Sender.ReadyToSendForTest(entryHeader));

				var logger = new TestServiceLogger();
				var processor = new FRAutoValidationMessageSenderForTest(new LoggerWrapper(logger), MessageType, CandidateEntriesRequiredStatus, IsUCC6);
				processor.Process(GlbCompany.CurrentCompany);

				AssertEquals($"Configuration prevents from having a Delta {MessageType} message automatically sent.", 0, entryHeader.Messages.Count);
				AssertContains(@$"Information|Automated Delta {MessageType} messages is not enabled in Company EDI Branch FR1.", logger.ToString());
			}
		}

		[TestDate(2023, 1, 15, 11, 52, 00)]
		public void TestReadyToSendDependOn_JEExportDate_ExportDateGreaterThanARVTime()
		{
			AssertReadyToSendDependOnJe_ExportDate(exportDate: ZDateTime.Now.AddDays(1), shouldBeSent: false, eventCode: Events.ArrivalCode, triggerPoint: TriggerPointsCodeList.Codes.PAB);
		}

		[TestDate(2023, 1, 15, 11, 52, 00)]
		public void TestReadyToSendDependOn_JEExportDate_ExportDateEmpty()
		{
			AssertReadyToSendDependOnJe_ExportDate(exportDate: ZDateTime.Empty, shouldBeSent: false, eventCode: Events.ArrivalCode, triggerPoint: TriggerPointsCodeList.Codes.PAB);
		}

		[TestDate(2023, 1, 15, 11, 52, 00)]
		public void TestReadyToSendDependOn_JEExportDate_ExportDateEqualARVTime()
		{
			AssertReadyToSendDependOnJe_ExportDate(exportDate: ZDateTime.Now, shouldBeSent: true, eventCode: Events.ArrivalCode, triggerPoint: TriggerPointsCodeList.Codes.PAB);
		}

		[TestDate(2023, 1, 15, 11, 52, 00)]
		public void TestReadyToSendDependOn_JEExportDate_ExportDateLessThanARVTime()
		{
			AssertReadyToSendDependOnJe_ExportDate(exportDate: ZDateTime.Now.AddDays(-1), shouldBeSent: true, eventCode: Events.ArrivalCode, triggerPoint: TriggerPointsCodeList.Codes.PAB);
		}

		[TestDate(2023, 1, 15, 11, 52, 00)]
		public void TestReadyToSendDependOn_JEExportDate_EventIsNotARV()
		{
			AssertReadyToSendDependOnJe_ExportDate(exportDate: ZDateTime.Now.AddDays(1), shouldBeSent: true, eventCode: Events.FreightUnloadedCode, triggerPoint: TriggerPointsCodeList.Codes.VAQ);
		}

		void AssertReadyToSendDependOnJe_ExportDate(ZDateTime exportDate, bool shouldBeSent, string eventCode, string triggerPoint)
		{
			var branch = CreateBranch();
			var declaration = CreateDeclaration(branch);
			declaration.JE_ExportDate = exportDate;

			var container = CreateContainer(declaration, Core.Constants.ContainerModes.FCL, "123");
			var entryHeader = CreateEntryHeader(declaration, CandidateEntriesRequiredStatus, ZString.Empty, triggerPoint);
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_From = FrenchPortSystemCodeList.Codes.MGI;
			var message = SetEDIMessageToIncludeContainerNumber("123");
			message.EM_EI = interchange.PK;

			var consol = Factory.New<CommonConsol>();
			consol.AutomaticallyUpdatePackLineContainers = false;
			var container1 = consol.Containers.AddNew();
			var shipment = consol.Shipments.AddNew();

			var line1 = shipment.OuterPackLines.AddNew();

			container1.JC_ContainerCount = 1;
			container1.PackLines.Add(line1);

			container1.JC_ContainerNum = "123";

			declaration.JE_JS = shipment.PK;

			var log = CreateEventForTransport(declaration.Shipment, message, eventCode);
			var log2 = CreateEventForContainer(declaration.Shipment, message, eventCode);

			var configuration = CreateConfiguration();

			var registryCodes = CreateRegistryCodes(FrenchPortSystemCodeList.Codes.MGI);
			using (PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCodes))
			using (FRCustomsDataRegistry.Instance.TriggerPointsConfiguration.SetTemporaryValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, configuration))
			{
				using (log.LockForUpdatingKeyFieldsForTesting())
				{
					log.SL_SE_NKEvent = eventCode;
					log.SL_EventTime = ZDateTime.Now;
					var sourceInfo = log.SourceInfoItems.AddNew();
					sourceInfo.Key = "Container Number";
					sourceInfo.Data = "123";
				}

				using (log2.LockForUpdatingKeyFieldsForTesting())
				{
					log2.SL_SE_NKEvent = eventCode;
					log2.SL_EventTime = ZDateTime.Now;
					var sourceInfo = log2.SourceInfoItems.AddNew();
					sourceInfo.Key = "Container Number";
					sourceInfo.Data = "123";
				}

				AssertEquals($"Time of ARV event is {log.SL_EventTime}, export date is {declaration.JE_ExportDate} then ReadyToSend should return {shouldBeSent}.", shouldBeSent, Sender.ReadyToSendForTest(entryHeader));
			}
			ReleaseFactory();
		}

		protected abstract ZString MessageType { get; }

		protected abstract ZString CandidateEntriesRequiredStatus { get; }

		protected abstract ZString UnsuitableEntryStatus { get; }

		protected abstract bool IsUCC6 { get; }

		protected static TriggerPointsConfiguration CreateConfiguration()
		{
			var configuration = new TriggerPointsConfiguration();
			configuration.EnableAutomatedValidation = true;
			return configuration;
		}

		protected static GlbBranch CreateBranch()
		{
			var company = GlbCompany.CurrentCompany;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "FR1";
			company.Factory.Save();
			return branch;
		}

		[TestDate(2023, 1, 15, 11, 52, 00)]
		protected JobDeclaration CreateDeclaration(GlbBranch branch)
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "Supplier1";

			var account = importer.DeltaAgreementNumberCollection.AddNew();
			account.CZ_Account = "100000";
			account.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;
			account.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			account.CZ_RepresentativeID = "Test1";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = branch.PK;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = "IMP";
			declaration.JE_RL_NKPortOfArrival = "FRFR2";
			declaration.JE_RL_NKFinalDestination = "FRFR2";
			declaration.JE_CustomsProfile = "100000";
			if (IsUCC6)
			{
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			}
			else
			{
				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			}
			
			declaration.JE_ExportDate = ZDateTime.Now.AddDays(-1);

			return declaration;
		}

		protected static EU.Business.Declaration.CusContainer CreateContainer(JobDeclaration declaration, ZString mode, ZString number)
		{
			var container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = mode;
			container.CO_ContainerNumber = number;
			return container;
		}

		protected CusEntryHeader CreateEntryHeader(JobDeclaration declaration, ZString entryStatus, ZString status, ZString triggerPoint)
		{
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = entryStatus;
			entryHeader.CH_Status = status;
			entryHeader.CH_BGMReference = "9999999999";
			Factory.Save();
			entryHeader.CH_TriggeringPointForValidation = triggerPoint;
			return entryHeader;
		}

		protected static CommunitySystemCodesOfForwarderAndAgentCollection CreateRegistryCodes(ZString pcsCode)
		{
			var registryCodes = new CommunitySystemCodesOfForwarderAndAgentCollection();
			var code = registryCodes.AddNew();
			code.AgentCode = "AGENT222";
			code.ForwarderCode = "FORWARDER222";
			code.Port = "FRFR2";
			code.PCS = pcsCode;
			return registryCodes;
		}

		protected StmALog CreateEventForTransport(ForwardingShipment shipment, EDIMessage message, ZString eventCode)
		{
			var transport = shipment.TransportsIncludingRelated.AddNew();
			transport.JW_ATD = ZDateTime.Now.AddDays(-1);
			transport.JW_RL_NKDiscPortForBinding = "FRFR2";
			return CreateEvent(transport, message, eventCode);
		}

		protected StmALog CreateEventForContainer(ForwardingShipment shipment, EDIMessage message, ZString eventCode)
		{
			var container = shipment.Containers.FirstOrDefault();
			return CreateEvent(container, message, eventCode);
		}

		protected StmALog CreateEvent(EnterpriseBusinessObject parent, EDIMessage message, ZString eventCode)
		{
			var log = parent.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_IsEstimate = false;
				log.SL_SE_NKEvent = eventCode;
				log.SL_EventTime = ZDateTime.Now;
			}
			Factory.Save();
			var pivot = Factory.NewWithValidTestData<GenPivot>();
			pivot.XX_RelationType = "XEM";
			pivot.Relation1ID = log.PK;
			pivot.Relation2ID = message.PK;
			Factory.Save();

			return log;
		}

		protected EDIMessage SetEDIMessageToIncludeContainerNumber(ZString containerNumber)
		{
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.EM_MessageText = $@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
    <Event>
        <DataContext>
            <DataTargetCollection>
                <DataTarget>
                    <Key>CNTE015702</Key>
                    <Type>ForwardingConsol</Type>
                </DataTarget>
            </DataTargetCollection>
        </DataContext>
        <EventTime>2022-10-12T15:18:00</EventTime>
        <EventType>ARV</EventType>
        <EventReference />
        <IsEstimate>FALSE</IsEstimate>
        <EventParameters>
            <Location>FRFOS</Location>
            <Facility>CTO</Facility>
        </EventParameters>
        <ContextCollection>
            <Context>
                <Type>ContainerNumber</Type>
                <Value>{containerNumber}</Value>
            </Context>
            <Context>
                <Type>VesselName</Type>
                <Value>DELPHIS RIGA</Value>
            </Context>
            <Context>
                <Type>PortLocation</Type>
                <Value>2XLSY</Value>
            </Context>
            <Context>
                <Type>PortArea</Type>
                <Value>FOS</Value>
            </Context>
            <Context>
                <Type>AMQReference</Type>
                <Value>183354985</Value>
            </Context>
            <Context>
                <Type>APPlusVoyageReference</Type>
                <Value>440100</Value>
            </Context>
            <Context>
                <Type>Warehouse</Type>
                <Value>SEAYARD</Value>
            </Context>
        </ContextCollection>
    </Event>
</UniversalEvent>";
			return message;
		}

		FRAutoValidationMessageSenderForTest Sender => sender ?? (sender = new FRAutoValidationMessageSenderForTest(new LoggerWrapper(new TestServiceLogger()), MessageType, CandidateEntriesRequiredStatus, IsUCC6));
		FRAutoValidationMessageSenderForTest sender;

		class FRAutoValidationMessageSenderForTest : FRAutoValidationMessageSender
		{
			public FRAutoValidationMessageSenderForTest(ICommonLogger logger, ZString messageType, ZString candidateEntriesRequiredStatus, bool isUCC6) : base(logger)
			{
				this.candidateEntriesRequiredStatus = candidateEntriesRequiredStatus;
				this.messageType = messageType;
				this.isUCC6 = isUCC6;
			}

			public bool ReadyToSendForTest(CusEntryHeader entry) => ReadyToSend(entry);

			public IEnumerable<ZGuid> GetCandidateCusEntryHeaderPKsPerBranchForTest(BusinessObjectFactory factory) => GetCandidateCusEntryHeaderPKsPerBranch(factory);

			public bool IsAutomationTurnedOnForTest() => IsAutomationTurnedOn();

			protected override ZString CandidateEntriesRequiredStatus => candidateEntriesRequiredStatus;

			readonly ZString candidateEntriesRequiredStatus;

			protected override ZString MessageType => messageType;

			readonly ZString messageType;

			protected override bool IsUCC6 => isUCC6;

			readonly bool isUCC6;
		}
	}
}
