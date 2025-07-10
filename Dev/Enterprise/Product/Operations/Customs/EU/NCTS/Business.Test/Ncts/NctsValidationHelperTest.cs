using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsValidationHelperTest : TestCase
	{
		public void TestIsEntryTypeT2OrT2F() => CombineAssertions(() =>
		{
			foreach (var entryType in new NctsPhase5DeclarationTypeList().GetAllCodes())
			{
				switch (entryType)
				{
					case NctsPhase5DeclarationTypeList.Codes.T2:
					case NctsPhase5DeclarationTypeList.Codes.T2F:
						AssertEquals(entryType, true, NctsValidationHelper.IsEntryTypeT2OrT2F(entryType));
						break;
					default:
						AssertEquals(entryType, false, NctsValidationHelper.IsEntryTypeT2OrT2F(entryType));
						break;
				}
			}
		});
	}
}
