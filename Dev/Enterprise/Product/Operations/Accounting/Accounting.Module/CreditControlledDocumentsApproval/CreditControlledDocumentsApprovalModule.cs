using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.GUI;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class CreditControlledDocumentsApprovalModule : ZFilterGridModule
	{
		public CreditControlledDocumentsApprovalModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.CreditControlledDocumentsApproval; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.CreditControlledDocumentsApproval);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CreditControlledDocumentsApprovalFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CreditControlledDocumentsApprovalCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CreditControlledDocumentsApprovalFilterBusinessObject();
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowEdit
		{
			get { return false; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CreditControlledDocumentsApproval; }
		}

		#region Action Menu Items

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var menuItems = new List<MenuItem>(base.GetNewActionMenuItems());
			var approveMenuItem = new ZMenuItem(ResString.GetMultilingualString("b85a492f-97fc-4d05-a891-6d904bba96c8", "&Approve"), new EventHandler(HandleApprove));
			menuItems.Add(approveMenuItem);
			var rejectMenuItem = new ZMenuItem(ResString.GetMultilingualString("0f1d78a2-f12b-4910-89bf-96dc756178ad", "&Reject"), new EventHandler(HandleReject));
			menuItems.Add(rejectMenuItem);
			var cancelMenuItem = new ZMenuItem(ResString.GetMultilingualString("124c1d8e-fb6c-479e-bdeb-88eae27fcf6d", "&Cancel"), new EventHandler(HandleCancel));
			menuItems.Add(cancelMenuItem);
			return menuItems.ToArray();
		}

#if DEBUG
		public MenuItem[] GetNewActionMenuItems_Test()
		{
			return GetNewActionMenuItems();
		}
#endif

		void HandleApprove(object sender, EventArgs e)
		{
			HandleAction(CreditControlledDocumentsApprovalFormModes.Approve,
				Res.GetData("38c8d9ce-70b6-4f2f-90c8-a5a9f33a7895",
@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to: 
Manage -> Receivables -> Receivables Transactions -> On Credit Hold Controller"),
				Res.GetData("25fd1a82-cb4f-49e4-82e8-e9973e61a97e", "Can't Approve - These approvals are NOT in REQ status: {0}"),
				r => r.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Requested);
		}

		void HandleReject(object sender, EventArgs e)
		{
			HandleAction(CreditControlledDocumentsApprovalFormModes.Reject,
				Res.GetData("6aa92be9-5ebc-4598-a514-71fa9ca514e8",
@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to: 
Manage -> Receivables -> Receivables Transactions -> On Credit Hold Controller"),
				Res.GetData("9762ffdf-58f8-4694-970f-748881873664", "Can't Reject - These approvals are NOT in REQ status: {0}"),
				r => r.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Requested);
		}

		void HandleCancel(object sender, EventArgs e)
		{
			HandleAction(CreditControlledDocumentsApprovalFormModes.Cancel,
				Res.GetData("8aafca9b-d78f-40e0-aac6-4376b190d760",
@"You can't cancel selected requests. 
A request can be canceled only by the user who either created it or has rights to approve it. Neither of those conditions were satisfied."),
				Res.GetData("119e7e6b-c140-4ac3-803f-af304ecdd8cd", "Can't Cancel - These approvals are already posted or canceled: {0}"),
				r => r.XP_ApprovalStatus != Constants.GenApprovalRequestApprovalStatus.Cancelled &&
					r.XP_ApprovalStatus != Constants.GenApprovalRequestApprovalStatus.Posted);
		}

		void HandleAction(
			CreditControlledDocumentsApprovalFormModes formMode,
			ResourceStringData noRightsMessage,
			ResourceStringData notAcceptableMessage,
			Func<CreditControlledDocumentsApproval, bool> isAcceptableFunction)
		{
			string targetStatus;

			switch (formMode)
			{
				case CreditControlledDocumentsApprovalFormModes.Approve:
					targetStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
					break;
				case CreditControlledDocumentsApprovalFormModes.Reject:
					targetStatus = Constants.GenApprovalRequestApprovalStatus.Rejected;
					break;
				case CreditControlledDocumentsApprovalFormModes.Cancel:
					targetStatus = Constants.GenApprovalRequestApprovalStatus.Cancelled;
					break;
				default:
					throw new InvalidEnumArgumentException("Mode not recognised.");
			}

			var approvals = this.Grid.SelectedElements;
			if (approvals.Length == 0)
			{
				ShowNoSelectedMessage();
			}
			else
			{
				var jobsForUnacceptableApprobals = from CreditControlledDocumentsApproval approval in approvals
												   where !isAcceptableFunction(approval)
												   select approval.JobNumber;

				if (jobsForUnacceptableApprobals.Any())
				{
					Globals.Message.ShowError(
						String.Format(CultureInfo.CurrentCulture,
							string.IsNullOrEmpty(notAcceptableMessage.Caption)
								? notAcceptableMessage.FullDescription
								: notAcceptableMessage.Caption, ZString.Join(",", jobsForUnacceptableApprobals.ToArray())));
				}
				else
				{
					var formBizo = new CreditControlledDocumentsApprovalBulk(new BusinessObjectFactory(), Grid.SelectedElements.Select(element => (CreditControlledDocumentsApproval)element).ToArray());
					if (formBizo.ChangeStatus(targetStatus))
					{
						using (var form = new CreditControlledDocumentsApprovalForm(formBizo, formMode))
						{
							ZFormModaliser.ShowDialogAndDispose(form);
						}
					}
					else
					{
						Globals.Message.ShowError(string.IsNullOrEmpty(noRightsMessage.Caption)
							? noRightsMessage.FullDescription
							: noRightsMessage.Caption);
					}
				}
			}
		}

		#endregion
	}
}
