using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public abstract class BasePackagingProvider : IPackaging
	{
		public BasePackagingProvider(NctsPackage package)
		{
			Package = Argument.NotNull(package, nameof(package));
		}

		public abstract string TypeOfPackages { get; }

		public abstract int? NumberOfPackages { get; }

		public abstract string ShippingMarks { get; }

		public virtual int SequenceNumber => Package.B5_SequenceNumber;

		protected NctsPackage Package { get; private set; }
	}
}
