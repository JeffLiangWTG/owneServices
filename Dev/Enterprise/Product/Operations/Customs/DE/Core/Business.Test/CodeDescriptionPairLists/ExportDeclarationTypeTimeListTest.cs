using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Customs.DE.Business.ExportDeclarationTypeTimeList.Codes;

namespace Enterprise.Customs.DE.Business.Testing
{
	class ExportDeclarationTypeTimeListTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestIs10()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				foreach (var subType in new ExportDeclarationTypeTimeList().GetAllCodesZString())
				{
					NUnit.Framework.Assert.That(ExportDeclarationTypeTimeList.Is10(subType), Is.EqualTo(subType == _10).Using(CustomComparers.TypeComparison), subType.ToString());
				}
			});
		}

		[ExpectNoExceptions]
		public void TestIsMultipleDeclarationForExport()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				foreach (var subType in new ExportDeclarationTypeTimeList().GetAllCodesZString())
				{
					NUnit.Framework.Assert.That(ExportDeclarationTypeTimeList.IsMultipleDeclarationForExport(subType), Is.EqualTo(subType == _20).Using(CustomComparers.TypeComparison), subType.ToString());
				}
			});
		}

		[ExpectNoExceptions]
		public void TestIs11Or12()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				foreach (var subType in new ExportDeclarationTypeTimeList().GetAllCodesZString())
				{
					NUnit.Framework.Assert.That(ExportDeclarationTypeTimeList.Is11Or12(subType), Is.EqualTo(subType == _11 || subType == _12).Using(CustomComparers.TypeComparison), subType.ToString());
				}
			});
		}

		[ExpectNoExceptions]
		public void TestIs11Or12Or13()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				foreach (var subType in new ExportDeclarationTypeTimeList().GetAllCodesZString())
				{
					NUnit.Framework.Assert.That(ExportDeclarationTypeTimeList.Is11Or12Or13(subType), Is.EqualTo(subType == _11 || subType == _12 || subType == _13).Using(CustomComparers.TypeComparison), subType.ToString());
				}
			});
		}

		[ExpectNoExceptions]
		public void TestIs13()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				foreach (var subType in new ExportDeclarationTypeTimeList().GetAllCodesZString())
				{
					NUnit.Framework.Assert.That(ExportDeclarationTypeTimeList.Is13(subType), Is.EqualTo(subType == _13).Using(CustomComparers.TypeComparison), subType.ToString());
				}
			});
		}
	}
}
