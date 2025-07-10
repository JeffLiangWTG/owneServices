#if NETFRAMEWORK
using CargoWise.Common;
#endif
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders
{
	public class AmendmentObjectWrapper
	{
		public IEnumerable<Difference> Differences { get; set; }
		public IEnumerable<AmendmentObject> AmendmentObjects => Differences?.DistinctBy(x => x.WCOIDPointers.FirstOrDefault()).Select(x => new AmendmentObject(x.Type, x.WCOIDPointers)) ?? new List<AmendmentObject>();
		public XElement Xml { get; set; }
	}
}
