using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.DE.Business
{
	public static class RegistrationNumberValidationHelper
	{
		public const int RegistrationNumberLength = 21;
		public const int MRNLength = 18;

		public static string ValidateRegistrationNumberLengthAndMrnFormat(ZString registrationNumber, BusinessObjectFactory factory)
		{
			var messageError = string.Empty;
			var registrationNumberLength = registrationNumber.Length;
			if (registrationNumberLength == MRNLength)
			{
				messageError = MRNFormatValidator.CheckMRNFormat(registrationNumber, factory, ZString.Empty);
			}
			else if (registrationNumberLength != RegistrationNumberLength)
			{
				messageError = Res.GetString("8F05255F-2B7C-4CCD-8AB7-BAA4DD2629B5", "The Reference must have 18 characters (MRN) or 21 characters (ATLAS Reg. No.).");
			}
			return messageError;
		}

		public static bool IsRegistrationNumberAWorkingNumber(string registrationNumber)
		{
			if (registrationNumber == null)
			{
				return false;
			}
			var registrationNumberLength = registrationNumber.Length;
			return registrationNumberLength == RegistrationNumberLength && registrationNumber.StartsWith(UniversalReferenceConstants.ATLASReferenceNumberIdentifier.ATA)
				|| registrationNumberLength == MRNLength && registrationNumber.Substring(9,1) == "A";
		}
	}
}
