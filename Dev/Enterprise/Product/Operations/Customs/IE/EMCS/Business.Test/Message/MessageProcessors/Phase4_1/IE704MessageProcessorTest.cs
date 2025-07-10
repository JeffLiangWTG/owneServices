using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE704;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing.Phase4_1
{
	[TestedType(typeof(IE704MessageProcessor))]
	sealed class IE704MessageProcessorTest : IE704MessageProcessorAbstractTest<Ie704Type>
	{
		protected override ZString MessageText => EMCSMessageProcessorTestHelper.GetStandardIE704Text();

		protected override string MessageSampleNameSpace => "Enterprise.Customs.IE.EMCS.Business.Testing.Message.MessageProcessors.Phase4_1.TestFiles.IE704MessageSample.txt";
	}
}
