using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class PackagingWrapper : IPackaging
	{
		PackagingWrapper(InvoiceLineCusLinkPackage package, int numberOfPackages)
		{
			this.package = Argument.NotNull(package, nameof(package));
			this.numberOfPackages = numberOfPackages;
		}

		public static PackagingWrapper New(InvoiceLineCusLinkPackage package, int numberOfPackages) => (package == null || package.Package == null) ? null : new PackagingWrapper(package, numberOfPackages);

		public string NumberOfPackages => numberOfPackages.ToString();

		public string ShippingMarks => shippingMarks ?? (shippingMarks = package.Package == null ? string.Empty : package.Package.CW_MarksAndNos.ToString());
		string shippingMarks;

		public string TypeOfPackages => typeOfPackages ?? (typeOfPackages = package.Package == null ? string.Empty : package.Package.CW_PackType.ToString());
		string typeOfPackages;

		readonly InvoiceLineCusLinkPackage package;
		readonly int numberOfPackages;
	}
}
