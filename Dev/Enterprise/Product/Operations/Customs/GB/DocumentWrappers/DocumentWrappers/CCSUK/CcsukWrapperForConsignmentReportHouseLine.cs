using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;

namespace Enterprise.Customs.GB.DocumentWrappers.Ccsuk
{
	public class CcsukWrapperForConsignmentReportHouseLine : CcsukWrapper
	{
		public static CcsukWrapperForConsignmentReportHouseLine New(CusHAWB hawb, BusinessObjectFactory factory)
		{
			return new CcsukWrapperForConsignmentReportHouseLine(hawb, factory);
		}

		CcsukWrapperForConsignmentReportHouseLine(CusHAWB hawb, BusinessObjectFactory factory)
			: base(hawb, factory)
		{
			this.hawb = hawb;
		}

		public ZString HAWB
		{
			get { return hawb.CS_HAWB; }
		}

		readonly CusHAWB hawb;
	}
}
