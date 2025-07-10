using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE801;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing.Phase4_1
{
	[TestedType(typeof(IE801MessageProcessor))]
	sealed class IE801MessageProcessorTest : IE801MessageProcessorAbstractTest<Ie801Type>
	{
		protected override ZString MessageText => EMCSMessageProcessorTestHelper.GetStandardIE801Text();

		protected override string MessageSampleNameSpace => "Enterprise.Customs.IE.EMCS.Business.Testing.Message.MessageProcessors.Phase4_1.TestFiles.IE801MessageSample.txt";
	}
}
