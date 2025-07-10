using CargoWise.Types;

namespace Enterprise.Integration.Packing
{
	public interface IPkgPackageJob { }
	public interface IPkgPackage
	{
		ZString KP_PackageID { get; set; }
		ZString PackageIDWithFallback { get; }
		ZString KP_F3_NKPackType { get; set; }
	}
	public interface IPackageDamagedReasonsCodeDescriptionPairProvider { }
}
