using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;

namespace Enterprise.Customs.GB.DocumentWrappers.Ccsuk
{
	public class CcsukWrapperForConsignmentReportSplitLine : CcsukWrapper
	{
		public static CcsukWrapperForConsignmentReportSplitLine New(SplitConsignment split, BusinessObjectFactory factory)
		{
			return new CcsukWrapperForConsignmentReportSplitLine(split, factory);
		}

		CcsukWrapperForConsignmentReportSplitLine(SplitConsignment split, BusinessObjectFactory factory)
			: base(split, factory)
		{
			this.split = split;
		}

		public ZString SPLITREFERENCE
		{
			get { return split.SplitReference; }
		}

		readonly SplitConsignment split;
	}
}
