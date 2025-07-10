using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie829;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing.Version4_1
{
	[TestedType(typeof(IE829MessageProcessor))]
	sealed class IE829MessageProcessorTest : IE829MessageProcessorAbstractTest<Ie829Type>
	{
		protected override ZString MessageText => EMCSMessageProcessorTestHelper.GetStandardIE829Text(false);

		protected override ZString MessageTextMRN => EMCSMessageProcessorTestHelper.GetStandardIE829Text(true);
	}
}
