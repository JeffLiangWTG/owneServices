using System;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.DE.Registry;
using Enterprise.MasterFiles.Business;
using Moq;
using ATLASVersion10_1 = CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using DETemporaryStorage = CargoWise.Customs.DE.MessageContracts.TemporaryStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	sealed class TemporaryStorageMessageBuilderLoaderTest : TestCaseWithFactory
	{
		public void TestChangeCustodyInformation10_1()
		{
			var storageDecProvider = new Mock<ICHGTSTTempStorageDec>();
			storageDecProvider.Setup(x => x.IdentificationIndicator).Returns(TemporaryStorageIdentificationIndicatorList.Codes.AWB);
			AssertCorrectMessageBuilder<DETemporaryStorage.ATLASVersion10_1.CHGTSTMessageBuilder, ATLASVersion10_1.SCHTSG>(ATLASVersionNumberList.Codes._101, TemporaryStorageMessageBuilderLoader.ChangeCustodyInformation, storageDecProvider.Object);
		}

		public void TestChangeDisposalEntitledTrader10_1()
		{
			var storageDecProvider = new Mock<ICHGOFFTempStorageDec>();
			storageDecProvider.Setup(x => x.IdentificationIndicator).Returns(TemporaryStorageIdentificationIndicatorList.Codes.AWB);
			AssertCorrectMessageBuilder<DETemporaryStorage.ATLASVersion10_1.CHGOFFMessageBuilder, ATLASVersion10_1.SCHOFG>(ATLASVersionNumberList.Codes._101, TemporaryStorageMessageBuilderLoader.ChangeDisposalEntitledTrader, storageDecProvider.Object);
		}

		public void TestChangeOwnerReference10_1()
		{
			var storageDecProvider = new Mock<ICHGSPOTempStorageDec>();
			AssertCorrectMessageBuilder<DETemporaryStorage.ATLASVersion10_1.CHGSPOMessageBuilder, ATLASVersion10_1.SCHSPE>(ATLASVersionNumberList.Codes._101, TemporaryStorageMessageBuilderLoader.ChangeOwnerReference, storageDecProvider.Object);
		}

		public void TestConsolidation10_1()
		{
			var storageDecProvider = new Mock<IPRLCONTempStorageDec>();
			storageDecProvider.Setup(x => x.IdentificationIndicator).Returns(TemporaryStorageIdentificationIndicatorList.Codes.AWB);
			AssertCorrectMessageBuilder<DETemporaryStorage.ATLASVersion10_1.PRLCONMessageBuilder, ATLASVersion10_1.SPCONH>(ATLASVersionNumberList.Codes._101, TemporaryStorageMessageBuilderLoader.Consolidation, storageDecProvider.Object);
		}

		public void TestSplit10_1()
		{
			var storageDecProvider = new Mock<ITempStorageDec>();
			storageDecProvider.Setup(x => x.IdentificationIndicator).Returns(TemporaryStorageIdentificationIndicatorList.Codes.AWB);
			AssertCorrectMessageBuilder<DETemporaryStorage.ATLASVersion10_1.CUSPCSMessageBuilder, ATLASVersion10_1.SCPCSH>(ATLASVersionNumberList.Codes._101, TemporaryStorageMessageBuilderLoader.Split, storageDecProvider.Object);
		}

		public void TestAmendmentSumA10_1()
		{
			var storageDecProvider = new Mock<ICUSPRLTempStorageDec>();
			AssertCorrectMessageBuilder<DETemporaryStorage.ATLASVersion10_1.CUSPRLMessageBuilder, ATLASVersion10_1.SCPRLK>(ATLASVersionNumberList.Codes._101, TemporaryStorageMessageBuilderLoader.AmendmentSumA, storageDecProvider.Object);
		}

		public void TestFinalSumAWithAPreliminary10_1()
		{
			var storageDecProvider = new Mock<ICUSPRLTempStorageDec>();
			AssertCorrectMessageBuilder<DETemporaryStorage.ATLASVersion10_1.CUSPRLMessageBuilder, ATLASVersion10_1.SCPRLK>(ATLASVersionNumberList.Codes._101, TemporaryStorageMessageBuilderLoader.FinalSumAWithAPreliminary, storageDecProvider.Object);
		}

		public void TestFinalSumAWithoutPreliminary10_1()
		{
			var storageDecProvider = new Mock<ICUSPRLTempStorageDec>();
			AssertCorrectMessageBuilder<DETemporaryStorage.ATLASVersion10_1.CUSPRLMessageBuilder, ATLASVersion10_1.SCPRLK>(ATLASVersionNumberList.Codes._101, TemporaryStorageMessageBuilderLoader.FinalSumAWithoutPreliminary, storageDecProvider.Object);
		}

		public void TestPreliminarySumA10_1()
		{
			var storageDecProvider = new Mock<ICUSPRLTempStorageDec>();
			AssertCorrectMessageBuilder<DETemporaryStorage.ATLASVersion10_1.CUSPRLMessageBuilder, ATLASVersion10_1.SCPRLK>(ATLASVersionNumberList.Codes._101, TemporaryStorageMessageBuilderLoader.PreliminarySumA, storageDecProvider.Object);
		}

		public void TestReExport10_1()
		{
			var storageDecProvider = new Mock<IREXDISTempStorageDec>();
			storageDecProvider.Setup(x => x.IdentificationIndicator).Returns(TemporaryStorageIdentificationIndicatorList.Codes.AWB);
			AssertCorrectMessageBuilder<DETemporaryStorage.ATLASVersion10_1.REXDISMessageBuilder, ATLASVersion10_1.SREXDJ>(ATLASVersionNumberList.Codes._101, TemporaryStorageMessageBuilderLoader.ReExport, storageDecProvider.Object);
		}

		public void TestZDeveloperErrorForInvalidMessageBuilderRequest()
		{
			ErrorReporter.Clear();
			CombineAssertions(() =>
			{
				foreach (var atlasVersion in new ATLASVersionNumberList().GetAllCodes())
				{
					var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = atlasVersion } };
					var validationErrorMessage = DECustomsDataRegistry.Instance.CustomsMessageVersion.GetValidationErrorMessage(messageVersionRegistryCollection, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
					if (validationErrorMessage.Contains("Enter a valid Version."))
					{
						continue;
					}
					using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
					{
						TemporaryStorageMessageBuilderLoader.Instance.GetMessageBuilder("INVALID", null);
					}
					AssertEquals($"ATLASVersion {atlasVersion}", $"Invalid DE Temporary Storage Message Builder for code: INVALID requested for ATLAS Version {atlasVersion}", ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
				}
			});
		}

		void AssertCorrectMessageBuilder<T, S>(ZString atlasVersionNumber, ZString messageCode, ITempStorageDec dataProvider)
			where T : MessageBuilder<S> where S : class
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = atlasVersionNumber } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				AssertType<T>("Message Builder " + atlasVersionNumber, TemporaryStorageMessageBuilderLoader.Instance.GetMessageBuilder(messageCode, dataProvider));
			}
		}
	}
}
