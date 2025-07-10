using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.MessageBuilders;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using static Enterprise.Customs.GB.Business.EnhancedValidationParticipationHelper;
using RefCusCodeListAttributes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeList.Attributes;

namespace Enterprise.Customs.GB.Business.Testing
{
	sealed class GbDeclarationMessageSenderTest : TestCaseWithFactory
	{
		public void TestAreAnyHeadersAwaitingAResponse()
		{
			var newDateTime = ZDateTime.UtcNow;
			var oldDateTime = newDateTime.AddSeconds(-1);

			var tests = new MessageTestDefinition[]
			{
				new MessageTestDefinition("Message (type AMD status SNT) should be expecting a response", new MessageParameters[]
				{
					new MessageParameters("AMD", "SNT", newDateTime)
				}, true),
				new MessageTestDefinition("Message (type AMD status ACK) should not be expecting a response", new MessageParameters[]
				{
					new MessageParameters("AMD", "ACK", newDateTime)
				}, false),
				new MessageTestDefinition("Single AMD message with status CAN should not be expecting a response", new MessageParameters[]
				{
					new MessageParameters("AMD", "CAN", newDateTime)
				}, false),
				new MessageTestDefinition("Single AMD message with status RCV should not be expecting a response", new MessageParameters[]
				{
					new MessageParameters("AMD", "RCV", newDateTime)
				}, false),
				new MessageTestDefinition("Single AMD message with status REJ should not be expecting a response", new MessageParameters[]
				{
					new MessageParameters("AMD", "REJ", newDateTime)
				}, false),
				new MessageTestDefinition("Messages(type AMD status REJ new),(type NAM status DCD new) should not be expecting a response", new MessageParameters[]
				{
					new MessageParameters("AMD", "REJ", newDateTime),
					new MessageParameters("NAM", "DCD", newDateTime)
				}, false),
				new MessageTestDefinition("Messages(type NAM status DCD new),(type AMD status REJ new) should not be expecting a response", new MessageParameters[]
				{
					new MessageParameters("NAM", "DCD", newDateTime),
					new MessageParameters("AMD", "REJ", newDateTime)
				}, false),
				new MessageTestDefinition("Messages(type AMD status REJ new),(type NAM status DCD old) should not be expecting a response", new MessageParameters[]
				{
					new MessageParameters("AMD", "REJ", newDateTime),
					new MessageParameters("NAM", "DCD", oldDateTime)
				}, false),
				new MessageTestDefinition("Messages(type NAM status DCD old),(type AMD status REJ new) should not be expecting a response", new MessageParameters[]
				{
					new MessageParameters("NAM", "DCD", oldDateTime),
					new MessageParameters("AMD", "REJ", newDateTime)
				}, false),
				new MessageTestDefinition("Messages(type AMD status SNT new),(type NAM status DCD old) should be expecting a response", new MessageParameters[]
				{
					new MessageParameters("AMD", "SNT", newDateTime),
					new MessageParameters("NAM", "DCD", oldDateTime)
				}, true),
				new MessageTestDefinition("Messages(type AMD status SNT old),(type NAM status DCD new) should be expecting a response", new MessageParameters[]
				{
					new MessageParameters("AMD", "SNT", oldDateTime),
					new MessageParameters("NAM", "DCD", newDateTime)
				}, true),
				new MessageTestDefinition("Messages(type NAM status DCD old),(type AMD status SNT new) should be expecting a response", new MessageParameters[]
				{
					new MessageParameters("NAM", "DCD", oldDateTime),
					new MessageParameters("AMD", "SNT", newDateTime)
				}, true),
				new MessageTestDefinition("Messages(type NAM status DCD new),(type AMD status SNT old) should be expecting a response", new MessageParameters[]
				{
					new MessageParameters("NAM", "DCD", newDateTime),
					new MessageParameters("AMD", "SNT", oldDateTime)
				}, true),
			};

			CombineAssertions(() =>
			{
				foreach (var test in tests)
				{
					var declaration = Factory.New<JobDeclaration>();
					var header = declaration.CustomsEntryHeaders.AddNew();

					foreach (var msg in test.Messages)
					{
						var message = Factory.New<GbEDIMessageForTest>();
						header.Messages.Add(message);
						message.EM_MessageType = msg.MessageType;
						message.EM_Status = msg.Status;
						message.EM_SystemCreateTimeUtc = msg.SystemCreateTimeUtc;
					}

					Factory.Save();

					Assert(test.Name, test.ExpectedResult == GbDeclarationMessageSender.AreAnyHeadersAwaitingAResponse(declaration.CustomsEntryHeaders));
				}
			});
		}

