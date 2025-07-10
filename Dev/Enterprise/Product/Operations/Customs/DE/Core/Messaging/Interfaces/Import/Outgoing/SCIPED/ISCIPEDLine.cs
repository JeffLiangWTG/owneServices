using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Messaging
{
	public interface ISCIPEDLine : IMonthlyClosingDecLine
	{
		string RequestedPreferentialTreatment { get; }

		IAmount InwardMovementAmount { get; }
	}
}
