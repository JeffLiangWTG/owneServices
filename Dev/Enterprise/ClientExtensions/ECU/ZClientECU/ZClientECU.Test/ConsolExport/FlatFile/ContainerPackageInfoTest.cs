using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.ECU.ConsolExport.Testing
{
	public class ContainerPackageInfoTest : TestCase
	{
		public void TestConstructor()
		{
			int packageCount = 11;
			ZDecimal volume = 10.55m;
			ZDecimal weight = 20.9m;
			ZString packType = Core.Constants.PkgUnit.Package;
			ContainerPackageInfo contInfo = new ContainerPackageInfo(packageCount, volume, weight, packType);
			AssertNotNull("ContInfo should not be null", contInfo);
			AssertEquals("No. of Package", packageCount, contInfo.PackageCount);
			AssertEquals("Volume", volume, contInfo.Volume);
			AssertEquals("Weight", weight, contInfo.Weight);
			AssertEquals("PackType", packType, contInfo.PackageType);
		}
	}
}
