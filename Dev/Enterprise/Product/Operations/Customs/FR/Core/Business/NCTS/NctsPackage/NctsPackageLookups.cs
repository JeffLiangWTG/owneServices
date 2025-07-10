
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsPackageLookups : EU.NCTS.Business.NctsPackageLookups
	{
		public NctsPackageLookups(NctsPackage parent) : base(parent)
		{
		}

		protected override ZBool ShouldIncludeDIFInUnloadedStatesList => true;
	}
}
