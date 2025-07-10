using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	class SupportIncidentHtmlParserTest : TestCaseWithFactory
	{
		public void TestCorrectWrapperAndBizOTypes()
		{
			var parser = new SupportIncidentHtmlParserForTesting(Factory);
			AssertEquals(typeof(DocSupportIncident), parser.GetTypeOfWrapper_ForTesting());
		}

		[ExpectNoExceptions]
		public void TestParse()
		{
			var parser = new SupportIncidentHtmlParser(Factory);
			var incident = Factory.New<SupportIncident>();
			incident.IM_Description = "Description with a 'tag' <b>!";
			AssertEquals("Should be html encoded", "Description with a &#39;tag&#39; &lt;b&gt;!", parser.Parse(incident, "(*Summary*)"));
		}

		#region Imeplementation

		class SupportIncidentHtmlParserForTesting : SupportIncidentHtmlParser
		{
			internal SupportIncidentHtmlParserForTesting(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			internal Type GetTypeOfWrapper_ForTesting()
			{
				return TypeOfWrapper;
			}
		}

		#endregion
	}
}