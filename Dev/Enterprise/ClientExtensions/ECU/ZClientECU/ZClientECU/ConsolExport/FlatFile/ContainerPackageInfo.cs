using CargoWise.Types;

namespace Enterprise.Client.ECU.ConsolExport
{
	public class ContainerPackageInfo
	{
		public ContainerPackageInfo(int packageCount, ZDecimal volume, ZDecimal weight, ZString packageType)
		{
			fPackageCount = packageCount;
			fVolume = volume;
			fWeight = weight;
			fPackageType = packageType;
		}

		public ZDecimal Volume
		{
			get { return fVolume; }
		}

		public ZDecimal Weight
		{
			get { return fWeight; }
		}

		public int PackageCount
		{
			get { return fPackageCount; }
		}

		public ZString PackageType
		{
			get { return fPackageType; }
		}

		readonly int fPackageCount;
		readonly ZDecimal fVolume;
		readonly ZDecimal fWeight;
		readonly ZString fPackageType;
	}
}
