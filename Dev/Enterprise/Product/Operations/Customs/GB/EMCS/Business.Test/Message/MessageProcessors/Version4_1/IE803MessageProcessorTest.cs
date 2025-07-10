using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie803;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing.Version4_1
{
	[TestedType(typeof(IE803MessageProcessor))]
	sealed class IE803MessageProcessorTest : IE803MessageProcessorAbstractTest<Ie803Type>
	{
		protected override ZString MessageText => EMCSMessageProcessorTestHelper.GetStandardIE803Text();
	}
}
