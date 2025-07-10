using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class SpecialCaseTypeListTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCodeDescriptionPairs()
		{
			var specialCaseTypeList = new SpecialCaseTypeList();
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(specialCaseTypeList.CodesAsString, Is.EqualTo("01, 02, 03, 04, 05, 06, 07, 08, 09"), "CodesAsString");
				NUnit.Framework.Assert.That(specialCaseTypeList.GetDescriptionFromCode("01"), Is.EqualTo("Replace with a percentage"), "Code '01'");
				NUnit.Framework.Assert.That(specialCaseTypeList.GetDescriptionFromCode("02"), Is.EqualTo("Replace with an amount per unit of measurement"), "Code '02'");
				NUnit.Framework.Assert.That(specialCaseTypeList.GetDescriptionFromCode("03"), Is.EqualTo("Correct by a factor"), "Code '03'");
				NUnit.Framework.Assert.That(specialCaseTypeList.GetDescriptionFromCode("04"), Is.EqualTo("Replacing the minimum rate or minimum amount"), "Code '04'");
				NUnit.Framework.Assert.That(specialCaseTypeList.GetDescriptionFromCode("05"), Is.EqualTo("Replacing the maximum rate or maximum amount"), "Code '05'");
				NUnit.Framework.Assert.That(specialCaseTypeList.GetDescriptionFromCode("06"), Is.EqualTo("Amount of duty according to Art. 86 (3) UCC and/or amount from INF.1"), "Code '06'");
				NUnit.Framework.Assert.That(specialCaseTypeList.GetDescriptionFromCode("07"), Is.EqualTo("Amount of duty in accordance with Article 205 UCC; amount of duty in accordance with Article 86 (3) UCC"), "Code '07'");
				NUnit.Framework.Assert.That(specialCaseTypeList.GetDescriptionFromCode("08"), Is.EqualTo("No collection of duties and taxes for this group of duties"), "Code '08'");
				NUnit.Framework.Assert.That(specialCaseTypeList.GetDescriptionFromCode("09"), Is.EqualTo("Change to 'Free'"), "Code '09'");
			});
		}
	}
}
