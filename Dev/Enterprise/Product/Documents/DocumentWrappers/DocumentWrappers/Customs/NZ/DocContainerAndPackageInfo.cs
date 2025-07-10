using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentWrappers.Customs.NZ
{
	[AllowPublicConstructor]
	[AllowNoStaticNew]
	public class DocContainerAndPackageInfo : DocBaseWrapper
	{
		public DocContainerAndPackageInfo(BusinessObjectFactory factoryToWrap, ZString billNumber, ZString billType, ZString containerNumber, ZString containerStatus, ZString packagesAndType)
			: base(null, factoryToWrap)
		{
			fBillNumber = billNumber;
			fBillType = billType;
			fContainerNumber = containerNumber;
			fContainerStatus = containerStatus;
			fPackagesAndType = packagesAndType;
		}

		public ZString BillNumber
		{
			get { return fBillNumber; }
		}

		public ZString BillType
		{
			get { return fBillType; }
		}

		public ZString ContainerNumber
		{
			get { return fContainerNumber; }
		}

		public ZString ContainerStatus
		{
			get { return fContainerStatus; }
		}

		public ZString PackagesAndType
		{
			get { return fPackagesAndType; }
		}

		protected readonly ZString fBillNumber;
		protected readonly ZString fBillType;
		protected readonly ZString fContainerNumber;
		protected readonly ZString fContainerStatus;
		protected readonly ZString fPackagesAndType;
	}
}
