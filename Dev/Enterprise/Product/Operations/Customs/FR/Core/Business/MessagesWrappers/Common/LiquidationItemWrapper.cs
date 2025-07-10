using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class LiquidationItemWrapper : ILiquidationItem
	{
		public LiquidationItemWrapper(CusEntryLineFee entryLineFee)
		{
			articleNumber = entryLineFee?.EntryLine?.CL_LineNumber ?? 1;
			taxDetail = new TaxationDetailWrapper(entryLineFee);
		}

		public LiquidationItemWrapper(CusEntryHeaderCharges entryHeaderCharge)
		{
			articleNumber = 1;
			taxDetail = new TaxationDetailWrapper(entryHeaderCharge);
		}

		public ZShort ArticleNumber => articleNumber;

		public ITaxationDetail TaxDetail => taxDetail;

		readonly ZShort articleNumber;
		readonly ITaxationDetail taxDetail;
	}
}
