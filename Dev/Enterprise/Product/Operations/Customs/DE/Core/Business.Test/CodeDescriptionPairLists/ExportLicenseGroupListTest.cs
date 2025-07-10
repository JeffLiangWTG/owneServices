using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	public class ExportLicenseGroupListTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestIsSingleBAFA()
		{
			 NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSingleBAFA(Factory, ExportLicenseGroupList.Codes._3LLA231), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSingleBAFA(Factory, ExportLicenseGroupList.Codes._3LLA82), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSingleBAFA(Factory, ExportLicenseGroupList.Codes._3LLB231), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSingleBAFA(Factory, ExportLicenseGroupList.Codes._3LLB81E), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSingleBAFA(Factory, ExportLicenseGroupList.Codes._3LLC231), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSingleBAFA(Factory, ExportLicenseGroupList.Codes._3LLC81E), Is.True);

				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSingleBAFA(Factory, ExportLicenseGroupList.Codes.E020231), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSingleBAFA(Factory, ExportLicenseGroupList.Codes.E020FWE), Is.True);

				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSingleBAFA(Factory, ExportLicenseGroupList.Codes.X002231), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSingleBAFA(Factory, ExportLicenseGroupList.Codes.X002DEE), Is.True);
			});
		}

		[ExpectNoExceptions]
		public void TestIsComplementaryBAFA()
		{
			NUnit.Framework.Assert.That(ExportLicenseGroupList.IsComplementaryBAFA(Factory, ExportLicenseGroupList.Codes._3LLB81K), Is.True, "IsComplementaryBAFA");
		}

		[ExpectNoExceptions]
		public void TestIsSingleBAFAAF()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSingleBAFAAF(Factory, ExportLicenseGroupList.Codes.C064DE), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSingleBAFAAF(Factory, ExportLicenseGroupList.Codes.E990DEE), Is.True);
			});
		}

		[ExpectNoExceptions]
		public void TestIsEmbargoBAFA()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsEmbargoBAFA(Factory, ExportLicenseGroupList.Codes.C052AF), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsEmbargoBAFA(Factory, ExportLicenseGroupList.Codes.C052BY), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsEmbargoBAFA(Factory, ExportLicenseGroupList.Codes.C052GN), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsEmbargoBAFA(Factory, ExportLicenseGroupList.Codes.C052GW), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsEmbargoBAFA(Factory, ExportLicenseGroupList.Codes.C052IR), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsEmbargoBAFA(Factory, ExportLicenseGroupList.Codes.C052KP), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsEmbargoBAFA(Factory, ExportLicenseGroupList.Codes.C052LY), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsEmbargoBAFA(Factory, ExportLicenseGroupList.Codes.C052MM), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsEmbargoBAFA(Factory, ExportLicenseGroupList.Codes.C052RU), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsEmbargoBAFA(Factory, ExportLicenseGroupList.Codes.C052SD), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsEmbargoBAFA(Factory, ExportLicenseGroupList.Codes.C052SS), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsEmbargoBAFA(Factory, ExportLicenseGroupList.Codes.C052SY), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsEmbargoBAFA(Factory, ExportLicenseGroupList.Codes.C052UA), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsEmbargoBAFA(Factory, ExportLicenseGroupList.Codes.C052VE), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsEmbargoBAFA(Factory, ExportLicenseGroupList.Codes.C052ZW), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsEmbargoBAFA(Factory, ExportLicenseGroupList.Codes.C069KP), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsEmbargoBAFA(Factory, ExportLicenseGroupList.Codes.C070LY), Is.True);
			});
		}

		[ExpectNoExceptions]
		public void TestIsGeneralEU()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsGeneralEU(Factory, ExportLicenseGroupList.Codes.C068), Is.True);

				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsGeneralEU(Factory, ExportLicenseGroupList.Codes.X002E01), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsGeneralEU(Factory, ExportLicenseGroupList.Codes.X002E02), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsGeneralEU(Factory, ExportLicenseGroupList.Codes.X002E03), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsGeneralEU(Factory, ExportLicenseGroupList.Codes.X002E04), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsGeneralEU(Factory, ExportLicenseGroupList.Codes.X002E05), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsGeneralEU(Factory, ExportLicenseGroupList.Codes.X002E06), Is.True);
			});
		}

		[ExpectNoExceptions]
		public void TestIsGeneralDE()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsGeneralDE(Factory, ExportLicenseGroupList.Codes.X002A09), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsGeneralDE(Factory, ExportLicenseGroupList.Codes.X002A10), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsGeneralDE(Factory, ExportLicenseGroupList.Codes.X002A12), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsGeneralDE(Factory, ExportLicenseGroupList.Codes.X002A13), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsGeneralDE(Factory, ExportLicenseGroupList.Codes.X002A14), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsGeneralDE(Factory, ExportLicenseGroupList.Codes.X002A16), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsGeneralDE(Factory, ExportLicenseGroupList.Codes.X002A17), Is.True);

				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsGeneralDE(Factory, ExportLicenseGroupList.Codes._3LLCA18), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsGeneralDE(Factory, ExportLicenseGroupList.Codes._3LLCA19), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsGeneralDE(Factory, ExportLicenseGroupList.Codes._3LLCA20), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsGeneralDE(Factory, ExportLicenseGroupList.Codes._3LLCA21), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsGeneralDE(Factory, ExportLicenseGroupList.Codes._3LLCA22), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsGeneralDE(Factory, ExportLicenseGroupList.Codes._3LLCA23), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsGeneralDE(Factory, ExportLicenseGroupList.Codes._3LLCA24), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsGeneralDE(Factory, ExportLicenseGroupList.Codes._3LLCA25), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsGeneralDE(Factory, ExportLicenseGroupList.Codes._3LLCA26), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsGeneralDE(Factory, ExportLicenseGroupList.Codes._3LLCA27), Is.True);
			});
		}

		[ExpectNoExceptions]
		public void TestIsSandCBAFA()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSandCBAFA(Factory, ExportLicenseGroupList.Codes._3LLA231), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSandCBAFA(Factory, ExportLicenseGroupList.Codes._3LLA82), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSandCBAFA(Factory, ExportLicenseGroupList.Codes._3LLB231), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSandCBAFA(Factory, ExportLicenseGroupList.Codes._3LLB81E), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSandCBAFA(Factory, ExportLicenseGroupList.Codes._3LLB81S), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSandCBAFA(Factory, ExportLicenseGroupList.Codes._3LLC231), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSandCBAFA(Factory, ExportLicenseGroupList.Codes._3LLC81E), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSandCBAFA(Factory, ExportLicenseGroupList.Codes._3LLC81S), Is.True);

				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSandCBAFA(Factory, ExportLicenseGroupList.Codes.X002231), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSandCBAFA(Factory, ExportLicenseGroupList.Codes.X002DEE), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSandCBAFA(Factory, ExportLicenseGroupList.Codes.X002DES), Is.True);
			});
		}

		[ExpectNoExceptions]
		public void TestIsSingleBAFAFWV()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSingleBAFAFWV(Factory, ExportLicenseGroupList.Codes.E020231), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSingleBAFAFWV(Factory, ExportLicenseGroupList.Codes.E020FWE), Is.True);
			});
		}

		[ExpectNoExceptions]
		public void TestIsSystemBAFA()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes._3LLA231), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes._3LLA82), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes._3LLB231), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes._3LLB81E), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes._3LLB81K), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes._3LLB81S), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes._3LLC231), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes._3LLC81E), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes._3LLC81S), Is.True);

				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes.C052AF), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes.C052BY), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes.C052GN), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes.C052GW), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes.C052IR), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes.C052KP), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes.C052LY), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes.C052MM), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes.C052RU), Is.True);

				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes.C052SD), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes.C052SS), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes.C052SY), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes.C052UA), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes.C052VE), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes.C052ZW), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes.C064DE), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes.C069KP), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes.C070LY), Is.True);

				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes.E020231), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes.E020FWE), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes.E020FWS), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes.E990DEE), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes.E990DES), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes.X002231), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes.X002DEE), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsSystemBAFA(Factory, ExportLicenseGroupList.Codes.X002DES), Is.True);
			});
		}

		[ExpectNoExceptions]
		public void TestIsMilitaryWeapons()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsMilitaryWeapons(Factory, ExportLicenseGroupList.Codes._3LLB231), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsMilitaryWeapons(Factory, ExportLicenseGroupList.Codes._3LLB81E), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsMilitaryWeapons(Factory, ExportLicenseGroupList.Codes._3LLB81K), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsMilitaryWeapons(Factory, ExportLicenseGroupList.Codes._3LLB81S), Is.True);
			});
		}

		[ExpectNoExceptions]
		public void TestIsNonMilitaryWeapons()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsNonMilitaryWeapons(Factory, ExportLicenseGroupList.Codes._3LLC231), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsNonMilitaryWeapons(Factory, ExportLicenseGroupList.Codes._3LLC81E), Is.True);
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsNonMilitaryWeapons(Factory, ExportLicenseGroupList.Codes._3LLC81S), Is.True);
			});
		}

		[ExpectNoExceptions]
		public void TestIsZeroNoticeBAFA()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsZeroNoticeBAFA(Factory, ExportLicenseGroupList.Codes._3LLDNB), Is.True);
			});
		}

		[ExpectNoExceptions]
		public void TestIsReferenceBAFAAWV()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(ExportLicenseGroupList.IsReferenceBAFAAWV(Factory, ExportLicenseGroupList.Codes.E020AWV), Is.True);
			});
		}
	}
}
