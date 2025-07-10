using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business
{
	internal class IContentInformationEqualityComparer : IEqualityComparer<IContentInformation>
	{
		public bool Equals(IContentInformation px, IContentInformation py) => ComparerHelper.Compare(px, py, (x, y) =>
			string.Equals(x.ContentType, y.ContentType) &&
			x.DegreePercentage.Equals(y.DegreePercentage));

		public int GetHashCode(IContentInformation obj) => 0;
	}
}