		public void TestAreAnyHeadersAwaitingAResponse2()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header1 = declaration.CustomsEntryHeaders.AddNew();
			var header2 = declaration.CustomsEntryHeaders.AddNew();
			var message = Factory.New<GbEDIMessageForTest>();
			header2.Messages.Add(message);
			message.EM_MessageType = "AMD";
			message.EM_Status = "SNT";
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;

			Assert("AreAnyHeadersAwaitingAResponse should be true when second header has a message that is awaiting a response!", GbDeclarationMessageSender.AreAnyHeadersAwaitingAResponse(declaration.CustomsEntryHeaders));
		}

		public void TestGetRelevantServiceTasksAreNotRunningHealthilyAndOverallHostState()
		{
			var sender = new GbDeclarationMessageSenderForTest();
			var mockQuerier = new Mock<IServiceManagerQuerier>();

			mockQuerier.Setup(x => x.CheckStateOfNamedServiceTask(It.IsAny<string>())).Returns(ServiceTaskStatus.NoAvailableHosts);
			using (ObjectFactory.Substitute(mockQuerier.Object))
			{
				var (badTasks, wasFirstCheckFailedDueToHostNotRunning) = sender.GetRelevantServiceTasksAreNotRunningHealthilyAndOverallHostState();
				AssertEquals(true, wasFirstCheckFailedDueToHostNotRunning);
				AssertArrayEqualsByElements(new string[] { "ABC" }, badTasks.ToArray());
			}

			mockQuerier.Setup(x => x.CheckStateOfNamedServiceTask(It.IsAny<string>())).Returns(ServiceTaskStatus.ServiceTaskIsInactive);
			using (ObjectFactory.Substitute(mockQuerier.Object))
			{
				var (badTasks, wasFirstCheckFailedDueToHostNotRunning) = sender.GetRelevantServiceTasksAreNotRunningHealthilyAndOverallHostState();
				AssertEquals(false, wasFirstCheckFailedDueToHostNotRunning);
				AssertArrayEqualsByElements(new string[] { "ABC", "DEF", "GHI", "JKL" }, badTasks.ToArray());
			}

			mockQuerier.Setup(x => x.CheckStateOfNamedServiceTask(It.IsAny<string>())).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);
			using (ObjectFactory.Substitute(mockQuerier.Object))
			{
				var (badTasks, wasFirstCheckFailedDueToHostNotRunning) = sender.GetRelevantServiceTasksAreNotRunningHealthilyAndOverallHostState();
				AssertEquals(false, wasFirstCheckFailedDueToHostNotRunning);
				AssertArrayEqualsByElements(Array.Empty<string>(), badTasks.ToArray());
			}

