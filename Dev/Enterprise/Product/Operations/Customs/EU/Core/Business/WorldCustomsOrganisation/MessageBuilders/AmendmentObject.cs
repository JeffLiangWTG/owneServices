using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders
{
	public class AmendmentObject
	{
		public AmendmentObject(ZString type, IEnumerable<ZString> pointers)
		{
			AmendmentType = type;
			Pointers = pointers;
		}

		public ZString AmendmentType { get; set; }
		public IEnumerable<ZString> Pointers { get; set; }
	}
}
