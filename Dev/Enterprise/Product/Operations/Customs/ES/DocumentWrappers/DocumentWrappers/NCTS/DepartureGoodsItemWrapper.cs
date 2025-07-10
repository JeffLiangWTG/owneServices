using System.Linq;
using Enterprise.Customs.ES.NCTS.Business;

namespace Enterprise.Customs.ES.DocumentWrappers.NCTS;

public class DepartureGoodsItemWrapper : EU.NCTS.Business.DepartureGoodsItemWrapper
{
	public DepartureGoodsItemWrapper(NctsDepartureCargoDesc line) : base(line)
	{
	}

	protected new NctsDepartureCargoDesc line => (NctsDepartureCargoDesc)base.line;

	protected override EU.NCTS.Business.PackageWrapper[] Getpackages() => line.Packages.OfType<NctsPackage>().Select(p => new PackageWrapper(p, line.IsVehicles)).ToArray();
}
