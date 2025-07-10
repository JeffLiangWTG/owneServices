using System;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.OperationalActions;
using Enterprise.Customs.FR.Messaging.Interfaces.COD;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.COD
{
	public class ApurWrapper : IApur
	{
		public ApurWrapper(CreditCODDataObject item)
		{
			this.item = item;
		}
		readonly CreditCODDataObject item;

		public ZBool IndicateurApurement => item.CreditMethod == CreditMethodList.Codes.CreditPartialAmountFromPreviousEntryLine;

		public ZDecimal Mnt => IndicateurApurement ? item.Amount : ZDecimal.Zero;

		public ZString Refdecapur => IndicateurApurement ? item.ReleasingEntryHeader?.EntryNumber ?? ZString.Empty : ZString.Empty;

		public ZDate ChangementDateLimiteApurement => throw new NotImplementedException();
	}
}
