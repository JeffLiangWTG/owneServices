using System;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.AESVersion3_0;
using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0;
using Enterprise.Customs.DE.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.ExitControl.Business.Testing
{
	sealed class ExitDeclarationMessageBuilderLoaderTest : TestCaseWithFactory
	{
		public void TestExitPresentation3_0()
		{
			AssertCorrectMessageBuilder<EXTPREMessageBuilder, DEXTPE, EXTPREMessageHeaderProvider>(AESVersionNumberList.Codes._30, ExitDeclarationMessageBuilderLoader.ExitPresentation);
		}

		public void TestExitAnticipation3_0()
		{
			AssertCorrectMessageBuilder<EXTANTMessageBuilder, DEXTAE, EXTANTMessageHeaderProvider>(AESVersionNumberList.Codes._30, ExitDeclarationMessageBuilderLoader.ExitAnticipation);
		}

		public void TestExitNotification3_0()
		{
			AssertCorrectMessageBuilder<EXTNOTMessageBuilder, DEXTNE, EXTNOTMessageHeaderProvider>(AESVersionNumberList.Codes._30, ExitDeclarationMessageBuilderLoader.ExitNotification);
		}

		public void TestExitInformation3_0()
		{
			AssertCorrectMessageBuilder<EXTINFMessageBuilder, DEXTIF, EXTINFMessageHeaderProvider>(AESVersionNumberList.Codes._30, ExitDeclarationMessageBuilderLoader.ExitInformation);
		}

		public void TestHasMessageBuildersForCurrentAESVersion()
		{
			CombineAssertions(() =>
			{
				var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AESSystemCode, VersionNumber = AESVersionNumberList.Codes._30 } };
				using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
				{
					AssertEquals("AESVersion 3.0", expected: true, ExitDeclarationMessageBuilderLoader.Instance.HasMessageBuildersForCurrentAESVersion);
				}

				var messageVersionRegistry = new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AESSystemCode };
				// AESVersionNumberList.Codes._40 is temporary not in VersionNumbers we have to suspend validation for test, remove it when it's back
				using (messageVersionRegistry.GetValidationSuspender())
				{
					messageVersionRegistry.VersionNumber = AESVersionNumberList.Codes._40;
				}
				messageVersionRegistryCollection = new MessageVersionRegistryCollection { messageVersionRegistry };

				using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
				{
					AssertEquals("AESVersion 4.0", expected: false, ExitDeclarationMessageBuilderLoader.Instance.HasMessageBuildersForCurrentAESVersion);
				}
			});
		}

		public void TestZDeveloperErrorForInvalidMessageBuilderRequest()
		{
			ErrorReporter.Clear();
			CombineAssertions(() =>
			{
				foreach (var aesVersionNumber in new AESVersionNumberList().GetAllCodes())
				{
					var messageVersionRegistry = new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AESSystemCode };
					// AESVersionNumberList.Codes._40 is temporary not in VersionNumbers we have to suspend validation for test, remove it when it's back
					using (messageVersionRegistry.GetValidationSuspender())
					{
						messageVersionRegistry.VersionNumber = aesVersionNumber;
					}
					var messageVersionRegistryCollection = new MessageVersionRegistryCollection { messageVersionRegistry };

					using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
					{
						ExitDeclarationMessageBuilderLoader.Instance.GetOutboundMessageDetailsForCurrentVersion("INVALID");
					}
					AssertEquals($"AESVersion {aesVersionNumber}", $"Invalid DE AES Message Builder for code: INVALID requested for AES Version {aesVersionNumber}", ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
				}
			});
		}

		void AssertCorrectMessageBuilder<T, S, P>(ZString aesVersionNumber, ZString messageCode)
			where T : MessageBuilder<S>
			where P : class
			where S : class
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AESSystemCode, VersionNumber = aesVersionNumber } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				CombineAssertions(() =>
				{
					var outboundMessageDetails = ExitDeclarationMessageBuilderLoader.Instance.GetOutboundMessageDetailsForCurrentVersion(messageCode);
					AssertEquals("MessageBuilder type", typeof(T), outboundMessageDetails.MessageBuilderType);
					AssertEquals("Provider type", typeof(P), outboundMessageDetails.ProviderType);
				});
			}
		}
	}
}
