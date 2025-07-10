using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.GB.MessageContracts.EMCS.Version4_1;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.GB.EMCS.Business.Testing.Version4_1
{
	sealed class EMCSMessageBuilderLoaderTest : TestCaseWithFactory
	{
		public void TestCancellationOfEAD()
		{
			var emcsDataProvider = new Mock<IEMCSMessageHeader>();
			AssertType<IE810MessageBuilder>(EMCSMessageBuilderLoader.Instance.GetMessageBuilder(EMCSMessageBuilderLoader.CancellationOfEAD, emcsDataProvider.Object));
		}

		public void TestSubmitDraftEAD()
		{
			var emcsDataProvider = new Mock<IEMCSMessageHeader>();
			AssertType<IE815MessageBuilder>(EMCSMessageBuilderLoader.Instance.GetMessageBuilder(EMCSMessageBuilderLoader.SubmitDraftEAD, emcsDataProvider.Object));
		}

		public void TestReportOfReceipt()
		{
			var emcsDataProvider = new Mock<IEMCSMessageHeader>();
			AssertType<IE818MessageBuilder>(EMCSMessageBuilderLoader.Instance.GetMessageBuilder(EMCSMessageBuilderLoader.ReportOfReceipt, emcsDataProvider.Object));
		}

		public void TestChangeOfDestination()
		{
			var emcsDataProvider = new Mock<IEMCSMessageHeader>();
			AssertType<IE813MessageBuilder>(EMCSMessageBuilderLoader.Instance.GetMessageBuilder(EMCSMessageBuilderLoader.ChangeOfDestination, emcsDataProvider.Object));
		}

		public void TestExplanationOnDelayForDelivery()
		{
			var emcsDataProvider = new Mock<IEMCSMessageHeader>();
			AssertType<IE837MessageBuilder>(EMCSMessageBuilderLoader.Instance.GetMessageBuilder(EMCSMessageBuilderLoader.ExplanationOnDelayForDelivery, emcsDataProvider.Object));
		}

		public void TestExplanationOnReasonForShortage()
		{
			var emcsDataProvider = new Mock<IIE871MessageHeader>();
			AssertType<IE871MessageBuilder>(EMCSMessageBuilderLoader.Instance.GetMessageBuilder(EMCSMessageBuilderLoader.ExplanationOnReasonForShortage, emcsDataProvider.Object));
		}

		public void TestRejectionOfEAD()
		{
			var emcsDataProvider = new Mock<IEMCSMessageHeader>();
			AssertType<IE819MessageBuilder>(EMCSMessageBuilderLoader.Instance.GetMessageBuilder(EMCSMessageBuilderLoader.RejectionOfEAD, emcsDataProvider.Object));
		}

		public void TestSplitting()
		{
			var emcsDataProvider = new Mock<IEMCSMessageHeader>();
			AssertType<IE825MessageBuilder>(EMCSMessageBuilderLoader.Instance.GetMessageBuilder(EMCSMessageBuilderLoader.Splitting, emcsDataProvider.Object));
		}

		public void TestZDeveloperErrorForInvalidMessageBuilderRequest()
		{
			ErrorReporter.Clear();
			EMCSMessageBuilderLoader.Instance.GetMessageBuilder("INVALID", null);
			AssertEquals($"Invalid GB EMCS Message Builder for code: INVALID", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}
	}
}
