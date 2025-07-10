using System.Globalization;
using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class REXDISCusTempStorageJobHeaderProvider : SumACusTempStorageJobHeaderProvider, IREXDISTempStorageHeader
	{
		public REXDISCusTempStorageJobHeaderProvider(CusTempStorageJobHeader tempStorageHeader)
			: base(tempStorageHeader)
		{
		}

		public string FlightReferenceNumber
		{
			get
			{
				string result = null;
				var transportRegistrationNumber = TempStorageHeader.SJH_TransportRegNo;
				if (!transportRegistrationNumber.IsEmpty)
				{
					var carrierCode = transportRegistrationNumber.Left(3).IsLettersOnlyOrEmpty ? transportRegistrationNumber.Left(3) : transportRegistrationNumber.Left(2);
					var additionalCode = transportRegistrationNumber.Right(1).IsLettersOnlyOrEmpty ? transportRegistrationNumber.Right(1) : ZString.Empty;
					var flightNumber = transportRegistrationNumber.SubstringSafe(carrierCode.Length, transportRegistrationNumber.Length - carrierCode.Length - additionalCode.Length);
					result = string.Join(" ", carrierCode, flightNumber);
					if (!additionalCode.IsEmpty)
					{
						result += " " + additionalCode;
					}
					var departureDate = TempStorageHeader.SJH_DepartureDate;
					if (departureDate.IsValid)
					{
						result += " " + departureDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
					}
				}
				return result;
			}
		}

		public string CustomsAuthorisationNumber
		{
			get
			{
				var result = string.Empty;
				var presenter = TempStorageHeader.Presenter?.Header.PK ?? ZGuid.Empty;
				if (!presenter.IsEmpty)
				{
					result = CusAuthorisationHeader.Loader.GetAuthorisationNumber(TempStorageHeader.Factory,
						Core.Constants.CountryCodes.Germany,
						CusAuthorizationHeaderTypeList.Codes.ElectronicTransportDocument,
						TempStorageHeader.SJH_DepartureDate,
						presenter);
				}
				return result;
			}
		}
	}
}
