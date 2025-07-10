using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Core.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI
{
	public partial class LicenceEnterpriseForm : ZTemplateForm
	{
		public LicenceEnterpriseForm(LicenceEnterprise licenceEnterprise) : base(licenceEnterprise)
		{
			PlugIns.Add(ClientControllerRegistration.EdiUserAgreementAssignment);
		}

		LicenceEnterprise LicEnterprise => BusinessEntity as LicenceEnterprise;

		#region GUI Setup

		public override string FormCaption
		{
			get { return "Enterprise Licence"; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DisableNewAction();

			var enterprise = LicEnterprise;
			if (enterprise != null)
			{
				originEnterpriseOrgPk = (ZGuid)enterprise.LE_OHInfo.OriginalValue;
				enterprise.LE_OHInfo.ValueChanged += LE_OHInfo_ValueChanged;
				PopulateOriginalDatabaseWebAccessOrgDict();
			}
		}

		#endregion

		#region Change Enterprise Org

		bool runContactCloner;
		ZGuid originEnterpriseOrgPk = ZGuid.Empty;
		readonly Dictionary<LicenceDatabase, OrgHeader> originalDatabaseWebAccessOrgDict = new Dictionary<LicenceDatabase, OrgHeader>(1);

		void PopulateOriginalDatabaseWebAccessOrgDict()
		{
			originalDatabaseWebAccessOrgDict.Clear();
			foreach (LicenceDatabase database in LicEnterprise.Databases)
			{
				if (database.ShouldCloneContactOnWebAccessOrgChanged)
				{
					database.Lookups.WebAccessOrgs.Load();
					originalDatabaseWebAccessOrgDict.Add(database, database.WebAccessOrg);
				}
			}
		}

		void LE_OHInfo_ValueChanged(object sender, EventArgs e)
		{
			var enterprise = LicEnterprise;

			if (enterprise != null
				&& enterprise.IsInDatabase
				&& enterprise.Header != null
				&& enterprise.LE_OH != originEnterpriseOrgPk)
			{
				var databasesWithChangedMasterOrg = originalDatabaseWebAccessOrgDict.Where(x => x.Value != enterprise.LE_OH && x.Key.Lookups.WebAccessOrgs.Any(y => y.PK == enterprise.LE_OH));
				if (databasesWithChangedMasterOrg.Any())
				{
					var isConfirmed = ShowChangeParentOrgConfirmationDialog(databasesWithChangedMasterOrg);
					if (isConfirmed)
					{
						runContactCloner = true;
						foreach (var pair in databasesWithChangedMasterOrg)
						{
							pair.Key.LD_OH_WebAccessOrg = enterprise.LE_OH;
						}
						originEnterpriseOrgPk = enterprise.LE_OH;
					}
					else
					{
						enterprise.LE_OH = originEnterpriseOrgPk;
					}
				}
			}
		}

		protected override void Save(ITransactionParticipant[] factories)
		{
			ContactCloner cloner = null;
			if (runContactCloner)
			{
				cloner = new ContactCloner(LicEnterprise.Factory);
				var licenceDatabaseBatchForContactsClone = originalDatabaseWebAccessOrgDict.Keys.Select(x => x.PK).ToList();
				foreach (var pair in originalDatabaseWebAccessOrgDict)
				{
					var database = pair.Key;
					var originalWebAccessOrg = pair.Value;
					var newWebAccessOrg = LicEnterprise.Header;
					cloner.CloneContacts(originalWebAccessOrg, newWebAccessOrg, database, licenceDatabaseBatchForContactsClone);
				}

				PopulateOriginalDatabaseWebAccessOrgDict();
				runContactCloner = false;
			}

			base.Save(factories);

			if (cloner != null)
			{
				cloner.MergeContactsPerson();

				if (cloner.HasPersonMergeFailure)
				{
					var messageBuilder = new ZStringBuilder();
					messageBuilder.AppendLine("The following person(s) is/are failed to merge. Please merge via Maintain -> Master Data -> MDM Administration.");
					messageBuilder.AppendLine();
					messageBuilder.Append(cloner.GetPersonMergeFailureMessage());

					UserNotification.Instance.ShowError(messageBuilder.ToString(), "Failed to Merge Person");
				}
			}
		}

		bool ShowChangeParentOrgConfirmationDialog(IEnumerable<KeyValuePair<LicenceDatabase, OrgHeader>> databasesWithChangedWebAccessOrg)
		{
			var messageBuilder = new ZStringBuilder();
			messageBuilder.AppendLine("Would you like to bulk update the master organisation of following production system(s) to the new enterprise orgaisation? This will affect contacts for MyAccount and eRequest login. Contacts from following production system(s) will be copied to another organisation.");
			messageBuilder.AppendLine();

			foreach (var pair in databasesWithChangedWebAccessOrg)
			{
				var database = pair.Key;
				var originalWebAccessOrg = pair.Value;
				var newWebAccessOrg = LicEnterprise.Header;

				messageBuilder.AppendLine(FormattableString.Invariant($"System {LicEnterprise.LE_EnterpriseCode}-{database.LD_ServerCode}: {originalWebAccessOrg.OH_Code} -> {newWebAccessOrg.OH_Code}"));
			}

			DialogResult result = UserNotification.Instance.ShowConfirmation(messageBuilder.ToString(), "Change Database Master Organisation", "confirm", MessageBoxIcon.Question);
			return result == DialogResult.OK;
		}

		void DbDetailsGrid_DoubleClick(object sender, EventArgs e)
		{
			if (DbDetailsGrid.ListManager.Position > -1)
			{
				var db = DbDetailsGrid.ListManager.GetCurrent() as LicenceDatabase;
				var controller = ZControllerFactory.Create(Modules.ClientControllerRegistration.LicenceDatabase);
				controller.ShowEditForm(db);
			}
		}

		#endregion
	}
}
