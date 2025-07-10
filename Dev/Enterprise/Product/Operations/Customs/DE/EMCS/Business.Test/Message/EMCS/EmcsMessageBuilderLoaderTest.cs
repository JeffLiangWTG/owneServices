using System;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Registry;
using Enterprise.MasterFiles.Business;
using Moq;
using EMCSVersion2_4 = CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using EMCSVersion2_5 = CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	class EmcsMessageBuilderLoaderTest : TestCaseWithFactory
	{
		public void TestSubmittedDraftEAD_Version2_4()
		{
			var emcsDataProvider = new Mock<IEMCSMessageHeader>();
			AssertCorrectMessageBuilder<CargoWise.Customs.DE.MessageContracts.EMCS.Version2_4.ED815MessageBuilder, EMCSVersion2_4.ED815D>(EmcsVersionNumberList.Codes._24, EmcsMessageBuilderLoader.SubmittedDraftEAD, emcsDataProvider.Object);
		}

		public void TestExplanationOnDelay_Version2_4()
		{
			var emcsDataProvider = new Mock<IEMCSMessageHeader>();
			AssertCorrectMessageBuilder<CargoWise.Customs.DE.MessageContracts.EMCS.Version2_4.ED837MessageBuilder, EMCSVersion2_4.ED837B>(EmcsVersionNumberList.Codes._24, EmcsMessageBuilderLoader.ExplanationOnDelay, emcsDataProvider.Object);
		}

		public void TestCancellationOfEAD_Version2_4()
		{
			var emcsDataProvider = new Mock<IEMCSMessageHeader>();
			AssertCorrectMessageBuilder<CargoWise.Customs.DE.MessageContracts.EMCS.Version2_4.ED810MessageBuilder, EMCSVersion2_4.ED810C>(EmcsVersionNumberList.Codes._24, EmcsMessageBuilderLoader.CancellationOfEAD, emcsDataProvider.Object);
		}

		public void TestReportOfReceipt_Version2_4()
		{
			var emcsDataProvider = new Mock<IEMCSMessageHeader>();
			AssertCorrectMessageBuilder<CargoWise.Customs.DE.MessageContracts.EMCS.Version2_4.ED818MessageBuilder, EMCSVersion2_4.ED818C>(EmcsVersionNumberList.Codes._24, EmcsMessageBuilderLoader.ReportOfReceipt, emcsDataProvider.Object);
		}

		public void TestRejectionOfEAD_Version2_4()
		{
			var emcsDataProvider = new Mock<IEMCSMessageHeader>();
			AssertCorrectMessageBuilder<CargoWise.Customs.DE.MessageContracts.EMCS.Version2_4.ED819MessageBuilder, EMCSVersion2_4.ED819C>(EmcsVersionNumberList.Codes._24, EmcsMessageBuilderLoader.RejectionOfEAD, emcsDataProvider.Object);
		}

		public void TestExplanationForShortage_Version2_4()
		{
			var emcsDataProvider = new Mock<IED871MessageHeader>();
			AssertCorrectMessageBuilder<CargoWise.Customs.DE.MessageContracts.EMCS.Version2_4.ED871MessageBuilder, EMCSVersion2_4.ED871C>(EmcsVersionNumberList.Codes._24, EmcsMessageBuilderLoader.ExplanationForShortage, emcsDataProvider.Object);
		}

		public void TestChangeOfDestination_Version2_4()
		{
			var emcsDataProvider = new Mock<IEMCSMessageHeader>();
			AssertCorrectMessageBuilder<CargoWise.Customs.DE.MessageContracts.EMCS.Version2_4.ED813MessageBuilder, EMCSVersion2_4.ED813E>(EmcsVersionNumberList.Codes._24, EmcsMessageBuilderLoader.ChangeOfDestination, emcsDataProvider.Object);
		}

		public void TestSubmittedDraftEAD_Version2_5()
		{
			var emcsDataProvider = new Mock<IEMCSMessageHeader>();
			AssertCorrectMessageBuilder<CargoWise.Customs.DE.MessageContracts.EMCS.Version2_5.ED815MessageBuilder, EMCSVersion2_5.ED815E>(EmcsVersionNumberList.Codes._25, EmcsMessageBuilderLoader.SubmittedDraftEAD, emcsDataProvider.Object);
		}

		public void TestExplanationOnDelay_Version2_5()
		{
			var emcsDataProvider = new Mock<IEMCSMessageHeader>();
			AssertCorrectMessageBuilder<CargoWise.Customs.DE.MessageContracts.EMCS.Version2_5.ED837MessageBuilder, EMCSVersion2_5.ED837C>(EmcsVersionNumberList.Codes._25, EmcsMessageBuilderLoader.ExplanationOnDelay, emcsDataProvider.Object);
		}

		public void TestCancellationOfEAD_Version2_5()
		{
			var emcsDataProvider = new Mock<IEMCSMessageHeader>();
			AssertCorrectMessageBuilder<CargoWise.Customs.DE.MessageContracts.EMCS.Version2_5.ED810MessageBuilder, EMCSVersion2_5.ED810C>(EmcsVersionNumberList.Codes._25, EmcsMessageBuilderLoader.CancellationOfEAD, emcsDataProvider.Object);
		}

		public void TestReportOfReceipt_Version2_5()
		{
			var emcsDataProvider = new Mock<IEMCSMessageHeader>();
			AssertCorrectMessageBuilder<CargoWise.Customs.DE.MessageContracts.EMCS.Version2_5.ED818MessageBuilder, EMCSVersion2_5.ED818D>(EmcsVersionNumberList.Codes._25, EmcsMessageBuilderLoader.ReportOfReceipt, emcsDataProvider.Object);
		}

		public void TestRejectionOfEAD_Version2_5()
		{
			var emcsDataProvider = new Mock<IEMCSMessageHeader>();
			AssertCorrectMessageBuilder<CargoWise.Customs.DE.MessageContracts.EMCS.Version2_5.ED819MessageBuilder, EMCSVersion2_5.ED819D>(EmcsVersionNumberList.Codes._25, EmcsMessageBuilderLoader.RejectionOfEAD, emcsDataProvider.Object);
		}

		public void TestExplanationForShortage_Version2_5()
		{
			var emcsDataProvider = new Mock<IED871MessageHeader>();
			AssertCorrectMessageBuilder<CargoWise.Customs.DE.MessageContracts.EMCS.Version2_5.ED871MessageBuilder, EMCSVersion2_5.ED871D>(EmcsVersionNumberList.Codes._25, EmcsMessageBuilderLoader.ExplanationForShortage, emcsDataProvider.Object);
		}

		public void TestChangeOfDestination_Version2_5()
		{
			var emcsDataProvider = new Mock<IEMCSMessageHeader>();
			AssertCorrectMessageBuilder<CargoWise.Customs.DE.MessageContracts.EMCS.Version2_5.ED813MessageBuilder, EMCSVersion2_5.ED813F>(EmcsVersionNumberList.Codes._25, EmcsMessageBuilderLoader.ChangeOfDestination, emcsDataProvider.Object);
		}

		public void TestZDeveloperErrorForInvalidMessageBuilderRequest()
		{
			ErrorReporter.Clear();
			CombineAssertions(() =>
			{
				foreach (var emcsVersion in new EmcsVersionNumberList().GetAllCodes())
				{
					var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.EmcsSystemCode, VersionNumber = emcsVersion } };
					using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
					{
						EmcsMessageBuilderLoader.Instance.GetMessageBuilder("INVALID", null);
					}
					AssertEquals($"EMCSVersion {emcsVersion}", $"Invalid DE EMCS Message Builder for code: INVALID requested for EMCS Version {emcsVersion}", ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
				}
			});
		}

		void AssertCorrectMessageBuilder<T, S>(ZString emcsVersionNumber, ZString messageCode, IEMCSMessageHeader dataProvider)
			where T : MessageBuilder<S> where S : class
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.EmcsSystemCode, VersionNumber = emcsVersionNumber } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				AssertType<T>(EmcsMessageBuilderLoader.Instance.GetMessageBuilder(messageCode, dataProvider));
			}
		}
	}
}
