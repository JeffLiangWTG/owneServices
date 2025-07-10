using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.GUI;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.EndpointManagement.GUI
{
	public class LicenceDatabaseModuleButtonGrid : ZModuleButtonGrid
	{
		/// <summary>
		/// Suppress DoubleClick in base class, since this has its own implementation;
		/// </summary>
		protected override bool AllowDoubleClick => false;

		public ILicenceDatabaseViewController LicDatabaseViewController { get; set; }

		public EdiTrustedSystem ParentTrustedSystem { get; set; }

		LicenceDatabaseForm cachedLicenceDatabaseForm;

		public LicenceDatabaseModuleButtonGrid()
			: base()
		{
			OnAttach += LicenceDatabaseModuleButtonGrid_OnAttach;
			InnerGrid.DoubleClick += new EventHandler(EditDatabase);
		}

		void EditDatabase(object sender, EventArgs e)
		{
			var licDB = InnerGrid.ListManager.GetCurrent() as LicenceDatabase;

			if (InnerGrid.ListManager.Count > 0 && licDB != null)
			{
				if (licDB.HasChanges)
				{
					Globals.Message.ShowInformation(Res.GetString("1E9CA748-190C-46D0-91D1-2589F526A903", "Please save the form before editing the database."));
				}
				else
				{
					cachedLicenceDatabaseForm = new LicenceDatabaseForm(new BusinessObjectFactory().Load<LicenceDatabase>(licDB.PK), LicDatabaseViewController);
					ZFormModaliser.Show(cachedLicenceDatabaseForm, ParentForm);
				}
			}
		}

		protected override void Detach(BusinessObject selected)
		{
			var detachedDatabase = (LicenceDatabase)selected;
			detachedDatabase.LD_ETS_TrustedSystem = ZGuid.Empty;
		}

		void LicenceDatabaseModuleButtonGrid_OnAttach(object sender, ModuleButtonGridOnAttachEventArgs e)
		{
			var hasChanges = false;

			foreach (var item in e.AttachedBusinessObjects.Cast<LicenceDatabase>())
			{
				if (item.LD_ETS_TrustedSystem != ParentTrustedSystem.PK)
				{
					hasChanges = true;
				}

				item.LD_ETS_TrustedSystem = ParentTrustedSystem.PK;
			}

			if (hasChanges)
			{
				ParentTrustedSystem.HasChanges = hasChanges;
			}
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			base.Dispose(isNotFinalizing);

			if (isNotFinalizing)
			{
				if (cachedLicenceDatabaseForm != null)
				{
					cachedLicenceDatabaseForm.Dispose();
				}
			}
		}
	}
}

