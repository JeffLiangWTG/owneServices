using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders.Testing
{
	class AdditionalInformationComparerTest : TestCaseWithFactory
	{
		public void TestCompare()
		{
			var document = XDocument.Parse("<root><Statement><StatementTypeCode>b</StatementTypeCode></Statement><Statement><StatementTypeCode>a</StatementTypeCode></Statement></root>");
			IEnumerable<XElement> elements = document.Elements().Select(x => x);
			var sorted = elements.OrderBy(x => x, new AdditionalInformationComparer());
			AssertNotEquals(elements, sorted);
			AssertEquals(elements.First(), sorted.Last());
			AssertEquals(elements.Last(), sorted.First());
		}
	}
}
