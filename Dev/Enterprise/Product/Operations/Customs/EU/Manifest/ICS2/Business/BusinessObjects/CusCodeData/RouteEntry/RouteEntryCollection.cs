using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class RouteEntryCollection : CusCodeDataCollection<RouteEntry>, ISequenceNumberHeader
	{
		public RouteEntryCollection(BusinessObject master)
			: base(master, CusCodeDataTypeList.Codes.EUICS2RouteEntry)
		{
			MaxCountValidationEnable(99);
		}

		#region ISequenceNumberHeader

		public ShortSequenceNumberGenerator SequenceNumberCalculator
		{
			get { return sequenceNumberCalculator ?? (sequenceNumberCalculator = new ShortSequenceNumberGenerator(this)); }
		}
		ShortSequenceNumberGenerator sequenceNumberCalculator;

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => this.Cast<ISequenceNumberLine>();

		#endregion
	}
}
