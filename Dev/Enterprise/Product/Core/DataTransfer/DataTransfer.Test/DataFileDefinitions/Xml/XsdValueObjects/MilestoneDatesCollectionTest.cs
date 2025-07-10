using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.MilestoneDatesCollection))]
	sealed class MilestoneDatesCollectionTest : ValueObjectCollectionTestCase
	{
		public void TestGeneratedCorrectly()
		{
			Xsd.MilestoneDatesCollection value = null;
			value = new Xsd.OrderOrderDetailMilestones().UserDate;
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
