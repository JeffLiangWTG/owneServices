using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ICTOCusHAWBInformationProvider
	{
		ZString MAWB
		{
			get;
		}

		ZDateTime ArrivalDate
		{
			get;
		}

		ZString FlightNumber
		{
			get;
		}
	}
}
