using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class ArrivalTransportMeansValidation : CusTransportMeansValidation
	{
		public ArrivalTransportMeansValidation(AutoCusTransportMeans parent) : base(parent)
		{
		}

		public new ArrivalTransportMeans Parent => (ArrivalTransportMeans)base.Parent;

		protected override void CheckTPM_IdentificationNumber()
		{
			base.CheckTPM_IdentificationNumber();
			CheckTPM_IdentificationNumber_NotEntered();
			CheckTPM_IdentificationNumber_IsCorrect();
		}

		void CheckTPM_IdentificationNumber_IsCorrect()
		{
			var parent = Parent;
			var parentHeader = parent.ParentHeader;
			if (parentHeader != null && !parentHeader.IsTransfer && !parentHeader.IsDeconsolidation)
			{
				var identificationNumber = parent.TPM_IdentificationNumber;
				if (parentHeader.TransportType == MeansOfTransportList.Codes.ImoShipIdentificationNumber)
				{
					if (!Regex.IsMatch(identificationNumber, @"^IMO[0-9]{7}$"))
					{
						parent.TPM_IdentificationNumberInfo.AddMessageError(shipIdentificationNumberInvalidFormatMessage);
					}
					else
					{
						char checkDigit;

						if (!TryGetCheckDigit(identificationNumber, out checkDigit))
						{
							parent.TPM_IdentificationNumberInfo.AddMessageError(shipIdentificationNumberInvalidFormatMessage);
						}
						else if (identificationNumber[9] != checkDigit)
						{
							parent.TPM_IdentificationNumberInfo.AddWarning(shipIdentificationNumberInvalidCheckDigitMessage(checkDigit));
						}
					}
				}

				else if (parentHeader.TransportType == MeansOfTransportList.Codes.EuropeanVesselIdentificationNumberEniCode)
				{
					if (!Regex.IsMatch(identificationNumber, @"^[0-9]{8}$"))
					{
						parent.TPM_IdentificationNumberInfo.AddMessageError(europeanVesselIdentificationNumberEniCodeFormatErrorMessage);
					}
				}
			}
		}

		protected virtual void CheckTPM_IdentificationNumber_NotEntered()
		{
			var parent = Parent;
			var parentHeader = parent.ParentHeader;
			if (parentHeader != null && !parentHeader.IsTransfer && !parentHeader.IsDeconsolidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.TPM_IdentificationNumberInfo);
			}
		}

		static bool TryGetCheckDigit(ZString identificationNumber, out char checkDigit)
		{
			checkDigit = '\0';

			var result = 0;

			for (var charPos = 3; charPos < 9; charPos++)
			{
				var charValue = 0;

				if (!Int32.TryParse(identificationNumber.SubstringSafe(charPos, 1), out charValue))
				{
					return false;
				}

				result += (10 - charPos) * charValue;
			}

			checkDigit = (char)(result % 10 + 48);

			return true;
		}

		static MultilingualString shipIdentificationNumberInvalidFormatMessage => ResString.GetMultilingualString("EB1F597B-4234-47DB-8FDD-D8E9AD3FBE6B", "When transport type is 10, the Arrival Transport Means should be started with IMO and followed with 6 numeric digits and a check digit.");
		static MultilingualString shipIdentificationNumberInvalidCheckDigitMessage(char expectedCheckDigit) => ResString.GetMultilingualString("ABFCEC63-B71D-4759-881D-5C4186B99FBF", "Arrival Transport Means does not have a valid check digit. The check digit should be {0}.", expectedCheckDigit);
		static MultilingualString europeanVesselIdentificationNumberEniCodeFormatErrorMessage => ResString.GetMultilingualString("29E6F6A4-9AED-4792-BF0E-8C90905FE7AB", "When transport type is 80, the Arrival Transport Means should be 8 digit number.");
	}
}
