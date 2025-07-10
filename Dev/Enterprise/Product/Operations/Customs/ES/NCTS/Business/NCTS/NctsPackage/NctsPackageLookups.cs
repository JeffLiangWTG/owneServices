
using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsPackageLookups : EU.NCTS.Business.NctsPackageLookups
	{
		public NctsPackageLookups(EU.NCTS.Business.NctsPackage parent) : base(parent)
		{
		}

		protected override ZBool ShouldIncludeDIFInUnloadedStatesList => true;
	}
}
