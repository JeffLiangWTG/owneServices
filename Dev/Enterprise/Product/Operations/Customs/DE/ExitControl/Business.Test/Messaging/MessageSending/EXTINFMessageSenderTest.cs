using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	[TestedType(typeof(EXTINFMessageSender))]
	sealed class EXTINFMessageSenderTest : ExitControlMessageSenderTest<EXTINFMessageSender>
	{
		protected override ZString ExpectedMessageType => nameof(DEXTIF);

		protected override bool LocalReferenceNumberExpected => true;
	}
}
