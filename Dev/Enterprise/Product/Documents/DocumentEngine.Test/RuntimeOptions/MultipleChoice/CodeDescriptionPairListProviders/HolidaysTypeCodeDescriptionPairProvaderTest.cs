using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class HolidaysTypeCodeDescriptionPairProvaderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new HolidaysTypeCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList(OLookUpEditType.CustomType);
			BusinessObjectFactory factory = new BusinessObjectFactory();
			GlbStaffHoliday holiday = factory.New<GlbStaffHoliday>();
			result.AddRange(new ReadOnlyCodeDescriptionPairList(holiday.Lookups.Types.GetCodeDescriptionPairList()));

			var holidaysType = CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();

			foreach (CodeDescriptionPair pair in result)
			{
				AssertCollectionContains(pair, holidaysType);
			}
		}
	}
}
