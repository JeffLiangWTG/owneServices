using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie905;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing.Version4_1
{
	[TestedType(typeof(IE905MessageProcessor))]
	sealed class IE905MessageProcessorTest : IE905MessageProcessorAbstractTest<Ie905Type>
	{
		protected override ZString MessageText => EMCSMessageProcessorTestHelper.GetStandardIE905Text();
	}
}
