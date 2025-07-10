using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EstablishmentCodeValidation : MasterFiles.Business.EstablishmentCodeValidator
	{
		public EstablishmentCodeValidation(IFactoryProvider factoryProvider)
			: base(factoryProvider)
		{
		}

		public const string InvalidEstablishmentCodeNNNNA = "Invalid Establishment Code. Expected Checksum is: ";
		public const string InvalidEstablishmentCodeAANNA = InvalidEstablishmentCodeNNNNA;
		public const string EstablishmentCodeDoesNotMatchCustomsDefinition = "Establishment Code does not match customs definition and may be incorrect. Expected Checksum is: ";
		public const string UnrecognisedCodeFormat = "Unrecognised Establishment Code Format.  Expected NNNNA, ANNNA, or AANNA";

		public bool ValidateEstablishmentCode(IZPropertyInfo establishmentCodeInfo)
		{
			var result = true;
			var establishmentCode = (ZString)establishmentCodeInfo.Value;
			if (establishmentCode != "")
			{
				char checksum;
				if (CodeIsNNNNA(establishmentCode))
				{
					if (!EstablishmentCodeValidNNNNA(establishmentCode, out checksum))
					{
						establishmentCodeInfo.AddMessageError(InvalidEstablishmentCodeNNNNA + checksum);
						result = false;
					}
				}
				else if (CodeIsANNNA(establishmentCode))
				{
					if (!EstablishmentCodeValidANNNA(establishmentCode, out checksum))
					{
						establishmentCodeInfo.AddWarning(EstablishmentCodeDoesNotMatchCustomsDefinition + checksum);
						result = false;
					}
				}
				else if (CodeIsAANNA(establishmentCode))
				{
					if (!EstablishmentCodeValidAANNA(establishmentCode, out checksum))
					{
						establishmentCodeInfo.AddMessageError(InvalidEstablishmentCodeAANNA + checksum);
						result = false;
					}
				}
				else
				{
					establishmentCodeInfo.AddMessageError(UnrecognisedCodeFormat);
					result = false;
				}
			}
			return result;
		}
	}
}
