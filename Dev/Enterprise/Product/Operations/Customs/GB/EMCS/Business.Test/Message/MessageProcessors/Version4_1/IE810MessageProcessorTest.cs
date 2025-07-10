using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie810;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing.Version4_1
{
	[TestedType(typeof(IE810MessageProcessor))]
	sealed class IE810MessageProcessorTest : IE810MessageProcessorAbstractTest<Ie810Type>
	{
		protected override ZString MessageText => EMCSMessageProcessorTestHelper.GetStandardIE810Text();
	}
}
