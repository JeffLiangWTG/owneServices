using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class HolidaysStatusCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList(OLookUpEditType.CustomType);
			BusinessObjectFactory factory = new BusinessObjectFactory();
			GlbStaffHoliday holiday = factory.New<GlbStaffHoliday>();
			result.AddRange(new ReadOnlyCodeDescriptionPairList(holiday.Lookups.Statuses));
			return result;
		}
	}
}
