using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	class SpecialCaseGroupListPartialTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestIsRequiredForProcedure_4054_4254()
		{
			var expectedGroups = new string[]
			{
				SpecialCaseGroupList.Codes._01,
				SpecialCaseGroupList.Codes._02,
				SpecialCaseGroupList.Codes._03,
				SpecialCaseGroupList.Codes._04,
				SpecialCaseGroupList.Codes._05,
				SpecialCaseGroupList.Codes._06,
				SpecialCaseGroupList.Codes._07,
				SpecialCaseGroupList.Codes._08,
				SpecialCaseGroupList.Codes._09,
				SpecialCaseGroupList.Codes._10,
				SpecialCaseGroupList.Codes._11,
				SpecialCaseGroupList.Codes._12,
				SpecialCaseGroupList.Codes._20
			};
			NUnit.Framework.Assert.Multiple(() =>
			{
				foreach (var group in expectedGroups)
				{
					NUnit.Framework.Assert.That(SpecialCaseGroupList.IsInGroups1to12And20(Factory, group), Is.True, group);
				}
				foreach (var group in new SpecialCaseGroupList().GetAllCodes().Except(expectedGroups))
				{
					NUnit.Framework.Assert.That(!SpecialCaseGroupList.IsInGroups1to12And20(Factory, group), Is.True, group);
				}
			});
		}
	}
}
