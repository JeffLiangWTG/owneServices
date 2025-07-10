using CargoWise.Customs.CH.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.CH.NCTS.Business;

public class CustomsOfficeReferenceDataProvider : ICustomsOffice
{
	public static CustomsOfficeReferenceDataProvider New(ZString referenceNumber)
	{
		return referenceNumber.IsEmpty ? null : new CustomsOfficeReferenceDataProvider(referenceNumber);
	}

	CustomsOfficeReferenceDataProvider(ZString referenceNumber)
	{
		ReferenceNumber = referenceNumber;
	}

	public string ReferenceNumber { get; }

	public int SequenceNumber => 0;

	public int? DaysFromActivationToArrival => null;
}
