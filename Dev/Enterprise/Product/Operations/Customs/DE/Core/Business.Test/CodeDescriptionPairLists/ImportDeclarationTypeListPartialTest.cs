using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using IMPDecTypeList = Enterprise.Customs.DE.Business.ImportDeclarationTypeList;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ImportDeclarationTypeListPartialTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestIsLocalClearanceDateRelevant()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(IMPDecTypeList.IsLocalClearanceDateRelevant(IMPDecTypeList.Codes.AAV), Is.EqualTo(true), IMPDecTypeList.Codes.AAV);
				NUnit.Framework.Assert.That(IMPDecTypeList.IsLocalClearanceDateRelevant(IMPDecTypeList.Codes.AZ), Is.EqualTo(true), IMPDecTypeList.Codes.AZ);
				NUnit.Framework.Assert.That(IMPDecTypeList.IsLocalClearanceDateRelevant(IMPDecTypeList.Codes.AZL), Is.EqualTo(true), IMPDecTypeList.Codes.AZL);
				NUnit.Framework.Assert.That(IMPDecTypeList.IsLocalClearanceDateRelevant(IMPDecTypeList.Codes.VAV), Is.EqualTo(false), IMPDecTypeList.Codes.VAV);
				NUnit.Framework.Assert.That(IMPDecTypeList.IsLocalClearanceDateRelevant(IMPDecTypeList.Codes.VZA), Is.EqualTo(false), IMPDecTypeList.Codes.VZA);
				NUnit.Framework.Assert.That(IMPDecTypeList.IsLocalClearanceDateRelevant(IMPDecTypeList.Codes.VZL), Is.EqualTo(false), IMPDecTypeList.Codes.VZL);
				NUnit.Framework.Assert.That(IMPDecTypeList.IsLocalClearanceDateRelevant(IMPDecTypeList.Codes.BA), Is.EqualTo(false), IMPDecTypeList.Codes.BA);
				NUnit.Framework.Assert.That(IMPDecTypeList.IsLocalClearanceDateRelevant(IMPDecTypeList.Codes.EAV), Is.EqualTo(false), IMPDecTypeList.Codes.EAV);
				NUnit.Framework.Assert.That(IMPDecTypeList.IsLocalClearanceDateRelevant(IMPDecTypeList.Codes.EGN), Is.EqualTo(false), IMPDecTypeList.Codes.EGN);
			});
		}

		[ExpectNoExceptions]
		public void TestIsForImportFromSpecialTerritoryEntryStyle()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(IMPDecTypeList.IsForImportFromSpecialTerritoryEntryStyle(IMPDecTypeList.Codes.AZ), Is.EqualTo(true), IMPDecTypeList.Codes.AZ);
				NUnit.Framework.Assert.That(IMPDecTypeList.IsForImportFromSpecialTerritoryEntryStyle(IMPDecTypeList.Codes.EZA), Is.EqualTo(true), IMPDecTypeList.Codes.EZA);
				NUnit.Framework.Assert.That(IMPDecTypeList.IsForImportFromSpecialTerritoryEntryStyle(IMPDecTypeList.Codes.VZA), Is.EqualTo(true), IMPDecTypeList.Codes.VZA);
			});
		}

		[ExpectNoExceptions]
		public void TestIsIndirectRepresentationNotAllowed()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(IMPDecTypeList.IsIndirectRepresentationNotAllowed(IMPDecTypeList.Codes.EAV), Is.EqualTo(true), IMPDecTypeList.Codes.EAV);
				NUnit.Framework.Assert.That(IMPDecTypeList.IsIndirectRepresentationNotAllowed(IMPDecTypeList.Codes.AAV), Is.EqualTo(false), IMPDecTypeList.Codes.AAV);
				NUnit.Framework.Assert.That(IMPDecTypeList.IsIndirectRepresentationNotAllowed(IMPDecTypeList.Codes.VAV), Is.EqualTo(false), IMPDecTypeList.Codes.VAV);
			});
		}

		[ExpectNoExceptions]
		public void TestIsLUZ()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				foreach (var style in new IMPDecTypeList().GetAllCodesZString())
				{
					NUnit.Framework.Assert.That(IMPDecTypeList.IsLUZ(style), Is.EqualTo(style == IMPDecTypeList.Codes.LUZ), style.ToString());
				}
			});
		}

		[ExpectNoExceptions]
		public void TestGetSimplifiedDeclarationTypeList()
		{
			var list = IMPDecTypeList.GetSimplifiedDeclarationTypeList(Factory);
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(list.CodesAsString, Is.EqualTo("AAV, AZ, AZL, VAV, VZA, VZL"), "CodesAsString");
				NUnit.Framework.Assert.That(IMPDecTypeList.GetSimplifiedDeclarationTypeList(Factory), Is.SameAs(list), "Cached");
				NUnit.Framework.Assert.That(list.GetDescriptionFromCode(MonthlyClosingDeclarationTypeList.Codes.AAV), Is.EqualTo("Local clearance procedure for Inward Processing"), "Code 'AAV'");
				NUnit.Framework.Assert.That(list.GetDescriptionFromCode(MonthlyClosingDeclarationTypeList.Codes.AZ), Is.EqualTo("Local clearance procedure for release of goods for free circulation"), "Code 'AZ'");
				NUnit.Framework.Assert.That(list.GetDescriptionFromCode(MonthlyClosingDeclarationTypeList.Codes.AZL), Is.EqualTo("Local clearance procedure for entry into the customs warehousing procedure"), "Code 'AZL'");
				NUnit.Framework.Assert.That(list.GetDescriptionFromCode(MonthlyClosingDeclarationTypeList.Codes.VAV), Is.EqualTo("Simplified customs declaration for inward processing"), "Code 'VAV'");
				NUnit.Framework.Assert.That(list.GetDescriptionFromCode(MonthlyClosingDeclarationTypeList.Codes.VZA), Is.EqualTo("Simplified customs declaration for release of goods for free circulation"), "Code 'VZA'");
				NUnit.Framework.Assert.That(list.GetDescriptionFromCode(MonthlyClosingDeclarationTypeList.Codes.VZL), Is.EqualTo("Simplified customs declaration for entry into the customs warehousing procedure"), "Code 'VZL'");
			});
		}

		[ExpectNoExceptions]
		public void TestIsSimplifiedWarehouse()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(IMPDecTypeList.IsSimplifiedWarehouse(IMPDecTypeList.Codes.AZL), Is.EqualTo(true), IMPDecTypeList.Codes.AZL);
				NUnit.Framework.Assert.That(IMPDecTypeList.IsSimplifiedWarehouse(IMPDecTypeList.Codes.VZL), Is.EqualTo(true), IMPDecTypeList.Codes.VZL);
				NUnit.Framework.Assert.That(IMPDecTypeList.IsSimplifiedWarehouse(IMPDecTypeList.Codes.AAV), Is.EqualTo(false), "Not AZL or VZL");
			});
		}

		[ExpectNoExceptions]
		public void TestIsSimplifiedInwardProcessing()
		{
			NUnit.Framework.Assert.That(IMPDecTypeList.IsSimplifiedInwardProcessing(IMPDecTypeList.Codes.AAV), Is.EqualTo(true), IMPDecTypeList.Codes.AAV);
			NUnit.Framework.Assert.That(IMPDecTypeList.IsSimplifiedInwardProcessing(IMPDecTypeList.Codes.VAV), Is.EqualTo(true), IMPDecTypeList.Codes.VAV);
			NUnit.Framework.Assert.That(IMPDecTypeList.IsSimplifiedInwardProcessing(IMPDecTypeList.Codes.VZL), Is.EqualTo(false), "Not AAV or VAV");
		}

		[ExpectNoExceptions]
		public void TestIsSimplifiedFreeCirculation()
		{
			NUnit.Framework.Assert.That(IMPDecTypeList.IsSimplifiedFreeCirculation(IMPDecTypeList.Codes.AZ), Is.EqualTo(true), IMPDecTypeList.Codes.AZ);
			NUnit.Framework.Assert.That(IMPDecTypeList.IsSimplifiedFreeCirculation(IMPDecTypeList.Codes.VZA), Is.EqualTo(true), IMPDecTypeList.Codes.VZA);
			NUnit.Framework.Assert.That(IMPDecTypeList.IsSimplifiedFreeCirculation(IMPDecTypeList.Codes.VZL), Is.EqualTo(false), "Not AZ or VZA");
		}
	}
}
