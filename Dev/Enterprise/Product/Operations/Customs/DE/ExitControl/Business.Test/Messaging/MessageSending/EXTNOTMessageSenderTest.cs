using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	[TestedType(typeof(EXTNOTMessageSender))]
	sealed class EXTNOTMessageSenderTest : ExitControlMessageSenderTest<EXTNOTMessageSender>
	{
		protected override ZString ExpectedMessageType => nameof(DEXTNE);

		protected override bool LocalReferenceNumberExpected => true;
	}
}
