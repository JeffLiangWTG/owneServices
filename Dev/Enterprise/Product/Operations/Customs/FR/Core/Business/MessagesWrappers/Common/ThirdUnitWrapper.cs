using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class ThirdUnitWrapper : ISupplementaryUnit
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public ThirdUnitWrapper(CusEntryLine entryLine)
		{
			this.itemEntryLine = Argument.NotNull(entryLine, "CusEntryLine cannot be null ");
			this.code = entryLine.ThirdUQ.SubstringSafe(0, 3);
			this.qualif = entryLine.ThirdUQ.SubstringSafe(3, 1);
			this.qty = entryLine.ThirdQuantity;
		}

		public ThirdUnitWrapper()
		{
			this.code = ZString.Empty;
			this.qualif = ZString.Empty;
			this.qty = ZDecimal.Zero;
		}

		public ZString Code => code;

		public ZString Qualif => qualif;

		public ZDecimal Qty => qty;

		protected readonly ZString code;
		protected readonly ZString qualif;
		protected readonly ZDecimal qty;
		protected readonly JobComInvoiceLineTax itemJobComInvoiceLineTax;
		protected readonly CusEntryLine itemEntryLine;
	}
}
