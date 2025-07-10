using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE704;
using CargoWise.Types;
using Enterprise.Customs.IE.EMCS.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing.Phase4_1
{
	[TestedType(typeof(IE704MessageInterpreter))]
	sealed class IE704MessageInterpreterTest : IE704MessageInterpreterAbstractTest
	{
		protected override ZString MessageText => EMCSMessageProcessorTestHelper.GetStandardIE704Text();

		protected override IIE704 GetProvider(TextReader reader) => new Messaging.Phase4_1.IE704Provider(IEXmlObjectSerializer.Deserialize<Ie704Type>(reader));
	}
}
