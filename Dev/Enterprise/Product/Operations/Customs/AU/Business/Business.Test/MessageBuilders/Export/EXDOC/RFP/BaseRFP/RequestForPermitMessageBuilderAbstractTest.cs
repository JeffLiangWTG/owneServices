using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class RequestForPermitMessageBuilderAbstractTest : TestCaseWithFactory
	{
		public virtual void TestRFPMessageBuilder()
		{
			var messageBuilder = GetMessageBuilder;
			messageBuilder.GenerateRFPMessage();
			AssertMultilineEquals("Check message matches", ExpectedRFPMessage, messageBuilder.MessageTextForTesting, '\'');
		}

		public void TestRFPSAmendmentMessageBuilder()
		{
			var messageBuilder = GetAmendmentMessageBuilder;
			if (messageBuilder == null)
			{
				AssertEquals(ZString.Empty, ExpectedAmendmentRFPMessage);
			}
			else
			{
				messageBuilder.GenerateRFPMessage();
				AssertMultilineEquals("Check message matches", ExpectedAmendmentRFPMessage, messageBuilder.MessageTextForTesting, '\'');
			}
		}

		public void TestWithdrawlMessageBuilder()
		{
			var messageBuilder = GetWithdrawlMessageBuilder;
			messageBuilder.GenerateWithdrawlRFPMessage();
			AssertMultilineEquals("Check message matches", ExpectedWithdrawlRFPMessage, messageBuilder.MessageTextForTesting, '\'');
		}

		public void TestTransferMessageBuilder()
		{
			var messageBuilder = GetTransferMessageBuilder;
			messageBuilder.GenerateTransferRFPMessage();
			AssertMultilineEquals("Check message matches", ExpectedTransferRFPMessage, messageBuilder.MessageTextForTesting, '\'');
		}

		public void TestCopyMessageBuilder()
		{
			var messageBuilder = GetCopyMessageBuilder;
			messageBuilder.GenerateCopyRFPMessage();
			AssertMultilineEquals("Check message matches", ExpectedCopyRFPMessage, messageBuilder.MessageTextForTesting, '\'');
		}

		public void TestAcceptTransferInMessage()
		{
			var messageBuilder = GetAcceptTransferInMessageBuilder;
			messageBuilder.GenerateTransferInRFPMessage();
			AssertMultilineEquals("Check message matches", ExpectedAcceptedTransferInRFPMessage, messageBuilder.MessageTextForTesting, '\'');
		}

		public void TestDeclineTransferInMessage()
		{
			var messageBuilder = GetDeclinetTransferInMessageBuilder;
			messageBuilder.GenerateTransferInRFPMessage();
			AssertMultilineEquals("Check message matches", ExpectedDeclinedTransferInRFPMessage, messageBuilder.MessageTextForTesting, '\'');
		}

		protected abstract RequestForPermitHeaderMessageBuilder GetMessageBuilder { get; }

		protected virtual RequestForPermitHeaderMessageBuilder GetAmendmentMessageBuilder => null;

		protected abstract RequestForPermitHeaderMessageBuilder GetWithdrawlMessageBuilder { get; }

		protected abstract RequestForPermitHeaderMessageBuilder GetTransferMessageBuilder { get; }

		protected abstract RequestForPermitHeaderMessageBuilder GetCopyMessageBuilder { get; }

		protected abstract RequestForPermitHeaderMessageBuilder GetAcceptTransferInMessageBuilder { get; }

		protected abstract RequestForPermitHeaderMessageBuilder GetDeclinetTransferInMessageBuilder { get; }

		protected abstract ZString ExpectedRFPMessage { get; }

		protected virtual ZString ExpectedAmendmentRFPMessage => ZString.Empty;

		protected abstract ZString ExpectedWithdrawlRFPMessage { get; }

		protected abstract ZString ExpectedTransferRFPMessage { get; }

		protected abstract ZString ExpectedCopyRFPMessage { get; }

		protected abstract ZString ExpectedAcceptedTransferInRFPMessage { get; }

		protected abstract ZString ExpectedDeclinedTransferInRFPMessage { get; }

		protected override void SetUp()
		{
			base.SetUp();
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata51, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata51, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false);
		}
	}
}
