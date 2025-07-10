using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class CA
		{
			public interface IReleaseStatus
			{
				ZString RL_TransactionNumber { get; }

				ZString RL_ServiceOption { get; }

				ZString ProcessingIndicatorCodeDescription { get; }

				ZDateTime RL_ProcessingDate { get; }

				ZDateTime RL_ReleaseDate { get; }

				ZString RL_CargoControlNumber { get; set; }

				ZString RL_DeliveryInstructions { get; }

				ZString RL_ReleaseOffice { get; }

				ZString RL_WarehouseCode { get; }

				IEnumerable<ZString> Containers { get; }

				ZGuid PK { get; }

				ZString GetServiceOptionDescription();

				ZString GetWarehouseCodeDescription();
			}
		}
	}
}
