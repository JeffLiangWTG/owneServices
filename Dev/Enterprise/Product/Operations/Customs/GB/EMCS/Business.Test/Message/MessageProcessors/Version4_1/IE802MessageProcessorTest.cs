using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie802;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing.Version4_1
{
	[TestedType(typeof(IE802MessageProcessor))]
	sealed class IE802MessageProcessorTest : IE802MessageProcessorAbstractTest<Ie802Type>
	{
		protected override ZString MessageText => EMCSMessageProcessorTestHelper.GetStandardIE802Text();
	}
}
