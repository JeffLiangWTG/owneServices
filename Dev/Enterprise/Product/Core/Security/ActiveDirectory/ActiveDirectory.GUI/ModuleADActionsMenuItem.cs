using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ActiveDirectory;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Security.ActiveDirectory.GUI
{
	public class ModuleADActionsMenuItem : ADActionsMenuItem
	{
		public ModuleADActionsMenuItem(ZFilterGridModule parentModule, BusinessObjectFactory factory)
			: base(null)
		{
			this.parentModule = parentModule;
			this.factory = factory;
		}

		readonly ZFilterGridModule parentModule;
		readonly BusinessObjectFactory factory;

		#region Build

		protected override void AddMenuItems()
		{
			if (SelectedObjects.Any())
			{
				base.AddMenuItems();
				AddSetDomainNameInBulkMenuItem();
			}
			else
			{
				MenuItems.Add(Res.GetString("a85854a9-8e6d-4972-a0aa-d4e30042d885", "Please select a record"));
			}
		}

		protected override bool ShouldShowReasonWhenNotAddingMenuItems => false;

		void AddSetDomainNameInBulkMenuItem()
		{
			if (SelectedObjects.Any(e => e.IsADLinkable))
			{
				MenuItems.Add(ResString.GetMultilingualString("58E5A136-8D8C-4433-A171-075B678CD657", "Set Domain Name"), SetDomainName_Click);
			}
		}

		void SetDomainName_Click(object sender, EventArgs e)
		{
			SetDomainInBulk();
		}

		void SetDomainInBulk()
		{
			ISecurityCheckpoint checkpoint = IsStaff ? EnvProxy.Instance.Security.FindCheckPoint("StaffSetDomainInBulk") : EnvProxy.Instance.Security.FindCheckPoint("GroupSetDomainInBulk");
			if (checkpoint.IsAllowed)
			{
				var args = new UserResponseArgument();
				args.Caption = Res.GetString("270FEAA2-99A4-414A-B3CB-B8919D02EB5A", "Set Domain Name");
				args.Message = Res.GetString("C96B38F1-7CD3-4F8C-A504-0658E8E14978", "New Domain Name");
				args.Buttons = ZMessageBoxButtons.OKCancel;
				args.DefaultButton = ZMessageBoxDefaultButton.Button2;
				args.Icon = ZMessageBoxIcon.Question;
				args.UserResponseDropEditCharactersCasing = ZCharacterCasing.Normal;
				args.UserResponseDropEditOnlyShowCode = true;
				args.AnswerList = (CodeDescriptionPairList)ObjectFactory.Get<IADRegistry>().DomainCredentialsCollectionAsCodeDescriptionPairList;
				args.AnswerList.AddPair(ClearDomainNameText, ClearDomainNameText); //Clear domain name

				var response = Globals.Message.QueryUserResponse(args);
				if (!string.IsNullOrEmpty(response))
				{
					var message = GetSetDomainNameConfirmationMessage(response);
					var caption = Res.GetString("297a9ac0-f853-47fb-ba80-4a68da49f1d1", "Confirm Domain Name change");
					if (Globals.Message.ShowConfirmation(message, caption, (NoResString)"Change", MessageBoxIcon.Exclamation) == DialogResult.OK)
					{
						DoSomethingAndThenSaveApplicableFactories(SelectedObjects, (entry) => SetDomainName(entry, response));
					}
				}
			}
			else
			{
				throw new SecurityAccessDeniedException(EnvProxy.Instance.Security.GetErrorMessageForNotAllowed(checkpoint));
			}

			void SetDomainName(IADLinkedEntity bizo, string newDomainName)
			{
				if (bizo.IsADLinkable)
				{
					if (IsClearDomainName(newDomainName))
					{
						bizo.DomainName = string.Empty;
					}
					else
					{
						bizo.DomainName = newDomainName;
					}
				}
			}
		}

		string ClearDomainNameText => Res.GetString("708ea246-79c8-4d3c-9df9-2fdb604c642e", "(empty)");

		bool IsClearDomainName(string response) => response.Equals(ClearDomainNameText, StringComparison.CurrentCulture);

		string GetSetDomainNameConfirmationMessage(string response)
		{
			var messageChangeDomain = Res.GetString("58543375-bde5-4575-8aaf-ad7b531f8321", "Are you sure you want to change the Domain Name of the selected record(s) to: {0}?", response);
			var messageClearDomain = Res.GetString("41ceb138-3344-48da-abfc-e9215fe58e80", "Are you sure you want to clear the Domain Name of the selected record(s)?");

			return IsClearDomainName(response) ? messageClearDomain : messageChangeDomain;
		}

		protected override bool IsGroup
		{
			get { return parentModule.ID == ModuleIDs.GlbGroup; }
		}

		protected override bool IsStaff
		{
			get { return parentModule.ID == ModuleIDs.GlbStaff; }
		}

		protected override bool CanShowSynchroniseOption()
		{
			return SelectedObjects.Any(e => CanShowSynchroniseOptionOnEntity(e));
		}

		protected override bool CanShowOptionsForLinkedEntities()
		{
			return SelectedObjects.Any(e => e.IsADLinked);
		}

		protected override bool CanShowStaffOptions()
		{
			return SelectedObjects.Any(e => e is GlbStaff);
		}

		#endregion

		#region Click handlers

		protected override void SynchroniseCore()
		{
			var selectedObjects = parentModule.GetSelectedBusinessObjects().Where(e => e is IADLinkedEntity iADLinkedEntity && iADLinkedEntity.IsADLinkable);

			if (!ConfirmLinkingStaffThatCannotLogin(selectedObjects))
			{
				return;
			}

			var selectedObjectsInThisFactory = selectedObjects.Select(o => factory.Load(o.GetType(), o.PK)).ToArray();
			try
			{
				using (new ZWaitCursorChanger())
				{
					var syncDirector = GetSyncDirector(factory, selectedObjectsInThisFactory);
					syncDirector.Synchronise();
					syncDirector.Save();
				}
			}
			catch (Exception ex) when (ex is DirectoryServicesException || ex is ActiveDirectoryUserException || ex is SecurityAccessDeniedException || ex is ZCannotSaveException)
			{
				Globals.Message.ShowError(ex.Message);
			}
			catch (InvalidOUException ex)
			{
				Globals.Message.ShowError(GetInValidOUExceptionMessage(ex.InvalidOUPath));
			}
		}

		static ISynchronisationDirector GetSyncDirector(BusinessObjectFactory factory, BusinessObject[] selectedObjects)
		{
			return ObjectFactory.Get<ISynchronisationDirectorProvider>().GetSyncDirector(factory, selectedObjects);
		}

		protected override void DisconnectFromADCore()
		{
			try
			{
				DoSomethingAndThenSaveApplicableFactories(SelectedObjects.Where(e => e.IsADLinked), e => e.DisconnectFromAD());
			}
			catch (SecurityAccessDeniedException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
			catch (ZCannotSaveException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
		}

		protected override void UnlockADAccountCore()
		{
			DoSomethingAndThenSaveApplicableFactories(SelectedObjects.OfType<IADUser>(), e => e.UnlockAccount());
		}

		static void DoSomethingAndThenSaveApplicableFactories<T>(IEnumerable<T> linkedEntitiesToDoSomethingTo, Action<T> action)
		{
			var factoriesToSave = new HashSet<BusinessObjectFactory>();

			foreach (var entity in linkedEntitiesToDoSomethingTo)
			{
				action(entity);

				var bizo = entity as BusinessObject;
				if (bizo != null && !factoriesToSave.Contains(bizo.Factory))
				{
					factoriesToSave.Add(bizo.Factory);
				}
			}

			foreach (var factory in factoriesToSave)
			{
				ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null, true);
			}
		}

		#endregion

		IEnumerable<IADLinkedEntity> SelectedObjects
		{
			get { return parentModule.GetSelectedBusinessObjects().OfType<IADLinkedEntity>(); }
		}
	}
}
