using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie818;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing.Version4_1
{
	[TestedType(typeof(IE818MessageProcessor))]
	sealed class IE818MessageProcessorTest : IE818MessageProcessorAbstractTest<Ie818Type>
	{
		protected override ZString MessageText => EMCSMessageProcessorTestHelper.GetStandardIE818Text();
	}
}
