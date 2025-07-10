using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ActiveDirectory;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Security.ActiveDirectory.GUI
{
	public class ADActionsMenuItem : ZMenuItem
	{
		public ADActionsMenuItem(IADLinkedEntity parent)
			: base(ResString.GetMultilingualString("5dc436a3-36ba-4c37-9548-fc4ed218e0a4", "Active Directory"))
		{
			this.parent = parent;
			this.MenuItems.Add("-");
			this.Popup += ADActionsMenuItem_Popup;
		}

		readonly IADLinkedEntity parent;

		#region Build

		void ADActionsMenuItem_Popup(object sender, EventArgs e)
		{
			MenuItems.Clear();
			AddMenuItems();
		}

		bool ShouldAddMenuItems()
		{
			if (!ActiveDirectoryRegistry.Instance.IsIntegrationEnabled)
			{
				if (ShouldShowReasonWhenNotAddingMenuItems)
				{
					MenuItems.Add(Res.GetString("8752408f-158a-4ebe-8c6c-93154538f993", "Active Directory Integration is not enabled."));
				}
			}
			else if (IsGroup && ActiveDirectoryRegistry.Instance.EntitiesToSync == EntitiesToSync.UsersOnly)
			{
				if (ShouldShowReasonWhenNotAddingMenuItems)
				{
					MenuItems.Add(Res.GetString("bd13e754-5d03-4655-9c90-602aff49fba2", "Groups are not synchronized with Active Directory"));
				}
			}
			else
			{
				return true;
			}

			return false;
		}

		protected virtual bool ShouldShowReasonWhenNotAddingMenuItems => true;

		protected virtual void AddMenuItems()
		{
			if (ShouldAddMenuItems())
			{
				if (CanShowSynchroniseOption())
				{
					MenuItems.Add(ResString.GetMultilingualString("e8ddebb6-0557-4b3a-9061-73aa313fd816", "Synchronize"), Synchronise_Click);
					if (CanShowOptionsForLinkedEntities())
					{
						MenuItems.Add(ResString.GetMultilingualString("98bb1ad2-7b7b-4316-aecc-6d98e31da0e1", "Disconnect From Active Directory"), DisconnectFromAD_Click);
						if (CanShowStaffOptions())
						{
							MenuItems.Add(ResString.GetMultilingualString("ab7a501c-1573-42ea-92ff-be734908d462", "Unlock Account"), UnlockADAccount_Click);
						}
					}
				}
				else
				{
					MenuItems.Add(IsStaff ?
						ResString.GetMultilingualString("fea63603-1255-4f93-88c8-18ea10c3d00f", "Only active or linked staff can be synchronized with AD.") :
						ResString.GetMultilingualString("433dcc9a-480d-4468-918f-780375410b3e", "Only non-system, active or linked groups can be synchronized with AD."));
				}
			}
		}

		protected virtual bool IsGroup
		{
			get { return parent is GlbGroup; }
		}

		protected virtual bool IsStaff
		{
			get { return parent is GlbStaff; }
		}

		protected bool CanShowSynchroniseOptionOnEntity(IADLinkedEntity entity)
		{
			return (entity.IsADLinked || entity.IsActive) && (entity.IsADLinkable);
		}

		protected virtual bool CanShowSynchroniseOption()
		{
			return CanShowSynchroniseOptionOnEntity(parent);
		}

		protected virtual bool CanShowOptionsForLinkedEntities()
		{
			return parent.IsADLinked;
		}

		protected virtual bool CanShowStaffOptions()
		{
			return parent is GlbStaff;
		}

		#endregion

		#region Click handlers

		void Synchronise_Click(object sender, EventArgs e)
		{
			Synchronise();
		}

#if DEBUG
		public
#endif
 void Synchronise()
		{
			var parentBizo = parent as BusinessObject;
			if (parentBizo == null || parentBizo.IsInDatabase && !parentBizo.HasChanges)
			{
				ExecuteHandlingDirectoryServicesExceptions(SynchroniseCore);
			}
			else
			{
				Globals.Message.Show(Res.GetString("87bc9784-39d6-47d9-85d6-07d75a989a08", "Please save this record first."));
			}
		}

		protected bool ConfirmLinkingStaffThatCannotLogin(IEnumerable<IBusiness> selectedObjects)
		{
			if (IsStaff && selectedObjects.Any(a => (a is GlbStaff s) && (!s.GS_CanLogin) && (!s.IsADLinked)))
			{
				var question = Res.GetString("BEAE2F33-E5B5-493C-9D10-F1F4F23278C0", "You are about to link and synchronize a Staff record without the 'Can Login' attribute to Active Directory.");
				var caption = Res.GetString("C9FA11E4-809C-4E24-AB1B-1E2E81BC887F", "Synchronizing staff without the 'Can Login' attribute");
				string confirmationString = Res.GetString("0CE11997-B3CE-408D-8564-C0067D68FBA7", "Yes");

				if (Globals.Message.ShowConfirmation(question, caption, confirmationString, MessageBoxIcon.Exclamation) != DialogResult.OK)
				{
					return false;
				}
				else
				{
					return ValidateLoginNamePrefixForUnlinkedCannotLoginUser();
				}
			}
			return true;

			bool ValidateLoginNamePrefixForUnlinkedCannotLoginUser()
			{
				var prefix = ActiveDirectoryRegistry.Instance.UserLoginPrefix.Value;
				if (!string.IsNullOrEmpty(prefix))
				{
					var loginsWithoutPrefix = selectedObjects.Cast<GlbStaff>().Where(s => !s.GS_LoginName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && (!s.GS_CanLogin) && (!s.IsADLinked)).Select(s => s.GS_LoginName.ToString()).ToList();
					if (loginsWithoutPrefix.Count > 0)
					{
						Globals.Message.ShowError(Res.GetString("74A456FA-E0E2-4C75-BF2D-E947F60FB2AC", "Login Name '{0}' must begin with '{1}'", string.Join(", ", loginsWithoutPrefix), prefix));
						return false;
					}
				}
				return true;
			}
		}

		protected virtual void SynchroniseCore()
		{
			if (!ConfirmLinkingStaffThatCannotLogin(new[] { parent }))
			{
				return;
			}

			try
			{
				parent.SynchroniseWithAD();
			}
			catch (Exception ex) when (ex is ActiveDirectoryUserException || ex is SecurityAccessDeniedException || ex is InvalidOperationException)
			{
				Globals.Message.ShowError(ex.Message);
			}
			catch (InvalidOUException ex)
			{
				Globals.Message.ShowError(GetInValidOUExceptionMessage(ex.InvalidOUPath));
			}
		}

		protected string GetInValidOUExceptionMessage(string invalidOUPath)
		{
			return Res.GetString("9EA17E54-A69B-4C74-A1D8-6A32FD26668A",
				"The Organizational Unit {0} is invalid, please review the setting in Registry items: {1}",
				invalidOUPath,
				((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.DomainCredentialsCollection).LocationMultilingual
			);
		}

		void DisconnectFromAD_Click(object sender, EventArgs e)
		{
			ExecuteHandlingDirectoryServicesExceptions(DisconnectFromADCore);
		}

		protected virtual void DisconnectFromADCore()
		{
			parent.DisconnectFromAD();
		}

		void UnlockADAccount_Click(object sender, EventArgs e)
		{
			ExecuteHandlingDirectoryServicesExceptions(UnlockADAccountCore);
		}

		protected virtual void UnlockADAccountCore()
		{
			var staff = parent as GlbStaff;
			if (staff != null)
			{
				staff.UnlockADAccount();
			}
		}

		#endregion

		static void ExecuteHandlingDirectoryServicesExceptions(Action action)
		{
			try
			{
				action();
			}
			catch (DirectoryServicesException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
			catch (SecurityAccessDeniedException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
		}

#if DEBUG
		public void OnPopup()
		{
			base.OnPopup(EventArgs.Empty);
		}
#endif
	}
}
