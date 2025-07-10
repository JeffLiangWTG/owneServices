using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Res = MailManager.Module.Res;
using ResString = MailManager.Module.ResString;

namespace Enterprise.MailManager.Module
{
	public class MailItemModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.MailItem; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.MailItem);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new MailItemFilterControl(GridCollection, (MailItemFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new StandardMailItemCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new MailItemFilterBusinessObject();
		}

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AlwaysAllow; }
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.Emails; }
		}

		#endregion

		public override bool AllowNew
		{
			get { return GlbStaff.CurrentUser.GS_IsDeveloper || GlbStaff.CurrentUser.IsSupportUser; }
		}

		public override bool AllowEdit
		{
			get { return false; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		public override bool AllowUniversalCopy
		{
			get { return false; }
		}

		#region Extra Menu Items

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());

			AddResetStatusMenuItem(result);

			SendCopyToMenu = new ZMenuItem(MenuItemSendCopyTo, new EventHandler(SendCopyToEvent));
			result.Add(SendCopyToMenu);

			return result.ToArray();
		}

		protected virtual void AddResetStatusMenuItem(List<MenuItem> result)
		{
			result.Add(new ZMenuItem(MenuItemResetStatusToQueued, new EventHandler(ResetStatusToQUEEvent)));
			result.Add(new ZMenuItem(MenuItemResetStatusToFailed, new EventHandler(ResetStatusToFALEvent)));
		}

		protected static MultilingualString MenuItemResetStatusToQueued
		{
			get { return ResString.GetMultilingualString("f72d74fa-0e99-433c-83f9-3d6c10945a5d", "&Reset Status To QUE"); }
		}

		protected static MultilingualString MenuItemResetStatusToFailed
		{
			get { return ResString.GetMultilingualString("1F4EAAE1-EE74-474A-8650-81613B8E74B1", "&Reset Status To FAL"); }
		}

		protected static MultilingualString MenuItemSendCopyTo
		{
			get { return ResString.GetMultilingualString("f52febe1-f49c-42fd-9943-6847bab2e61b", "&Send Copy To"); }
		}
		protected MenuItem SendCopyToMenu;

		void ResetStatusToQUEEvent(object sender, EventArgs e)
		{
			if (ResetStatusTo(Grid.SelectedElements, MailStatus.Queued) > 0)
			{
				PerformSearch();
			}
		}

		void ResetStatusToFALEvent(object sender, EventArgs e)
		{
			if (ResetStatusTo(Grid.SelectedElements, MailStatus.Failed) > 0)
			{
				PerformSearch();
			}
		}

		void SendCopyToEvent(object sender, EventArgs e)
		{
			if (SendCopyTo(Grid.SelectedElements) > 0)
			{
				PerformSearch();
			}
		}

		#endregion

		#region Implementation

		public int ResetStatusTo(BusinessObject[] selectedElements, string newStatus)
		{
			try
			{
				if (selectedElements.Length > 0)
				{
					MailStatusAssigner assigner = GetMailStatusAssigner(selectedElements, newStatus);
					assigner.Assign();

					if (!assigner.HasDBChanged)
					{
						if (assigner.Errors.Length == 0)
						{
							if (assigner.ItemsAffected > 0)
							{
								ShowInformation(Res.GetString("d5c0ebd8-49ce-401d-bf88-96fd5f6fbdd5", "Reset Status to {0} for {1} Email(s) from {2} selected.", newStatus, assigner.ItemsAffected, selectedElements.Length));
								return assigner.ItemsAffected;
							}
							else
							{
								ShowInformation(Res.GetString("b79ec236-8e11-4c5f-a428-fdbdb9f1c5e8", "No Emails changed Status to {0} from {1} selected.", newStatus, selectedElements.Length));
							}
						}
						else
						{
							ShowError(Res.GetString("2e525ff0-b080-4d69-9c69-acee64ade391", "Errors were encountered trying to reset the Status of Mail Item(s).\r\n{0}", assigner.Errors));
						}
					}
					else
					{
						ShowError(Res.GetString("13eab110-356d-43f2-b102-24d974d14d27", "One or more of the selected Emails has been changed. Please refresh the grid and try again."));
					}
				}
				else
				{
					ShowError(Res.GetString("e1500b82-fa37-49b1-8e7c-d91c0602d339", "Please select one or more Emails to reset the Status."));
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("Exception Resetting Email Status", ex);
			}

			return 0;
		}

		public virtual MailStatusAssigner GetMailStatusAssigner(BusinessObject[] selectedElements, string newStatus)
		{
			return new MailStatusAssigner(selectedElements, newStatus);
		}

		public int SendCopyTo(BusinessObject[] selectedElements)
		{
			try
			{
				if (selectedElements.Length > 0)
				{
					MailItemCopySender copySender = GetMailItemCopySender(Grid.SelectedElements);

					if (!AssignMailAddressToSendCopy(copySender))
					{
						return 0;
					}

					copySender.SendCopyTo();

					if (copySender.Errors.Length == 0)
					{
						ShowInformation(Res.GetString("3288ec38-d6b7-4fab-84ce-c4c10aba34fe", "Copies of {0} Email(s) were successfully sent to the {1} address.", copySender.MessagesToSend, copySender.MailAddressToSendCopyTo));
					}
					else
					{
						if (copySender.HasErrors)
						{
							ShowError(copySender.Errors);
						}
						else
						{
							ShowError(Res.GetString("c85e6eef-d7df-4cd8-a5f2-e60d30a0f100", "Errors were encountered trying to send copy of Email(s) to the {0} address.\r\n{1}", copySender.MailAddressToSendCopyTo, copySender.Errors));
						}
					}
				}
				else
				{
					ShowError(Res.GetString("6d80e4cf-28e4-49a5-a148-37dc5517d34c", "Please select one or more Emails to send a copy."));
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("Exception sending copy of Email(s) ", ex);
			}

			return 0;
		}

		public virtual bool AssignMailAddressToSendCopy(MailItemCopySender copySender)
		{
			return (ZFormModaliser.ShowDialogAndDispose(new MailItemCopySenderForm(copySender)) == DialogResult.OK);
		}

		public virtual MailItemCopySender GetMailItemCopySender(BusinessObject[] selectedElements)
		{
			return new MailItemCopySender(Array.ConvertAll(selectedElements, el => (MailItem)el));
		}

		void ShowInformation(string message)
		{
			Globals.Message.ShowInformation(message);
		}

		void ShowError(string message)
		{
			Globals.Message.ShowError(message);
		}

		#endregion

	}
}
