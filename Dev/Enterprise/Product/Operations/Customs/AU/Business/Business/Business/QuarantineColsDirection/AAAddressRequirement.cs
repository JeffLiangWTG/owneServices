using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AAAddressRequirement : JobDocAddressRequirement
	{
		public AAAddressRequirement(QuarantineColsDirection addressParent) : base(DocAddressType.COLSDirectionAAAddress)
		{
			this.addressParent = addressParent;
			Initialize();
		}
		readonly QuarantineColsDirection addressParent;

		void Initialize()
		{
			ValidateGovRegNo = ValidateE2_GovRegNumType;
			ValidateAddress1 = ValidateE2_Address1;
			ValidateCity = ValidateE2_City;
			ValidateCompanyName = ValidateE2_CompanyName;
			ValidatePostCode = ValidateE2_Postcode;
			ValidateState = ValidateE2_State;
		}

		void ValidateE2_GovRegNumType(JobDocAddressValidation validation)
		{
			if (addressParent.QCD_Direction != "Release on Documents")
			{
				MandatoryValidation.MessageErrorIfNotEntered(validation.Parent.E2_GovRegNumInfo, addressParent.AAIdInfo.HumanReadableName);
			}
		}

		void ValidateE2_Address1(JobDocAddressValidation validation)
		{
			var maxLength = QuarantineColsDirection.AALocationMaxLength;
			if (addressParent.AALocation.Length > maxLength)
			{
				validation.Parent.E2_Address1Info.AddMessageError(Res.GetString("AAAddressRequirement|ValidateE2_Address1", "The maximum length {0} has been exceeded.", maxLength));
			}
		}

		void ValidateE2_City(JobDocAddressValidation validation)
		{
		}

		void ValidateE2_CompanyName(JobDocAddressValidation validation)
		{
			var maxLength = QuarantineColsDirection.AANameMaxLength;
			if (addressParent.AAName.Length > maxLength)
			{
				validation.Parent.E2_CompanyNameInfo.AddMessageError(Res.GetString("AAAddressRequirement|ValidateE2_CompanyName", "The maximum length {0} has been exceeded.", maxLength));
			}
		}

		void ValidateE2_Postcode(JobDocAddressValidation validation)
		{
		}

		void ValidateE2_State(JobDocAddressValidation validation)
		{
		}
	}
}
