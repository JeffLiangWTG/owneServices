using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Data.Mutex;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class SeaCargoProcessorJobForConsol : SeaCargoMessageProcessorJobBase, Integration.Customs.AU.ISeaCargoMessageProcessorJob
	{
		public SeaCargoProcessorJobForConsol(ForwardingConsol consol)
		{
			this.consol = Argument.NotNull(consol, "consol");
		}

		#region Implementation

		readonly ForwardingConsol consol;

		static bool IsSAC(ForwardingShipment shipment)
		{
			bool result = false;

			if (shipment != null)
			{
				StmALog log = shipment.Logs.MostRecentLogByEventTime(Events.DataImport, "AU Declaration Style: SAC");
				if (log != null)
				{
					result = !log.IsInDatabase;
				}
			}

			return result;
		}

		protected override bool IsDischargedAtAustralianPort
		{
			get
			{
				return consol.JK_JX_JB_RL_NKPortOfDischarge.StartsWith(Core.Constants.CountryCodes.Australia);
			}
		}

		protected override bool IsSea => consol.IsSea;

		protected override CusSCAOceanBill OceanBillCore => Synchroniser.OceanBill;

		protected override bool HasOceanBill => Synchroniser.ExistingOceanBill != null;

		public CMRSeaCargoSynchroniser Synchroniser => synchroniser ?? (synchroniser = new CMRSeaCargoSynchroniser(consol));
		CMRSeaCargoSynchroniser synchroniser;

		protected override ZGlobalMutex MutexCore => mutex ?? (mutex = CusSCAOceanBill.CreateMutexForConsol(consol.PK, Core.Constants.CountryCodes.Australia));
		ZGlobalMutex mutex;

		protected override ZString JobNumberCore => HasOceanBill ? OceanBill.CB_OceanBill : consol.JK_MasterBillNum;

		protected override void SetSACIfRequiredCore()
		{
			foreach (CusSCAHouse cusSCAHouse in this.OceanBill.HouseBills)
			{
				if (SeaCargoProcessorJobForConsol.IsSAC(cusSCAHouse.Shipment))
				{
					foreach (CusSCAPivot pivot in cusSCAHouse.Pivot)
					{
						pivot.CV_IsSAC = true;
					}
				}
			}
		}

		#endregion // Implementation
	}
}