			mockQuerier.Setup(x => x.CheckStateOfNamedServiceTask(It.IsAny<string>())).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);
			mockQuerier.Setup(x => x.CheckStateOfNamedServiceTask("DEF")).Returns(ServiceTaskStatus.NoMachineUponWhichToRunTheTask);
			using (ObjectFactory.Substitute(mockQuerier.Object))
			{
				var (badTasks, wasFirstCheckFailedDueToHostNotRunning) = sender.GetRelevantServiceTasksAreNotRunningHealthilyAndOverallHostState();
				AssertEquals(false, wasFirstCheckFailedDueToHostNotRunning);
				AssertArrayEqualsByElements(new string[] { "DEF" }, badTasks.ToArray());
			}
		}

		public void TestGetRequiredServiceTasksAndWarnIfNotRunning()
		{
			var sender = new GbDeclarationMessageSenderForTest();
			string savedMessage = null;
			string savedCaption = null;

			var mockSendMessagesToCustoms = new Mock<ISendsMessagesToCustoms>();
			mockSendMessagesToCustoms.Setup(x => x.WarnUserAboutSomething(It.IsAny<string>(), It.IsAny<string>()))
				.Callback(new Action<string, string>((message, caption) =>
				{
					savedMessage = message;
					savedCaption = caption;
				}));

			var mockQuerier = new Mock<IServiceManagerQuerier>();

			mockQuerier.Setup(x => x.CheckStateOfNamedServiceTask(It.IsAny<string>())).Returns(ServiceTaskStatus.NoAvailableHosts);
			using (ObjectFactory.Substitute(mockQuerier.Object))
			{
				sender.GetRequiredServiceTasksAndWarnIfNotRunning(mockSendMessagesToCustoms.Object);
				AssertEquals("Not all the required service tasks are currently running. The message will be queued, but until the service tasks are all started it may not be sent. The process controller was reported to be not found or not running.", savedMessage);
				AssertEquals("Required service(s) not running.", savedCaption);
			}

			mockQuerier.Setup(x => x.CheckStateOfNamedServiceTask(It.IsAny<string>())).Returns(ServiceTaskStatus.NoSuchTaskIsInstalledInThisDb);
			using (ObjectFactory.Substitute(mockQuerier.Object))
			{
				sender.GetRequiredServiceTasksAndWarnIfNotRunning(mockSendMessagesToCustoms.Object);
				AssertEquals(string.Join(System.Environment.NewLine,
					"Not all the required service tasks are currently running. The message will be queued, but until the service tasks are all started it may not be sent. Please have your administrator start/activate tasks with these codes:",
					"ABC", "DEF", "GHI", "JKL"), savedMessage);
				AssertEquals("Required service task(s) not running.", savedCaption);
			}
		}

		public void TestGetEnhancedValidationParticipation()
		{
			var sender = new GbDeclarationMessageSenderForTest();
			IEnhancedValidationEntryWrapper savedWrapper = null;

			var mockEnhancedValidation = new Mock<IEnhancedValidation>();
			_ = mockEnhancedValidation.Setup(x => x.ShowEnhancedValidation(It.IsAny<IEnhancedValidationEntryWrapper>()))
				.Callback(new Action<IEnhancedValidationEntryWrapper>(wrapper => { savedWrapper = wrapper; }))
				.Returns(true);
			var mockSendMessagesToCustoms = mockEnhancedValidation.As<ISendsMessagesToCustoms>();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			var org = Factory.New<OrgHeader>();
			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedKingdom);
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = country.Code;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			cusCode.OK_CustomsRegNo = "606091760000";
			declaration.Branch.GB_OH_OrgProxy = org.PK;
			AssertEquals("Pre-Req:", "GB606091760000", declaration.GetEori());
			AssertEquals("Pre-Req:", Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF, declaration.JE_ApplicationCode);

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.LRN = "PZLLGA163004306145012377115";
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.LRN = "PZLLGA163004306145012377116";
			var entriesToSend = new[] { entry1, entry2 };

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionalityAttribute(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, RefCusCodeListAttributes.WarningFactor, "2.0"))
			{
				var result = sender.GetEnhancedValidationParticipation(declaration, entriesToSend, mockSendMessagesToCustoms.Object);
				AssertNull("No enhanced validation for CHIEF", savedWrapper);
				AssertEquals("No enhanced validation for CHIEF", expected: true, result);
			}

			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today.AddDays(3), true))
			{
				var result = sender.GetEnhancedValidationParticipation(declaration, entriesToSend, mockSendMessagesToCustoms.Object);
				AssertNull("No enhanced validation for CDS, when FUNCS is off", savedWrapper);
				AssertEquals("No enhanced validation for CDS, when FUNCS is off", expected: true, result);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true))
			{
				var result = sender.GetEnhancedValidationParticipation(declaration, entriesToSend, mockSendMessagesToCustoms.Object);
				AssertNull("No enhanced validation for CDS, when attribute isn't set", savedWrapper);
				AssertEquals("No enhanced validation for CDS, when attribute isn't set", expected: true, result);
			}

			using (GBCustomsDataRegistry.Instance.DigitalPromptsTrialParticipateInNudgeTrial.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionalityAttribute(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, RefCusCodeListAttributes.WarningFactor, "2.0"))
			{
				var result = sender.GetEnhancedValidationParticipation(declaration, entriesToSend, mockSendMessagesToCustoms.Object);
				AssertNull("No enhanced validation for CDS, when Registry is disabled", savedWrapper);
				AssertEquals("No enhanced validation for CDS, when Registry is disabled", expected: true, result);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionalityAttribute(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, RefCusCodeListAttributes.WarningFactor, "2.0"))
			{
				var result = sender.GetEnhancedValidationParticipation(declaration, entriesToSend, mockSendMessagesToCustoms.Object);
				AssertNotNull("Enhanced validation for CDS", savedWrapper);
				AssertEquals("Enhanced validation for CDS, with OK", expected: true, result);

				_ = mockEnhancedValidation.Setup(x => x.ShowEnhancedValidation(It.IsAny<IEnhancedValidationEntryWrapper>()))
					.Callback(new Action<IEnhancedValidationEntryWrapper>(wrapper => { savedWrapper = wrapper; }))
					.Returns(false);

				result = sender.GetEnhancedValidationParticipation(declaration, entriesToSend, mockSendMessagesToCustoms.Object);
				AssertEquals("Enhanced validation for CDS, with Cancel", expected: false, result);
			}
		}

		[ExpectNoExceptions]
		public void TestGetEnhancedValidationParticipation_CountShowEnhancedValidationInvocations()
		{
			var sender = new GbDeclarationMessageSenderForTest();

			var mockEnhancedValidation = new Mock<IEnhancedValidation>();
			_ = mockEnhancedValidation.Setup(x => x.ShowEnhancedValidation(It.IsAny<IEnhancedValidationEntryWrapper>())).Returns(true);
			var mockSendMessagesToCustoms = mockEnhancedValidation.As<ISendsMessagesToCustoms>();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var org = Factory.New<OrgHeader>();
			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedKingdom);
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = country.Code;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			declaration.Branch.GB_OH_OrgProxy = org.PK;

			foreach (var eoriGroup in EnhancedValidationParticipationHelperTestData.GetData().GroupBy(x => x.EORI))
			{
				var eori = eoriGroup.Key;
				cusCode.OK_CustomsRegNo = eori.Substring(2);

				declaration.CustomsEntryHeaders.Clear();
				eoriGroup.ForEach(x =>
				{
					var entry = declaration.CustomsEntryHeaders.AddNew();
					entry.LRN = x.LRN;
				});

				sender.CanEntryParticipateEnhancedValidationExposed = (entry) => true;
				var expectedFactorTwoNudgeCount = eoriGroup.Count(x => x.ShowNudgeFactorTwoTreatment);
				AssertEnhancedValidationParticipation(WarningFactor.Two, expectedFactorTwoNudgeCount);

				var expectedFactorSixteenNudgeCount = eoriGroup.Count(x => x.ShowNudgeFactorSixteenTreatment);
				AssertEnhancedValidationParticipation(WarningFactor.Sixteen, expectedFactorSixteenNudgeCount);

				sender.CanEntryParticipateEnhancedValidationExposed = (entry) => false;
				AssertEnhancedValidationParticipation(WarningFactor.Two, 0);
				AssertEnhancedValidationParticipation(WarningFactor.Sixteen, 0);
			}

			void AssertEnhancedValidationParticipation(WarningFactor warningFactor, int expectedNudgesCount)
			{
				var attributeValue = warningFactor == WarningFactor.Two ? "2.0" : "16.0";
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true))
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionalityAttribute(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, RefCusCodeListAttributes.WarningFactor, attributeValue))
				{
					_ = sender.GetEnhancedValidationParticipation(declaration, declaration.CustomsEntryHeaders, mockSendMessagesToCustoms.Object);
					mockEnhancedValidation.Verify(x => x.ShowEnhancedValidation(It.IsAny<IEnhancedValidationEntryWrapper>()), Times.Exactly(expectedNudgesCount));
					mockEnhancedValidation.Invocations.Clear();
				}
			}
		}

		struct MessageParameters
		{
			public string MessageType;
			public string Status;
			public ZDateTime SystemCreateTimeUtc;

			public MessageParameters(string messageType, string status, ZDateTime systemCreateTimeUtc)
			{
				MessageType = messageType;
				Status = status;
				SystemCreateTimeUtc = systemCreateTimeUtc;
			}
		}
		struct MessageTestDefinition
		{
			public string Name;
			public MessageParameters[] Messages;
			public bool ExpectedResult;

			public MessageTestDefinition(string name, MessageParameters[] messages, bool expectedResult)
			{
				Name = name;
				Messages = messages;
				ExpectedResult = expectedResult;
			}
		}

		class GbEDIMessageForTest : GbEDIMessage
		{
			public GbEDIMessageForTest(BusinessObjectFactory fact, DataRow row) : base(fact, row)
			{
				MessageNumberStrategy = new GbMessageNumberStrategy(Factory, "X");
			}
		}

		class GbDeclarationMessageSenderForTest : GbDeclarationMessageSender
		{
			public new void GetRequiredServiceTasksAndWarnIfNotRunning(ISendsMessagesToCustoms sendMessagesToCustoms)
			{
				base.GetRequiredServiceTasksAndWarnIfNotRunning(sendMessagesToCustoms);
			}

			public new (List<string> badTasks, bool wasCheckFailedDueToHostNotRunning) GetRelevantServiceTasksAreNotRunningHealthilyAndOverallHostState()
			{
				return base.GetRelevantServiceTasksAreNotRunningHealthilyAndOverallHostState();
			}

			public override string[] GetCodesOfRelevantServiceTasksThatShouldBeCheckedBeforeSending() => new string[]
				{ "ABC", "DEF", "GHI", "JKL" };

			public override IMessageGenerator<EU.Business.Declaration.CusEntryHeader> GetTransmissionGenerator(CusdecMessageFunction declarationMessageFunction)
			{
				throw new NotImplementedException();
			}

			protected override bool ValidateAndShowUserAnyWarningsOrErrors(IBusiness bizO, ISendsMessagesToCustoms sendMessagesToCustoms, CusdecMessageFunction functionNewDeletedAmended)
			{
				throw new NotImplementedException();
			}

			public new bool GetEnhancedValidationParticipation(EU.Business.Declaration.JobDeclaration declaration, IEnumerable<EU.Business.Declaration.CusEntryHeader> entriesToSend, ISendsMessagesToCustoms sendMessagesToCustoms) =>
				base.GetEnhancedValidationParticipation(declaration, entriesToSend, sendMessagesToCustoms);

			public Func<Declaration.CusEntryHeader, bool> CanEntryParticipateEnhancedValidationExposed { get; set; } = (entry) => true;

			protected override bool CanEntryParticipateEnhancedValidation(Declaration.CusEntryHeader entry) => CanEntryParticipateEnhancedValidationExposed(entry);
		}
	}
}
