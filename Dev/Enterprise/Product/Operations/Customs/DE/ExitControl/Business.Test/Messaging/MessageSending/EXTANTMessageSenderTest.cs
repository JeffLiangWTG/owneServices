using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	[TestedType(typeof(EXTANTMessageSender))]
	sealed class EXTANTMessageSenderTest : ExitControlMessageSenderTest<EXTANTMessageSender>
	{
		protected override ZString ExpectedMessageType => nameof(DEXTAE);

		protected override bool LocalReferenceNumberExpected => true;
	}
}
