using System;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Registry;
using Enterprise.MasterFiles.Business;
using MessageDefinitionsAESVersion3_0 = CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ExportDeclarationMessageBuilderLoaderTest : TestCaseWithFactory
	{
		public void TestAmendment3_0()
		{
			AssertCorrectMessageBuilder<Messaging.AESVersion3_0.EXPAMDMessageBuilder, MessageDefinitionsAESVersion3_0.DEXPAE, AESVersion3_0.EXPAMDMessageHeaderProvider>(AESVersionNumberList.Codes._30, ExportDeclarationMessageBuilderLoader.Amendment);
		}

		public void TestCancellationRequest3_0()
		{
			AssertCorrectMessageBuilder<CargoWise.Customs.DE.MessageContracts.AESVersion3_0.EXPINVMessageBuilder, MessageDefinitionsAESVersion3_0.DEXPCD, AESVersion3_0.EXPINVMessageHeaderProvider>(AESVersionNumberList.Codes._30, ExportDeclarationMessageBuilderLoader.CancellationRequest);
		}

		public void TestExportData3_0()
		{
			AssertCorrectMessageBuilder<Messaging.AESVersion3_0.EXPDATMessageBuilder, MessageDefinitionsAESVersion3_0.DEXPDF, AESVersion3_0.EXPDATMessageHeaderProvider>(AESVersionNumberList.Codes._30, ExportDeclarationMessageBuilderLoader.ExportData);
		}

		public void TestEntireData3_0()
		{
			AssertCorrectMessageBuilder<CargoWise.Customs.DE.MessageContracts.AESVersion3_0.EXPENTMessageBuilder, MessageDefinitionsAESVersion3_0.DEXPEE, AESVersion3_0.EXPENTMessageHeaderProvider>(AESVersionNumberList.Codes._30, ExportDeclarationMessageBuilderLoader.EntireData);
		}

		public void TestExportExit3_0()
		{
			AssertCorrectMessageBuilder<CargoWise.Customs.DE.MessageContracts.AESVersion3_0.EXPEXTMessageBuilder, MessageDefinitionsAESVersion3_0.DEXPXC, AESVersion3_0.EXPEXTMessageHeaderProvider>(AESVersionNumberList.Codes._30, ExportDeclarationMessageBuilderLoader.ExportExit);
		}

		public void TestStatusRequest3_0()
		{
			AssertCorrectMessageBuilder<CargoWise.Customs.DE.MessageContracts.AESVersion3_0.EXQQUEMessageBuilder, MessageDefinitionsAESVersion3_0.DEXQQB, AESVersion3_0.EXQQUEProvider>(AESVersionNumberList.Codes._30, ExportDeclarationMessageBuilderLoader.StatusRequest);
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
						ExportDeclarationMessageBuilderLoader.Instance.GetOutboundMessageDetailsForCurrentVersion("INVALID");
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
					var outboundMessageDetails = ExportDeclarationMessageBuilderLoader.Instance.GetOutboundMessageDetailsForCurrentVersion(messageCode);
					AssertEquals("MessageBuilder", typeof(T), outboundMessageDetails.MessageBuilderType);
					AssertEquals("MessageBuilder", typeof(P), outboundMessageDetails.ProviderType);
				});
			}
		}
	}
}
