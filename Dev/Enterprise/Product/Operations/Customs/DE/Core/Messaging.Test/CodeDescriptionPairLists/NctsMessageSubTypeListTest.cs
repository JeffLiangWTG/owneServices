using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.Testing.CodeDescriptionPairLists
{
	sealed class NctsMessageSubTypeListTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestCodes()
		{
			CombineAssertions(() =>
			{
				var list = new NctsMessageSubTypeList();
				NUnit.Framework.Assert.That(list.GetDescriptionFromCode("DES"), Is.EqualTo("Destination Message"), "DES");
				NUnit.Framework.Assert.That(list.GetDescriptionFromCode("DEP"), Is.EqualTo("Departure Message"), "DEP");
				NUnit.Framework.Assert.That(list.GetDescriptionFromCode("TRQ"), Is.EqualTo("Status Request Message"), "TRQ");
				NUnit.Framework.Assert.That(list.GetDescriptionFromCode("GUA"), Is.EqualTo("Guarantee Access Handling"), "GUA");
			});
		}
	}
}
