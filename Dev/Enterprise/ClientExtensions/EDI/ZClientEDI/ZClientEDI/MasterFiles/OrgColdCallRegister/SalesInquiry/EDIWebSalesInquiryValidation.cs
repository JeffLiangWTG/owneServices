using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIWebSalesInquiryValidation : SalesEnquiryValidation
	{
		public EDIWebSalesInquiryValidation(EDIWebSalesInquiry parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly EDIWebSalesInquiry parent;

		#region O1_Address1

		protected override void CheckO1_Address1()
		{
			base.CheckO1_Address1();
			if (parent.O1_Address1_IsMandatory)
			{
				MandatoryValidation.CheckEntered(parent.O1_Address1Info);
			}
		}

		#endregion

		#region O1_City

		protected override void CheckO1_City()
		{
			base.CheckO1_City();
			MandatoryValidation.CheckEntered(parent.O1_CityInfo);
		}

		#endregion

		#region O1_CompanyName

		protected override void CheckO1_CompanyName()
		{
			base.CheckO1_CompanyName();
			MandatoryValidation.CheckEntered(parent.O1_CompanyNameInfo);
		}

		#endregion

		#region O1_ContactName

		protected override void CheckO1_ContactName()
		{
			base.CheckO1_ContactName();
			MandatoryValidation.CheckEntered(parent.O1_ContactNameInfo);
		}

		#endregion

		#region O1_Email

		protected override void CheckO1_Email()
		{
			base.CheckO1_Email();
			MandatoryValidation.CheckEntered(parent.O1_EmailInfo);
		}

		#endregion

		#region O1_State

		protected override void CheckO1_State()
		{
			base.CheckO1_State();
			MandatoryValidation.CheckEntered(parent.O1_StateInfo);
		}

		#endregion

		#region O1_PostCode

		protected override void CheckO1_PostCode()
		{
			base.CheckO1_PostCode();
			if (parent.O1_PostCode_IsMandatory)
			{
				MandatoryValidation.CheckEntered(parent.O1_PostCodeInfo);
			}
		}

		#endregion

		#region O1_Phone

		protected override void CheckO1_Phone()
		{
			base.CheckO1_Phone();
			MandatoryValidation.CheckEntered(parent.O1_PhoneInfo);
		}

		public void ValidateWorkPhoneNationalCode()
		{
			parent.WorkPhoneNationalCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(parent.WorkPhoneNationalCodeInfo);
		}

		#endregion

		#region Additional Details

		public void ValidateJobTitle()
		{
			parent.JobTitleInfo.ClearAllNotifications();
			if (parent.JobTitle_IsMandatory)
			{
				MandatoryValidation.CheckEntered(parent.JobTitleInfo);
			}
		}

		public void ValidateJobRole()
		{
			parent.JobRoleInfo.ClearAllNotifications();
			if (parent.JobRole_IsMandatory)
			{
				MandatoryValidation.CheckEntered(parent.JobRoleInfo);
			}
		}

		public void ValidateCompanySize()
		{
			parent.CompanySizeInfo.ClearAllNotifications();
			if (parent.CompanySize_IsMandatory)
			{
				MandatoryValidation.CheckEntered(parent.CompanySizeInfo);
			}
		}

		public void ValidateTypeOfBusiness()
		{
			parent.TypeOfBusinessOtherInfo.ClearAllNotifications();
			if (parent.TypeOfBusiness_IsMandatory)
			{
				bool isNoCheckedItem = true;
				foreach (var item in parent.TypeOfBusinessSelections)
				{
					if (item.BoolValue)
					{
						isNoCheckedItem = false;
						break;
					}
				}

				if (isNoCheckedItem && parent.TypeOfBusinessOther.Trim().IsEmpty)
				{
					parent.TypeOfBusinessOtherInfo.AddError("Please select at least one business type or specify if other.");
				}
			}
		}

		public void ValidateReasonForRequestingAccess()
		{
			parent.ReasonForRequestingAccessOverwriteInfo.ClearAllNotifications();
			if (parent.ReasonForRequestingAccess.IsEmpty)
			{
				parent.ReasonForRequestingAccessOverwriteInfo.AddError("Please select one reason for requesting access.");
			}
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateWorkPhoneNationalCode();
			ValidateJobTitle();
			ValidateJobRole();
			ValidateCompanySize();
			ValidateTypeOfBusiness();
			ValidateReasonForRequestingAccess();
		}
	}
}
