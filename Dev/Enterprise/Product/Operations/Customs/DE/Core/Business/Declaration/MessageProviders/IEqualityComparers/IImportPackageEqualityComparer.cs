using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business
{
	internal class IImportPackageEqualityComparer : IEqualityComparer<IImportPackage>
	{
		public bool Equals(IImportPackage px, IImportPackage py) => ComparerHelper.Compare(px, py, (x, y) =>
			string.Equals(x.Kind, y.Kind) &&
			x.Quantity.Equals(y.Quantity) &&
			string.Equals(x.MarksNumbers, y.MarksNumbers));

		public int GetHashCode(IImportPackage obj) => 0;
	}
}
