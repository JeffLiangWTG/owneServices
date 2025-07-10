using System.Text.RegularExpressions;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	class CcsukAirWaybillValidator : Freight.Business.AirWayBillValidator
	{
		protected override bool IsMatchingBillFormat(string masterBillNumber)
		{
			return Regex.IsMatch(masterBillNumber, @"^(\d{3}|[A-Z]{3})\d{8}$");
		}

		protected override string MAWBLengthWarningMessage
		{
			get { return "MAWB number must be 11 characters"; }
		}

		protected override string InvalidMAWBFormatWarningMessage
		{
			get { return "MAWP must be 3 letters or digits; MAWN must be 8 digits"; }
		}

		protected override ValidationLevel MAWBLengthValidationLevel => ValidationLevel.MessageError;
		protected override ValidationLevel MAWBFormatValidationLevel => ValidationLevel.MessageError;
	}
}
