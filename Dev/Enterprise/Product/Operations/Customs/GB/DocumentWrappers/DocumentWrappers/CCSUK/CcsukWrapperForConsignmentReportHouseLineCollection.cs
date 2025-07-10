using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.GB.DocumentWrappers.Ccsuk
{
	public class CcsukWrapperForConsignmentReportHouseLineCollection : DocBaseWrapperCollection<CcsukWrapperForConsignmentReportHouseLine>
	{
		public CcsukWrapperForConsignmentReportHouseLineCollection(CusMAWB mawb, BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (CusHAWB hawb in mawb.ChildBills)
			{
				Add(CcsukWrapperForConsignmentReportHouseLine.New(hawb, factory));
			}
		}
	}
}
