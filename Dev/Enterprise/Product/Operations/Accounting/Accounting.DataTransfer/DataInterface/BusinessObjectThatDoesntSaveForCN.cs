using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes;

namespace Enterprise.Accounting.DataTransfer.DataInterface
{
	public class BusinessObjectThatDoesntSaveForCN : BusinessObjectThatDoesntSave
	{
		public BusinessObjectThatDoesntSaveForCN(BusinessObjectFactory factory)
			: base(factory)
		{ }

		public ZBool ChartType { get; set; }
		public ZGuid BranchPK { get; set; }
		public ZString BranchCode { get; set; }
		public ZInt Period { get; set; }

		#region FromDate ToDate
		public ZDateTime FromDate { get; set; }
		public ZDateTime ToDate { get; set; }
		#endregion
	}
}
