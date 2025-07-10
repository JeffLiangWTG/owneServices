#if DEBUG

using CargoWise.Types;

namespace Enterprise.Accounting.Business.Base.Unmatching
{
	public partial class MatchGroupFilterHelper
	{
		public ZString OrganisationFilterString_ForTestOnly => OrganisationFilterString;
	}
}

#endif
