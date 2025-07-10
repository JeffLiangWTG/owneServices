using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie837;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing.Version4_1
{
	[TestedType(typeof(IE837MessageProcessor))]
	sealed class IE837MessageProcessorTest : IE837MessageProcessorAbstractTest<Ie837Type>
	{
		protected override ZString MessageText => EMCSMessageProcessorTestHelper.GetStandardIE837Text();
	}
}
