using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class SupplementaryUnitWrapper : ISupplementaryUnit
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public SupplementaryUnitWrapper(CusEntryLine entryLine)
		{
			this.itemEntryLine = Argument.NotNull(entryLine, "CusEntryLine cannot be null ");
			this.code = entryLine.SupplementaryUQ.SubstringSafe(0, 3);
			this.qualif = entryLine.SupplementaryUQ.SubstringSafe(3, 1);
			this.qty = entryLine.SupplementaryQuantity;
		}

		public SupplementaryUnitWrapper(CusEntryLineFee fee)
		{
			this.fee = Argument.NotNull(fee, "Fee cannot be null ");
			this.code = fee.CF_MethodOfCalculation.SubstringSafe(0, 3);
			this.qualif = fee.CF_MethodOfCalculation.SubstringSafe(3, 1);
			this.qty = fee.G4_BaseAmount;
		}

		public SupplementaryUnitWrapper()
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
		protected readonly CusEntryLineFee fee;
		protected readonly CusEntryLine itemEntryLine;
	}
}
