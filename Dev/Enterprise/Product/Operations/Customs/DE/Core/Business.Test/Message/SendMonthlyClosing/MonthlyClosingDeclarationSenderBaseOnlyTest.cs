using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using NUnit.Framework;
using ATLASVersion10_1 = CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(MonthlyClosingDeclarationSenderForTest))]
	class MonthlyClosingDeclarationSenderBaseOnlyTest : MonthlyClosingDeclarationSenderAbstractTest<MonthlyClosingDeclarationSenderForTest>
	{
		public void TestFinalizationFlagNote_FirstPartialMessage_101()
		{
			AssertHasFinalizationFlagNote_101(MonthlyClosingMessageRoleList.Codes.FirstPartialMessage, MonthlyClosingHelper.DeclarationNotFinalizedFlag);
		}

		public void TestFinalizationFlagNote_AmendmentMessage_101()
		{
			AssertHasFinalizationFlagNote_101(MonthlyClosingMessageRoleList.Codes.AmendmentMessage, MonthlyClosingHelper.DeclarationNotFinalizedFlag);
		}

		public void TestFinalizationFlagNote_FinalMessage_101()
		{
			AssertHasFinalizationFlagNote_101(MonthlyClosingMessageRoleList.Codes.FinalMessage, MonthlyClosingHelper.DeclarationIsFinalizedFlag);
		}

		public void TestFinalizationFlagNote_FinalizationMessage_101()
		{
			AssertHasFinalizationFlagNote_101(MonthlyClosingMessageRoleList.Codes.FinalMessage, MonthlyClosingHelper.DeclarationIsFinalizedFlag);
		}

		public void TestFinalizationFlagNote_ModificationMessage_DoesNotCreateNote_101()
		{
			DoUsingATLAS101(() =>
			{
				var sender = new MonthlyClosingDeclarationSenderForTest(declaration, MonthlyClosingMessageRoleList.Codes.ModificationMessage);
				sender.Send();
				AssertNull(declaration.Notes.FindByDescription(MonthlyClosingHelper.FinalizationFlagNoteDescription).SingleOrDefault());
			});
		}

		void AssertHasFinalizationFlagNote_101(string messageRole, string expectedFinalizationFlagNote)
		{
			DoUsingATLAS101(() =>
			{
				var sender = new MonthlyClosingDeclarationSenderForTest(declaration, messageRole);
				sender.Send();
				AssertEquals(expectedFinalizationFlagNote, declaration.GetFinalizationFlagNote());
			});
		}

		protected override ZString ExpectedMessageTypeATLASVersion10_2 => ZString.Empty;

		protected override ZString ExpectedMessageTypeATLASVersion10_1 => nameof(ATLASVersion10_1.ECFCPF);

		protected override ZString ExpectedMessageSubType => MonthlyClosingMessageSubTypeList.Codes.MonthlyClosingFreeCirculation;

		protected override MonthlyClosingDeclarationSenderForTest GetMonthlyClosingDeclarationSender(CusReconDeclaration declaration)
			=> new MonthlyClosingDeclarationSenderForTest(declaration, MonthlyClosingMessageRoleList.Codes.FirstPartialMessage);
	}

	class MonthlyClosingDeclarationSenderForTest : MonthlyClosingDeclarationSender
	{
		public MonthlyClosingDeclarationSenderForTest(CusReconDeclaration declaration, ZString messageRole)
			: base(declaration, MonthlyClosingMessageBuilderLoader.MonthlyClosingFreeCirculation, new CFCPEDMessageHeaderProvider(declaration, messageRole), messageRole)
		{
		}

		public override bool MessageHasInformationToSend => true;

		protected override IEnumerable<int> LineNumbersInMessage10_1 => new[] { 1, 2 };
	}
}
