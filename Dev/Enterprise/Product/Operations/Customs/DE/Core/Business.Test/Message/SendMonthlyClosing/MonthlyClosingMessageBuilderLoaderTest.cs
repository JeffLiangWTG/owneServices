using System;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Registry;
using Enterprise.MasterFiles.Business;
using Moq;
using ATLASVersion10_1 = CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class MonthlyClosingMessageBuilderLoaderTest : TestCaseWithFactory
	{
		public void TestZDeveloperErrorForInvalidMessageBuilderRequest()
		{
			ErrorReporter.Clear();
			CombineAssertions(() =>
			{
				foreach (var atlasVersionNumber in new ATLASVersionNumberList().GetAllCodes())
				{
					var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = atlasVersionNumber } };
					var validationErrorMessage = DECustomsDataRegistry.Instance.CustomsMessageVersion.GetValidationErrorMessage(messageVersionRegistryCollection, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
					if (validationErrorMessage.Contains("Enter a valid Version."))
					{
						continue;
					}
					using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
					{
						MonthlyClosingMessageBuilderLoader.Instance.GetMessageBuilder("INVALID", null);
					}
					AssertEquals($"ATLASVersion {atlasVersionNumber}", $"Invalid DE MonthlyClosing Message Builder for code: INVALID requested for ATLAS Version {atlasVersionNumber}", ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
				}
			});
		}

		public void TestMonthlyClosingFreeCirculation10_1()
		{
			AssertCorrectMessageBuilder<Messaging.ATLASVersion10_1.CFCPEDMessageBuilder, ATLASVersion10_1.ECFCPF>(ATLASVersionNumberList.Codes._101, MonthlyClosingMessageBuilderLoader.MonthlyClosingFreeCirculation);
		}

		public void TestMonthlyClosingInwardProcessing10_1()
		{
			AssertCorrectMessageBuilder<Messaging.ATLASVersion10_1.SCIPEDMessageBuilder, ATLASVersion10_1.VSCIPK>(ATLASVersionNumberList.Codes._101, MonthlyClosingMessageBuilderLoader.MonthlyClosingInwardProcessing);
		}

		public void TestMonthlyClosingCustomsWarehouse10_1()
		{
			AssertCorrectMessageBuilder<Messaging.ATLASVersion10_1.SCWPEDMessageBuilder, ATLASVersion10_1.LSCWPM>(ATLASVersionNumberList.Codes._101, MonthlyClosingMessageBuilderLoader.MonthlyClosingBondedWarehouse);
		}

		void AssertCorrectMessageBuilder<T, S>(ZString atlasVersionNumber, ZString messageCode)
			where T : MessageBuilder<S> where S : class
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = atlasVersionNumber } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				AssertType<T>(MonthlyClosingMessageBuilderLoader.Instance.GetMessageBuilder(messageCode, new Mock<IImportMessageHeader>().Object));
			}
		}
	}
}
