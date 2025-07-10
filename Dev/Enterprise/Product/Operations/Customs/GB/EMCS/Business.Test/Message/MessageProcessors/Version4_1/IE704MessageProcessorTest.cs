using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie704uk;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing.Version4_1
{
	[TestedType(typeof(IE704MessageProcessor))]
	sealed class IE704MessageProcessorTest : IE704MessageProcessorAbstractTest<Ie704Type>
	{
		protected override ZString MessageText => EMCSMessageProcessorTestHelper.GetStandardIE704Text();
	}
}
