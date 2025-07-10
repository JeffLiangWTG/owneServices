using CargoWise.Customs.BR.MessageContracts.Duimp.Outgoing;
using CargoWise.Customs.Shared.MessageContracts;

namespace Enterprise.Customs.BR.Business.Duimp
{
	public class LpcoProvider : ILpco
	{
		LpcoProvider(Permit permit)
		{
			this.permit = Argument.NotNull(permit, nameof(permit));
		}
		readonly Permit permit;

		public static LpcoProvider New(Permit permit) => permit == null ? null : new LpcoProvider(permit);

		public string Number => permit.CSI_ReferenceNumber;
	}
}
