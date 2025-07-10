using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC043CPackagingProvider
	{
		public CC043CPackagingProvider(PackagingType02 packagingType)
		{
			this.packagingType = Argument.NotNull(packagingType, nameof(packagingType));
		}

		readonly PackagingType02 packagingType;

		public ZString SequenceNumber => packagingType.SequenceNumber ?? ZString.Empty;
		public ZString TypeOfPackages => packagingType.TypeOfPackages ?? ZString.Empty;
		public ZString NumberOfPackages => packagingType.NumberOfPackages ?? ZString.Empty;
		public ZString ShippingMarks => packagingType.ShippingMarks ?? ZString.Empty;
	}
}
