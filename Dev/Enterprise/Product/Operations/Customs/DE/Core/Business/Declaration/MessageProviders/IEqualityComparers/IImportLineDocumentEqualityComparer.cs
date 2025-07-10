using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business
{
	internal class IImportLineDocumentEqualityComparer : IEqualityComparer<IImportLineDocument>
	{
		public bool Equals(IImportLineDocument px, IImportLineDocument py) => ComparerHelper.Compare(px, py, (x, y) =>
			string.Equals(x.Division, y.Division) &&
			string.Equals(x.DocumentType, y.DocumentType) &&
			string.Equals(x.ReferenceNumber, y.ReferenceNumber) &&
			x.IssuingDate.Equals(y.IssuingDate) &&
			string.Equals(x.AtHandFlag, y.AtHandFlag) &&
			new IAmountEqualityComparer().Equals(x.WriteOff, y.WriteOff));

		public int GetHashCode(IImportLineDocument obj) => 0;
	}
}
