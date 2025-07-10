using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE802;
using CargoWise.Types;
using Enterprise.Customs.IE.EMCS.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing.Phase4_1
{
	[TestedType(typeof(IE802MessageInterpreter))]
	sealed class IE802MessageInterpreterTest : IE802MessageInterpreterAbstractTest
	{
		protected override ZString MessageText => EMCSMessageProcessorTestHelper.GetStandardIE802Text();

		protected override IIE802 GetProvider(TextReader reader) => new Messaging.Phase4_1.IE802Provider(IEXmlObjectSerializer.Deserialize<Ie802Type>(reader));
	}
}
