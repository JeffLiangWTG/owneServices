using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class PackagingWrapper : IPackaging
	{
		protected PackagingWrapper(NctsPackage package)
		{
			this.package = Argument.NotNull(package, nameof(package));
		}
		protected readonly NctsPackage package;

		public static PackagingWrapper New(NctsPackage package) => package == null ? null : new PackagingWrapper(package);

		public virtual string SequenceNumber => string.Empty;

		public virtual string TypeOfPackages => typeOfPackages ?? (typeOfPackages = package.B5_UnitType);
		string typeOfPackages;

		public virtual string NumberOfPackages => numberOfPackages ?? (numberOfPackages = package.B5_UnitCount.ToString());
		string numberOfPackages;

		public virtual string ShippingMarks => shippingMarks ?? (shippingMarks = package.B5_MarksAndNumbers);
		string shippingMarks;
	}
}
