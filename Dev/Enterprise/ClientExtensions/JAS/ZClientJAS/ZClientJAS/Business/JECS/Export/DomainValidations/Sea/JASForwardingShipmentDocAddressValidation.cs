
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations
{
	public class JASForwardingSEAShipmentDocAddressValidation : JobDocAddressValidation
	{
		public JASForwardingSEAShipmentDocAddressValidation(JobDocAddress addressToValidate)
			: base(addressToValidate)
		{
		}

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			switch (Parent.E2_AddressType)
			{
				case DocAddressTypes.Codes.ConsigneeDocumentaryAddress:
					ValidateConsigneePK();
					break;

				case DocAddressTypes.Codes.ConsignorDocumentaryAddress:
					ValidateConsignorPK();
					break;
			}
		}

		void ValidateConsignorPK()
		{
			ValidationHelper.AddJXCWarningIfNotEntered(Parent.OrganisationPKInfo);
			if (Parent.Organisation != null)
			{
				CheckFullName(Parent.OrganisationPKInfo, Parent.Organisation);
				CheckMainAddressLine1(Parent.OrganisationPKInfo, Parent.Organisation);
				CheckMainAddressCity(Parent.OrganisationPKInfo, Parent.Organisation);
				CheckStateOrPostCodeExist(Parent.OrganisationPKInfo, Parent.Organisation);
				CheckPortCode(Parent.OrganisationPKInfo, Parent.Organisation);
			}
		}

		void ValidateConsigneePK()
		{
			ValidationHelper.AddJXCWarningIfNotEntered(Parent.OrganisationPKInfo);
			if (Parent.Organisation != null)
			{
				CheckFullName(Parent.OrganisationPKInfo, Parent.Organisation);
				CheckMainAddressLine1(Parent.OrganisationPKInfo, Parent.Organisation);
				CheckMainAddressCity(Parent.OrganisationPKInfo, Parent.Organisation);
				if (Parent.Organisation.MainAddress.OA_PostCode.IsEmpty)
				{
					ValidationHelper.AddJXCWarning(Parent.OrganisationPKInfo, "Post Code cannot be blank");
				}
				if (Parent.Organisation.MainAddress.OA_State.IsEmpty)
				{
					ValidationHelper.AddJXCWarning(Parent.OrganisationPKInfo, "State cannot be blank");
				}
				CheckPortCode(Parent.OrganisationPKInfo, Parent.Organisation);
			}
		}

		#region Implementation

		void CheckFullName(ZPropertyInfo propertyInfo, OrgHeader orgHeader)
		{
			if (orgHeader.OH_FullName.IsEmpty)
			{
				ValidationHelper.AddJXCWarning(propertyInfo, "Organisation Name cannot be blank");
			}
		}

		void CheckMainAddressLine1(ZPropertyInfo propertyInfo, OrgHeader orgHeader)
		{
			if (orgHeader.MainAddress.OA_Address1.IsEmpty)
			{
				ValidationHelper.AddJXCWarning(propertyInfo, "Address Line 1 cannot be blank");
			}
		}

		void CheckMainAddressCity(ZPropertyInfo propertyInfo, OrgHeader orgHeader)
		{
			if (orgHeader.MainAddress.OA_City.IsEmpty)
			{
				ValidationHelper.AddJXCWarning(propertyInfo, "City cannot be blank");
			}
		}

		void CheckStateOrPostCodeExist(ZPropertyInfo propertyInfo, OrgHeader orgHeader)
		{
			if (orgHeader.MainAddress.OA_PostCode.IsEmpty && orgHeader.MainAddress.OA_State.IsEmpty)
			{
				string errorMessage = "Post Code and State cannot both be blank";
				ValidationHelper.AddJXCWarning(propertyInfo, errorMessage);
			}
		}

		void CheckPortCode(ZPropertyInfo propertyInfo, OrgHeader orgHeader)
		{
			if (orgHeader.OH_RL_NKClosestPort.IsEmpty)
			{
				ValidationHelper.AddJXCWarning(propertyInfo, "Port Code cannot be blank");
			}
		}

		ValidationHelper ValidationHelper
		{
			get
			{
				if (fValidationHelper == null)
				{
					fValidationHelper = new ValidationHelper();
				}
				return fValidationHelper;
			}
		}
		ValidationHelper fValidationHelper;

		#endregion
	}
}

#region Implementation
#endregion
