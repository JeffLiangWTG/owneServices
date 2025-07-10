using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.ClassificationRelatedOrg))]
	sealed class ClassificationRelatedOrgTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.ClassificationRelatedOrg classificationRelatedOrg = new Xsd.ClassificationRelatedOrg();
			AssertEquals(false, classificationRelatedOrg.IsSpecified);

			classificationRelatedOrg.Code = "XXXXX";
			AssertEquals(true, classificationRelatedOrg.IsSpecified);

			classificationRelatedOrg.Code = "";
			AssertEquals(false, classificationRelatedOrg.IsSpecified);

			classificationRelatedOrg.Relationship = Xsd.ClassificationRelatedOrgRelationship.OWN;
			AssertEquals(false, classificationRelatedOrg.IsSpecified);
		}
	}
}
