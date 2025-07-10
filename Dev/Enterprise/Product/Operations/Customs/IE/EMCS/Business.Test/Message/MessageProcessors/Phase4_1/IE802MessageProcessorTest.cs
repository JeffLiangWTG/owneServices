using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE802;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing.Phase4_1
{
	[TestedType(typeof(IE802MessageProcessor))]
	sealed class IE802MessageProcessorTest : IE802MessageProcessorAbstractTest<Ie802Type>
	{
		protected override ZString MessageText => EMCSMessageProcessorTestHelper.GetStandardIE802Text();

		protected override string MessageSampleNameSpace => "Enterprise.Customs.IE.EMCS.Business.Testing.Message.MessageProcessors.Phase4_1.TestFiles.IE802MessageSample.txt";
	}
}
