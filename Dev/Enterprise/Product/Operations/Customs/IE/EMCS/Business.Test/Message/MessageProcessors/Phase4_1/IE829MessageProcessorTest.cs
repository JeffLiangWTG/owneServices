using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE829;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing.Phase4_1
{
	[TestedType(typeof(IE829MessageProcessor))]
	sealed class IE829MessageProcessorTest : IE829MessageProcessorAbstractTest<Ie829Type>
	{
		protected override ZString MessageText => EMCSMessageProcessorTestHelper.GetStandardIE829Text(false);

		protected override ZString MessageTextMRN => EMCSMessageProcessorTestHelper.GetStandardIE829Text(true);

		protected override string MessageSampleNameSpace => "Enterprise.Customs.IE.EMCS.Business.Testing.Message.MessageProcessors.Phase4_1.TestFiles.IE829MessageSample.txt";
	}
}
