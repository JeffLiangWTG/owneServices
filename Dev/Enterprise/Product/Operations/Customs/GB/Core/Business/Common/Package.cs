using CargoWise.Types;
using Enterprise.Customs.EU.Integration.SadH;

namespace Enterprise.Customs.GB.Business.Messaging
{
	public class Package : IPackage
	{
		public void Add(Customs.Business.InvoiceLinePackagePivot pivot)
		{
			PackageCount += pivot.CHC_NumberOfPacks;
			PackageKind = pivot.Package.CW_PackType;
			PackageMarks = pivot.Package.CW_MarksAndNos;
		}

		public ZInt PackageCount
		{
			get;
			set;
		}

		public ZString PackageKind
		{
			get;
			set;
		}

		public ZString PackageMarks
		{
			get;
			set;
		}
	}
}
