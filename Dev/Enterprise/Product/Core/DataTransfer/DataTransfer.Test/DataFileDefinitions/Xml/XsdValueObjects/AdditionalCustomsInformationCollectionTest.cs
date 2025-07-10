using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.AdditionalCustomsInformationCollection))]
	sealed class AdditionalCustomsInformationCollectionTest : ValueObjectCollectionTestCase
	{
		public void TestFindByName()
		{
			Xsd.AdditionalCustomsInformationCollection collection = new Xsd.AdditionalCustomsInformationCollection();

			Xsd.AdditionalCustomsInformation info1 = collection.AddNew();
			info1.CustomsDetailType = "Type1";
			info1.CustomsDetailValue = "Value1";
			Xsd.AdditionalCustomsInformation info2 = collection.AddNew();
			info2.CustomsDetailType = "Type2";
			info2.CustomsDetailValue = "Value2";

			Xsd.AdditionalCustomsInformation foundInfo1 = collection.FindByType("Type1");
			Xsd.AdditionalCustomsInformation foundInfo2 = collection.FindByType("Type2");
			AssertEquals("Should find correct type", "Type1", foundInfo1.CustomsDetailType);
			AssertEquals("Should find correct type", "Type2", foundInfo2.CustomsDetailType);

			AssertNull("Should find null if the type doesnt exist", collection.FindByType("NotExistTypeName"));
		}

		public void TestCompileTimeCheck()
		{
			Xsd.AdditionalCustomsInformationCollection value = null;
			value = new Xsd.Declaration().AddCustomsDetails;
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
