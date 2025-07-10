using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE818;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing.Phase4_1
{
	[TestedType(typeof(IE818MessageProcessor))]
	sealed class IE818MessageProcessorTest : IE818MessageProcessorAbstractTest<Ie818Type>
	{
		protected override ZString MessageText => EMCSMessageProcessorTestHelper.GetStandardIE818Text();

		protected override string MessageSampleNameSpace => "Enterprise.Customs.IE.EMCS.Business.Testing.Message.MessageProcessors.Phase4_1.TestFiles.IE818MessageSample.txt";
	}
}
