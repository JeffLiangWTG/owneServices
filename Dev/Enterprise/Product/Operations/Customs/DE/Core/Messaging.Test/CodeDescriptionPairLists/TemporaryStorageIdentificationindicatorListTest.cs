using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.Testing
{
	class TemporaryStorageIdentificationIndicatorListTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCodeDescriptionPairs()
		{
			var list = new TemporaryStorageIdentificationIndicatorList();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(list.Count, Is.EqualTo(3), "Count");
				NUnit.Framework.Assert.That(list.GetDescriptionFromCode("AWB"), Is.EqualTo("Ordnungsbegriff-bezogene Identifikation"), "Code 'AWB'");
				NUnit.Framework.Assert.That(list.GetDescriptionFromCode("REG"), Is.EqualTo("Registriernummer-/Positionsnummer-bezogene Identifikation"), "Code 'REG'");
				NUnit.Framework.Assert.That(list.GetDescriptionFromCode("SIN"), Is.EqualTo("Sendungsidentifikationsnummer"), "Code 'SIN'");
			});
		}
	}
}
