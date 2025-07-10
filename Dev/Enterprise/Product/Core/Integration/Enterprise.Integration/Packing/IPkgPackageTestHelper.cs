using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration.Packing
{
	public interface IPkgPackageTestHelper
	{
		BusinessObject CreatePackage(ZGuid packageJobPK, ZString packageID, ZInt quantity, ZString packageType, ZDecimal volume, ZString volumeUQ, ZDecimal weight, ZString weightUQ);
	}
}
