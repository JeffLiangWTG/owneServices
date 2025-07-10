using System.Reflection;
using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders.Testing
{
	class AmendmentDetailsTest : TestCaseWithFactory
	{
		public void TestAmendmentDetails()
		{
			var element = XElement.Parse("<Root>Test</Root>");
			var amendmentDetails = new AmendmentDetails(element, "comparison", "new");
			AssertEquals(element, amendmentDetails.DiffGram);
			AssertEquals("comparison", amendmentDetails.ComparisonXml);
			AssertEquals("new", amendmentDetails.NewXml);
		}

		public void TestHasDifferences()
		{
			var element = XElement.Parse("<Root>Test</Root>");
			var amendmentDetails = new AmendmentDetails(element, "", "");
			AssertEquals(false, amendmentDetails.HasDifferences);

			element = XElement.Parse("<Root><child>Test</child></Root>");
			amendmentDetails = new AmendmentDetails(element, "", "");
			AssertEquals(true, amendmentDetails.HasDifferences);
		}

		public void TestAmendments()
		{
			var property = typeof(AmendmentDetails).GetTypeInfo().GetProperty("Amendments");
			AssertContains(typeof(AmendmentObjectWrapper).ToString(), property.PropertyType.FullName);
		}
	}
}
