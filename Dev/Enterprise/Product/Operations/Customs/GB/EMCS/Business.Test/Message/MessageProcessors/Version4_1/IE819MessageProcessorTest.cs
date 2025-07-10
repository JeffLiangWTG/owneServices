using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie819;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing.Version4_1
{
	[TestedType(typeof(IE819MessageProcessor))]
	sealed class IE819MessageProcessorTest : IE819MessageProcessorAbstractTest<Ie819Type>
	{
		protected override ZString MessageText => EMCSMessageProcessorTestHelper.GetStandardIE819Text();
	}
}
