using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IManifestLineWrapper
	{
		int PackCount
		{
			get;
		}

		int ContainerCount
		{
			get;
		}

		ZString CAN
		{
			get;
		}

		ZString CCAN
		{
			get;
		}

		ZString AirWaybillNumber
		{
			get;
		}

		ZString CountryOfDestination
		{
			get;
		}

		ZString GoodsOwner
		{
			get;
		}

		ZString GoodsOwnerPartyID
		{
			get;
		}

		ZString GoodsDescription
		{
			get;
		}

		ZString ExemptionCode
		{
			get;
		}

		int LineNumber
		{
			get;
		}

		ZString Reference
		{
			get;
		}

		ZString HouseBillNumber
		{
			get;
		}

		ZString LineActionCode
		{
			get;
		}
	}
}
