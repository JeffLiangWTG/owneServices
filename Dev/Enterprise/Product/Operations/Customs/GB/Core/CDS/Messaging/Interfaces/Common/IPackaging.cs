using CargoWise.Types;
using Enterprise.Customs.EU.Integration.SadH;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface IPackaging
	{
		ZString TypeCode { get; }
		ZDecimal Quantity { get; }
		ZString MarksNumbersID { get; }
	}

	class PackagingWrapper : IPackaging
	{
		PackagingWrapper(IPackage package)
		{
			this.package = package;
		}

		public static PackagingWrapper New(IPackage package)
		{
			return new PackagingWrapper(package);
		}

		ZString IPackaging.TypeCode => package.PackageKind;

		ZDecimal IPackaging.Quantity => (ZDecimal)package.PackageCount;

		ZString IPackaging.MarksNumbersID => package.PackageMarks.StripNewlineCharacters(CDSDataElementsLengths.PackagingMarksNumbersIDMaxLength);

		readonly IPackage package;
	}
}
