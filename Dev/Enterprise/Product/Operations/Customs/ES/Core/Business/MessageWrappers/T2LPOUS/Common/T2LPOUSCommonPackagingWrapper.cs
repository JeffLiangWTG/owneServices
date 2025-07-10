using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class T2LPOUSCommonPackagingWrapper : IT2LPOUSCommonPackaging
	{
		public T2LPOUSCommonPackagingWrapper(ZString packageType, ZInt packageNum, BusinessObjectFactory factory)
		{
			TypeOfPackages = packageType;
			NumberOfPackages = packageNum;
			NumberOfPackagesValueSpecified = !PackageHelper.PackTypeIsBulk(packageType, factory);
		}

		public ZString TypeOfPackages { get; }

		public ZInt NumberOfPackages { get; }

		public ZBool NumberOfPackagesValueSpecified { get; }
	}
}
