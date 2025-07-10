using CargoWise.EntityFramework;
namespace Enterprise.Customs.GB.CDS.MessagingRules
{
	using CargoWise.Types;
	using Enterprise.MasterFiles.Business;
	using EU.Business;

	public static class EoriCodeValidationHelper
	{
		/// <summary>
		/// Checks that the organisation has an EORI code.  Pass in declaration.Importer if you want to ensure the importer has a TURN (ie for imports),
		/// pass in the supplier if you want to ensure the supplier has an EORI (ie for exports)
		/// </summary>
		public static void CheckNoEoriCodeInPropertyInfo(JobDocAddress docAddress, ZPropertyInfo infoToWhichTheErrorMustBeAdded)
		{
			string eoriCode = docAddress.E2_AddressOverride ? docAddress.E2_GovRegNum : docAddress.Organisation.GetEuIdentificationNumber();
			CheckNoEoriCodeInPropertyInfo(eoriCode, infoToWhichTheErrorMustBeAdded);
		}

		public static void CheckNoEoriCodeInPropertyInfo(ZString eoriCode, ZPropertyInfo infoToWhichTheErrorMustBeAdded)
		{
			if (string.IsNullOrEmpty(eoriCode))
			{
				if (!infoToWhichTheErrorMustBeAdded.HasError(ErrorMessageNoEoriCode))
				{
					infoToWhichTheErrorMustBeAdded.AddMessageError(ErrorMessageNoEoriCode);
				}
			}
		}

		public const string ErrorMessageNoEoriCode = "You need to supply an EORI code for this party. Open the party for editing, chose 'config', 'registration numbers' and enter a registration code of type 'EOR'.";
	}
}
