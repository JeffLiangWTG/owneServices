using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE839;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing.Phase4_1
{
	[TestedType(typeof(IE839MessageProcessor))]
	sealed class IE839MessageProcessorTest : IE839MessageProcessorAbstractTest<Ie839Type>
	{
		protected override ZString MessageTextMRN => EMCSMessageProcessorTestHelper.GetStandardIE839Text(true);

		protected override ZString MessageText => EMCSMessageProcessorTestHelper.GetStandardIE839Text(false);

		protected override string MessageSampleNameSpace => "Enterprise.Customs.IE.EMCS.Business.Testing.Message.MessageProcessors.Phase4_1.TestFiles.IE839MessageSample.txt";
	}
}
