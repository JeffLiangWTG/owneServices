using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie839;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing.Version4_1
{
	[TestedType(typeof(IE839MessageProcessor))]
	sealed class IE839MessageProcessorTest : IE839MessageProcessorAbstractTest<Ie839Type>
	{
		protected override ZString MessageText => EMCSMessageProcessorTestHelper.GetStandardIE839Text(false);

		protected override ZString MessageTextMRN => EMCSMessageProcessorTestHelper.GetStandardIE839Text(true);
	}
}
