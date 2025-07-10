using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE819;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing.Phase4_1
{
	[TestedType(typeof(IE819MessageProcessor))]
	sealed class IE819MessageProcessorTest : IE819MessageProcessorAbstractTest<Ie819Type>
	{
		protected override ZString MessageText => EMCSMessageProcessorTestHelper.GetStandardIE819Text();

		protected override string MessageSampleNameSpace => "Enterprise.Customs.IE.EMCS.Business.Testing.Message.MessageProcessors.Phase4_1.TestFiles.IE819MessageSample.txt";
	}
}
