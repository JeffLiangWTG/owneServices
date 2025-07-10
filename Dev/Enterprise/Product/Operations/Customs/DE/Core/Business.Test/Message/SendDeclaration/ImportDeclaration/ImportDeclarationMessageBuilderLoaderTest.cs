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
	sealed class ImportDeclarationMessageBuilderLoaderTest : TestCaseWithFactory
	{
		public void TestImportDeclarationConfirmation10_1()
		{
			AssertCorrectMessageBuilder<Messaging.ATLASVersion10_1.CUSCONMessageBuilder, ATLASVersion10_1.GCCONJ>(ATLASVersionNumberList.Codes._101, ImportDeclarationMessageBuilderLoader.ImportDeclarationConfirmation);
		}

		public void TestSingleDeclarationFreeCirculation10_1()
		{
			AssertCorrectMessageBuilder<Messaging.ATLASVersion10_1.CFCDECMessageBuilder, ATLASVersion10_1.FCFCDF>(ATLASVersionNumberList.Codes._101, ImportDeclarationMessageBuilderLoader.SingleDeclarationFreeCirculation);
		}

		public void TestSimplifiedDeclarationCustomsWarehousing10_1()
		{
			AssertCorrectMessageBuilder<Messaging.ATLASVersion10_1.SCWRECMessageBuilder, ATLASVersion10_1.LSCWRL>(ATLASVersionNumberList.Codes._101, ImportDeclarationMessageBuilderLoader.SimplifiedDeclarationIntoBondedWarehouse);
		}

		public void TestSingleDeclarationIntoBondedWarehouse10_1()
		{
			AssertCorrectMessageBuilder<Messaging.ATLASVersion10_1.SCWDECMessageBuilder, ATLASVersion10_1.LSCWDL>(ATLASVersionNumberList.Codes._101, ImportDeclarationMessageBuilderLoader.SingleDeclarationIntoBondedWarehouse);
		}

		public void TestSingleDeclarationForInwardProcessing10_1()
		{
			AssertCorrectMessageBuilder<Messaging.ATLASVersion10_1.SCIDECMessageBuilder, ATLASVersion10_1.VSCIDC>(ATLASVersionNumberList.Codes._101, ImportDeclarationMessageBuilderLoader.SingleDeclarationOutwardProcessing);
		}

		public void TestSimplifiedDeclarationForInwardProcessing10_1()
		{
			AssertCorrectMessageBuilder<Messaging.ATLASVersion10_1.SCIRECMessageBuilder, ATLASVersion10_1.VSCIRJ>(ATLASVersionNumberList.Codes._101, ImportDeclarationMessageBuilderLoader.SimplifiedDeclarationForInwardProcessing);
		}

		public void TestSimplifiedDeclarationIntoFreeCirculation10_1()
		{
			AssertCorrectMessageBuilder<Messaging.ATLASVersion10_1.CFCRECMessageBuilder, ATLASVersion10_1.FCFCRF>(ATLASVersionNumberList.Codes._101, ImportDeclarationMessageBuilderLoader.SimplifiedDeclarationIntoFreeCirculation);
		}

		public void TestWarehouseStockTransfer10_1()
		{
			AssertCorrectMessageBuilder<Messaging.ATLASVersion10_1.CUSWATMessageBuilder, ATLASVersion10_1.LCUSWK>(ATLASVersionNumberList.Codes._101, ImportDeclarationMessageBuilderLoader.WarehouseStockTransfer);
		}

		public void TestCollectiveClearanceBondedWarehouse10_1()
		{
			AssertCorrectMessageBuilder<Messaging.ATLASVersion10_1.ECWCCMMessageBuilder, ATLASVersion10_1.LECWCG>(ATLASVersionNumberList.Codes._101, ImportDeclarationMessageBuilderLoader.CollectiveClearanceBondedWarehouse);
		}

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
						ImportDeclarationMessageBuilderLoader.Instance.GetMessageBuilder("INVALID", null);
					}
					AssertEquals($"ATLASVersion {atlasVersionNumber}", $"Invalid DE Import Message Builder for code: INVALID requested for ATLAS Version {atlasVersionNumber}", ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
				}
			});
		}

		void AssertCorrectMessageBuilder<T, S>(ZString atlasVersionNumber, ZString messageCode)
			where T : MessageBuilder<S> where S : class
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = atlasVersionNumber } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				AssertType<T>(ImportDeclarationMessageBuilderLoader.Instance.GetMessageBuilder(messageCode, new Mock<IImportMessageHeader>().Object));
			}
		}
	}
}
