using System.Collections.Generic;
using CargoWise.Customs.AR.MessageContracts;

namespace Enterprise.Customs.AR.Manifest.Business
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This class will be used in a future Work Item")]
	public class BLCancellationArgentinaWrapper : ICancellation
	{
		public BLCancellationArgentinaWrapper(AsycudaBill bill)
		{
			this.bill = CargoWise.Common.Argument.NotNull(bill, "AsycudaBill cannot be null");
		}
		readonly AsycudaBill bill;

		string ICancellation.VoyageID => bill.Header.RegistrationNumber;

		IReadOnlyCollection<IPort> ICancellation.HBLPorts => new IPort[1] { new BLCancellationArgentinaPortWrapper(bill) };

		IAuth ICancellation.Authentication => new BLArgentinaWrapperAuth();
	}

	class BLCancellationArgentinaPortWrapper : IPort
	{
		internal BLCancellationArgentinaPortWrapper(AsycudaBill bill)
		{
			this.bill = CargoWise.Common.Argument.NotNull(bill, "AsycudaBill cannot be null");
		}
		readonly AsycudaBill bill;

		string IPort.LoadingPort => bill.Header.AMA_CustomsLoadPort;

		string IPort.HBLNumber => bill.ABL_BillNumber;
	}
}
