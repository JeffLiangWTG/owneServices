using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE803;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing.Phase4_1
{
	[TestedType(typeof(IE803MessageProcessor))]
	sealed class IE803MessageProcessorTest : IE803MessageProcessorAbstractTest<Ie803Type>
	{
		protected override ZString MessageText => EMCSMessageProcessorTestHelper.GetStandardIE803Text();

		protected override string MessageSampleNameSpace => "Enterprise.Customs.IE.EMCS.Business.Testing.Message.MessageProcessors.Phase4_1.TestFiles.IE803MessageSample.txt";
	}
}
