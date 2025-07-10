using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class MonthlyClosingDeclarationTypeListTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCodeDescriptionPairs()
		{
			var list = new MonthlyClosingDeclarationTypeList();
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(list.CodesAsString, Is.EqualTo("AAV, AZ, AZL, VAV, VZA, VZL"), "CodesAsString");
				NUnit.Framework.Assert.That(list.GetDescriptionFromCode(MonthlyClosingDeclarationTypeList.Codes.AAV), Is.EqualTo("Local clearance procedure for Inward Processing"), "Code 'AAV'");
				NUnit.Framework.Assert.That(list.GetDescriptionFromCode(MonthlyClosingDeclarationTypeList.Codes.AZ), Is.EqualTo("Local clearance procedure for release of goods for free circulation"), "Code 'AZ'");
				NUnit.Framework.Assert.That(list.GetDescriptionFromCode(MonthlyClosingDeclarationTypeList.Codes.AZL), Is.EqualTo("Local clearance procedure for entry into the customs warehousing procedure"), "Code 'AZL'");
				NUnit.Framework.Assert.That(list.GetDescriptionFromCode(MonthlyClosingDeclarationTypeList.Codes.VAV), Is.EqualTo("Simplified customs declaration for inward processing"), "Code 'VAV'");
				NUnit.Framework.Assert.That(list.GetDescriptionFromCode(MonthlyClosingDeclarationTypeList.Codes.VZA), Is.EqualTo("Simplified customs declaration for release of goods for free circulation"), "Code 'VZA'");
				NUnit.Framework.Assert.That(list.GetDescriptionFromCode(MonthlyClosingDeclarationTypeList.Codes.VZL), Is.EqualTo("Simplified customs declaration for entry into the customs warehousing procedure"), "Code 'VZL'");
			});
		}
	}
}
