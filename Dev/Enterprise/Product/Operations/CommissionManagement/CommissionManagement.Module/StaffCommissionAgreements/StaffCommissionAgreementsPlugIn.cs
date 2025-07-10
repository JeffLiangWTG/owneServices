using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.CommissionManagement.Business;
using Enterprise.CommissionManagement.GUI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Module
{
	public class StaffCommissionAgreementsPlugIn : ZPlugIn
	{
		public StaffCommissionAgreementsPlugIn(GlbStaff hostBusinessEntity)
			: base(hostBusinessEntity)
		{
		}

		public override string Name
		{
			get { return Res.GetString("a8890a99-87ad-47d7-86f3-95aa71402384", "Staff Commission Agreements"); }
		}

		protected override ZBool HasUserControl
		{
			get { return false; }
		}

		new GlbStaff HostBusinessEntity
		{
			get { return (GlbStaff)base.HostBusinessEntity; }
		}

		#region Form Events

		protected override void HookFormEventsCore()
		{
			base.HookFormEventsCore();

			IFileMenuItemsProvider menuItemsProvider = Form;
			menuItemsProvider.ActionsMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("c4e6d9da-c4ee-4267-b011-5d0783cf9b9c", "Disable Staff Commission Agreements"), DisableStaffCommissionAgreementsMenuItemClick));
			HostBusinessEntity.GS_IsActiveInfo.ValueChanged += GS_IsActiveInfo_ValueChanged;
		}

		protected override void UnHookFormEventsCore()
		{
			base.UnHookFormEventsCore();
			HostBusinessEntity.GS_IsActiveInfo.ValueChanged -= GS_IsActiveInfo_ValueChanged;
		}

		#endregion

		#region DisableStaffCommissionAgreements

		void DisableStaffCommissionAgreementsMenuItemClick(object sender, EventArgs e)
		{
			if (!CheckHasAgreementRecipient())
			{
				Globals.Message.ShowInformation(Res.GetString("fa10aa3f-e003-4171-815f-148e2399da3a", "Staff does not have any commission agreements."), Res.GetString("d34b8a82-c3a5-4201-a247-3589189b4b1b", "Cannot disable commission agreements"));
				return;
			}

			var action = DisableStaffCommissionAgreementsAction.New(HostBusinessEntity);
			var message = Res.GetData("b682c8b8-8f7e-48ab-b8ab-9df47a82d101", @"Please select a date to disable staff commission agreement rates from.
Caution: It won't be possible to revert this change.");
			var yesCaption = Res.GetData("24726346-2080-4ac2-8061-84dc5299183c", "Confirm");
			var yesAndApproveCaption = Res.GetData("b5081d4f-275b-4b9a-9bca-6f832204413e", "Confirm && Approve");
			var noCaption = Res.GetData("56ce1b6a-a205-477d-be23-c900e4ded85c", "Cancel");

			var strings = new DisableStaffCommissionAgreementsFormStrings(message, yesCaption, yesAndApproveCaption, noCaption);
			var disableForm = new DisableStaffCommissionAgreementsForm(action, strings);

			ZFormModaliser.Show(disableForm, Form);
		}

		void GS_IsActiveInfo_ValueChanged(object sender, EventArgs e)
		{
			if (HostBusinessEntity.IsInDatabase && !HostBusinessEntity.GS_IsActive)
			{
				if (CheckHasAgreementRecipient())
				{
					var action = DisableStaffCommissionAgreementsAction.New(HostBusinessEntity);
					var message = Res.GetData("a87a71ec-01a0-488e-ac91-aba5a3c91e78", @"Commission Agreements exist for the staff. Do you wish to disable these rates?
Caution: It won't be possible to revert this change.");
					var yesCaption = Res.GetData("41221380-7f8d-40b6-8b4a-44ff12252d59", "Yes");
					var yesAndApproveCaption = Res.GetData("fca4ec06-277c-4d5b-9049-3bb63f2b33b0", "Yes && Approve");
					var noCaption = Res.GetData("f190e42a-06d6-4423-bae4-78c5983a0bbc", "No");

					var strings = new DisableStaffCommissionAgreementsFormStrings(message, yesCaption, yesAndApproveCaption, noCaption);
					var disableForm = new DisableStaffCommissionAgreementsForm(action, strings);

					ZFormModaliser.Show(disableForm, Form);
				}
			}
		}

		bool CheckHasAgreementRecipient()
		{
			var recipientsQuery = new ZQuery(OrgCommissionAgreementRecipientSchema.CAR_GS_NKStaff, HostBusinessEntity.GS_Code);
			return HostBusinessEntity.Factory.LoadTop1<OrgCommissionAgreementRecipient>(recipientsQuery) != null;
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Core; }
		}

		#endregion
	}
}
