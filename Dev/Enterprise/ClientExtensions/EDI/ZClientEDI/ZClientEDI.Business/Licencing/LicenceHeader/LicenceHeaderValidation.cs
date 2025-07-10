using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Core;
namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class LicenceHeaderValidation : AutoLicenceHeaderValidation
	{
		public LicenceHeaderValidation(AutoLicenceHeader parent)
			: base(parent)
		{
		}

		public new LicenceHeader Parent
		{
			get { return (LicenceHeader)base.Parent; }
		}

		#region LA_SupportStartDate

		protected override void CheckLA_SupportStartDate()
		{
			base.CheckLA_SupportStartDate();

			LicenceModules licenceCore = Parent.Modules.FindByCode(LegacyLicence.Codes.Core);
			if (licenceCore != null && licenceCore.LM_LicenceType == LicenceTypes.Codes.PUR)
			{
				MandatoryValidation.CheckEntered(Parent.LA_SupportStartDateInfo);
			}
		}

		#endregion

		#region LA_SupportMode

		protected override void CheckLA_SupportMode()
		{
			MandatoryValidation.CheckEntered(Parent.LA_SupportModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.LA_SupportModeInfo, Parent.Lookups.SupportModeList);
		}

		#endregion

		#region  LA_LicenceAdvStdOth

		protected override void CheckLA_LicenceAdvStdOth()
		{
			base.CheckLA_LicenceAdvStdOth();
			var info = Parent.LA_LicenceAdvStdOthInfo;
			MandatoryValidation.CheckEntered(info);
			ListValidation.ErrorIfInvalidCode(info, Parent.Lookups.LicenceAdvStdOth);

			if (info.HasChanges && !info.HasErrors())
			{
				if (Parent.IsInDatabase && !Parent.Lookups.ActiveEditions.ContainsCode(Parent.LA_LicenceAdvStdOth))
				{
					info.AddError("Inactive Edition");
				}
			}

			foreach (LicenceModules module in Parent.Modules)
			{
				module.Validation.ValidateLM_LicenceType();
			}
		}

		#endregion

		#region LA_AMS_USMode

		protected override void CheckLA_AMS_USMode()
		{
			ListValidation.ErrorIfInvalidCode(Parent.LA_AMS_USModeInfo, Parent.Lookups.AMSModeList);
		}

		#endregion

		#region LA_RX_NKPriceCurrency

		protected override void CheckLA_RX_NKPriceCurrency()
		{
			base.CheckLA_RX_NKPriceCurrency();
			if (Parent.Database.LD_IsBilledPerCompany)
			{
				MandatoryValidation.CheckEntered(Parent.LA_RX_NKPriceCurrencyInfo);
				ListValidation.ErrorIfInvalidCode(Parent.LA_RX_NKPriceCurrencyInfo);
			}
		}

		#endregion

		#region Installation / Live / Contract Dates

		protected override void CheckLA_InstallationCompleteDateIsValidZDateTimeRange()
		{
		}

		protected override void CheckLA_ContractExpiryDateIsValidZDateTimeRange()
		{
		}

		protected override void CheckLA_InstallationStartDateIsValidZDateTimeRange()
		{
		}

		protected override void CheckLA_AgreedLiveDateIsValidZDateTimeRange()
		{
		}

		protected override void CheckLA_SiteLiveDateIsValidZDateTimeRange()
		{
		}

		protected override void CheckLA_SupportStartDateIsValidZDateTimeRange()
		{
		}

		protected override void CheckLA_LastFaxReportIsValidZDateTimeRange()
		{
		}

		#endregion

		#region LA_AgreedLiveDate

		protected override void CheckLA_AgreedLiveDate()
		{
			var parent = Parent;
			if (parent.LA_AgreedLiveDate.IsEmpty)
			{
				var db = parent.Database;
				if (db.LD_IsActive &&
					db.LD_LicenceType == DatabaseTypes.Codes.Production &&
					db.IsEnterpriseFamilyDatabase &&
					db.PriceHeaderLinks.Count != 0 &&
					db.UsageOwnerOrFirstLicence == parent)
				{
					parent.LA_AgreedLiveDateInfo.AddError("Please enter a value - it is mandatory for the owner of an active, production database with an STL price list");
				}
			}
			else
			{
				if (parent.LA_AgreedLiveDate.IsValid && parent.LA_AgreedLiveDate.Day != 1 && (!parent.IsInDatabase || parent.LA_AgreedLiveDateInfo.HasChanges))
				{
					parent.LA_AgreedLiveDateInfo.AddError("Date must be the first day of the month");
				}
			}
		}

		#endregion
	}
}

