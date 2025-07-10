using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE917;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing.Phase4_1
{
	[TestedType(typeof(IE917MessageProcessor))]
	sealed class IE917MessageProcessorTest : IE917MessageProcessorAbstractTest<Ie917Type>
	{
		protected override ZString MessageText => EMCSMessageProcessorTestHelper.GetStandardIE917Text();

		protected override string MessageSampleNameSpace => "Enterprise.Customs.IE.EMCS.Business.Testing.Message.MessageProcessors.Phase4_1.TestFiles.IE917MessageSample.txt";
	}
}
