using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ICargoReportHeader
	{
		bool CanDelaySending
		{
			get;
		}

		ZString ResponsiblePartyID
		{
			get;
		}

		ZString MethodOfPayment
		{
			get;
		}

		ZString Origin
		{
			get;
		}

		ZString Destination
		{
			get;
		}

		ZString Loading
		{
			get;
		}

		ZString Discharge
		{
			get;
		}

		ZString[] Routings
		{
			get;
		}

		ZString FirstArrivalPort
		{
			get;
		}

		ZString ConsigneeName
		{
			get;
		}

		ZString ConsigneeStreet
		{
			get;
		}

		ZString ConsigneeStreet2
		{
			get;
		}

		ZString ConsigneeCity
		{
			get;
		}

		ZString ConsigneePostCode
		{
			get;
		}

		ZString ConsigneeCountry
		{
			get;
		}

		ZString ConsigneeGeneralAddress
		{
			get;
		}

		ZString ConsignorName
		{
			get;
		}

		ZString ConsignorStreet
		{
			get;
		}

		ZString ConsignorStreet2
		{
			get;
		}

		ZString ConsignorCity
		{
			get;
		}

		ZString ConsignorPostCode
		{
			get;
		}

		ZString ConsignorCountry
		{
			get;
		}

		ZString ConsignorGeneralAddress
		{
			get;
		}

		ZString ConsigneeIdentifier { get; }

		ZString ConsigneeABN { get; }

		ZString ConsigneeCAC { get; }

		ZString ConsigneeTIN { get; }

		ZString ConsignorIdentifier { get; }

		ZString ConsignorVendor { get; }

		ZString ConsignorTIN { get; }

		ZString NotifyPartyName { get; }

		ZString NotifyPartyStreet { get; }

		ZString NotifyPartyStreet2 { get; }

		ZString NotifyPartyCity { get; }

		ZString NotifyPartyPostCode { get; }

		ZString NotifyPartyCountry { get; }

		ZString NotifyPartyGeneralAddress { get; }
	}
}
