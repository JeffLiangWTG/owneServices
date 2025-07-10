using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImportEntryOrEntryLine
	{
		public ZDateTime PaidDate { get; }
		public ICharges PaidAmounts { get; }
		public IEnumerable<IChargesIn5WN> RefundAmounts { get; }
	}
}
