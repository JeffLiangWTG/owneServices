using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Telematics;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Telematics.ServiceTasks;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using WTG.Telematics.Common;
using WTG.Telematics.Data.CargoWiseOne;

namespace ZClientEDI.Test.Telematics
{
	public class ServerRegistrationRequestMessageProcessorTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			loggerMock = new Mock<ILogger>();
			eHubMessageSenderMock = new Mock<IEHubMessageSender>();
			serverRegistrationRequestMessageProcessor = new ServerRegistrationRequestMessageProcessor(loggerMock.Object, eHubMessageSenderMock.Object);
		}

		public void TestFunctionalityIsOnlyAvailableOnEdiProd()
		{
			// Arrange
			var licence = Env.CurrentCompany.GetLicenceCode();
			var configRequest = XmlDataSerializer.Serialize(new ServerRegistrationRequestMessage());

			// Act
			var result = serverRegistrationRequestMessageProcessor.Process(Factory, configRequest);

			// Assert
			AssertEquals(0, result);
			AssertNoExceptionThrown(() =>
			{
				loggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.Once);
				loggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
				loggerMock.Verify(mock => mock.Log(LogType.Warning, It.Is<string>(s => s.Contains(licence) && s.Contains(SystemDataRegistry.Instance.EdiProdLicenceIdentifier.Value))), Times.Once);
			});
		}

		public void TestNewEHubIdsAreRegistered()
		{
			CombineAssertions(() =>
			{
				Test("123");
				Test("456");
			});

			void Test(string eHubId)
			{
				// Arrange
				EDIDataRegistry.Instance.ActiveMiddlewareService.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, EDIDataRegistry.Instance.ActiveMiddlewareService.DefaultValue);
				var configRequest = XmlDataSerializer.Serialize(new ServerRegistrationRequestMessage
				{
					EHubId = eHubId,
				});
				using (SystemDataRegistry.Instance.EdiProdLicenceIdentifier.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Env.CurrentCompany.GetLicenceCode()))
				{
					// Act
					var result = serverRegistrationRequestMessageProcessor.Process(Factory, configRequest);

					// Assert
					AssertEquals(1, result);
					AssertEquals(1, EDIDataRegistry.Instance.ActiveMiddlewareService.Value.GetActiveCodeDescriptionPairList().Count);
					var record = EDIDataRegistry.Instance.ActiveMiddlewareService.Value[0];
					AssertEquals(eHubId, record.Code);
					AssertEquals(true, record.Bool);
				}
			}
		}

		public void TestDuplicateEHubIdsAreNotAdded()
		{
			CombineAssertions(() =>
			{
				Test("123");
				Test("456");
			});

			void Test(string eHubId)
			{
				// Arrange
				EDIDataRegistry.Instance.ActiveMiddlewareService.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, EDIDataRegistry.Instance.ActiveMiddlewareService.DefaultValue);
				var configRequest = XmlDataSerializer.Serialize(new ServerRegistrationRequestMessage
				{
					EHubId = eHubId,
				});
				using (SystemDataRegistry.Instance.EdiProdLicenceIdentifier.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Env.CurrentCompany.GetLicenceCode()))
				{
					// Act
					var result = new[]
					{
						serverRegistrationRequestMessageProcessor.Process(Factory, configRequest),
						serverRegistrationRequestMessageProcessor.Process(Factory, configRequest),
						serverRegistrationRequestMessageProcessor.Process(Factory, configRequest),
					};

					// Assert
					AssertContainsExactElementsInAnyOrder(new[] { 1, 1, 1 }, result);
					AssertEquals(1, EDIDataRegistry.Instance.ActiveMiddlewareService.Value.GetActiveCodeDescriptionPairList().Count);
					var record = EDIDataRegistry.Instance.ActiveMiddlewareService.Value[0];
					AssertEquals(eHubId, record.Code);
					AssertEquals(true, record.Bool);
				}
			}
		}

		public void TestInactiveEHubIdsAreReactivatedUponRequest()
		{
			CombineAssertions(() =>
			{
				Test("123");
				Test("456");
			});

			void Test(string eHubId)
			{
				// Arrange
				EDIDataRegistry.Instance.ActiveMiddlewareService.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, EDIDataRegistry.Instance.ActiveMiddlewareService.DefaultValue);
				EDIDataRegistry.Instance.ActiveMiddlewareService.Value.Add(eHubId, (NoResString)string.Empty, false);
				var configRequest = XmlDataSerializer.Serialize(new ServerRegistrationRequestMessage
				{
					EHubId = eHubId,
				});
				using (SystemDataRegistry.Instance.EdiProdLicenceIdentifier.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Env.CurrentCompany.GetLicenceCode()))
				{
					// Act
					var result = serverRegistrationRequestMessageProcessor.Process(Factory, configRequest);

					// Assert
					AssertEquals(1, result);
					AssertEquals(1, EDIDataRegistry.Instance.ActiveMiddlewareService.Value.GetActiveCodeDescriptionPairList().Count);
					var record = EDIDataRegistry.Instance.ActiveMiddlewareService.Value[0];
					AssertEquals(eHubId, record.Code);
					AssertEquals(true, record.Bool);
				}
			}
		}

		public void TestOverwritesRegistryValueIfValueDoesNotExist()
		{
			// Arrange
			const string eHubId = "aaabbb";
			var configRequest = XmlDataSerializer.Serialize(new ServerRegistrationRequestMessage
			{
				EHubId = eHubId,
			});
			using (SystemDataRegistry.Instance.EdiProdLicenceIdentifier.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Env.CurrentCompany.GetLicenceCode()))
			{
				// Act
				var result = serverRegistrationRequestMessageProcessor.Process(Factory, configRequest);

				// Assert
				AssertEquals(1, result);
				AssertEquals(true, EDIDataRegistry.Instance.ActiveMiddlewareService.Value.HasChanges);
				AssertEquals(1, EDIDataRegistry.Instance.ActiveMiddlewareService.Value.GetActiveCodeDescriptionPairList().Count);
				var record = EDIDataRegistry.Instance.ActiveMiddlewareService.Value[0];
				AssertEquals(eHubId, record.Code);
				AssertEquals(true, record.Bool);
			}
		}

		public void TestDoesNotOverwriteRegistryValueIfValueExists()
		{
			// Arrange
			const string eHubId = "aaabbb";
			var configRequest = XmlDataSerializer.Serialize(new ServerRegistrationRequestMessage
			{
				EHubId = eHubId,
			});
			var collection = new CodeDescriptionBoolCollection
			{
				{ ServerRegistrationRequestMessageProcessor.TelematicsServiceCodeMaxLength, "code1", (NoResString)string.Empty, true },
				{ ServerRegistrationRequestMessageProcessor.TelematicsServiceCodeMaxLength, "code2", (NoResString)string.Empty, true },
				{ ServerRegistrationRequestMessageProcessor.TelematicsServiceCodeMaxLength, "code3", (NoResString)string.Empty, true },
			};
			EDIDataRegistry.Instance.ActiveMiddlewareService.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var expectedCodes = collection.Cast<ICodeDescription>().Select(description => description.Code).Concat(new[] { eHubId });

			using (SystemDataRegistry.Instance.EdiProdLicenceIdentifier.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Env.CurrentCompany.GetLicenceCode()))
			{
				// Act
				var result = serverRegistrationRequestMessageProcessor.Process(Factory, configRequest);

				// Assert
				AssertEquals(1, result);
				AssertEquals(true, EDIDataRegistry.Instance.ActiveMiddlewareService.Value.HasChanges);
				AssertEquals(collection.Count + 1, EDIDataRegistry.Instance.ActiveMiddlewareService.Value.GetActiveCodeDescriptionPairList().Count);
				var codes = EDIDataRegistry.Instance.ActiveMiddlewareService.Value.GetCodeDescriptionPairList().Cast<ICodeDescription>().Select(code => code.Code);
				AssertContainsExactElementsInAnyOrder(expectedCodes, codes);
			}
		}

		public void TestNotSuitableMessageExitsInstantly()
		{
			CombineAssertions(() =>
			{
				Test(null);
				Test(string.Empty);
				Test("       ");
				Test("some inappropriate message");
			});

			void Test(string messageText)
			{
				// Arrange

				// Act
				var result = serverRegistrationRequestMessageProcessor.Process(Factory, messageText);

				// Assert
				AssertEquals(0, result);
				loggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.Never);
				loggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
			}
		}

		public void TestSendsToRightRecipient()
		{
			CombineAssertions(() =>
			{
				Test("123");
				Test("456");
			});

			void Test(string eHubId)
			{
				// Arrange
				eHubMessageSenderMock.Reset();
				var messageText = XmlDataSerializer.Serialize(new ServerRegistrationRequestMessage
				{
					EHubId = eHubId,
				});
				IEnumerable<string> recipients = null;
				eHubMessageSenderMock
					.Setup(sender => sender.Send(Factory, It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
					.Callback<BusinessObjectFactory, IEnumerable<string>, IEnumerable<string>>((factory, messages, recs) => recipients = recs);

				using (SystemDataRegistry.Instance.EdiProdLicenceIdentifier.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Env.CurrentCompany.GetLicenceCode()))
				{
					// Act
					serverRegistrationRequestMessageProcessor.Process(Factory, messageText);

					// Assert
					eHubMessageSenderMock.Verify(sender => sender.Send(It.IsAny<BusinessObjectFactory>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()), Times.Once);
					eHubMessageSenderMock.Verify(sender => sender.Send(Factory, It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()), Times.Once);
					AssertContainsExactElementsInAnyOrder(new[] { eHubId }, recipients);
				}
			}
		}

		[TestDate(2020, 05, 07, 16, 17, 51, 510)]
		public void TestSendsRightRegistrations()
		{
			// Arrange
			eHubMessageSenderMock.Reset();
			const string eHubId = "eHubId";
			var messageText = XmlDataSerializer.Serialize(new ServerRegistrationRequestMessage { EHubId = eHubId });
			IEnumerable<string> sentMessages = null;
			eHubMessageSenderMock
				.Setup(sender => sender.Send(Factory, It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
				.Callback<BusinessObjectFactory, IEnumerable<string>, IEnumerable<string>>((factory, messages, recipients) => sentMessages = messages);

			var licence = SetupLicence();

			var headerGood1 = CreateHeader();
			headerGood1.CDH_DeviceIdentifier = nameof(headerGood1);
			headerGood1.CDH_Description = "Model1";
			headerGood1.CDH_EnterpriseCode = licence.LicEnterprise.LE_EnterpriseCode;
			headerGood1.CDH_ServerCode = licence.LD_ServerCode;
			var headerApple = CreateHeader(ClientDeviceHeaderLookups.Kinds.AppleMobility);
			var headerTemplate = CreateHeader(isTemplate: true);
			var headerInactive = CreateHeader(status: ClientDeviceHeaderLookups.Statuses.Retired);
			var headerGood2 = CreateHeader();
			headerGood2.CDH_DeviceIdentifier = nameof(headerGood2);
			headerGood2.CDH_Description = "Model2";
			headerGood2.CDH_EnterpriseCode = licence.LicEnterprise.LE_EnterpriseCode;
			headerGood2.CDH_ServerCode = licence.LD_ServerCode;
			var headerGoodWithoutLicence = CreateHeader();
			Factory.Save();

			const string expectedMessage = @"<TelematicsXmlData>
	<DeviceAssignmentChangeMessage>
		<AssignDevicesToClients>
			<AssignDeviceToClient DeviceAssignmentTime=""2020-05-07 16:17:51.510 +00:00"" DeviceHardwareIdentifier=""headerGood1"" DeviceHumanReadableIdentifier=""TD00000001"" DeviceModel=""Model1"" CargoWiseOneLicense=""A12B34C56"" />
			<AssignDeviceToClient DeviceAssignmentTime=""2020-05-07 16:17:51.510 +00:00"" DeviceHardwareIdentifier=""headerGood2"" DeviceHumanReadableIdentifier=""TD00000004"" DeviceModel=""Model2"" CargoWiseOneLicense=""A12B34C56"" />
		</AssignDevicesToClients>
	</DeviceAssignmentChangeMessage>
</TelematicsXmlData>";

			using (SystemDataRegistry.Instance.EdiProdLicenceIdentifier.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Env.CurrentCompany.GetLicenceCode()))
			{
				// Act
				serverRegistrationRequestMessageProcessor.Process(Factory, messageText);

				// Assert
				eHubMessageSenderMock.Verify(sender => sender.Send(It.IsAny<BusinessObjectFactory>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()), Times.Once);
				eHubMessageSenderMock.Verify(sender => sender.Send(Factory, It.IsAny<IEnumerable<string>>(), It.Is<IEnumerable<string>>(enumerable => enumerable.Single().Equals(eHubId))), Times.Once);
				AssertXMLEquals(expectedMessage, sentMessages.Single());
			}

			ClientDeviceHeader CreateHeader(string deviceKind = ClientDeviceHeaderLookups.Kinds.WiseTechEmbedded, bool isTemplate = false, string status = ClientDeviceHeaderLookups.Statuses.Active)
			{
				var header = Factory.NewWithValidTestData<ClientDeviceHeader>();
				header.CDH_DeviceKind = deviceKind;
				header.CDH_IsTemplate = isTemplate;
				header.CDH_Status = status;
				return header;
			}

			LicenceDatabase SetupLicence()
			{
				var orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_Code = "TEST000";
				orgHeader.OH_FullName = "Test Organization";

				var licenceEnterprise = Factory.New<LicenceEnterprise>();
				licenceEnterprise.LE_EnterpriseCode = "A12";
				licenceEnterprise.LE_OH = orgHeader.PK;

				var licenceCompany = Factory.New<LicenceCompany>();
				licenceCompany.LC_CompanyCode = "B34";
				licenceCompany.LC_LE = licenceEnterprise.PK;
				licenceCompany.LC_OH = orgHeader.PK;

				var licenceDatabase = Factory.New<LicenceDatabase>();
				licenceDatabase.LD_ServerCode = "C56";
				licenceDatabase.LD_LE = licenceEnterprise.PK;

				var licenceHeader = Factory.New<LicenceHeader>();
				licenceHeader.LA_IsActive = true;
				licenceHeader.LA_LC = licenceCompany.PK;
				licenceHeader.LA_LD = licenceDatabase.PK;

				return licenceDatabase;
			}
		}

		public void TestWrongParamsCall()
		{
			CombineAssertions(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => new ServerRegistrationRequestMessageProcessor(null));
				AssertEquals("logger", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new ServerRegistrationRequestMessageProcessor(null, eHubMessageSenderMock.Object));
				AssertEquals("logger", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new ServerRegistrationRequestMessageProcessor(loggerMock.Object, null));
				AssertEquals("eHubMessageSender", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => serverRegistrationRequestMessageProcessor.Process(null, string.Empty));
				AssertEquals("factory", result.ParamName);
			});
		}

		Mock<IEHubMessageSender> eHubMessageSenderMock;
		Mock<ILogger> loggerMock;
		ServerRegistrationRequestMessageProcessor serverRegistrationRequestMessageProcessor;
	}
}
