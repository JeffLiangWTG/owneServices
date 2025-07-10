using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI
{
	public class ModuleButtonGridForLicencing : ZModuleButtonGrid
	{
		public ModuleButtonGridForLicencing()
			: base()
		{
			OnAttach += ModuleButtonGridForLicencing_OnAttach;
		}

		public ILicenceDatabaseViewController LicDatabaseViewController { get; set; }

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (!IsDisposing)
			{
				SetupEditDatabaseContextMenu();
			}
		}

		protected override void DetachButton_Click(object sender, EventArgs e)
		{
			if (EDISecurityCheckpoints.OrgLicenceRemoveLicenceDatabase.IsAllowed)
			{
				base.DetachButton_Click(sender, e);
			}
			else
			{
				Globals.Message.Show(EDISecurityCheckpoints.OrgLicenceRemoveLicenceDatabase.ErrorMessageForNotAllowed);
			}
		}

		protected override void Detach(BusinessObject selected)
		{
			ZBool noOtherChangesExceptThis = AllLicDBsRelatedToThisOrgHaveNoChanges;
			LicenceDatabase detachedDatabase = (LicenceDatabase)selected;
			DetachDatabase(selected);

			BusinessObjectFactory factory = new BusinessObjectFactory();
			int numOfLicenceHeadersPointingToDB = factory.Load<LicenceHeader>(new ZQuery(LicenceHeaderSchema.LA_LD, detachedDatabase.PK)).Length;
			if (numOfLicenceHeadersPointingToDB <= 1)
			{
				if (!noOtherChangesExceptThis)
				{
					Globals.Message.Show(Res.GetString("e46c2695-cf1f-42c8-88d5-ca6cd8b34214", "No other companies use this license database, but you need to save your changes before deleting unused license database"));
				}
				else
				{
					DialogResult dialogResult = Globals.Message.Show(Res.GetString("9da3b845-b866-4170-9f08-d45633ad6958", "No other companies use this license database. Would you like to delete this license database?"), Res.GetString("05cc6c69-6a48-4665-bfb6-57f35f1e1ae1", "Delete unused license database"), MessageBoxButtons.YesNo, DialogResult.No);
					if (dialogResult == DialogResult.Yes)
					{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						Organisation.Logs.AddNew(ZArchitecture.Business.Events.DeletedARecordInTheSystem, "deleted license database server '" + detachedDatabase.LD_ServerCode + "'");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						detachedDatabase.Delete();
					}
				}
			}
		}

		protected virtual void DetachDatabase(BusinessObject selected)
		{
			base.Detach(selected);
		}

		#region Attach New Database

		protected override void AttachButton_Click(object sender, EventArgs e)
		{
			EDIOrgHeader header = (((ZForm)ParentForm).BusinessEntity as EDIOrgHeader);
			if (header != null && header.AllowAddingNewDatabases)
			{
				AttachDatabase(sender, e);
			}
			else
			{
				Globals.Message.Show(Res.GetString("9a308ffe-3279-462f-9a94-5329d33e0ddd", "You must first save your changes, before attempting to add Databases to the Organization."), Res.GetString("e3b0f5d8-92e8-4d82-a810-85fa534d2791", "Cannot Add Database"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		protected virtual void AttachDatabase(object sender, EventArgs e)
		{
			base.AttachButton_Click(sender, e);
		}

		void ModuleButtonGridForLicencing_OnAttach(object sender, ModuleButtonGridOnAttachEventArgs e)
		{
			var org = (((ZForm)ParentForm).BusinessEntity as EDIOrgHeader);
			if (e.AttachedBusinessObjects.Cast<LicenceDatabase>().Any(x => x.IsEnterpriseFamilyDatabase)
				&& org != null
				&& !Country.IsSupportedForLicenceBuilder(org.LicCompany.LC_CompanyCountry))
			{
				Globals.Message.ShowWarning(LicenceCompanyValidation.SelectedCountryNotSupported);
			}

			if (org != null)
			{
				org.Validation.ValidateLicenceEnterpriseCode();
			}
		}

		#endregion

		#region Edit Database

		void SetupEditDatabaseContextMenu()
		{
			InnerGrid.ContextMenu.MenuItems.Add("-");
			InnerGrid.ContextMenu.MenuItems.Add("Edit Database", new EventHandler(EditDatabase));

			InnerGrid.DoubleClick += new EventHandler(EditDatabase);
		}

		/// <summary>
		/// Suppress DoubleClick in base class, since this has its own implementation;
		/// </summary>
		protected override bool AllowDoubleClick => false;

		void EditDatabase(object sender, EventArgs e)
		{
			var licDB = InnerGrid.ListManager.GetCurrent() as LicenceDatabase;

			if (InnerGrid.ListManager.Count > 0 && licDB != null)
			{
				if (licDB.HasChanges)
				{
					Globals.Message.ShowInformation(Res.GetString("2358ff95-e6ab-43e9-aff1-0ba02d2d9b8a", "Please save the form before editing the database."));
				}
				else
				{
					fCachedLicenceDatabaseForm = new LicenceDatabaseForm(new BusinessObjectFactory().Load<LicenceDatabase>(licDB.PK), LicDatabaseViewController, ParentControl?.ContextBusinessEntity);
					ZFormModaliser.Show(fCachedLicenceDatabaseForm, ParentForm);
				}
			}
		}

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			base.Dispose(isNotFinalizing);

			if (isNotFinalizing)
			{
				if (fCachedLicenceDatabaseForm != null)
				{
					fCachedLicenceDatabaseForm.Dispose();
				}
			}
		}

		LicenceDatabaseForm fCachedLicenceDatabaseForm;

		#endregion

		#endregion

		#region Implementation

		protected bool AllLicDBsRelatedToThisOrgHaveNoChanges
		{
			get
			{
				bool result = true;
				if (Organisation != null && Organisation.LicCompany != null)
				{
					foreach (LicenceDatabase licDB in Organisation.LicCompany.LicDatabases)
					{
						if ((!licDB.IsInDatabase || licDB.IsDeleted) && licDB != (LicenceDatabase)InnerGrid.ListManager.GetCurrent())
						{
							result = false;
						}
					}
				}
				return result;
			}
		}

		#endregion

		EDIOrgHeader Organisation
		{
			get { return ParentControl.Organisation; }
		}

		public LicenceKeyBuilderControl ParentControl { get; set; }
	}
}

