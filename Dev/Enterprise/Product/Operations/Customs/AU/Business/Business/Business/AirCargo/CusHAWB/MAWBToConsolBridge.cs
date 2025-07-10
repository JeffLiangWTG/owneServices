using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class MAWBToConsolBridge : IUpdateFromConsol
	{
		public MAWBToConsolBridge(CusMAWB mAWB)
		{
			this.mAWB = Argument.NotNull(mAWB, "MAWB cannot be null");
			fIsAir = true;
		}
		readonly CusMAWB mAWB;

		#region Should We Synchronise

		public bool ShouldWeSynchronise
		{
			get
			{
				bool result = IsAir && !mAWB.IsDeleted;
				if (result)
				{
					FetchForMessages();
					foreach (CusHAWB hAWB in mAWB.ChildBills)
					{
						ZString cMRMessageStatus = hAWB.CMRMessageStatus.Code;
						if (cMRMessageStatus == CMRBaseStatuses.Codes.OriginalAccepted ||
							cMRMessageStatus == CMRBaseStatuses.Codes.AmendmentAccepted ||
							cMRMessageStatus == CMRBaseStatuses.Codes.AwaitingResponseToOriginal ||
							cMRMessageStatus == CMRBaseStatuses.Codes.AwaitingResponseToAmendment)
						{
							result = false;
							break;
						}
					}
				}
				return result;
			}
		}

		bool hasRunFetchForMessages;
		void FetchForMessages()
		{
			if (!hasRunFetchForMessages)
			{
				foreach (CusHAWB hawb in mAWB.ChildBills)
				{
					BusinessObjectFactory factory = hawb.Factory;
					factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, hawb.PK);
				}
				hasRunFetchForMessages = true;
			}
		}

		#endregion

		#region IAmConsolSynchroniser Members

		public ZDateTime ArrivalDate
		{
			set
			{
				if (ShouldWeSynchronise)
				{
					mAWB.CM_ArrivalDate = value;
				}
			}
		}

		public ZDateTime DepartureDate
		{
			set
			{
				if (ShouldWeSynchronise)
				{
					mAWB.CM_DepartureDate = value;
				}
			}
		}

		public ZString LoadPort
		{
			set
			{
				if (ShouldWeSynchronise)
				{
					mAWB.CM_RL_NKLoadPort = value;
				}
			}
		}

		public ZString MAWBNumber
		{
			set
			{
				if (ShouldWeSynchronise)
				{
					mAWB.CM_MAWB = value;
				}
			}
		}

		public ZString FlightNumber
		{
			set
			{
				if (ShouldWeSynchronise)
				{
					mAWB.CM_FlightNo = value;
				}
			}
		}

		public ZString DischargePort
		{
			set
			{
				if (ShouldWeSynchronise)
				{
					mAWB.CM_RL_NKDischargePort = value;
				}
			}
		}

		bool fIsAir;
		public bool IsAir
		{
			get { return fIsAir; }
			set { fIsAir = value; }
		}

		public void UpdateLoadPort(Transport transport)
		{
			var consol = transport.Parent as ForwardingConsol;
			if (consol != null && consol.JK_TransportMode == Core.Constants.TransportModes.Air)
			{
				LoadPort = consol.JK_RL_NKLoadForFirstImportTransport;
			}
		}

		public void UpdateDischargePort(Transport transport)
		{
			var consol = transport.Parent as ForwardingConsol;
			if (consol != null && consol.JK_TransportMode == Core.Constants.TransportModes.Air)
			{
				DischargePort = consol.JK_RL_NKDiscForFirstImportTransport;
			}
		}
		#endregion
	}
}
