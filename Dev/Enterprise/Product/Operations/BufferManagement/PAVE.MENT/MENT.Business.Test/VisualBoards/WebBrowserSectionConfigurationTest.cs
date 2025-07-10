using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.PAVE.MENT.Shared;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.Business.Test
{
	[TestedType(typeof(WebBrowserSectionConfiguration))]
	class WebBrowserSectionConfigurationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAddress_Valid()
		{
			var section = Factory.NewWithValidTestData<BMBoardSection>();
			section.MS_SectionType = MENTConstants.WebBrowserSectionType;

			var configuration = new WebBrowserSectionConfiguration(section);

			configuration.URL = "http://www.hasthelhcdestroyedtheearth.com";

			AssertNotNull(configuration.Address);
		}

		public void TestAddress_InValidWithoutProtocol()
		{
			var section = Factory.NewWithValidTestData<BMBoardSection>();
			section.MS_SectionType = MENTConstants.WebBrowserSectionType;

			var configuration = new WebBrowserSectionConfiguration(section);

			configuration.URL = "www.hasthelhcdestroyedtheearth.com";

			AssertNull(configuration.Address);
		}

		public void TestAddress_InValidSillyAddress()
		{
			var section = Factory.NewWithValidTestData<BMBoardSection>();
			section.MS_SectionType = MENTConstants.WebBrowserSectionType;

			var configuration = new WebBrowserSectionConfiguration(section);

			configuration.URL = "hasthelhcdestroyedtheearth";

			AssertNull(configuration.Address);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var section = Factory.NewWithValidTestData<BMBoardSection>();
			section.MS_SectionType = MENTConstants.WebBrowserSectionType;
			return new WebBrowserSectionConfiguration(section);
		}

		protected override IEnumerable<string> XmlMemberNames
		{
			get
			{
				yield return "URL";
			}
		}

		#endregion
	}
}
