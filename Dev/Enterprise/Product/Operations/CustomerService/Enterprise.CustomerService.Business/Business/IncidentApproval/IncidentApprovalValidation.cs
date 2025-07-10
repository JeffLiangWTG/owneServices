using CargoWise.EntityFramework;
using Enterprise.Registry.Business;

namespace Enterprise.CustomerService.Business
{
	public class IncidentApprovalValidation : AutoIncidentApprovalValidation
	{
		public IncidentApprovalValidation(AutoIncidentApproval parent)
			: base(parent)
		{
		}

		new IncidentApproval Parent
		{
			get { return (IncidentApproval)base.Parent; }
		}

		#region IA_Criticality

		protected override void CheckIA_Criticality()
		{
			base.CheckIA_Criticality();
			MandatoryValidation.CheckEntered(Parent.IA_CriticalityInfo);
			ListValidation.ErrorIfInvalidCode(Parent.IA_CriticalityInfo, Parent.Lookups.CriticalityList);
		}

		#endregion

		#region IA_GS_NKReportingStaff

		protected override void CheckIA_GS_NKReportingStaff()
		{
			base.CheckIA_GS_NKReportingStaff();
			if (SystemDataRegistry.Instance.CustomerServiceRequestNotificationRecipientsSetting.Value == IncidentApprovalLookups.EmailRecipientSettings.ThirdPartyNotifyOnly)
			{
				MandatoryValidation.CheckEntered(Parent.IA_GS_NKReportingStaffInfo);
			}
			ListValidation.ErrorIfInvalidCode(Parent.IA_GS_NKReportingStaffInfo, Parent.Lookups.ReportingStaff);
		}

		#endregion

		#region IA_Module

		protected override void CheckIA_Module()
		{
			base.CheckIA_Module();
			if (!Parent.IA_ModuleInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.IA_ModuleInfo);

				if (Parent.ModuleType == ModuleListType.MenuSection)
				{
					ListValidation.ErrorIfInvalidCode(Parent.MenuSectionInfo);
				}
				else if (Parent.ModuleType == ModuleListType.Cr8)
				{
					ListValidation.ErrorIfInvalidCode(Parent.Cr8ModuleInfo);
				}
				else if (Parent.ModuleType == ModuleListType.Cr9)
				{
					ListValidation.ErrorIfInvalidCode(Parent.Cr9ModuleInfo);
				}
			}
		}

		#endregion

		#region IA_IncidentSummary

		protected override void CheckIA_IncidentSummary()
		{
			base.CheckIA_IncidentSummary();
			MandatoryValidation.CheckEntered(Parent.IA_IncidentSummaryInfo);
		}

		#endregion

		#region IA_IncidentDetails

		protected override void CheckIA_IncidentDetails()
		{
			base.CheckIA_IncidentDetails();
			MandatoryValidation.CheckEntered(Parent.IA_IncidentDetailsInfo);
			if (IncidentDetailsValidator.HasWesternCharactersAndIsTooShort(Parent.IA_IncidentDetails))
			{
				Parent.IA_IncidentDetailsInfo.AddError(Res.GetString("02bfd932-dd38-426f-8547-6ea93cae8995", "Incident details too short - please provide more detailed information (at least 5 words)."));
			}
			else if (IncidentDetailsValidator.HasNonWesternCharactersAndIsTooShort(Parent.IA_IncidentDetails))
			{
				Parent.IA_IncidentDetailsInfo.AddError(Res.GetString("73F4D6B8-E036-43BA-9731-7F4066B97E0A", "Incident details too short - please provide more detailed information (at least 5 characters)."));
			}
		}

		#endregion

		#region IA_ClientSpecifiedStatus

		protected override void CheckIA_ClientSpecifiedStatus()
		{
			base.CheckIA_ClientSpecifiedStatus();
			ListValidation.ErrorIfInvalidCode(Parent.IA_ClientSpecifiedStatusInfo, Parent.Lookups.CustomerStatusList);
		}

		#endregion

		#region IA_LicenceCode

		protected override void CheckIA_LicenceCode()
		{
			base.CheckIA_LicenceCode();
			if (!Parent.IA_LicenceCode_ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.IA_LicenceCodeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.IA_LicenceCodeInfo, Parent.Lookups.LicenceCompanyList);
			}
		}

		#endregion
	}
}
