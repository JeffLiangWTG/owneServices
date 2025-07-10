using CargoWise.Types;

namespace Enterprise.DocumentWrappers
{
	class PackageProductAttributesInfo
	{
		public ZBool IsSameProductUsedOnAllPackages { get; set; }

		public ZBool IsAttribute1Used { get; set; }
		public ZBool IsSameAttribute1UsedOnAllPackages { get; set; }

		public ZBool IsAttribute2Used { get; set; }
		public ZBool IsSameAttribute2UsedOnAllPackages { get; set; }

		public ZBool IsAttribute3Used { get; set; }
		public ZBool IsSameAttribute3UsedOnAllPackages { get; set; }

		public ZBool IsExpiryDateUsed { get; set; }
		public ZBool IsSameExpiryDateUsedOnAllPackages { get; set; }

		public ZBool IsPackingDateUsed { get; set; }
		public ZBool IsSamePackingDateUsedOnAllPackages { get; set; }

		public ZBool IsTrackedSerialUsed { get; set; }
	}
}
