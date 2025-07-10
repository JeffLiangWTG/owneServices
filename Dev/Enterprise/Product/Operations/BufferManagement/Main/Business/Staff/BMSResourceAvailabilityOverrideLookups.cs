using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.BufferManagement.Business
{
	public class BMSResourceAvailabilityOverrideLookups : GlbStaffHolidayLookupsReal
	{
		public BMSResourceAvailabilityOverrideLookups(AutoGlbStaffHoliday parent)
			: base(parent)
		{
		}

		protected override CodeDescriptionBoolCollection GetTypes()
		{
			var leaves = base.GetTypes();
			leaves.Add(new CodeDescriptionBool
			{
				Code = "BMS",
				Description = ResString.GetMultilingualString("3b67e874-de93-4928-b71b-230deadf7e61", "Buffer Management System Leave")
			});
			return leaves;
		}
	}
}
