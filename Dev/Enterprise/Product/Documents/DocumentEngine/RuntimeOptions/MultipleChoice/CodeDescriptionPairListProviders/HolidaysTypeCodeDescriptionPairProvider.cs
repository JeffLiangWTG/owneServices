using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class HolidaysTypeCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList(OLookUpEditType.CustomType);
			BusinessObjectFactory factory = new BusinessObjectFactory();
			GlbStaffHoliday holiday = factory.New<GlbStaffHoliday>();
			result.AddRange(new ReadOnlyCodeDescriptionPairList(holiday.Lookups.Types.GetCodeDescriptionPairList()));
			return result;
		}
	}
}
