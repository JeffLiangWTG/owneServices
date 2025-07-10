using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	[TestedType(typeof(EXTPREMessageSender))]
	sealed class EXTPREMessageSenderTest : ExitControlMessageSenderTest<EXTPREMessageSender>
	{
		protected override ZString ExpectedMessageType => nameof(DEXTPE);

		protected override bool LocalReferenceNumberExpected => true;
	}
}
