using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSDISQueryMessageValidation : EDIMessageValidation
	{
		public CDSDISQueryMessageValidation(AutoEDIMessage parent) : base(parent) { }

		CDSDISQueryMessage QueryMessage => Parent as CDSDISQueryMessage;

		public override void ValidateAll()
		{
			base.ValidateAll();

			if (!QueryMessage.ReadOnly)
			{
				ValidateDeclarationCategory();
				ValidateDateFrom();
				ValidateDateTo();
				ValidateDeclarationStatus();
				ValidatePageNumber();
			}
		}

		protected override void CheckEM_ApplicationReference()
		{
			base.CheckEM_ApplicationReference();
			MandatoryValidation.CheckEntered(QueryMessage.EM_ApplicationReferenceInfo);
		}

		protected override void CheckEM_MessageOwner()
		{
			if (!QueryMessage.ReadOnly)
			{
				base.CheckEM_MessageOwner();
				MandatoryValidation.CheckEntered(QueryMessage.EM_MessageOwnerInfo);
				ListValidation.ErrorIfInvalidCode(QueryMessage.EM_MessageOwnerInfo);
				var glbExternalPassword = QueryMessage.GlbExternalPassword;

				if (glbExternalPassword != null && glbExternalPassword.GP_PasswordStatus != PasswordStatusList.Codes.Valid)
				{
					var text = !glbExternalPassword.GP_StatusReason.IsEmpty ? string.Format("Please select a valid profile (token). The profile's status is {0}, and the status message is '{1}'", glbExternalPassword.GP_PasswordStatus, glbExternalPassword.GP_StatusReason)
						: string.Format("Please select a valid profile (token). The profile's status is {0}", glbExternalPassword.GP_PasswordStatus);
					QueryMessage.EM_MessageOwnerInfo.AddMessageError(text);
				}
			}
		}

		protected override void CheckEM_MessageTextIsWesternEuropean()
		{
		}

		public void ValidateDeclarationCategory()
		{
			if (!QueryMessage.ReadOnly)
			{
				ValidateCalculatedProperty(QueryMessage.DeclarationCategoryInfo);
			}
		}

		protected virtual void CheckDeclarationCategory()
		{
			MandatoryValidation.CheckEntered(QueryMessage.DeclarationCategoryInfo);
			ListValidation.ErrorIfInvalidCode(QueryMessage.DeclarationCategoryInfo, QueryMessage.Lookups.DeclarationCategories);
		}

		public void ValidateDateFrom()
		{
			if (!QueryMessage.ReadOnly)
			{
				ValidateCalculatedProperty(QueryMessage.DateFromInfo);
			}
		}

		protected virtual void CheckDateFrom()
		{
			var dateInfo = QueryMessage.DateFromInfo;
			MandatoryValidation.CheckEntered(dateInfo);

			if (!dateInfo.Value.IsEmpty && !dateInfo.Value.IsValid)
			{
				dateInfo.AddError(Res.GetString("2C17F806-B1E3-444F-906D-DC374C3FDFF2", "Enter a valid Date From."));
			}
			else if (!dateInfo.Value.IsEmpty && dateInfo.Value.IsValid && QueryMessage.DateFrom > QueryMessage.DateTo)
			{
				dateInfo.AddError(Res.GetString("97544A06-1733-44C9-B7F6-C7C579D4B144", "Date From cannot be greater then Date To."));
			}
		}

		public void ValidateDateTo()
		{
			if (!QueryMessage.ReadOnly)
			{
				ValidateCalculatedProperty(QueryMessage.DateToInfo);
			}
		}

		protected virtual void CheckDateTo()
		{
			var dateInfo = QueryMessage.DateToInfo;
			MandatoryValidation.CheckEntered(dateInfo);

			if (!dateInfo.Value.IsEmpty && !dateInfo.Value.IsValid)
			{
				dateInfo.AddError(Res.GetString("42AA3A48-2F20-4234-9A65-F14B530D35BF", "Enter a valid Date To."));
			}
			else if (!dateInfo.Value.IsEmpty && dateInfo.Value.IsValid && QueryMessage.DateTo < QueryMessage.DateFrom)
			{
				dateInfo.AddError(Res.GetString("90D58A19-3DDD-42AE-9257-E59D586648F9", "Date To cannot be lower then Date From."));
			}
		}

		public void ValidateDeclarationStatus()
		{
			if (!QueryMessage.ReadOnly)
			{
				ValidateCalculatedProperty(QueryMessage.DeclarationStatusInfo);
			}
		}

		protected virtual void CheckDeclarationStatus()
		{
			MandatoryValidation.CheckEntered(QueryMessage.DeclarationStatusInfo);
			ListValidation.ErrorIfInvalidCode(QueryMessage.DeclarationStatusInfo, QueryMessage.Lookups.DeclarationStatuses);
		}

		public void ValidatePageNumber()
		{
			if (!QueryMessage.ReadOnly)
			{
				ValidateCalculatedProperty(QueryMessage.PageNumberInfo);
			}
		}

		protected virtual void CheckPageNumber()
		{
			MandatoryValidation.CheckEntered(QueryMessage.PageNumberInfo);
		}
	}
}
