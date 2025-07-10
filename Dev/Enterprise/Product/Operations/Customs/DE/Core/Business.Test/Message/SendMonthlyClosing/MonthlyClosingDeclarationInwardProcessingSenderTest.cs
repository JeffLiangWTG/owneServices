using CargoWise.Types;
using NUnit.Framework;
using ATLASVersion10_1 = CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(MonthlyClosingDeclarationInwardProcessingSender))]
	class MonthlyClosingDeclarationInwardProcessingSenderTest : MonthlyClosingDeclarationSenderAbstractTest<MonthlyClosingDeclarationInwardProcessingSender>
	{
		public void TestHasInformationToSend_NoInformation_101()
		{
			AssertHasInformationToSend_NoInformation_NoCusReconEntryLines_101();
		}

		public void TestHasInformationToSend_101()
		{
			AssertHasInformationToSend_HasInformation_HasCusReconEntryLines_101();
		}

		protected override ZString ExpectedMessageTypeATLASVersion10_1 => nameof(ATLASVersion10_1.VSCIPK);

		protected override ZString ExpectedMessageTypeATLASVersion10_2 => ZString.Empty;

		protected override ZString ExpectedMessageSubType => Messaging.MonthlyClosingMessageSubTypeList.Codes.MonthlyClosingInwardProcessing;

		protected override MonthlyClosingDeclarationInwardProcessingSender GetMonthlyClosingDeclarationSender(CusReconDeclaration declaration)
			=> new MonthlyClosingDeclarationInwardProcessingSender(declaration, Messaging.MonthlyClosingMessageRoleList.Codes.FirstPartialMessage);
	}
}
