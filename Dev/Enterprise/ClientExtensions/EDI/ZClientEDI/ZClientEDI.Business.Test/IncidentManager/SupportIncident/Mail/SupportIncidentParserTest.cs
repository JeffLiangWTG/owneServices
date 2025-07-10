using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	class SupportIncidentParserTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestParse()
		{
			SupportIncidentParser parser = new SupportIncidentParser(Factory);
			SupportIncident incident = Factory.New<SupportIncident>();
			parser.Parse(incident, "");
		}
	}
}