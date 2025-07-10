using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgMatchings.MatchProduct
{
	class Part : IPart
	{
		#region IPart Members

		public IOrgHeader Buyer { get; set; }

		public ZString Description { get; set; }

		public ZString PartNum { get; set; }

		public IOrgHeader Supplier { get; set; }

		public ZString StockKeepingUnit { get; set; }

		#endregion
	}
}