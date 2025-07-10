using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class PackageCommonWrapper : IPackageCommon
	{
		public PackageCommonWrapper(ZString packageType, ZString packageMarks)
		{
			PackageType = packageType;
			Marks = packageMarks;
		}

		public ZString PackageType { get; }

		public ZString Marks { get; }
	}
}
