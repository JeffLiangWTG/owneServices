using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISLoadingEstablishmentAddressRequirement : JobDocAddressRequirement
	{
		public AQISLoadingEstablishmentAddressRequirement() : base(DocAddressType.AQISLoadingEstablishment)
		{
			GetRegistrationNumberResult = GetRegistrationNumberResultFunc;
			ValidateCity = NoValidation;
			ValidateAddress1 = NoValidation;
			ValidateCompanyName = NoValidation;
		}

		void NoValidation(JobDocAddressValidation validation)
		{
		}

		RegistrationNumberResult GetRegistrationNumberResultFunc(JobDocAddress docAddress)
		{
			var result = new RegistrationNumberResult(docAddress.Factory, true, () =>
			{
				var result = new RegistrationNumber();
				result.NumberType = OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber;
				result.Number = docAddress.Address?.CustomsCodes.GetCustomsRegNo(OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber, Core.Constants.CountryCodes.Australia) ?? ZString.Empty;
				return result;
			});
			return result;
		}
	}
}
