using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	public class ExportFollowUpTypeListTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCodeDescriptionPairs()
		{
			var exportFollowUpTypeList = new ExportFollowUpTypeList();
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(exportFollowUpTypeList.GetDescriptionFromCode("1"), Is.EqualTo("Nachfrage zum Verbleib der Waren"), "Code '1'");
				NUnit.Framework.Assert.That(exportFollowUpTypeList.GetDescriptionFromCode("2"), Is.EqualTo("Aufforderung zur Vorlage eines Alternativnachweises"), "Code '2'");
			});
		}
	}
}
