using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ICusHAWBInformationProvider
	{
		ZString MAWB
		{
			get;
		}

		ZString HAWB
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
