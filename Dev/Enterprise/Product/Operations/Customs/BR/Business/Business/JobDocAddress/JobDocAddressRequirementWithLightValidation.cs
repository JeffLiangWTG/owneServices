using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.BR.Business
{
	public class JobDocAddressRequirementWithLightValidation : JobDocAddressRequirement
	{
		public JobDocAddressRequirementWithLightValidation(DocAddressType defaultDocAddressType) : base(defaultDocAddressType)
		{
			ValidateCity = novalidation;
			ValidateCompanyName = novalidation;
			ValidateCountry = novalidation;
			ValidatePostCode = novalidation;
			ValidateState = novalidation;
		}

		void novalidation(JobDocAddressValidation validation)
		{ }
	}
}
