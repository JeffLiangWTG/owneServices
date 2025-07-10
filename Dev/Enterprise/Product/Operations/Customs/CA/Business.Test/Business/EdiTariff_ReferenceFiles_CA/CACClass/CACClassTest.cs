using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACClass))]
	sealed class CACClassTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCodeDescription()
		{
			CACClass tariff = Factory.New<CACClass>();
			tariff.CT_Tariff = "0000000000";
			tariff.CT_LongDescription = "Long description";

			AssertEquals("Code", "0000000000", CodePropertyAttribute.CodeFromBusinessObject(tariff));
			AssertEquals("Desc", "Long description", DescriptionPropertyAttribute.DescriptionFromBusinessObject(tariff));
		}

		public void TestFormattedCode()
		{
			CACClass tariff = Factory.New<CACClass>();
			AssertEquals("CT_FormattedCode", "", tariff.CT_FormattedCode);
			tariff.CT_Tariff = "1234567890";
			AssertEquals("CT_FormattedCode", "1234.56.78 90", tariff.CT_FormattedCode);
		}
	}
}
