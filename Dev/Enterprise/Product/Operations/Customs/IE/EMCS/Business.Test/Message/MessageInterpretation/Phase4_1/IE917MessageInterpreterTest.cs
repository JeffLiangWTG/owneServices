using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE917;
using CargoWise.Types;
using Enterprise.Customs.IE.EMCS.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing.Phase4_1
{
	[TestedType(typeof(IE917MessageInterpreter))]
	sealed class IE917MessageInterpreterTest : IE917MessageInterpreterAbstractTest
	{
		protected override ZString MessageText => EMCSMessageProcessorTestHelper.GetStandardIE917Text();

		protected override IIE917 GetProvider(TextReader reader) => new Messaging.Phase4_1.IE917Provider(IEXmlObjectSerializer.Deserialize<Ie917Type>(reader));
	}
}
