using System.Collections.Generic;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	class ISCIPEDBodyEqualityComparer : IMonthlyClosingDecBodyEqualityComparer, IEqualityComparer<ISCIPEDBody>
	{
		public bool Equals(ISCIPEDBody px, ISCIPEDBody py) => ComparerHelper.Compare(px, py, (x, y) =>
			x.ConsignorPK == y.ConsignorPK &&
			base.Equals(x, y));

		public int GetHashCode(ISCIPEDBody obj) => 0;
	}
}
