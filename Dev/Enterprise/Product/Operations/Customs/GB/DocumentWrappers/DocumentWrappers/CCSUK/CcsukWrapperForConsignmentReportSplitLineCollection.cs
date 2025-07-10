using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.GB.DocumentWrappers.Ccsuk
{
	public class CcsukWrapperForConsignmentReportSplitLineCollection : DocBaseWrapperCollection<CcsukWrapperForConsignmentReportSplitLine>
	{
		public CcsukWrapperForConsignmentReportSplitLineCollection(ICcsukCusAwb awb, BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (SplitConsignment split in awb.Splits)
			{
				Add(CcsukWrapperForConsignmentReportSplitLine.New(split, factory));
			}
		}
	}
}
