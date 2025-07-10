using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;

namespace Enterprise.Customs.JP.Manifest.Business
{
	sealed class LocationProvider : ILocation
	{
		public LocationProvider(AsycudaBill bill, string type)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
			this.type = Argument.NotNull(type, nameof(type));
		}

		readonly AsycudaBill bill;
		readonly string type;

		public string Code
		{
			get
			{
				var result = string.Empty;
				switch (type)
				{
					case nameof(BillProvider.FinalDestination):
						result = bill.ABL_RL_NKFinalDestination;
						break;
					case nameof(BillProvider.PlaceOfDelivery):
						result = bill.ABL_RL_NKPortOfDischarge;
						break;
					default:
						break;
				}
				return result;
			}
		}

		public string Name => string.Empty;
	}
}
