using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE801;
using CargoWise.Types;
using Enterprise.Customs.IE.EMCS.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing.Phase4_1
{
	[TestedType(typeof(IE801MessageInterpreter))]
	sealed class IE801MessageInterpreterTest : IE801MessageInterpreterAbstractTest
	{
		protected override ZString MessageText => EMCSMessageProcessorTestHelper.GetStandardIE801Text();

		protected override IIE801 GetProvider(TextReader reader) => new Messaging.Phase4_1.IE801Provider(IEXmlObjectSerializer.Deserialize<Ie801Type>(reader));
	}
}
