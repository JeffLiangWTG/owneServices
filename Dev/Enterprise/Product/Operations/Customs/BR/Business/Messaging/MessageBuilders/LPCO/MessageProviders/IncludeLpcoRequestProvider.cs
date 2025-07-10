using System;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.LPCO.Outgoing;

namespace Enterprise.Customs.BR.Business.LPCO
{
	public class IncludeLpcoRequestProvider : IIncludeLpcoRequest
	{
		public IncludeLpcoRequestProvider(LPCOMessageSendingObject sendingObject)
		{
			lpcoHeader = Argument.NotNull(sendingObject, nameof(sendingObject)).Parent.LPCOHeader;
		}

		readonly CusLPCOHeader lpcoHeader;

		public DateTime? ReferenceDate => lpcoHeader.CPH_RetroactiveDate.IsEmpty ? null : lpcoHeader.CPH_RetroactiveDate.ToDateTime();
	}
}
