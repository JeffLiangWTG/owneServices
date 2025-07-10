using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public sealed class CUSPRLCusTempStorageJobHeaderProvider : SumACusTempStorageJobHeaderProvider, ICUSPRLTempStorageHeader
	{
		public CUSPRLCusTempStorageJobHeaderProvider(CusTempStorageJobHeader tempStorageHeader)
			: base(tempStorageHeader)
		{
		}

		public int BorderTransportMode
		{
			get
			{
				var result = 0;
				switch (TempStorageHeader.SJH_TransportMode)
				{
					case TransportTypeList.Codes.Sea:
						result = 1;
						break;
					case TransportTypeList.Codes.Rail:
						result = 2;
						break;
					case TransportTypeList.Codes.Road:
						result = 3;
						break;
					case TransportTypeList.Codes.Air:
						result = 4;
						break;
					case TransportTypeList.Codes.Mail:
						result = 5;
						break;
					case TransportTypeList.Codes.FixedTransportInstallations:
						result = 7;
						break;
					case TransportTypeList.Codes.InlandWaterwayTransport:
						result = 8;
						break;
					case TransportTypeList.Codes.OwnPropulsion:
						result = 9;
						break;
				}
				return result;
			}
		}

		public string ConveyanceReferenceNumber
		{
			get
			{
				var result = string.Empty;
				var transportRegNo = TempStorageHeader.SJH_TransportRegNo;
				if (!transportRegNo.IsEmpty)
				{
					switch (TempStorageHeader.SJH_TransportMeansCode)
					{
						case TemporaryStorageTransportMeansList.Codes.Vessel:
							var vessel = TempStorageHeader.Factory.LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, transportRegNo);
							if (vessel != null)
							{
								result = vessel.RV_LloydsNumber;
							}
							break;
						case TemporaryStorageTransportMeansList.Codes.Aircraft:
							result = transportRegNo;
							break;
					}
				}
				return result;
			}
		}
	}
}
